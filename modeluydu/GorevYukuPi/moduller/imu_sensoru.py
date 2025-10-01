# -*- coding: utf-8 -*-
"""
IMU Sensör Yöneticisi

Bu modül TÜRKSAT Model Uydu yarışması için IMU sensör sistemini yönetir:

🎯 YENİ MPU6050 MODU:
- MPU6050: 6-axis accelerometer + gyroscope (ivmeölçer + jiroskop)

📜 ESKİ 10-DOF MODU:
- ADXL345: 3-axis accelerometer (ivmeölçer)
- ITG3200: 3-axis gyroscope (jiroskop) 
- HMC5883L: 3-axis magnetometer (pusula)
- BMP280: Barometric pressure sensor (basınç - ayrı modülde)

Pitch, Roll, Yaw hesaplaması ve telemetri entegrasyonu sağlar.
"""

import time
import math
import logging
from typing import Dict, Optional, Tuple
from moduller.yapilandirma import IS_RASPBERRY_PI

# 🎯 MPU6050 MODU SEÇİMİ
USE_MPU6050 = True  # True: MPU6050 kullan, False: 10-DOF (ADXL345+ITG3200+HMC5883L) kullan

# MPU6050 modülü import
if USE_MPU6050:
    from moduller.mpu6050_imu import MPU6050IMUYoneticisi

# Raspberry Pi üzerinde gerçek I2C kütüphanesi
try:
    import smbus
    import struct
except ImportError:
    print("UYARI: smbus kütüphanesi bulunamadı. IMU simülasyon modunda çalışacak.")
    smbus = None

class IMUSensorYoneticisi:
    """
    IMU sensör yöneticisi - Pitch, Roll, Yaw hesaplama
    
    🎯 YENİ: MPU6050 Modu - 6-axis (gyro + accel)
    📜 ESKİ: 10-DOF Modu - ADXL345 + ITG3200 + HMC5883L + BMP280
    """
    
    def __init__(self, bus_number=1, simulate=not IS_RASPBERRY_PI):
        """
        IMU sensör yöneticisini başlatır
        
        Args:
            bus_number: I2C bus numarası (Raspberry Pi'da genellikle 1)
            simulate: Simülasyon modu (True ise donanım kullanmaz)
        """
        self.simulate = simulate
        self.bus_number = bus_number
        
        # Logger kurulum
        self.setup_logger()
        
        # 🎯 MPU6050 MODU SEÇİMİ
        if USE_MPU6050:
            self.logger.info("🎯 MPU6050 IMU modu seçildi")
            self.mpu6050 = MPU6050IMUYoneticisi(bus_number=bus_number, simulate=simulate)
            self.imu_aktif = self.mpu6050.is_active()
        else:
            self.logger.info("📜 10-DOF IMU modu seçildi (ADXL345+ITG3200+HMC5883L)")
            self.mpu6050 = None
            
            # Eski 10-DOF sistem başlatma
            self.bus = None
            
            # I2C adresleri (10-DOF modül standart adresleri)
            self.ADXL345_ADDR = 0x53    # İvmeölçer
            self.ITG3200_ADDR = 0x69    # Gyro (AD0=HIGH için 0x69)  
            self.HMC5883L_ADDR = 0x1E   # Pusula
            
            # Kalibrasyon değerleri
            self.gyro_offset_x = 0.0
            self.gyro_offset_y = 0.0  
            self.gyro_offset_z = 0.0
            
            # Açı hesaplama için değişkenler
            self.pitch = 0.0
            self.roll = 0.0
            self.yaw = 0.0
            self.last_time = time.time()
            
            # Sensör durumu
            self.imu_aktif = False
            
            # Sensörleri başlat
            if not self.simulate:
                self._init_i2c_bus()
                self._init_sensors()
            else:
                self.logger.info("IMU sensörleri simülasyon modunda başlatıldı")
                self.imu_aktif = True
            self.imu_aktif = True
    
    def setup_logger(self):
        """Logger konfigürasyonu"""
        self.logger = logging.getLogger('IMUSensorYoneticisi')
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
            self.simulate = True
    
    def _init_sensors(self):
        """Tüm IMU sensörlerini başlatır"""
        success_count = 0
        
        # ADXL345 İvmeölçer
        if self._init_adxl345():
            success_count += 1
        
        # ITG3200 Gyro
        if self._init_itg3200():
            success_count += 1
            
        # HMC5883L Pusula
        if self._init_hmc5883l():
            success_count += 1
        
        # En az 2 sensör çalışıyorsa sistem aktif
        if success_count >= 2:
            self.imu_aktif = True
            self.logger.info(f"✅ IMU sistemi aktif ({success_count}/3 sensör çalışıyor)")
            
            # Gyro kalibrasyonu yap
            if success_count >= 1:  # Gyro varsa
                self._calibrate_gyro()
        else:
            self.logger.warning(f"⚠️ IMU sistemi pasif ({success_count}/3 sensör), simülasyona geçiliyor")
            self.simulate = True
            self.imu_aktif = True
    
    def _init_adxl345(self) -> bool:
        """ADXL345 İvmeölçer başlatma"""
        try:
            # Güç kontrol - ölçüm modunu aktif et
            self.bus.write_byte_data(self.ADXL345_ADDR, 0x2D, 0x08)
            
            # Veri formatı - ±16g, tam çözünürlük
            self.bus.write_byte_data(self.ADXL345_ADDR, 0x31, 0x0B)
            
            # Test okuma
            self.bus.read_byte_data(self.ADXL345_ADDR, 0x00)
            
            self.logger.info("✅ ADXL345 İvmeölçer başlatıldı")
            return True
        except Exception as e:
            self.logger.warning(f"❌ ADXL345 başlatma hatası: {e}")
            return False
    
    def _init_itg3200(self) -> bool:
        """ITG3200 Gyro başlatma"""
        try:
            # Güç yönetimi - X gyro referans olarak
            self.bus.write_byte_data(self.ITG3200_ADDR, 0x3E, 0x01)
            
            # Örnekleme hızı ve filtre - 100Hz, 20Hz LPF
            self.bus.write_byte_data(self.ITG3200_ADDR, 0x15, 0x18)
            
            # Test okuma
            self.bus.read_byte_data(self.ITG3200_ADDR, 0x00)
            
            self.logger.info("✅ ITG3200 Gyro başlatıldı")
            return True
        except Exception as e:
            self.logger.warning(f"❌ ITG3200 başlatma hatası: {e}")
            return False
    
    def _init_hmc5883l(self) -> bool:
        """HMC5883L Pusula başlatma"""
        try:
            # Konfigürasyon A - 8 sample, 75Hz
            self.bus.write_byte_data(self.HMC5883L_ADDR, 0x00, 0x78)
            
            # Konfigürasyon B - ±1.3Ga
            self.bus.write_byte_data(self.HMC5883L_ADDR, 0x01, 0x20)
            
            # Mod - Sürekli ölçüm modu
            self.bus.write_byte_data(self.HMC5883L_ADDR, 0x02, 0x00)
            
            # Test okuma
            self.bus.read_byte_data(self.HMC5883L_ADDR, 0x00)
            
            self.logger.info("✅ HMC5883L Pusula başlatıldı")
            return True
        except Exception as e:
            self.logger.warning(f"❌ HMC5883L başlatma hatası: {e}")
            return False
    
    def _calibrate_gyro(self):
        """Gyro kalibrasyonu - statik offset hesaplama"""
        if self.simulate:
            self.gyro_offset_x = 0.0
            self.gyro_offset_y = 0.0
            self.gyro_offset_z = 0.0
            self.logger.info("✅ Gyro kalibrasyonu (simülasyon) tamamlandı")
            return
            
        self.logger.info("🔧 Gyro kalibrasyonu başlatılıyor... (5 saniye)")
        
        offset_x_sum = 0.0
        offset_y_sum = 0.0
        offset_z_sum = 0.0
        sample_count = 0
        
        start_time = time.time()
        while time.time() - start_time < 5.0:  # 5 saniye kalibrasyon
            try:
                gyro_data = self._read_itg3200_raw()
                if gyro_data:
                    offset_x_sum += gyro_data[0]
                    offset_y_sum += gyro_data[1]
                    offset_z_sum += gyro_data[2]
                    sample_count += 1
                    
                time.sleep(0.01)  # 100Hz örnekleme
            except:
                continue
        
        if sample_count > 0:
            self.gyro_offset_x = offset_x_sum / sample_count
            self.gyro_offset_y = offset_y_sum / sample_count
            self.gyro_offset_z = offset_z_sum / sample_count
            
            self.logger.info(f"✅ Gyro kalibrasyonu tamamlandı - Offset: X={self.gyro_offset_x:.2f}, Y={self.gyro_offset_y:.2f}, Z={self.gyro_offset_z:.2f}")
        else:
            self.logger.warning("⚠️ Gyro kalibrasyonu başarısız - varsayılan değerler kullanılacak")
    
    def _read_adxl345(self) -> Optional[Dict[str, float]]:
        """ADXL345 ivme verilerini oku (g cinsinden)"""
        if self.simulate:
            # Simülasyon değerleri
            return {
                'x': 0.05 * math.sin(time.time() * 0.1),
                'y': 0.03 * math.cos(time.time() * 0.15), 
                'z': 1.0 + 0.02 * math.sin(time.time() * 0.05)
            }
            
        try:
            # 6 byte veri oku (X, Y, Z - 2 byte each)
            data = self.bus.read_i2c_block_data(self.ADXL345_ADDR, 0x32, 6)
            
            # 16-bit signed değerlere çevir
            x = struct.unpack('<h', bytes(data[0:2]))[0]
            y = struct.unpack('<h', bytes(data[2:4]))[0] 
            z = struct.unpack('<h', bytes(data[4:6]))[0]
            
            # 🔧 ADXL345 ±16g için doğru scale factor (Analiz0.txt düzeltmesi - web doğrulamalı)
            # ±16g modunda 31.2 mg/LSB (Engineers Garage datasheet verification)
            # NOT: Analiz4.txt'deki 3.9 mg/LSB yanlış - bu ±2g modu için
            scale_factor = 31.2 / 1000.0  # 31.2 mg/LSB → g/LSB dönüşümü
            
            return {
                'x': x * scale_factor * 9.81,  # m/s² cinsine çevir
                'y': y * scale_factor * 9.81,
                'z': z * scale_factor * 9.81
            }
        except Exception as e:
            self.logger.error(f"ADXL345 okuma hatası: {e}")
            return None
    
    def _read_itg3200_raw(self) -> Optional[list]:
        """ITG3200 ham gyro verilerini oku"""
        if self.simulate:
            return [0.1, -0.05, 0.02]  # Simülasyon değerleri
            
        try:
            # 6 byte veri oku (X, Y, Z - 2 byte each)
            data = self.bus.read_i2c_block_data(self.ITG3200_ADDR, 0x1D, 6)
            
            # 16-bit signed değerlere çevir
            x = struct.unpack('>h', bytes(data[0:2]))[0]  # Big-endian
            y = struct.unpack('>h', bytes(data[2:4]))[0]
            z = struct.unpack('>h', bytes(data[4:6]))[0]
            
            return [x, y, z]
        except Exception as e:
            self.logger.error(f"ITG3200 okuma hatası: {e}")
            return None
    
    def _read_itg3200(self) -> Optional[Dict[str, float]]:
        """ITG3200 gyro verilerini oku (derece/saniye)"""
        raw_data = self._read_itg3200_raw()
        if raw_data:
            # ITG3200 scale factor: 14.375 LSB/°/s
            scale_factor = 1.0 / 14.375
            
            return {
                'x': (raw_data[0] - self.gyro_offset_x) * scale_factor,
                'y': (raw_data[1] - self.gyro_offset_y) * scale_factor,
                'z': (raw_data[2] - self.gyro_offset_z) * scale_factor
            }
        return None
    
    def _read_hmc5883l(self) -> Optional[Dict[str, float]]:
        """HMC5883L pusula verilerini oku"""
        if self.simulate:
            # Simülasyon değerleri
            return {
                'x': 100 + 10 * math.sin(time.time() * 0.02),
                'y': 50 + 5 * math.cos(time.time() * 0.03),
                'z': 200 + 8 * math.sin(time.time() * 0.01)
            }
            
        try:
            # 6 byte veri oku (X, Z, Y sırasında - HMC5883L özelliği)
            data = self.bus.read_i2c_block_data(self.HMC5883L_ADDR, 0x03, 6)
            
            # 16-bit signed değerlere çevir
            x = struct.unpack('>h', bytes(data[0:2]))[0]  # Big-endian
            z = struct.unpack('>h', bytes(data[2:4]))[0]  
            y = struct.unpack('>h', bytes(data[4:6]))[0]
            
            return {'x': x, 'y': y, 'z': z}
        except Exception as e:
            self.logger.error(f"HMC5883L okuma hatası: {e}")
            return None
    
    def get_orientation(self):
        """Pitch, Roll, Yaw açılarını döndürür (derece cinsinden)."""
        # 🎯 MPU6050 MODU
        if USE_MPU6050 and self.mpu6050:
            return self.mpu6050.get_orientation()
        
        # 📜 ESKİ 10-DOF MODU
        try:
            accel = self._read_adxl345()
            gyro = self._read_itg3200()
            compass = self._read_hmc5883l()
            
            current_time = time.time()
            dt = current_time - self.last_time
            self.last_time = current_time
            
            # İvmeölçerden statik açı hesaplama
            accel_pitch = math.atan2(accel['y'], math.sqrt(accel['x']**2 + accel['z']**2)) * 180.0 / math.pi
            accel_roll = math.atan2(-accel['x'], accel['z']) * 180.0 / math.pi
            
            # Jiroskop entegrasyonu ile açı tahmini
            gyro_pitch_estimate = self.pitch + gyro['x'] * dt
            gyro_roll_estimate = self.roll + gyro['y'] * dt
            
            # Complementary filter (doğru sıralama)
            alpha = 0.95  # Jiroskop ağırlığı (95%), ivmeölçer (5%)
            self.pitch = alpha * gyro_pitch_estimate + (1 - alpha) * accel_pitch
            self.roll = alpha * gyro_roll_estimate + (1 - alpha) * accel_roll
            
            # Yaw: sadece jiroskop entegrasyonu (drift var ama kısa vadede doğru)
            self.yaw += gyro['z'] * dt
            
            # Pusula ile yaw drift düzeltmesi (tilt compensation ile)
            if compass:
                compass_yaw = self._get_tilt_compensated_heading(compass, self.pitch, self.roll)
                yaw_alpha = 0.99  # Yaw için daha konservatif birleştirme
                self.yaw = yaw_alpha * self.yaw + (1 - yaw_alpha) * compass_yaw
                
            return {
                'pitch': self.pitch,
                'roll': self.roll, 
                'yaw': self.yaw
            }
            
        except Exception as e:
            self.logger.error(f"Duruş hesaplama hatası: {e}")
            return {'pitch': 0.0, 'roll': 0.0, 'yaw': 0.0}
    
    def _get_tilt_compensated_heading(self, compass, pitch, roll):
        """Pitch ve Roll etkisini düzeltilmiş pusula yönü hesaplar."""
        try:
            # Açıları radyana çevir
            pitch_rad = math.radians(pitch)
            roll_rad = math.radians(roll)
            
            # Tilt compensation matrix uygulaması
            cos_pitch = math.cos(pitch_rad)
            sin_pitch = math.sin(pitch_rad)
            cos_roll = math.cos(roll_rad)
            sin_roll = math.sin(roll_rad)
            
            # Düzeltilmiş manyetik bileşenler
            Xh = compass['x'] * cos_pitch + compass['z'] * sin_pitch
            Yh = (compass['x'] * sin_roll * sin_pitch + 
                  compass['y'] * cos_roll - 
                  compass['z'] * sin_roll * cos_pitch)
            
            # Heading hesapla (0-360 derece)
            heading = math.atan2(Yh, Xh) * 180.0 / math.pi
            if heading < 0:
                heading += 360.0
                
            return heading
            
        except Exception as e:
            self.logger.error(f"Tilt compensation hatası: {e}")
            return math.atan2(compass['y'], compass['x']) * 180.0 / math.pi
    
    def get_acceleration(self) -> Optional[Dict[str, float]]:
        """Ham ivme verilerini döndür"""
        return self._read_adxl345()
    
    def get_gyro_rates(self) -> Optional[Dict[str, float]]:
        """Ham gyro oranlarını döndür"""
        return self._read_itg3200()
    
    def get_magnetic_field(self) -> Optional[Dict[str, float]]:
        """Ham manyetik alan verilerini döndür"""
        return self._read_hmc5883l()
    
    def is_active(self) -> bool:
        """IMU sistemi aktif mi?"""
        # 🎯 MPU6050 MODU
        if USE_MPU6050 and self.mpu6050:
            return self.mpu6050.is_active()
        
        # 📜 ESKİ 10-DOF MODU
        return self.imu_aktif
    
    def get_telemetry_data(self) -> Dict[str, float]:
        """Telemetri için IMU verilerini döndür"""
        # 🎯 MPU6050 MODU
        if USE_MPU6050 and self.mpu6050:
            return self.mpu6050.get_telemetry_data()
        
        # 📜 ESKİ 10-DOF MODU
        orientation = self.get_orientation()
        
        if orientation:
            return {
                'pitch': orientation['pitch'],
                'roll': orientation['roll'],
                'yaw': orientation['yaw']
            }
        else:
            # IMU olmadığında varsayılan değerler
            return {
                'pitch': 0.0,
                'roll': 0.0,
                'yaw': 0.0
            }
    
    def get_raw_data(self) -> Dict[str, Dict[str, float]]:
        """Ham accelerometer, gyroscope ve magnetometer verilerini döndür"""
        # 🎯 MPU6050 MODU
        if USE_MPU6050 and self.mpu6050:
            return self.mpu6050.get_raw_data()
        
        # 📜 ESKİ 10-DOF MODU
        try:
            result = {}
            
            # Accelerometer verileri
            accel = self._read_adxl345()
            if accel:
                result['accelerometer'] = accel
            
            # Gyroscope verileri
            gyro = self._read_itg3200()
            if gyro:
                result['gyroscope'] = gyro
            
            # Magnetometer verileri
            compass = self._read_hmc5883l()
            if compass:
                result['magnetometer'] = compass
            
            return result
            
        except Exception as e:
            self.logger.error(f"Ham veri okuma hatası: {e}")
            return {
                'accelerometer': {'x': 0.0, 'y': 0.0, 'z': 0.0},
                'gyroscope': {'x': 0.0, 'y': 0.0, 'z': 0.0},
                'magnetometer': {'x': 0.0, 'y': 0.0, 'z': 0.0}
            } 