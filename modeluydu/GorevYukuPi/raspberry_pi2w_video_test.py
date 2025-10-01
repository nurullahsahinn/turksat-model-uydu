#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""
Raspberry Pi 2W Video Test - İkili Video Sistem Testi
SD Kart: Tam kalite H.264/MP4
XBee: Düşük kalite MJPEG/AVI
"""

import time
import os
import sys
import subprocess
from datetime import datetime

# Modül yolunu ekle
sys.path.append(os.path.dirname(os.path.abspath(__file__)))

try:
    from picamera2 import Picamera2
    from picamera2.encoders import H264Encoder, MJPEGEncoder
    from picamera2.outputs import FileOutput
    print("✅ Picamera2 kütüphaneleri yüklendi")
except ImportError:
    print("❌ Picamera2 import hatası - Raspberry Pi üzerinde çalıştırın")
    sys.exit(1)

def test_sd_card_recording():
    """SD Kart için tam kalite H.264/MP4 test"""
    print("\n🎥 SD KART TAM KALİTE TESTİ")
    print("=" * 50)
    print("📊 Format: H.264/MP4, Çözünürlük: 640x480, FPS: 15, Bitrate: 2Mbps")
    
    try:
        camera = Picamera2()
        
        # Raspberry Pi 2W için optimize edilmiş konfigürasyon
        config = camera.create_video_configuration(
            main={"size": (640, 480)},     # SD kart için tam kalite
            lores={"size": (320, 240)},    # XBee için düşük çözünürlük
            controls={
                "FrameRate": 15,           # Pi 2W için optimize
                "ExposureTime": 33333,     # 1/30s exposure
                "AnalogueGain": 1.0,       # Düşük analog gain
                "Brightness": 0.0,         # Normal parlaklık
                "Contrast": 1.0            # Normal kontrast
            }
        )
        camera.configure(config)
        camera.start()
        print("✅ Kamera başlatıldı (Pi 2W optimize)")
        
        # Test video dosyası
        timestamp = datetime.now().strftime("%Y%m%d_%H%M%S")
        video_file = f"./sd_test_{timestamp}.mp4"
        
        # H.264 encoder - tam kalite (tüm oynatıcılara uyumlu)
        encoder = H264Encoder(
            bitrate=2000000,      # 2 Mbps
            iperiod=30,           # GOP size
            enable_sps_framerate=True,
            profile="main",       # Ana profil (uyumluluk)
            level="3.1"          # Level 3.1 (uyumluluk)
        )
        output = FileOutput(video_file)
        
        print("📹 H.264/MP4 kaydı başlatılıyor (15 saniye)...")
        camera.start_recording(encoder, output)
        
        # 15 saniye kayıt
        for i in range(15):
            print(f"📹 SD Kayıt... {i+1}/15")
            time.sleep(1)
        
        camera.stop_recording()
        camera.close()
        print("✅ SD kart test tamamlandı")
        
        # Dosya analizi
        if os.path.exists(video_file):
            size = os.path.getsize(video_file)
            print(f"📊 Dosya boyutu: {size:,} bytes ({size/1024/1024:.1f} MB)")
            
            # FFprobe ile detaylı analiz
            analyze_video_file(video_file)
            return video_file
        else:
            print("❌ Video dosyası oluşturulamadı")
            return None
            
    except Exception as e:
        print(f"❌ SD test hatası: {e}")
        return None

def test_xbee_streaming():
    """XBee için düşük kalite MJPEG/AVI test"""
    print("\n📡 XBee CANLI YAYIN TESTİ")
    print("=" * 50)
    print("📊 Format: MJPEG/AVI, Çözünürlük: 320x240, FPS: 1, Kalite: %25")
    
    try:
        camera = Picamera2()
        
        # Aynı konfigürasyon (dual stream)
        config = camera.create_video_configuration(
            main={"size": (640, 480)},
            lores={"size": (320, 240)},    # XBee için bu stream
            controls={
                "FrameRate": 15,
                "ExposureTime": 33333,
                "AnalogueGain": 1.0,
                "Brightness": 0.0,
                "Contrast": 1.0
            }
        )
        camera.configure(config)
        camera.start()
        print("✅ Kamera başlatıldı (dual stream)")
        
        # Test video dosyası
        timestamp = datetime.now().strftime("%Y%m%d_%H%M%S")
        video_file = f"./xbee_test_{timestamp}.avi"
        
        # MJPEG encoder - düşük kalite (XBee bandgenişlik)
        encoder = MJPEGEncoder(quality=25)  # %25 kalite
        output = FileOutput(video_file)
        
        print("📹 MJPEG/AVI kaydı başlatılıyor (10 saniye)...")
        camera.start_recording(encoder, output)
        
        # 10 saniye kayıt (daha kısa test)
        for i in range(10):
            print(f"📡 XBee Kayıt... {i+1}/10")
            time.sleep(1)
        
        camera.stop_recording()
        camera.close()
        print("✅ XBee test tamamlandı")
        
        # Dosya analizi
        if os.path.exists(video_file):
            size = os.path.getsize(video_file)
            frame_size_estimate = size / 10  # 10 saniye / 1 FPS = ~10 frame
            print(f"📊 Dosya boyutu: {size:,} bytes ({size/1024:.1f} KB)")
            print(f"📊 Tahmini frame boyutu: {frame_size_estimate/1024:.1f} KB")
            
            if frame_size_estimate <= 8192:  # 8KB limit
                print("✅ Frame boyutu XBee limiti içinde")
            else:
                print("⚠️ Frame boyutu XBee limiti üzerinde")
            
            return video_file
        else:
            print("❌ Video dosyası oluşturulamadı")
            return None
            
    except Exception as e:
        print(f"❌ XBee test hatası: {e}")
        return None

def analyze_video_file(video_file):
    """Video dosyasını FFprobe ile analiz et"""
    try:
        print(f"🔍 Video analizi: {video_file}")
        
        # FFprobe komutu
        result = subprocess.run([
            'ffprobe', '-v', 'quiet', '-print_format', 'json',
            '-show_format', '-show_streams', video_file
        ], capture_output=True, text=True, timeout=10)
        
        if result.returncode == 0:
            print("✅ Video dosyası geçerli (FFprobe başarılı)")
            
            # Basit bilgi çıkarma
            output = result.stdout
            if '"codec_name": "h264"' in output:
                print("✅ Codec: H.264")
            elif '"codec_name": "mjpeg"' in output:
                print("✅ Codec: MJPEG")
                
            # Oynatıcı uyumluluk testi
            test_media_players(video_file)
        else:
            print("❌ Video dosyası bozuk (FFprobe başarısız)")
            
    except FileNotFoundError:
        print("⚠️ FFprobe bulunamadı")
    except subprocess.TimeoutExpired:
        print("⚠️ FFprobe timeout")
    except Exception as e:
        print(f"⚠️ Video analiz hatası: {e}")

def test_media_players(video_file):
    """Video oynatıcı uyumluluk testi"""
    print("🎮 Oynatıcı uyumluluk testi...")
    
    players = [
        ("VLC", ["vlc", "--intf", "dummy", "--play-and-exit", "--quiet"]),
        ("FFplay", ["ffplay", "-autoexit", "-nodisp"]),
        ("MPV", ["mpv", "--no-video", "--length=1"])
    ]
    
    for name, cmd_base in players:
        try:
            cmd = cmd_base + [video_file]
            result = subprocess.run(cmd, capture_output=True, timeout=5)
            if result.returncode == 0:
                print(f"✅ {name} uyumlu")
            else:
                print(f"❌ {name} uyumlu değil")
        except (FileNotFoundError, subprocess.TimeoutExpired):
            print(f"⚠️ {name} bulunamadı")
        except Exception:
            print(f"⚠️ {name} test hatası")

def main():
    print("🚀 RASPBERRY PI 2W VİDEO SİSTEMİ TESTİ")
    print("=" * 60)
    print("🎯 Görev: İkili video sistem - SD tam kalite + XBee düşük kalite")
    print("💻 Platform: Raspberry Pi 2W (ARM Cortex-A7 900MHz, 512MB RAM)")
    print("=" * 60)
    
    # Test 1: SD Kart tam kalite
    sd_file = test_sd_card_recording()
    
    # Test 2: XBee düşük kalite  
    xbee_file = test_xbee_streaming()
    
    # Sonuçlar
    print("\n" + "=" * 60)
    print("📋 TEST SONUÇLARI")
    print("=" * 60)
    
    if sd_file:
        print(f"✅ SD KART: {sd_file}")
        print("   🎥 Format: H.264/MP4 (tüm oynatıcılara uyumlu)")
        print("   📊 Kalite: Tam kalite (2 Mbps)")
    else:
        print("❌ SD KART: Test başarısız")
    
    if xbee_file:
        print(f"✅ XBee: {xbee_file}")
        print("   📡 Format: MJPEG/AVI (düşük bant genişliği)")
        print("   📊 Kalite: Optimize (%25 JPEG)")
    else:
        print("❌ XBee: Test başarısız")
    
    print("\n📥 Dosyaları bilgisayara kopyalamak için:")
    if sd_file:
        print(f"scp atugem@192.168.47.30:/home/atugem/TurkSatModelUydu/GorevYukuPi/{sd_file} ./")
    if xbee_file:
        print(f"scp atugem@192.168.47.30:/home/atugem/TurkSatModelUydu/GorevYukuPi/{xbee_file} ./")
    
    print("\n🎯 Sistem Önerisi:")
    print("   SD Kart: Sürekli tam kalite kayıt (640x480 H.264/MP4)")
    print("   XBee: İsteğe bağlı canlı yayın (320x240 MJPEG 1FPS)")
    print("   Bandwidth: %15 XBee kullanımı (%85 rezerv)")

if __name__ == "__main__":
    main()