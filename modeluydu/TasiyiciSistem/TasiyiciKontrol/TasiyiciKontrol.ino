/*******************************************************************************
 * TasiyiciKontrol.ino - TAŞIYICI KONTROL KODU
 * TÜRKSAT Model Uydu Yarışması
 * 
 * Bu yazılım, SADECE TAŞIYICI sistemin görevlerini yönetir.
 * 
 * MEVCUT DONANIM ✅ UYUMLU:
 * - Arduino Nano (ATMEGA328) ✅ MEVCUT
 * - BMP280 Basınç Sensörü ✅ MEVCUT (I2C 0x77)
 * - SG90 Servo Motor ✅ MEVCUT (ayrılma mekanizması)
 * - XBee 3 Pro ✅ MEVCUT (SAHA protokolü)
 * - Buzzer ✅ MEVCUT (kurtarma sinyali)
 * 
 * Sorumlulukları:
 * 1. Kendi basınç sensöründen (BMP280) irtifa verisi okumak.
 * 2. Şartnamede belirtilen irtifada (400m) görev yükü ayrılma mekanizmasını (servo) tetiklemek.
 * 3. ✅ ŞARTNAME GEREKSİNİM 33: Ölçtüğü basınç verisini SAHA protokolü ile Görev Yükü'ne
 *    SADECE AYRILMA SONRASI gönderir (XBee ile)
 * 4. Yere indikten sonra kurtarma için buzzer ile sinyal vermek.
 ******************************************************************************/

// Gerekli Kütüphaneler
#include <Wire.h>
#include <Adafruit_BMP280.h>
#include <Servo.h>
#include <SoftwareSerial.h>

//************************** Pin Tanımlamaları **************************//
#define AYRILMA_SERVO_PIN   9   // Ayrılma mekanizmasını kontrol eden servo motorun pini
#define BUZZER_PIN          4   // Kurtarma sinyali için buzzer pini
#define XBEE_RX_PIN         2   // XBee DOUT -> Arduino Pin 2
#define XBEE_TX_PIN         3   // XBee DIN  -> Arduino Pin 3

//************************** Sistem Sabitleri ***************************//
// SAHA (Sistemler Arası Haberleşme Ağı) Protokolü Ayarları
#define SAHA_BAUD_RATE      57600 // XBee haberleşme hızı
#define SAHA_GONDERIM_ARALIGI 200 // Basınç verisini gönderme sıklığı (ms) - 5 Hz

// Ayrılma Mekanizması Ayarları
#define AYRILMA_IRTIFASI    400.0f // Görev yükünün ayrılacağı irtifa (metre)
#define SERVO_KILIT_ACISI   0      // Servonun görev yükünü kilitlediği pozisyon
#define SERVO_ACIK_ACISI    90     // Servonun görev yükünü serbest bıraktığı pozisyon

// Fiziksel ve Çevresel Sabitler
// Coğrafi kalibrasyon sistemi (yer istasyonu ile uyumlu)
float DENIZ_SEVIYESI_BASINC = 911.75f; // Ankara varsayılan (850m): 1013.25 - (850/8.3)

// 🔧 HARDCODED VALUES FIX: Magic numbers için sabitler
#define BMP280_I2C_ADDRESS  0x77     // Taşıyıcı BMP280 I2C adresi
#define ERROR_TONE_FREQ     1000     // Hata tonu frekansı (Hz)
#define ERROR_TONE_DURATION 200      // Hata tonu süresi (ms)
#define ERROR_REPEAT_DELAY  500      // Hata tonu tekrar aralığı (ms)
#define RESCUE_BEEP_DELAY   1500     // Kurtarma sinyali aralığı (ms)

// Kalibrasyon komutları için
bool kalibrasyon_alinmis = false;
float kalibre_basinc = 911.75f; // Varsayılan Ankara

// Kalibrasyon fonksiyonu
void kalibre_et_basinc(float yeni_basinc) {
  if (yeni_basinc > 800.0 && yeni_basinc < 1100.0) { // Geçerli aralık
    kalibre_basinc = yeni_basinc;
    DENIZ_SEVIYESI_BASINC = yeni_basinc;
    kalibrasyon_alinmis = true;
    Serial.print("✅ Kalibrasyon güncellemesi: ");
    Serial.print(yeni_basinc);
    Serial.println(" hPa");
  } else {
    Serial.println("❌ Geçersiz kalibrasyon değeri!");
  }
}

// XBee Adresleri (DOĞRU MAC ADRESİ - 27 TEMMUZ 2025)
#define PAYLOAD_ADDR_HIGH 0x0013A200  // Görev yükü XBee üst adres
#define PAYLOAD_ADDR_LOW  0x42677E8F  // Görev yükü XBee gerçek MAC adresi
#define PAYLOAD_ADDR_16   0x0001      // Görev yükü MY adresi (XCTU: MY=1)

// XBee Network Ayarları (XCTU ile senkronize) - Taşıyıcı
#define XBEE_PAN_ID         0x6570      // PAN ID: 6570 (286570 takım numarası)
#define XBEE_CHANNEL        0x0E        // Kanal E (14) - 2.414 GHz (SAHA kanalı)
#define XBEE_MY_ADDRESS     0x0003      // Bu sistem MY=3 (XCTU ayarı)

//************************** Global Nesneler ve Değişkenler *****************//
Adafruit_BMP280 carrier_bmp; // Taşıyıcıya ait basınç sensörü nesnesi
Servo ayrilmaServo;          // Ayrılma servosu nesnesi
SoftwareSerial xbeeSerial(XBEE_RX_PIN, XBEE_TX_PIN); // XBee haberleşme

bool ayrilmaGerceklesti = false; // Ayrılma işleminin durumunu tutan bayrak
bool kurtarmaModuAktif = false;  // Kurtarma modunun durumunu tutan bayrak
unsigned long sonSahaGonderimZamani = 0; // En son ne zaman SAHA verisi gönderildiğini tutar
bool xbeeHazir = false; // XBee hazır durumu

void setup() {
  // Debug için seri iletişimi başlat
  Serial.begin(SAHA_BAUD_RATE);
  
  // XBee seri haberleşme başlat
  xbeeSerial.begin(SAHA_BAUD_RATE);
  
  // Basınç sensörünü başlat
  if (!carrier_bmp.begin(BMP280_I2C_ADDRESS)) { // 🔧 HARDCODED FIX: Sabit kullan
    Serial.println("❌ HATA: BMP280 sensörü bulunamadı (0x77 adresi)!");
    while (1) {
      tone(BUZZER_PIN, ERROR_TONE_FREQ, ERROR_TONE_DURATION);  // 🔧 HARDCODED FIX
      delay(ERROR_REPEAT_DELAY);  // 🔧 HARDCODED FIX
    }
  } else {
    Serial.println("✅ BMP280 sensörü başarıyla başlatıldı (0x77 adresi)");
  }

  // Ayrılma servosu pinini ayarla ve kilitli pozisyona getir
  ayrilmaServo.attach(AYRILMA_SERVO_PIN);
  ayrilmaServo.write(SERVO_KILIT_ACISI);

  // Buzzer pini çıkış olarak ayarla
  pinMode(BUZZER_PIN, OUTPUT);
  
  // XBee konfigürasyonu
  setupXBee();
  
  Serial.println("✅ Taşıyıcı sistem hazır - SAHA protokolü ayrılma sonrası aktif");
}

void loop() {
  // 0. KALİBRASYON KOMUTLARINI KONTROL ET (Serial Monitor)
  if (Serial.available() > 0) {
    String gelen_komut = Serial.readStringUntil('\n');
    gelen_komut.trim();
    
    // Kalibrasyon komutu: #CALIB_PRESSURE:911.75:850#
    if (gelen_komut.startsWith("#CALIB_PRESSURE:")) {
      int ilk_nokta = gelen_komut.indexOf(':', 16);
      int ikinci_nokta = gelen_komut.indexOf(':', ilk_nokta + 1);
      
      if (ilk_nokta > 0 && ikinci_nokta > 0) {
        float yeni_basinc = gelen_komut.substring(16, ilk_nokta).toFloat();
        int rakim = gelen_komut.substring(ilk_nokta + 1, ikinci_nokta).toInt();
        
        kalibre_et_basinc(yeni_basinc);
        Serial.print("📍 Şehir rakımı: ");
        Serial.print(rakim);
        Serial.println("m");
      }
    }
    // Manuel ayrılma komutu (Serial Monitor)
    else if (gelen_komut == "AYIRMA" && !ayrilmaGerceklesti) {
      ayrilmaServo.write(SERVO_ACIK_ACISI);
      ayrilmaGerceklesti = true;
      Serial.println("🚨 MANUEL AYRILMA TETIKLENDI! (Serial)");
    }
  }

  // 0.5. XBee'DEN GELEN KOMUTLARI KONTROL ET (Görev Yükünden)
  if (xbeeSerial.available() > 0) {
    String xbee_komut = xbeeSerial.readStringUntil('\n');
    xbee_komut.trim();
    
    Serial.print("📡 XBee'den komut alındı: ");
    Serial.println(xbee_komut);
    
    // Manuel ayrılma komutu (XBee - Görev Yükünden)
    if (xbee_komut == "AYIRMA" && !ayrilmaGerceklesti) {
      ayrilmaServo.write(SERVO_ACIK_ACISI);
      ayrilmaGerceklesti = true;
      Serial.println("🚨 XBee MANUEL AYRILMA KOMUTU ALINDI!");
      Serial.println("✅ Görev yükü taşıyıcıdan ayrıldı!");
    }
    // Diğer XBee komutları burada işlenebilir
  }

  // 1. TAŞIYICI İRTİFASINI HESAPLA (kalibre basınç ile)
  float basinc_hpa = carrier_bmp.readPressure() / 100.0F; // hPa cinsinden oku
  float basinc_pascal = basinc_hpa * 100.0F; // 🔧 ŞARTNAME: Pascal birimine çevir
  float irtifa = carrier_bmp.readAltitude(kalibre_basinc); // Kalibre basınç kullan

  // 2. AYRILMA MANTIĞINI KONTROL ET
  if (!ayrilmaGerceklesti && irtifa >= AYRILMA_IRTIFASI) {
    ayrilmaServo.write(SERVO_ACIK_ACISI); // Görev yükünü serbest bırak
    ayrilmaGerceklesti = true;
    Serial.println("🚨 OTOMATİK AYRILMA GERÇEKLEŞTI - 400m!");
  }

  // 3. ✅ ŞARTNAME GEREKSİNİM 33: SAHA PROTOKOLÜ - SADECE AYRILMA SONRASI
  if (ayrilmaGerceklesti && xbeeHazir && 
      (millis() - sonSahaGonderimZamani > SAHA_GONDERIM_ARALIGI)) {
    
    // BASINÇ2 verisini XBee ile görev yüküne gönder (ŞARTNAME: Pascal birimi)
    String sahaMessage = "SAHA:BASINC2:" + String(basinc_pascal, 0); // Pascal, ondalıksız
    xbeeSerial.println(sahaMessage);
    
    sonSahaGonderimZamani = millis();
    Serial.println("📡 SAHA: " + sahaMessage + " Pa (Ayrılma sonrası)");
  }
  
  // 4. KURTARMA MANTIĞINI KONTROL ET
  if (ayrilmaGerceklesti && !kurtarmaModuAktif && irtifa < 10.0) {
      kurtarmaModuAktif = true;
      Serial.println("🔍 KURTARMA MODU AKTİF");
  }

  // Kurtarma modu aktifse, periyodik olarak buzzer ile sinyal ver
  if (kurtarmaModuAktif) {
    tone(BUZZER_PIN, 1500, 500); // 1.5 kHz frekansta yarım saniye çal
          delay(RESCUE_BEEP_DELAY);  // 🔧 HARDCODED FIX: Sabit kullan
  }

  delay(10); 
}

//************************** XBee Kurulum ********************************//
void setupXBee() {
  // 🔧 XCTU ile konfigüre edilmiş - AT komutlarına gerek yok (Analiz4.txt)
  Serial.println("XBee SAHA modülü başlatılıyor... (XCTU konfigürasyonlu)");
  
  // Sadece seri haberleşme başlat - konfigürasyon XCTU'da yapıldı
  delay(100);  // Stabilizasyon
  
  // XBee hazır durumunu ayarla
  xbeeHazir = true;  // 🔧 DÜZELTME: XBee hazır - SAHA protokolü aktif edilebilir
  
  Serial.println("✅ XBee hazır (XCTU konfigürasyonu aktif)");
}


// 🔧 waitForXBeeResponse fonksiyonu kaldırıldı - XCTU konfigürasyonu ile gerek yok 