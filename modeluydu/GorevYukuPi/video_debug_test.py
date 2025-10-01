#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""
Video Debug Test - Pi Camera Module 3 NoIR Detaylı Test
"""

import time
import os
import subprocess
from datetime import datetime

def test_basic_picamera2():
    """Temel Picamera2 test"""
    print("🎥 Temel Picamera2 Test...")
    
    try:
        from picamera2 import Picamera2
        from picamera2.encoders import H264Encoder
        from picamera2.outputs import FileOutput
        
        print("✅ Picamera2 kütüphaneleri yüklendi")
        
        # Kamera başlat
        cam = Picamera2()
        print("✅ Kamera nesnesi oluşturuldu")
        
        # Konfigürasyon
        config = cam.create_video_configuration(
            main={"size": (640, 480)},
            controls={"FrameRate": 30}
        )
        cam.configure(config)
        print("✅ Kamera konfigüre edildi")
        
        # Başlat
        cam.start()
        print("✅ Kamera başlatıldı")
        time.sleep(2)
        
        # Video dosyası
        timestamp = datetime.now().strftime("%Y%m%d_%H%M%S")
        video_file = f"./debug_test_{timestamp}.mp4"
        
        # Encoder ve output
        encoder = H264Encoder(bitrate=1000000)  # 1 Mbps - düşük bitrate
        output = FileOutput(video_file)
        
        print(f"📹 Video kaydı başlatılıyor: {video_file}")
        cam.start_recording(encoder, output)
        
        # 10 saniye kayıt
        for i in range(10):
            print(f"📹 Kayıt... {i+1}/10")
            time.sleep(1)
        
        # Kaydı durdur
        cam.stop_recording()
        print("✅ Kayıt durduruldu")
        
        # Kamerayı kapat
        cam.close()
        print("✅ Kamera kapatıldı")
        
        # Dosya kontrolü
        if os.path.exists(video_file):
            size = os.path.getsize(video_file)
            print(f"✅ Video dosyası oluşturuldu: {size} bytes")
            
            # FFprobe ile analiz
            try:
                result = subprocess.run([
                    'ffprobe', '-v', 'quiet', '-print_format', 'json', 
                    '-show_format', '-show_streams', video_file
                ], capture_output=True, text=True)
                
                if result.returncode == 0:
                    print("✅ Video dosyası geçerli (FFprobe test başarılı)")
                else:
                    print("❌ Video dosyası bozuk (FFprobe test başarısız)")
            except FileNotFoundError:
                print("⚠️ FFprobe bulunamadı, manuel test gerekli")
                
            return video_file
        else:
            print("❌ Video dosyası oluşturulamadı")
            return None
            
    except Exception as e:
        print(f"❌ Test hatası: {e}")
        return None

def test_video_players():
    """Video oynatıcı testi"""
    print("\n🎮 Video Oynatıcı Testi...")
    
    players = [
        ("VLC", "vlc"),
        ("MPV", "mpv"), 
        ("FFplay", "ffplay")
    ]
    
    for name, cmd in players:
        try:
            subprocess.run([cmd, "--version"], capture_output=True, timeout=5)
            print(f"✅ {name} kurulu")
        except (FileNotFoundError, subprocess.TimeoutExpired):
            print(f"❌ {name} bulunamadı")

def main():
    print("🚀 Video Debug Test Başlatılıyor...")
    print("=" * 60)
    
    # Test 1: Video oynatıcıları
    test_video_players()
    
    # Test 2: Temel kamera testi
    print("\n" + "=" * 60)
    video_file = test_basic_picamera2()
    
    if video_file:
        print(f"\n📁 Test video dosyası: {video_file}")
        print("💡 Bu dosyayı bilgisayara kopyalayıp test edin:")
        print(f"   scp atugem@192.168.199.30:/home/atugem/TurkSatModelUydu/GorevYukuPi/{video_file} ./")
    
    print("\n" + "=" * 60)
    print("🎥 Video Debug Test Tamamlandı")

if __name__ == "__main__":
    main()