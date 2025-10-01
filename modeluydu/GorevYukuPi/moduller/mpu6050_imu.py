# -*- coding: utf-8 -*-
"""
MPU6050 IMU Sensör Yöneticisi

Bu modül TÜRKSAT Model Uydu yarışması için MPU6050 IMU sensörünü yönetir:
- MPU6050: 6-axis accelerometer + gyroscope (ivmeölçer + jiroskop)
- BMP280: Barometric pressure sensor (basınç - ayrı modülde)

Pitch, Roll, Yaw hesaplaması ve telemetri entegrasyonu sağlar.
"""

import time
import math
import logging
import random
from typing import Dict, Optional, Tuple
from moduller.yapilandirma import IS_RASPBERRY_PI

# Raspberry Pi üzerinde gerçek I2C kütüphanesi
try:
    import smbus
    import struct
except ImportError:
    smbus = None

class MPU6050IMUYoneticisi:
    """
    MPU6050 IMU sensör yöneticisi - Pitch, Roll, Yaw hesaplama
    Donanım: MPU6050 (6-axis gyro + accelerometer)
    """
    
    def __init__(self, bus_number=1, simulate=not IS_RASPBERRY_PI):
        """
        MPU6050 IMU sensör yöneticisini başlatır
        
        Args:
            bus_number: I2C bus numarası (Raspberry Pi'da genellikle 1)
            simulate: Simülasyon modu (True ise donanım kullanmaz)
        """
        self.simulate = simulate
        self.bus_number = bus_number
        self.bus = None
        
        # MPU6050 I2C adresi
        self.MPU6050_ADDR = 0x68    # Varsayılan MPU6050 adresi (AD0=LOW)
        
        # MPU6050 Register adresleri
        self.PWR_MGMT_1 = 0x6B      # Power Management 1
        self.SMPLRT_DIV = 0x19      # Sample Rate Divider
        self.CONFIG = 0x1A          # Configuration
        self.GYRO_CONFIG = 0x1B     # Gyroscope Configuration
        self.ACCEL_CONFIG = 0x1C    # Accelerometer Configuration
        self.INT_ENABLE = 0x38      # Interrupt Enable
        
        # Veri register'ları
        self.ACCEL_XOUT_H = 0x3B    # Accelerometer X-axis high byte
        self.ACCEL_XOUT_L = 0x3C    # Accelerometer X-axis low byte
        self.ACCEL_YOUT_H = 0x3D    # Accelerometer Y-axis high byte
        self.ACCEL_YOUT_L = 0x3E    # Accelerometer Y-axis low byte
        self.ACCEL_ZOUT_H = 0x3F    # Accelerometer Z-axis high byte
        self.ACCEL_ZOUT_L = 0x40    # Accelerometer Z-axis low byte
        
        self.GYRO_XOUT_H = 0x43     # Gyroscope X-axis high byte
        self.GYRO_XOUT_L = 0x44     # Gyroscope X-axis low byte
        self.GYRO_YOUT_H = 0x45     # Gyroscope Y-axis high byte
        self.GYRO_YOUT_L = 0x46     # Gyroscope Y-axis low byte
        self.GYRO_ZOUT_H = 0x47     # Gyroscope Z-axis high byte
        self.GYRO_ZOUT_L = 0x48     # Gyroscope Z-axis low byte
        
        # Kalibrasyon değerleri
        self.gyro_offset_x = 0.0
        self.gyro_offset_y = 0.0  
        self.gyro_offset_z = 0.0
        
        # Accelerometer ve gyroscope scale faktörleri
        self.accel_scale = 16384.0  # ±2g için LSB/g
        self.gyro_scale = 131.0     # ±250°/s için LSB/(°/s)
        
        # Açı hesaplama için değişkenler
        self.pitch = 0.0
        self.roll = 0.0
        self.yaw = 0.0
        self.last_time = time.time()
        
        # Sensör durumu
        self.mpu_aktif = False
        
        # Logger kurulum
        self.setup_logger()
        
        # 🔧 FIX: Simülasyon modu devre dışı - sadece gerçek sensör
        if self.simulate:
            raise Exception("IMU simülasyon modu devre dışı! Sadece gerçek sensör kullanılabilir.")
        
        # Gerçek sensörü başlat
        self._init_i2c_bus()
        self._init_mpu6050()

    def setup_logger(self):
        """Logger konfigürasyonu"""
        self.logger = logging.getLogger('MPU6050IMUYoneticisi')
        if not self.logger.handlers:
            handler = logging.StreamHandler()
            formatter = logging.Formatter(
                '%(asctime)s - %(name)s - %(levelname)s - %(message)s'
            )
            handler.setFormatter(formatter)
            self.logger.addHandler(handler)
            self.logger.setLevel(logging.INFO)
    
    def _init_i2c_bus(self):
        """I2C bus'ını başlatır"""
        try:
            if smbus:
                self.bus = smbus.SMBus(self.bus_number)
                self.logger.info(f"I2C bus {self.bus_number} başlatıldı")
            else:
                raise ImportError("smbus kütüphanesi yok")
        except Exception as e:
            self.logger.error(f"I2C bus başlatma hatası: {e}")

    def _init_mpu6050(self) -> bool:
        """MPU6050 sensörünü başlatır"""
        try:
            # MPU6050'yi uyandır (varsayılan olarak sleep modunda başlar)
            self.bus.write_byte_data(self.MPU6050_ADDR, self.PWR_MGMT_1, 0x00)
            time.sleep(0.1)
            
            # Sample rate ayarla (1kHz / (1 + SMPLRT_DIV))
            # SMPLRT_DIV = 7 → 125 Hz sampling rate
            self.bus.write_byte_data(self.MPU6050_ADDR, self.SMPLRT_DIV, 7)
            
            # DLPF (Digital Low Pass Filter) ayarla
            # CONFIG = 3 → 44Hz bandwidth
            self.bus.write_byte_data(self.MPU6050_ADDR, self.CONFIG, 3)
            
            # Gyroscope konfigürasyonu: ±250°/s
            self.bus.write_byte_data(self.MPU6050_ADDR, self.GYRO_CONFIG, 0x00)
            
            # Accelerometer konfigürasyonu: ±2g
            self.bus.write_byte_data(self.MPU6050_ADDR, self.ACCEL_CONFIG, 0x00)
            
            # WHO_AM_I register'ını test et
            who_am_i = self.bus.read_byte_data(self.MPU6050_ADDR, 0x75)
            
            # MPU6050 family desteklenen değerler
            valid_who_am_i = [0x68, 0x70, 0x71, 0x72, 0x73]  # MPU6050/6500/9250 variants
            
            if who_am_i in valid_who_am_i:
                self.mpu_aktif = True
                
                # Sensör tipini belirle
                sensor_type = {
                    0x68: "MPU6050",
                    0x70: "MPU6500", 
                    0x71: "MPU6500/MPU9250",
                    0x72: "MPU6500 (Alt)",
                    0x73: "MPU9250"
                }.get(who_am_i, f"MPU Familie (0x{who_am_i:02X})")
                
                self.logger.info(f"✅ {sensor_type} başarıyla başlatıldı (WHO_AM_I: 0x{who_am_i:02X})")
                
                # Gyroscope kalibrasyon yap
                self._calibrate_gyro()
                return True
            else:
                self.logger.error(f"❌ Desteklenmeyen WHO_AM_I: 0x{who_am_i:02X} (beklenen: MPU6050/6500/9250 family)")
                return False
                
        except Exception as e:
            self.logger.error(f"❌ MPU6050 başlatma hatası: {e}")
            return False
    
    def _calibrate_gyro(self, samples=500):
        """Gyroscope kalibrasyonu yapar (hızlandırılmış) - SADECE GERÇEK SENSÖR"""
            
        self.logger.info("🔧 MPU6050 gyroscope kalibrasyonu başlıyor...")
        
        sum_x = sum_y = sum_z = 0
        
        for i in range(samples):
            gyro_data = self._read_gyro_raw()
            if gyro_data:
                sum_x += gyro_data['x']
                sum_y += gyro_data['y']
                sum_z += gyro_data['z']
            time.sleep(0.001)  # 1ms bekleme
        
        self.gyro_offset_x = sum_x / samples
        self.gyro_offset_y = sum_y / samples
        self.gyro_offset_z = sum_z / samples
        
        self.logger.info(f"✅ Gyroscope kalibrasyonu tamamlandı")
        self.logger.info(f"   Offset X: {self.gyro_offset_x:.2f}")
        self.logger.info(f"   Offset Y: {self.gyro_offset_y:.2f}")
        self.logger.info(f"   Offset Z: {self.gyro_offset_z:.2f}")
    
    def _read_accel_raw(self) -> Optional[Dict[str, float]]:
        """MPU6050'den ham accelerometer verisi okur - SADECE GERÇEK SENSÖR"""
        
        try:
            # 6 byte accelerometer verisi oku
            data = self.bus.read_i2c_block_data(self.MPU6050_ADDR, self.ACCEL_XOUT_H, 6)
            
            # 16-bit signed integer'lara çevir
            accel_x = struct.unpack('>h', bytes(data[0:2]))[0]
            accel_y = struct.unpack('>h', bytes(data[2:4]))[0]
            accel_z = struct.unpack('>h', bytes(data[4:6]))[0]
            
            # Scale faktörü ile gerçek değerlere çevir (g cinsinden)
            accel_x_g = accel_x / self.accel_scale
            accel_y_g = accel_y / self.accel_scale
            accel_z_g = accel_z / self.accel_scale
            
            # Debug çıktısı
            print(f"🔧 DEBUG MPU6050 Accel: Ham=({accel_x}, {accel_y}, {accel_z}) → G=({accel_x_g:.3f}, {accel_y_g:.3f}, {accel_z_g:.3f}) → m/s²=({accel_x_g*9.81:.2f}, {accel_y_g*9.81:.2f}, {accel_z_g*9.81:.2f})")
            
            # g cinsinden döndür (get_orientation için)
            return {
                'x': accel_x_g,  # g
                'y': accel_y_g,  # g  
                'z': accel_z_g   # g
            }
            
        except Exception as e:
            self.logger.error(f"Accelerometer okuma hatası: {e}")
            return None
    
    def _read_gyro_raw(self) -> Optional[Dict[str, float]]:
        """MPU6050'den ham gyroscope verisi okur - SADECE GERÇEK SENSÖR"""
        
        try:
            # 6 byte gyroscope verisi oku
            data = self.bus.read_i2c_block_data(self.MPU6050_ADDR, self.GYRO_XOUT_H, 6)
            
            # 16-bit signed integer'lara çevir
            gyro_x = struct.unpack('>h', bytes(data[0:2]))[0]
            gyro_y = struct.unpack('>h', bytes(data[2:4]))[0]
            gyro_z = struct.unpack('>h', bytes(data[4:6]))[0]
            
            # Scale faktörü ile gerçek değerlere çevir (°/s)
            gyro_x_dps = gyro_x / self.gyro_scale
            gyro_y_dps = gyro_y / self.gyro_scale
            gyro_z_dps = gyro_z / self.gyro_scale
            
            # Debug çıktısı
            print(f"🔧 DEBUG MPU6050 Gyro: Ham=({gyro_x}, {gyro_y}, {gyro_z}) → °/s=({gyro_x_dps:.2f}, {gyro_y_dps:.2f}, {gyro_z_dps:.2f})")
            
            return {
                'x': gyro_x_dps,
                'y': gyro_y_dps,
                'z': gyro_z_dps
            }
            
        except Exception as e:
            self.logger.error(f"Gyroscope okuma hatası: {e}")
            return None
    
    def get_orientation(self):
        """Pitch, Roll, Yaw açılarını döndürür (derece cinsinden)."""
        try:
            accel = self._read_accel_raw()
            gyro = self._read_gyro_raw()
            
            if not accel or not gyro:
                return {'pitch': 0.0, 'roll': 0.0, 'yaw': 0.0}
            
            current_time = time.time()
            dt = current_time - self.last_time
            self.last_time = current_time
            
            # 🔧 FIX: dt kontrolü - ilk okuma veya anormal dt
            if dt <= 0 or dt > 1.0:  # 1 saniyeden fazla ise reset
                dt = 0.01  # 10ms varsayılan
            
            # 🔧 FIX: Veri doğrulama - aşırı değerleri filtrele
            if abs(accel['x']) > 4 or abs(accel['y']) > 4 or abs(accel['z']) > 20:
                # Aşırı ivme değerleri - önceki değerleri koru
                return {'pitch': self.pitch, 'roll': self.roll, 'yaw': self.yaw}
            
            # İvmeölçerden statik açı hesaplama
            accel_pitch = math.atan2(accel['y'], math.sqrt(accel['x']**2 + accel['z']**2)) * 180.0 / math.pi
            accel_roll = math.atan2(-accel['x'], accel['z']) * 180.0 / math.pi
            
            # Açı sınırlaması
            accel_pitch = max(-90, min(90, accel_pitch))
            accel_roll = max(-90, min(90, accel_roll))
            
            # Gyroscope kalibrasyon uygulaması
            gyro_x_cal = gyro['x'] - self.gyro_offset_x
            gyro_y_cal = gyro['y'] - self.gyro_offset_y
            gyro_z_cal = gyro['z'] - self.gyro_offset_z
            
            # 🔧 FIX: Gyro veri sınırlaması (aşırı değerleri filtrele)
            gyro_x_cal = max(-250, min(250, gyro_x_cal))  # ±250°/s sınırı
            gyro_y_cal = max(-250, min(250, gyro_y_cal))
            gyro_z_cal = max(-250, min(250, gyro_z_cal))
            
            # Jiroskop entegrasyonu ile açı tahmini
            gyro_pitch_estimate = self.pitch + gyro_x_cal * dt
            gyro_roll_estimate = self.roll + gyro_y_cal * dt
            
            # 🔧 FIX: Complementary filter - daha dengeli ağırlık
            alpha = 0.85  # Jiroskop ağırlığı (85%), ivmeölçer (15%) - drift azaltıldı
            self.pitch = alpha * gyro_pitch_estimate + (1 - alpha) * accel_pitch
            self.roll = alpha * gyro_roll_estimate + (1 - alpha) * accel_roll
            
            # 🔧 FIX: Final açı sınırlaması
            self.pitch = max(-90, min(90, self.pitch))
            self.roll = max(-90, min(90, self.roll))
            
            # Yaw: sadece jiroskop entegrasyonu (drift var ama kısa vadede doğru)
            self.yaw += gyro_z_cal * dt
            
            # Yaw'ı -180 ile +180 derece arasında tut
            while self.yaw > 180:
                self.yaw -= 360
            while self.yaw < -180:
                self.yaw += 360
                
            return {
                'pitch': self.pitch,
                'roll': self.roll, 
                'yaw': self.yaw
            }
            
        except Exception as e:
            self.logger.error(f"Duruş hesaplama hatası: {e}")
            return {'pitch': 0.0, 'roll': 0.0, 'yaw': 0.0}
    
    def get_telemetry_data(self):
        """Telemetri için formatlanmış veri döndürür - SADECE GERÇEK SENSÖR"""
        orientation = self.get_orientation()
        return {
            'pitch': round(orientation['pitch'], 1),
            'roll': round(orientation['roll'], 1),
            'yaw': round(orientation['yaw'], 1)
        }
    
    def is_active(self) -> bool:
        """MPU6050 sensörünün aktif olup olmadığını döndürür"""
        return self.mpu_aktif
    
    def get_raw_data(self):
        """Ham accelerometer, gyroscope verilerini döndürür (magnetometer MPU6050'de yok)"""
        accel_g = self._read_accel_raw()  # g cinsinden
        gyro = self._read_gyro_raw()
        
        # Accelerometer'ı telemetri için m/s²'ye çevir
        accel_ms2 = None
        if accel_g:
            accel_ms2 = {
                'x': accel_g['x'] * 9.81,  # g → m/s²
                'y': accel_g['y'] * 9.81,  # g → m/s²
                'z': accel_g['z'] * 9.81   # g → m/s²
            }
        
        return {
            'accelerometer': accel_ms2,
            'gyroscope': gyro,
            'magnetometer': {'x': 0.0, 'y': 0.0, 'z': 0.0}  # MPU6050'de magnetometer yok
        }
    
    def reset_orientation(self):
        """Açı hesaplamalarını sıfırlar"""
        self.pitch = 0.0
        self.roll = 0.0
        self.yaw = 0.0
        self.last_time = time.time()
        self.logger.info("🔄 Açı hesaplamaları sıfırlandı")
    
    def kapat(self):
        """MPU6050 sensörünü kapat"""
        if self.bus and not self.simulate:
            try:
                # MPU6050'yi sleep moduna al
                self.bus.write_byte_data(self.MPU6050_ADDR, self.PWR_MGMT_1, 0x40)
                self.bus.close()
                self.logger.info("MPU6050 sensörü kapatıldı")
            except Exception as e:
                self.logger.error(f"MPU6050 kapatma hatası: {e}")

# Geriye uyumluluk için alias
IMUSensorYoneticisi = MPU6050IMUYoneticisi

if __name__ == "__main__":
    # Test kodu
    print("MPU6050 IMU Sensör Test Başlıyor...")
    
    mpu = MPU6050IMUYoneticisi(simulate=True)
    
    for i in range(10):
        orientation = mpu.get_orientation()
        telemetry = mpu.get_telemetry_data()
        
        print(f"Test {i+1}:")
        print(f"  Orientation: {orientation}")
        print(f"  Telemetry: {telemetry}")
        print(f"  Active: {mpu.is_active()}")
        
        time.sleep(1)
    
    mpu.kapat()
    print("Test tamamlandı!")