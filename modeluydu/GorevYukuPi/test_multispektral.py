#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""
Multispektral Filtre Test Scripti
TÜRKSAT Model Uydu Yarışması - Görev Yükü

Bu script multispektral komut sistemi ve XBee iletişimini test eder.
"""

import time
import sys
import os

# Ana program modüllerini import et
sys.path.append('/home/atugem/TurkSatModelUydu/GorevYukuPi')

from moduller.birlesik_xbee_alici import BirlesikXBeeAlici
from moduller.aktuatorler import AktuatorYoneticisi
from moduller.yapilandirma import IS_RASPBERRY_PI

def test_callback(command):
    """Test komut callback fonksiyonu"""
    print(f"🎯 TEST: Komut alındı: {command}")
    
    # Test komutları
    if command == "!XBEE_TEST!":
        print("✅ XBee test komutu başarıyla alındı!")
    elif command.startswith("!") and command.endswith("!"):
        komut_ici = command[1:-1]
        if len(komut_ici) == 4:
            print(f"✅ Multispektral komut alındı: {komut_ici}")
        elif "M1:" in komut_ici and "M2:" in komut_ici:
            print(f"✅ Motor kontrol komutu alındı: {komut_ici}")
        else:
            print(f"⚠️ Bilinmeyen komut formatı: {komut_ici}")
    else:
        print(f"ℹ️ Diğer komut: {command}")

def main():
    """Ana test fonksiyonu"""
    print("🛰️ TÜRKSAT Multispektral Test v1.0")
    print("=" * 50)
    
    # XBee sistemini başlat
    print("📡 XBee sistemi başlatılıyor...")
    xbee = BirlesikXBeeAlici(
        command_callback=test_callback,
        debug=True,  # Debug açık
        simulate=not IS_RASPBERRY_PI
    )
    
    if not xbee.connect_xbee():
        print("❌ XBee bağlantısı kurulamadı!")
        return
    
    xbee.start_listening()
    print("✅ XBee sistemi başlatıldı")
    
    # Aktuator sistemini başlat
    print("🔧 Aktuator sistemi başlatılıyor...")
    aktuator = AktuatorYoneticisi(simulate=not IS_RASPBERRY_PI)
    print("✅ Aktuator sistemi başlatıldı")
    
    print("\n🔍 Test Modları:")
    print("1. XBee İletişim Testi")
    print("2. Multispektral Komut Testi")
    print("3. Sürekli Dinleme Modu")
    print("4. Manuel Test Komutları")
    
    try:
        while True:
            print("\n" + "="*50)
            secim = input("Test seçin (1-4, q=çıkış): ").strip().lower()
            
            if secim == 'q':
                break
            elif secim == '1':
                print("📡 XBee test mesajı gönderiliyor...")
                xbee.send_telemetry("XBEE_TEST_FROM_PAYLOAD")
                print("✅ Test mesajı gönderildi")
                
            elif secim == '2':
                print("🌈 Multispektral test komutları:")
                test_komutlari = ["6G9R", "7R8B", "6Y9C"]
                for komut in test_komutlari:
                    print(f"🎯 Test komutu: {komut}")
                    test_callback(f"!{komut}!")
                    
                    # Aktuator test
                    if aktuator:
                        try:
                            aktuator.calistir_filtre_gorevi(komut)
                            print(f"✅ Aktuator test başarılı: {komut}")
                        except Exception as e:
                            print(f"❌ Aktuator test hatası: {e}")
                    
                    time.sleep(2)
                    
            elif secim == '3':
                print("👂 Sürekli dinleme modu (Ctrl+C ile çıkış)")
                print("Yer istasyonundan komut gönderin...")
                try:
                    while True:
                        time.sleep(1)
                except KeyboardInterrupt:
                    print("\n⏹️ Dinleme modu durduruldu")
                    
            elif secim == '4':
                print("⌨️ Manuel test komutları:")
                print("Örnek komutlar:")
                print("  !6G9R! - Basit multispektral")
                print("  !M1:6:G:2000;M2:9:R:2000! - Motor kontrol")
                print("  !XBEE_TEST! - XBee test")
                
                komut = input("Komut girin: ").strip()
                if komut:
                    test_callback(komut)
            else:
                print("❌ Geçersiz seçim!")
                
    except KeyboardInterrupt:
        print("\n⏹️ Test kullanıcı tarafından durduruldu")
    except Exception as e:
        print(f"❌ Test hatası: {e}")
    finally:
        print("🔧 Temizlik yapılıyor...")
        if xbee:
            xbee.stop_listening()
        if aktuator:
            aktuator.temizle()
        print("✅ Test tamamlandı!")

if __name__ == "__main__":
    main()





