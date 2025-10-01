# 🛰️ TÜRKSAT Model Uydu Yarışması 2025 - Görev Yükü Sistemi

<div align="center">

![TÜRKSAT Logo](https://img.shields.io/badge/TÜRKSAT-Model%20Uydu%20Yarışması-red?style=for-the-badge)
![Teknofest](https://img.shields.io/badge/TEKNOFEST-2025-orange?style=for-the-badge)
![Takım](https://img.shields.io/badge/Takım-NONGRAVITY%20(286570)-blue?style=for-the-badge)
![Platform](https://img.shields.io/badge/Platform-Raspberry%20Pi%20Zero%202W-green?style=for-the-badge)
![Durum](https://img.shields.io/badge/Durum-Uçuşa%20Hazır-success?style=for-the-badge)

[▶️ Model Uydu Tanıtım Videosu (LinkedIn)](https://www.linkedin.com/posts/nurullahsahinn_taesrksat-teknofest-modeluydu-activity-7361415194155040769-peXQ?utm_source=share&utm_medium=member_desktop&rcm=ACoAADk6ykIBxQMI8iO9VxnYcCEeqIe3bIY4YS8)

**Sıfırdan geliştirilen, uçtan uca model uydu yazılım mimarisi**  
*Tam otomatik telemetri, video akışı, IoT haberleşme ve ayrılma kontrolü*

[Özellikler](#-özellikler) • [Takım](#-takım-hakkında) • [Sistem Mimarisi](#-sistem-mimarisi) • [Kurulum](#-kurulum) • [Kazanımlar](#-kazanımlarımız)

---

### 🏆 Teknofest 2025 TÜRKSAT Model Uydu Yarışması

*"Sınırları zorlayan bir mühendislik sürecini ekip ruhuyla başarmak"*

</div>

---

## 📋 İçindekiler

- [Genel Bakış](#-genel-bakış)
- [Takım Hakkında](#-takım-hakkında)
- [Özellikler](#-özellikler)
- [Sistem Mimarisi](#-sistem-mimarisi)
- [Donanım Listesi](#-donanım-listesi)
- [Yazılım Mimarisi](#-yazılım-mimarisi)
- [Kurulum](#-kurulum)
  - [Raspberry Pi Kurulumu](#1-raspberry-pi-kurulumu)
  - [Arduino Kurulumu](#2-arduino-kurulumu)
  - [Yer İstasyonu Kurulumu](#3-yer-i̇stasyonu-kurulumu)
- [Kullanım](#-kullanım)
- [Proje Yapısı](#-proje-yapısı)
- [Protokoller ve Standartlar](#-protokoller-ve-standartlar)
- [Test ve Kalibrasyon](#-test-ve-kalibrasyon)
- [Sorun Giderme](#-sorun-giderme)
- [Kazanımlarımız](#-kazanımlarımız)
- [Geliştiriciler İçin](#-geliştiriciler-i̇çin)
- [Lisans](#-lisans)
- [İletişim](#-i̇letişim)

---

## 🌟 Genel Bakış

Bu proje, **TÜRKSAT Model Uydu Yarışması 2025** için geliştirilmiş tam otomatik bir görev yükü sistemidir. Sistem, balon ile 700m yüksekliğe çıkartılan model uydudan ayrılarak kendi paraşütüyle inerken gerçek zamanlı telemetri, video ve IoT verilerini yer istasyonuna iletir.

### 🎯 Proje Hedefleri

- ✅ **1 Hz frekansında** kesintisiz telemetri gönderimi
- ✅ **Gerçek zamanlı video akışı** (240x180@3fps) XBee üzerinden
- ✅ **SD karta yüksek kaliteli video kaydı** (640x480@15fps H.264/MP4)
- ✅ **IoT S2S (Station-to-Satellite)** bonus görevi - 412-707m mesafe
- ✅ **SAHA (Sistemler Arası Haberleşme Ağı)** protokolü
- ✅ **ARAS (Arayüz Alarm Sistemi)** ile hata izleme
- ✅ **Multi-spektral filtreleme** (opsiyonel)
- ✅ **Otomatik ayrılma** @ 400m
- ✅ **Kurtarma modu** buzzer ile konum bildirimi

### 🏆 Yarışma Gereksinimleri

Sistem, TÜRKSAT şartnamesinin tüm gereksinimlerini karşılar:

| Gereksinim | Durum | Açıklama |
|-----------|--------|----------|
| Telemetri (1 Hz) | ✅ | 30 alan içeren paket formatı |
| GPS Konum | ✅ | UbloxNeo-8M (2.5m hassasiyet) |
| Basınç/İrtifa | ✅ | BMP280 (±1m hassasiyet) |
| IMU Verileri | ✅ | 10-DOF (İvme, Gyro, Manyetik) |
| Video Kayıt | ✅ | H.264/MP4 (VLC uyumlu) |
| Video Akışı | ✅ | MJPEG XBee üzerinden |
| Otomatik Ayrılma | ✅ | Servo motor @ 400m |
| Kurtarma Sinyali | ✅ | Buzzer @ <10m |
| SAHA Protokolü | ✅ | Taşıyıcı basınç verisi |
| IoT S2S | ✅ | 2 istasyon → Görev yükü |
| ARAS Alarm | ✅ | 6-bit hata kodu sistemi |

---

## 👥 Takım Hakkında

### **NON GRAVITY - Takım 286570**

**TEKNOFEST 2025 TÜRKSAT Model Uydu Yarışması** için bir araya gelen multidisipliner bir ekibiz. Projemiz, sıfırdan geliştirilen **uçtan uca model uydu yazılım mimarisi** ile sınırları zorlayan bir mühendislik sürecinin ürünüdür.

#### 🎯 Misyonumuz
*"Sınırları zorlayan bir mühendislik sürecini ekip ruhuyla başarmak"*

#### 💡 Ekip Ruhu
- **Disiplinler Arası İşbirliği:** Yazılım, donanım, mekanik ve elektronik mühendisliği
- **Problem Çözme Odaklı:** Her zorluk için yaratıcı ve pratik çözümler
- **Öğrenme ve Gelişim:** Sürekli iyileştirme ve yeni teknolojilere adaptasyon
- **Şartnameye Uyum:** Detaylara özen ve standartlara bağlılık

#### 🏗️ Geliştirme Süreci
Bu proje kapsamında:
- ✅ **Sıfırdan tasarım:** Tüm yazılım mimarisi özgün geliştirildi
- ✅ **Hata toleranslı sistem:** Raspberry Pi resetlense bile veri kaybı yok
- ✅ **Gerçek zamanlı işleme:** Çok iş parçacıklı mimari ile 1 Hz telemetri
- ✅ **Platformlar arası entegrasyon:** Python, C#, Arduino uyumlu çalışıyor
- ✅ **Operasyonel esneklik:** Manuel yedek komutlar ve acil durum modları

#### 📡 Teknik Altyapı Özellikleri
| Özellik | Açıklama |
|---------|----------|
| **Çok İş Parçacıklı Mimari** | Telemetri, video, veri kayıt paralel çalışıyor |
| **SAHA Protokolü** | Taşıyıcıdan basınç verisi alma |
| **IoT S2S** | Yerdeki 2 IoT istasyonundan sıcaklık iletimi |
| **Multi-Spektral Mekanik Filtreleme** | Çift servo kontrolü ile filtre değiştirme |
| **ARAS Alarm Sistemi** | 6 kritik metriği anlık izleme, görsel/sesli uyarılar |
| **Hata Toleranslı Yapı** | Paket numarası ve zaman korunması (servis tabanlı) |
| **Standartlaştırılmış İletişim** | XBee 802.15.4 - kararlı RF altyapısı |

---

## 🚀 Özellikler

### 🎛️ Görev Yükü (Raspberry Pi Zero 2W)

#### 📡 Telemetri Sistemi
- **1 Hz** frekansında kesintisiz veri gönderimi
- **30 alan** içeren zengin telemetri paketi
- XOR checksum ile veri bütünlüğü
- Paket numarası ve görev zamanı kalıcı saklama
- SD karta otomatik yedekleme

#### 📹 Video Sistemi (İKİLİ SİSTEM)
1. **SD Kart Kaydı** - Tam Kalite
   - Çözünürlük: 640x480 @ 15 FPS
   - Format: H.264/MP4 (VLC/tüm oynatıcılara uyumlu)
   - Bitrate: 2 Mbps
   - Sürekli kayıt (uçuş boyunca)

2. **XBee Canlı Yayın** - Optimize
   - Çözünürlük: 240x180 @ 2 FPS
   - Format: MJPEG
   - Frame boyutu: ~3KB (DEADBEEF/CAFEBABE frame marking)
   - Bandwidth: ~48 Kbps (%22 kullanım)

#### 🛰️ Sensörler
- **BMP280** - Basınç/Sıcaklık (I2C 0x76)
- **10-DOF IMU** - İvme, Gyro, Manyetometre (ADXL345, ITG3200, HMC5883L)
- **UbloxNeo-8M GPS** - Konum (UART)
- **ADS1115** - 16-bit ADC (Pil seviyesi)
- **DS3231 RTC** - Gerçek zamanlı saat (opsiyonel)

#### ⚙️ Aktuatörler
- **2x SG90 Servo** - Multi-spektral filtreleme (opsiyonel)
- **Buzzer** - Kurtarma sinyali

#### 🔋 Güç Yönetimi
- NCR18650B Li-Ion pil izleme
- Düşük pil uyarısı
- Güvenli kapanma sistemi
- LM2596/LM2577 voltaj regülatörleri

### 🏗️ Taşıyıcı Sistem (Arduino Nano)

- **BMP280** basınç sensörü (I2C 0x77)
- **SG90 Servo** ayrılma mekanizması
- **XBee** SAHA protokolü haberleşmesi
- **Buzzer** kurtarma sinyali
- Otomatik ayrılma @ 400m
- Ayrılma sonrası taşıyıcı basıncını görev yüküne gönderir

### 📟 IoT İstasyonları

#### İstasyon #1 (Arduino Nano)
- **BMP280** sıcaklık sensörü (I2C 0x76)
- **XBee 3 Pro** (MY=0x0004, Kanal 12)
- SoftwareSerial (Pin 2/3)
- 412-707m mesafe iletişimi

#### İstasyon #2 (Arduino Mega 2560)
- **BMP280** sıcaklık sensörü (I2C 0x77)
- **XBee 3 Pro** (MY=0x0005, Kanal 13)
- Hardware Serial1 (Pin 18/19) - daha stabil
- 412-707m mesafe iletişimi

### 💻 Yer İstasyonu (C# Windows Forms)

#### Gerçek Zamanlı İzleme
- **Telemetri grafikleri** (basınç, yükseklik, sıcaklık, hız)
- **3D uydu görselleştirmesi** (OpenGL)
- **Harita üzerinde konum** (GMap.NET)
- **Canlı video akışı** (MJPEG decoder)

#### Veri Yönetimi
- SD kart simülasyonu (gerçek sistem uyumlu)
- CSV export (Excel uyumlu)
- Otomatik yedekleme
- Telemetri replay (analiz için)

#### Komut Gönderme
- Manuel ayrılma komutu
- Multi-spektral filtreleme kontrolleri (opsiyonel)
- Kalibrasyon komutları (gyro, basınç)

#### Alarm Sistemi (ARAS)
- 6-bit hata kodu görselleştirmesi
- Gerçek zamanlı alarm bildirimleri
- Hata geçmişi kaydı

---

## 🏗️ Sistem Mimarisi

```
┌─────────────────────────────────────────────────────────────────┐
│                         TÜRKSAT MODEL UYDU                       │
│                          Görev Yükü Sistemi                      │
└─────────────────────────────────────────────────────────────────┘

         ┌──────────────────────────────────────────┐
         │    Raspberry Pi Zero 2W (Görev Yükü)    │
         │  ┌────────────────────────────────────┐  │
         │  │  ana_program.py (Ana Kontrol)      │  │
         │  │  ├─ Sensör Yönetimi                │  │
         │  │  ├─ Telemetri İşleyici             │  │
         │  │  ├─ Kamera Yöneticisi              │  │
         │  │  ├─ XBee Haberleşme (Birleşik)     │  │
         │  │  ├─ SD Kayıtçı                     │  │
         │  │  └─ Güç Yöneticisi                 │  │
         │  └────────────────────────────────────┘  │
         │                                           │
         │  Sensörler:                               │
         │  • BMP280 (I2C)                          │
         │  • 10-DOF IMU (I2C)                      │
         │  • GPS (UART)                            │
         │  • Kamera (CSI)                          │
         │  • ADS1115 ADC (I2C)                     │
         │                                           │
         │  XBee 3 Pro (250 Kbps)                   │
         └────────────┬──────────────────┬──────────┘
                      │                  │
         ┌────────────┴─────┐   ┌───────┴──────────────┐
         │  Taşıyıcı (Nano) │   │  Yer İstasyonu (PC)  │
         │  • BMP280        │   │  • C# Windows Forms  │
         │  • Servo Motor   │   │  • Telemetri Display │
         │  • Buzzer        │   │  • Video Viewer      │
         │  • XBee          │   │  • Harita            │
         └──────────────────┘   │  • 3D Görselleştirme │
                                └──────────────────────┘
                                
         ┌────────────────────────────────────────────┐
         │          IoT Bonus Sistemi (S2S)          │
         │  İstasyon #1 (Nano)    İstasyon #2 (Mega)│
         │  • BMP280              • BMP280           │
         │  • XBee (Kanal 12)     • XBee (Kanal 13) │
         │  412-707m Mesafe  →  Görev Yükü          │
         └────────────────────────────────────────────┘
```

### 📊 Veri Akış Diyagramı

```
Sensörler → SensorManager → TelemetryHandler → XBee → Yer İstasyonu
    ↓                                              ↓
SD Kayıtçı                                    Video Akışı
    ↓                                              ↓
CSV Dosyası                                 Canlı Görüntü

         ┌──────────────────────────────┐
         │   Telemetri Paketi (1 Hz)    │
         │  30 Alan × ~150 byte         │
         │  + XOR Checksum              │
         └──────────────────────────────┘
                      ↓
         ┌──────────────────────────────┐
         │    XBee Frame (API Mode 1)   │
         │  DEADBEEF marker başlangıç   │
         │  CAFEBABE marker bitiş       │
         └──────────────────────────────┘
```

### 🔄 Uçuş Durumları (State Machine)

```
0: Uçuşa Hazır
    ↓ (Basınç düşüyor)
1: Yükselme
    ↓ (Apogee/Basınç artıyor)
2: Model Uydu İniş (Hız: 12-14 m/s)
    ↓ (400m ± 10m)
3: Ayrılma (Servo aktif + 5s timeout)
    ↓ (Ayrılma onayı)
4: Görev Yükü İniş (Hız: 6-8 m/s)
    ↓ (<10m yükseklik)
5: Kurtarma (Buzzer aktif + 10s telemetri)
```

---

## 🛠️ Donanım Listesi

### ✅ Görev Yükü (Raspberry Pi)

| Bileşen | Model | Miktar | Durum |
|---------|-------|--------|-------|
| Ana İşlemci | Raspberry Pi Zero 2W | 1 | ✅ Mevcut |
| Basınç/Sıcaklık | BMP280 (I2C 0x76) | 1 | ✅ Mevcut |
| IMU Sensör | 10-DOF (ADXL345+ITG3200+HMC5883L) | 1 | ✅ Mevcut |
| GPS Modülü | UbloxNeo-8M | 1 | ✅ Mevcut |
| Kamera | Raspberry Pi Camera Module 3 (11.9MP) | 1 | ✅ Mevcut |
| XBee Modül | XBee 3 Pro (63mW, 2.4GHz) | 1 | ✅ Mevcut |
| ADC | ADS1115 (16-bit, I2C) | 1 | ✅ Mevcut |
| RTC | DS3231 (I2C, ±2 dk/yıl) | 1 | ✅ Mevcut |
| Servo Motor | SG90 (1.8 kg⋅cm) | 2 | ✅ Mevcut |
| Buzzer | Passive 3-12V | 1 | ✅ Mevcut |
| SD Kart | 64GB Class 10 | 1 | ✅ Mevcut |
| Pil | NCR18650B Li-Ion 3400mAh | 2 | ✅ Mevcut |
| Voltaj Regülatörü | LM2596 (Step-Down) + LM2577 (Step-Up) | 2 | ✅ Mevcut |

### ✅ Taşıyıcı Sistem (Arduino Nano)

| Bileşen | Model | Miktar | Durum |
|---------|-------|--------|-------|
| Mikroişlemci | Arduino Nano (ATMEGA328) | 1 | ✅ Mevcut |
| Basınç Sensörü | BMP280 (I2C 0x77) | 1 | ✅ Mevcut |
| Servo Motor | SG90 (Ayrılma mekanizması) | 1 | ✅ Mevcut |
| XBee Modül | XBee 3 Pro | 1 | ✅ Mevcut |
| Buzzer | Passive 3-12V | 1 | ✅ Mevcut |

### ✅ IoT İstasyonları

#### İstasyon #1 (Arduino Nano)
| Bileşen | Model | Miktar | Durum |
|---------|-------|--------|-------|
| Mikroişlemci | Arduino Nano (ATMEGA328) | 1 | ✅ Mevcut |
| Sıcaklık Sensörü | BMP280 (I2C 0x76) | 1 | ✅ Mevcut |
| XBee Modül | XBee 3 Pro (Kanal 12) | 1 | ✅ Mevcut |
| Anten | Yagi 25dBi | 1 | ✅ Mevcut |

#### İstasyon #2 (Arduino Mega 2560)
| Bileşen | Model | Miktar | Durum |
|---------|-------|--------|-------|
| Mikroişlemci | Arduino Mega 2560 (ATMEGA2560) | 1 | ✅ Mevcut |
| Sıcaklık Sensörü | BMP280 (I2C 0x77) | 1 | ✅ Mevcut |
| XBee Modül | XBee 3 Pro (Kanal 13) | 1 | ✅ Mevcut |
| Anten | Yagi 25dBi | 1 | ✅ Mevcut |

### 📡 XBee Network Konfigürasyonu

| Sistem | MY Adresi | Kanal | PAN ID | MAC Adresi |
|--------|-----------|-------|--------|------------|
| Görev Yükü | 0x0001 | - | 0x6570 | 0x0013A20042677E8F |
| Taşıyıcı | 0x0003 | 14 (0x0E) | 0x6570 | - |
| IoT #1 | 0x0004 | 12 (0x0C) | 0x6570 | - |
| IoT #2 | 0x0005 | 13 (0x0D) | 0x6570 | - |
| Yer İstasyonu | 0x0002 | - | 0x6570 | - |

**Not:** Tüm XBee modüller **XCTU** ile yapılandırılmıştır, firmware sürümü tutarlıdır (2014).

---

## 💾 Yazılım Mimarisi

### 🐍 Python (Raspberry Pi Görev Yükü)

```
modeluydu/
├── GorevYukuPi/                     # Raspberry Pi Görev Yükü
│   ├── ana_program.py               # Ana kontrol döngüsü
│   ├── moduller/
│   │   ├── yapilandirma.py         # Tüm sabitler ve konfigürasyon
│   │   ├── sensorler.py            # Sensör okuma (BMP280, IMU, GPS)
│   │   ├── imu_sensoru.py          # 10-DOF IMU yönetimi
│   │   ├── pil_gerilimi.py         # ADS1115 ADC pil izleme
│   │   ├── telemetri_isleyici.py  # Telemetri paketi oluşturma
│   │   ├── birlesik_xbee_alici.py # XBee haberleşme (Birleşik)
│   │   ├── kamera_basit.py         # Video kayıt ve akış
│   │   ├── aktuatorler.py          # Servo ve buzzer kontrolü
│   │   ├── sd_kayitci.py           # SD kart veri saklama
│   │   └── guc_yoneticisi.py       # Güç yönetimi ve safe shutdown
│   ├── requirements.txt             # Python bağımlılıkları
│   ├── gorevyuku.service           # Systemd otomatik başlatma
│   └── service_install.sh          # Servis kurulum scripti
```

#### 🎯 Ana Modüller

**`ana_program.py`** - Ana Kontrol Döngüsü
- Multi-threading yönetimi (telemetri, video, veri kayıt)
- Güvenli başlatma ve kapanma
- Hata yönetimi ve yeniden başlatma mekanizması
- Windows test modu desteği

**`sensorler.py`** - Sensör Yöneticisi
- BMP280 basınç/sıcaklık okuma (I2C)
- 10-DOF IMU entegrasyonu (otomatik kalibrasyon)
- GPS NMEA parsing (pynmea2)
- ADS1115 pil voltajı okuma
- Veri bütünlüğü kontrolleri

**`telemetri_isleyici.py`** - Telemetri İşleyici
- 30 alanlı telemetri paketi oluşturma
- ARAS hata kodu hesaplama (6-bit)
- Uçuş durumu yönetimi (state machine)
- Hız hesaplama (smoothing filter ile)
- XOR checksum

**`birlesik_xbee_alici.py`** - XBee Yöneticisi
- Telemetri gönderme (API Mode 1)
- Yer istasyonu komut alma
- SAHA protokolü (taşıyıcı basınç)
- IoT verisi alma (2 istasyon)
- Binary frame parsing (DEADBEEF/CAFEBABE)

**`kamera_basit.py`** - Kamera Yöneticisi
- İkili video sistemi (SD + XBee)
- H.264/MP4 kayıt (FfmpegOutput)
- MJPEG streaming (düşük gecikme)
- Frame marking (DEADBEEF/CAFEBABE)

### 🤖 Arduino (C++)

#### Taşıyıcı Sistem - `TasiyiciKontrol.ino`
```cpp
// Temel Özellikler
• BMP280 basınç okuma (I2C 0x77)
• Barometrik irtifa hesaplama
• Otomatik ayrılma @ 400m (servo)
• SAHA protokolü (XBee)
• Manuel ayrılma komutu (Serial + XBee)
• Kalibrasyon komutu desteği
• Kurtarma buzzer
```

#### IoT İstasyon #1 - `IoTStation1_XBee.ino`
```cpp
// Özellikler
• Arduino Nano (ATMEGA328)
• BMP280 sıcaklık (I2C 0x76)
• SoftwareSerial XBee (Pin 2/3)
• XBee API Frame gönderimi
• 1 Hz veri aktarımı
• Mesafe: 412-707m
```

#### IoT İstasyon #2 - `IoTStation2_Mega.ino`
```cpp
// Özellikler
• Arduino Mega 2560 (ATMEGA2560)
• BMP280 sıcaklık (I2C 0x77)
• Hardware Serial1 XBee (Pin 18/19)
• XBee API Frame gönderimi
• 1 Hz veri aktarımı
• Mesafe: 412-707m
• Daha stabil Serial1 kullanımı
```

### 💻 C# (Yer İstasyonu - Windows Forms)

```
yerAltiİstasyonu/ModelUydu/
├── Program.cs                         # Uygulama başlatma
├── Form1.cs                           # Ana form (UI)
├── BaglantiYoneticisi.cs             # XBee bağlantı yönetimi
├── TelemetriYoneticisi.cs            # Telemetri parsing ve işleme
├── KameraYoneticisi.cs               # Video decode ve görüntüleme
├── MultiSpektralFiltreYoneticisi.cs  # Filtre kontrol komutları
├── GrafikYoneticisi.cs               # Gerçek zamanlı grafikler
├── HaritaYoneticisi.cs               # GMap.NET entegrasyonu
├── UyduGorsellestime.cs              # 3D OpenGL görselleştirme
├── AlarmSistemiYoneticisi.cs         # ARAS alarm sistemi
├── ExcelVeriAktarmaYoneticisi.cs     # CSV export
├── KalibrasyonYoneticisi.cs          # Kalibrasyon araçları
├── HataYoneticisi.cs                 # Log ve hata yönetimi
└── SD_Kart_Simulasyon/               # SD kart simülasyon dizini
```

#### 🖥️ Ana Form Bileşenleri
- **Telemetri Paneli** - Gerçek zamanlı sensör verileri
- **Video Görüntüleyici** - MJPEG canlı akış
- **Harita** - GPS konum izleme (GMap.NET)
- **3D Görselleştirme** - Uydu oryantasyonu (OpenGL)
- **Grafikler** - Yükseklik, basınç, sıcaklık, hız (ZedGraph)
- **ARAS Panel** - 6-bit hata kodu görselleştirme
- **Komut Paneli** - Manuel komutlar (ayrılma, filtre)

---

## 📦 Kurulum

### 1. Raspberry Pi Kurulumu

#### 1.1 Sistem Hazırlığı

```bash
# Sistem güncelleme
sudo apt update && sudo apt upgrade -y

# GPIO, I2C, SPI, UART aktifleştirme
sudo raspi-config
# Interface Options > I2C > Enable
# Interface Options > SPI > Enable
# Interface Options > Serial Port > Enable (Login shell: No)
# Interface Options > Camera > Enable

# Reboot
sudo reboot
```

#### 1.2 Python Ortamı

```bash
# Proje dizini oluştur
cd /home/atugem
mkdir -p TurkSatModelUydu
cd TurkSatModelUydu

# Projeyi klonla
git clone https://github.com/nurullahsahinn/turksat-model-uydu.git .

# Proje dizinine git
cd modeluydu/GorevYukuPi

# Sanal ortam oluştur
python3 -m venv venv
source venv/bin/activate

# Bağımlılıkları yükle
pip install -r requirements.txt
```

#### 1.3 Donanım Kontrolü

```bash
# I2C cihazları tara
sudo i2cdetect -y 1
# Beklenen: 0x68 (IMU), 0x76 (BMP280), 0x48 (ADS1115), 0x68 (RTC)

# Serial portları kontrol
ls -la /dev/ttyAMA0 /dev/serial0

# Kamera testi
libcamera-hello --list
```

#### 1.4 Otomatik Başlatma Servisi

```bash
# Servis dosyasını kopyala
sudo cp gorevyuku.service /etc/systemd/system/

# Servisi aktifleştir
sudo systemctl daemon-reload
sudo systemctl enable gorevyuku.service
sudo systemctl start gorevyuku.service

# Durum kontrolü
sudo systemctl status gorevyuku.service

# Logları izle
journalctl -u gorevyuku.service -f
```

### 2. Arduino Kurulumu

#### 2.1 Arduino IDE Kurulumu

1. [Arduino IDE](https://www.arduino.cc/en/software) indir ve kur
2. **Tools > Manage Libraries** → "Adafruit BMP280" kütüphanesini yükle

#### 2.2 Kod Yükleme

**Taşıyıcı Sistem (Arduino Nano):**
```bash
# Dosya: TasiyiciSistem/TasiyiciKontrol/TasiyiciKontrol.ino
Board: Arduino Nano
Processor: ATmega328P (Old Bootloader)
Port: (COM portunu seç)
```

**IoT İstasyon #1 (Arduino Nano):**
```bash
# Dosya: IoTIstasyonu1/IoTStation1_XBee/IoTStation1_XBee.ino
Board: Arduino Nano
Processor: ATmega328P (Old Bootloader)
Port: (COM portunu seç)
```

**IoT İstasyon #2 (Arduino Mega):**
```bash
# Dosya: IoTIstasyonu2/IoTStation2_Mega/IoTStation2_Mega.ino
Board: Arduino Mega 2560
Port: (COM portunu seç)
```

#### 2.3 Seri Monitor Test

Upload sonrası Serial Monitor'ı açın (57600 baud):
```
✅ BMP280 sensörü başarıyla başlatıldı
✅ XBee hazır
```

### 3. Yer İstasyonu Kurulumu

#### 3.1 Visual Studio Kurulumu

1. [Visual Studio 2019/2022 Community](https://visualstudio.microsoft.com/tr/downloads/) indir
2. .NET Desktop Development workload'ı yükle
3. .NET Framework 4.7.2 veya üstü yükle

#### 3.2 Proje Açma

```bash
# Solution dosyasını aç
yerAltiİstasyonu/ModelUydu.sln

# NuGet paketlerini restore et
Tools > NuGet Package Manager > Restore

# Build
Build > Rebuild Solution

# Çalıştır
Debug > Start (F5)
```

#### 3.3 Gerekli Kütüphaneler (NuGet)

- `GMap.NET.Windows.Forms` - Harita görselleştirme
- `ZedGraph` - Grafikler
- `OpenTK` - 3D görselleştirme
- `System.IO.Ports` - Serial port haberleşme

---

## 🎮 Kullanım

### 🚀 Uçuş Öncesi Hazırlık

#### 1. Donanım Kontrolleri

```bash
# Raspberry Pi - Sensör testi
cd /home/atugem/TurkSatModelUydu/GorevYukuPi
source venv/bin/activate
python quick_test.py

# Beklenen Çıktı:
✅ BMP280: 1013.2 hPa, 25.3°C
✅ IMU: Pitch=0.1° Roll=-0.2° Yaw=180.5°
✅ GPS: Fix var, 41.0138°N 28.9497°E
✅ ADS1115: 7.4V (%95)
✅ XBee: Port açık
```

#### 2. XBee Konfigürasyon Kontrolü (XCTU)

| Parametre | Değer | Açıklama |
|-----------|-------|----------|
| PAN ID | 0x6570 | Tüm modüller aynı |
| Baud Rate | 57600 | Tüm modüller aynı |
| API Mode | 1 | API enabled |
| Power Level | 4 (63mW) | Maksimum güç |

#### 3. Kalibrasyon

**Basınç Kalibrasyonu (Şehre Göre):**

Yer İstasyonu → Kalibrasyon Sekmesi:
- Şehir seç (Ankara varsayılan: 911.75 hPa)
- Manuel kalibrasyon:
  ```
  #CALIB_PRESSURE:911.75:850#
  Format: #CALIB_PRESSURE:<hPa>:<rakım_m>#
  ```

**IMU Gyro Kalibrasyonu:**
```
Komut: #CALIB_GYRO:RESET#
Cihazı düz yüzeyde sabit tutun → 5 saniye bekleyin
```

#### 4. Video Kayıt Testi

```bash
# SD kart bağlantısı kontrol
df -h | grep sdcard

# Video test
python raspberry_pi2w_video_test.py

# 10 saniye sonra durdur (Ctrl+C)
# VLC ile video oynat
vlc /media/sdcard/video_kayitlari/video_*.mp4
```

### ✈️ Uçuş Sırası

#### 1. Sistem Başlatma

```bash
# Manuel başlatma (test için)
cd /home/atugem/TurkSatModelUydu/GorevYukuPi
source venv/bin/activate
python ana_program.py

# Servis ile otomatik başlatma
sudo systemctl restart gorevyuku.service
journalctl -u gorevyuku.service -f
```

**Beklenen Log:**
```
🚀 TÜRKSAT Model Uydu Sistemi Başlatılıyor...
✅ SD Kayıt sistemi başlatıldı
✅ Birleşik XBee sistemi başlatıldı
✅ Sensör Yöneticisi başlatıldı
✅ Kamera sistemi başlatıldı
✅ Telemetri gönderimi aktif
📡 Telemetri #1 gönderildi
```

#### 2. Yer İstasyonu Bağlantısı

1. **XBee USB Adaptör** PC'ye tak
2. Yer İstasyonu uygulamasını başlat
3. **COM Port** seç (Device Manager'dan kontrol)
4. **Bağlan** butonuna tıkla
5. Telemetri akışını gözlemle

**Beklenen UI:**
- ✅ "Bağlantı Kuruldu" (yeşil)
- 📊 Grafiklerde veri akışı
- 📹 Video akışı (2-3 saniye içinde)
- 🗺️ Harita üzerinde konum

#### 3. Uçuş İzleme

**Telemetri Paneli:**
- Paket numarası artıyor
- GPS fix durumu "3D Fix"
- Basınç azalıyor (yükselme)
- Yükseklik artıyor

**Video:**
- 2 FPS canlı akış
- Frame quality slider ile ayarla

**Harita:**
- Kırmızı nokta: Görev yükü
- Mavi çizgi: Uçuş yolu

**3D Görselleştirme:**
- Uydu modeli gerçek zamanlı dönüyor
- Pitch, Roll, Yaw açıları

#### 4. Ayrılma Sekansı (400m)

**Otomatik:**
- Görev yükü 400m ± 10m'ye ulaşınca servo otomatik tetiklenir
- Durum: `3 - Ayrılma`
- SAHA protokolü: Taşıyıcı basınç verisi başlar

**Manuel (Test/Acil):**
1. Yer İstasyonu → Komut Sekmesi
2. "Manuel Ayrılma" butonuna tıkla
3. Onay penceresi → Evet
4. Komut gönderilir: `!xT!`

#### 5. Kurtarma Modu (<10m)

- Görev yükü 10m altına inince otomatik aktif
- Buzzer çalmaya başlar (1.5 kHz)
- 10 saniye daha telemetri gönderir
- Video kaydı durur

### 📊 Uçuş Sonrası Analiz

#### 1. SD Kart Verileri

```bash
# SD kartı PC'ye tak
# Verileri kopyala
D:/
├── telemetri/
│   └── telemetri_2025-10-01.csv
├── video/
│   └── video_2025-10-01_14-30-00.mp4
└── logs/
    └── sistem_log_2025-10-01.txt
```

#### 2. Telemetri Analizi

**Excel/LibreOffice:**
```
Dosya > Aç > telemetri_2025-10-01.csv
Grafik ekle > Yükseklik vs Zaman
```

**Python Analizi:**
```python
import pandas as pd
import matplotlib.pyplot as plt

df = pd.read_csv('telemetri_2025-10-01.csv')
df['Yukseklik'].plot()
plt.show()
```

#### 3. Video İnceleme

```bash
# VLC ile oynat
vlc video_2025-10-01_14-30-00.mp4

# Frame-by-frame analiz
ffmpeg -i video.mp4 -vf fps=1 frames/frame_%04d.png
```

#### 4. Yer İstasyonu Replay

1. Yer İstasyonu → Dosya > Telemetri Replay
2. CSV dosyasını seç
3. Hız ayarla (1x, 2x, 5x)
4. Play

---

## 📂 Proje Yapısı

```
turksat-model-uydu/
├── README.md                          # Bu dosya
├── .gitignore                         # Git ignore kuralları
│
└── modeluydu/                         # Ana proje klasörü
    │
    ├── GorevYukuPi/                   # Raspberry Pi Görev Yükü
    │   ├── ana_program.py             # Ana kontrol programı
    │   ├── requirements.txt           # Python bağımlılıkları
    │   ├── gorevyuku.service          # Systemd servisi
    │   ├── service_install.sh         # Servis kurulum
    │   ├── SERVICE_KULLANIM.md        # Servis kullanım rehberi
    │   ├── moduller/                  # Python modülleri
    │   │   ├── yapilandirma.py
    │   │   ├── sensorler.py
    │   │   ├── imu_sensoru.py
    │   │   ├── pil_gerilimi.py
    │   │   ├── telemetri_isleyici.py
    │   │   ├── birlesik_xbee_alici.py
    │   │   ├── kamera_basit.py
    │   │   ├── aktuatorler.py
    │   │   ├── sd_kayitci.py
    │   │   └── guc_yoneticisi.py
    │   └── test scriptleri/           # Test dosyaları
    │       ├── quick_test.py
    │       ├── telemetri_only_test.py
    │       └── video_debug_test.py
    │
    ├── TasiyiciSistem/                # Arduino Taşıyıcı
    │   └── TasiyiciKontrol/
    │       └── TasiyiciKontrol.ino
    │
    ├── IoTIstasyonu1/                 # Arduino Nano IoT #1
    │   └── IoTStation1_XBee/
    │       └── IoTStation1_XBee.ino
    │
    ├── IoTIstasyonu2/                 # Arduino Mega IoT #2
    │   └── IoTStation2_Mega/
    │       └── IoTStation2_Mega.ino
    │
    └── yerAltiİstasyonu/              # C# Yer İstasyonu
        ├── ModelUydu.sln
        └── Model Uydu/
            ├── Program.cs
            ├── Form1.cs               # Ana UI
            ├── BaglantiYoneticisi.cs
            ├── TelemetriYoneticisi.cs
            ├── KameraYoneticisi.cs
            ├── GrafikYoneticisi.cs
            ├── HaritaYoneticisi.cs
            ├── UyduGorsellestime.cs
            └── AlarmSistemiYoneticisi.cs
```

---

## 📡 Protokoller ve Standartlar

### 📦 Telemetri Paketi Formatı (TÜRKSAT Uyumlu)

**30 Alan × Virgülle Ayrılmış:**

```
<Paket#>,<Durum>,<Hata>,<Tarih/Saat>,<Basınç1>,<Basınç2>,<Yükseklik1>,
<Yükseklik2>,<İrtifaFarkı>,<İnişHızı>,<Sıcaklık>,<PilGerilimi>,<Enlem>,
<Boylam>,<GPSYükseklik>,<Pitch>,<Roll>,<Yaw>,<AccX>,<AccY>,<AccZ>,
<GyroX>,<GyroY>,<GyroZ>,<MagX>,<MagY>,<MagZ>,<RHRH>,<IoT1>,<IoT2>,<TakımNo>
```

**Örnek:**
```
$1,1,000000,01/10/2025 14:30:05,101325,101200,125.3,120.5,4.8,2.5,
25.3,7.4,41.013840,28.949660,125.0,0.1,-0.2,180.5,0.05,0.02,9.81,
0.01,-0.01,0.00,25.0,26.0,23.0,00,24.5,25.2,286570*3A
```

### 📋 Telemetri Alan Açıklamaları

| # | Alan | Birim | Açıklama |
|---|------|-------|----------|
| 1 | Paket Numarası | - | 1'den başlar, kalıcı saklanır |
| 2 | Uydu Durumu | - | 0:Hazır, 1:Yükselme, 2:İniş, 3:Ayrılma, 4:GörevYükü, 5:Kurtarma |
| 3 | Hata Kodu | 6-bit | ARAS alarm sistemi (000000 = hata yok) |
| 4 | Tarih/Saat | - | DD/MM/YYYY HH:MM:SS (RTC veya sistem) |
| 5 | Basınç 1 | Pa | Görev yükü BMP280 (Pascal) |
| 6 | Basınç 2 | Pa | Taşıyıcı BMP280 (SAHA protokolü, ayrılma sonrası) |
| 7 | Yükseklik 1 | m | Görev yükü irtifası (barometrik) |
| 8 | Yükseklik 2 | m | Taşıyıcı irtifası (barometrik) |
| 9 | İrtifa Farkı | m | Yükseklik1 - Yükseklik2 |
| 10 | İniş Hızı | m/s | Mutlak hız (smoothing filter) |
| 11 | Sıcaklık | °C | BMP280 sıcaklık |
| 12 | Pil Gerilimi | V | ADS1115 ADC ölçümü |
| 13 | Enlem | ° | GPS koordinat (WGS84) |
| 14 | Boylam | ° | GPS koordinat (WGS84) |
| 15 | GPS Yükseklik | m | GPS irtifası |
| 16 | Pitch | ° | IMU eğim açısı (x-ekseni) |
| 17 | Roll | ° | IMU eğim açısı (y-ekseni) |
| 18 | Yaw | ° | IMU yön açısı (z-ekseni, manyetometre) |
| 19-21 | AccX/Y/Z | m/s² | 10-DOF ivmeölçer (ham veri) |
| 22-24 | GyroX/Y/Z | °/s | 10-DOF jiroskop (ham veri) |
| 25-27 | MagX/Y/Z | µT | 10-DOF manyetometre (ham veri) |
| 28 | RHRH | - | Rezerv alan (00) |
| 29 | IoT İst. 1 | °C | IoT istasyon 1 sıcaklık |
| 30 | IoT İst. 2 | °C | IoT istasyon 2 sıcaklık |
| 31 | Takım No | - | 286570 (NONGRAVITY) |

**Checksum:** `*3A` (XOR checksum, hex formatında)

### 🚨 ARAS Hata Kodu (6-bit)

| Bit | Hata Açıklaması | Kontrol |
|-----|-----------------|---------|
| 0 | Model uydu iniş hızı (12-14 m/s dışında) | Durum=2 iken hız kontrolü |
| 1 | Görev yükü iniş hızı (6-8 m/s dışında) | Durum=4 iken hız kontrolü |
| 2 | Taşıyıcı basınç verisi alınamama | 15s timeout |
| 3 | GPS konum verisi alınamama | 30s timeout |
| 4 | Ayrılma gerçekleşmeme | 5s timeout @ Durum=3 |
| 5 | Multi-spektral sistem hatası | Filtre hareketleri |

**Örnek:** `010010` = Bit 1 ve 4 aktif (Görev yükü hız hatası + Ayrılma timeout)

### 📡 XBee Frame Formatı (API Mode 1)

**Telemetri Frame:**
```
0x7E | LenMSB | LenLSB | 0x10 | FrameID | DestAddr64 | DestAddr16 | 
Options | TelemetriData | Checksum
```

**Video Frame (MJPEG):**
```
DEADBEEF | FrameSize (4 byte) | JPEGData | CAFEBABE
```

### 🤝 SAHA Protokolü (Sistemler Arası Haberleşme)

**Format:**
```
SAHA:BASINC2:<pascal_degeri>
```

**Örnek:**
```
SAHA:BASINC2:101200
```

**Timing:**
- Sadece ayrılma sonrası aktif
- 5 Hz (200ms aralık)
- XBee serial port üzerinden

---

## 🧪 Test ve Kalibrasyon

### ✅ Ön Uçuş Kontrol Listesi

#### 1. Donanım Testleri

**Raspberry Pi:**
```bash
# I2C sensörleri
sudo i2cdetect -y 1
✅ 0x48 (ADS1115)
✅ 0x68 (IMU/RTC)
✅ 0x76 (BMP280)

# Kamera
libcamera-hello --list
✅ /base/soc/i2c0mux/i2c@1/imx219@10

# Serial portlar
ls -la /dev/serial0 /dev/ttyAMA0
✅ crw-rw---- 1 root dialout

# XBee bağlantısı
python3 -c "import serial; s=serial.Serial('/dev/serial0', 57600, timeout=1); print('✅ XBee OK' if s.is_open else '❌ Hata')"
```

**Arduino (Serial Monitor @ 57600):**
```
Taşıyıcı:
✅ BMP280 sensörü başarıyla başlatıldı (0x77 adresi)
✅ XBee hazır

IoT #1:
✅ BMP280 sensörü başarıyla başlatıldı
✅ XBee hazır (İstasyon #1 - XCTU aktif)

IoT #2:
✅ BMP280 sensörü başarıyla başlatıldı (MEGA I2C)
✅ XBee hazır (İstasyon #2 - MEGA Serial1 aktif)
```

#### 2. Fonksiyonel Testler

**Telemetri:**
```bash
python telemetri_only_test.py
# Beklenen: 1 Hz telemetri paketi
```

**Video:**
```bash
python video_debug_test.py
# Beklenen: Video kaydı başlar, 10s sonra VLC ile oynat
```

**XBee Mesh:**
```bash
# Yer İstasyonu → Telemetri sekmesi
✅ Görev yükü telemetrisi gelmiyor
✅ IoT #1 sıcaklığı: 24.5°C
✅ IoT #2 sıcaklığı: 25.2°C
```

**Ayrılma Testi (Masa Üstü):**
```bash
# Arduino Serial Monitor (Taşıyıcı)
> AYIRMA
🚨 MANUEL AYRILMA TETIKLENDI!
✅ Servo 90° döndü
```

#### 3. Entegrasyon Testi

**Senaryo: Simüle Uçuş**
```bash
# 1. Sistemi başlat
sudo systemctl restart gorevyuku.service

# 2. Yer istasyonunu bağla
# Beklenen: Telemetri akışı başladı

# 3. Kamera görüntüsü
# Beklenen: 2-3 saniye içinde video akışı

# 4. Basınç simülasyonu (test için)
# moduller/yapilandirma.py → DENIZ_SEVIYESI_BASINC_HPA değerini değiştir
# Yeniden başlat → Yükseklik değişecek

# 5. Manuel ayrılma komutu gönder
# Yer İstasyonu → Manuel Ayrılma
# Beklenen: Taşıyıcı servo çalıştı

# 6. Loglara bak
journalctl -u gorevyuku.service -n 100
```

### 🎯 Kalibrasyon Prosedürleri

#### 1. Basınç Kalibrasyonu (Şehir Bazlı)

**Yer İstasyonu Yöntemi:**
1. Kalibrasyon sekmesini aç
2. Şehir seç (dropdown):
   - Ankara: 911.75 hPa (850m)
   - İstanbul: 1008.42 hPa (40m)
   - İzmir: 1011.44 hPa (25m)
   - Kayseri: 886.25 hPa (1054m)
3. "Gönder" butonuna tıkla
4. Görev yükü onayı: `PRESSURE_CALIB_OK:911.75`

**Manuel Yöntem (Serial Monitor):**
```bash
# Arduino Taşıyıcı
#CALIB_PRESSURE:911.75:850#
✅ Kalibrasyon güncellemesi: 911.75 hPa
📍 Şehir rakımı: 850m
```

#### 2. IMU Gyro Kalibrasyonu

**Otomatik (Başlangıçta):**
- Cihaz ilk başlatıldığında otomatik gyro bias hesabı
- 100 örnek ortalaması
- ~5 saniye sürer

**Manuel (Komut ile):**
```bash
# Yer İstasyonu → Kalibrasyon
#CALIB_GYRO:RESET#

# Raspberry Pi yanıtı:
🔄 Gyro kalibrasyon komutu alındı...
✅ Gyro kalibrasyonu yeniden başlatıldı
```

**Önemli:**
- Cihaz düz, stabil yüzeyde durmalı
- Hareket etmemeli
- Kalibrasyon sırasında motor vb. çalışmamalı

#### 3. GPS Ayarları

**Fix Bekleme:**
```bash
# İlk başlatmada GPS fix almak 30-60 saniye sürebilir
# Açık alanda test edin
```

**GPS Status Kontrolü:**
```python
# Yer istasyonunda GPS paneli
3D Fix: ✅     # En az 4 uydu
Satellites: 8  # İyi sinyal
HDOP: 1.2     # <2 iyi hassasiyet
```

---

## 🔧 Sorun Giderme

### ❌ Yaygın Hatalar ve Çözümleri

#### 1. XBee Bağlantısı Yok

**Belirtiler:**
- Telemetri gelmiyor
- Video akışı yok
- Komutlar gönderilemiyor

**Çözüm:**
```bash
# 1. Port kontrolü
ls -la /dev/serial0 /dev/ttyAMA0
# Yoksa: sudo raspi-config → Serial Port → Enable

# 2. İzin kontrolü
sudo usermod -a -G dialout atugem
# Logout/login gerekli

# 3. XBee test
python3 -c "
import serial
ser = serial.Serial('/dev/serial0', 57600, timeout=2)
print('Port açık:', ser.is_open)
ser.write(b'TEST')
print('Yazma OK')
ser.close()
"

# 4. XCTU ile XBee konfigürasyonu kontrol
# PAN ID: 0x6570
# Baud Rate: 57600
# API Mode: 1
```

#### 2. I2C Sensör Bulunamadı

**Belirtiler:**
- `❌ HATA: BMP280 sensörü bulunamadı`
- I2C adres taramasında görünmüyor

**Çözüm:**
```bash
# 1. I2C aktif mi?
sudo raspi-config
# Interface Options > I2C > Enable

# 2. Tarama yap
sudo i2cdetect -y 1
# BMP280: 0x76 veya 0x77 görünmeli

# 3. Kablolama kontrolü
# SDA: Pin 3 (GPIO 2)
# SCL: Pin 5 (GPIO 3)
# VCC: 3.3V (Pin 1)
# GND: GND (Pin 6)

# 4. Pull-up rezistansları (4.7kΩ)
# Uzun kablolarda gerekli olabilir
```

#### 3. GPS Fix Alınamıyor

**Belirtiler:**
- GPS verileri 0.0 / 0.0
- "GPS fix yok" uyarısı

**Çözüm:**
```bash
# 1. Açık alanda test edin
# Bina içi/ağaçlık alan sinyal almaz

# 2. GPS modülü kontrolü
cat /dev/ttyS0
# NMEA cümleleri akmalı:
# $GPGGA,143005.00,4100.8304,N,02856.9796,E,1,08,1.2,125.0,M,...

# 3. Soğuk start (ilk kullanım)
# İlk fix 30-60 saniye sürer
# Sabırla bekleyin

# 4. Anten kontrolü
# GPS anten konektörü sağlam mı?
```

#### 4. Kamera Başlamıyor

**Belirtiler:**
- `❌ Kamera başlatılamadı`
- Video kaydı başlamıyor

**Çözüm:**
```bash
# 1. Kamera etkin mi?
sudo raspi-config
# Interface Options > Camera > Enable

# 2. Kamera kablo bağlantısı
# CSI portuna doğru yönde takılı olmalı
# Kırmızı şerit HDMI'ye yakın

# 3. Kamera testi
libcamera-hello --list
# /base/soc/i2c0mux/i2c@1/imx219@10 görünmeli

# 4. Kamera device
ls -la /dev/video*
# /dev/video0 olmalı

# 5. Picamera2 kurulumu
pip install picamera2
```

#### 5. Video Kaydı 3 Saniyede Duruyor

**Belirtiler:**
- Video dosyası çok kısa
- Erken sonlanma

**Çözüm:**
```bash
# BU SORUN ÇÖZÜLDÜ - kamera_basit.py güncellendi
# Eğer hala oluyorsa:

# 1. SD kart doluluk kontrolü
df -h /media/sdcard
# %90'dan az olmalı

# 2. Kamera streaming thread kontrolü
journalctl -u gorevyuku.service | grep "Video"
# "Video kaydı başlatıldı" sonrası 10+ saniye görmeli

# 3. Manuel test
python raspberry_pi2w_video_test.py
# 30 saniye kayıt yapmalı
```

#### 6. Servo Çalışmıyor

**Belirtiler:**
- Ayrılma komutu servo döndürmüyor
- Multi-spektral filtreler hareket etmiyor

**Çözüm:**
```bash
# 1. PWM testi
python3 -c "
import RPi.GPIO as GPIO
import time
GPIO.setmode(GPIO.BCM)
GPIO.setup(18, GPIO.OUT)
pwm = GPIO.PWM(18, 50)
pwm.start(7.5)  # Orta pozisyon
time.sleep(2)
pwm.stop()
GPIO.cleanup()
"

# 2. Güç kontrolü
# Servo 5V'a bağlı mı?
# Ayrı güç kaynağı kullanılmalı (yüksek akım)

# 3. Pin kontrolü
# Servo 1: GPIO 17 (Pin 11)
# Servo 2: GPIO 19 (Pin 35)
```

#### 7. SD Kart Kaydı Çalışmıyor

**Belirtiler:**
- CSV dosyası oluşmuyor
- Video kaydedilmiyor

**Çözüm:**
```bash
# 1. SD kart mount kontrolü
mount | grep sdcard
# /dev/mmcblk0p1 on /media/sdcard

# 2. Yazma izni
touch /media/sdcard/test.txt
# Hata vermemeli

# 3. Dosya sistemi kontrolü
sudo fsck /dev/mmcblk0p1

# 4. SD kart kapasitesi
df -h /media/sdcard
# En az 2GB boş olmalı

# 5. Manuel klasör oluşturma
mkdir -p /media/sdcard/telemetri
mkdir -p /media/sdcard/video
mkdir -p /media/sdcard/logs
```

### 📊 Performans Sorunları

#### Yüksek CPU Kullanımı

```bash
# CPU kullanımı kontrol
top
# ana_program.py %90'ın altında olmalı

# CPU governor ayarı
echo performance | sudo tee /sys/devices/system/cpu/cpu*/cpufreq/scaling_governor

# Video FPS düşür (kamera_basit.py)
VIDEO_XBEE_FPS = 1  # 2'den 1'e
```

#### Düşük Bellek

```bash
# Bellek kontrolü
free -h
# Available >100MB olmalı

# Swap kullanımı azalt
sudo sysctl vm.swappiness=10

# Gereksiz servisleri kapat
sudo systemctl disable bluetooth.service
```

---

## 👨‍💻 Geliştiriciler İçin

### 🔧 Geliştirme Ortamı

#### Raspberry Pi Üzerinde

```bash
# SSH bağlantısı
ssh atugem@192.168.1.100

# VS Code Remote SSH
# Extension: Remote - SSH
# Host: atugem@192.168.1.100

# Nano editör (basit düzenlemeler)
nano ana_program.py

# Vim editör
vim ana_program.py
```

#### Windows/Mac Üzerinde

```bash
# Sanal ortam
cd GorevYukuPi
python -m venv venv
source venv/bin/activate  # Windows: venv\Scripts\activate

# Test modu (Windows simülasyon)
set FORCE_SIMULATION=1
python ana_program.py

# Çıktı:
🧪 WINDOWS TEST MODU: Simülasyon zorla aktifleştirildi
⚠️ UYARI: Raspberry Pi tespit edilmedi, simülasyon modu aktif
```

### 📝 Kod Katkısı

#### Branch Stratejisi

```bash
# Main branch - kararlı sürüm
git checkout main

# Yeni özellik için branch
git checkout -b feature/yeni-ozellik

# Bug fix için branch
git checkout -b fix/hata-duzeltme

# Commit mesajları (Türkçe)
git commit -m "🐛 Fix: GPS timeout sorunu düzeltildi"
git commit -m "✨ Feat: Multi-spektral oto mod eklendi"
git commit -m "📝 Docs: README telemetri bölümü güncellendi"
```

#### Commit Emoji Standardı

| Emoji | Tip | Açıklama |
|-------|-----|----------|
| ✨ | Feat | Yeni özellik |
| 🐛 | Fix | Hata düzeltme |
| 📝 | Docs | Dokümantasyon |
| 🎨 | Style | Kod formatı |
| ♻️ | Refactor | Kod yeniden yapılandırma |
| ⚡ | Perf | Performans iyileştirme |
| ✅ | Test | Test ekleme/güncelleme |
| 🔧 | Chore | Konfigürasyon/build |

### 🧪 Test Yazma

#### Python Unit Test

```python
# test/test_telemetri.py
import unittest
from moduller.telemetri_isleyici import TelemetryHandler

class TestTelemetry(unittest.TestCase):
    def setUp(self):
        self.handler = TelemetryHandler()
    
    def test_checksum(self):
        data = "286570,1,000000,..."
        checksum = self.handler._hesapla_checksum(data)
        self.assertIsInstance(checksum, int)
        self.assertLessEqual(checksum, 0xFF)
    
    def test_packet_format(self):
        sensor_data = {
            'basinc': 101325,
            'sicaklik': 25.0,
            # ...
        }
        packet = self.handler.olustur_telemetri_paketi(sensor_data)
        self.assertIn('ham_veri', packet)
        self.assertIn('xbee_paketi', packet)

if __name__ == '__main__':
    unittest.main()
```

#### Arduino Test (Serial Monitor)

```cpp
// Test fonksiyonu ekle
void testBMP280() {
    Serial.println("=== BMP280 TEST ===");
    float temp = bmp280.readTemperature();
    float pres = bmp280.readPressure() / 100.0;
    
    if (temp > -40 && temp < 85) {
        Serial.println("✅ Sıcaklık: OK");
    } else {
        Serial.println("❌ Sıcaklık: HATA");
    }
    
    if (pres > 300 && pres < 1100) {
        Serial.println("✅ Basınç: OK");
    } else {
        Serial.println("❌ Basınç: HATA");
    }
}

// setup() içinde çağır
void setup() {
    // ...
    testBMP280();
}
```

### 📚 Dokümantasyon Yazma

#### Docstring Formatı (Python)

```python
def hesapla_irtifa(basinc, deniz_seviyesi_basinc=1013.25):
    """
    Barometrik basınçtan irtifa hesaplar.
    
    Args:
        basinc (float): Anlık basınç (hPa)
        deniz_seviyesi_basinc (float): Referans basınç (hPa)
    
    Returns:
        float: Hesaplanan irtifa (metre)
    
    Raises:
        ValueError: Basınç negatif ise
    
    Examples:
        >>> hesapla_irtifa(1013.25)
        0.0
        >>> hesapla_irtifa(900.0)
        950.5
    """
    if basinc <= 0:
        raise ValueError("Basınç pozitif olmalı")
    
    return 44330.0 * (1.0 - pow(basinc / deniz_seviyesi_basinc, 0.1903))
```

#### Arduino Komut Açıklaması

```cpp
/**
 * @brief BMP280 sensöründen basınç okur
 * 
 * @return float Basınç değeri (Pascal)
 * @throws I2C_ERROR Sensör bağlantısı yoksa
 * 
 * @example
 * float pressure = readPressure();
 * Serial.println(pressure);
 */
float readPressure() {
    // ...
}
```

---

## 🎓 Kazanımlarımız

### 💡 Bu Süreçte Öğrendiklerimiz

TÜRKSAT Model Uydu Yarışması bize yalnızca teknik bilgi değil, aynı zamanda **disiplinler arası ekip çalışması** ve **problem çözme** konusunda da paha biçilmez bir deneyim sundu.

#### 🔧 Teknik Kazanımlar

##### 1. **Şartnameye Uyumun Önemi**
- TÜRKSAT şartnamesinin her detayına uyum sağlamak
- Telemetri formatı, frekans, ve paket yapısı standardizasyonu
- Test süreçlerinde şartname gereksinimlerini karşılama
- Dokümantasyon ve raporlama standartları

##### 2. **Hata Toleranslı Sistem Tasarımı**
- Raspberry Pi reset durumunda bile veri kaybı yaşanmaması
- Paket numarası ve görev zamanının kalıcı saklanması (JSON dosyaları)
- XBee bağlantı kopması durumunda otomatik yeniden bağlanma
- SD kart hatalarında emergency backup mekanizmaları
- Thread-safe veri paylaşımı ve lock mekanizmaları

##### 3. **Gerçek Zamanlı Veri İşleme**
- Çok iş parçacıklı (multi-threading) mimari tasarımı
- 1 Hz telemetri gönderimi ile eş zamanlı video kaydı
- Sensör verilerinin paralel okunması ve işlenmesi
- Buffer yönetimi ve bellek optimizasyonu
- CPU ve RAM kullanımı optimizasyonu (Raspberry Pi Zero 2W için)

##### 4. **Çift Yönlü Komut ve Veri İletişimi**
- XBee API Mode 1 ile frame-based haberleşme
- Yer istasyonundan komut alma ve işleme
- Manuel ayrılma ve kalibrasyon komutları
- Binary video frame'lerinin DEADBEEF/CAFEBABE marking ile paketlenmesi
- Checksum kontrolü ile veri bütünlüğü

##### 5. **Platformlar Arası Entegrasyon**
- **Python (Raspberry Pi):** Sensör okuma, telemetri işleme, video
- **C# (.NET):** Windows Forms yer istasyonu, gerçek zamanlı grafikler
- **Arduino (C++):** Taşıyıcı kontrol, IoT istasyonları
- Farklı platformlar arası veri formatı standardizasyonu
- Serial port haberleşme protokolleri (UART, I2C, SPI)

##### 6. **Operasyonel Esneklik**
- Manuel yedek komutlar (ayrılma, kalibrasyon)
- Test modu ve simülasyon desteği (Windows geliştirme ortamı)
- Servis tabanlı otomatik başlatma (systemd)
- Hot-reload ve runtime konfigürasyon değişiklikleri
- Debug logging ve hata izleme sistemi

##### 7. **Sensörler ve Bağlantı Protokolleri**
- **I2C Protokolü:** BMP280, IMU, ADS1115, DS3231
  - Pull-up rezistans gereksinimleri
  - Adres çakışmalarının çözümü
  - Bus hızı optimizasyonu
  
- **UART Protokolü:** GPS, XBee
  - Baud rate uyumu
  - TX/RX çapraz bağlantı
  - Flow control
  
- **SPI Protokolü:** SD kart (opsiyonel)
  - MOSI/MISO/SCK/CS pinleri
  - Clock hızı ayarları
  
- **PWM Kontrolü:** Servo motorlar
  - Frekans ve duty cycle hesaplamaları
  - GPIO pin multiplexing

##### 8. **Video İşleme ve Streaming**
- H.264/MP4 encoding (SD kart için)
- MJPEG streaming (XBee için)
- Frame boyutu optimizasyonu (bandwidth limitleri)
- İkili video sistemi (yüksek kalite kayıt + düşük kalite stream)
- FfmpegOutput ve Picamera2 entegrasyonu

#### 🌐 Protokol ve Standartlar

##### **SAHA (Sistemler Arası Haberleşme Ağı)**
- Taşıyıcı-görev yükü arasında basınç verisi iletimi
- Ayrılma sonrası aktif protokol
- Simple text-based format: `SAHA:BASINC2:<pascal>`

##### **IoT S2S (Station-to-Satellite)**
- 412-707m mesafede XBee mesh network
- 2 yer istasyonundan görev yüküne sıcaklık verisi
- Farklı kanallar (12, 13) ile interference önleme

##### **ARAS (Arayüz Alarm Sistemi)**
- 6-bit hata kodu sistemi
- İniş hızı, GPS timeout, ayrılma kontrolü
- Gerçek zamanlı alarm ve bildirim

##### **XBee 802.15.4**
- API Mode 1 frame yapısı
- PAN ID ve mesh network konfigürasyonu
- Power level optimizasyonu (63mW)

#### 🎯 Yazılım Mühendisliği Pratikleri

##### **Versiyon Kontrol (Git)**
- Branch stratejisi (main, feature, bugfix)
- Commit mesaj standartları (emoji ile)
- Code review süreçleri

##### **Test ve Kalibrasyon**
- Unit test yazma (Python unittest)
- Entegrasyon testleri
- Manuel ve otomatik kalibrasyon prosedürleri
- Serial Monitor ile debug

##### **Dokümantasyon**
- README ve markdown dosyaları
- Kod içi docstring'ler (Python)
- Arduino comment standartları
- API dokümantasyonu

#### 🤝 Soft Skills Kazanımları

##### **Ekip Çalışması**
- Farklı disiplinlerden ekip üyeleriyle koordinasyon
- Task dağılımı ve zaman yönetimi
- İletişim ve problem paylaşımı
- Code ownership ve sorumluluk

##### **Problem Çözme**
- Sistemik düşünme ve root cause analizi
- Alternatif çözümler üretme
- Trade-off analizi (performans vs. güvenilirlik)
- Hızlı karar verme (yarışma deadline'ları)

##### **Stres Yönetimi**
- Deadline baskısı altında çalışma
- Son dakika hata giderme
- Test günü operasyonel hazırlık
- Beklenmedik sorunlara adaptasyon

### 📊 Proje İstatistikleri

| Metrik | Değer |
|--------|-------|
| **Geliştirme Süresi** | 6+ ay |
| **Kod Satırı** | 15,000+ |
| **Dosya Sayısı** | 50+ |
| **Test Süresi** | 100+ saat |
| **Sensör Çeşidi** | 8 (BMP280, IMU, GPS, Kamera, ADC, RTC) |
| **Haberleşme Protokolü** | 6 (I2C, UART, SPI, PWM, XBee, SAHA) |
| **Platform** | 3 (Python, C#, Arduino) |
| **Thread Sayısı** | 5+ (paralel işlem) |
| **Telemetri Frekansı** | 1 Hz (kesintisiz) |
| **Video Bandwidth** | ~48 Kbps (optimize) |

### 🏆 Proje Başarıları

- ✅ **Şartname uyumu:** %100 gereksinim karşılama
- ✅ **Hata toleransı:** Sıfır kritik hata
- ✅ **Video sistemi:** İkili sistem (kayıt + stream)
- ✅ **IoT bonus:** 412-707m mesafede başarılı iletişim
- ✅ **ARAS sistemi:** 6-bit hata izleme aktif
- ✅ **Otomasyon:** Tam otomatik uçuş profili
- ✅ **Dokümantasyon:** Kapsamlı teknik raporlama

---

## 📜 Lisans

Bu proje **TÜRKSAT Model Uydu Yarışması 2025** için geliştirilmiştir.

**Takım:** NONGRAVITY (286570)

**Lisans:** MIT License (Açık kaynak)

```
MIT License

Copyright (c) 2025 NONGRAVITY Team

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
```

---

## 📞 İletişim

**Takım:** NONGRAVITY

**Takım Numarası:** 286570

**Yarışma:** TÜRKSAT Model Uydu Yarışması 2025

**GitHub:** [github.com/nurullahsahinn/turksat-model-uydu](https://github.com/nurullahsahinn/turksat-model-uydu)

**E-posta:** [nurullahsahin0088@gmail.com](mailto:nurullahsahin0088@gmail.com)

**Dokümantasyon:** [Wiki sayfası](https://github.com/nurullahsahinn/turksat-model-uydu/wiki)

---



---

## 📊 İstatistikler

- 📝 **Kod Satırları:** ~15,000+
- 🐍 **Python Dosyaları:** 25+
- 🤖 **Arduino Sketch'leri:** 3
- 💻 **C# Dosyaları:** 15+
- 📋 **Dokümantasyon:** 4 MD dosyası
- ⚡ **Gerçek Zamanlı İşlem:** 1 Hz telemetri
- 📹 **Video Akışı:** 2 FPS @ 240x180
- 💾 **Video Kaydı:** 15 FPS @ 640x480

---

<div align="center">

**🛰️ Başarılı Uçuşlar Dileriz! 🚀**

[![GitHub Stars](https://img.shields.io/github/stars/nurullahsahinn/turksat-model-uydu?style=social)](https://github.com/nurullahsahinn/turksat-model-uydu)
[![GitHub Forks](https://img.shields.io/github/forks/nurullahsahinn/turksat-model-uydu?style=social)](https://github.com/nurullahsahinn/turksat-model-uydu)
[![GitHub Issues](https://img.shields.io/github/issues/nurullahsahinn/turksat-model-uydu)](https://github.com/nurullahsahinn/turksat-model-uydu/issues)

---

### 🏷️ Teknoloji Stack

![Python](https://img.shields.io/badge/Python-3.11-blue?logo=python&logoColor=white)
![C#](https://img.shields.io/badge/C%23-.NET%20Framework-purple?logo=csharp&logoColor=white)
![Arduino](https://img.shields.io/badge/Arduino-C++-teal?logo=arduino&logoColor=white)
![Raspberry Pi](https://img.shields.io/badge/Raspberry%20Pi-Zero%202W-red?logo=raspberrypi&logoColor=white)
![XBee](https://img.shields.io/badge/XBee-802.15.4-green?logo=zigbee&logoColor=white)

### 📢 Anahtar Kelimeler

`#TÜRKSAT` `#TEKNOFEST` `#ModelUydu` `#EmbeddedSystems` `#IoT` `#Python` `#CSharp` `#Arduino` `#RaspberryPi` `#XBee` `#Telemetry` `#SpaceTechnology` `#Engineering` `#RealTimeData` `#MultiThreading` `#VideoStreaming` `#SensorFusion` `#WirelessCommunication` `#SystemIntegration` `#FaultTolerant`

</div>

