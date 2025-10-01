#!/usr/bin/env python3
"""
IMU Düzeltme Testi
Yeni düzeltmelerin çalışıp çalışmadığını test eder
"""

import time
import sys
import os

# Modül yolunu ekle
sys.path.append(os.path.dirname(os.path.abspath(__file__)))

from moduller.mpu6050_imu import MPU6050IMUYoneticisi

def test_imu_improvements():
    """Düzeltilmiş IMU sistemini test et"""
    print("🧪 IMU Düzeltme Testi Başlıyor...")
    print("=" * 50)
    
    # IMU'yu başlat - GERÇEK SENSÖR
    try:
        imu = MPU6050IMUYoneticisi(simulate=False)  # Gerçek sensör modu
    except Exception as e:
        print(f"❌ IMU başlatılamadı: {e}")
        print("💡 Bu test sadece Raspberry Pi'de gerçek IMU sensörü ile çalışır!")
        return
    
    print(f"✅ IMU Aktif: {imu.is_active()}")
    print("\n📊 10 Saniye Test - Gerçekçi Değişim Kontrolü:")
    
    previous_data = None
    for i in range(10):
        # Telemetri verisi al
        telemetry = imu.get_telemetry_data()
        orientation = imu.get_orientation()
        
        print(f"Test {i+1:2d}: P={telemetry['pitch']:6.1f}° R={telemetry['roll']:6.1f}° Y={telemetry['yaw']:6.1f}°")
        
        # Değişim hızını kontrol et
        if previous_data:
            pitch_change = abs(telemetry['pitch'] - previous_data['pitch'])
            roll_change = abs(telemetry['roll'] - previous_data['roll'])
            yaw_change = abs(telemetry['yaw'] - previous_data['yaw'])
            
            # Aşırı hızlı değişim kontrolü
            if pitch_change > 10 or roll_change > 10 or yaw_change > 20:
                print(f"⚠️ Aşırı hızlı değişim tespit edildi!")
                print(f"   Pitch: {pitch_change:.1f}°, Roll: {roll_change:.1f}°, Yaw: {yaw_change:.1f}°")
            else:
                print(f"✅ Normal değişim: P={pitch_change:.1f}° R={roll_change:.1f}° Y={yaw_change:.1f}°")
        
        previous_data = telemetry
        time.sleep(1)
    
    print("\n🔧 Ham Veri Testi:")
    raw_data = imu.get_raw_data()
    if raw_data:
        print(f"Accelerometer: {raw_data.get('accelerometer', 'N/A')}")
        print(f"Gyroscope: {raw_data.get('gyroscope', 'N/A')}")
    
    print("\n✅ Test Tamamlandı!")
    
    # Temizlik
    imu.kapat()

def test_real_hardware():
    """Gerçek donanımda test (sadece Raspberry Pi'de)"""
    print("🔧 Gerçek Donanım Testi...")
    
    try:
        # Gerçek IMU başlat
        imu = MPU6050IMUYoneticisi(simulate=False)
        
        if not imu.is_active():
            print("❌ IMU başlatılamadı - I2C bağlantısını kontrol edin")
            return
        
        print("✅ Gerçek IMU başlatıldı")
        print("📊 5 Saniye Gerçek Veri Testi:")
        
        for i in range(5):
            telemetry = imu.get_telemetry_data()
            print(f"Gerçek {i+1}: P={telemetry['pitch']:6.1f}° R={telemetry['roll']:6.1f}° Y={telemetry['yaw']:6.1f}°")
            time.sleep(1)
        
        imu.kapat()
        print("✅ Gerçek donanım testi tamamlandı")
        
    except Exception as e:
        print(f"❌ Gerçek donanım testi hatası: {e}")

if __name__ == "__main__":
    print("🎯 IMU GERÇEK SENSÖR Test Paketi")
    print("=" * 40)
    print("⚠️ Bu test sadece Raspberry Pi'de gerçek IMU sensörü ile çalışır!")
    
    # Gerçek donanım testi (sadece Raspberry Pi'de)
    try:
        import smbus
        print("\n🔧 I2C kütüphanesi bulundu, gerçek donanım testi yapılıyor...")
        test_imu_improvements()  # Artık gerçek sensör kullanıyor
        test_real_hardware()
    except ImportError:
        print("\n❌ I2C kütüphanesi bulunamadı!")
        print("💡 Bu test sadece Raspberry Pi'de çalışır.")
    
    print("\n🎉 Tüm testler tamamlandı!")
