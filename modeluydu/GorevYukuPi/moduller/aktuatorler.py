# -*- coding: utf-8 -*-
"""
Aktüatör Yönetici Modülü

Bu modül, görev yükü üzerindeki tüm aktüatörleri (servo motorlar, buzzer vb.)
kontrol etmekten sorumludur.
- Multi-Spektral Filtreleme Görevi (Servolar)
- Kurtarma Sinyali (Buzzer)
"""

import time
import threading

# Raspberry Pi üzerinde çalışırken gerçek kütüphaneleri kullanmayı dene
try:
    import RPi.GPIO as GPIO
    from moduller.yapilandirma import PIN_SERVO_FILTRE_1, PIN_SERVO_FILTRE_2, PIN_BUZZER, IS_RASPBERRY_PI
    # Alias tanımlamaları (eski kodla uyumluluk için)
    PIN_SERVO_1 = PIN_SERVO_FILTRE_1
    PIN_SERVO_2 = PIN_SERVO_FILTRE_2
    GPIO_AVAILABLE = True
except ImportError:
    print("UYARI: RPi.GPIO kütüphanesi bulunamadı. Aktüatörler simülasyon modunda çalışacak.")
    # Simülasyon için pinleri tanımla
    PIN_SERVO_1, PIN_SERVO_2, PIN_BUZZER = None, None, None
    GPIO_AVAILABLE = False
    # IS_RASPBERRY_PI import edil
    from moduller.yapilandirma import IS_RASPBERRY_PI

# Servo pozisyonları (SG90 servo 0-180° limitli)
SERVO_0_DERECE = 0      # 0° - Normal pozisyon
SERVO_90_DERECE = 90    # 90° - Birinci filtre pozisyonu  
SERVO_180_DERECE = 180  # 180° - İkinci filtre pozisyonu
# SG90 servo 270° desteklemez - maksimum 180°

# Multi-spektral filtre pozisyonları (SG90 için)
FILTRE_POZISYON_0 = SERVO_0_DERECE    # Normal (filtresiz)
FILTRE_POZISYON_1 = SERVO_90_DERECE   # Birinci filtre pozisyonu
FILTRE_POZISYON_2 = SERVO_180_DERECE  # İkinci filtre pozisyonu

# FILTRE_NORMAL eksik tanım eklendi (alias for FILTRE_POZISYON_0)
FILTRE_NORMAL = FILTRE_POZISYON_0     # Normal (filtresiz) pozisyon

# Şartname sabitleri
MAX_GECIKME_SANIYE = 2
MAX_FILTRELEME_SURESI = 15

class AktuatorYoneticisi:
    def __init__(self, simulate=not IS_RASPBERRY_PI):
        """
        Aktüatör yöneticisini başlatır.
        :param simulate: True ise simülasyon modunda çalışır.
        """
        # 🚫 SERVO KONTROLÜ DEVRE DIŞI - Her zaman simülasyon modunda çalış
        self.simulate = True  # Zorla simülasyon modu - servo kontrolü kapalı
        self.servo_disabled = True  # Servo devre dışı flag'i
        self.is_command_active = False
        self._command_lock = threading.Lock()  # 🔧 DÜZELTME: Thread safety için lock eklendi
        self.command_thread = None
        self.servo1_pwm = None
        self.servo2_pwm = None
        self.buzzer_active = False

        print("🚫 SERVO KONTROLÜ DEVRE DIŞI - Aktüatörler servo'suz modda başlatıldı.")
        print("⚠️ Spektral filtreleme komutları çalışmayacak")

    def _setup_gpio(self):
        """GPIO pinlerini servo PWM çıkışları için yapılandırır."""
        try:
            GPIO.setmode(GPIO.BCM)  # BCM pin numaralandırması
            
            # Servo pinlerini PWM çıkışı olarak ayarla
            GPIO.setup(PIN_SERVO_1, GPIO.OUT)
            GPIO.setup(PIN_SERVO_2, GPIO.OUT)
            GPIO.setup(PIN_BUZZER, GPIO.OUT)

            # PWM nesnelerini oluştur (50Hz servo standartı)
            self.servo1_pwm = GPIO.PWM(PIN_SERVO_1, 50)
            self.servo2_pwm = GPIO.PWM(PIN_SERVO_2, 50)
            
            # PWM'leri başlat (servo neutral konumunda)
            self.servo1_pwm.start(self._angle_to_duty_cycle(90))  # 90° neutral
            self.servo2_pwm.start(self._angle_to_duty_cycle(90))  # 90° neutral
            
            print(f"✅ GPIO başlatıldı - Servo1: Pin {PIN_SERVO_1}, Servo2: Pin {PIN_SERVO_2}")
            time.sleep(1)  # Servolar konumlanması için bekle
            
        except Exception as e:
            print(f"HATA: GPIO başlatılamadı: {e}")
            self.simulate = True

    def _angle_to_duty_cycle(self, angle):
        """
        Servo açısını PWM duty cycle'a çevirir.
        SG90 servo için: 0° = 2.5%, 90° = 7.5%, 180° = 12.5%
        """
        if angle < 0:
            angle = 0
        elif angle > 180:
            angle = 180
        
        # Linear interpolation: 0° -> 2.5%, 180° -> 12.5%
        duty_cycle = 2.5 + (angle / 180.0) * 10.0
        return duty_cycle

    def _set_servo_angle(self, servo_pin, angle):
        """Belirtilen servo motorunu belirtilen açıya hareket ettirir. - DEVRE DIŞI"""
        if self.servo_disabled:
            print(f"🚫 SERVO DEVRE DIŞI: Servo Pin {servo_pin} -> {angle}° (İşlem gerçekleştirilmedi)")
            return False
            
        if self.simulate:
            print(f"SİMÜLASYON: Servo Pin {servo_pin} -> {angle}°")
            return True

        try:
            duty_cycle = self._angle_to_duty_cycle(angle)
            
            if servo_pin == PIN_SERVO_1 and self.servo1_pwm:
                self.servo1_pwm.ChangeDutyCycle(duty_cycle)
                print(f"🔧 Servo1 (Pin {PIN_SERVO_1}) -> {angle}° (duty: {duty_cycle:.1f}%)")
            elif servo_pin == PIN_SERVO_2 and self.servo2_pwm:
                self.servo2_pwm.ChangeDutyCycle(duty_cycle)
                print(f"🔧 Servo2 (Pin {PIN_SERVO_2}) -> {angle}° (duty: {duty_cycle:.1f}%)")
            else:
                print(f"HATA: Geçersiz servo pin: {servo_pin}")
                return False
                
            # Servo hareket süresi için bekle
            time.sleep(0.5)
            return True
            
        except Exception as e:
            print(f"HATA: Servo kontrolü başarısız (Pin {servo_pin}): {e}")
            return False

    def _get_filter_position(self, filter_type, disk_num, is_standart_konum=False):
        """
        Şartname Tablo 7'ye göre servo pozisyonu döndürür.
        
        :param filter_type: Filtre tipi (R, G, B, C, P, Y, N, M, F)
        :param disk_num: Disk numarası (1 veya 2)
        :param is_standart_konum: True ise N harfi standart konum, False ise Navy Blue
        """
        # 🔧 KRİTİK DÜZELTME: N harfinin iki farklı anlamı:
        # 1. Şartname Tablo 7'de: N = Navy Blue (Blue + Blue) ✅
        # 2. Sistem başlangıç/bitiş: (N) = Normal/Filtresiz ✅
        
        # ŞARTNAME TABLO 7 - Spektral Görüntüleme Tablosu (90° adımlarla)
        if filter_type == 'R': # Light Red (Red + Normal / Normal + Red)
            return FILTRE_POZISYON_2 if disk_num == 1 else FILTRE_NORMAL
        elif filter_type == 'G': # Light Green (Green + Normal / Normal + Green)
            return FILTRE_NORMAL if disk_num == 1 else FILTRE_POZISYON_1
        elif filter_type == 'B': # Light Blue (Blue + Normal / Normal + Blue)
            return FILTRE_POZISYON_1 if disk_num == 1 else FILTRE_NORMAL
        elif filter_type == 'C': # Cyan, Turquoise (Blue + Green / Green + Blue)
            return FILTRE_POZISYON_1 if disk_num == 1 else FILTRE_POZISYON_1
        elif filter_type == 'P': # Purple, Pink (Red + Blue / Blue + Red)
            return FILTRE_POZISYON_2 if disk_num == 1 else FILTRE_POZISYON_1
        elif filter_type == 'Y': # Yellow, Brown (Red + Green / Green + Red)
            return FILTRE_POZISYON_2 # Her iki diskte de aktif
        elif filter_type == 'M': # Maroon Red (Red + Red)
            return FILTRE_POZISYON_2 # Her iki diskte de kırmızı
        elif filter_type == 'F': # Forest/Dark Green (Green + Green)
            return FILTRE_POZISYON_1 # Her iki diskte de yeşil
        elif filter_type == 'N': 
            if is_standart_konum:
                # 🔧 STANDART KONUM: Sadece başlangıç/bitiş için - Normal/Filtresiz
                print("📍 N harfi: Standart/Filtresiz konum (Başlangıç/Bitiş)")
                return FILTRE_NORMAL # Her iki disk de filtresiz konumda
            else:
                # 🔧 ŞARTNAME TABLO 7: N = Navy Blue (Blue + Blue)
                print("📍 N harfi: Navy Blue (Blue + Blue) - ŞARTNAME TABLO 7")
                return FILTRE_POZISYON_1 # Her iki diskte de mavi filtre
        else: # Geçersiz filtre
            print(f"⚠️ Geçersiz filtre tipi: {filter_type} - Standart konuma dönülüyor")
            return FILTRE_NORMAL

    def calistir_filtre_gorevi(self, komut):
        """
        Yer istasyonundan gelen 4 haneli komuta göre filtreleme görevini yürütür. - DEVRE DIŞI
        """
        print(f"🚫 FİLTRELEME GÖREVİ DEVRE DIŞI: {komut}")
        print("⚠️ Servo kontrolü kapalı olduğu için spektral filtreleme çalışmayacak")
        return False

    def _filtre_gorev_dongusu(self, duration1, filter1, duration2, filter2):
        """
        Multi-spektral filtreleme görevinin ana döngüsü. - DEVRE DIŞI
        """
        print(f"🚫 FİLTRELEME DÖNGÜSÜ DEVRE DIŞI: {duration1}s {filter1} + {duration2}s {filter2}")
        print("⚠️ Servo kontrolü kapalı - filtreleme işlemi gerçekleştirilmeyecek")
        
        with self._command_lock:
            self.is_command_active = False

    def buzzer_baslat(self, sure_saniye=None):
        """
        Kurtarma sinyali için buzzer'ı başlatır.
        :param sure_saniye: Buzzer çalma süresi. None ise sürekli çalar.
        """
        if self.buzzer_active:
            print("Buzzer zaten aktif.")
            return

        self.buzzer_active = True
        
        if self.simulate:
            print(f"SİMÜLASYON: Buzzer başlatıldı ({sure_saniye if sure_saniye else 'sürekli'} saniye)")
            if sure_saniye:
                time.sleep(sure_saniye)
                self.buzzer_active = False
            return

        try:
            if sure_saniye:
                # Belirli süre için buzzer
                def buzzer_thread():
                    for i in range(int(sure_saniye)):
                        if not self.buzzer_active:
                            break
                        GPIO.output(PIN_BUZZER, GPIO.HIGH)
                        time.sleep(0.5)
                        GPIO.output(PIN_BUZZER, GPIO.LOW)
                        time.sleep(0.5)
                    self.buzzer_active = False
                
                threading.Thread(target=buzzer_thread, daemon=True).start()
            else:
                # Sürekli buzzer
                GPIO.output(PIN_BUZZER, GPIO.HIGH)
                
            print(f"🔊 Buzzer başlatıldı (Pin {PIN_BUZZER})")
        except Exception as e:
            print(f"HATA: Buzzer başlatılamadı: {e}")
            self.buzzer_active = False

    def buzzer_durdur(self):
        """Buzzer'ı durdurur."""
        if not self.buzzer_active:
            return

        self.buzzer_active = False
        
        if self.simulate:
            print("SİMÜLASYON: Buzzer durduruldu")
            return

        try:
            GPIO.output(PIN_BUZZER, GPIO.LOW)
            print("🔇 Buzzer durduruldu")
        except Exception as e:
            print(f"HATA: Buzzer durdurulamadı: {e}")

    def bekle_gorev_bitene_kadar(self, timeout_saniye=20):
        """
        🔧 DÜZELTME: Multi-spektral görevin bitmesini bekler (test için gerekli)
        :param timeout_saniye: Maksimum bekleme süresi
        :return: True ise görev tamamlandı, False ise timeout
        """
        import time
        start_time = time.time()
        
        while self._is_command_active_safe():  # 🔧 DÜZELTME: Thread-safe kontrol
            if time.time() - start_time > timeout_saniye:
                print(f"⚠️ Multi-spektral görev timeout ({timeout_saniye}s)")
                return False
            time.sleep(0.1)  # CPU kullanımını azalt
        
        print("✅ Multi-spektral görev tamamlandı")
        return True

    def buzzer_kontrol(self, aktif):
        """
        🔧 DÜZELTME: Buzzer'ı aç/kapat kontrolü (ana_program.py'dan çağrılan metod)
        :param aktif: True ise buzzer çalar, False ise durur
        """
        if aktif and not self.buzzer_active:
            self.buzzer_baslat()
            print("🔊 Kurtarma modu aktif - Buzzer başlatıldı")
        elif not aktif and self.buzzer_active:
            self.buzzer_durdur()
            print("🔇 Normal mod - Buzzer durduruldu")

    def _is_command_active_safe(self):
        """Thread-safe is_command_active getter"""
        with self._command_lock:
            return self.is_command_active
    
    def temizle(self):
        """Aktüatör yöneticisini temizler ve GPIO'yu serbest bırakır."""
        try:
            with self._command_lock:  # 🔧 DÜZELTME: Thread-safe atama
                self.is_command_active = False
            
            # 🔧 RESOURCE CLEANUP: Command thread cleanup eklendi
            if self.command_thread and self.command_thread.is_alive():
                print("⏳ Command thread bitmesi bekleniyor...")
                self.command_thread.join(timeout=3)  # 3 saniye timeout
                if self.command_thread.is_alive():
                    print("⚠️ Command thread timeout - zorla sonlandırıldı")
            
            self.buzzer_durdur()
            
            if not self.simulate and GPIO_AVAILABLE:
                if self.servo1_pwm:
                    self.servo1_pwm.stop()
                if self.servo2_pwm:
                    self.servo2_pwm.stop()
                GPIO.cleanup()
                print("✅ GPIO temizlendi")
        except Exception as e:
            print(f"UYARI: GPIO temizleme hatası: {e}")

    def __del__(self):
        """Destructor - GPIO temizliği için."""
        self.temizle()
