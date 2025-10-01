# -*- coding: utf-8 -*-
"""
TÜRKSAT Model Uydu Yarışması - Görev Yükü (Raspberry Pi)
Konfigürasyon Dosyası

Bu dosya, projedeki tüm donanım pinlerini, sabitleri ve yapılandırma
değişkenlerini merkezi bir yerde toplar.
"""

import os
import json
from datetime import datetime, timedelta

# Platform tespiti (BASE_DIR'i erken tanımla)
IS_RASPBERRY_PI = True  # Raspberry Pi modunda çalış
BASE_DIR = "/home/atugem"  # Varsayılan
try:
    import RPi.GPIO as GPIO
    IS_RASPBERRY_PI = True
    BASE_DIR = "/home/atugem"
except ImportError:
    IS_RASPBERRY_PI = False
    BASE_DIR = os.path.expanduser("~")

# Logging Konfigürasyonu (BASE_DIR tanımlandıktan sonra)
LOGGING_LEVEL = "INFO"  # DEBUG, INFO, WARNING, ERROR, CRITICAL
LOGGING_FORMAT = "%(asctime)s - %(name)s - %(levelname)s - %(message)s"
LOGGING_FILE = os.path.join(BASE_DIR, "system.log")

def setup_logging():
    """
    Merkezi logging konfigürasyonu - VERİ KORUMA MODU
    64GB SD kart - Log dosyaları ASLA silinmez
    """
    import logging
    import logging.handlers
    
    # Log seviyesini belirle
    level = getattr(logging, LOGGING_LEVEL.upper(), logging.INFO)
    
    # Veri koruma için özel log handler sınıfı
    class DataProtectionRotatingFileHandler(logging.handlers.RotatingFileHandler):
        """Veri koruma amaçlı log handler - dosyaları silmez"""
        
        def doRollover(self):
            """
            Log rotasyonu - VERİ KORUMA: Eski dosyalar silinmez
            Sadece yeni dosya oluşturur, eskiler korunur
            """
            if self.stream:
                self.stream.close()
                self.stream = None
            
            # Yeni dosya adı oluştur (timestamp ile)
            timestamp = datetime.now().strftime("%Y%m%d_%H%M%S")
            base_filename = self.baseFilename
            
            # Dosya uzantısını ayır
            if '.' in base_filename:
                name, ext = base_filename.rsplit('.', 1)
                new_filename = f"{name}_{timestamp}.{ext}"
            else:
                new_filename = f"{base_filename}_{timestamp}"
            
            # Yeni dosyayı aç
            self.baseFilename = new_filename
            self.stream = self._open()
            
            print(f"📄 Yeni log dosyası: {new_filename}")
            print("✅ Eski log dosyaları korundu - hiçbir veri silinmedi")
    
    # Root logger'ı configure et
    logging.basicConfig(
        level=level,
        format=LOGGING_FORMAT,
        handlers=[
            # Console handler
            logging.StreamHandler(),
            # Veri koruma file handler (dosya silmez)
            DataProtectionRotatingFileHandler(
                LOGGING_FILE,
                maxBytes=10*1024*1024,  # 10MB (daha büyük dosya boyutu)
                backupCount=0  # Backup limiti yok - tüm dosyalar korunur
            )
        ]
    )
    
    # Gürültülü kütüphaneleri sustur
    logging.getLogger('PIL').setLevel(logging.WARNING)
    logging.getLogger('picamera2').setLevel(logging.WARNING)
    
    print(f"✅ VERİ KORUMA Logging sistemi başlatıldı: {LOGGING_FILE}")
    print("📊 Log dosyaları asla silinmez - 64GB SD kart tam koruma modu")

# Performans Ayarları
CPU_NICE_PRIORITY = -10  # Yüksek öncelik (-20 ile 19 arası, düşük = yüksek öncelik)
MAX_CPU_USAGE_PERCENT = 90
MAX_MEMORY_USAGE_PERCENT = 85
MAX_CPU_TEMPERATURE = 70
MEMORY_LIMIT_MB = 200   # Maximum memory usage limit

# Takım Bilgileri
TAKIM_NUMARASI = 286570
XBEE_PAN_ID = 0x6570   # PAN ID: 6570 (286570 son 4 hane - geçerli aralık)

# Görev Zamanı Korunma Sistemi (Gereksinim 14)
# -------------------------------------------------
# 🔧 DÜZELTME: Platform-bağımsız dosya yolları
GOREV_BASLANGIC_DOSYASI = os.path.join(BASE_DIR, "gorev_baslangic.json")
PAKET_SAYISI_DOSYASI = os.path.join(BASE_DIR, "paket_sayisi.json")

def gorev_baslangic_zamani_yukle():
    """Görev başlangıç zamanını kalıcı dosyadan yükler veya yeni oluşturur."""
    try:
        if os.path.exists(GOREV_BASLANGIC_DOSYASI):
            with open(GOREV_BASLANGIC_DOSYASI, 'r') as f:
                data = json.load(f)
                return datetime.fromisoformat(data['baslangic_zamani'])
        else:
            # İlk kez çalışıyorsa yeni başlangıç zamanı oluştur
            baslangic_zamani = datetime.now()
            gorev_baslangic_zamani_kaydet(baslangic_zamani)
            return baslangic_zamani
    except Exception as e:
        print(f"HATA: Görev başlangıç zamanı yüklenemedi: {e}")
        return datetime.now()

def gorev_baslangic_zamani_kaydet(baslangic_zamani):
    """Görev başlangıç zamanını kalıcı dosyaya kaydeder."""
    try:
        os.makedirs(os.path.dirname(GOREV_BASLANGIC_DOSYASI), exist_ok=True)
        with open(GOREV_BASLANGIC_DOSYASI, 'w') as f:
            json.dump({
                'baslangic_zamani': baslangic_zamani.isoformat(),
                'kaydedilme_zamani': datetime.now().isoformat()
            }, f)
    except Exception as e:
        print(f"HATA: Görev başlangıç zamanı kaydedilemedi: {e}")

def gorev_suresini_hesapla():
    """Görev süresini hesaplar (T+000:00:00 formatında)."""
    try:
        baslangic_zamani = gorev_baslangic_zamani_yukle()
        gecen_sure = datetime.now() - baslangic_zamani
        
        # T+HHH:MM:SS formatında döndür
        total_seconds = int(gecen_sure.total_seconds())
        hours = total_seconds // 3600
        minutes = (total_seconds % 3600) // 60
        seconds = total_seconds % 60
        
        return f"T+{hours:03d}:{minutes:02d}:{seconds:02d}"
    except Exception as e:
        print(f"HATA: Görev süresi hesaplanamadı: {e}")
        return "T+000:00:00"

# Paket Sayısı Korunma Sistemi (Gereksinim 15)
# -------------------------------------------------
def paket_sayisi_yukle():
    """Paket sayısını kalıcı dosyadan yükler."""
    try:
        if os.path.exists(PAKET_SAYISI_DOSYASI):
            with open(PAKET_SAYISI_DOSYASI, 'r') as f:
                data = json.load(f)
                return data.get('paket_sayisi', 1)
        else:
            return 1  # İlk paket numarası
    except Exception as e:
        print(f"HATA: Paket sayısı yüklenemedi: {e}")
        return 1

def paket_sayisi_kaydet(paket_sayisi):
    """Paket sayısını kalıcı dosyaya kaydeder."""
    try:
        os.makedirs(os.path.dirname(PAKET_SAYISI_DOSYASI), exist_ok=True)
        with open(PAKET_SAYISI_DOSYASI, 'w') as f:
            json.dump({
                'paket_sayisi': paket_sayisi,
                'kaydedilme_zamani': datetime.now().isoformat()
            }, f)
    except Exception as e:
        print(f"HATA: Paket sayısı kaydedilemedi: {e}")

# Pin Tanımlamaları (BCM Numaralandırması)
# -------------------------------------------------
# I2C Arayüzü (Tüm I2C cihazları için ortak)
PIN_I2C_SDA = 2  # GPIO 2
PIN_I2C_SCL = 3  # GPIO 3

# SPI Arayüzü (SD Kart için)
PIN_SPI_MOSI = 10 # GPIO 10
PIN_SPI_MISO = 9  # GPIO 9
PIN_SPI_SCK = 11 # GPIO 11
PIN_SPI_CS = 8   # GPIO 8 (CE0)

# UART Arayüzleri - TEK XBee KONFİGÜRASYONU  
# 🔥 BİRLEŞİK XBee - Tüm haberleşme tek modül üzerinden

# XBee port sabit tanımla (otomatik tespit kaldırıldı)
SERIAL_PORT_XBEE = "/dev/serial0"  # Hardware UART (Pin 8/10)
SERIAL_BAUD_XBEE = 57600  # 🚀 VIDEO İÇİN OPTİMİZE EDİLMİŞ BAUD RATE
print(f"📡 XBee Port: {SERIAL_PORT_XBEE}")

# GPS Modülü (Raspberry Pi Hardware UART)
# GY-GPS6MV2 doğrudan GPIO'ya bağlı
SERIAL_PORT_GPS = "/dev/ttyS0"  # GPIO seri port  # Hardware UART (GPIO 14/15)
SERIAL_BAUD_GPS = 57600
SIMULATE_GPS = not IS_RASPBERRY_PI  # 🧪 Windows test modu için GPS simülasyonu

# ESKİ AYARLAR (Artık kullanılmıyor - tek XBee ile tümü yapılıyor)
# SAHA_PORT = "/dev/ttyUSB2"  # Artık gerekli değil
# IOT_XBEE_PORT = "/dev/ttyUSB0"  # Artık gerekli değil

# Geçici uyumluluk için (eski kodların çalışması için)
SAHA_PORT = SERIAL_PORT_XBEE  # Aynı XBee portu
SAHA_BAUD_RATE = SERIAL_BAUD_XBEE
IOT_XBEE_PORT = SERIAL_PORT_XBEE  # Aynı XBee portu  
IOT_XBEE_BAUD = SERIAL_BAUD_XBEE
# Not: Pi 3/4/Zero W modellerinde Bluetooth ttyAMA0'ı kullanabilir.
# /boot/config.txt içinde dtoverlay=disable-bt eklenmesi gerekebilir.

# PWM Çıkışları (Servolar ve Buzzer) - ÇAKıŞMA DÜZELTİLDİ
PIN_SERVO_AYRILMA = 23      # GPIO 23 (Ayrılma servosu - Taşıyıcıda)
PIN_SERVO_FILTRE_1 = 17     # GPIO 17 (Multi-spektral disk 1)
PIN_SERVO_FILTRE_2 = 19     # GPIO 19 (Multi-spektral disk 2) - GPIO 27 yerine güvenli pin
PIN_BUZZER = 13             # GPIO 13 (Buzzer) - GPIO 4 yerine güvenli pin

# Güç Yönetimi GPIO Pinleri
PIN_POWER_BUTTON = 21       # GPIO 21 - Güç butonu girişi (pull-up)
PIN_POWER_LED = 20          # GPIO 20 - Güç LED'i çıkışı

# Sistem Sabitleri
# -------------------------------------------------
# Görev Frekansı
TELEMETRI_GONDERIM_SIKLIGI = 1.0 # Saniye (1 Hz)

# ARAS (Arayüz Alarm Sistemi) Limitleri
# -------------------------------------------------
AYRILMA_YUKSEKLIK = 400.0 # metre
AYRILMA_TOLERANS = 10.0   # metre
AYRILMA_TIMEOUT = 5       # saniye (Bu sürede ayrılmazsa hata üret)
HIZ_LIMIT_MODEL_UYDU_MIN = 12.0 # m/s
HIZ_LIMIT_MODEL_UYDU_MAX = 14.0 # m/s
HIZ_LIMIT_GOREV_YUKU_MIN = 6.0  # m/s
HIZ_LIMIT_GOREV_YUKU_MAX = 8.0

# Kurtarma Modu
KURTARMA_SURESI = 10 # Saniye (yere indikten sonra veri gönderimine devam etme süresi)
BUZZER_IKAZ_SURESI = 3600 # Saniye (1 saat boyunca sesli ikaz)

# Multi-Spektral Filtreleme
FILTRELEME_MAKS_GECIKME_SN = 2.0  # Saniye
FILTRELEME_MAKS_TOPLAM_SURE_SN = 15.0 # Saniye
FILTRE_SURE_MIN = 6 # Saniye
FILTRE_SURE_MAX = 9 # Saniye

# SD Kart Kayıt Ayarları
SD_KART_TELEMETRI_DOSYASI = "/media/sdcard/telemetri_verileri.csv"
SD_KART_VIDEO_KLASORU = "/media/sdcard/video_kayitlari/"
SD_KART_LOG_DOSYASI = "/media/sdcard/sistem_log.txt"

# Diğer
# Coğrafi Kalibrasyon Sistemi (Yer İstasyonu ile uyumlu)
# Aynı hesaplama formülü: 1013.25 - (rakim / 8.3)
SEHIR_KALIBRASYONLARI = {
    "ankara": {"rakim": 850, "basinc": 911.75},
    "istanbul": {"rakim": 40, "basinc": 1008.42},
    "izmir": {"rakim": 25, "basinc": 1011.44},
    "antalya": {"rakim": 50, "basinc": 1007.24},
    "kayseri": {"rakim": 1054, "basinc": 886.25},
    "bursa": {"rakim": 100, "basinc": 1001.20}
}

# Varsayılan şehir (değiştirilebilir)
VARSAYILAN_SEHIR = "ankara"

def get_kalibrasyon_basinci(sehir=None):
    """
    Seçili şehre göre kalibrasyon basıncını döndürür
    Yer istasyonu ile aynı hesaplama: 1013.25 - (rakim/8.3)
    """
    if sehir is None:
        sehir = VARSAYILAN_SEHIR
    
    sehir = sehir.lower()
    if sehir in SEHIR_KALIBRASYONLARI:
        return SEHIR_KALIBRASYONLARI[sehir]["basinc"]
    else:
        print(f"⚠️ Bilinmeyen şehir: {sehir}, Ankara kullanılıyor")
        return SEHIR_KALIBRASYONLARI["ankara"]["basinc"]

# 🔧 GLOBAL STATE FIX: Thread-safe lazy loading için global değişken yerine getter kullan
import threading
_basinc_lock = threading.Lock()
_cached_basinc = None

def get_deniz_seviyesi_basinc_hpa():
    """Thread-safe basınç değeri getter (lazy loading)"""
    global _cached_basinc
    
    if _cached_basinc is None:
        with _basinc_lock:
            # Double-check locking pattern
            if _cached_basinc is None:
                _cached_basinc = get_kalibrasyon_basinci(VARSAYILAN_SEHIR)
    
    return _cached_basinc

# Backward compatibility için global değişken (ama thread-safe getter kullan)
DENIZ_SEVIYESI_BASINC_HPA = get_deniz_seviyesi_basinc_hpa()

# Multi-Spektral Filtreleme Görevi - SERVO PİNLERİ (Çakışmayı önlemek için tanımladık)
PIN_SERVO_1 = PIN_SERVO_FILTRE_1  # GPIO 17 (alias)
PIN_SERVO_2 = PIN_SERVO_FILTRE_2  # GPIO 19 (alias)
# PIN_BUZZER zaten yukarıda GPIO 13 olarak tanımlandı

# Kamera Canlı Yayın Paketleme
VIDEO_FRAME_BASLANGIC = b'\xDE\xAD\xBE\xEF' # (DEADBEEF) Benzersiz başlangıç byte'ları
VIDEO_FRAME_BITIS = b'\xCA\xFE\xBA\xBE'     # (CAFEBABE) Benzersiz bitiş byte'ları

# 🔧 İKİLİ VİDEO SİSTEMİ - Raspberry Pi 2W Optimizasyonu
# 🎥 SD KART TAM KALİTE: H.264/MP4 (tüm oynatıcılara uyumlu)
VIDEO_SD_RESOLUTION = (640, 480)    # SD kart için tam kalite
VIDEO_SD_FPS = 15                   # Pi 2W için optimize
VIDEO_SD_BITRATE = 2000000          # 2 Mbps H.264

# 📡 XBee CANLI YAYIN: MJPEG düşük kalite (250 Kbps bandgenişlik) - OPTIMIZE
VIDEO_XBEE_RESOLUTION = (240, 180)  # 🔧 FIX: Daha küçük çözünürlük (performans için)
VIDEO_XBEE_FPS = 2                  # 🔧 FIX: 2 FPS (1'den yüksek ama hala hafif)
VIDEO_XBEE_JPEG_QUALITY = 20       # 🔧 FIX: %20 JPEG kalitesi (daha sıkıştırılmış)
VIDEO_MAX_FRAME_SIZE_KB = 6         # 🔧 FIX: 6KB limit (daha küçük paketler)

# 🛡️ GÜVENLİ XBee Bandwidth Hesabı (250 Kbps limit) - YENİ HESAPLAMA:
# 📡 Video: ~3KB/frame × 2 FPS = 6 KB/s = 48 Kbps (optimize edilmiş)
# 📡 Telemetri: ~150 byte/packet × 1 Hz = 1.2 Kbps  
# 📡 IoT/SAHA traffic: ~5 Kbps (tahmin)
# TOPLAM: ~54 Kbps < 250 Kbps ✅ (%22 kullanım - hala çok güvenli)
# REZERV: 196 Kbps (%78 - XBee retry/buffer için güvenli)

# 🔧 DÜZELTME: ARAS Hata Kodu Validation (Analiz3'ten)
def validate_hata_kodu(hata_kodu):
    """
    6 haneli ARAS hata kodunu doğrular (Gereksinim 2.2)
    :param hata_kodu: String formatında 6 haneli hata kodu
    :return: True ise geçerli, False ise geçersiz
    """
    if not isinstance(hata_kodu, str):
        return False
    if len(hata_kodu) != 6:
        return False
    return all(c in '01' for c in hata_kodu)

# Performans İzleme
import psutil
import threading

def get_system_stats():
    """Sistem kaynak kullanımını döndürür"""
    try:
        return {
            'cpu_percent': psutil.cpu_percent(interval=0.1),
            'memory_percent': psutil.virtual_memory().percent,
            'memory_available_mb': psutil.virtual_memory().available // (1024*1024),
            'disk_usage_percent': psutil.disk_usage('/').percent,
            'temperature': get_cpu_temperature()
        }
    except Exception as e:
        print(f"⚠️ Sistem istatistikleri alınamadı: {e}")
        return {'cpu_percent': 0, 'memory_percent': 0, 'memory_available_mb': 0, 'disk_usage_percent': 0, 'temperature': 0}

def get_cpu_temperature():
    """Raspberry Pi CPU sıcaklığını döndürür"""
    try:
        if IS_RASPBERRY_PI:
            with open('/sys/class/thermal/thermal_zone0/temp', 'r') as f:
                temp = int(f.read().strip()) / 1000.0
                return temp
        return 0.0
    except:
        return 0.0

def check_system_health():
    """Sistem sağlığını kontrol eder"""
    stats = get_system_stats()
    warnings = []
    
    if stats['cpu_percent'] > 90:
        warnings.append(f"⚠️ Yüksek CPU kullanımı: {stats['cpu_percent']:.1f}%")
    
    if stats['memory_percent'] > 85:
        warnings.append(f"⚠️ Yüksek RAM kullanımı: {stats['memory_percent']:.1f}%")
    
    if stats['temperature'] > 70:
        warnings.append(f"🌡️ Yüksek CPU sıcaklığı: {stats['temperature']:.1f}°C")
    
    if stats['disk_usage_percent'] > 90:
        warnings.append(f"💾 Disk dolmaya yakın: {stats['disk_usage_percent']:.1f}%")
    
    return warnings

# Performans ayarları uygula
def apply_performance_settings():
    """Raspberry Pi için performans optimizasyonları"""
    try:
        if IS_RASPBERRY_PI:
            import os
            # CPU nice priority ayarla (düşük = yüksek öncelik)
            os.nice(CPU_NICE_PRIORITY)
            print(f"✅ CPU önceliği ayarlandı: {CPU_NICE_PRIORITY}")
    except Exception as e:
        print(f"⚠️ Performans ayarları uygulanamadı: {e}")

# 🔧 FIX: Telemetri gönderim sıklığı - 1Hz (saniyede 1 paket)
TELEMETRI_GONDERIM_SIKLIGI = 1.0  # saniye (1Hz için)
