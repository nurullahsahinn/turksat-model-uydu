#!/usr/bin/env python3
"""
SADECE KAMERA YAYIN - Direkt XBee Video Streaming
Sade ve etkili: Sadece kamera açar, video çeker, XBee ile yer istasyonuna gönderir.
Telemetri yok, sensör yok, karmaşıklık yok!
"""

import time
import sys
import os
import serial
import threading
from datetime import datetime
from io import BytesIO

print("🎬 SADECE KAMERA YAYIN BAŞLATIYOR!")
print("=" * 60)

# Raspberry Pi kamera kontrolü
IS_RASPBERRY_PI = True
try:
    if IS_RASPBERRY_PI:
        from picamera2 import Picamera2
        print("✅ Picamera2 kütüphanesi yüklendi")
except ImportError:
    print("❌ Picamera2 bulunamadı, simülasyon modunda çalışacak")
    IS_RASPBERRY_PI = False

class SadeKameraYayinci:
    def __init__(self):
        self.camera = None
        self.xbee_serial = None
        self.is_streaming = False
        self.streaming_thread = None
        
        print("🎥 Sade Kamera Yayıncısı başlatılıyor...")
        
    def kamera_baslat(self):
        """Kamerayı başlat - SADE VERSİYON"""
        if not IS_RASPBERRY_PI:
            print("📱 Simülasyon modu - gerçek kamera yok")
            return True
            
        try:
            print("🎥 Raspberry Pi kamerası başlatılıyor...")
            self.camera = Picamera2()
            
            # DENGE AYARLARI - Kalite vs Boyut (57600 baud için)
            config = self.camera.create_video_configuration(
                 main={"size": (240, 180), "format": "RGB888"},  # Daha büyük boyut
                 controls={"FrameRate": 4}  # 4 FPS - hızlandırılmış
             )
            
            self.camera.configure(config)
            self.camera.start()
            
            print("✅ Kamera başlatıldı: 240x180 @ 4 FPS")
            time.sleep(2)  # Kamera ısınsın
            return True
            
        except Exception as e:
            print(f"❌ Kamera başlatma hatası: {e}")
            return False
    
    def xbee_baglanti_ac(self, port='/dev/serial0', baudrate=57600):
        """XBee seri bağlantısını aç"""
        try:
            print(f"📡 XBee bağlantısı açılıyor: {port} @ {baudrate}")
            
            self.xbee_serial = serial.Serial(
                port=port,
                baudrate=baudrate,
                timeout=1,
                bytesize=serial.EIGHTBITS,
                parity=serial.PARITY_NONE,
                stopbits=serial.STOPBITS_ONE
            )
            
            print("✅ XBee bağlantısı başarılı")
            return True
            
        except Exception as e:
            print(f"❌ XBee bağlantı hatası: {e}")
            print("💡 Port kullanımda olabilir, başka program kapatın")
            return False
    
    def video_frame_yakala(self):
        """Tek bir video frame yakala"""
        if not IS_RASPBERRY_PI:
            # Simülasyon için sahte JPEG data
            return b'\xff\xd8\xff\xe0\x00\x10JFIF\x00\x01\x01\x00\x00\x01\x00\x01\x00\x00\xff\xd9'
        
        try:
            stream = BytesIO()
            self.camera.capture_file(stream, format='jpeg')
            stream.seek(0)
            frame_data = stream.getvalue()
            stream.close()
            
            # Frame boyutunu kontrol et (XBee için maksimum 5KB - hızlandırılmış)
            if len(frame_data) > 5120:  # 5KB limit (hızlandırılmış ayar)
                # PIL ile sıkıştır
                try:
                    from PIL import Image
                    import io
                    
                    img = Image.open(io.BytesIO(frame_data))
                    output_stream = BytesIO()
                    img.save(output_stream, format='JPEG', quality=18, optimize=True)
                    frame_data = output_stream.getvalue()
                    output_stream.close()
                    
                except ImportError:
                    print("⚠️ PIL bulunamadı, frame sıkıştırılamıyor")
                    
            return frame_data
            
        except Exception as e:
            print(f"❌ Frame yakalama hatası: {e}")
            return None
    
    def video_frame_gonder(self, frame_data):
        """Video frame'i XBee ile yer istasyonuna gönder"""
        if not self.xbee_serial or not frame_data:
            return False
            
        try:
            # YER İSTASYONU UYUMLU FORMAT: #VIDEO:base64_data#
            import base64
            base64_data = base64.b64encode(frame_data).decode('utf-8')
            video_message = f"#VIDEO:{base64_data}#\n"
            
            # XBee ile gönder
            self.xbee_serial.write(video_message.encode('utf-8'))
            self.xbee_serial.flush()
            
            print(f"📡 Video frame gönderildi: {len(frame_data)} bytes → {len(video_message)} chars")
            return True
            
        except Exception as e:
            print(f"❌ Video gönderim hatası: {e}")
            return False
    
    def canli_yayin_baslat(self):
        """Canlı yayın thread'ini başlat"""
        if self.is_streaming:
            print("⚠️ Canlı yayın zaten aktif")
            return
            
        self.is_streaming = True
        self.streaming_thread = threading.Thread(target=self._yayin_dongusu, daemon=True)
        self.streaming_thread.start()
        print("🎬 Canlı yayın başlatıldı!")
    
    def _yayin_dongusu(self):
        """Ana yayın döngüsü - sürekli çalışır"""
        print("📹 Yayın döngüsü başladı")
        frame_sayisi = 0
        
        while self.is_streaming:
            try:
                # Frame yakala
                frame_data = self.video_frame_yakala()
                
                if frame_data and len(frame_data) > 100:  # Geçerli frame kontrolü
                    # Yer istasyonuna gönder
                    if self.video_frame_gonder(frame_data):
                        frame_sayisi += 1
                        
                        # Her 10 frame'de bir rapor
                        if frame_sayisi % 10 == 0:
                            print(f"🎥 {frame_sayisi} frame gönderildi")
                else:
                    print("⚠️ Geçersiz frame, atlanıyor")
                
                # 1 FPS için bekleme (XBee bandgenişliği için optimize)
                time.sleep(1.0)
                
            except KeyboardInterrupt:
                print("\n⏹️ Kullanıcı tarafından durduruldu")
                break
            except Exception as e:
                print(f"❌ Yayın döngüsü hatası: {e}")
                time.sleep(2)  # Hata durumunda bekle
                
        print("📹 Yayın döngüsü sonlandı")
    
    def canli_yayin_durdur(self):
        """Canlı yayını durdur"""
        if self.is_streaming:
            self.is_streaming = False
            if self.streaming_thread:
                self.streaming_thread.join(timeout=5)
            print("⏹️ Canlı yayın durduruldu")
    
    def kapat(self):
        """Tüm kaynakları kapat"""
        print("🔌 Sistem kapatılıyor...")
        
        # Yayını durdur
        self.canli_yayin_durdur()
        
        # XBee bağlantısını kapat
        if self.xbee_serial:
            try:
                self.xbee_serial.close()
                print("📡 XBee bağlantısı kapatıldı")
            except:
                pass
        
        # Kamerayı kapat
        if self.camera and IS_RASPBERRY_PI:
            try:
                self.camera.close()
                print("🎥 Kamera kapatıldı")
            except:
                pass
        
        print("✅ Sistem temizlendi")

def main():
    """Ana program"""
    print("🚀 SADE KAMERA YAYIN SİSTEMİ")
    print("📺 Amaç: Yer istasyonunda canlı video görmek")
    print("⚡ Özellik: Sade, hızlı, etkili")
    print("-" * 60)
    
    # Kamera yayıncısını oluştur
    yayinci = SadeKameraYayinci()
    
    try:
        # 1. Kamerayı başlat
        if not yayinci.kamera_baslat():
            print("❌ Kamera başlatılamadı, çıkılıyor")
            return
        
        # 2. XBee bağlantısını aç
        if not yayinci.xbee_baglanti_ac():
            print("❌ XBee bağlantısı kurulamadı, çıkılıyor")
            return
        
        # 3. Canlı yayını başlat
        yayinci.canli_yayin_baslat()
        
        print("\n" + "=" * 60)
        print("🎬 CANLI YAYIN AKTİF!")
        print("📺 Yer istasyonu arayüzünde video görmelisin")
        print("⚡ 1 FPS, 240x180, JPEG formatında (kalite-boyut dengesi)")
        print("🔄 Her 10 frame'de rapor verilecek")
        print("⏹️  Ctrl+C ile durdur")
        print("=" * 60)
        
        # Sürekli çalıştır
        while True:
            time.sleep(1)
            
    except KeyboardInterrupt:
        print("\n⏹️ Program durduruldu")
    except Exception as e:
        print(f"\n❌ Beklenmeyen hata: {e}")
    finally:
        yayinci.kapat()
        print("👋 Program sonlandı")

if __name__ == "__main__":
    main()
