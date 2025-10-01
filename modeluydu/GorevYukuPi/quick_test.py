#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""
Quick Test Script - Telemetri sistemini hızlı test et
Bu script sadece önemli bileşenleri test eder
"""

import os
import sys
import time

# Proje yolunu ekle
sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))

def quick_test():
    """Hızlı telemetri test"""
    print("=== HIZLI TELEMETRI TEST ===")
    
    try:
        # Sahte SAHA alıcı
        class SahteSAHA:
            def get_tasiyici_basinici(self): 
                return 1015.0
            def is_basinc2_available(self): 
                return False
            def get_basinc2_value(self): 
                return 1015.0
        
        print("1. Modülleri yükleniyor...")
        from moduller.sensorler import SensorManager
        from moduller.telemetri_isleyici import TelemetryHandler
        from moduller.sd_kayitci import SDKayitci
        print("   Başarılı!")
        
        print("2. Bileşenler başlatılıyor...")
        saha = SahteSAHA()
        sensor_manager = SensorManager(saha_alici_instance=saha, simulate=False)
        telemetri_handler = TelemetryHandler(saha_alici=saha)
        sd_kayitci = SDKayitci(kayit_ana_klasoru="quick_test_kayitlar")
        print("   Başarılı!")
        
        print("3. 3 telemetri paketi test ediliyor...")
        for i in range(3):
            # Sensör verisi al
            sensor_data = sensor_manager.oku_tum_sensorler()
            
            # Telemetri paketi oluştur
            telemetri_paketi = telemetri_handler.olustur_telemetri_paketi(sensor_data)
            
            # SD'ye kaydet
            basarili = sd_kayitci.kaydet_telemetri(telemetri_paketi)
            
            if basarili:
                if isinstance(telemetri_paketi, dict):
                    veri_onizleme = str(telemetri_paketi.get("ham_veri", telemetri_paketi))[:50]
                else:
                    veri_onizleme = str(telemetri_paketi)[:50]
                print(f"   Paket {i+1}: OK - {veri_onizleme}...")
            else:
                print(f"   Paket {i+1}: HATA")
                return False
                
            time.sleep(0.5)
        
        # Dosya kontrolü
        if os.path.exists(sd_kayitci.telemetri_dosya_yolu):
            with open(sd_kayitci.telemetri_dosya_yolu, 'r', encoding='utf-8') as f:
                lines = f.readlines()
                print(f"4. Dosya kontrolü: {len(lines)} satır yazıldı")
                
                if len(lines) >= 4:  # Header + 3 data
                    print("   Son satır:", lines[-1].strip())
                    print("\n*** TELEMETRI SISTEMI ÇALIŞIYOR! ***")
                    print(f"Dosya: {sd_kayitci.telemetri_dosya_yolu}")
                    return True
                else:
                    print("   Veri eksik!")
                    return False
        else:
            print("4. Dosya oluşturulamadı!")
            return False
        
        sd_kayitci.temizle()
        
    except Exception as e:
        print(f"HATA: {e}")
        import traceback
        traceback.print_exc()
        return False

if __name__ == "__main__":
    if quick_test():
        print("\n✅ Sistem BAŞARILI - ana_program.py çalıştırabilirsiniz")
        print("Raspberry Pi'de çalıştırmak için:")
        print("python3 ana_program.py")
    else:
        print("\n❌ Sistem HATALI - problemleri çözün")