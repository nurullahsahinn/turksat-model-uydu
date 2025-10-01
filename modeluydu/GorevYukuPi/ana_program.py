# -*- coding: utf-8 -*-
"""
TÜRKSAT Model Uydu Yarışması - Görev Yükü Ana Uygulaması (Raspberry Pi)

Bu script, görev yükü üzerindeki tüm modülleri yöneten ve ana görev döngüsünü
çalıştıran ana programdır.
"""

import time
import threading
import queue
from datetime import datetime

# 🧪 WINDOWS TEST MODU - Çevre değişkeni kontrolü
import os
FORCE_SIMULATION = os.environ.get('FORCE_SIMULATION', '0') == '1'
if FORCE_SIMULATION:
    print("🧪 WINDOWS TEST MODU: Simülasyon zorla aktifleştirildi")
    os.environ['IS_RASPBERRY_PI'] = '0'  # Platform tespitini zorla override et

# Proje modüllerini içeri aktar (Türkçe isimlerle güncellendi)
from moduller.yapilandirma import TELEMETRI_GONDERIM_SIKLIGI, SERIAL_PORT_XBEE, SERIAL_BAUD_XBEE, IS_RASPBERRY_PI

from moduller.sensorler import SensorManager
# from moduller.haberlesme import Communication  # DEPRECATED - BirlesikXBeeAlici kullanılıyor
from moduller.telemetri_isleyici import TelemetryHandler
from moduller.aktuatorler import AktuatorYoneticisi
# from moduller.kamera import KameraYoneticisi
from moduller.kamera_basit import BasitKameraYoneticisi as KameraYoneticisi
from moduller.sd_kayitci import SDKayitci
# ESKİ MODÜLLER (Artık birlesik_xbee_alici kullanılıyor):
# from moduller.saha_alici import SahaAlici
# from moduller.iot_xbee_alici import IoTXBeeAlici
from moduller.birlesik_xbee_alici import BirlesikXBeeAlici  # 🔥 TEK XBee modülü
from moduller.guc_yoneticisi import GucYoneticisi

# Global değişkenler ve olaylar
stop_event = threading.Event()
is_running = True
program_start_time = None  # Program başlangıç zamanı tracking

# Global referanslar (komut işleme için)
aktuator_yoneticisi = None
telemetri_isleyici = None
saha_alici = None
guc_yoneticisi = None
birlesik_xbee = None  # 🔧 DÜZELTME: Global birlesik_xbee tanımı eklendi
threads = []  # Global threads listesi

def shutdown_callback():
    """Güvenli kapanma için temizlik fonksiyonu"""
    global stop_event, threads
    print("🧹 Güvenli kapanma: Tüm modüller temizleniyor...")
    
    # Ana döngüyü durdur
    stop_event.set()
    
    # Thread'leri bekle
    for thread in threads:
        if thread.is_alive():
            thread.join(timeout=2)
    
    print("✅ Güvenli kapanma tamamlandı")

def komut_isle(command):
    """
    Yer istasyonundan gelen komutları işler
    Requirements.md Gereksinim 21: Manuel ayrılma komutu
    Requirements.md Gereksinim 35: Multi-spektral filtreleme komutları
    """
    global aktuator_yoneticisi, birlesik_xbee, sensor_yonetici
    
    try:
        print(f"📻 Komut alındı: {command}")
        
        # Manuel ayrılma komutu (Gereksinim 21)
        if command == "!MANUAL_SEPARATION!" or command == "!xT!":
            print("🚨 MANUEL AYRILMA KOMUTU ALINDI!")
            
            # Ayrılma komutunu XBee üzerinden gönder
            if birlesik_xbee:
                try:
                    birlesik_xbee.send_telemetry("AYIRMA_KOMUTU")
                    print("✅ Ayrılma komutu taşıyıcıya gönderildi")
                except Exception as e:
                    print(f"❌ Ayrılma komutu gönderilemedi: {e}")
            
            # Buzzer ile ses uyarısı
            if aktuator_yoneticisi:
                aktuator_yoneticisi.buzzer_kontrol(True)
                time.sleep(2)  # 2 saniye buzzer
                aktuator_yoneticisi.buzzer_kontrol(False)
            
            return
        
        # 🔧 KALİBRASYON KOMUTLARI (Yer istasyonu kalibrasyon komutları için)
        if command.startswith("#CALIB_"):
            if "CALIB_GYRO:RESET" in command:
                print("🔄 Gyro kalibrasyon komutu alındı...")
                # IMU yöneticisi sensor_yonetici içinde bulunur
                if sensor_yonetici and hasattr(sensor_yonetici, 'imu_yoneticisi'):
                    try:
                        # Mevcut gyro kalibrasyonunu yeniden başlat
                        sensor_yonetici.imu_yoneticisi._calibrate_gyro()
                        print("✅ Gyro kalibrasyonu yeniden başlatıldı")
                        
                        # Yer istasyonuna onay gönder
                        if birlesik_xbee:
                            birlesik_xbee.send_telemetry("GYRO_CALIB_OK")
                    except Exception as e:
                        print(f"❌ Gyro kalibrasyon hatası: {e}")
                        if birlesik_xbee:
                            birlesik_xbee.send_telemetry("GYRO_CALIB_ERROR")
                else:
                    print("❌ IMU yöneticisi bulunamadı")
                    
            elif "CALIB_PRESSURE" in command:
                print("🔧 Basınç kalibrasyon komutu alındı...")
                try:
                    # Komut formatı: #CALIB_PRESSURE:911.75:850#
                    parts = command.replace("#", "").split(":")
                    if len(parts) >= 3:
                        deniz_seviyesi_basinc = float(parts[1])
                        rakim = int(parts[2])
                        
                        print(f"📊 Yeni basınç kalibrasyonu: {deniz_seviyesi_basinc} hPa, Rakım: {rakim}m")
                        
                        # yapilandirma.py değerlerini güncelle (runtime)
                        import GorevYukuPi.moduller.yapilandirma as config
                        config._cached_basinc = deniz_seviyesi_basinc
                        
                        # Yer istasyonuna onay gönder
                        if birlesik_xbee:
                            birlesik_xbee.send_telemetry(f"PRESSURE_CALIB_OK:{deniz_seviyesi_basinc}")
                        
                        print("✅ Basınç kalibrasyonu güncellendi")
                    else:
                        print("❌ Geçersiz basınç kalibrasyon formatı")
                except Exception as e:
                    print(f"❌ Basınç kalibrasyon hatası: {e}")
                    if birlesik_xbee:
                        birlesik_xbee.send_telemetry("PRESSURE_CALIB_ERROR")
            else:
                print(f"⚠️ Bilinmeyen kalibrasyon komutu: {command}")
            
            return
        
        # 🔧 MOTOR KONTROL KOMUTLARI (Yer istasyonu motor komutları için) - DEVRE DIŞI
        if command.startswith("!M1:") or command.startswith("!M2:") or (command.startswith("!") and ("M1:" in command or "M2:" in command)):
            print(f"🚫 Motor kontrol komutu devre dışı: {command}")
            print("⚠️ Spektral filtreleme şu anda devre dışı - servo kontrolü kapalı")
            return
        
        # Multi-spektral filtreleme komutları (Gereksinim 35) - DEVRE DIŞI
        if command.startswith("!") and command.endswith("!"):
            # Komut formatı: !6R7G! (4 haneli: Rakam-Harf-Rakam-Harf)
            komut_ici = command[1:-1]  # ! işaretlerini kaldır
            
            print(f"🚫 Multi-spektral filtreleme komutu devre dışı: {komut_ici}")
            print("⚠️ Spektral filtreleme şu anda devre dışı - servo kontrolü kapalı")
            return
        
    except Exception as e:
        print(f"❌ Komut işleme hatası: {e}")

def telemetry_sending_worker(
    sensor_yonetici,
    telemetri_isleyici,
    aktuator_yoneticisi,
    haberlesme_yoneticisi,
    data_queue,
    iot_xbee_alici=None
):
    """
    DÜZELTILMIŞ: Sensör verilerini periyodik olarak okuyan, telemetri oluşturan,
    gönderen ve SD kart için kuyruğa ekleyen iş parçacığı.
    """
    print("🔄 Telemetri gönderme thread'i başlatıldı")
    
    # Gereksinim 24: 10 saniye timer değişkenleri
    kurtarma_baslangic_zamani = None
    kurtarma_modu_aktif = False
    
    # 🔧 DEBUG: Counter ve error tracking
    telemetri_counter = 0
    consecutive_errors = 0
    max_consecutive_errors = 3  # 5 → 3 (daha hızlı fail)
    
    while not stop_event.is_set():
        try:
            telemetri_counter += 1
            if telemetri_counter % 10 == 1:  # Her 10 döngüde bir detaylı log
                print(f"🔄 Telemetri döngüsü #{telemetri_counter}")
            
            # 1. Sensörlerden veri oku
            sensor_verisi = sensor_yonetici.oku_tum_sensorler()

            # 1.5. IoT istasyonlarından sıcaklık verilerini al (HIZLI GEÇIŞ)
            iot_s1_data, iot_s2_data = None, None

            # 2. Telemetri paketini oluştur (HIZLI)
            telemetri_paketi_dict = None
            
            try:
                telemetri_paketi_dict = telemetri_isleyici.olustur_telemetri_paketi(
                    sensor_verisi,
                    iot_s1_data,
                    iot_s2_data
                )
            except Exception as telemetri_error:
                print(f"  🚨 TELEMETRI OLUŞTURMA HATASI: {telemetri_error}")
                # EMERGENCY: Manuel telemetri paketi oluştur
                emergency_data = f"286570,{telemetri_counter},T+{telemetri_counter:03d}:00:00,{sensor_verisi.get('basinc', 0)},25.0,0.0,0.0,0.0,0.0,0.0,7.4,0.0,0.0,0.0,0.0,0.0,0.0,0.0,0.0,0.0,286570"
                telemetri_paketi_dict = {
                    'ham_veri': emergency_data,
                    'xbee_paketi': f"${emergency_data}*00"
                }
                print(f"  🆘 EMERGENCY telemetri paketi oluşturuldu")
            
            if telemetri_paketi_dict:
                ham_veri = telemetri_paketi_dict['ham_veri']
                
                # 3. SD KARTA KAYDET (HIZLI)
                try:
                    data_queue.put(ham_veri, timeout=0.5)  # Hızlı timeout
                except:
                    # SD kuyruk doluysa direkt dosyaya yaz
                    with open(f"emergency_{telemetri_counter}.csv", "w") as f:
                        f.write(ham_veri + '\n')
                
                # 4. XBee'ye gönder (HIZLI)
                try:
                    haberlesme_yoneticisi.send_telemetry(telemetri_paketi_dict['xbee_paketi'])
                except:
                    pass  # Sessiz hata - SD zaten kaydedildi
                
                # Başarılı işlem sonrası error counter'ı sıfırla
                consecutive_errors = 0
                
                if telemetri_counter % 10 == 0:  # Her 10 pakette bir rapor
                    print(f"  ✅ Telemetri #{telemetri_counter} (10 paket gönderildi)")
            else:
                print("  ❌ Telemetri paketi oluşturulamadı")
                consecutive_errors += 1
                
                if consecutive_errors >= max_consecutive_errors:
                    print(f"  🚨 {max_consecutive_errors} ardışık telemetri hatası! 5s bekle...")
                    time.sleep(5)
                    consecutive_errors = 0

            # 5. Aktüatör kontrolü (BASIT)
            try:
                if telemetri_isleyici.uydu_statusu == 5:  # Kurtarma Modu
                    if not kurtarma_modu_aktif:
                        kurtarma_baslangic_zamani = time.time()
                        kurtarma_modu_aktif = True
                        print("  🚨 KURTARMA MODU: 10 saniye timer başlatıldı!")
                    
                    aktuator_yoneticisi.buzzer_kontrol(True)
                    
                    # 10 saniye kontrolü
                    gecen_sure = time.time() - kurtarma_baslangic_zamani
                    if gecen_sure >= 10.0:
                        print("  🚨 KURTARMA MODU: 10 saniye tamamlandı. ÇIKIŞ!")
                        break
                    else:
                        kalan_sure = 10.0 - gecen_sure
                        print(f"  ⏰ KURTARMA: {kalan_sure:.1f}s kaldı")
                else:
                    aktuator_yoneticisi.buzzer_kontrol(False)
            except Exception as actuator_error:
                print(f"  ⚠️ Aktüatör kontrol hatası: {actuator_error}")

            # 6. Sleep (1 Hz için optimize)
            time.sleep(TELEMETRI_GONDERIM_SIKLIGI)
            
        except Exception as e:
            consecutive_errors += 1
            print(f"🔧 GENEL HATA #{consecutive_errors}: {e}")
            
            if consecutive_errors >= max_consecutive_errors:
                print(f"🚨 {max_consecutive_errors} ardışık hata! 10s bekle...")
                time.sleep(10)
                consecutive_errors = 0
            else:
                print(f"🔧 {3 * consecutive_errors}s bekleyip devam...")
                time.sleep(3 * consecutive_errors)
            
            # 🔧 KRİTİK: Thread'in ölmesini ASLA İZİN VERME
            continue
    
    print(f"🔄 Telemetri thread sonlandı (toplam {telemetri_counter} döngü)")

def sd_logging_worker(data_queue, sd_kayitci):
    """
    Veri kuyruğundan gelen telemetri paketlerini SD karta kaydeden iş parçacığı.
    """
    print("📁 SD kart kayıt iş parçacığı başlatıldı.")
    kayit_sayaci = 0
    
    while not stop_event.is_set():
        try:
            # Kuyruktan veri almayı bekle (1 saniye zaman aşımı ile)
            # Bu, program sonlandığında thread'in takılı kalmasını engeller.
            telemetri_paketi = data_queue.get(timeout=1)
            kayit_sayaci += 1
            
            print(f"📥 SD Worker: Paket #{kayit_sayaci} alındı")
            
            # SD karta kaydet
            basarili = sd_kayitci.kaydet_telemetri(telemetri_paketi)
            if basarili:
                print(f"✅ SD'YE KAYDEDİLDİ #{kayit_sayaci}: {telemetri_paketi.strip()}")
            else:
                print(f"❌ SD KAYIT HATASI #{kayit_sayaci}: {telemetri_paketi.strip()}")
            
            data_queue.task_done()
            
        except queue.Empty:
            # Kuyruk boşsa sorun değil, döngüye devam et ve stop_event'i kontrol et
            continue
        except Exception as e:
            print(f"🔧 HATA: SD kart kayıt döngüsünde beklenmeyen hata: {e}")
            import traceback
            traceback.print_exc()
            # Hata olsa bile devam et
            time.sleep(0.5)
    
    print(f"📁 SD kart kayıt iş parçacığı sonlandırıldı. Toplam {kayit_sayaci} kayıt yapıldı.")

def main():
    """Ana program fonksiyonu"""
    global is_running, program_start_time
    
    # 🔧 PROGRAM BAŞLANGIÇ ZAMANI TRACKING
    program_start_time = time.time()
    
    print("🚀 TÜRKSAT Model Uydu Sistemi Başlatılıyor...")
    print("📅 Başlatma Zamanı:", datetime.now().strftime("%Y-%m-%d %H:%M:%S"))
    
    # Sistem performans optimizasyonları
    try:
        from moduller.yapilandirma import apply_performance_settings, setup_logging
        apply_performance_settings()
        setup_logging()
        print("✅ Sistem optimizasyonları uygulandı")
    except Exception as e:
        print(f"⚠️ Performans ayarları uygulanamadı: {e}")
    
    # Sistem sağlık kontrolü
    try:
        from moduller.yapilandirma import check_system_health
        health_warnings = check_system_health()
        if health_warnings:
            print("⚠️ Sistem Sağlık Uyarıları:")
            for warning in health_warnings:
                print(f"   {warning}")
        else:
            print("✅ Sistem sağlığı normal")
    except Exception as e:
        print(f"⚠️ Sistem sağlık kontrolü yapılamadı: {e}")
    
    global is_running
    
    haberlesme_yoneticisi = None
    kamera_yonetici = None
    sd_kayitci = None
    sensor_yonetici = None  # 🔧 DÜZELTME: Variable scope için ön tanımlama
    # threads = []  # 🔧 DÜZELTME: Duplicate definition kaldırıldı - global threads (satır 37) kullanılıyor
    
    # 🔧 KRİTİK: Ana sistem başlatma - fallback mekanizması ile
    try:
        print("🚀 TÜRKSAT Model Uydu Sistemi başlatılıyor...")
        print("=" * 50)
        
        # Platform detection
        is_simulation = not IS_RASPBERRY_PI or FORCE_SIMULATION  # 🧪 Windows test modu desteği
        if FORCE_SIMULATION:
            print("🧪 WINDOWS TEST MODU: Simülasyon zorla aktifleştirildi")
        elif is_simulation:
            print("⚠️  UYARI: Raspberry Pi tespit edilmedi, simülasyon modu aktif")
        else:
            print("✅ Raspberry Pi tespit edildi, gerçek donanım modu")
        
        # Kuyrukları oluştur
        telemetry_queue = queue.Queue()

        # 🔧 KRİTİK MODÜL 1: SD Kayıt Sistemi
        try:
            sd_kayitci = SDKayitci()
            print("✅ SD Kayıt sistemi başlatıldı")
        except Exception as e:
            print(f"❌ KRITIK: SD Kayıt sistemi başlatılamadı: {e}")
            print("🔧 FALLBACK: Sistem sadece telemetri gönderim modunda çalışacak")
            sd_kayitci = None
        
        # 🔥 TEK XBee MODÜLÜ: Birleşik XBee Alıcı Sistemi
        try:
            birlesik_xbee = BirlesikXBeeAlici(
                command_callback=komut_isle, 
                debug=False, 
                simulate=is_simulation
            )
            if birlesik_xbee.connect_xbee():
                birlesik_xbee.start_listening()
                print("✅ Birleşik XBee sistemi başlatıldı (SAHA + IoT + Komutlar)")
            else:
                print("❌ UYARI: Birleşik XBee bağlantısı kurulamadı")
                print("🔧 FALLBACK: XBee'siz telemetri modu - Sadece SD kart kaydı aktif")
                # Sahte SAHA alıcı oluştur
                class SahteSAHA:
                    def __init__(self):
                        self.simulate = True
                    def get_tasiyici_basinici(self):
                        return 1015.0
                    def is_basinc2_available(self):
                        return False
                    def get_basinc2_value(self):
                        return 1015.0
                    def get_iot_temperatures(self):
                        return (24.5, 25.2)  # Simülasyon sıcaklık değerleri
                    def send_telemetry(self, telemetri_data):
                        print(f"📡 SİMÜLASYON: Telemetri gönderildi (XBee yok): {telemetri_data[:50]}...")
                        return True
                    def start_listening(self):
                        pass
                    def stop_listening(self):
                        pass
                birlesik_xbee = SahteSAHA()
                print("✅ Sahte SAHA alıcı oluşturuldu - Sistem XBee'siz çalışacak")
        except Exception as e:
            print(f"❌ KRITIK: Birleşik XBee sistemi başlatılamadı: {e}")
            print("🔧 FALLBACK: Simülasyon modunda çalışacak")
            birlesik_xbee = BirlesikXBeeAlici(
                command_callback=komut_isle,
                debug=False, 
                simulate=True
            )
            birlesik_xbee.connect_xbee()
            birlesik_xbee.start_listening()
        
        # 🔧 KRİTİK MODÜL 3: Sensör Yöneticisi
        try:
            sensor_yonetici = SensorManager(saha_alici_instance=birlesik_xbee, simulate=is_simulation) 
            print("✅ Sensör Yöneticisi başlatıldı")
            
            # DS3231 RTC senkronizasyonu kontrolü
            sensor_yonetici.sync_system_time_with_rtc()
            
        except Exception as e:
            print(f"❌ KRITIK: Sensör sistemi başlatılamadı: {e}")
            print("🔧 FALLBACK: Tamamen simülasyon modunda çalışacak")
            sensor_yonetici = SensorManager(saha_alici_instance=birlesik_xbee, simulate=True)
        
        # 🔧 KRİTİK MODÜL 4: Telemetri İşleyici
        try:
            telemetri_isleyici = TelemetryHandler(saha_alici=birlesik_xbee)
            print("✅ Telemetri İşleyici başlatıldı (SAHA entegrasyonu aktif)")
        except Exception as e:
            print(f"❌ FATALh HATA: Telemetri İşleyici başlatılamadı: {e}")
            print("🚨 Sistem telemetri olmadan çalışamaz!")
            return
        
        # 🔧 KRİTİK MODÜL 5: Aktüatör Yöneticisi
        try:
            aktuator_yoneticisi = AktuatorYoneticisi(simulate=is_simulation)
            # XBee referansını aktuatör yöneticisine geç (multispektral görev tamamlandı mesajı için)
            aktuator_yoneticisi.birlesik_xbee = birlesik_xbee
            print("✅ Aktüatör Yöneticisi başlatıldı (XBee entegrasyonu aktif)")
        except Exception as e:
            print(f"❌ UYARI: Aktüatör sistemi başlatılamadı: {e}")
            print("🔧 FALLBACK: Simülasyon modunda çalışacak")
            aktuator_yoneticisi = AktuatorYoneticisi(simulate=True)
            # Fallback durumunda da XBee referansını ver
            aktuator_yoneticisi.birlesik_xbee = birlesik_xbee
        
        # 6. Güç yöneticisi (Gereksinim 22, 20)
        guc_yoneticisi = None
        try:
            guc_yoneticisi = GucYoneticisi(
                simulate=is_simulation, 
                aktuator_yoneticisi=aktuator_yoneticisi, 
                shutdown_callback=shutdown_callback
            )
            
            # Pil yöneticisi ile entegrasyon (Gereksinim 20)
            if sensor_yonetici and hasattr(sensor_yonetici, 'pil_yoneticisi'):
                if sensor_yonetici.pil_yoneticisi:
                    guc_yoneticisi.set_battery_manager(sensor_yonetici.pil_yoneticisi)
                    print("🔋 Pil izleme sistemi güç yöneticisi ile entegre edildi")
            
            guc_yoneticisi.start_monitoring()
            print("✅ Güç yöneticisi başlatıldı ve izleme aktif")
        except Exception as e:
            print(f"❌ HATA: Güç yöneticisi başlatılamadı: {e}")
            guc_yoneticisi = None
        
        # ✅ IoT verilerini artık birleşik XBee modülü üzerinden alıyoruz
        # Ayrı IoT XBee modülü kullanmıyoruz - tek XBee tüm işleri yapıyor
        # ✅ IoT verilerini artık birleşik XBee modülü üzerinden alıyoruz
        iot_xbee_alici = birlesik_xbee  # IoT verileri için birleşik modülü kullan
        print("✅ IoT verisi birleşik XBee modülü üzerinden alınacak")
        print(f"   Port: {SERIAL_PORT_XBEE} (Tek XBee - Mesh network)")
        print("   Hedef mesafe: 412-707m (IoT istasyonları → Görev yükü)")
        
        # 🔧 KRİTİK MODÜL 6: Kamera Sistemi (GERÇEK DONANIM MODU)
        try:
            print("🎥 Kamera sistemi başlatılıyor...")
            print(f"🔍 Debug: is_simulation = {is_simulation}")
            kamera_yonetici = KameraYoneticisi(simulate=False)  # ZORLA GERÇEK KAMERA
            
            # Video kayıt yolu - SD kayıtçı varsa al
            if sd_kayitci:
                video_dosya_yolu = sd_kayitci.get_video_kayit_yolu()
            else:
                video_dosya_yolu = f"./temp_video_{int(time.time())}.mp4"
                
            # 🎥 Video kaydını başlat (BASİT VERSİYON)
            print("🎥 Video kaydı başlatılıyor...")
            kamera_yonetici.baslat_kayit(video_dosya_yolu)
            print(f"✅ Video kaydı başlatıldı: {video_dosya_yolu}")
                
            print("✅ Kamera sistemi başlatıldı")
        except Exception as e:
            print(f"❌ UYARI: Kamera sistemi başlatılamadı: {e}")
            print("🔧 FALLBACK: Simülasyon modunda çalışacak")
            kamera_yonetici = KameraYoneticisi(simulate=True)
        
        # ✅ Haberleşme artık birleşik XBee modülü üzerinden yapılıyor
        haberlesme_yoneticisi = birlesik_xbee  # Tek XBee ile haberleşme
        
        # Kamera canlı yayın bağlantısı - GÜVENLİ MOD
        try:
            if kamera_yonetici:
                print("📹 Video streaming başlatılıyor...")
                # Video streaming'i ayrı thread'de başlat (ana programı etkilemesin)
                import threading
                def video_streaming_wrapper():
                    try:
                        kamera_yonetici.baslat_canli_yayin(haberlesme_yoneticisi.send_telemetry)
                        print("✅ Canlı video yayını başlatıldı")
                    except Exception as ve:
                        print(f"❌ Video streaming thread hatası: {ve}")
                
                video_thread = threading.Thread(target=video_streaming_wrapper, daemon=True)
                video_thread.start()
                print("📹 Video streaming thread başlatıldı")
        except Exception as e:
            print(f"❌ UYARI: Canlı video yayını başlatılamadı: {e}")
            print("🔧 FALLBACK: Sadece video kayıt çalışacak")
        
        print("✅ Haberleşme sistemi birleşik XBee modülü ile aktif")

        # Ana görevler için iş parçacıklarını oluştur ve başlat
        telemetry_thread = threading.Thread(
            target=telemetry_sending_worker,
            args=(sensor_yonetici, telemetri_isleyici, aktuator_yoneticisi, haberlesme_yoneticisi, telemetry_queue, iot_xbee_alici)
        )
        
        # SD thread sadece SD kayıtçı mevcutsa oluştur
        sd_thread = None
        if sd_kayitci:
            sd_thread = threading.Thread(
                target=sd_logging_worker,
                args=(telemetry_queue, sd_kayitci)
            )
            threads.append(sd_thread)
        else:
            print("⚠️ SD kayıt sistemi yok, sadece telemetri gönderimi aktif")
        
        threads.append(telemetry_thread)
        
        # Thread'leri başlat
        telemetry_thread.start()
        if sd_thread:
            sd_thread.start()

        print("=" * 50)
        print("🚀 TÜRKSAT Model Uydu sistemi başarıyla başlatıldı!")
        print("✅ Tüm kritik modüller çalışıyor")
        print("📡 Telemetri gönderimi aktif")
        if sd_kayitci:
            print("💾 SD kart kaydı aktif")
        print("Çıkmak için CTRL+C'ye basın.")
        print("=" * 50)
        
        # 🔧 ÇÖZÜM: Sonsuz döngü ekle - program sürekli çalışsın
        print("🔄 Ana döngü başlatılıyor... (CTRL+C ile durdurun)")
        
        try:
            # Sonsuz döngü - thread'lerin durumunu kontrol et
            last_status_time = time.time()
            
            # Video kontrol değişkeni ekle
            video_check_time = time.time()
            
            while not stop_event.is_set():
                current_time = time.time()
                
                # 🎥 VIDEO KAYIT KONTROL (Her 10 saniyede)
                if current_time - video_check_time >= 10:
                    try:
                        if kamera_yonetici and not kamera_yonetici.is_recording:
                            print("🔧 Video kaydı yeniden başlatılıyor...")
                            if sd_kayitci:
                                video_dosya_yolu = sd_kayitci.get_video_kayit_yolu()
                            else:
                                video_dosya_yolu = f"./temp_video_{int(time.time())}.mp4"
                            kamera_yonetici.baslat_kayit(video_dosya_yolu)
                            print(f"✅ Video kaydı yeniden başlatıldı: {video_dosya_yolu}")
                    except Exception as video_error:
                        print(f"⚠️ Video kontrol hatası: {video_error}")
                    video_check_time = current_time
                
                # Thread'lerin canlı olup olmadığını kontrol et
                active_threads = [t for t in threads if t and t.is_alive()]
                
                # Eğer kritik thread'ler öldüyse uyar ama devam et
                if len(active_threads) != len([t for t in threads if t is not None]):
                    dead_threads = len([t for t in threads if t is not None]) - len(active_threads)
                    print(f"⚠️ {dead_threads} thread öldü ama sistem devam ediyor...")
                
                # Her 30 saniyede bir durum bilgisi ver
                if current_time - last_status_time >= 30:
                    uptime = int(current_time - program_start_time)
                    print(f"✅ Sistem çalışıyor: {len(active_threads)}/{len([t for t in threads if t is not None])} thread aktif")
                    print(f"📊 Çalışma süresi: {uptime} saniye ({uptime//60}:{uptime%60:02d})")
                    last_status_time = current_time
                
                # 5 saniye bekle ve tekrar kontrol et
                time.sleep(5)
                
        except KeyboardInterrupt:
            print("\n🛑 CTRL+C ile program durduruldu")
        except Exception as main_loop_error:
            print(f"🔧 Ana döngü hatası: {main_loop_error}")
            print("🔄 Sistem stabil kalıyor...")
            time.sleep(5)  # 5 saniye bekle ve devam et
        
        # Program sonlanırken thread'leri nazikçe bekle
        print("🧹 Thread'ler temizleniyor...")
        for t in threads:
            if t and t.is_alive():
                t.join(timeout=3)  # Thread başına 3 saniye bekle

    except KeyboardInterrupt:
        print("\n🛑 CTRL+C algılandı. Program sonlandırılıyor...")
    except Exception as fatal_error:
        print(f"\n🚨 FATALhATA: {fatal_error}")
        print("🔧 Sistem beklenmeyen bir hatayla karşılaştı")
    finally:
        # Program sonlanırken tüm kaynakları temizle
        print("🔧 Tüm iş parçacıkları durduruluyor...")
        stop_event.set()
        
        # 🎥 VIDEO KAYIT DURDUR
        if 'kamera_yonetici' in locals() and kamera_yonetici:
            try:
                print("🎥 Video kaydı durduruluyor...")
                kamera_yonetici.durdur_kayit()
                print("✅ Video kaydı durduruldu")
            except Exception as e:
                print(f"UYARI: Video durdurma hatası: {e}")

        # Thread'lerin düzgünce sonlanmasını bekle
        for t in threads:
            if t and t.is_alive():
                t.join(timeout=8)  # 2s → 8s - 8 saniye bekle, sonra devam et

        # Modülleri güvenli şekilde kapat
        if saha_alici:
            try:
                saha_alici.durdur()
                print("✅ SAHA Alıcı durduruldu")
            except:
                pass

        if iot_xbee_alici:
            try:
                iot_xbee_alici.stop_receiving()
                print("✅ IoT XBee Alıcı durduruldu")
            except:
                pass

        if kamera_yonetici:
            try:
                kamera_yonetici.durdur_kayit()
                kamera_yonetici.temizle()
                print("✅ Kamera sistemi durduruldu")
            except:
                pass

        if haberlesme_yoneticisi:
            try:
                haberlesme_yoneticisi.stop()
                print("✅ Haberleşme sistemi durduruldu")
            except:
                pass

        if aktuator_yoneticisi:
            try:
                aktuator_yoneticisi.temizle()
                print("✅ Aktüatör sistemi durduruldu")
            except:
                pass

        if guc_yoneticisi:
            try:
                guc_yoneticisi.cleanup()
                print("✅ Güç yöneticisi durduruldu")
            except:
                pass

        print("🚀 TÜRKSAT Model Uydu sistemi güvenli şekilde sonlandırıldı!")
        print("=" * 50)

if __name__ == "__main__":
    try:
        print("=" * 60)
        print("🛰️  TÜRKSAT MODEL UYDU PROJESİ v6.0")
        print("📡 Görev Yükü Ana Sistemi Başlatılıyor...")
        print("=" * 60)
        
        # 🔧 Pre-flight system checks
        print("Sistem ön kontrolleri yapılıyor...")
        
        # Python version check
        import sys
        print(f"✅ Python {sys.version.split()[0]} tespit edildi")
        
        # Critical imports check
        essential_modules = [
            ('threading', 'Thread management'),
            ('queue', 'Data queuing'),
            ('time', 'Time management'),
            ('json', 'Configuration files')
        ]
        
        for module_name, description in essential_modules:
            try:
                __import__(module_name)
                print(f"✅ {description} - OK")
            except ImportError as e:
                print(f"❌ {description} - FAILED: {e}")
                sys.exit(1)
        
        # Project modules check
        try:
            from moduller import yapilandirma
            print(f"✅ Proje konfigürasyonu - OK (Takım: {yapilandirma.TAKIM_NUMARASI})")
        except ImportError as e:
            print(f"❌ Proje modülleri yüklenemedi: {e}")
            print("🔧 moduller/ klasörünün Python path'inde olduğundan emin olun")
            sys.exit(1)
        
        print("✅ Sistem ön kontrolleri başarılı!")
        print("🚀 Ana sistem başlatılıyor...\n")
        
        # Ana sistemi başlat
        main()
        
    except KeyboardInterrupt:
        print("\n🛑 Başlatma işlemi kullanıcı tarafından iptal edildi")
    except Exception as startup_error:
        print(f"\n🚨 Sistem başlatma hatası: {startup_error}")
        print("🔧 Lütfen hata loglarını kontrol edin")
        sys.exit(1)
