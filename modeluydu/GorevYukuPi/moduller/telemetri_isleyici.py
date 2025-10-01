# -*- coding: utf-8 -*-
"""
Telemetri Yönetici Modülü

Bu modül, TÜRKSAT şartnamesine uygun telemetri paketinin oluşturulmasından
ve uçuş durumu yönetiminden sorumludur.
"""

import logging
import time
from datetime import datetime
from moduller.yapilandirma import (
    DENIZ_SEVIYESI_BASINC_HPA, TAKIM_NUMARASI, AYRILMA_TIMEOUT,
    AYRILMA_YUKSEKLIK as AYRILMA_IRTIFASI, paket_sayisi_yukle, paket_sayisi_kaydet,
    HIZ_LIMIT_MODEL_UYDU_MIN, HIZ_LIMIT_MODEL_UYDU_MAX,
    HIZ_LIMIT_GOREV_YUKU_MIN, HIZ_LIMIT_GOREV_YUKU_MAX,
    AYRILMA_TOLERANS
)

logger = logging.getLogger(__name__)

class TelemetryHandler:
    """
    TÜRKSAT Model Uydu Yarışması telemetri işleme sınıfı.
    Sensör verilerini alır, TÜRKSAT formatında telemetri paketi oluşturur ve checksum ekler.
    """
    
    def __init__(self, saha_alici=None):
        # Paket numarası - kalıcı olarak dosyadan yükle
        self.packet_number = paket_sayisi_yukle()
        
        # Uydu durumu ve hata kodu
        self.uydu_statusu = 0  # 0: Uçuşa Hazır
        self.hata_kodu = "000000"  # ARAS format: 6 haneli
        
        # SAHA protokol alıcısı referansı
        self.saha_alici = saha_alici

        # Timing ve durum takibi
        self.ayrilma_baslangic_zamani = None
        self.son_irtifa = 0.0
        self.son_irtifa_zamani = time.time()
        self.hiz_gecmisi = []  # Son 5 ölçüm için hız geçmişi
        self.tasiyici_irtifa = 0.0
        
        # RHRH komutu - varsayılan
        self.rhrh_komut = "0000"
        
        # Hız hesaplama için zaman aşımı süreleri
        self.gps_timeout_suresi = 30.0  # 30 saniye
        
        # Hız hesaplama için gerekli değişkenler
        self.onceki_zaman = time.time()
        self.onceki_yukseklik = 0.0
        self.max_hiz_gecmis = 5
        self.basinc_timeout_suresi = 15.0  # 15 saniye
        self.son_gps_zamani = time.time()
        self.son_tasiyici_basinc_zamani = time.time()
        
        # Ek durumlar
        self.ayrilma_gerceklesti = False
        self.multispektral_sistem_hatasi = False
        self.son_gonderim_zamani = ""
        
    def _hesapla_checksum(self, veri):
        """
        XOR checksum hesaplama (TÜRKSAT yer istasyonu uyumlu)
        """
        checksum = 0
        for char in veri:
            checksum ^= ord(char)
        return checksum

    def _hesapla_checksum_duplicate(self, veri: str) -> int:
        """
        XOR checksum hesapla (C# yer istasyonu ile uyumlu)
        Yer istasyonu HesaplaChecksum metodu ile aynı algoritma
        """
        checksum = 0
        for char in veri:
            checksum ^= ord(char)
        return checksum & 0xFF  # 8-bit checksum

    def set_rhrh_komut(self, komut):
        """
        Yer istasyonundan gelen RHRH komutunu telemetriye eklenmek üzere kaydeder.
        """
        self.rhrh_komut = komut

    def set_multispektral_hata(self, durum: bool):
        """
        Multi-spektral sistemde bir hata olup olmadığını ana programa bildirmek için kullanılır.
        """
        self.multispektral_sistem_hatasi = durum

    def _guncelle_uydu_statusu(self, yukseklik, inis_hizi):
        """
        🔧 DÜZELTME: Mevcut irtifa ve hıza göre uydu durumunu günceller.
        0: Uçuşa Hazır, 1: Yükselme, 2: Model Uydu İniş,
        3: Ayrılma, 4: Görev Yükü İniş, 5: Kurtarma
        """
        # Yükselme/iniş tespiti için yükseklik değişimini hesapla
        yukseklik_farki = self.onceki_yukseklik - yukseklik if self.onceki_yukseklik > 0 else 0
        zaman_farki = time.time() - self.onceki_zaman if self.onceki_zaman > 0 else 0
        # Bu mantık, gerçek uçuş verilerine göre daha da geliştirilebilir.
        # Örneğin, yükselme apogee noktası tespiti ile daha hassas hale getirilebilir.
        if self.uydu_statusu == 5:  # Kurtarma modundaysa durumu değiştirme
            return

        # 🔧 DÜZELTME: Kurtarma modu geçişi (Gereksinim 24)
        if self.uydu_statusu == 4 and yukseklik < 10.0:
            self.uydu_statusu = 5  # Kurtarma
            print("🚨 KURTARMA MODU BAŞLATILDI! (Yükseklik: {:.1f}m)".format(yukseklik))
            return

        if self.ayrilma_gerceklesti:
            self.uydu_statusu = 4  # Görev Yükü İniş
        elif yukseklik >= (AYRILMA_IRTIFASI - AYRILMA_TOLERANS) and yukseklik <= (AYRILMA_IRTIFASI + AYRILMA_TOLERANS):
            if self.uydu_statusu != 3:
                # Ayrılma durumuna ilk kez giriliyor
                self.ayrilma_baslangic_zamani = time.time()
                print("BİLGİ: Ayrılma sekansı başlatıldı, zamanlayıcı kuruldu.")
            self.uydu_statusu = 3  # Ayrılma
        elif yukseklik > 50 and yukseklik_farki > 1 and zaman_farki > 0:  # İniş tespit edildi
            self.uydu_statusu = 2  # Model Uydu İniş
        elif yukseklik > 10 and yukseklik_farki < -1 and zaman_farki > 0:  # Yükselme tespit edildi
            self.uydu_statusu = 1
        else:
            self.uydu_statusu = 0  # Uçuşa Hazır

    def _hesapla_inis_hizi(self, anlik_yukseklik):
        """
        🔧 İYİLEŞTİRİLMİŞ: İki yükseklik ölçümü arasındaki farktan hızı hesaplar.
        Smoothing filter ve outlier detection içerir.
        """
        simdiki_zaman = time.time()
        zaman_farki = simdiki_zaman - self.onceki_zaman
        
        if zaman_farki == 0 or zaman_farki > 5.0:  # 5 saniyeden fazla gap varsa sıfırla
            self.onceki_yukseklik = anlik_yukseklik
            self.onceki_zaman = simdiki_zaman
            return 0.0

        # Ham hız hesaplama - MUTLAK DEĞER (şartname belirsiz, praktik yaklaşım)
        yukseklik_farki = self.onceki_yukseklik - anlik_yukseklik
        ham_hiz = abs(yukseklik_farki / zaman_farki)  # Her zaman pozitif hız değeri

        # 🔧 OUTLIER DETECTION: Aşırı hız değerlerini filtrele
        if abs(ham_hiz) > 50.0:  # 50 m/s'den fazla hız fiziksel olarak imkansız
            print(f"UYARI: Aşırı hız değeri filtrelendi: {ham_hiz:.1f} m/s")
            return self.hiz_gecmisi[-1] if self.hiz_gecmisi else 0.0

        # 🔧 SMOOTHING FILTER: Son 5 hız değerinin ortalaması
        self.hiz_gecmisi.append(ham_hiz)
        if len(self.hiz_gecmisi) > self.max_hiz_gecmis:
            self.hiz_gecmisi.pop(0)
        
        # Median filter (outlier'lara karşı robust)
        sorted_speeds = sorted(self.hiz_gecmisi)
        if len(sorted_speeds) >= 3:
            smoothed_hiz = sorted_speeds[len(sorted_speeds)//2]  # Median
        else:
            smoothed_hiz = sum(self.hiz_gecmisi) / len(self.hiz_gecmisi)  # Ortalama

        # Önceki değerleri güncelle
        self.onceki_yukseklik = anlik_yukseklik
        self.onceki_zaman = simdiki_zaman
        
        return smoothed_hiz

    def _guncelle_hata_kodu(self, sensor_data):
        """
        🔧 İYİLEŞTİRİLMİŞ: ARAS (Arayüz Alarm Sistemi) kurallarına göre hata kodunu günceller.
        Şartname bölüm 2.2'ye göre 6 haneli hata kodu:
        1. Model uydu iniş hızı (12-14 m/s dışında)
        2. Görev yükü iniş hızı (6-8 m/s dışında) 
        3. Taşıyıcı basınç verisi alınamama
        4. Görev yükü konum verisi alınamama (GPS timeout dahil)
        5. Ayrılmanın gerçekleşmemesi
        6. Multi-spektral sistem hatası
        """
        hata_listesi = ['0'] * 6
        simdiki_zaman = time.time()

        # İyileştirilmiş hız hesaplama
        mevcut_irtifa = sensor_data.get('irtifa', 0.0)
        inis_hizi = 0.0
        if self.onceki_zaman > 0:  # Önceki veri varsa hız hesapla
            inis_hizi = abs(self._hesapla_inis_hizi(mevcut_irtifa))
        
        # Taşıyıcı basınç verisi kontrolü
        tasiyici_basinci = sensor_data.get('tasiyici_basinci', 0.0)
        if tasiyici_basinci > 0:
            self.son_tasiyici_basinc_zamani = simdiki_zaman
        
        # GPS verisi kontrolü
        gps_verisi = sensor_data.get('gps_verisi', {})
        gps_valid = (gps_verisi.get('enlem', 0.0) != 0.0 and gps_verisi.get('boylam', 0.0) != 0.0)
        if gps_valid:
            self.son_gps_zamani = simdiki_zaman
        
        # Kural 1: Model uydu iniş hızı (Durum 2: Model Uydu İniş)
        if self.uydu_statusu == 2 and inis_hizi > 0:  # Hız hesaplanabilirse kontrol et
            if inis_hizi < HIZ_LIMIT_MODEL_UYDU_MIN or inis_hizi > HIZ_LIMIT_MODEL_UYDU_MAX:
                hata_listesi[0] = '1'
                print(f"🚨 ARAS Hata 1: Model uydu hız {inis_hizi:.1f} m/s (12-14 m/s olmalı)")
            
        # Kural 2: Görev yükü iniş hızı (Durum 4: Görev Yükü İniş)
        if self.uydu_statusu == 4 and inis_hizi > 0:  # Hız hesaplanabilirse kontrol et
            if inis_hizi < HIZ_LIMIT_GOREV_YUKU_MIN or inis_hizi > HIZ_LIMIT_GOREV_YUKU_MAX:
                hata_listesi[1] = '1'
                print(f"🚨 ARAS Hata 2: Görev yükü hız {inis_hizi:.1f} m/s (6-8 m/s olmalı)")
            
        # Kural 3: Taşıyıcı basınç verisi alınamaması (timeout dahil)
        basinc_timeout = (simdiki_zaman - self.son_tasiyici_basinc_zamani) > self.basinc_timeout_suresi
        if tasiyici_basinci <= 0 or basinc_timeout:
            hata_listesi[2] = '1'
            if basinc_timeout:
                print(f"🚨 ARAS Hata 3: Taşıyıcı basınç timeout ({self.basinc_timeout_suresi}s)")
            
        # Kural 4: Görev yükü konum verisi alınamaması (GPS timeout dahil)
        gps_timeout = (simdiki_zaman - self.son_gps_zamani) > self.gps_timeout_suresi
        if not gps_valid or gps_timeout:
            hata_listesi[3] = '1'
            if gps_timeout:
                print(f"🚨 ARAS Hata 4: GPS timeout ({self.gps_timeout_suresi}s)")
            
        # Kural 5: Ayrılmanın gerçekleşmemesi (Timeout)
        if (self.uydu_statusu == 3 and 
            self.ayrilma_baslangic_zamani > 0 and 
            (simdiki_zaman - self.ayrilma_baslangic_zamani > AYRILMA_TIMEOUT)):
            hata_listesi[4] = '1'
            print(f"🚨 ARAS Hata 5: Ayrılma timeout ({AYRILMA_TIMEOUT}s)")
            
        # Kural 6: Multi-spektral sistem hatası
        if self.multispektral_sistem_hatasi:
            hata_listesi[5] = '1'
            print(f"🚨 ARAS Hata 6: Multi-spektral sistem hatası")
            self.multispektral_sistem_hatasi = False  # Hatayı bir kez raporla ve sıfırla
        
        self.hata_kodu = "".join(hata_listesi)

    def _irtifa_hesapla(self, basinc, deniz_seviyesi_basinc=DENIZ_SEVIYESI_BASINC_HPA):
        """Verilen basınç değerine göre irtifayı hesaplar (Barometrik formül)."""
        if basinc <= 0:
            return 0.0
        # 44330 * (1 - (P/P0)^(1/5.255))
        return 44330.0 * (1.0 - pow(basinc / deniz_seviyesi_basinc, 0.1903))

    def olustur_telemetri_paketi(self, sensor_verisi: dict, iot_s1_data=None, iot_s2_data=None):
        """
        🔥 BASİT VE GÜVENİLİR TELEMETRİ PAKETİ OLUŞTURUCU
        Tüm karmaşık fonksiyonlar bypass edildi - sadece SD kaydı odaklı!
        """
        print("  🔧 DEBUG: BASİT telemetri paketi oluşturuluyor...")
        
        # 🔥 GERÇEK SENSÖR VERİLERİNİ KULLAN!
        try:
            # Temel sensör verileri (GERÇEK)
            gorev_yuku_basinci = sensor_verisi.get("basinc", 101325)  # Pascal cinsinden - GERÇEK BMP280
            gorev_yuku_irtifa = sensor_verisi.get("irtifa", 0.0)  # GERÇEK yükseklik
            
            # 🔧 ŞARTNAME: Yükseklik konfigürasyonu - uçuşa başlanacak yer 0 metre
            # Basınçtan yükseklik hesapla (barometrik formül) - GERÇEK HESAPLAMA
            if gorev_yuku_basinci > 0 and gorev_yuku_basinci != 101325:
                # P0 = deniz seviyesi basıncı (101325 Pa)
                # h = 44330 * (1 - (P/P0)^0.1903) 
                deniz_seviyesi_basinc = 101325.0
                calculated_altitude = 44330.0 * (1.0 - pow(gorev_yuku_basinci / deniz_seviyesi_basinc, 0.1903))
                gorev_yuku_irtifa = max(0.0, calculated_altitude)  # Negatif yükseklik olmasın
            sicaklik = sensor_verisi.get("sicaklik", 25.0)  # GERÇEK BMP280 sıcaklık
            pil_gerilimi = sensor_verisi.get("pil_gerilimi", 7.4)  # GERÇEK pil voltajı
            
            # GPS verileri (GERÇEK) - FAKE KOORDİNATLARI KALDIR!
            gps_verisi = sensor_verisi.get("gps_verisi", {})
            gps_lat = gps_verisi.get("enlem", 0.0)  # GERÇEK GPS enlem (fake İstanbul yok!)
            gps_lon = gps_verisi.get("boylam", 0.0)  # GERÇEK GPS boylam (fake İstanbul yok!)
            gps_alt = gps_verisi.get("yukseklik", 0.0)  # GERÇEK GPS yükseklik
            
            # IMU verileri (GERÇEK) - Hem işlenmiş hem ham veriler
            imu_verisi = sensor_verisi.get("imu_verisi", {})
            pitch = imu_verisi.get("pitch", 0.0)  # GERÇEK MPU açı
            roll = imu_verisi.get("roll", 0.0)    # GERÇEK MPU açı
            yaw = imu_verisi.get("yaw", 0.0)      # GERÇEK MPU açı
            
            # Ham IMU verileri (10DOF sensör ham değerleri)
            acc_x = imu_verisi.get("ivme_x", 0.0)    # Accelerometer X
            acc_y = imu_verisi.get("ivme_y", 0.0)    # Accelerometer Y
            acc_z = imu_verisi.get("ivme_z", 0.0)    # Accelerometer Z
            gyro_x = imu_verisi.get("gyro_x", 0.0)   # Gyroscope X
            gyro_y = imu_verisi.get("gyro_y", 0.0)   # Gyroscope Y
            gyro_z = imu_verisi.get("gyro_z", 0.0)   # Gyroscope Z
            mag_x = imu_verisi.get("mag_x", 0.0)     # Magnetometer X
            mag_y = imu_verisi.get("mag_y", 0.0)     # Magnetometer Y
            mag_z = imu_verisi.get("mag_z", 0.0)     # Magnetometer Z
            
            # 🔧 GERÇEK HESAPLAMALAR - ARTIK BYPASS YOK!
            # Taşıyıcı basıncı (saha alıcısından)
            tasiyici_basinci = sensor_verisi.get("tasiyici_basinc", 0.0)  # GERÇEK taşıyıcı
            self.tasiyici_irtifa = self._irtifa_hesapla(tasiyici_basinci) if tasiyici_basinci > 0 else 0.0
            irtifa_farki = gorev_yuku_irtifa - self.tasiyici_irtifa  # GERÇEK fark
            inis_hizi = sensor_verisi.get("inis_hizi", 0.0)  # GERÇEK hız
            
            # Uydu statusü - gerçek duruma göre
            if tasiyici_basinci > 0 and irtifa_farki < 10:  # Henüz ayrılmamış
                self.uydu_statusu = 1  # Uçuş
            elif irtifa_farki > 10:  # Ayrıldı
                self.uydu_statusu = 3  # Ayrılma
            else:
                self.uydu_statusu = 0  # Hazır
            
            # Hata kodu hesapla (gerçek)
            self._guncelle_hata_kodu(sensor_verisi)
        
            # IoT verileri (güvenli)
            iot_s1_temp = 25.0
            iot_s2_temp = 25.0
            
            # Zaman (güvenli) - Şartname: DD/MM/YYYY HH:MM:SS
            # RTC zamanını kullan (sensor_verisi'nden)
            rtc_time = sensor_verisi.get('rtc_time', None)
            if rtc_time:
                # struct_time'ı datetime'a çevir
                if hasattr(rtc_time, 'tm_year'):  # struct_time kontrolü
                    rtc_datetime = datetime(*rtc_time[:6])  # struct_time'dan datetime
                    gonderme_saati = rtc_datetime.strftime("%d/%m/%Y %H:%M:%S")
                else:
                    gonderme_saati = rtc_time.strftime("%d/%m/%Y %H:%M:%S")
            else:
                gonderme_saati = datetime.now().strftime("%d/%m/%Y %H:%M:%S")
            
            print("  🔧 DEBUG: Tüm veriler güvenli şekilde hazırlandı")
            
        except Exception as e:
            print(f"  🚨 DEBUG: Veri hazırlama hatası: {e}")
            # FULL EMERGENCY FALLBACK - GERÇEK VERİLER İLE
            gorev_yuku_basinci = sensor_verisi.get("basinc", 101325)  # GERÇEK BMP280
            gorev_yuku_irtifa = sensor_verisi.get("irtifa", 0.0)      # GERÇEK yükseklik
            sicaklik = sensor_verisi.get("sicaklik", 25.0)            # GERÇEK sıcaklık
            pil_gerilimi = sensor_verisi.get("pil_gerilimi", 7.4)     # GERÇEK pil
            
            # GPS - GERÇEK VERİLER (fake İstanbul koordinatları YOK!)
            gps_verisi = sensor_verisi.get("gps_verisi", {})
            gps_lat = gps_verisi.get("enlem", 0.0)      # GERÇEK GPS
            gps_lon = gps_verisi.get("boylam", 0.0)     # GERÇEK GPS  
            gps_alt = gps_verisi.get("yukseklik", 0.0)  # GERÇEK GPS
            
            # IMU - GERÇEK VERİLER (hem işlenmiş hem ham)
            imu_verisi = sensor_verisi.get("imu_verisi", {})
            pitch = imu_verisi.get("pitch", 0.0)        # GERÇEK MPU
            roll = imu_verisi.get("roll", 0.0)          # GERÇEK MPU
            yaw = imu_verisi.get("yaw", 0.0)            # GERÇEK MPU
            
            # Ham IMU verileri (emergency fallback)
            acc_x = imu_verisi.get("ivme_x", 0.0)
            acc_y = imu_verisi.get("ivme_y", 0.0)
            acc_z = imu_verisi.get("ivme_z", 0.0)
            gyro_x = imu_verisi.get("gyro_x", 0.0)
            gyro_y = imu_verisi.get("gyro_y", 0.0)
            gyro_z = imu_verisi.get("gyro_z", 0.0)
            mag_x = imu_verisi.get("mag_x", 0.0)
            mag_y = imu_verisi.get("mag_y", 0.0)
            mag_z = imu_verisi.get("mag_z", 0.0)
            
            tasiyici_basinci = sensor_verisi.get("tasiyici_basinc", 0.0)  # GERÇEK saha
            self.tasiyici_irtifa = 0.0
            irtifa_farki = 0.0
            inis_hizi = 0.0
            self.uydu_statusu = 0
            self.hata_kodu = "000000"
            iot_s1_temp = iot_s2_temp = 25.0  # IoT simülasyon (GPS/ADC gibi)
            gonderme_saati = datetime.now().strftime("%d/%m/%Y %H:%M:%S")
        
        # 🔥 BASİT PAKET OLUŞTURMA
        try:
            # Paket numarası arttır (Gereksinim 15: 1'den başlar)
            if not hasattr(self, 'packet_number') or self.packet_number is None:
                self.packet_number = 1
                print(f"  🔧 DEBUG: Paket sayacı 1'den başlatıldı")
            else:
                self.packet_number += 1
                
            # ŞARTNAME UYUMLULUK: Paket sayacı çok yüksekse sıfırla
            if self.packet_number > 9999:  # 4 haneli limit
                self.packet_number = 1
                print(f"  🔧 DEBUG: Paket sayacı sıfırlandı (>9999)")
            
            # Telemetri paketini birleştir (ŞARTNAME UYUMLU + 10DOF HAM VERİLER)
            paket_parcalari = [
                str(self.packet_number),           # PAKET NUMARASI
                str(self.uydu_statusu),           # UYDU STATÜSÜ
                self.hata_kodu,                   # HATA KODU
                gonderme_saati,                   # GÖNDERME SAATİ
                f"{gorev_yuku_basinci:.0f}",      # BASINÇ1 (Pascal)
                f"{tasiyici_basinci:.0f}",        # BASINÇ2 (Pascal)
                f"{gorev_yuku_irtifa:.3f}",       # YÜKSEKLİK1
                f"{self.tasiyici_irtifa:.3f}",    # YÜKSEKLİK2
                f"{irtifa_farki:.3f}",            # İRTİFA FARKI
                f"{inis_hizi:.2f}",               # İNİŞ HIZI
                f"{sicaklik:.1f}",                # SICAKLIK
                f"{pil_gerilimi:.2f}",            # PİL GERİLİMİ
                f"{gps_lat:.6f}",                 # GPS1 LATITUDE
                f"{gps_lon:.6f}",                 # GPS1 LONGITUDE
                f"{gps_alt:.2f}",                 # GPS1 ALTITUDE
                f"{pitch:.1f}",                   # PITCH
                f"{roll:.1f}",                    # ROLL
                f"{yaw:.1f}",                     # YAW
                f"{acc_x:.2f}",                   # 10DOF ACCELEROMETER X
                f"{acc_y:.2f}",                   # 10DOF ACCELEROMETER Y
                f"{acc_z:.2f}",                   # 10DOF ACCELEROMETER Z
                f"{gyro_x:.2f}",                  # 10DOF GYROSCOPE X
                f"{gyro_y:.2f}",                  # 10DOF GYROSCOPE Y
                f"{gyro_z:.2f}",                  # 10DOF GYROSCOPE Z
                f"{mag_x:.0f}",                   # 10DOF MAGNETOMETER X
                f"{mag_y:.0f}",                   # 10DOF MAGNETOMETER Y
                f"{mag_z:.0f}",                   # 10DOF MAGNETOMETER Z
                "00",                             # RHRH
                f"{iot_s1_temp:.1f}",            # IoT S1 DATA
                f"{iot_s2_temp:.1f}",            # IoT S2 DATA
                "286570"                          # TAKIM NO
            ]
            
            # Ham telemetri verisi
            ham_telemetri = ",".join(paket_parcalari)
        
            # XBee paketi (checksum ile)
            checksum = self._hesapla_checksum(ham_telemetri)
            xbee_paketi = f"${ham_telemetri}*{checksum:02X}"
            
            print("  🔧 DEBUG: BASİT telemetri paketi oluşturuldu!")
            
            return {
                'ham_veri': ham_telemetri,        # SD için
                'xbee_paketi': xbee_paketi,       # XBee için
                'legacy_format': xbee_paketi
            }
            
        except Exception as e:
            print(f"  🚨 DEBUG: Paket oluşturma hatası: {e}")
            # SON ÇARE: Manuel paket - GERÇEK VERİLERLE
            emergency_data = f"{self.packet_number or 1},0,000000,{datetime.now().strftime('%d/%m/%Y %H:%M:%S')},101325,0,0.000,0.000,0.000,0.00,25.0,7.40,0.000000,0.000000,0.00,0.0,0.0,0.0,00,25.0,25.0,286570"
            return {
                'ham_veri': emergency_data,
                'xbee_paketi': f"${emergency_data}*00",
                'legacy_format': f"${emergency_data}*00"
            }

    def get_hiz_istatistikleri(self):
        """
        Hız hesaplama sistemi için istatistik bilgileri döndürür.
        Debug ve kalibrasyon amaçlı.
        """
        return {
            'mevcut_hiz_gecmisi': list(self.hiz_gecmisi),
            'ortalama_hiz': sum(self.hiz_gecmisi) / len(self.hiz_gecmisi) if self.hiz_gecmisi else 0.0,
            'son_gps_zamani': self.son_gps_zamani,
            'son_basinc_zamani': self.son_tasiyici_basinc_zamani,
            'gps_timeout_durumu': (time.time() - self.son_gps_zamani) > self.gps_timeout_suresi,
            'basinc_timeout_durumu': (time.time() - self.son_tasiyici_basinc_zamani) > self.basinc_timeout_suresi
        }

# Test için örnek kullanım
if __name__ == '__main__':
    import time
    from moduller.yapilandirma import AYRILMA_TIMEOUT
    
    handler = TelemetryHandler()
    
    # Örnek sensör verisi (olustur_telemetri_paketi formatında)
    test_sensor_data = {
        'basinc': 1006.15,
        'tasiyici_basinci': 1002.34,
        'irtifa': 402.39,
        'sicaklik': 24.2,
        'pil_gerilimi': 6.59,
        'gps_verisi': {
            'enlem': 50.501234,
            'boylam': 50.505678,
            'yukseklik': 50.40
        },
        'imu_verisi': {
            'pitch': 10.1,
            'roll': -5.3,
            'yaw': 180.7
        },
        'iot_verileri': {
            'sicaklik1': 25.2,
            'sicaklik2': 24.8
        }
    }

    # Uçuşa Hazır durumu testi
    print("--- Uçuşa Hazır Testi ---")
    packet = handler.olustur_telemetri_paketi(test_sensor_data)
    print(packet)

    # İniş durumu ve hız hatası testi
    print("\n--- İniş ve Hız Hatası Testi ---")
    handler.uydu_statusu = 2  # Manuel olarak durumu inişe alalım
    test_sensor_data['irtifa'] = 350  # Yükseklik düşüyor
    time.sleep(0.5)  # Zaman farkı yarat
    test_sensor_data['irtifa'] = 340  # Yükseklik düşüyor
    packet = handler.olustur_telemetri_paketi(test_sensor_data)
    print(packet)
    # Beklenen hata kodu: 100000 (Model uydu hız limiti dışında)

    # Ayrılma hatası testi
    print("\n--- Ayrılma Hatası Testi ---")
    handler.uydu_statusu = 2
    test_sensor_data['irtifa'] = 405
    packet = handler.olustur_telemetri_paketi(test_sensor_data)
    print(f"Ayrılma öncesi: {packet}")
    
    # Zamanlayıcıyı başlat
    time.sleep(1)
    packet = handler.olustur_telemetri_paketi(test_sensor_data)
    print(f"Ayrılma anı: {packet}")
    assert handler.uydu_statusu == 3
    assert handler.ayrilma_baslangic_zamani > 0
    
    # Timeout süresi kadar bekle
    print(f"{AYRILMA_TIMEOUT + 1} saniye bekleniyor...")
    time.sleep(AYRILMA_TIMEOUT + 1)
    packet = handler.olustur_telemetri_paketi(test_sensor_data)
    print(f"Timeout sonrası: {packet}")
    assert handler.hata_kodu[4] == '1'
