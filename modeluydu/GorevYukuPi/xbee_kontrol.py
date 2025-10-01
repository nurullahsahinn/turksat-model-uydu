#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""
XBee Konfigürasyon Kontrol ve Test Scripti
TÜRKSAT Model Uydu Yarışması - Görev Yükü

Bu script XBee'nin doğru konfigüre edilip edilmediğini kontrol eder
ve temel iletişim testlerini yapar.
"""

import serial
import time
import sys
import threading
from moduller.yapilandirma import SERIAL_PORT_XBEE, SERIAL_BAUD_XBEE

class XBeeKontrolcu:
    def __init__(self):
        self.xbee_port = SERIAL_PORT_XBEE
        self.baud_rate = SERIAL_BAUD_XBEE
        self.xbee_serial = None
        self.is_connected = False
        
    def baglan(self):
        """XBee'ye bağlan"""
        try:
            self.xbee_serial = serial.Serial(
                port=self.xbee_port,
                baudrate=self.baud_rate,
                timeout=1
            )
            self.is_connected = True
            print(f"✅ XBee bağlantısı başarılı: {self.xbee_port} @ {self.baud_rate}")
            time.sleep(2)  # XBee stabilizasyonu için bekle
            return True
        except Exception as e:
            print(f"❌ XBee bağlantı hatası: {e}")
            return False
    
    def at_komut_gonder(self, komut, bekleme_suresi=1):
        """AT komutu gönder ve yanıt al"""
        try:
            # Command mode'a geç
            print("📡 Command mode'a geçiliyor...")
            self.xbee_serial.write(b"+++")
            time.sleep(1.2)  # Guard time
            
            # Yanıt kontrol et
            yanit = self.xbee_serial.read_all().decode('utf-8', errors='ignore')
            if "OK" not in yanit:
                print(f"⚠️ Command mode yanıtı beklenmedik: '{yanit}'")
            else:
                print("✅ Command mode aktif")
            
            # AT komutunu gönder
            komut_bytes = (komut + "\r").encode('utf-8')
            print(f"📤 Komut gönderiliyor: {komut}")
            self.xbee_serial.write(komut_bytes)
            time.sleep(bekleme_suresi)
            
            # Yanıt al
            yanit = self.xbee_serial.read_all().decode('utf-8', errors='ignore').strip()
            print(f"📥 Yanıt: '{yanit}'")
            
            # Command mode'dan çık
            self.xbee_serial.write(b"ATCN\r")
            time.sleep(0.5)
            
            return yanit
            
        except Exception as e:
            print(f"❌ AT komut hatası: {e}")
            return None
    
    def konfigurasyonu_kontrol_et(self):
        """XBee konfigürasyonunu kontrol et"""
        print("\n🔍 XBee Konfigürasyon Kontrolü")
        print("=" * 40)
        
        kontrol_listesi = [
            ("ATCH", "12", "Kanal (Channel) - 12 olmalı"),
            ("ATID", "6570", "PAN ID - 6570 olmalı"),
            ("ATMY", "1", "16-bit Address - 1 olmalı (Payload)"),
            ("ATAP", "0", "API Mode - 0 olmalı (Transparent)"),
            ("ATBD", "3", "Baud Rate - 3 olmalı (57600)"),
            ("ATDH", "13A200", "Destination High - 13A200 olmalı"),
            ("ATDL", "FFFF", "Destination Low - FFFF olmalı (Broadcast)"),
            ("ATAI", "0", "Association - 0 olmalı (Başarılı)"),
            ("ATVR", "", "Firmware Version")
        ]
        
        sonuclar = {}
        for komut, beklenen, aciklama in kontrol_listesi:
            yanit = self.at_komut_gonder(komut)
            if yanit is not None:
                yanit = yanit.replace("OK", "").strip()
                sonuclar[komut] = yanit
                
                if beklenen and yanit == beklenen:
                    print(f"✅ {aciklama}: {yanit}")
                elif beklenen:
                    print(f"❌ {aciklama}: {yanit} (beklenen: {beklenen})")
                else:
                    print(f"ℹ️ {aciklama}: {yanit}")
            else:
                print(f"❌ {aciklama}: Komut başarısız")
                sonuclar[komut] = "HATA"
        
        return sonuclar
    
    def iletisim_testi(self):
        """XBee iletişim testi"""
        print("\n📡 XBee İletişim Testi")
        print("=" * 30)
        
        # Dinleme thread'i başlat
        self.dinleme_aktif = True
        dinleme_thread = threading.Thread(target=self.dinleme_worker, daemon=True)
        dinleme_thread.start()
        
        print("📤 Test mesajları gönderiliyor...")
        test_mesajlari = [
            "!XBEE_TEST!",
            "!TEST!",
            "!6G9R!",
            "SAHA:BASINC2:1013.25"
        ]
        
        for mesaj in test_mesajlari:
            print(f"📤 Gönderiliyor: {mesaj}")
            self.xbee_serial.write((mesaj + "\n").encode('utf-8'))
            time.sleep(2)
        
        print("⏳ 10 saniye dinleme...")
        time.sleep(10)
        self.dinleme_aktif = False
    
    def dinleme_worker(self):
        """XBee'den gelen mesajları dinle"""
        while self.dinleme_aktif:
            try:
                if self.xbee_serial.in_waiting > 0:
                    veri = self.xbee_serial.read_all()
                    mesaj = veri.decode('utf-8', errors='ignore').strip()
                    if mesaj:
                        print(f"📥 Alınan: {mesaj}")
                time.sleep(0.1)
            except Exception as e:
                print(f"❌ Dinleme hatası: {e}")
                break
    
    def kapat(self):
        """Bağlantıyı kapat"""
        if self.xbee_serial:
            self.xbee_serial.close()
            print("✅ XBee bağlantısı kapatıldı")

def main():
    """Ana test fonksiyonu"""
    print("🛰️ TÜRKSAT Model Uydu XBee Kontrol v1.0")
    print("=" * 50)
    
    kontrol = XBeeKontrolcu()
    
    try:
        # XBee'ye bağlan
        if not kontrol.baglan():
            print("❌ XBee bağlantısı kurulamadı!")
            return
        
        # Konfigürasyonu kontrol et
        kontrol.konfigurasyonu_kontrol_et()
        
        # İletişim testi yap
        kontrol.iletisim_testi()
        
        print("\n✅ XBee kontrol testi tamamlandı!")
        
    except KeyboardInterrupt:
        print("\n⏹️ Test kullanıcı tarafından durduruldu")
    except Exception as e:
        print(f"❌ Test hatası: {e}")
    finally:
        kontrol.kapat()

if __name__ == "__main__":
    main()





