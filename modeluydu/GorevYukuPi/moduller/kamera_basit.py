#!/usr/bin/env python3
"""
BASIT KAMERA MODÜLÜ - Video kayıt sorunlarını çözmek için tamamen yeniden yazıldı
3 saniye sorunu için köklü çözüm
"""
import os
import time
import threading
from datetime import datetime
from moduller.yapilandirma import IS_RASPBERRY_PI

try:
    if IS_RASPBERRY_PI:
        from picamera2 import Picamera2
        from picamera2.encoders import H264Encoder
        from picamera2.outputs import FfmpegOutput
except ImportError:
    print("UYARI: Picamera2 kütüphanesi bulunamadı. Kamera simülasyon modunda çalışacak.")

class BasitKameraYoneticisi:
    def __init__(self, simulate=not IS_RASPBERRY_PI):
        self.simulate = simulate
        self.camera = None
        self.is_recording = False
        self.current_file = None
        self.encoder = None
        self.output = None
        
        # Canlı yayın için
        self.is_streaming = False
        self.streaming_thread = None
        self.gonder_callback = None
        
        print(f"🎥 Basit Kamera Yöneticisi başlatılıyor (simulate={simulate})")
        
        if not self.simulate:
            self._init_camera()
    
    def _init_camera(self):
        """Kamerayı başlat - BASİT VERSİYON"""
        try:
            print("🎥 Kamera başlatılıyor...")
            self.camera = Picamera2()
            
            # BASİT KONFİGÜRASYON - XBee için optimize (HIZLANDIRILMIŞ)
            config = self.camera.create_video_configuration(
                main={"size": (240, 180), "format": "RGB888"},  # 🔧 FIX: Optimum çözünürlük 
                controls={"FrameRate": 3}  # 🔧 FIX: 3 FPS (5'ten düşürüldü - performans için)
            )
            
            self.camera.configure(config)
            self.camera.start()
            
            print("✅ Kamera başarıyla başlatıldı")
            time.sleep(2)  # Kamera ısınsın
            
        except Exception as e:
            print(f"❌ Kamera başlatılamadı: {e}")
            self.camera = None
            self.simulate = True
    
    def baslat_kayit(self, dosya_yolu):
        """Video kaydını başlat - BASİT VERSİYON"""
        if self.is_recording:
            print("⚠️ Zaten kayıt yapılıyor")
            return False
            
        if self.simulate:
            print(f"SİMÜLASYON: Video kaydı başlatıldı: {dosya_yolu}")
            self.is_recording = True
            self.current_file = dosya_yolu
            return True
        
        if not self.camera:
            print("❌ Kamera yok, kayıt başlatılamıyor")
            return False
            
        try:
            # FFMPEG OUTPUT - VLC/TÜMÜ UYUMLU MP4
            self.encoder = H264Encoder(bitrate=2000000)
            self.output = FfmpegOutput(dosya_yolu, audio=False)
            
            print(f"🎥 Video kaydı başlatılıyor: {dosya_yolu}")
            
            # FFMPEG İLE KAYIT BAŞLATMA
            self.camera.start_recording(self.encoder, self.output)
            
            self.is_recording = True
            self.current_file = dosya_yolu
            
            print(f"✅ Video kaydı başlatıldı: {dosya_yolu}")
            print("📹 Video sürekli kayıt modunda (FFMPEG/MP4 - VLC uyumlu)")
            
            return True
            
        except Exception as e:
            print(f"❌ Video kayıt başlatma hatası: {e}")
            self.is_recording = False
            return False
    
    def durdur_kayit(self):
        """Video kaydını durdur - BASİT VERSİYON"""
        if not self.is_recording:
            print("🔍 Video zaten kayıt yapmıyor")
            return
            
        if self.simulate:
            print("SİMÜLASYON: Video kaydı durduruldu")
            self.is_recording = False
            self.current_file = None
            return
        
        if self.camera and self.encoder:
            try:
                print("🛑 Video kaydı durduruluyor...")
                
                # DOĞRU SIRALAMA: Encoder → Output → Cleanup
                self.camera.stop_encoder(self.encoder)
                
                # FFMPEG output'u düzgün kapat
                if self.output:
                    time.sleep(0.5)  # Encoder temizlensin
                    self.output.stop()
                    print("📹 FFMPEG output kapatıldı")
                
                print("✅ Video kaydı durduruldu (VLC uyumlu)")
                
            except Exception as e:
                print(f"HATA: Video durdurma hatası: {e}")
        
        # Cleanup
        self.is_recording = False
        self.current_file = None
        self.encoder = None
        self.output = None
    
    def kapat(self):
        """Kamerayı kapat"""
        if self.is_recording:
            self.durdur_kayit()
            
        if self.is_streaming:
            self.durdur_canli_yayin()
            
        if self.camera and not self.simulate:
            try:
                self.camera.close()
                print("✅ Kamera kapatıldı")
            except:
                pass
        
        self.camera = None
    
    def baslat_canli_yayin(self, gonder_callback):
        """Canlı yayın başlat - YER İSTASYONU İÇİN"""
        if self.simulate:
            print("📹 Canlı yayın simülasyonu (basit kamera)")
            return
            
        if not self.camera:
            print("❌ Kamera yok, canlı yayın başlatılamıyor")
            return
            
        try:
            print("📹 Canlı yayın başlatılıyor (yer istasyonu)")
            self.gonder_callback = gonder_callback
            
            # Canlı yayın thread'i başlat
            self.streaming_thread = threading.Thread(target=self._canli_yayin_loop, daemon=True)
            self.is_streaming = True
            self.streaming_thread.start()
            
            print("✅ Canlı yayın başlatıldı")
            
        except Exception as e:
            print(f"❌ Canlı yayın başlatma hatası: {e}")
            
    def _canli_yayin_loop(self):
        """Canlı yayın döngüsü - YER İSTASYONU İÇİN OPTİMİZE EDİLDİ"""
        print("📹 Canlı yayın döngüsü başladı (Optimize Edilmiş)")
        
        try:
            while self.is_streaming and self.camera:
                try:
                    # Frame yakala (Optimize edilmiş)
                    from io import BytesIO
                    
                    stream = BytesIO()
                    # JPEG formatı ve düşük kalite, 57600 baud için optimize
                    self.camera.capture_file(stream, format='jpeg')
                    stream.seek(0)
                    frame_data = stream.getvalue()
                    stream.close()
                    
                    # Frame boyutu çok büyükse PIL ile kaliteyi daha da düşür (5KB sınırı - hızlandırılmış)
                    if len(frame_data) > 5120:
                        try:
                            from PIL import Image
                            import io
                            
                            img = Image.open(io.BytesIO(frame_data))
                            output_stream = BytesIO()
                            # Kaliteyi düşürerek boyutu küçült (hızlandırılmış ayar)
                            img.save(output_stream, format='JPEG', quality=18, optimize=True)
                            frame_data = output_stream.getvalue()
                            output_stream.close()
                        except ImportError:
                            print("⚠️ PIL kütüphanesi bulunamadı, frame sıkıştırılamıyor!")

                    # Sadece geçerli ve boyutu uygun frame'leri gönder (hızlandırılmış limit)
                    if self.gonder_callback and len(frame_data) > 100 and len(frame_data) <= 5120:
                        # YER İSTASYONU UYUMLU FORMAT: #VIDEO:base64_data#
                        import base64
                        base64_data = base64.b64encode(frame_data).decode('utf-8')
                        video_string = f"#VIDEO:{base64_data}#\n"
                        
                        # Callback ile string formatını gönder
                        self.gonder_callback(video_string.encode('utf-8'))
                        print(f"📡 Video frame gönderildi ({len(frame_data)} bytes)")
                    elif len(frame_data) > 5120:
                        print(f"⚠️ Frame çok büyük ({len(frame_data)} bytes), atlanıyor (>5KB)")

                    # FPS kontrolü (XBee için hızlandırılmış)
                    time.sleep(0.25)  # 4 FPS'e denk gelir (1/4) - HIZLANDIRILDI
                    
                except Exception as e:
                    print(f"⚠️ Frame yakalama/gönderme hatası: {e}")
                    time.sleep(1) # Hata durumunda bekle
                    
        except Exception as e:
            print(f"❌ Canlı yayın döngüsü ana hatası: {e}")
        finally:
            print("📹 Canlı yayın döngüsü sonlandı")
    
    def durdur_canli_yayin(self):
        """Canlı yayın durdur - BASİT VERSİYON"""
        if hasattr(self, 'is_streaming'):
            self.is_streaming = False
            
        if hasattr(self, 'streaming_thread') and self.streaming_thread:
            try:
                self.streaming_thread.join(timeout=3)
            except:
                pass
                
        print("📹 Canlı yayın durduruldu (basit kamera)")
    
    def __del__(self):
        """Destructor - BASİT VERSİYON"""
        # HİÇBİR ŞEY YAPMA - Auto cleanup devre dışı
        pass

# Test fonksiyonu
if __name__ == "__main__":
    print("🎥 Basit Kamera Test")
    
    kamera = BasitKameraYoneticisi()
    
    if kamera.baslat_kayit("test_video.mp4"):
        print("⏰ 10 saniye kayıt yapılıyor...")
        time.sleep(10)
        kamera.durdur_kayit()
    
    kamera.kapat()
    print("✅ Test tamamlandı")