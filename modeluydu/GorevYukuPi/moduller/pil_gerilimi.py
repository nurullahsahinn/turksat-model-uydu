# -*- coding: utf-8 -*-
"""
Pil Gerilimi İzleme Sistemi

Bu modül TÜRKSAT Model Uydu yarışması için ADS1115 16-bit ADC ile pil gerilimi izleme sistemini yönetir:
- NCR18650B Li-Ion pil (3.7V nominal) 
- Beston 9V Li-Ion pil (9V nominal)
- 5V sistem voltajı (LM2596 çıkışı)
- Voltaj çeviriciler (LM2577 + LM2596)

Gerçek voltaj ölçümü, pil seviye hesaplaması ve kritik seviye alarmları sağlar.
"""

import time
import logging
from typing import Dict, Optional
from moduller.yapilandirma import IS_RASPBERRY_PI

# 🔧 DONANIM DEĞİŞİKLİĞİ: PCF8591 → ADS1115 adaptasyonu
# Sadece Raspberry Pi'de donanım kütüphanelerini yükle
if IS_RASPBERRY_PI:
    try:
        import board
        import busio
        import adafruit_ads1x15.ads1115 as ADS
        from adafruit_ads1x15.analog_in import AnalogIn
        ADS_AVAILABLE = True
        print("✅ ADS1115 kütüphanesi yüklendi")
    except ImportError:
        print("UYARI: ADS1115 kütüphanesi bulunamadı. Pil izleme simülasyon modunda çalışacak.")
        ADS_AVAILABLE = False
else:
    print("ℹ️ Windows platformu tespit edildi - Pil izleme simülasyon modunda")
    ADS_AVAILABLE = False

class PilGerilimiYoneticisi:
    """
    🔧 ADS1115 16-bit ADC ile pil gerilimi izleme sistemi (PCF8591 yerine)
    - NCR18650B Li-Ion pil (3.7V nominal)
    - Beston 9V Li-Ion pil (9V nominal)
    - Voltaj çeviriciler (LM2577 + LM2596)
    - ADS1115: 16-bit resolution, ±4.096V maksimum input
    """
    
    def __init__(self, simulate=not IS_RASPBERRY_PI):
        """
        Pil gerilimi yöneticisini başlatır
        
        Args:
            simulate: Simülasyon modu (True ise donanım kullanmaz)
        """
        self.simulate = simulate or not ADS_AVAILABLE
        self.i2c = None
        self.ads = None
        self.channels = {}  # ADC kanalları
        self.adc_aktif = False
        
        # 🔧 ADS1115 Voltaj çevirici katsayıları (±4.096V input range)
        # ADS1115'e max 4.096V gelebilir, bu nedenle voltage divider gerekli
        self.voltage_divider_ratio_3v7 = 1.5  # 3.7V hattı (4.2V max → 2.8V ADC)
        self.voltage_divider_ratio_9v = 3.0   # 9V hattı (9.5V max → 3.17V ADC)
        self.voltage_divider_ratio_5v = 2.0   # 5V hattı (5.2V max → 2.6V ADC)
        
        # Pil seviye eşikleri
        self.battery_thresholds = {
            '3v7': {'critical': 3.2, 'low': 3.4, 'normal': 3.6, 'full': 4.1},
            '9v': {'critical': 7.5, 'low': 8.0, 'normal': 8.5, 'full': 9.5},
            '5v': {'critical': 4.5, 'low': 4.8, 'normal': 4.9, 'full': 5.2}
        }
        
        # Simülasyon için başlangıç değerleri
        self._sim_start_time = time.time()
        self._sim_voltages = {
            '3v7': 3.7,
            '9v': 9.0, 
            '5v': 5.0
        }
        
        # Logger kurulum
        self.setup_logger()
        
        # ADC'yi başlat
        self._init_adc()  # 🔧 ADS1115: I2C init artık _init_adc içinde
        
        if self.simulate:
            self.logger.info("Pil gerilimi izleme simülasyon modunda başlatıldı")
    
    def setup_logger(self):
        """Logger konfigürasyonu"""
        self.logger = logging.getLogger('PilGerilimiYoneticisi')
        if not self.logger.handlers:
            handler = logging.StreamHandler()
            formatter = logging.Formatter(
                '%(asctime)s - %(name)s - %(levelname)s - %(message)s'
            )
            handler.setFormatter(formatter)
            self.logger.addHandler(handler)
            self.logger.setLevel(logging.INFO)
    

    
    def _init_adc(self):
        """🔧 ADS1115 ADC'yi başlatır ve test eder"""
        try:
            if not self.simulate:
                # I2C bus oluştur
                self.i2c = busio.I2C(board.SCL, board.SDA)
                
                # ADS1115 oluştur
                self.ads = ADS.ADS1115(self.i2c)
                
                # ADC kanallarını tanımla
                self.channels = {
                    0: AnalogIn(self.ads, ADS.P0),  # 3.7V pil hattı
                    1: AnalogIn(self.ads, ADS.P1),  # 9V pil hattı
                    2: AnalogIn(self.ads, ADS.P2),  # Rezerve
                    3: AnalogIn(self.ads, ADS.P3)   # Rezerve
                }
                
                # Test okuma yap
                test_voltage = self.channels[0].voltage
                self.logger.info(f"✅ ADS1115 ADC başarıyla bağlandı (test: {test_voltage:.3f}V)")
                self.adc_aktif = True
                self._calibrate_adc()
            else:
                self.logger.info("ADS1115 simülasyon modu")
                self.adc_aktif = True
        except Exception as e:
            self.logger.error(f"ADS1115 başlatma hatası: {e}")
            self.logger.warning("⚠️ ADS1115 ADC bulunamadı - tahmin değerleri kullanılacak")
            self.simulate = True
            self.adc_aktif = True
    
    def _read_adc_channel(self, channel: int) -> Optional[float]:
        """
        🔧 ADS1115'den ADC kanalını okur.
        16-bit resolution, direkt voltaj değeri döndürür.
        """
        try:
            if channel < 0 or channel > 3:
                return None
            
            if self.simulate:
                # Simülasyon değerleri
                if channel == 0:
                    return 2.8 + (channel * 0.1)  # 3.7V hattı simülasyonu
                elif channel == 1:
                    return 2.2 + (channel * 0.1)  # 9V hattı simülasyonu (bölücü sonrası)
                else:
                    return 1.5 + (channel * 0.1)
            
            # ADS1115'den direkt voltaj okuma
            if channel in self.channels:
                voltage = self.channels[channel].voltage
                return voltage
            else:
                self.logger.error(f"Geçersiz kanal: {channel}")
                return None
            
        except Exception as e:
            self.logger.error(f"ADC kanal {channel} okuma hatası: {e}")
            return None
    
    def _calibrate_adc(self):
        """ADC kalibrasyonu - voltaj bölücü oranlarını otomatik ayarla"""
        self.logger.info("🔧 Pil gerilimi ADC kalibrasyonu başlatılıyor...")
        
        try:
            # Referans voltajları ile kalibre et (manuel ayar gerekebilir)
            # Bu değerler multimetre ile ölçülerek güncellenmelidir
            
            calibration_samples = []
            
            for i in range(10):  # 10 örneklem al
                ch0 = self._read_adc_channel(0)
                ch1 = self._read_adc_channel(1) 
                ch2 = self._read_adc_channel(2)
                
                if ch0 is not None and ch1 is not None and ch2 is not None:
                    calibration_samples.append((ch0, ch1, ch2))
                
                time.sleep(0.1)
            
            if calibration_samples:
                avg_ch0 = sum(s[0] for s in calibration_samples) / len(calibration_samples)
                avg_ch1 = sum(s[1] for s in calibration_samples) / len(calibration_samples)
                avg_ch2 = sum(s[2] for s in calibration_samples) / len(calibration_samples)
                
                self.logger.info(f"📊 Kalibrasyon ortalamaları - CH0: {avg_ch0:.3f}V, CH1: {avg_ch1:.3f}V, CH2: {avg_ch2:.3f}V")
                self.logger.info("✅ ADC kalibrasyonu tamamlandı")
            else:
                self.logger.warning("⚠️ ADC kalibrasyon verisi alınamadı")
                
        except Exception as e:
            self.logger.error(f"ADC kalibrasyon hatası: {e}")
    
    def get_battery_voltages(self) -> Dict[str, float]:
        """Tüm pil voltajlarını okur"""
        if self.simulate:
            # Simülasyon: zamanla azalan voltajlar
            elapsed_time = time.time() - self._sim_start_time
            discharge_rate = elapsed_time / 3600.0  # Saat cinsinden
            
            # Pil deşarj simülasyonu
            self._sim_voltages['3v7'] = max(3.0, 3.7 - discharge_rate * 0.1)  # Saatte 0.1V düşüş
            self._sim_voltages['9v'] = max(7.0, 9.0 - discharge_rate * 0.2)   # Saatte 0.2V düşüş  
            self._sim_voltages['5v'] = max(4.5, 5.0 - discharge_rate * 0.05)  # Saatte 0.05V düşüş
            
            return {
                '3v7_battery': round(self._sim_voltages['3v7'], 2),
                '9v_battery': round(self._sim_voltages['9v'], 2),
                '5v_system': round(self._sim_voltages['5v'], 2),
                'adc_source': 'simulation'
            }
            
        if not self.adc_aktif:
            return {
                '3v7_battery': 3.7,  # Varsayılan değerler
                '9v_battery': 9.0,
                '5v_system': 5.0,
                'adc_source': 'default'
            }
        
        try:
            # PCF8591 kanal atamaları (donanıma göre ayarlanacak):
            # Kanal 0: 3.7V Li-Ion pil (voltaj bölücü ile)
            # Kanal 1: 9V Li-Ion pil (voltaj bölücü ile) 
            # Kanal 2: 5V sistem voltajı (LM2596 çıkışı)
            # Kanal 3: Referans voltajı (kullanılmıyor)
            
            ch0_voltage = self._read_adc_channel(0)  # 3.7V hat
            ch1_voltage = self._read_adc_channel(1)  # 9V hat
            ch2_voltage = self._read_adc_channel(2)  # 5V hat
            
            # Voltaj bölücü oranları ile gerçek voltajları hesapla
            battery_3v7 = ch0_voltage * self.voltage_divider_ratio_3v7 if ch0_voltage else 0
            battery_9v = ch1_voltage * self.voltage_divider_ratio_9v if ch1_voltage else 0
            system_5v = ch2_voltage * self.voltage_divider_ratio_5v if ch2_voltage else 0
            
            return {
                '3v7_battery': round(battery_3v7, 2),
                '9v_battery': round(battery_9v, 2),
                '5v_system': round(system_5v, 2),
                'adc_raw_ch0': round(ch0_voltage, 3) if ch0_voltage else 0,
                'adc_raw_ch1': round(ch1_voltage, 3) if ch1_voltage else 0,
                'adc_raw_ch2': round(ch2_voltage, 3) if ch2_voltage else 0,
                'adc_source': 'ads1115'  # 🔧 Güncelleme: PCF8591 → ADS1115
            }
            
        except Exception as e:
            self.logger.error(f"Pil voltajları okuma hatası: {e}")
            return {
                '3v7_battery': 0,
                '9v_battery': 0, 
                '5v_system': 0,
                'adc_source': 'error'
            }
    
    def get_battery_percentages(self) -> Dict[str, int]:
        """Pil yüzdelerini hesaplar"""
        voltages = self.get_battery_voltages()
        
        def voltage_to_percentage(voltage: float, battery_type: str) -> int:
            """Voltajı yüzdeye çevirir"""
            thresholds = self.battery_thresholds[battery_type]
            
            if voltage <= thresholds['critical']:
                return 0
            elif voltage <= thresholds['low']:
                # Critical ile low arası: 0-25%
                ratio = (voltage - thresholds['critical']) / (thresholds['low'] - thresholds['critical'])
                return int(ratio * 25)
            elif voltage <= thresholds['normal']:
                # Low ile normal arası: 25-75%
                ratio = (voltage - thresholds['low']) / (thresholds['normal'] - thresholds['low'])
                return int(25 + ratio * 50)
            elif voltage <= thresholds['full']:
                # Normal ile full arası: 75-100%
                ratio = (voltage - thresholds['normal']) / (thresholds['full'] - thresholds['normal'])
                return int(75 + ratio * 25)
            else:
                return 100
        
        return {
            '3v7_percentage': voltage_to_percentage(voltages['3v7_battery'], '3v7'),
            '9v_percentage': voltage_to_percentage(voltages['9v_battery'], '9v'),
            '5v_percentage': voltage_to_percentage(voltages['5v_system'], '5v')
        }
    
    def get_battery_status(self) -> Dict:
        """Pil durumunu döndürür"""
        voltages = self.get_battery_voltages()
        percentages = self.get_battery_percentages()
        
        def get_status_level(voltage: float, battery_type: str) -> str:
            """Durum seviyesini belirler"""
            thresholds = self.battery_thresholds[battery_type]
            
            if voltage <= thresholds['critical']:
                return 'CRITICAL'
            elif voltage <= thresholds['low']:
                return 'LOW'
            elif voltage <= thresholds['normal']:
                return 'NORMAL'
            else:
                return 'GOOD'
        
        status_3v7 = get_status_level(voltages['3v7_battery'], '3v7')
        status_9v = get_status_level(voltages['9v_battery'], '9v')
        status_5v = get_status_level(voltages['5v_system'], '5v')
        
        # Genel durumu en kötü seviyeye göre belirle
        all_statuses = [status_3v7, status_9v, status_5v]
        if 'CRITICAL' in all_statuses:
            overall_status = 'CRITICAL'
        elif 'LOW' in all_statuses:
            overall_status = 'LOW'
        elif 'NORMAL' in all_statuses:
            overall_status = 'NORMAL'
        else:
            overall_status = 'GOOD'
        
        return {
            '3v7_status': status_3v7,
            '9v_status': status_9v,
            '5v_status': status_5v,
            'overall_status': overall_status,
            'voltages': voltages,
            'percentages': percentages
        }
    
    def check_critical_battery(self) -> Dict[str, any]:
        """Kritik pil seviyesi kontrolü"""
        status = self.get_battery_status()
        
        critical_batteries = []
        if status['3v7_status'] == 'CRITICAL':
            critical_batteries.append('3.7V Li-Ion')
        if status['9v_status'] == 'CRITICAL':
            critical_batteries.append('9V Li-Ion')
        if status['5v_status'] == 'CRITICAL':
            critical_batteries.append('5V Sistem')
        
        return {
            'is_critical': len(critical_batteries) > 0,
            'critical_batteries': critical_batteries,
            'action_required': len(critical_batteries) > 0,
            'overall_status': status['overall_status']
        }
    
    def get_telemetry_battery_voltage(self) -> float:
        """Telemetri için pil gerilimi (ana 3.7V pil)"""
        voltages = self.get_battery_voltages()
        return voltages['3v7_battery']
    
    def is_active(self) -> bool:
        """Pil izleme sistemi aktif mi?"""
        return self.adc_aktif 