# 🛰️ TÜRKSAT Model Uydu - Service Kullanım Kılavuzu

## 📋 Service Özellikleri

### 🔧 **Analiz6 Optimizasyonları Uygulandı:**
- ✅ XBee port otomatik tespiti
- ✅ Sonsuz döngü sistemi (erken bitme sorunu çözüldü)
- ✅ Robust exception handling
- ✅ Program çalışma süresi tracking
- ✅ Video sistemi dual-quality optimizasyonu

### 🖥️ **Sistem Özellikleri:**
- **Platform:** Raspberry Pi 2W optimize
- **Memory Limit:** 350MB (%68 RAM kullanımı)
- **CPU Limit:** %85 kullanım (%15 sistem rezervi)
- **Auto-restart:** Crash durumunda otomatik yeniden başlatma
- **Watchdog:** 2 dakika yanıt kontrolü
- **Security:** Sertleştirilmiş güvenlik ayarları

---

## 🚀 Kurulum

### 1. Service Kurulumu
```bash
cd /home/atugem/TurkSatModelUydu/GorevYukuPi

# Script'leri çalıştırılabilir yap
chmod +x service_install.sh
chmod +x service_control.sh

# Service'i kur
./service_install.sh
```

### 2. İlk Başlatma
```bash
# Service'i başlat
sudo systemctl start gorevyuku

# Durumu kontrol et
systemctl status gorevyuku

# Real-time log izle
journalctl -u gorevyuku -f
```

---

## 🎮 Service Yönetimi

### Pratik Kontrol Scripti
```bash
# Service durumu
./service_control.sh status

# Service başlat
./service_control.sh start

# Service durdur  
./service_control.sh stop

# Service yeniden başlat
./service_control.sh restart

# Real-time logları izle
./service_control.sh follow

# Sistem bilgileri
./service_control.sh info
```

### Manuel Systemctl Komutları
```bash
# Service işlemleri
sudo systemctl start gorevyuku      # Başlat
sudo systemctl stop gorevyuku       # Durdur
sudo systemctl restart gorevyuku    # Yeniden başlat
sudo systemctl reload gorevyuku     # Konfigürasyon yenile

# Service durumu
systemctl status gorevyuku          # Detaylı durum
systemctl is-active gorevyuku       # Aktif mi?
systemctl is-enabled gorevyuku      # Boot'ta başlar mı?

# Boot ayarları
sudo systemctl enable gorevyuku     # Boot'ta otomatik başlat
sudo systemctl disable gorevyuku    # Boot'ta başlatma
```

---

## 📝 Log Yönetimi

### Log Görüntüleme
```bash
# Son 50 log satırı
journalctl -u gorevyuku -n 50

# Real-time log izleme
journalctl -u gorevyuku -f

# Belirli tarih aralığı
journalctl -u gorevyuku --since "2024-01-01" --until "2024-01-02"

# Hata seviyesi filtreleme
journalctl -u gorevyuku -p err

# JSON formatında
journalctl -u gorevyuku -o json
```

### Log Analizi
```bash
# Service yeniden başlatma sayısı
journalctl -u gorevyuku | grep -c "Started TURKSAT"

# Hata sayısı
journalctl -u gorevyuku | grep -c "ERROR"

# Son başlatma zamanı
journalctl -u gorevyuku | grep "Started TURKSAT" | tail -1
```

---

## 🔧 Troubleshooting

### Yaygın Sorunlar ve Çözümleri

#### 1. Service başlamıyor
```bash
# Detaylı durum kontrolü
systemctl status gorevyuku -l

# Service dosyası syntax kontrolü
sudo systemd-analyze verify /etc/systemd/system/gorevyuku.service

# Python path kontrolü
/home/atugem/TurkSatModelUydu/GorevYukuPi/venv/bin/python --version
```

#### 2. Permission denied hataları
```bash
# Kullanıcı gruplarını kontrol et
groups atugem

# Gerekli grupları ekle
sudo usermod -a -G dialout,gpio,i2c,spi,video,audio atugem

# Yeniden giriş yap
exit
```

#### 3. XBee port bulunamıyor
```bash
# USB portları listele
ls -la /dev/ttyUSB* /dev/ttyACM*

# Port izinlerini kontrol et
ls -la /dev/ttyUSB0

# Manual port testi
python3 -c "
from moduller.yapilandirma import detect_xbee_port
print('Port:', detect_xbee_port())
"
```

#### 4. Kamera hatası
```bash
# Kamera device kontrolü
ls -la /dev/video*

# Kamera process'leri temizle
sudo pkill -f libcamera
sudo pkill -f rpicam

# Kamera modülü yeniden yükle
sudo modprobe -r bcm2835_v4l2
sudo modprobe bcm2835_v4l2
```

#### 5. Memory/CPU aşırı kullanım
```bash
# Resource kullanımı
systemctl status gorevyuku

# Process detayları
ps aux | grep python

# Memory analizi
cat /proc/meminfo | grep Available
```

---

## 📊 İzleme ve Metrikler

### Sistem Sağlığı
```bash
# CPU sıcaklığı
cat /sys/class/thermal/thermal_zone0/temp

# Memory kullanımı
free -h

# Disk kullanımı
df -h

# Uptime
uptime
```

### Service Metrikleri
```bash
# Service çalışma süresi
systemctl show gorevyuku --property=ActiveEnterTimestamp

# Restart sayısı
systemctl show gorevyuku --property=NRestarts

# Memory kullanımı
systemctl show gorevyuku --property=MemoryCurrent

# CPU kullanımı
systemctl show gorevyuku --property=CPUUsageNSec
```

---

## 🔒 Güvenlik

### Service İzinleri
Service şu güvenlik önlemleri ile çalışır:
- ✅ NoNewPrivileges (yeni ayrıcalık kazanımı yasak)
- ✅ ProtectSystem=strict (sistem dosyaları korumalı)
- ✅ ProtectHome=read-only (home dizini salt okunur)
- ✅ Device allowlist (sadece gerekli cihazlara erişim)

### Log Güvenliği
```bash
# Log dosyası izinleri
ls -la /var/log/journal/*/system.journal

# Journal boyutu kontrolü
journalctl --disk-usage
```

---

## 🚨 Acil Durum

### Hızlı Durdurma
```bash
# Immediate stop
sudo systemctl kill gorevyuku

# Force kill (son çare)
sudo systemctl kill -s SIGKILL gorevyuku
```

### Service Kaldırma
```bash
# Service'i durdur ve devre dışı bırak
sudo systemctl stop gorevyuku
sudo systemctl disable gorevyuku

# Service dosyasını sil
sudo rm /etc/systemd/system/gorevyuku.service

# Systemd reload
sudo systemctl daemon-reload
```

---

## 📞 Destek

### Debug Bilgisi Toplama
```bash
# Sistem bilgisi paketi oluştur
{
    echo "=== SYSTEM INFO ==="
    uname -a
    cat /etc/os-release
    
    echo "=== SERVICE STATUS ==="
    systemctl status gorevyuku
    
    echo "=== RECENT LOGS ==="
    journalctl -u gorevyuku -n 100
    
    echo "=== HARDWARE ==="
    lsusb
    ls -la /dev/video* /dev/tty*
    
    echo "=== RESOURCES ==="
    free -h
    df -h
    cat /sys/class/thermal/thermal_zone0/temp
    
} > debug_info_$(date +%Y%m%d_%H%M%S).txt
```

Bu dosya TÜRKSAT Model Uydu Yarışması Görev Yükü v6.0 için hazırlanmıştır.
Analiz6 optimizasyonları dahil olmak üzere tüm sistem özellikleri active edilmiştir.