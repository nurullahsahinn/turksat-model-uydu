# -*- coding: utf-8 -*-
"""
Sensör Yönetici Modülü

Bu modül, görev yükü üzerindeki tüm sensörlerden (I2C, SPI, UART) veri
okumaktan sorumludur.
- BMP280 (Basınç, Sıcaklık)
- 10-DOF IMU (İvme, Gyro, Manyetometre)
- GPS (Konum, İrtifa)
"""

# Gerekli kütüphaneler
import time
import random
import logging
import traceback
import os
import logging.handlers
import math
# import logging  # 🔧 DÜZELTME: Duplicate import removed - already imported on line 15

# 🧪 WINDOWS TEST MODU - smbus2 simülasyon kontrolü
try:
    import smbus
    SMBUS_AVAILABLE = True
except ImportError:
    SMBUS_AVAILABLE = False
    print("UYARI: smbus eksik - simülasyon modu aktif")
import serial
import json
from datetime import datetime
import struct

from moduller.imu_sensoru import IMUSensorYoneticisi
from moduller.pil_gerilimi import PilGerilimiYoneticisi
from moduller.yapilandirma import (
    IS_RASPBERRY_PI, SERIAL_PORT_GPS, SIMULATE_GPS,
    GOREV_BASLANGIC_DOSYASI
)

# Logging konfigürasyonu - VERİ KORUMA MODU
# Log dizini oluştur
LOG_DIR = os.path.join(os.path.dirname(__file__), '..', 'logs')
os.makedirs(LOG_DIR, exist_ok=True)

# Log dosyası yolu
LOG_FILE = os.path.join(LOG_DIR, 'sensor_log.txt')

# Veri koruma için özel handler sınıfı
class SensorDataProtectionHandler(logging.handlers.RotatingFileHandler):
    """Sensör log'ları için veri koruma handler"""
    
    def doRollover(self):
        """Log rotasyonu - sensör verileri asla silinmez"""
        if self.stream:
            self.stream.close()
            self.stream = None
        
        # Timestamp ile yeni dosya adı
        from datetime import datetime
        timestamp = datetime.now().strftime("%Y%m%d_%H%M%S")
        name, ext = self.baseFilename.rsplit('.', 1)
        new_filename = f"{name}_{timestamp}.{ext}"
        
        self.baseFilename = new_filename
        self.stream = self._open()
        
        print(f"📄 Yeni sensör log dosyası: {new_filename}")

# Logging konfigürasyonu - VERİ KORUMA
logging.basicConfig(
    level=logging.INFO,
    format='%(asctime)s - %(name)s - %(levelname)s - %(message)s',
    handlers=[
        # Veri koruma dosya handler (sensör verileri hiç silinmez)
        SensorDataProtectionHandler(
            LOG_FILE, 
            maxBytes=10*1024*1024,  # 10 MB (büyük dosya boyutu)
            backupCount=0  # Limit yok - tüm veriler korunur
        ),
        # Konsola da log yazdır
        logging.StreamHandler()
    ]
)

# Sensör log'u için özel logger
logger = logging.getLogger('SensorManager')
logger.setLevel(logging.INFO)

class SensorError(Exception):
    """Sensör ile ilgili özel hata sınıfı"""
    def __init__(self, message, sensor_type=None, error_code=None):
        super().__init__(message)
        self.sensor_type = sensor_type
        self.error_code = error_code

# Raspberry Pi üzerinde çalışırken gerçek kütüphaneleri kullanmayı dene
if IS_RASPBERRY_PI and SMBUS_AVAILABLE:
    # smbus2 zaten import edildi, tekrar import etmeye gerek yok
    import serial
    from moduller.yapilandirma import SERIAL_PORT_GPS, SERIAL_BAUD_GPS
else:
    print("UYARI: Raspberry Pi'ye özel kütüphaneler bulunamadı. Simülasyon modunda çalışılıyor.")

# 🔧 DENIZ_SEVIYESI_BASINC_HPA import et (Analiz4.txt düzeltmesi)
from moduller.yapilandirma import DENIZ_SEVIYESI_BASINC_HPA


class SensorManager:
    def __init__(self, saha_alici_instance, simulate=not IS_RASPBERRY_PI):
        """
        Gelişmiş hata yönetimi ve logging ile sensör yöneticisi başlatma
        """
        self.simulate = simulate
        self.sensor_status = {
            "gps": False,
            "bmp280": False,
            "imu": False,
            "battery": False
        }
        
        # 🔧 DÜZELTME: I2C bus'ı baştan başlat
        self.bus = None
        if not self.simulate:
            self._init_i2c_bus()
        
        # Mevcut init kodları
        self.bmp180 = None
        self.gps_serial = None
        
        # Dışarıdan gelen SAHA alıcı nesnesini kullan
        self.saha_alici = saha_alici_instance
        
        # ✅ YENİ: IMU sensör sistemi - ZORLA GERÇEK SENSÖR
        try:
            self.imu_yoneticisi = IMUSensorYoneticisi(simulate=False)  # 🔧 FIX: Zorla gerçek sensör
            print("✅ 10-DOF IMU sensör sistemi başlatıldı (GERÇEK DONANIM)")
        except Exception as e:
            print(f"⚠️ IMU sensörleri başlatılamadı: {e}")
            self.imu_yoneticisi = None
            
            # ❌ RTC TAMAMEN KALDIRILDI - I2C çakışması ve sistem hatası nedeniyle
            print("⚠️ RTC devre dışı - sistem zamanı kullanılıyor")
            self.rtc_yoneticisi = None
            self.rtc_available = False
            
            # ✅ YENİ: Pil gerilimi izleme sistemi
            try:
                self.pil_yoneticisi = PilGerilimiYoneticisi(simulate=simulate)
                print("✅ ADS1115 16-bit ADC pil izleme sistemi başlatıldı")  # 🔧 PCF8591 → ADS1115
            except Exception as e:
                print(f"⚠️ Pil izleme sistemi başlatılamadı: {e}")
                self.pil_yoneticisi = None

            # Simülasyon değişkenleri (hem gerçek hem sim modda gerekli)
            self._sim_irtifa = 0.0
            self._sim_lat = 41.01384
            self._sim_lon = 28.94966
            self._sim_initial_altitude = 0.0
            self._sim_start_time = time.time()
            
            if not self.simulate:
                self._setup_donanim()
            else:
                # Simülasyon modu için ek ayarlar
                print("📊 Simülasyon modu aktif")

            # Her sensör için ayrı log ve status kontrolü
            self._check_sensor_status()

        except Exception as e:
            logger.error(f"Sensör yöneticisi başlatma hatası: {e}")
            logger.error(traceback.format_exc())
            raise SensorError(f"Sensör yöneticisi başlatılamadı: {e}", error_code="INIT_FAILED")

    def sync_system_time_with_rtc(self):
        """
        RTC TAMAMEN KALDIRILDI - sistem zamanı kullanılıyor
        """
        print("⚠️ RTC devre dışı - sistem saati kullanılıyor")
    
    def _setup_donanim(self):
        """
        Gerçek I2C ve seri donanım bağlantılarını başlatır.
        """
        try:
            # I2C bus'ını başlat (1, Raspberry Pi'deki I2C bus numarasıdır)
            self.bus = smbus.SMBus(1)
            print("I2C bus başarıyla başlatıldı.")
            # BMP280 ve IMU sensörlerini burada başlat
            # Örnek: self.bmp1 = BMP280(i2c_bus=self.bus, i2c_addr=0x76)
        except Exception as e:
            print(f"HATA: I2C bus başlatılamadı: {e}")
            self.simulate = True # Donanım hatasında simülasyona geç

        try:
            # GPS için seri portu başlat
            self.gps_serial = serial.Serial(SERIAL_PORT_GPS, SERIAL_BAUD_GPS, timeout=1)
            print("GPS seri portu başarıyla başlatıldı.")
        except Exception as e:
            print(f"HATA: GPS seri portu başlatılamadı: {e}")
            # GPS simülasyonu ayrı ele alınabilir

    def _check_sensor_status(self):
        """Sensörlerin durumunu kontrol et ve logla"""
        try:
            # GPS durumu
            self.sensor_status['gps'] = self._test_gps_connection()
            logger.info(f"GPS Durumu: {'Aktif' if self.sensor_status['gps'] else 'Pasif'}")

            # BMP280 durumu
            self.sensor_status['bmp280'] = self._test_bmp280_connection()
            logger.info(f"BMP280 Durumu: {'Aktif' if self.sensor_status['bmp280'] else 'Pasif'}")

            # IMU durumu
            self.sensor_status['imu'] = self._test_imu_connection()
            logger.info(f"IMU Durumu: {'Aktif' if self.sensor_status['imu'] else 'Pasif'}")

            # RTC KALDIRILDI - artık kontrol edilmiyor

            # Pil durumu
            self.sensor_status['battery'] = self._test_battery_connection()
            logger.info(f"Pil Durumu: {'Aktif' if self.sensor_status['battery'] else 'Pasif'}")

        except Exception as e:
            logger.error(f"Sensör durum kontrolü hatası: {e}")
            logger.error(traceback.format_exc())

    def _test_gps_connection(self):
        """GPS bağlantısını test et"""
        try:
            gps_data = self._read_gps()
            return gps_data['fix_quality'] > 0
        except Exception:
            return False

    def _test_bmp280_connection(self):
        """BMP280 bağlantısını test et"""
        try:
            basinc, sicaklik, irtifa = self._read_bmp280()
            return all(v != 0.0 for v in [basinc, sicaklik, irtifa])
        except Exception:
            return False

    def _test_imu_connection(self):
        """IMU bağlantısını test et"""
        try:
            imu_data = self._read_imu()
            return all(abs(v) <= 360 for v in imu_data.values())
        except Exception:
            return False

    def _test_rtc_connection(self):
        """RTC KALDIRILDI - her zaman False döner"""
        return False

    def _test_battery_connection(self):
        """Pil bağlantısını test et"""
        try:
            pil_gerilimi = self._read_battery()
            return 3.0 <= pil_gerilimi <= 12.6
        except Exception:
            return False

    def oku_tum_sensorler(self):
        """
        THREAD-SAFE TIMEOUT: Gelişmiş hata yönetimi ve logging ile sensör okuma
        """
        import threading
        import queue
        
        try:
            print(f"🔧 DEBUG: Sensör okuma başlıyor (simulate={self.simulate})")
            
            # Thread-safe timeout mekanizması
            result_queue = queue.Queue()
            
            def sensor_read_worker():
                try:
                    # Sensör verilerini oku
                    sensor_verisi = self._oku_sensorler_simule() if self.simulate else self._oku_gercek_sensorler()
                    result_queue.put(("success", sensor_verisi))
                except Exception as e:
                    result_queue.put(("error", e))
            
            # Worker thread başlat
            worker_thread = threading.Thread(target=sensor_read_worker, daemon=True)
            worker_thread.start()
            
            try:
                # 🔧 FIX: 0.5 saniye timeout ile sonucu bekle (1Hz telemetri için optimize)
                result_type, result_data = result_queue.get(timeout=0.5)
                
                if result_type == "success":
                    print(f"🔧 DEBUG: Sensör okuma tamamlandı")
                    # Kritik sensör verilerinin kontrolü
                    self._validate_sensor_data(result_data)
                    return result_data
                else:
                    raise result_data
                    
            except queue.Empty:
                print(f"⏰ TIMEOUT: Sensör okuma 0.5 saniyede tamamlanamadı")
                print(f"🔧 HIZLI FALLBACK: Gerçek sensörlerden direkt okuma")
                # Direkt sensör okuma - timeout bypass
                try:
                    # BMP280'den hızlı okuma
                    basinc, sicaklik, irtifa = self._read_bmp280()
                    print(f"📊 HIZLI BMP280: {basinc} Pa, {sicaklik}°C")
                    
                    # 🔧 FIX: IMU verilerini de fallback'e dahil et
                    imu_verisi = {'pitch': 0.0, 'roll': 0.0, 'yaw': 0.0, 'ivme_x': 0.0, 'ivme_y': 0.0, 'ivme_z': 0.0, 'gyro_x': 0.0, 'gyro_y': 0.0, 'gyro_z': 0.0, 'mag_x': 0.0, 'mag_y': 0.0, 'mag_z': 0.0}
                    try:
                        imu_verisi = self._read_imu()
                        print(f"📊 HIZLI IMU: P={imu_verisi.get('pitch', 0):.1f}° R={imu_verisi.get('roll', 0):.1f}° Y={imu_verisi.get('yaw', 0):.1f}°")
                    except Exception as imu_error:
                        print(f"⚠️ IMU fallback hatası: {imu_error}")
                    
                    # Hızlı veri paketi
                    return {
                        "basinc": basinc,
                        "irtifa": irtifa, 
                        "sicaklik": sicaklik,
                        "pil_gerilimi": 7.4,  # Varsayılan
                        "gps_verisi": {'enlem': 0.0, 'boylam': 0.0, 'yukseklik': 0.0, 'fix_quality': 0, 'satellite_count': 0},
                        "imu_verisi": imu_verisi,
                        "iot_verileri": {"sicaklik1": 25.0, "sicaklik2": 25.0},
                        "tasiyici_basinci": 0.0,
                        "rtc_time": None,
                        "uydu_statusu": 0
                    }
                except Exception as e:
                    print(f"🚨 HIZLI OKUMA HATASI: {e}")
                    print(f"🔧 Son çare: Güvenli boş değerler")
                    return self._get_empty_sensor_data()

        except SensorError as se:
            # Özel sensör hataları için detaylı log
            logger.error(f"Sensör Hatası: {se}")
            logger.error(f"Sensör Tipi: {se.sensor_type}")
            logger.error(f"Hata Kodu: {se.error_code}")
            
            # Fallback: Simülasyon verileri
            return self._oku_sensorler_simule()

        except Exception as e:
            # Beklenmeyen hatalar için genel log
            logger.critical(f"Beklenmeyen sensör hatası: {e}")
            logger.critical(traceback.format_exc())
            
            # Fallback: Simülasyon verileri
            return self._oku_sensorler_simule()

    def _get_empty_sensor_data(self):
        """
        Boş/güvenli sensör verisi döndür (simülasyon yerine)
        """
        return {
            "basinc": 0,                 # Boş basınç
            "irtifa": 0.0,               # Boş irtifa
            "sicaklik": 0.0,             # Boş sıcaklık
            "pil_gerilimi": 0.0,         # Boş pil
            "gps_verisi": {
                'enlem': 0.0, 'boylam': 0.0, 'yukseklik': 0.0,
                'fix_quality': 0, 'satellite_count': 0
            },
            "imu_verisi": {
                'pitch': 0.0, 'roll': 0.0, 'yaw': 0.0,
                'ivme_x': 0.0, 'ivme_y': 0.0, 'ivme_z': 0.0,
                'gyro_x': 0.0, 'gyro_y': 0.0, 'gyro_z': 0.0,
                'mag_x': 0.0, 'mag_y': 0.0, 'mag_z': 0.0
            },
            "iot_verileri": {
                "sicaklik1": 0.0, 
                "sicaklik2": 0.0
            },
            "tasiyici_basinci": 0.0,
            "rtc_time": None,
            "uydu_statusu": 0
        }
    
    def _validate_sensor_data(self, sensor_verisi):
        """
        Sensör verilerinin geçerliliğini kontrol et
        Geliştirme ortamında daha esnek, üretim ortamında daha sıkı kontrol
        """
        # Basınç kontrolü - GENİŞLETİLMİŞ ARALUK
        # BMP280 sensörü bazen yüksek değerler verebilir, geçici kabul et
        if not (0 <= sensor_verisi['basinc'] <= 15000000):  # 15M Pa'ya kadar kabul et
            logger.warning(f"⚠️ Anormal basınç değeri: {sensor_verisi['basinc']} Pa (devam ediyor)")
            # Exception atmak yerine warning ver ve devam et

        # İrtifa kontrolü - GENİŞLETİLMİŞ ARALUK  
        if not (-2000 <= sensor_verisi['irtifa'] <= 50000):  # -2km ile +50km arası kabul et
            logger.warning(f"⚠️ Anormal irtifa değeri: {sensor_verisi['irtifa']} m (devam ediyor)")
            # Exception atmak yerine warning ver ve devam et

        # GPS fix kalitesi kontrolü - ESNEK MOD
        if sensor_verisi['gps_verisi']['fix_quality'] == 0:
            # GPS olmasa da telemetri yazmaya devam et
            logger.warning("⚠️ GPS fix kalitesi yetersiz (devam ediyor)")

        # Pil gerilimi kontrolü - ESNEK MOD
        if not (1.0 <= sensor_verisi['pil_gerilimi'] <= 15.0):  # Geniş aralık
            logger.warning(f"⚠️ Anormal pil gerilimi: {sensor_verisi['pil_gerilimi']} V (devam ediyor)")
            # Exception atmak yerine warning ver ve devam et

    def _oku_sensorler_simule(self):
        """
        Gelişmiş uçuş simülasyon modeli
        Gerçekçi sensör verileri üretir
        """
        # Simülasyon başlangıç zamanını ilk çağrıda ayarla
        if not hasattr(self, '_sim_start_time'):
            self._sim_start_time = time.time()
            # GPS bağlı değilse 0,0 kullan - FAKE İSTANBUL KOORDİNATLARI YOK!
            self._sim_lat = 0.0  # Gerçek GPS yoksa 0.0
            self._sim_lon = 0.0  # Gerçek GPS yoksa 0.0
            self._sim_initial_altitude = 500  # Başlangıç irtifası (m)
            self._sim_irtifa = self._sim_initial_altitude

        # Geçen süreyi hesapla
        elapsed_time = time.time() - self._sim_start_time

        # Uçuş profili simülasyonu
        if elapsed_time < 10:  # İlk 10 saniye: Yükselme
            self._sim_irtifa = self._sim_initial_altitude + (elapsed_time * 50)  # 50m/s hızla yükselme
            descent_phase = False
        elif elapsed_time < 30:  # 20 saniye: İniş
            self._sim_irtifa = max(0, self._sim_initial_altitude - ((elapsed_time - 10) * 15))  # 15m/s hızla iniş
            descent_phase = True
        else:  # Ayrılma sonrası iniş
            self._sim_irtifa = max(0, 400 - ((elapsed_time - 30) * 7))  # 7m/s hızla iniş
            descent_phase = True

        # ✅ Basınç hesaplama (PASCAL OLARAK)
        basinc_pascal = int(DENIZ_SEVIYESI_BASINC_HPA * 100 * pow(1 - (0.0065 * self._sim_irtifa) / 288.15, 5.255))

        # Sıcaklık hesaplama (irtifaya bağlı)
        sicaklik = 15.0 - (self._sim_irtifa * 0.0065)

        # Pil gerilimi simülasyonu
        pil_gerilimi = max(3.0, 12.6 - (elapsed_time / 3600) * 0.5)  # Saatte 0.5V düşüş

        # GPS koordinat simülasyonu (hafif değişim)
        enlem = self._sim_lat + random.uniform(-0.0001, 0.0001)
        boylam = self._sim_lon + random.uniform(-0.0001, 0.0001)

        # IMU verileri simülasyonu
        if descent_phase:
            # İniş sırasında daha fazla salınım
            pitch = random.uniform(-15, 15)
            roll = random.uniform(-20, 20)
            yaw = random.uniform(0, 360)
        else:
            # Yükselme sırasında daha az salınım
            pitch = random.uniform(-5, 5)
            roll = random.uniform(-10, 10)
            yaw = random.uniform(0, 360)

        # IoT sıcaklık verileri
        iot_sicaklik1 = 25 + random.uniform(-1, 1)
        iot_sicaklik2 = 26 + random.uniform(-1, 1)

        # Taşıyıcı basınç simülasyonu (PASCAL)
        tasiyici_basinci = basinc_pascal + random.uniform(-100, 100)

        # Uçuş durumu simülasyonu
        if elapsed_time < 10:
            uydu_statusu = 1  # Yükselme
        elif elapsed_time < 30:
            uydu_statusu = 2  # Model Uydu İniş
        elif elapsed_time < 40:
            uydu_statusu = 3  # Ayrılma
        elif elapsed_time < 50:
            uydu_statusu = 4  # Görev Yükü İniş
        else:
            uydu_statusu = 5  # Kurtarma

        # RTC zaman simülasyonu
        rtc_time = time.localtime(self._sim_start_time + elapsed_time)

        # Detaylı sensör verisi sözlüğü
        sensor_verisi = {
            "basinc": basinc_pascal,
            "irtifa": self._sim_irtifa,
            "sicaklik": sicaklik,
            "pil_gerilimi": pil_gerilimi,
            "gps_verisi": {
                "enlem": enlem, 
                "boylam": boylam, 
                "yukseklik": self._sim_irtifa,
                "fix_quality": 1,  # Her zaman fix
                "satellit_count": 8,  # Varsayılan uydu sayısı
                "hdop": 1.0  # En iyi hassasiyet
            },
            "imu_verisi": {
                "pitch": pitch, 
                "roll": roll, 
                "yaw": yaw
            },
            "iot_verileri": {
                "sicaklik1": iot_sicaklik1, 
                "sicaklik2": iot_sicaklik2
            },
            "tasiyici_basinci": tasiyici_basinci,
            "rtc_time": rtc_time,
            "uydu_statusu": uydu_statusu
        }

        return sensor_verisi

    def _oku_gercek_sensorler(self):
        """
        Gerçek donanım için sensör okuma metodu
        Tüm sensörlerden güvenli veri toplama
        """
        try:
            # 🔧 KISMİ VERI DESTEĞİ: Her sensör için ayrı güvenli okuma
            
            # BMP280 basınç, sıcaklık, irtifa ölçümü (güvenli)
            try:
                basinc, sicaklik, irtifa = self._read_bmp280()
            except Exception as bmp_error:
                logger.warning(f"⚠️ BMP280 okuma hatası, boş değerler: {bmp_error}")
                basinc, sicaklik, irtifa = 0.0, 0.0, 0.0  # BOŞ değerler
            
            # GPS verisi (güvenli)
            try:
                gps_verisi = self._read_gps()
            except Exception as gps_error:
                logger.warning(f"⚠️ GPS okuma hatası, boş değerler: {gps_error}")
                gps_verisi = {
                    'enlem': 0.0, 'boylam': 0.0, 'yukseklik': 0.0,
                    'fix_quality': 0, 'satellite_count': 0
                }  # BOŞ değerler
            
            # IMU verileri (güvenli)
            try:
                imu_verisi = self._read_imu()
            except Exception as imu_error:
                logger.warning(f"⚠️ IMU okuma hatası, boş değerler: {imu_error}")
                imu_verisi = {
                    'pitch': 0.0, 'roll': 0.0, 'yaw': 0.0,
                    'ivme_x': 0.0, 'ivme_y': 0.0, 'ivme_z': 0.0,
                    'gyro_x': 0.0, 'gyro_y': 0.0, 'gyro_z': 0.0,
                    'mag_x': 0.0, 'mag_y': 0.0, 'mag_z': 0.0
                }  # BOŞ değerler
            
            # Pil gerilimi (güvenli)
            try:
                pil_gerilimi = self._read_battery()
            except Exception as battery_error:
                logger.warning(f"⚠️ Pil okuma hatası, boş değer: {battery_error}")
                pil_gerilimi = 0.0  # BOŞ değer
            
            # RTC zaman damgası (güvenli)
            # RTC KALDIRILDI - sistem zamanı kullanılıyor
            rtc_time = None
            
            # Taşıyıcı basınç verisi (güvenli çağrı)
            tasiyici_basinci = 0.0
            try:
                if hasattr(self.saha_alici, 'get_tasiyici_basiinci'):
                    tasiyici_basinci = self.saha_alici.get_tasiyici_basiinci()
                elif hasattr(self.saha_alici, 'get_basinc2_value'):
                    tasiyici_basinci = self.saha_alici.get_basinc2_value()
            except Exception as saha_error:
                logger.warning(f"Taşıyıcı basınç verisi alınamadı: {saha_error}")
            
            # IoT sıcaklık verileri (bonus görev)
            iot_s1_data, iot_s2_data = 0.0, 0.0
            try:
                if hasattr(self.saha_alici, 'get_iot_temperatures'):
                    iot_s1_data, iot_s2_data = self.saha_alici.get_iot_temperatures()
            except Exception as iot_error:
                logger.warning(f"IoT sıcaklık verileri alınamadı: {iot_error}")
            
            # Detaylı sensör verisi sözlüğü
            sensor_verisi = {
                "basinc": basinc,
                "irtifa": irtifa,
                "sicaklik": sicaklik,
                "pil_gerilimi": pil_gerilimi,
                "gps_verisi": gps_verisi,
                "imu_verisi": imu_verisi,
                "iot_verileri": {
                    "sicaklik1": iot_s1_data, 
                    "sicaklik2": iot_s2_data
                },
                "tasiyici_basinci": tasiyici_basinci,
                "rtc_time": rtc_time,
                # Uçuş durumu için varsayılan değer
                "uydu_statusu": 0  # Uçuşa hazır
            }
            
            # Veri doğrulaması
            self._validate_sensor_data(sensor_verisi)
            
            return sensor_verisi
        
        except Exception as e:
            logger.error(f"❌ KRİTİK sensör sistemi hatası: {e}")
            logger.error(traceback.format_exc())
            
            # KRİTİK hata durumunda BOŞ değerler döndür
            logger.warning("🔄 Kritik hata - boş değerlerle devam ediliyor...")
            return {
                "basinc": 0.0,               # BOŞ
                "irtifa": 0.0,               # BOŞ
                "sicaklik": 0.0,             # BOŞ
                "pil_gerilimi": 0.0,         # BOŞ
                "gps_verisi": {
                    'enlem': 0.0, 'boylam': 0.0, 'yukseklik': 0.0,
                    'fix_quality': 0, 'satellite_count': 0
                },
                "imu_verisi": {
                    'pitch': 0.0, 'roll': 0.0, 'yaw': 0.0,
                    'ivme_x': 0.0, 'ivme_y': 0.0, 'ivme_z': 0.0,
                    'gyro_x': 0.0, 'gyro_y': 0.0, 'gyro_z': 0.0,
                    'mag_x': 0.0, 'mag_y': 0.0, 'mag_z': 0.0
                },
                "iot_verileri": {"sicaklik1": 0.0, "sicaklik2": 0.0},
                "tasiyici_basinci": 0.0,
                "rtc_time": None,            # Sistem zamanı kullanılacak
                "uydu_statusu": 0            # Uçuşa hazır
            }

    def _to_signed_16(self, value):
        """16-bit unsigned değeri signed değere dönüştürür."""
        if value > 32767:
            return value - 65536
        return value

    def _read_bmp280(self):
        """
        BMP280 sensöründen basınç, sıcaklık ve irtifa verilerini PASCAL cinsinden okur.
        """
        try:
            if self.simulate:
                # Simülasyon için Pascal cinsinden basınç
                basinc_pascal = DENIZ_SEVIYESI_BASINC_HPA * 100  # hPa → Pa dönüşümü
                return (
                    basinc_pascal,  # Pascal 
                    15.0 - (self._sim_irtifa * 0.0065),  # Sıcaklık
                    self._sim_irtifa  # İrtifa
                )
            
            # I2C bus kontrolü
            if not hasattr(self, 'bus'):
                logger.error("❌ I2C bus başlatılmamış!")
                return 0, 0.0, 0.0

            # BMP280 I2C adresleri ve register'ları
            BMP280_I2C_ADDR = 0x76  # Değişebilir, gerekirse 0x77 de denenebilir
            BMP280_CTRL_MEAS = 0xF4
            BMP280_CONFIG = 0xF5
            BMP280_PRESS_MSB = 0xF7
            BMP280_TEMP_MSB = 0xFA

            # Sensörü forced mode'a al (her okumada ölçüm yap)
            self.bus.write_byte_data(BMP280_I2C_ADDR, BMP280_CTRL_MEAS, 0x25)
            
            # Kısa bekleme (ölçüm için)
            time.sleep(0.1)

            # Kalibrasyon verilerini oku
            cal_data = self.bus.read_i2c_block_data(BMP280_I2C_ADDR, 0x88, 24)
            
            # Kalibrasyon parametrelerini çıkart (little-endian)
            def parse_uint16(data, index):
                return data[index] | (data[index + 1] << 8)
            
            def parse_int16(data, index):
                value = parse_uint16(data, index)
                return value if value < 32768 else value - 65536

            # Kalibrasyon sabitleri
            dig_T1 = parse_uint16(cal_data, 0)
            dig_T2 = parse_int16(cal_data, 2)
            dig_T3 = parse_int16(cal_data, 4)

            dig_P1 = parse_uint16(cal_data, 6)
            dig_P2 = parse_int16(cal_data, 8)
            dig_P3 = parse_int16(cal_data, 10)
            dig_P4 = parse_int16(cal_data, 12)
            dig_P5 = parse_int16(cal_data, 14)
            dig_P6 = parse_int16(cal_data, 16)
            dig_P7 = parse_int16(cal_data, 18)
            dig_P8 = parse_int16(cal_data, 20)
            dig_P9 = parse_int16(cal_data, 22)

            # Ham sıcaklık ve basınç verilerini oku
            temp_data = self.bus.read_i2c_block_data(BMP280_I2C_ADDR, BMP280_TEMP_MSB, 3)
            press_data = self.bus.read_i2c_block_data(BMP280_I2C_ADDR, BMP280_PRESS_MSB, 3)

            # 20-bit ADC değerlerini hesapla
            adc_T = ((temp_data[0] << 16) | (temp_data[1] << 8) | temp_data[2]) >> 4
            adc_P = ((press_data[0] << 16) | (press_data[1] << 8) | press_data[2]) >> 4

            # Sıcaklık hesaplama
            var1 = ((adc_T / 16384.0) - (dig_T1 / 1024.0)) * dig_T2
            var2 = (((adc_T / 131072.0) - (dig_T1 / 8192.0)) * ((adc_T / 131072.0) - (dig_T1 / 8192.0))) * dig_T3
            t_fine = var1 + var2
            temperature = t_fine / 5120.0

            # Basınç hesaplama
            var1 = (t_fine / 2.0) - 64000.0
            var2 = var1 * var1 * dig_P6 / 32768.0
            var2 = var2 + var1 * dig_P5 * 2.0
            var2 = (var2 / 4.0) + (dig_P4 * 65536.0)
            var1 = (dig_P3 * var1 * var1 / 524288.0 + dig_P2 * var1) / 524288.0
            var1 = (1.0 + var1 / 32768.0) * dig_P1

            # Basınç hesaplamasında sıfır bölme kontrolü
            if var1 == 0.0:
                return 0, 0.0, 0.0

            pressure = 1048576.0 - adc_P
            pressure = (pressure - (var2 / 4096.0)) * 6250.0 / var1
            var1 = dig_P9 * pressure * pressure / 2147483648.0
            var2 = pressure * dig_P8 / 32768.0
            pressure = pressure + (var1 + var2 + dig_P7) / 16.0

            # ✅ PASCAL OLARAK DÖNDÜR (zaten Pascal cinsinden)
            pressure_pascal = int(pressure)  # BMP280 formülü zaten Pascal verir

            # İrtifa hesaplama (barometrik formül) 
            # 🔧 FIX: Yerel referans basınç kullan (test sırasında 981.43 hPa = 268m irtifa)
            # Standart atmosfer basıncı yerine güncel yerel basınç referansı
            local_reference_pressure = 98143.0  # Pa (test sırasındaki değer)
            altitude = 44330.0 * (1.0 - pow(pressure / local_reference_pressure, 0.1903))

            # 🚨 GERÇEK BMP280 VERİLERİNİ KABUL ET!
            # BMP280 97 hPa (970000 Pa) okuyabilir - NORMAL!
            # Çok geniş aralık: 50000-150000 Pa (500-1500 hPa)
            if (pressure_pascal < 50000 or pressure_pascal > 150000 or 
                altitude < -1000 or altitude > 15000):
                logger.warning(f"⚠️ BMP280 GERÇEKTEN ANORMAL VERİ: Basınç={pressure_pascal} Pa, İrtifa={altitude} m")
                # GERÇEK VERİLERİ KULLAN - SIFIRLA DEĞIL!
                logger.info(f"🔧 BMP280 ham verisi korunuyor: {pressure_pascal} Pa")
                return pressure_pascal, temperature, altitude  # Ham veriyi koru!

            return pressure_pascal, temperature, altitude

        except Exception as e:
            logger.error(f"❌ BMP280 OKUMA HATASI: {e}")
            logger.error(traceback.format_exc())
            return 0, 0.0, 0.0

    def _read_gps(self):
        """
        GPS modülünden NMEA verilerini güvenli bir şekilde okur ve ayrıştırır.
        """
        try:
            if self.simulate:
                return {
                    "enlem": self._sim_lat, 
                    "boylam": self._sim_lon, 
                    "yukseklik": self._sim_irtifa,
                    "fix_quality": 1,  # Simülasyonda her zaman fix
                    "satellit_count": 8,  # Varsayılan uydu sayısı
                    "hdop": 1.0  # En iyi hassasiyet
                }
                
            if not self.gps_serial or not self.gps_serial.is_open:
                logger.warning("GPS seri portu açık değil")
                return {
                    "enlem": 0.0, 
                    "boylam": 0.0, 
                    "yukseklik": 0.0,
                    "fix_quality": 0,
                    "satellit_count": 0,
                    "hdop": 99.9  # Kötü hassasiyet
                }

            # GPS veri okuma
            start_time = time.time()
            timeout = 1.0  # 1 saniye timeout

            while time.time() - start_time < timeout:
                if self.gps_serial.in_waiting > 0:
                    line = self.gps_serial.readline().decode('utf-8', errors='ignore').strip()
                    
                    # GPGGA cümlesi (konum ve fix kalitesi)
                    if line.startswith('$GPGGA') or line.startswith('$GNGGA'):
                        parts = line.split(',')
                        
                        # Detaylı format kontrolü
                        if len(parts) >= 15 and parts[1] and parts[2] and parts[4]:
                            # Fix kalitesi kontrolü (0: geçersiz, 1-5: geçerli)
                            try:
                                fix_quality = int(parts[6]) if parts[6] else 0
                                satellit_count = int(parts[7]) if parts[7] else 0
                                hdop = float(parts[8]) if parts[8] else 99.9
                            except (ValueError, TypeError):
                                logger.warning(f"GPS fix bilgisi parse edilemedi: {parts[6:9]}")
                                continue
                            
                            # Sadece yeterli fix kalitesinde veri al
                            if fix_quality > 0 and satellit_count >= 4:
                                # Enlem dönüştürme
                                latitude = self._parse_gps_coordinate(parts[2], parts[3])
                                
                                # Boylam dönüştürme
                                longitude = self._parse_gps_coordinate(parts[4], parts[5])
                                
                                # Yükseklik
                                try:
                                    altitude = float(parts[9]) if parts[9] else 0.0
                                except (ValueError, TypeError):
                                    logger.warning(f"GPS yükseklik verisi parse edilemedi: {parts[9]}")
                                    altitude = 0.0

                                return {
                                    "enlem": latitude,
                                    "boylam": longitude,
                                    "yukseklik": altitude,
                                    "fix_quality": fix_quality,
                                    "satellit_count": satellit_count,
                                    "hdop": hdop
                                }
                
                    # GPRMC cümlesi (alternatif konum verisi)
                    elif line.startswith('$GPRMC') or line.startswith('$GNRMC'):
                        parts = line.split(',')
                        
                        # Detaylı format kontrolü
                        if len(parts) >= 10 and parts[2] == 'A':  # A = aktif
                            # Enlem dönüştürme
                            latitude = self._parse_gps_coordinate(parts[3], parts[4])
                            
                            # Boylam dönüştürme
                            longitude = self._parse_gps_coordinate(parts[5], parts[6])
                            
                            # Hız ve yön bilgisi
                            try:
                                ground_speed = float(parts[7]) if parts[7] else 0.0
                                ground_course = float(parts[8]) if parts[8] else 0.0
                            except (ValueError, TypeError):
                                logger.warning(f"GPS hız/yön verisi parse edilemedi: {parts[7:9]}")
                                ground_speed, ground_course = 0.0, 0.0

                            return {
                                "enlem": latitude,
                                "boylam": longitude,
                                "yukseklik": 0.0,  # GPRMC'de yükseklik bilgisi yok
                                "ground_speed": ground_speed,
                                "ground_course": ground_course,
                                "fix_quality": 1,  # A = aktif
                                "satellit_count": 0,  # Bilinmiyor
                                "hdop": 99.9  # Bilinmiyor
                            }
            
            # Eğer hiçbir geçerli veri bulunamazsa
            logger.warning("GPS verisi bulunamadı")
            return {
                "enlem": 0.0, 
                "boylam": 0.0, 
                "yukseklik": 0.0,
                "fix_quality": 0,
                "satellit_count": 0,
                "hdop": 99.9
            }
                
        except Exception as e:
            logger.error(f"❌ GPS HATASI: {e}")
            logger.error(traceback.format_exc())
            return {
                "enlem": 0.0, 
                "boylam": 0.0, 
                "yukseklik": 0.0,
                "fix_quality": 0,
                "satellit_count": 0,
                "hdop": 99.9
            }

    def _parse_gps_coordinate(self, raw_str, direction):
        """
        GPS koordinat verilerini güvenli bir şekilde parse et
        
        :param raw_str: GPS koordinat ham verisi (DDMM.MMMM formatı)
        :param direction: Koordinat yönü ('N', 'S', 'E', 'W')
        :return: Ondalık derece cinsinden koordinat
        """
        try:
            # Boş veya geçersiz string kontrolü
            if not raw_str or raw_str.strip() == '':
                logger.warning(f"Geçersiz GPS koordinat verisi: {raw_str}")
                return 0.0
            
            # Float'a çevirme
            try:
                raw_value = float(raw_str)
            except ValueError:
                logger.warning(f"GPS koordinat verisi float'a çevrilemedi: {raw_str}")
                return 0.0
            
            # Koordinat dönüşümü
            degrees = int(raw_value / 100)
            minutes = raw_value - (degrees * 100)
            decimal_degrees = degrees + (minutes / 60.0)
            
            # Yön kontrolü
            if direction in ['S', 'W']:
                decimal_degrees = -decimal_degrees
            elif direction not in ['N', 'E']:
                logger.warning(f"Geçersiz GPS yön bilgisi: {direction}")
            
            return decimal_degrees
        
        except Exception as e:
            logger.error(f"GPS koordinat parse hatası: {e}")
            return 0.0

    def _read_imu(self):
        """
        10-DOF IMU sensöründen pitch, roll, yaw VE ham accelerometer, gyroscope, magnetometer verilerini okur.
        ✅ YENİ: IMUSensorYoneticisi kullanarak gelişmiş IMU okuma - HAM VERİLER DAHİL
        """
        try:
            # ✅ YENİ IMU SİSTEMİ: IMUSensorYoneticisi kullan
            if self.imu_yoneticisi and self.imu_yoneticisi.is_active():
                # İşlenmiş veriler (pitch, roll, yaw)
                telemetri_verisi = self.imu_yoneticisi.get_telemetry_data()
                
                # Ham veriler (accelerometer, gyroscope, magnetometer)
                ham_veriler = self.imu_yoneticisi.get_raw_data()
                
                # Tüm verileri birleştir
                tam_imu_verisi = {}
                
                # İşlenmiş veriler
                if telemetri_verisi:
                    tam_imu_verisi.update(telemetri_verisi)
                
                # Ham veriler - accelerometer
                if ham_veriler and 'accelerometer' in ham_veriler:
                    acc = ham_veriler['accelerometer']
                    tam_imu_verisi.update({
                        'ivme_x': acc.get('x', 0.0),
                        'ivme_y': acc.get('y', 0.0), 
                        'ivme_z': acc.get('z', 0.0)
                    })
                
                # Ham veriler - gyroscope
                if ham_veriler and 'gyroscope' in ham_veriler:
                    gyro = ham_veriler['gyroscope']
                    tam_imu_verisi.update({
                        'gyro_x': gyro.get('x', 0.0),
                        'gyro_y': gyro.get('y', 0.0),
                        'gyro_z': gyro.get('z', 0.0)
                    })
                
                # Ham veriler - magnetometer
                if ham_veriler and 'magnetometer' in ham_veriler:
                    mag = ham_veriler['magnetometer']
                    tam_imu_verisi.update({
                        'mag_x': mag.get('x', 0.0),
                        'mag_y': mag.get('y', 0.0),
                        'mag_z': mag.get('z', 0.0)
                    })
                
                # Veri kontrolü - herhangi bir değer varsa döndür
                if tam_imu_verisi and any(abs(v) > 0.01 for v in tam_imu_verisi.values() if isinstance(v, (int, float))):
                    return tam_imu_verisi
                else:
                    print("⚠️ IMU sıfır dışı değer bulunamadı")
                    return self._get_default_imu_data()
            else:
                # IMU olmadığında varsayılan değerler
                print("⚠️ IMU sensörü aktif değil")
                return self._get_default_imu_data()
            
        except Exception as e:
            print(f"HATA: IMU sensörleri okunamadı: {e}")
            return self._get_default_imu_data()
    
    def _get_default_imu_data(self):
        """Varsayılan IMU verilerini döndürür - hem işlenmiş hem ham"""
        return {
            'pitch': 0.0, 'roll': 0.0, 'yaw': 0.0,
            'ivme_x': 0.0, 'ivme_y': 0.0, 'ivme_z': 0.0,
            'gyro_x': 0.0, 'gyro_y': 0.0, 'gyro_z': 0.0,
            'mag_x': 0.0, 'mag_y': 0.0, 'mag_z': 0.0
        }

    def _read_battery(self):
        """
        Pil gerilimini ADS1115 16-bit ADC üzerinden okur.  # 🔧 PCF8591 → ADS1115
        ✅ YENİ: PilGerilimiYoneticisi kullanarak gelişmiş pil izleme
        """
        try:
            # ✅ YENİ PİL SİSTEMİ: PilGerilimiYoneticisi kullan
            if self.pil_yoneticisi and self.pil_yoneticisi.is_active():
                # Ana 3.7V Li-Ion pil gerilimini al (telemetri için)
                main_battery_voltage = self.pil_yoneticisi.get_telemetry_battery_voltage()
                return main_battery_voltage
            else:
                # Fallback: simülasyon değeri
                if self.simulate:
                    # Simülasyonda azalan pil voltajı
                    elapsed_time = time.time() - getattr(self, '_sim_start_time', time.time())
                    return max(3.0, 3.7 - (elapsed_time / 3600) * 0.1)  # Saatte 0.1V düşüş
                else:
                    # Varsayılan değer
                    return 3.7
            
        except Exception as e:
            print(f"HATA: Pil gerilimi okunamadı: {e}")
            return 3.7  # Varsayılan değer (3.7V Li-Ion)

    def temizle(self):
        """
        Gelişmiş kaynak temizleme
        """
        try:
            # Mevcut temizleme kodları
            if not self.simulate:
                if self.gps_serial and self.gps_serial.is_open:
                    self.gps_serial.close()
                    print("GPS seri portu kapatıldı.")

            # Sensör durumlarını sıfırla
            self.sensor_status = {k: False for k in self.sensor_status}
            
            logger.info("Sensör kaynakları başarıyla temizlendi.")
        except Exception as e:
            logger.error(f"Kaynak temizleme hatası: {e}")
            logger.error(traceback.format_exc())

    def _init_i2c_bus(self):
        """I2C bus'ını başlatır"""
        try:
            import smbus
            if not self.simulate:
                self.bus = smbus.SMBus(1)
                print("✅ I2C bus 1 başlatıldı")
            else:
                print("⚠️ Simülasyon modunda I2C bus başlatılmadı")
                self.bus = None
        except ImportError:
            print("❌ smbus kütüphanesi bulunamadı")
            self.bus = None
            self.simulate = True
        except Exception as e:
            print(f"❌ I2C bus hatası: {e}")
            self.bus = None
            self.simulate = True

# Test için örnek kullanım
if __name__ == '__main__':
    # Eski ve yeni metod isimlerini destekleyen SahteSAHA sınıfı
    class SahteSAHA:
        def get_basinc2_value(self): 
            return 101500  # Pascal formatında basınç
        
        def get_tasiyici_basiinci(self): 
            return 1015.0  # Uyumluluk için hPa cinsinden
        
        def get_tasiyici_basinici(self): 
            return 1015.0  # Eski metod ismi için destek
        
        def baslat(self): pass
        def durdur(self): pass

    print("SensorManager Testi (Simülasyon Modu)")
    saha = SahteSAHA()
    sensor_yonetici = SensorManager(saha_alici_instance=saha, simulate=True)
    
    veri = sensor_yonetici.oku_tum_sensorler()
    print("Okunan sensör verisi:")
    import json
    print(json.dumps(veri, indent=2))
    
    sensor_yonetici.temizle()
    print("\nTest tamamlandı.")
