# -*- coding: utf-8 -*-
"""
Güç Yöneticisi Modülü

Bu modül, güç butonu kontrolü ve güç yönetimi işlemlerini yapar.
- Güç butonu durumu izleme
- Güç LED'i kontrolü
- Güvenli kapanma prosedürü
"""

import time
import threading
from moduller.yapilandirma import IS_RASPBERRY_PI, PIN_POWER_BUTTON, PIN_POWER_LED

if IS_RASPBERRY_PI:
    import RPi.GPIO as GPIO
else:
    print("UYARI: GPIO kütüphanesi bulunamadı. Güç yöneticisi simülasyon modunda.")

class GucYoneticisi:
    def __init__(self, simulate=not IS_RASPBERRY_PI, aktuator_yoneticisi=None, shutdown_callback=None):
        """Güç yöneticisini başlatır"""
        self.simulate = simulate
        self.aktuator_yoneticisi = aktuator_yoneticisi  # 🔧 DÜZELTME: Buzzer kontrolü için
        self.shutdown_callback = shutdown_callback  # 🔧 DÜZELTME: Callback tanımı
        self.monitoring = False
        self.monitor_thread = None
        self.power_led_state = False
        
        # 🔧 LONG_PRESS_DURATION tanımı (Analiz4.txt düzeltmesi)
        self.LONG_PRESS_DURATION = 3.0  # 3 saniye uzun basma süresi
        
        # Kritik pil seviyesi kontrolü (Gereksinim 20)
        self.critical_battery_voltage = 3.2  # Li-Ion için kritik seviye
        self.battery_check_interval = 30  # 30 saniyede bir kontrol
        self.last_battery_check = 0
        
        # Pil gerilimi yöneticisi referansı (set edilecek)
        self.battery_manager = None
        
        if not self.simulate:
            self._setup_gpio()
            print("✅ Güç yöneticisi başlatıldı (GPIO modu)")
        else:
            print("✅ Güç yöneticisi başlatıldı (simülasyon modu)")
    
    def set_battery_manager(self, battery_manager):
        """Pil gerilimi yöneticisi referansını ayarlar"""
        self.battery_manager = battery_manager
        print("✅ Pil gerilimi yöneticisi bağlandı")
    
    def check_critical_battery(self):
        """
        Kritik pil seviyesi kontrolü (Gereksinim 20)
        Sistemin 1 saatlik çalışma süresi garantisi
        """
        if not self.battery_manager:
            return False
            
        try:
            # Ana Li-Ion pil voltajını kontrol et
            battery_data = self.battery_manager.get_battery_voltages()
            main_voltage = battery_data.get('3v7', 0.0)
            
            if main_voltage > 0 and main_voltage < self.critical_battery_voltage:
                print(f"🚨 KRİTİK PİL UYARISI: {main_voltage:.2f}V (Minimum: {self.critical_battery_voltage}V)")
                return True
                
            return False
            
        except Exception as e:
            print(f"⚠️ Pil kontrolü hatası: {e}")
            return False
    
    def initiate_emergency_shutdown(self):
        """
        Acil durum kapanması (kritik pil seviyesi)
        """
        print("🚨 ACİL DURUM KAPANMASI BAŞLATILIYOR!")
        print("📧 Sebep: Kritik pil seviyesi")
        
        # Buzzer uyarısı (kısa-uzun pattern)
        if self.aktuator_yoneticisi:  # 🔧 DÜZELTME: Aktuator referans kontrolü
            for _ in range(3):
                self.aktuator_yoneticisi.buzzer_kontrol(True)
                time.sleep(0.2)
                self.aktuator_yoneticisi.buzzer_kontrol(False)
                time.sleep(0.1)
                self.aktuator_yoneticisi.buzzer_kontrol(True)
                time.sleep(0.5)
                self.aktuator_yoneticisi.buzzer_kontrol(False)
                time.sleep(0.2)
        
        # LED yanıp sönme
        for _ in range(10):
            self.set_power_led(True)
            time.sleep(0.1)
            self.set_power_led(False)
            time.sleep(0.1)
        
        # Sistem kapanması
        self._initiate_shutdown()

    def _setup_gpio(self):
        """GPIO pinlerini yapılandır"""
        try:
            GPIO.setmode(GPIO.BCM)
            GPIO.setwarnings(False)
            
            # Güç butonu girişi (pull-up dirençli)
            GPIO.setup(PIN_POWER_BUTTON, GPIO.IN, pull_up_down=GPIO.PUD_UP)
            
            # Güç LED'i çıkışı
            GPIO.setup(PIN_POWER_LED, GPIO.OUT)
            
            # Başlangıçta LED'i aç (sistem çalışıyor)
            self.set_power_led(True)
            
            print("✅ Güç yöneticisi GPIO'ları başlatıldı")
            
        except Exception as e:
            print(f"HATA: Güç yöneticisi GPIO kurulumu: {e}")
            self.simulate = True

    def start_monitoring(self):
        """Güç butonu izlemeyi başlat"""
        if self.monitoring:
            return
            
        self.monitoring = True
        self.monitor_thread = threading.Thread(target=self._monitor_power_button, daemon=True)
        self.monitor_thread.start()
        print("🔍 Güç butonu izleme başlatıldı")

    def stop_monitoring(self):
        """Güç butonu izlemeyi durdur"""
        self.monitoring = False
        if self.monitor_thread:
            self.monitor_thread.join(timeout=1)
        print("⏹️ Güç butonu izleme durduruldu")

    def _monitor_power_button(self):
        """Güç butonunu ve pil seviyesini izleyen döngü"""
        button_press_start = None
        
        print("🔋 Güç butonu ve pil izleme başlatıldı")
        
        while self.monitoring:
            try:
                current_time = time.time()
                
                # Kritik pil seviyesi kontrolü (30 saniyede bir)
                if current_time - self.last_battery_check > self.battery_check_interval:
                    self.last_battery_check = current_time
                    
                    if self.check_critical_battery():
                        print("🚨 Kritik pil seviyesi - acil kapanma başlatılıyor!")
                        self.initiate_emergency_shutdown()
                        break
                
                if self.simulate:
                    # Simülasyon modunda sadece pil kontrolü
                    time.sleep(1)
                    continue
                
                # Güç butonu kontrolü
                button_state = GPIO.input(PIN_POWER_BUTTON)
                
                if button_state == GPIO.LOW:  # Buton basılı
                    if button_press_start is None:
                        button_press_start = current_time
                        print("🔘 Güç butonu basıldı")
                    
                    press_duration = current_time - button_press_start
                    
                    # 3 saniye uzun basma kontrolü (Gereksinim 22)
                    if press_duration >= self.LONG_PRESS_DURATION:
                        print(f"🔴 Uzun basma algılandı ({press_duration:.1f}s) - Kapanıyor...")
                        self._initiate_shutdown()
                        button_press_start = None
                        break
                        
                else:  # Buton bırakıldı
                    if button_press_start is not None:
                        press_duration = current_time - button_press_start
                        print(f"🔘 Güç butonu bırakıldı ({press_duration:.1f}s)")
                        button_press_start = None
                
                time.sleep(0.1)  # CPU kullanımını azalt
                
            except Exception as e:
                print(f"HATA: Güç butonu/pil izleme: {e}")
                time.sleep(1)
        
        print("🔋 Güç butonu ve pil izleme durduruldu")

    def _initiate_shutdown(self):
        """Güvenli kapanma prosedürünü başlat"""
        try:
            print("🛑 GÜVENLİ KAPANMA BAŞLATILUYOR...")
            
            # LED'i yanıp söndür (kapanma sinyali)
            for _ in range(6):
                self.set_power_led(False)
                time.sleep(0.25)
                self.set_power_led(True)
                time.sleep(0.25)
            
            # Callback fonksiyonunu çağır (ana programdan temizlik)
            if self.shutdown_callback:
                self.shutdown_callback()
            
            # LED'i kapat
            self.set_power_led(False)
            
            if not self.simulate:
                # Raspberry Pi'yi güvenli şekilde kapat
                import subprocess
                print("💀 Sistem kapatılıyor...")
                subprocess.run(["sudo", "shutdown", "now"], check=False)
            else:
                print("🔧 Simülasyon: Sistem kapatılmadı")
                
        except Exception as e:
            print(f"HATA: Güvenli kapanma: {e}")

    def set_power_led(self, state):
        """Güç LED'ini kontrol et"""
        try:
            if not self.simulate:
                GPIO.output(PIN_POWER_LED, GPIO.HIGH if state else GPIO.LOW)
            
            self.power_led_state = state
            
        except Exception as e:
            print(f"HATA: Güç LED kontrolü: {e}")

    def get_power_button_state(self):
        """Güç butonu durumunu döndür"""
        if self.simulate:
            return False
            
        try:
            return GPIO.input(PIN_POWER_BUTTON) == GPIO.LOW
        except:
            return False

    def cleanup(self):
        """Güç yöneticisini temizle"""
        self.stop_monitoring()
        
        if not self.simulate:
            try:
                self.set_power_led(False)
                # GPIO'yu burada temizleme, ana program tarafından yapılacak
                print("🧹 Güç yöneticisi temizlendi")
            except:
                pass
        
    def __del__(self):
        """Destructor"""
        self.cleanup() 