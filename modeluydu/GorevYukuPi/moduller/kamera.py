# -*- coding: utf-8 -*-
"""
Kamera Yönetici Modülü

Bu modül, Raspberry Pi kamerasını yönetmekten, SD karta video kaydetmekten ve
canlı video akışını seri port üzerinden yer istasyonuna göndermekten sorumludur.
"""

import time
import threading
import base64
import subprocess
import os
from io import BytesIO

# Proje modüllerinden sabitleri içeri aktar
from moduller.yapilandirma import (
    IS_RASPBERRY_PI, 
    VIDEO_FRAME_BASLANGIC, VIDEO_FRAME_BITIS,
    VIDEO_MAX_FRAME_SIZE_KB
)

# Raspberry Pi üzerinde çalışırken gerçek kütüphaneleri kullanmayı dene
try:
    from picamera2 import Picamera2
    from picamera2.encoders import JpegEncoder
    from picamera2.outputs import FileOutput
except ImportError:
    print("UYARI: Picamera2 kütüphanesi bulunamadı. Kamera simülasyon modunda çalışacak.")


class KameraYoneticisi:
    def __init__(self, simulate=not IS_RASPBERRY_PI):
        # Temel başlatma
        self.simulate = simulate
        self.camera = None
        self.is_recording = False
        self.is_streaming = False
        self.streaming_thread = None
        self.current_encoder = None
        self.current_output = None
        
        # Memory leak önlemi: BytesIO buffer'ı yeniden kullan (Analiz10)
        self.stream_buffer = BytesIO()
        self._buffer_lock = threading.Lock()
        
        # Thread safety için RLock (Analiz10 düzeltmesi)
        self._streaming_lock = threading.RLock()
        self._cleanup_flag = False
        
        # Video streaming callback
        self.gonder_callback = None

        if not self.simulate:
            self._setup_camera()
        else:
            print("Kamera simülasyon modunda başlatıldı.")

    def _setup_camera(self):
        """Raspberry Pi kamerasını başlatır ve yapılandırır."""
        # Önce kamera process'lerini temizle
        self._cleanup_camera_processes()
        
        max_attempts = 3
        for attempt in range(max_attempts):
            try:
                print(f"🎥 Kamera başlatma denemesi {attempt + 1}/{max_attempts}")
                
                if attempt > 0:
                    print(f"  ⏱️ {attempt * 2} saniye bekleniyor...")
                    time.sleep(attempt * 2)
                
                self.camera = Picamera2()
                # 🔧 RASPBERRY PI 2W OPTİMİZASYONU
                # Pi 2W: ARM Cortex-A7 900MHz, 512MB RAM - düşük performans
                config = self.camera.create_video_configuration(
                    main={"size": (640, 480)},     # SD kart için tam kalite
                    lores={"size": (320, 240)},    # XBee canlı yayın için düşük çözünürlük
                    controls={
                        "FrameRate": 15,           # Pi 2W için optimize edilmiş FPS
                        "ExposureTime": 33333,     # 1/30s exposure (iyi ışık dengesi)
                        "AnalogueGain": 1.0,       # Düşük analog gain (az gürültü)
                        "Brightness": 0.0,         # Normal parlaklık
                        "Contrast": 1.0            # Normal kontrast
                    }
                )
                self.camera.configure(config)
                self.camera.start()
                print("✅ Raspberry Pi 2W kamera başlatıldı (640x480 SD / 320x240 XBee @ 15FPS)")
                time.sleep(2) # Kameranın ısınması için bekle
                return  # Başarılı, çık
                
            except Exception as e:
                print(f"❌ Kamera başlatma denemesi {attempt + 1} başarısız: {e}")
                
                # Kamera nesnesini temizle
                if hasattr(self, 'camera') and self.camera:
                    try:
                        self.camera.close()
                    except:
                        pass
                    self.camera = None
                
                # Son deneme değilse, daha agresif temizlik yap
                if attempt < max_attempts - 1:
                    print("  🔧 Agresif kamera temizliği yapılıyor...")
                    self._aggressive_camera_cleanup()
        
        # Tüm denemeler başarısız
        print("❌ Tüm kamera başlatma denemeleri başarısız, simülasyona geçiliyor")
        self.camera = None
        self.simulate = True

    def _cleanup_camera_processes(self):
        """Kamerayı kullanan process'leri temizler."""
        try:
            print("🔧 Kamera process'leri kontrol ediliyor...")
            
            # Kamerayı kullanan process'leri bul ve öldür (ANA PROGRAM HARİÇ!)
            camera_patterns = ['libcamera-', 'rpicam-', 'picamera2']
            
            for pattern in camera_patterns:
                try:
                    # pkill ile pattern'e uyan process'leri öldür
                    result = subprocess.run(['pkill', '-f', pattern], 
                                          capture_output=True, text=True, check=False)
                    if result.returncode == 0:
                        print(f"  ✅ {pattern} process'leri temizlendi")
                except Exception as e:
                    print(f"  ⚠️ {pattern} temizleme hatası: {e}")
            
            # lsof ile device'ları kontrol et ve temizle
            for device in ['/dev/video0', '/dev/media0']:
                try:
                    result = subprocess.run(['lsof', device], 
                                          capture_output=True, text=True, check=False)
                    if result.returncode == 0:
                        # Device kullanılıyor, kullanıcıları öldür
                        lines = result.stdout.strip().split('\n')[1:]  # Header'ı atla
                        for line in lines:
                            if line.strip():
                                try:
                                    pid = line.split()[1]
                                    subprocess.run(['kill', '-9', pid], check=False)
                                    print(f"  🔧 Device {device} kullanıcısı PID {pid} öldürüldü")
                                except:
                                    pass
                except:
                    pass
                    
            print("✅ Process temizleme tamamlandı")
            time.sleep(1)  # Process'lerin tamamen kapanması için bekle
            
        except Exception as e:
            print(f"⚠️ Process temizleme hatası: {e}")

    def _aggressive_camera_cleanup(self):
        """Agresif kamera temizleme işlemi."""
        try:
            print("  🚨 Agresif kamera temizliği başlatılıyor...")
            
            # Tüm kamera process'lerini force kill
            subprocess.run(['pkill', '-9', '-f', 'libcamera'], check=False)
            subprocess.run(['pkill', '-9', '-f', 'rpicam'], check=False)
            subprocess.run(['pkill', '-9', '-f', 'picamera'], check=False)
            
            # V4L2 modülünü yeniden yükle (sudo gerekebilir)
            try:
                subprocess.run(['sudo', '-n', 'modprobe', '-r', 'bcm2835_v4l2'], 
                             check=False, capture_output=True, timeout=5)
                time.sleep(0.5)
                subprocess.run(['sudo', '-n', 'modprobe', 'bcm2835_v4l2'], 
                             check=False, capture_output=True, timeout=5)
                print("  ✅ V4L2 modülü yeniden yüklendi")
            except:
                print("  ⚠️ V4L2 modülü yeniden yüklenemedi (sudo yetkisi gerekiyor)")
            
            time.sleep(2)
            print("  ✅ Agresif temizlik tamamlandı")
            
        except Exception as e:
            print(f"  ⚠️ Agresif temizlik hatası: {e}")

    def baslat_kayit(self, dosya_yolu, high_quality=True):
        """
        Videoyu belirtilen dosyaya kaydetmeye başlar.
        :param dosya_yolu: Videonun kaydedileceği tam dosya yolu.
        :param high_quality: True ise tam kalite SD kart kaydı, False ise XBee canlı yayın
        """
        if self.is_recording:
            print("UYARI: Zaten bir kayıt devam ediyor.")
            return

        if self.simulate:
            print(f"SİMÜLASYON: Video kaydı başlatıldı -> {dosya_yolu}")
            self.is_recording = True
            return

        if self.camera:
            try:
                # 🔧 DÜZELTME: H.264/MP4 format (tüm oynatıcılara uyumlu)
                from picamera2.encoders import H264Encoder, MJPEGEncoder
                from picamera2.outputs import FileOutput
                
                if high_quality:
                    # 🎥 TAM KALİTE SD KART KAYDI - H.264/MP4 (Analiz10 düzeltmesi)
                    encoder = H264Encoder(
                        bitrate=2000000,    # 2 Mbps Pi 2W optimize
                        iperiod=30          # I-frame interval
                    )
                    
                    # MP4 dosya uzantısını zorla
                    if not dosya_yolu.endswith('.mp4'):
                        dosya_yolu = dosya_yolu.replace('.avi', '.mp4')
                        if not dosya_yolu.endswith('.mp4'):
                            dosya_yolu += '.mp4'
                    
                    print(f"✅ TAM KALİTE modu: H.264/MP4 2Mbps (Analiz10 optimized)")
                else:
                    # 🎥 XBee CANLI YAYIN - MJPEG/AVI (düşük bant genişliği)
                    encoder = MJPEGEncoder(quality=30)  # Düşük kalite
                    
                    # AVI dosya uzantısını zorla
                    if dosya_yolu.endswith('.mp4'):
                        dosya_yolu = dosya_yolu.replace('.mp4', '.avi')
                    
                    print(f"✅ XBee CANLI YAYIN modu: MJPEG/AVI düşük kalite")
                
                # FileOutput oluştur ve encoder'a bağla
                output = FileOutput(dosya_yolu)
                
                # Current encoder ve output'u sakla (stop için gerekli)  
                self.current_encoder = encoder
                self.current_output = output
                
                # 🔧 BASİT API: start_recording
                print(f"🎥 Video başlatılıyor: {dosya_yolu}")
                self.camera.start_recording(encoder, output)
                self.is_recording = True
                print(f"✅ Video kaydı başlatıldı: {dosya_yolu}")
                
                # 🔧 UZUN SÜRE KAYIT GARANTİSİ
                print(f"📹 Video sürekli kayıt modunda - manuel durdurulana kadar devam edecek")
            except ImportError as ie:
                print(f"⚠️ Picamera2 encoder import hatası: {ie}, simülasyon moduna geçiliyor")
                self.simulate = True
                self.baslat_kayit(dosya_yolu, high_quality)  # Simülasyon modu ile tekrar dene
            except Exception as e:
                print(f"❌ HATA: Kayıt başlatılamadı: {e}")

    def baslat_canli_yayin(self, gonder_callback):
        """Canlı video yayınını başlatır."""
        with self._streaming_lock:
            if self.is_streaming:
                print("UYARI: Canlı yayın zaten aktif.")
                return
            
            self.is_streaming = True
            self.gonder_callback = gonder_callback
            
            # Thread başlat
            self.streaming_thread = threading.Thread(target=self._stream_loop, daemon=True)
            self.streaming_thread.start()
            print("✅ Canlı video yayını başlatıldı")

    def durdur_canli_yayin(self):
        """Canlı video yayınını durdurur."""
        with self._streaming_lock:
            if not self.is_streaming:
                return
                
            self.is_streaming = False
        
        # Thread'in bitmesini bekle
        if self.streaming_thread and self.streaming_thread.is_alive():
            # 🔧 Thread timeout artırıldı (Analiz4.txt düzeltmesi)
            self.streaming_thread.join(timeout=8)  # 2s → 8s
        
        print("✅ Canlı video yayını durduruldu")

    def durdur_kayit(self):
        """Video kaydını durdurur (Analiz10 thread-safe düzeltme)."""
        with self._streaming_lock:  # Thread-safe
            if not self.is_recording:
                print("🔍 Video zaten kayıt yapmiyor")
                return
                
            if self.simulate:
                print("SİMÜLASYON: Video kaydı durduruldu")
                self.is_recording = False
                return
                
            # Mevcut video dosya yolunu sakla (MP4 dönüştürme için)
            current_video_path = None
            if hasattr(self, 'current_output') and self.current_output:
                if hasattr(self.current_output, 'file') and hasattr(self.current_output.file, 'name'):
                    current_video_path = self.current_output.file.name
                
            if self.camera and hasattr(self, 'current_encoder') and self.current_encoder:
                try:
                    print("🛑 Video kaydı durduruluyor...")
                    # ✅ DOĞRU API: stop_encoder() kullan
                    self.camera.stop_encoder(self.current_encoder)
                    print("✅ Video kaydı durduruldu")
                    
                    # 🔧 OTOMATIK MP4 DÖNÜŞTÜRMESİ (Windows uyumluluğu için)
                    if current_video_path and current_video_path.endswith('.mp4'):
                        self._convert_to_proper_mp4(current_video_path)
                        
                except Exception as e:
                    print(f"HATA: Video kaydı durdurulamadı: {e}")
            
            # Cleanup flag set et
            self._cleanup_flag = True
            self.is_recording = False
            self.current_encoder = None
            self.current_output = None

    def _convert_to_proper_mp4(self, raw_video_path):
        """Raw H.264 dosyasını proper MP4 container'a çevirir (Windows uyumluluğu için)"""
        try:
            import subprocess
            import os
            
            print(f"🔄 MP4 dönüştürme başlatılıyor: {raw_video_path}")
            
            # Yedek dosya adı
            backup_path = raw_video_path + ".raw_backup"
            fixed_path = raw_video_path.replace('.mp4', '_fixed.mp4')
            
            # Orijinal dosyayı yedekle
            os.rename(raw_video_path, backup_path)
            
            # FFmpeg ile proper MP4'e çevir
            cmd = [
                'ffmpeg', '-y',  # -y: overwrite without asking
                '-i', backup_path,  # input: raw H.264 file
                '-c', 'copy',       # copy codec (no re-encoding)
                '-f', 'mp4',        # force MP4 container
                '-movflags', '+faststart',  # optimize for streaming
                raw_video_path      # output: proper MP4 file
            ]
            
            result = subprocess.run(cmd, capture_output=True, text=True, timeout=30)
            
            if result.returncode == 0:
                # Başarılı - yedek dosyayı sil
                os.remove(backup_path)
                print(f"✅ MP4 dönüştürme başarılı: {raw_video_path}")
                print("   🎥 Video artık Windows'ta oynatılabilir")
            else:
                # Başarısız - orijinal dosyayı geri yükle
                os.rename(backup_path, raw_video_path)
                print(f"⚠️ MP4 dönüştürme başarısız: {result.stderr}")
                print("   📁 Orijinal raw dosya korundu")
                
        except subprocess.TimeoutExpired:
            print("⚠️ MP4 dönüştürme timeout - dosya çok büyük olabilir")
        except FileNotFoundError:
            print("⚠️ FFmpeg bulunamadı - MP4 dönüştürme atlandı")
        except Exception as e:
            print(f"⚠️ MP4 dönüştürme hatası: {e}")

    def _stream_loop(self):
        """Video akışını sürekli gönderen loop (thread içinde çalışır)."""
        print("📹 Video streaming loop başlatıldı")
        
        while self.is_streaming:
            try:
                if self.simulate:
                    # Simülasyon modu: sahte frame gönder
                    time.sleep(0.33)  # 3 FPS
                    sahte_frame = b"FAKE_JPEG_DATA_FOR_SIMULATION"
                    paket = VIDEO_FRAME_BASLANGIC + sahte_frame + VIDEO_FRAME_BITIS
                    if self.gonder_callback:
                        self.gonder_callback(paket)
                    continue
                
                if not self.camera:
                    print("HATA: Kamera başlatılmamış")
                    break
                
                # 🔧 XBee BANDWIDTH OPTİMİZESİ: Düşük çözünürlük + düşük kalite
                try:
                    # Memory-safe frame capture: BytesIO buffer'ı yeniden kullan
                    self.stream_buffer.seek(0)
                    self.stream_buffer.truncate(0)
                    
                    # 🎯 XBee için optimize edilmiş frame yakalama
                    # lores stream kullan (320x240) - quality parametresi API'de yok
                    try:
                        self.camera.capture_file(
                            self.stream_buffer, 
                            format='jpeg',
                            name='lores'      # Düşük çözünürlük stream kullan
                            # quality parametresi kaldırıldı (API uyumsuzluğu)
                        )
                    except Exception as capture_error:
                        # capture_file başarısız olursa array metodunu dene
                        import numpy as np
                        from PIL import Image
                        
                        array = self.camera.capture_array("lores")
                        if array is not None:
                            # Array boyutlarını kontrol et
                            if array.ndim == 3 and array.shape[2] >= 3:  # RGB format
                                img = Image.fromarray(array[:, :, :3], 'RGB')
                            elif array.ndim == 3:  # YUV veya diğer format
                                img = Image.fromarray(array[:, :, 0], 'L')  # Sadece Y kanalı
                            elif array.ndim == 2:  # Grayscale
                                img = Image.fromarray(array, 'L')
                            else:
                                raise capture_error  # Desteklenmeyen format
                            
                            # BytesIO buffer'a kaydet
                            self.stream_buffer.seek(0)
                            self.stream_buffer.truncate()
                            img.save(self.stream_buffer, format='JPEG', quality=25)
                        else:
                            raise capture_error
                    
                    self.stream_buffer.seek(0)
                    frame_data = self.stream_buffer.getvalue()
                    
                    # Frame boyutu kontrolü (XBee için maksimum ~8KB)
                    if frame_data and len(frame_data) <= 8192:  # 8KB limit
                        paket = VIDEO_FRAME_BASLANGIC + frame_data + VIDEO_FRAME_BITIS
                        if self.gonder_callback:
                            self.gonder_callback(paket)
                    elif len(frame_data) > 8192:
                        print(f"⚠️ Frame çok büyük ({len(frame_data)} bytes), atlanıyor")
                
                except Exception as buffer_error:
                    print(f"HATA: Buffer işlemi başarısız: {buffer_error}")
                    # Buffer'ı güvenli şekilde reset et
                    try:
                        self.stream_buffer.seek(0)
                        self.stream_buffer.truncate(0)
                    except:
                        pass  # Buffer corrupted olabilir, ignore
                    
                    # Alternatif capture yöntemi dene
                    try:
                        import numpy as np
                        from PIL import Image
                        
                        # Array olarak yakala
                        array = self.camera.capture_array("lores")
                        if array is not None:
                            # Array'i PIL Image'e çevir
                            if array.ndim == 3:  # 3D array (RGB/YUV)
                                if array.shape[2] == 3:  # RGB
                                    img = Image.fromarray(array, 'RGB')
                                else:  # YUV - sadece Y kanalını kullan
                                    img = Image.fromarray(array[:,:,0], 'L')
                            else:  # 2D array (grayscale)
                                img = Image.fromarray(array, 'L')
                            
                            # JPEG olarak kaydet (hızlandırılmış kalite)
                            self.stream_buffer.seek(0)
                            self.stream_buffer.truncate(0)
                            img.save(self.stream_buffer, format='JPEG', quality=18)
                            frame_data = self.stream_buffer.getvalue()
                            
                            # Frame gönder
                            if frame_data and len(frame_data) <= 8192:
                                paket = VIDEO_FRAME_BASLANGIC + frame_data + VIDEO_FRAME_BITIS
                                if self.gonder_callback:
                                    self.gonder_callback(paket)
                            
                    except Exception as alt_error:
                        print(f"HATA: Alternatif capture da başarısız: {alt_error}")
                        # Son çare: sahte frame gönder
                        sahte_frame = self._create_dummy_frame()
                        paket = VIDEO_FRAME_BASLANGIC + sahte_frame + VIDEO_FRAME_BITIS
                        if self.gonder_callback:
                            self.gonder_callback(paket)
                
                time.sleep(0.25)  # 4 FPS timing (XBee hızlandırılmış)
                
            except Exception as e:
                print(f"HATA: Frame yakalanamadı: {e}")
                time.sleep(0.33)  # Hata durumunda da 3 FPS timing
                continue
        
        print("📹 Video streaming loop sonlandırıldı")

    def _create_dummy_frame(self):
        """Simülasyon için sahte bir JPEG byte dizisi oluşturur."""
        # Bu, gerçek bir 1x1 piksel JPEG'in byte dizisidir.
        # Test için kullanılır, gerçek bir görüntü değildir.
        return b'\xff\xd8\xff\xe0\x00\x10JFIF\x00\x01\x01\x00\x00\x01\x00\x01\x00\x00\xff\xdb\x00\x43\x00\x01\x01\x01\x01\x01\x01\x01\x01\x01\x01\x01\x01\x01\x01\x01\x01\x01\x01\x01\x01\x01\x01\x01\x01\x01\x01\x01\x01\x01\x01\x01\x01\x01\x01\x01\x01\x01\x01\x01\x01\x01\x01\x01\x01\x01\x01\x01\x01\x01\x01\x01\x01\x01\x01\x01\x01\x01\x01\x01\x01\xff\xc0\x00\x0b\x08\x00\x01\x00\x01\x01\x01\x11\x00\xff\xc4\x00\x14\x00\x01\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\xff\xda\x00\x08\x01\x01\x00\x00\x00\x00\x01\xff\xd9'

    def temizle(self):
        """Kamera kaynaklarını temizler ve serbest bırakır."""
        print("🧹 Kamera kaynakları temizleniyor...")
        
        # Video streaming'i durdur
        self.durdur_canli_yayin()
        
        # Video kaydını durdur
        self.durdur_kayit()
        
        # BytesIO buffer'ı kapat
        if hasattr(self, 'stream_buffer') and self.stream_buffer:
            self.stream_buffer.close()
        
        # Kamerayı kapat
        if self.camera and not self.simulate:
            try:
                self.camera.close()
                print("✅ Kamera kapatıldı")
            except Exception as e:
                print(f"UYARI: Kamera kapatılırken hata: {e}")
        
        self.camera = None
        print("✅ Kamera kaynakları temizlendi")

    def __del__(self):
        """Destructor: TAMAMEN DEVRE DIŞI (Analiz10 çözümü)"""
        # __del__ metodunu tamamen devre dışı bırak
        # Video recording asla otomatik durmayacak
        pass

if __name__ == '__main__':
    # Modülün tek başına test edilmesi için örnek kod
    print("Kamera Yönetici Modülü Testi")
    
    # Test callback fonksiyonu (gelen veriyi ekrana basar)
    def test_gonder(data):
        print(f"--> Frame gönderildi ({len(data)} bytes)")

    kamera_yonetici = KameraYoneticisi(simulate=True)
    
    # Canlı yayın testi
    kamera_yonetici.baslat_canli_yayin(test_gonder)
    time.sleep(1) # 1 saniye yayın yap
    kamera_yonetici.durdur_canli_yayin()

    # Kayıt testi
    kamera_yonetici.baslat_kayit("test_video.mjpeg")
    time.sleep(2) # 2 saniye kayıt yap
    kamera_yonetici.durdur_kayit()

    kamera_yonetici.temizle()
    print("\nTest tamamlandı.")
