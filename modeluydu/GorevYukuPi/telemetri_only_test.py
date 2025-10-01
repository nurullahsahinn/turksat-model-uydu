#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""
Sadece Telemetri Test - XBee'siz, sadece SD kart kaydı

Bu script ana programın telemetri kısmını XBee olmadan test eder.
"""

import os
import sys
import time
import threading
import queue

# Proje yolunu ekle
sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))

def telemetry_only_test():
    """Sadece telemetri ve SD kayıt testi"""
    print("=== XBee'SİZ TELEMETRİ SİSTEMİ TEST ===")
    
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
        
        print("2. Bileşenler başlatılıyor...")
        saha = SahteSAHA()
        sensor_manager = SensorManager(saha_alici_instance=saha, simulate=True)
        telemetri_handler = TelemetryHandler(saha_alici=saha)
        sd_kayitci = SDKayitci(kayit_ana_klasoru="telemetri_only_test")
        
        # Queue ve thread hazırlama
        telemetry_queue = queue.Queue()
        stop_event = threading.Event()
        
        def telemetry_worker():
            """Telemetri üretici thread"""
            print("🔄 Telemetri worker başlatıldı")
            paket_sayisi = 0
            
            while not stop_event.is_set() and paket_sayisi < 10:  # Max 10 paket
                try:
                    paket_sayisi += 1
                    print(f"\n--- Paket #{paket_sayisi} ---")
                    
                    # Sensör verisi al
                    sensor_data = sensor_manager.oku_tum_sensorler()
                    print(f"✅ Sensör verisi alındı")
                    
                    # Telemetri paketi oluştur
                    telemetri_paketi = telemetri_handler.olustur_telemetri_paketi(sensor_data)
                    print(f"✅ Telemetri paketi oluşturuldu: {len(telemetri_paketi)} karakter")
                    
                    # Queue'ya ekle
                    telemetry_queue.put(telemetri_paketi)
                    print(f"✅ Queue'ya eklendi: {telemetri_paketi[:50]}...")
                    
                    time.sleep(2)  # 2 saniyede bir paket
                    
                except Exception as e:
                    print(f"❌ Telemetri worker hatası: {e}")
                    
            print("🔄 Telemetri worker tamamlandı")
        
        def sd_worker():
            """SD kayıt thread"""
            print("💾 SD worker başlatıldı")
            kayit_sayisi = 0
            
            while not stop_event.is_set():
                try:
                    # Queue'dan al (timeout ile)
                    try:
                        telemetri_paketi = telemetry_queue.get(timeout=1)
                        kayit_sayisi += 1
                        
                        print(f"💾 SD Worker: Paket #{kayit_sayisi} alındı")
                        
                        # SD'ye kaydet
                        basarili = sd_kayitci.kaydet_telemetri(telemetri_paketi)
                        if basarili:
                            print(f"✅ SD'YE KAYDEDİLDİ #{kayit_sayisi}: {telemetri_paketi[:50]}...")
                        else:
                            print(f"❌ SD KAYIT HATASI #{kayit_sayisi}")
                        
                        telemetry_queue.task_done()
                        
                    except queue.Empty:
                        continue
                        
                except Exception as e:
                    print(f"❌ SD worker hatası: {e}")
                    
            print(f"💾 SD worker tamamlandı - {kayit_sayisi} kayıt")
        
        # Thread'leri başlat
        print("3. Thread'ler başlatılıyor...")
        telemetry_thread = threading.Thread(target=telemetry_worker, name="TelemetryWorker")
        sd_thread = threading.Thread(target=sd_worker, name="SDWorker")
        
        telemetry_thread.start()
        sd_thread.start()
        
        print("4. 25 saniye test çalışacak...")
        time.sleep(25)
        
        # Thread'leri durdur
        print("5. Thread'ler durduruluyor...")
        stop_event.set()
        
        telemetry_thread.join(timeout=5)
        sd_thread.join(timeout=5)
        
        # Dosya kontrolü
        if os.path.exists(sd_kayitci.telemetri_dosya_yolu):
            with open(sd_kayitci.telemetri_dosya_yolu, 'r', encoding='utf-8') as f:
                lines = f.readlines()
                print(f"\n6. SONUÇ: {len(lines)} satır yazıldı")
                
                if len(lines) > 1:  # Header + data
                    print(f"✅ BAŞARILI: Telemetri sistemi çalışıyor!")
                    print(f"📁 Dosya: {sd_kayitci.telemetri_dosya_yolu}")
                    print(f"📊 Veri satırları: {len(lines)-1}")
                    print("Son 3 satır:")
                    for line in lines[-3:]:
                        print(f"  {line.strip()}")
                    return True
                else:
                    print("❌ BAŞARISIZ: Sadece başlık var, veri yok")
                    return False
        else:
            print("❌ BAŞARISIZ: Dosya oluşturulamadı")
            return False
        
        sd_kayitci.temizle()
        
    except Exception as e:
        print(f"❌ Test hatası: {e}")
        import traceback
        traceback.print_exc()
        return False

if __name__ == "__main__":
    if telemetry_only_test():
        print("\n🎉 TELEMETRİ SİSTEMİ ÇALIŞIYOR!")
        print("Ana programda XBee serial port sorununu çözdükten sonra tam sistem çalışacak")
    else:
        print("\n❌ Telemetri sistemi sorunu var")