# -*- coding: utf-8 -*-
"""
SD Kart Kayıt Yönetici Modülü

Bu modül, telemetri verilerinin ve video kayıtlarının SD karta yazılmasından,
dosya isimlerinin yönetiminden ve kayıt klasörlerinin oluşturulmasından sorumludur.
"""

import os
import time
import shutil
from datetime import datetime

class SDKayitci:
    def __init__(self, kayit_ana_klasoru="kayitlar"):
        """SD Kart kayıtçısı başlatma"""
        self.kayit_ana_klasoru = kayit_ana_klasoru
        
        # Disk alanı izleme ayarları (64GB SD kart için)
        # VERİLER ASLA SİLİNMEZ - sadece izleme amaçlı
        self.low_space_warning_gb = 2  # 2GB altında uyarı
        self.critical_space_warning_mb = 500  # 500MB altında ciddi uyarı
        
        # Zaman damgası oluştur
        zaman_damgasi = datetime.now().strftime("%Y%m%d_%H%M%S")
        
        # Ana klasörü oluştur
        self._ensure_directory_exists(self.kayit_ana_klasoru)
        
        # Dosya yollarını belirle
        self.telemetri_dosya_yolu = os.path.join(
            self.kayit_ana_klasoru, 
            f"telemetri_{zaman_damgasi}.csv"
        )
        
        self.video_dosya_yolu = os.path.join(
            self.kayit_ana_klasoru, 
            f"video_{zaman_damgasi}.mp4"  # MP4 format (H.264 uyumlu)
        )
        
        # İlk disk alanı kontrolü
        self._check_disk_space(show_info=True)
        
        # Telemetri dosyasını başlat
        self._init_telemetri_file()
        
        print(f"✅ SD Kayıtçı başlatıldı")
        print(f"📄 Telemetri: {self.telemetri_dosya_yolu}")
        print(f"🎥 Video: {self.video_dosya_yolu}")
    
    def _check_disk_space(self, show_info=False):
        """
        Disk alanı kontrolü (Gereksinim 16, 17 - SD kart izleme)
        64GB SD kart için - VERİLER ASLA SİLİNMEZ
        """
        try:
            total, used, free = shutil.disk_usage(self.kayit_ana_klasoru)
            
            # MB cinsine çevir
            free_mb = free // (1024 * 1024)
            total_mb = total // (1024 * 1024)
            used_mb = used // (1024 * 1024)
            
            if show_info:
                print(f"💾 Disk Durumu: {free_mb}MB boş / {total_mb}MB toplam ({used_mb}MB kullanılan)")
            
            # Sadece bilgilendirme amaçlı uyarılar (veri silinmez)
            if free_mb < 500:  # 500MB altında ciddi uyarı
                print(f"🚨 UYARI: SD kart dolmaya yakın! Sadece {free_mb}MB kaldı")
                print(f"📊 Toplam {total_mb}MB kartın %{(used_mb/total_mb)*100:.1f}'si dolu")
            elif free_mb < 2000:  # 2GB altında erken uyarı
                print(f"⚠️ BİLGİ: SD kart kullanımı artıyor. {free_mb}MB boş alan kaldı")
            
            # Disk dolu olsa bile kayda devam et (64GB yeterli olmalı)
            return True
            
        except Exception as e:
            print(f"⚠️ Disk alanı kontrolü yapılamadı: {e}")
            return True  # Hata durumunda devam et
    
    def _cleanup_old_files(self):
        """
        VERİ KORUMA: Bu metod artık hiçbir dosya silmez!
        Sadece disk durumu raporu verir.
        """
        print("📊 Disk durumu raporu oluşturuluyor...")
        
        try:
            # Kayıt klasöründeki dosyaları analiz et (SADECE BİLGİ İÇİN)
            files = []
            total_size = 0
            
            for file in os.listdir(self.kayit_ana_klasoru):
                file_path = os.path.join(self.kayit_ana_klasoru, file)
                if os.path.isfile(file_path):
                    stat = os.stat(file_path)
                    files.append((file_path, stat.st_mtime, stat.st_size))
                    total_size += stat.st_size
            
            # Dosyaları yaşa göre sırala (bilgi amaçlı)
            files.sort(key=lambda x: x[1])
            
            print(f"📄 Toplam dosya sayısı: {len(files)}")
            print(f"💾 Toplam veri boyutu: {total_size//1024//1024}MB")
            
            # En eski ve en yeni dosyaları göster
            if files:
                oldest_file = files[0]
                newest_file = files[-1]
                
                oldest_date = time.strftime("%Y-%m-%d %H:%M:%S", time.localtime(oldest_file[1]))
                newest_date = time.strftime("%Y-%m-%d %H:%M:%S", time.localtime(newest_file[1]))
                
                print(f"📅 En eski dosya: {os.path.basename(oldest_file[0])} ({oldest_date})")
                print(f"📅 En yeni dosya: {os.path.basename(newest_file[0])} ({newest_date})")
            
            print("✅ Tüm veriler korunuyor - hiçbir dosya silinmedi")
            
        except Exception as e:
            print(f"⚠️ Disk analizi hatası: {e}")

    def _ensure_directory_exists(self, directory_path):
        """
        Verilen klasörün var olup olmadığını kontrol eder ve yoksa oluşturur.
        """
        if not os.path.exists(directory_path):
            try:
                os.makedirs(directory_path)
                print(f"�� Klasör oluşturuldu: {directory_path}")
            except OSError as e:
                print(f"HATA: Klasör oluşturulamadı {directory_path}: {e}")
        else:
            print(f"📁 Klasör zaten var: {directory_path}")

    def _init_telemetri_file(self):
        """
        Telemetri dosyasını başlatır ve başlık satırını yazar.
        """
        try:
            # Telemetri dosyasını aç ve başlık satırını yaz
            self.telemetri_dosyasi = open(self.telemetri_dosya_yolu, 'w', encoding='utf-8')
            # Şartnameye uygun başlık satırı
            baslik = ("PAKET_NUMARASI,UYDU_STATUSU,HATA_KODU,GONDERME_SAATI,"
                      "BASINC1,BASINC2,YUKSEKLIK1,YUKSEKLIK2,IRTIFA_FARKI,"
                      "INIS_HIZI,SICAKLIK,PIL_GERILIMI,GPS1_LAT,GPS1_LONG,GPS1_ALT,"
                      "PITCH,ROLL,YAW,RHRH_KOMUT,IOT_S1,IOT_S2,TAKIM_NO\n")
            self.telemetri_dosyasi.write(baslik)
            self.telemetri_dosyasi.flush() # Verinin hemen diske yazılmasını sağla
        except Exception as e:
            print(f"HATA: Telemetri dosyası başlatılırken hata oluştu: {e}")
            self.telemetri_dosyasi = None

    def kaydet_telemetri(self, telemetri_paketi):
        """
        Telemetri paketini SD karta kaydeder (Gereksinim 16)
        64GB SD kart - VERİLER ASLA SİLİNMEZ
        """
        if not self.telemetri_dosyasi:
            print("⚠️ Telemetri dosyası açık değil")
            return False
        
        # Disk alanı izleme (her 100 kayıtta bir - sadece bilgi amaçlı)
        if hasattr(self, '_write_counter'):
            self._write_counter += 1
        else:
            self._write_counter = 1
        
        if self._write_counter % 100 == 0:  # Her 100 kayıtta durum raporu
            print(f"📊 {self._write_counter} telemetri paketi kaydedildi")
            self._check_disk_space(show_info=True)
        
        try:
            # Telemetri paketini kaydet (Dictionary veya string desteği)
            if isinstance(telemetri_paketi, dict):
                veri = telemetri_paketi.get("ham_veri", str(telemetri_paketi))
            else:
                veri = str(telemetri_paketi)
            
            self.telemetri_dosyasi.write(veri.strip() + '\n')
            self.telemetri_dosyasi.flush()  # Verinin hemen yazılmasını sağla
            return True
        except Exception as e:
            print(f"❌ Telemetri kayıt hatası: {e}")
            return False

    def get_video_kayit_yolu(self):
        """Oluşturulan video kayıt yolunu döndürür."""
        return self.video_dosya_yolu

    def temizle(self):
        """Tüm açık dosyaları kapatır."""
        if self.telemetri_dosyasi:
            try:
                self.telemetri_dosyasi.close()
                print("Telemetri kayıt dosyası kapatıldı.")
            except Exception as e:
                print(f"HATA: Telemetri dosyası kapatılırken hata: {e}")
        print("SD Kayıtçı temizlendi.")

if __name__ == '__main__':
    # Modülün tek başına test edilmesi için örnek kod
    print("SD Kayıtçı Modülü Testi")
    
    sd_kayitci = SDKayitci(kayit_ana_klasoru="test_kayitlari")
    
    # Video yolu alma testi
    video_yolu = sd_kayitci.get_video_kayit_yolu()
    print(f"Alınan video yolu: {video_yolu}")
    assert "video_" in video_yolu

    # Telemetri kaydetme testi
    ornek_paket = "1,0,000000,28/07/2024 15:30:00,1013.25,0,0,0,0,0,25.0,7.4,0,0,0,0,0,0,,0,0,286570"
    sd_kayitci.kaydet_telemetri(ornek_paket)
    print("Örnek telemetri paketi kaydedildi.")
    
    sd_kayitci.temizle()
    
    # Dosyanın içeriğini kontrol et
    try:
        with open(sd_kayitci.telemetri_dosya_yolu, 'r') as f:
            lines = f.readlines()
            print(f"'{sd_kayitci.telemetri_dosya_yolu}' dosyasında {len(lines)} satır bulundu.")
            assert len(lines) == 2 # Başlık + 1 veri satırı
            assert "TAKIM_NO" in lines[0]
            assert ornek_paket in lines[1]
        print("Dosya içeriği doğrulandı.")
    except Exception as e:
        print(f"Dosya doğrulama hatası: {e}")
        
    print("\nTest tamamlandı.")
