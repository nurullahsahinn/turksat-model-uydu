#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""
TÜRKSAT Telemetri Debug Sistemi
Analiz7'ye göre sorunları tespit eder ve çözer
"""

import os
import time
import csv
from datetime import datetime

def fix_rtc_time():
    """RTC KALDIRILDI - artık sistem zamanı kullanılıyor"""
    print("🕒 RTC Devre Dışı")
    print("=" * 30)
    print("⚠️ RTC tamamen kaldırıldı - sistem zamanı kullanılıyor")
    return True  # Her zaman başarılı

def check_csv_files():
    """CSV dosyalarını kontrol eder"""
    print("\n📄 CSV Dosya Kontrolü")
    print("=" * 30)
    
    kayit_klasoru = "kayitlar"
    if not os.path.exists(kayit_klasoru):
        print(f"❌ Kayıt klasörü bulunamadı: {kayit_klasoru}")
        return False
    
    # En son telemetri dosyasını bul
    telemetri_dosyalari = [f for f in os.listdir(kayit_klasoru) if f.startswith("telemetri_")]
    
    if not telemetri_dosyalari:
        print("❌ Telemetri dosyası bulunamadı")
        return False
    
    # En son dosyayı al
    en_son_dosya = sorted(telemetri_dosyalari)[-1]
    dosya_yolu = os.path.join(kayit_klasoru, en_son_dosya)
    
    print(f"📁 En son telemetri dosyası: {en_son_dosya}")
    
    try:
        with open(dosya_yolu, 'r', encoding='utf-8') as f:
            lines = f.readlines()
        
        print(f"📊 Toplam satır sayısı: {len(lines)}")
        
        if len(lines) <= 1:
            print("⚠️ Sadece başlık satırı var, veri yok!")
            print("🔧 Telemetri thread'i düzgün çalışmıyor")
            return False
        
        print("✅ Veri satırları mevcut")
        
        # Son birkaç satırı göster
        print("\n📋 Son telemetri verileri:")
        for i, line in enumerate(lines[-3:], start=max(1, len(lines)-2)):
            if i > 1:  # Başlık satırını atla
                print(f"  Satır {i}: {line.strip()}")
        
        return True
        
    except Exception as e:
        print(f"❌ Dosya okuma hatası: {e}")
        return False

def create_test_telemetry():
    """Test telemetri paketi oluşturur"""
    print("\n📤 Test Telemetri Paketi")
    print("=" * 30)
    
    # Manuel test paketi
    zaman = datetime.now().strftime("%d/%m/%Y %H:%M:%S")
    test_paketi = f"999,1,000000,{zaman},101325,0,500.0,0.0,0.0,0.0,25.0,3.7,41.0138,28.9497,500.0,0.0,0.0,180.0,0000,25.1,25.2,286570"
    
    print(f"📦 Test paketi: {test_paketi}")
    
    # XBee'ye göndermeyi test et
    try:
        if os.path.exists("/dev/ttyAMA0"):
            with open("/dev/ttyAMA0", "w") as xbee:
                xbee.write(test_paketi + "\n")
            print("✅ Test paketi XBee'ye gönderildi")
        else:
            print("⚠️ XBee portu bulunamadı")
    except Exception as e:
        print(f"❌ XBee gönderim hatası: {e}")

def force_csv_write():
    """Manuel CSV yazma testi"""
    print("\n💾 Manuel CSV Yazma Testi")
    print("=" * 30)
    
    test_dosya = "kayitlar/test_telemetri.csv"
    
    try:
        # Test CSV dosyası oluştur
        with open(test_dosya, 'w', newline='', encoding='utf-8') as csvfile:
            writer = csv.writer(csvfile)
            
            # Başlık satırı
            header = ["paket_no", "uydu_status", "hata_kodu", "gonderme_saati", 
                     "basinc1", "basinc2", "yukseklik1", "yukseklik2", "irtifa_farki",
                     "inis_hizi", "sicaklik", "pil_gerilimi", "gps_lat", "gps_lon", 
                     "gps_alt", "pitch", "roll", "yaw", "rhrh", "iot_s1", "iot_s2", "takim_no"]
            writer.writerow(header)
            
            # Test verisi
            zaman = datetime.now().strftime("%d/%m/%Y %H:%M:%S")
            test_veri = [999, 1, "000000", zaman, 101325, 0, 500.0, 0.0, 0.0, 0.0,
                        25.0, 3.7, 41.0138, 28.9497, 500.0, 0.0, 0.0, 180.0, 
                        "0000", 25.1, 25.2, 286570]
            writer.writerow(test_veri)
        
        print(f"✅ Test CSV dosyası oluşturuldu: {test_dosya}")
        
        # Dosyayı kontrol et
        with open(test_dosya, 'r', encoding='utf-8') as f:
            lines = f.readlines()
        
        print(f"📊 Oluşturulan satır sayısı: {len(lines)}")
        print(f"📋 İçerik: {lines[-1].strip()}")
        
        return True
        
    except Exception as e:
        print(f"❌ CSV yazma hatası: {e}")
        return False

def test_sensor_validation():
    """Sensör validation testini gevşet"""
    print("\n🔬 Sensör Validation Test")
    print("=" * 30)
    
    # BMP280 anormal verilerini test et
    test_basinc = 9754789  # Anormal değer
    test_irtifa = -573.64  # Anormal irtifa
    
    print(f"🧪 Test basınç: {test_basinc} Pa")
    print(f"🧪 Test irtifa: {test_irtifa} m")
    
    # Normal aralıklar
    normal_basinc_min = 85000   # ~1500m irtifa
    normal_basinc_max = 106000  # Deniz seviyesi + biraz
    
    if normal_basinc_min <= test_basinc <= normal_basinc_max:
        print("✅ Basınç normal aralıkta")
    else:
        print("❌ Basınç anormal - validation çok katı!")
        print(f"   Normal aralık: {normal_basinc_min}-{normal_basinc_max} Pa")
        print("   Çözüm: BMP280 sensörü fiziksel kontrol edin")

def main():
    print("🔍 TÜRKSAT Telemetri Debug Sistemi v7.0")
    print("=" * 50)
    print("📋 Analiz7'ye göre sorun tespiti ve çözümü")
    print("")
    
    # 1. RTC zamanını düzelt
    rtc_ok = fix_rtc_time()
    
    # 2. CSV dosyalarını kontrol et
    csv_ok = check_csv_files()
    
    # 3. Manuel CSV yazma testi
    csv_write_ok = force_csv_write()
    
    # 4. Sensör validation test
    test_sensor_validation()
    
    # 5. Test telemetri oluştur
    create_test_telemetry()
    
    print("\n" + "=" * 50)
    print("🎯 SORUN RAPORU:")
    print(f"1. RTC Zamanı: ⚠️ Devre Dışı (Sistem zamanı kullanılıyor)")
    print(f"2. CSV Dosyası: {'✅ Çalışıyor' if csv_ok else '❌ Boş/Yok'}")
    print(f"3. CSV Yazma: {'✅ Çalışıyor' if csv_write_ok else '❌ Sorunlu'}")
    
    print("\n🔧 ÖNERİLEN ADIMLAR:")
    print("1. RTC tamamen kaldırıldı - sistem zamanı kullanılıyor")
    if not csv_ok:
        print("2. Ana programı yeniden başlatın")
    if not csv_write_ok:
        print("3. SD kart izinlerini kontrol edin: sudo chmod 777 kayitlar/")
    
    print("\n📡 XBee KONFIGÜRASYON HATASI:")
    print("❌ Kanal ayarları: C/D/E (YANLIŞ)")
    print("✅ Doğru ayarlar: 12/13/14 (SAYI)")
    print("   XCTU'da CH parametresini 12 yapın (0x0C)")

if __name__ == "__main__":
    main()