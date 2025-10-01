#!/usr/bin/env python3
"""
XBee VIDEO TEST - Yer istasyonuna gerçek video frame gönderir
Bu script yer istasyonu ile gerçek haberleşme yapar
"""

import time
import sys
import os
import serial

# Modül yolunu ekle
sys.path.append(os.path.dirname(os.path.abspath(__file__)))

from moduller.kamera_basit import BasitKameraYoneticisi

class XBeeVideoSender:
    def __init__(self, port='/dev/serial0', baudrate=57600):
        self.port = port
        self.baudrate = baudrate
        self.serial_conn = None
        
    def connect(self):
        """XBee seri bağlantısını aç"""
        try:
            self.serial_conn = serial.Serial(
                port=self.port,
                baudrate=self.baudrate,
                timeout=1
            )
            print(f"✅ XBee bağlantısı açıldı: {self.port} @ {self.baudrate}")
            return True
        except Exception as e:
            print(f"❌ XBee bağlantı hatası: {e}")
            return False
    
    def send_video_frame(self, video_packet):
        """Video frame'i XBee üzerinden yer istasyonuna gönder"""
        try:
            if self.serial_conn and self.serial_conn.is_open:
                # Video packet'i XBee üzerinden gönder
                self.serial_conn.write(video_packet)
                self.serial_conn.flush()
                print(f"📡 XBee ile video frame gönderildi: {len(video_packet)} bytes")
                return True
            else:
                print("❌ XBee bağlantısı yok")
                return False
        except Exception as e:
            print(f"❌ XBee gönderim hatası: {e}")
            return False
    
    def close(self):
        """XBee bağlantısını kapat"""
        if self.serial_conn and self.serial_conn.is_open:
            self.serial_conn.close()
            print("🔌 XBee bağlantısı kapatıldı")

def main():
    """Ana test fonksiyonu - Gerçek XBee haberleşmesi"""
    print("📡 XBee VIDEO TEST BAŞLATIYOR")
    print("=" * 50)
    
    # XBee göndericiyi başlat
    xbee_sender = XBeeVideoSender()
    
    if not xbee_sender.connect():
        print("❌ XBee bağlantısı kurulamadı, test durduruluyor")
        return
    
    # Kamera yöneticisini oluştur
    kamera = BasitKameraYoneticisi(simulate=False)
    
    try:
        print("📹 Yer istasyonuna canlı video gönderimi başlatılıyor...")
        print("   - Video frame'ler XBee üzerinden gönderilecek")
        print("   - Yer istasyonu arayüzünde video görünecek")
        print("   - Ctrl+C ile durdurun")
        
        # Canlı yayın başlat (XBee callback ile)
        kamera.baslat_canli_yayin(xbee_sender.send_video_frame)
        
        # Sürekli çalıştır
        while True:
            time.sleep(1)
            
    except KeyboardInterrupt:
        print("\n⏹️ Kullanıcı tarafından durduruldu")
    
    finally:
        print("🛑 Sistem kapatılıyor...")
        kamera.durdur_canli_yayin()
        kamera.kapat()
        xbee_sender.close()
        print("✅ Test tamamlandı")

if __name__ == "__main__":
    main()

