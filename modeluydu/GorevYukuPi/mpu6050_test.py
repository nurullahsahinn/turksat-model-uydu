#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""
MPU6050 IMU Test Scripti

Bu script MPU6050 IMU sensörünün doğru çalışıp çalışmadığını test eder.
"""

import time
import sys
import os

# ModulePATH ayarı
sys.path.insert(0, os.path.dirname(__file__))

from moduller.mpu6050_imu import MPU6050IMUYoneticisi
from moduller.imu_sensoru import IMUSensorYoneticisi

def test_mpu6050_direct():
    """MPU6050'yi doğrudan test et"""
    print("=" * 60)
    print("🎯 MPU6050 DOKrudan Test")
    print("=" * 60)
    
    try:
        # MPU6050 doğrudan başlat
        mpu = MPU6050IMUYoneticisi(simulate=False)
        
        print(f"📊 MPU6050 Aktif: {mpu.is_active()}")
        
        if mpu.is_active():
            print("\n🔄 5 saniye veri okuma testi:")
            
            for i in range(5):
                # Ham veri oku
                raw_data = mpu.get_raw_data()
                
                # Orientation oku
                orientation = mpu.get_orientation()
                
                # Telemetri oku
                telemetry = mpu.get_telemetry_data()
                
                print(f"\n📈 Test {i+1}:")
                print(f"  🧭 Orientation: P={orientation['pitch']:.1f}° R={orientation['roll']:.1f}° Y={orientation['yaw']:.1f}°")
                print(f"  📡 Telemetry: P={telemetry['pitch']:.1f}° R={telemetry['roll']:.1f}° Y={telemetry['yaw']:.1f}°")
                
                if raw_data['accelerometer']:
                    acc = raw_data['accelerometer']
                    print(f"  📏 Accel: X={acc['x']:.2f}g Y={acc['y']:.2f}g Z={acc['z']:.2f}g")
                
                if raw_data['gyroscope']:
                    gyro = raw_data['gyroscope']
                    print(f"  🌀 Gyro: X={gyro['x']:.1f}°/s Y={gyro['y']:.1f}°/s Z={gyro['z']:.1f}°/s")
                
                time.sleep(1)
        
        mpu.kapat()
        print("\n✅ MPU6050 doğrudan test tamamlandı")
        return True
        
    except Exception as e:
        print(f"❌ MPU6050 doğrudan test hatası: {e}")
        return False

def test_imu_wrapper():
    """IMUSensorYoneticisi wrapper ile test et"""
    print("\n" + "=" * 60)
    print("🔄 IMUSensorYoneticisi Wrapper Test")
    print("=" * 60)
    
    try:
        # IMUSensorYoneticisi ile başlat (MPU6050 modu)
        imu = IMUSensorYoneticisi(simulate=False)
        
        print(f"📊 IMU Sistemi Aktif: {imu.is_active()}")
        
        if imu.is_active():
            print("\n🔄 5 saniye wrapper testi:")
            
            for i in range(5):
                # Orientation
                orientation = imu.get_orientation()
                
                # Telemetry
                telemetry = imu.get_telemetry_data()
                
                print(f"\n📈 Wrapper Test {i+1}:")
                print(f"  🧭 Orientation: P={orientation['pitch']:.1f}° R={orientation['roll']:.1f}° Y={orientation['yaw']:.1f}°")
                print(f"  📡 Telemetry: P={telemetry['pitch']:.1f}° R={telemetry['roll']:.1f}° Y={telemetry['yaw']:.1f}°")
                
                time.sleep(1)
        
        print("\n✅ IMU wrapper test tamamlandı")
        return True
        
    except Exception as e:
        print(f"❌ IMU wrapper test hatası: {e}")
        return False

def test_simulation_mode():
    """Simülasyon modu test"""
    print("\n" + "=" * 60)
    print("🎮 Simülasyon Modu Test")
    print("=" * 60)
    
    try:
        # Simülasyon modunda başlat
        mpu_sim = MPU6050IMUYoneticisi(simulate=True)
        imu_sim = IMUSensorYoneticisi(simulate=True)
        
        print(f"📊 MPU6050 Simülasyon Aktif: {mpu_sim.is_active()}")
        print(f"📊 IMU Wrapper Simülasyon Aktif: {imu_sim.is_active()}")
        
        print("\n🔄 3 saniye simülasyon testi:")
        
        for i in range(3):
            # MPU6050 simülasyon
            mpu_tel = mpu_sim.get_telemetry_data()
            
            # IMU wrapper simülasyon
            imu_tel = imu_sim.get_telemetry_data()
            
            print(f"\n📈 Sim Test {i+1}:")
            print(f"  🎯 MPU6050: P={mpu_tel['pitch']:.1f}° R={mpu_tel['roll']:.1f}° Y={mpu_tel['yaw']:.1f}°")
            print(f"  🔄 IMU Wrap: P={imu_tel['pitch']:.1f}° R={imu_tel['roll']:.1f}° Y={imu_tel['yaw']:.1f}°")
            
            time.sleep(1)
        
        mpu_sim.kapat()
        print("\n✅ Simülasyon test tamamlandı")
        return True
        
    except Exception as e:
        print(f"❌ Simülasyon test hatası: {e}")
        return False

def main():
    """Ana test fonksiyonu"""
    print("🚀 MPU6050 IMU Sensör Test Başlıyor")
    print("=" * 60)
    
    # Test sonuçları
    results = []
    
    # 1. Simülasyon modu test (her zaman çalışır)
    results.append(("Simülasyon Modu", test_simulation_mode()))
    
    # 2. MPU6050 doğrudan test (sadece Raspberry Pi'de)
    results.append(("MPU6050 Doğrudan", test_mpu6050_direct()))
    
    # 3. IMU wrapper test (sadece Raspberry Pi'de)
    results.append(("IMU Wrapper", test_imu_wrapper()))
    
    # Sonuçları göster
    print("\n" + "=" * 60)
    print("📊 TEST SONUÇLARI")
    print("=" * 60)
    
    all_passed = True
    for test_name, result in results:
        status = "✅ BAŞARILI" if result else "❌ BAŞARISIZ"
        print(f"{test_name:20}: {status}")
        if not result:
            all_passed = False
    
    print("\n" + "=" * 60)
    if all_passed:
        print("🎉 TÜM TESTLER BAŞARILI! MPU6050 sistemi hazır.")
    else:
        print("⚠️ Bazı testler başarısız. Lütfen hata mesajlarını kontrol edin.")
    print("=" * 60)

if __name__ == "__main__":
    main()