#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""
I2C Bus Tarama Scripti
MPU6050 ve diğer I2C sensörlerini tespit eder
"""

import smbus2
import time

def scan_i2c_bus(bus_number=1):
    """I2C bus'ını tarar ve bağlı cihazları listeler"""
    print(f"🔍 I2C Bus {bus_number} Taranıyor...")
    print("=" * 50)
    
    try:
        bus = smbus2.SMBus(bus_number)
        
        devices_found = []
        
        # I2C adresleri 0x08 ile 0x77 arasında taranır
        for addr in range(0x08, 0x78):
            try:
                # Cihaza okuma deneyi yap
                bus.read_byte(addr)
                devices_found.append(addr)
                
                # Bilinen cihazları tanımla
                device_name = get_device_name(addr)
                print(f"📍 0x{addr:02X}: {device_name}")
                
            except Exception:
                # Cihaz yok, devam et
                pass
        
        bus.close()
        
        print("=" * 50)
        print(f"✅ Toplam {len(devices_found)} cihaz bulundu")
        
        # MPU6050 kontrolü
        if 0x68 in devices_found:
            print("🎯 MPU6050 (0x68) TESPİT EDİLDİ!")
        elif 0x69 in devices_found:
            print("🎯 MPU6050 (0x69) TESPİT EDİLDİ!")
        else:
            print("❌ MPU6050 tespit edilemedi")
            print("💡 Kontrol listesi:")
            print("   - MPU6050 bağlantıları kontrol edin")
            print("   - VCC: 3.3V veya 5V")
            print("   - GND: Ground")
            print("   - SDA: GPIO 2 (Pin 3)")
            print("   - SCL: GPIO 3 (Pin 5)")
            print("   - AD0: Low (0x68) veya High (0x69)")
        
        return devices_found
        
    except Exception as e:
        print(f"❌ I2C bus {bus_number} tarama hatası: {e}")
        return []

def get_device_name(addr):
    """I2C adresine göre cihaz adını döndürür"""
    known_devices = {
        0x1E: "HMC5883L (Magnetometer)",
        0x48: "ADS1115 (ADC)",
        0x53: "ADXL345 (Accelerometer)",
        0x68: "MPU6050/MPU6500/DS3231 (IMU/RTC)",
        0x69: "MPU6050/MPU6500 (IMU - AD0=High)",
        0x76: "BMP280/BME280 (Pressure)",
        0x77: "BMP280/BME280 (Pressure - Alt Addr)"
    }
    
    return known_devices.get(addr, "Bilinmeyen Cihaz")

def test_mpu6050_communication(addr=0x68):
    """MPU6050 ile haberleşme testi"""
    print(f"\n🧪 MPU6050 (0x{addr:02X}) Haberleşme Testi")
    print("-" * 40)
    
    try:
        bus = smbus2.SMBus(1)
        
        # WHO_AM_I register'ını oku (0x75)
        who_am_i = bus.read_byte_data(addr, 0x75)
        print(f"📊 WHO_AM_I: 0x{who_am_i:02X}")
        
        if who_am_i == 0x68:
            print("✅ MPU6050 doğrulandı!")
            
            # Power management register'ını oku
            pwr_mgmt = bus.read_byte_data(addr, 0x6B)
            print(f"📊 PWR_MGMT_1: 0x{pwr_mgmt:02X}")
            
            if pwr_mgmt & 0x40:
                print("⚠️ MPU6050 sleep modunda")
            else:
                print("✅ MPU6050 aktif")
                
        else:
            print(f"❌ Yanlış WHO_AM_I değeri (beklenen: 0x68)")
        
        bus.close()
        return True
        
    except Exception as e:
        print(f"❌ MPU6050 test hatası: {e}")
        return False

def main():
    """Ana test fonksiyonu"""
    print("🚀 I2C Sensör Tarama Başlıyor")
    print("=" * 50)
    
    # I2C bus 1'i tara
    devices = scan_i2c_bus(1)
    
    # MPU6050 varsa detaylı test yap
    if 0x68 in devices:
        test_mpu6050_communication(0x68)
    elif 0x69 in devices:
        test_mpu6050_communication(0x69)
    
    print("\n" + "=" * 50)
    print("📋 I2C TARAMA TAMAMLANDI")
    print("=" * 50)

if __name__ == "__main__":
    main()