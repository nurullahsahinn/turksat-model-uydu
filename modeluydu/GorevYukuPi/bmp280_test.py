#!/usr/bin/env python3
# -*- coding: utf-8 -*-

import smbus
import time
import traceback

# Sabitler
BMP280_I2C_ADDR = 0x76
DENIZ_SEVIYESI_BASINC_HPA = 1013.25

def to_signed_16(value):
    """16-bit signed integer'a dönüştürür."""
    return value if value < 32768 else value - 65536

def read_bmp280_raw():
    """
    BMP280 sensöründen ham ve işlenmiş verileri okur ve yazdırır.
    """
    try:
        bus = smbus.SMBus(1)
        print("✅ I2C bus 1 başarıyla başlatıldı.")

        # Kalibrasyon verilerini oku
        cal_data = bus.read_i2c_block_data(BMP280_I2C_ADDR, 0x88, 24)
        print("✅ Kalibrasyon verileri okundu.")

        # Kalibrasyon parametreleri
        dig_T1 = cal_data[0] | (cal_data[1] << 8)
        dig_T2 = to_signed_16(cal_data[2] | (cal_data[3] << 8))
        dig_T3 = to_signed_16(cal_data[4] | (cal_data[5] << 8))
        dig_P1 = cal_data[6] | (cal_data[7] << 8)
        dig_P2 = to_signed_16(cal_data[8] | (cal_data[9] << 8))
        dig_P3 = to_signed_16(cal_data[10] | (cal_data[11] << 8))
        dig_P4 = to_signed_16(cal_data[12] | (cal_data[13] << 8))
        dig_P5 = to_signed_16(cal_data[14] | (cal_data[15] << 8))
        dig_P6 = to_signed_16(cal_data[16] | (cal_data[17] << 8))
        dig_P7 = to_signed_16(cal_data[18] | (cal_data[19] << 8))
        dig_P8 = to_signed_16(cal_data[20] | (cal_data[21] << 8))
        dig_P9 = to_signed_16(cal_data[22] | (cal_data[23] << 8))

        # Forced mode'a al
        bus.write_byte_data(BMP280_I2C_ADDR, 0xF4, 0x25)
        time.sleep(0.1)

        # Ham verileri oku
        raw_data = bus.read_i2c_block_data(BMP280_I2C_ADDR, 0xF7, 6)
        adc_P = ((raw_data[0] << 16) | (raw_data[1] << 8) | raw_data[2]) >> 4
        adc_T = ((raw_data[3] << 16) | (raw_data[4] << 8) | raw_data[5]) >> 4
        print(f"✅ Ham ADC verileri: Basınç={adc_P}, Sıcaklık={adc_T}")

        # Sıcaklık hesapla
        var1 = ((adc_T / 16384.0) - (dig_T1 / 1024.0)) * dig_T2
        var2 = (((adc_T / 131072.0) - (dig_T1 / 8192.0)) * ((adc_T / 131072.0) - (dig_T1 / 8192.0))) * dig_T3
        t_fine = var1 + var2
        temperature = t_fine / 5120.0
        
        # Basınç hesapla (Pa olarak)
        var1 = (t_fine / 2.0) - 64000.0
        var2 = var1 * var1 * dig_P6 / 32768.0
        var2 = var2 + var1 * dig_P5 * 2.0
        var2 = (var2 / 4.0) + (dig_P4 * 65536.0)
        var1 = (dig_P3 * var1 * var1 / 524288.0 + dig_P2 * var1) / 524288.0
        var1 = (1.0 + var1 / 32768.0) * dig_P1
        
        if var1 == 0.0:
            print("❌ HATA: Basınç hesaplamasında sıfıra bölme!")
            return

        pressure_pa = 1048576.0 - adc_P
        pressure_pa = (pressure_pa - (var2 / 4096.0)) * 6250.0 / var1
        var1 = dig_P9 * pressure_pa * pressure_pa / 2147483648.0
        var2 = pressure_pa * dig_P8 / 32768.0
        pressure_pa = pressure_pa + (var1 + var2 + dig_P7) / 16.0
        
        # Ana programdaki potansiyel hatalı satır
        pressure_pascal_buggy = int(pressure_pa * 100)

        # İrtifa hesapla
        altitude = 44330.0 * (1.0 - pow(pressure_pa / 101325.0, 0.1903))

        print("\n" + "="*30)
        print("📊 HESAPLAMA SONUÇLARI")
        print("="*30)
        print(f"🌡️ Sıcaklık          : {temperature:.2f} °C")
        print(f"📉 Basınç (Doğru)    : {pressure_pa:.2f} Pa")
        print(f"📈 Basınç (Doğru hPa): {pressure_pa / 100:.2f} hPa")
        print(f"❌ Basınç (Hatalı?)  : {pressure_pascal_buggy} Pa (Ana koddaki gibi 100 ile çarpılmış)")
        print(f"⛰️ İrtifa            : {altitude:.2f} m")
        print("="*30)

    except FileNotFoundError:
        print("❌ HATA: I2C bus 1 bulunamadı. `sudo raspi-config` ile I2C'yi etkinleştirdiğinizden emin olun.")
    except Exception as e:
        print(f"❌ BEKLENMEDİK HATA: {e}")
        print(traceback.format_exc())

if __name__ == "__main__":
    read_bmp280_raw()
