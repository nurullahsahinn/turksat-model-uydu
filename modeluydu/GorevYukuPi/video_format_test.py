#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""
Video Format Test
Farklı video formatlarını test eder
"""

import time
import os
import sys

# Modül yolunu ekle
sys.path.append(os.path.dirname(os.path.abspath(__file__)))

try:
    from picamera2 import Picamera2
    from picamera2.encoders import H264Encoder, MJPEGEncoder
    from picamera2.outputs import FileOutput
except ImportError:
    print("❌ Picamera2 import hatası")
    sys.exit(1)

def test_mjpeg_recording():
    """MJPEG formatında video kaydı test et"""
    print("🎥 MJPEG Video Test Başlatılıyor...")
    
    try:
        camera = Picamera2()
        
        # Video konfigürasyonu
        config = camera.create_video_configuration(
            main={"size": (416, 312)},
            controls={"FrameRate": 3}
        )
        camera.configure(config)
        camera.start()
        print("✅ Kamera başlatıldı")
        
        # MJPEG encoder (daha uyumlu)
        encoder = MJPEGEncoder(quality=75)
        output = FileOutput("test_mjpeg.avi")
        
        print("📹 MJPEG kaydı başlatılıyor (10 saniye)...")
        camera.start_recording(encoder, output)
        time.sleep(10)  # 10 saniye kayıt
        camera.stop_recording()
        
        camera.close()
        print("✅ MJPEG test tamamlandı: test_mjpeg.avi")
        
        # Dosya bilgisi
        if os.path.exists("test_mjpeg.avi"):
            size = os.path.getsize("test_mjpeg.avi")
            print(f"📊 Dosya boyutu: {size} bytes")
        
        return True
        
    except Exception as e:
        print(f"❌ MJPEG test hatası: {e}")
        return False

def test_h264_mp4_recording():
    """H.264 MP4 formatında video kaydı test et"""
    print("\n🎥 H.264 MP4 Video Test Başlatılıyor...")
    
    try:
        camera = Picamera2()
        
        # Video konfigürasyonu
        config = camera.create_video_configuration(
            main={"size": (416, 312)},
            controls={"FrameRate": 3}
        )
        camera.configure(config)
        camera.start()
        print("✅ Kamera başlatıldı")
        
        # H.264 encoder - MP4 uyumlu ayarlar
        encoder = H264Encoder(
            bitrate=300000,  # 300 Kbps
            iperiod=30,      # GOP size
            enable_sps_framerate=True,
            profile="main",  # Main profile (daha uyumlu)
            level="3.1"      # Level 3.1 (daha uyumlu)
        )
        output = FileOutput("test_h264.mp4")
        
        print("📹 H.264 MP4 kaydı başlatılıyor (10 saniye)...")
        camera.start_recording(encoder, output)
        time.sleep(10)  # 10 saniye kayıt
        camera.stop_recording()
        
        camera.close()
        print("✅ H.264 MP4 test tamamlandı: test_h264.mp4")
        
        # Dosya bilgisi
        if os.path.exists("test_h264.mp4"):
            size = os.path.getsize("test_h264.mp4")
            print(f"📊 Dosya boyutu: {size} bytes")
        
        return True
        
    except Exception as e:
        print(f"❌ H.264 MP4 test hatası: {e}")
        return False

def main():
    print("🧪 VIDEO FORMAT TEST BAŞLIYOR")
    print("=" * 50)
    
    # 1. MJPEG test
    mjpeg_success = test_mjpeg_recording()
    
    # 2. H.264 MP4 test
    h264_success = test_h264_mp4_recording()
    
    print("\n" + "=" * 50)
    print("📋 TEST SONUÇLARI:")
    print(f"  MJPEG (.avi): {'✅ BAŞARILI' if mjpeg_success else '❌ BAŞARISIZ'}")
    print(f"  H.264 (.mp4): {'✅ BAŞARILI' if h264_success else '❌ BAŞARISIZ'}")
    
    print("\n📥 Test dosyalarını bilgisayarınıza indirin:")
    if mjpeg_success:
        print("  scp atugem@192.168.47.30:/home/atugem/TurkSatModelUydu/GorevYukuPi/test_mjpeg.avi .")
    if h264_success:
        print("  scp atugem@192.168.47.30:/home/atugem/TurkSatModelUydu/GorevYukuPi/test_h264.mp4 .")

if __name__ == "__main__":
    main()