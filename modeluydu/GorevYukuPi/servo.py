#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""
SG90 Servo Test Kodu - Tam çalışır versiyon
"""

import time
import threading
import RPi.GPIO as GPIO

# GPIO pin ayarları
PIN_SERVO_1 = 12  # GPIO 12 birinci servo için
PIN_SERVO_2 = 13  # GPIO 13 ikinci servo için

# Temel renk pozisyonları
FILTRE_NORMAL = 0       # Normal/Şeffaf: 0° 
FILTRE_KIRMIZI = 45     # Kırmızı: 45°
FILTRE_YESIL = 90       # Yeşil: 90°
FILTRE_MAVI = 135       # Mavi: 135°

class ServoTester:
    def __init__(self):
        """Servo test için başlatma"""
        self.servo1_pwm = None
        self.servo2_pwm = None
        
        # Multi-spektral harita tanımla (class içinde)
        self.MULTI_SPEKTRAL_HARITASI = {
            # Tek renkler (aynı renk + aynı renk)
            'M': (FILTRE_KIRMIZI, FILTRE_KIRMIZI),  # Maroon Red = Red + Red
            'F': (FILTRE_YESIL, FILTRE_YESIL),      # Forest Green = Green + Green  
            'N': (FILTRE_MAVI, FILTRE_MAVI),        # Navy Blue = Blue + Blue
            
            # Açık renkler (renk + normal)
            'R': (FILTRE_KIRMIZI, FILTRE_NORMAL),   # Light Red = Red + Normal
            'G': (FILTRE_YESIL, FILTRE_NORMAL),     # Light Green = Green + Normal
            'B': (FILTRE_MAVI, FILTRE_NORMAL),      # Light Blue = Blue + Normal
            
            # Karışık renkler
            'P': (FILTRE_KIRMIZI, FILTRE_MAVI),     # Purple = Red + Blue
            'Y': (FILTRE_KIRMIZI, FILTRE_YESIL),    # Yellow = Red + Green
            'C': (FILTRE_MAVI, FILTRE_YESIL),       # Cyan = Blue + Green
        }
        
        # Basit renk haritası (eski sistem uyumluluğu)
        self.RENK_HARITASI = {
            'B': FILTRE_NORMAL,    # Normal/Şeffaf
            'K': FILTRE_KIRMIZI,   # Kırmızı
            'Y': FILTRE_YESIL,     # Yeşil
            'M': FILTRE_MAVI       # Mavi
        }
        
        self._gpio_kurulum()

    def _gpio_kurulum(self):
        """GPIO ve PWM kurulum"""
        try:
            GPIO.setmode(GPIO.BCM)
            
            # Pinleri output olarak ayarla
            GPIO.setup(PIN_SERVO_1, GPIO.OUT)
            GPIO.setup(PIN_SERVO_2, GPIO.OUT)

            # PWM nesnelerini oluştur (50Hz servo için)
            self.servo1_pwm = GPIO.PWM(PIN_SERVO_1, 50)
            self.servo2_pwm = GPIO.PWM(PIN_SERVO_2, 50)
            
            # PWM başlat (orta pozisyon)
            self.servo1_pwm.start(self._aci_duty_cycle(90))
            self.servo2_pwm.start(self._aci_duty_cycle(90))
            
            print(f"✅ GPIO kurulum tamam - Servo1: Pin {PIN_SERVO_1}, Servo2: Pin {PIN_SERVO_2}")
            time.sleep(1)  # Servo stabilizasyonu için bekle
            
        except Exception as e:
            print(f"❌ GPIO kurulum hatası: {e}")
            raise

    def _aci_duty_cycle(self, aci):
        """
        Açıyı SG90 için duty cycle'a çevir
        
        SG90 özellikleri:
        0° = 2.5% duty cycle (500µs pulse)
        90° = 7.5% duty cycle (1500µs pulse)  
        180° = 12.5% duty cycle (2500µs pulse)
        """
        # Açıyı 0-180 arası sınırla
        aci = max(0, min(180, aci))
        
        # Duty cycle hesapla: 2.5% to 12.5%
        duty_cycle = 2.5 + (aci / 180.0) * 10.0
        return duty_cycle

    def servo_hareket_et(self, servo_no, aci):
        """Belirli servoya belirli açıya hareket ettir"""
        try:
            duty_cycle = self._aci_duty_cycle(aci)
            
            if servo_no == 1 and self.servo1_pwm:
                self.servo1_pwm.ChangeDutyCycle(duty_cycle)
                print(f"🔧 Servo 1 -> {aci}° (duty: {duty_cycle:.2f}%)")
            elif servo_no == 2 and self.servo2_pwm:
                self.servo2_pwm.ChangeDutyCycle(duty_cycle)
                print(f"🔧 Servo 2 -> {aci}° (duty: {duty_cycle:.2f}%)")
            else:
                print(f"❌ Hata: Geçersiz servo numarası: {servo_no}")
                return False
                
            time.sleep(0.8)  # Servo hareketi için bekle
            return True
            
        except Exception as e:
            print(f"❌ Servo {servo_no} hareket hatası: {e}")
            return False

    def her_iki_servo_hareket_et(self, aci):
        """Her iki servoya aynı açıya hareket ettir"""
        print(f"\n📍 Her iki servo {aci}° açısına hareket ediyor...")
        
        try:
            duty_cycle = self._aci_duty_cycle(aci)
            
            self.servo1_pwm.ChangeDutyCycle(duty_cycle)
            self.servo2_pwm.ChangeDutyCycle(duty_cycle)
            
            print(f"🔧 Her iki servo -> {aci}° (duty: {duty_cycle:.2f}%)")
            time.sleep(1.0)  # Senkron hareket için daha uzun bekle
            return True
            
        except Exception as e:
            print(f"❌ Servo hareket hatası: {e}")
            return False

    def temel_pozisyon_test(self):
        """Temel pozisyonları test et (4 aşama)"""
        print("\n🧪 Temel pozisyon testi (4 aşama)...")
        
        test_pozisyonlari = [
            (FILTRE_NORMAL, f"Normal - {FILTRE_NORMAL}°"),
            (FILTRE_KIRMIZI, f"Kırmızı - {FILTRE_KIRMIZI}°"),
            (FILTRE_YESIL, f"Yeşil - {FILTRE_YESIL}°"),
            (FILTRE_MAVI, f"Mavi - {FILTRE_MAVI}°")
        ]
        
        for aci, aciklama in test_pozisyonlari:
            print(f"\n🔸 Test: {aciklama}")
            print("Devam etmek için Enter'a basın...")
            input()
            
            self.her_iki_servo_hareket_et(aci)
            
        print("\n✅ Temel pozisyon testi tamamlandı!")

    def multi_spektral_komut_calistir(self, komut):
        """
        Multi-spektral komut çalıştır (örnek: 6G 9R)
        Format: [süre][kod] [süre][kod]
        Kodlar: M,F,N,R,G,B,P,Y,C (Jdol 1'e göre)
        """
        print(f"\n🌈 Multi-spektral komut çalıştırılıyor: {komut}")
        
        try:
            # Komutu parçalara ayır
            parcalar = komut.strip().upper().split()
            
            if len(parcalar) != 2:
                print("❌ Hata: Komut formatı yanlış! Örnek: 6G 9R")
                return False
            
            # İlk komutu parse et
            ilk_komut = parcalar[0]
            if len(ilk_komut) < 2:
                print("❌ Hata: İlk komut çok kısa!")
                return False
            
            sure1 = int(ilk_komut[:-1])  # Süre
            kod1 = ilk_komut[-1]        # Multi-spektral kod
            
            # İkinci komutu parse et  
            ikinci_komut = parcalar[1]
            if len(ikinci_komut) < 2:
                print("❌ Hata: İkinci komut çok kısa!")
                return False
                
            sure2 = int(ikinci_komut[:-1])  # Süre
            kod2 = ikinci_komut[-1]        # Multi-spektral kod
            
            # Kodları kontrol et
            if kod1 not in self.MULTI_SPEKTRAL_HARITASI or kod2 not in self.MULTI_SPEKTRAL_HARITASI:
                print(f"❌ Hata: Geçersiz kod! Geçerli kodlar: {list(self.MULTI_SPEKTRAL_HARITASI.keys())}")
                print("🎨 Geçerli kodlar: M, F, N, R, G, B, P, Y, C")
                return False
            
            # Süreleri kontrol et (6-9 saniye arası şartname gereği)
            if not (6 <= sure1 <= 9) or not (6 <= sure2 <= 9):
                print("❌ Hata: Süreler 6-9 saniye arasında olmalı! (Şartname)")
                return False
                
            if sure1 + sure2 != 15:
                print("❌ Hata: Toplam süre 15 saniye olmalı! (Şartname)")
                return False
            
            # Renk kombinasyonlarını al
            servo1_aci1, servo2_aci1 = self.MULTI_SPEKTRAL_HARITASI[kod1]
            servo1_aci2, servo2_aci2 = self.MULTI_SPEKTRAL_HARITASI[kod2]
            
            print(f"✅ Multi-spektral komut analiz edildi:")
            print(f"   1. Aşama: {sure1}sn {kod1} (Servo1:{servo1_aci1}°, Servo2:{servo2_aci1}°)")
            print(f"   2. Aşama: {sure2}sn {kod2} (Servo1:{servo1_aci2}°, Servo2:{servo2_aci2}°)")
            
            # 1. BAŞLANGIÇ: Her iki servo normal pozisyona (0°)
            print(f"\n1️⃣ Başlangıç: Her iki servo normal pozisyona (0°)...")
            self.servo_hareket_et(1, FILTRE_NORMAL)
            self.servo_hareket_et(2, FILTRE_NORMAL)
            time.sleep(1)  # Stabilizasyon
            
            # 2. İLK AŞAMA: Kod1 kombinasyonu
            print(f"\n2️⃣ İlk aşama: {kod1} - {sure1} saniye")
            self.servo_hareket_et(1, servo1_aci1)
            self.servo_hareket_et(2, servo2_aci1)
            
            # Süre sayacı
            for i in range(sure1):
                print(f"   ⏱️ {kod1}: {i+1}/{sure1} saniye...")
                time.sleep(1)
            
            # 3. İKİNCİ AŞAMA: Kod2 kombinasyonu
            print(f"\n3️⃣ İkinci aşama: {kod2} - {sure2} saniye")
            self.servo_hareket_et(1, servo1_aci2)
            self.servo_hareket_et(2, servo2_aci2)
            
            # Süre sayacı
            for i in range(sure2):
                print(f"   ⏱️ {kod2}: {i+1}/{sure2} saniye...")
                time.sleep(1)
            
            # 4. BİTİŞ: Her iki servo normal pozisyona dön
            print(f"\n4️⃣ Bitiş: Her iki servo normal pozisyona dönüyor (0°)...")
            self.servo_hareket_et(1, FILTRE_NORMAL)
            self.servo_hareket_et(2, FILTRE_NORMAL)
            
            print(f"\n✅ Multi-spektral komut başarıyla tamamlandı!")
            print(f"   📊 Toplam süre: {sure1 + sure2} saniye")
            return True
            
        except ValueError:
            print("❌ Hata: Süre değeri geçersiz! Sayı olmalı.")
            return False
        except Exception as e:
            print(f"❌ Multi-spektral komut hatası: {e}")
            return False

    def multi_spektral_interaktif_mod(self):
        """Multi-spektral sistem için interaktif mod"""
        print("\n🌈 Multi-Spektral İnteraktif Mod")
        print("=" * 50)
        print("📋 Komut formatı: [süre][kod] [süre][kod]")
        print("🎨 Multi-spektral kodlar:")
        print("   M = Maroon Red (Red+Red)")
        print("   F = Forest Green (Green+Green)")  
        print("   N = Navy Blue (Blue+Blue)")
        print("   R = Light Red (Red+Normal)")
        print("   G = Light Green (Green+Normal)")
        print("   B = Light Blue (Blue+Normal)")
        print("   P = Purple (Red+Blue)")
        print("   Y = Yellow (Red+Green)")
        print("   C = Cyan (Blue+Green)")
        print("⏱️ Süre: 6-9 saniye (toplam 15sn)")
        print("💡 Örnek komutlar:")
        print("   6G 9R = 6sn Light Green + 9sn Light Red")
        print("   7Y 8P = 7sn Yellow + 8sn Purple")
        print("   9M 6C = 9sn Maroon Red + 6sn Cyan")
        print("❌ Çıkmak için 'q' yazın")
        print("=" * 50)
        
        while True:
            print(f"\n🎯 Multi-spektral komut girin:", end=" ")
            kullanici_komut = input().strip()
            
            if kullanici_komut.lower() == 'q':
                print("👋 Multi-spektral mod sonlandırıldı")
                break
            
            if not kullanici_komut:
                print("⚠️ Boş komut! Tekrar deneyin.")
                continue
            
            # Multi-spektral komutu çalıştır
            basarili = self.multi_spektral_komut_calistir(kullanici_komut)
            
            if basarili:
                print("🎉 Multi-spektral komut başarıyla tamamlandı!")
            else:
                print("❌ Multi-spektral komut başarısız! Tekrar deneyin.")
                print("💡 Hatırlatma: Format [6-9][kod] [6-9][kod], toplam 15sn (örnek: 6G 9R)")

    def normal_pozisyona_don(self):
        """Servoları normal pozisyona getir (normal)"""
        print("\n🔄 Servoları normal pozisyona getiriyor (normal)...")
        self.her_iki_servo_hareket_et(FILTRE_NORMAL)
        print(f"✅ Servolar normal pozisyonda ({FILTRE_NORMAL}°)")

    def temizle(self):
        """GPIO temizle"""
        try:
            if self.servo1_pwm:
                self.servo1_pwm.stop()
            if self.servo2_pwm:
                self.servo2_pwm.stop()
            GPIO.cleanup()
            print("✅ GPIO temizlendi")
        except Exception as e:
            print(f"⚠️ Uyarı: GPIO temizleme hatası: {e}")

def main():
    """Ana test programı"""
    servo_tester = None
    
    try:
        print("=" * 50)
        print("🚀 SG90 Servo Test Programı")
        print("=" * 50)
        
        # Test nesnesi oluştur
        servo_tester = ServoTester()
        
        while True:
            print("\n" + "=" * 50)
            print("📋 Test Menüsü:")
            print("1. Temel pozisyon testi (4 renk)")
            print("2. Normal pozisyona dön")
            print("3. Multi-spektral mod (6G 9R)")
            print("4. Çıkış")
            print("=" * 50)
            print("🌈 Renk pozisyonları:")
            print("   ⚪ Normal: 0°")
            print("   ❤️ Kırmızı: 45°")
            print("   💚 Yeşil: 90°")
            print("   💙 Mavi: 135°")
            print("=" * 50)
            
            secim = input("Test numarasını seçin: ").strip()
            
            if secim == '1':
                servo_tester.temel_pozisyon_test()
            elif secim == '2':
                servo_tester.normal_pozisyona_don()
            elif secim == '3':
                servo_tester.multi_spektral_interaktif_mod()
            elif secim == '4':
                print("👋 Program sonlanıyor...")
                break
            else:
                print("❌ Geçersiz seçim!")
                
    except KeyboardInterrupt:
        print("\n⏹️ Program kullanıcı tarafından durduruldu")
    except Exception as e:
        print(f"❌ Program hatası: {e}")
    finally:
        if servo_tester:
            servo_tester.temizle()

if __name__ == "__main__":
    main()