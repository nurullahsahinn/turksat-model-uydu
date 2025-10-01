/*******************************************************************************
 * IoTStation2_Mega.ino - TÜRKSAT Model Uydu Yarışması
 * IoT S2S (Stations to Satellite) Veri Transferi İstasyonu #2
 * 
 * ✅ ARDUINO MEGA OPTİMİZE EDİLMİŞ VERSİYON
 * 
 * Bu yazılım, XBee 3 Pro modülü kullanarak 400-700m uzaklıktaki 
 * görev yüküne kablosuz sıcaklık verisi gönderir.
 * 
 * MESAFE HESABI:
 * - IoT istasyonu → Ana yer istasyonu: 100m yatay
 * - Görev yükü yüksekliği: 400-700m dikey  
 * - Gerçek mesafe: √(100² + 400²) = ~412m (minimum)
 *                  √(100² + 700²) = ~707m (maksimum)
 * 
 * MEVCUT DONANIM (ARDUINO MEGA UYUMLU):
 * - Arduino Mega 2560 (ATMEGA2560) ✅ YENİ PLATFORM
 * - XBee 3 Pro (63mW çıkış gücü, 2-4km menzil) ✅ MEVCUT
 * - BMP280 Basınç/Sıcaklık Sensörü ✅ MEVCUT (DS18B20 yerine)
 * - Yagi Anten (25dBi kazanç) ✅ MEVCUT
 * - NCR18650B Li-Ion Pil + LM2596/LM2577 Çeviriciler ✅ MEVCUT
 * 
 * MEGA AVANTAJLARı:
 * - Hardware Serial1 (Pin 18/19) - SoftwareSerial yerine
 * - 256KB Flash (8x daha fazla)
 * - 8KB SRAM (4x daha fazla)
 * - 54 dijital pin + 16 analog pin
 * - 4 adet UART portu
 ******************************************************************************/

#include <Wire.h>
#include <Adafruit_BMP280.h>

//************************** Pin Tanımlamaları (MEGA) **************************//
// MEGA Hardware Serial1 kullanımı - Pin tanımı gerekmez (otomatik 18/19)
#define BMP280_SDA_PIN      20  // Mega I2C SDA (Pin 20)
#define BMP280_SCL_PIN      21  // Mega I2C SCL (Pin 21)
#define LED_STATUS_PIN      13  // Durum LED (Arduino Mega onboard)
#define LED_TX_PIN          5   // Veri gönderme LED
#define BATTERY_PIN         A0  // Pil seviyesi analog okuma (ADS1115 yerine basit ADC)
#define XBEE_RESET_PIN      6   // XBee Reset pin (opsiyonel)

//************************** Sistem Sabitleri ***************************//
#define STATION_ID          2       // İstasyon kimliği (2)
#define SEND_INTERVAL       1000    // Veri gönderme aralığı (ms) - 1 Hz
#define BMP280_I2C_ADDRESS  0x77    // BMP280 I2C adresi (İstasyon 2: 0x77)
#define XBEE_BAUD_RATE      57600    // XBee iletişim hızı
#define BATTERY_VOLTAGE_DIV 2.5     // NCR18650B pil için voltaj bölücü faktörü

// XBee Hedef Adresleri (DOĞRU MAC ADRESİ - 27 TEMMUZ 2025) - İstasyon 2
#define SATELLITE_ADDR_HIGH 0x0013A200  // Görev yükü XBee üst adres  
#define SATELLITE_ADDR_LOW  0x42677E8F  // Görev yükü XBee gerçek MAC adresi
#define SATELLITE_ADDR_16   0x0001      // Görev yükü MY adresi (XCTU: MY=1)

// XBee Network Ayarları (XCTU ile senkronize) - İstasyon 2 Mega
#define XBEE_PAN_ID         0x6570      // PAN ID: 6570 (286570 takım numarası)
#define XBEE_CHANNEL        0x0D        // Kanal D (13) - 2.413 GHz (İstasyon #2)
#define XBEE_MY_ADDRESS     0x0005      // Bu istasyon MY=5 (XCTU ayarı)

//************************** Global Nesneler ve Değişkenler *****************//
// ✅ MEGA ADVANTAGE: Hardware Serial1 kullanımı (SoftwareSerial yerine)
#define xbeeSerial Serial1  // Mega Hardware Serial1 (Pin 18/19)

Adafruit_BMP280 bmp280; // BMP280 sensörü (sıcaklık ve basınç)

unsigned long lastSendTime = 0;
unsigned long packetCounter = 0;
float currentTemperature = 0.0;
float currentPressure = 0.0;
int batteryLevel = 0;
bool xbeeReady = false;
bool bmp280Ready = false;

// IoT Veri Paketi Formatı (TÜRKSAT Uyumlu - SADECE SICAKLIK)
struct IoTDataPacket {
    uint8_t stationID;
    uint32_t packetNumber;
    float temperature;     // BMP280'den sadece sıcaklık ✅ ŞARTNAME GEREĞİ
    uint8_t batteryLevel;
    uint32_t timestamp;
    uint16_t checksum;
};

//************************** XBee Kurulum (MEGA Hardware Serial) **************//
void setupXBee() {
  // ✅ MEGA ADVANTAGE: Hardware Serial1 kullanımı
  Serial.println("XBee 3 Pro modülü başlatılıyor... (İstasyon #2 - MEGA Hardware Serial1)");
  
  // Hardware Serial1 başlat (Pin 18/19 otomatik)
  xbeeSerial.begin(XBEE_BAUD_RATE);
  delay(100);  // Stabilizasyon
  
  xbeeReady = true;  // XCTU konfigürasyonu ile hazır
  Serial.println("✅ XBee hazır (İstasyon #2 - MEGA Serial1 aktif)");
}

//************************** Ana Kurulum Fonksiyonu **********************//
void setup() {
    // ✅ MEGA: Serial Monitor için Serial0 kullan
    Serial.begin(57600);
    Serial.println("TÜRKSAT IoT İstasyonu #2 (BMP280+XBee Pro+MEGA) Başlatılıyor...");
    Serial.println("✅ Platform: Arduino Mega 2560 (Hardware Serial1)");
    Serial.println("Hedef: Görev yükü (400-700m yükseklik + 100m yatay)");
    
    // Pin konfigürasyonları
    pinMode(LED_STATUS_PIN, OUTPUT);
    pinMode(LED_TX_PIN, OUTPUT);
    pinMode(BATTERY_PIN, INPUT);
    
    // LED başlangıç testi (2 kez - İstasyon 2 Mega)
    for (int i = 0; i < 2; i++) {
        digitalWrite(LED_STATUS_PIN, HIGH);
        digitalWrite(LED_TX_PIN, HIGH);
        delay(500);
        digitalWrite(LED_STATUS_PIN, LOW);
        digitalWrite(LED_TX_PIN, LOW);
        delay(500);
    }
    
    // ✅ MEGA: I2C başlat (Pin 20/21 otomatik)
    Wire.begin();
    
    // BMP280 sensörünü başlat
    if (!bmp280.begin(BMP280_I2C_ADDRESS)) {
        Serial.println("❌ HATA: BMP280 sensörü bulunamadı!");
        Serial.println("  - I2C adres kontrol edilsin: 0x76 veya 0x77");
        Serial.println("  - MEGA Kablolama: SDA=Pin20, SCL=Pin21");
        bmp280Ready = false;
    } else {
        bmp280Ready = true;
        Serial.println("✅ BMP280 sensörü başarıyla başlatıldı (MEGA I2C)");
        Serial.print("   Sensör ID: 0x");
        Serial.println(bmp280.sensorID(), HEX);
    }
    
    // XBee modülünü başlat
    setupXBee();
    
    Serial.println("IoT İstasyonu #2 (MEGA+BMP280+XBee Pro) hazır!");
    Serial.println("MEGA Özellikleri:");
    Serial.println("- Hardware Serial1 (Pin 18/19) ✅");
    Serial.println("- 256KB Flash + 8KB SRAM ✅"); 
    Serial.println("- I2C Pin 20/21 ✅");
    Serial.println("Mesafe analizi:");
    Serial.println("- Minimum: 412m (400m yükseklik)");  
    Serial.println("- Maksimum: 707m (700m yükseklik)");
    Serial.println("- XBee Pro menzil: 2-4km ✓");
    Serial.println("- Kanal: 13 (2.413 GHz)");
    Serial.println("- Sensör: BMP280 (sıcaklık + basınç)");
}

//************************** Ana Döngü ***********************************//
void loop() {
    // Her saniye veri gönder
    if (millis() - lastSendTime >= SEND_INTERVAL) {
        
        // Sensör verilerini oku
        readBMP280Sensors();
        readBatteryLevel();
        
        // XBee ile veri gönder
        if (xbeeReady && bmp280Ready) {
            sendDataToSatellite();
        } else {
            if (!xbeeReady) {
                Serial.println("XBee hazır değil, yeniden başlatılıyor... (MEGA İst#2)");
                setupXBee();
            }
            if (!bmp280Ready) {
                Serial.println("BMP280 hazır değil, yeniden başlatılıyor... (MEGA İst#2)");
                bmp280Ready = bmp280.begin(BMP280_I2C_ADDRESS);
            }
        }
        
        lastSendTime = millis();
    }
    
    // Durum LED'i (İstasyon 2 Mega: çift yanıp sönme)
    static unsigned long ledTime = 0;
    static int ledState = 0;
    if (millis() - ledTime > 250) {
        ledState = (ledState + 1) % 4;
        digitalWrite(LED_STATUS_PIN, (ledState == 0 || ledState == 1) ? HIGH : LOW);
        ledTime = millis();
    }
    
    // ✅ MEGA: Hardware Serial1'den gelen yanıtları oku
    if (xbeeSerial.available()) {
        Serial.print("XBee RX (MEGA İst#2): ");
        while (xbeeSerial.available()) {
            Serial.write(xbeeSerial.read());
        }
        Serial.println();
    }
    
    delay(50); // CPU yükünü azalt
}

//************************** BMP280 Sensör Okuma ****************************//
void readBMP280Sensors() {
    if (!bmp280Ready) {
        currentTemperature = -999.0;
        currentPressure = -999.0;
        Serial.println("HATA: BMP280 sensörü hazır değil! (MEGA İst#2)");
        return;
    }
    
    // ✅ ŞARTNAME: Sadece sıcaklık oku (Celsius)
    currentTemperature = bmp280.readTemperature();
    
    // Hatalı okuma kontrolü
    if (isnan(currentTemperature)) {
        currentTemperature = -999.0;
        Serial.println("HATA: BMP280 sıcaklık sensöründen geçersiz veri! (MEGA İst#2)");
        bmp280Ready = false;
        return;
    }
    
    Serial.print("BMP280 (MEGA İst#2) - Sıcaklık: ");
    Serial.print(currentTemperature);
    Serial.println(" °C (IoT S2S)");
}

//************************** Pil Seviyesi Okuma ***************************//
void readBatteryLevel() {
    int analogValue = analogRead(BATTERY_PIN);
    
    // NCR18650B Li-Ion pil için voltaj bölücü
    // 4.2V max -> 5V ADC için R1=2.2k, R2=10k voltaj bölücü
    float voltage = (analogValue * 5.0) / 1024.0;
    float actualVoltage = voltage * BATTERY_VOLTAGE_DIV; // Voltaj bölücü faktörü
    
    // NCR18650B Li-Ion: 4.2V = %100, 3.0V = %0
    float minVoltage = 3.0;
    float maxVoltage = 4.2;
    
    batteryLevel = ((actualVoltage - minVoltage) / (maxVoltage - minVoltage)) * 100.0;
    batteryLevel = constrain(batteryLevel, 0, 100);
    
    Serial.print("NCR18650B Pil (MEGA İst#2): ");
    Serial.print(batteryLevel);
    Serial.print("% (");
    Serial.print(actualVoltage);
    Serial.println("V)");
}

//************************** XBee Veri Gönderme ***************************//
void sendDataToSatellite() {
    packetCounter++;
    
    // Veri paketini hazırla
    IoTDataPacket packet;
    packet.stationID = STATION_ID;
    packet.packetNumber = packetCounter;
    packet.temperature = currentTemperature;  // ✅ ŞARTNAME: Sadece sıcaklık
    packet.batteryLevel = batteryLevel;
    packet.timestamp = millis();
    
    // Checksum hesapla
    packet.checksum = calculateChecksum((uint8_t*)&packet, sizeof(packet) - 2);
    
    // XBee API Frame oluştur
    sendXBeeAPIFrame((uint8_t*)&packet, sizeof(packet));
    
    // TX LED sinyali (2 kez - İstasyon 2 Mega)
    for (int i = 0; i < 2; i++) {
        digitalWrite(LED_TX_PIN, HIGH);
        delay(50);
        digitalWrite(LED_TX_PIN, LOW);
        delay(50);
    }
    
    Serial.print("Görev yüküne gönderildi (MEGA İst#2): İst=");
    Serial.print(packet.stationID);
    Serial.print(", Paket=");
    Serial.print(packet.packetNumber);
    Serial.print(", Sıcaklık=");
    Serial.print(packet.temperature);
    Serial.print("°C, Pil=");
    Serial.print(packet.batteryLevel);
    Serial.print("%, Mesafe=412-707m");
    Serial.println();
}

//************************** XBee API Frame Gönderme (MEGA Hardware Serial) **//
void sendXBeeAPIFrame(uint8_t* data, int dataLen) {
    // XBee API Frame formatı:
    // 0x7E | Length MSB | Length LSB | Frame Type | Frame ID | 64-bit Addr | 16-bit Addr | Options | Data | Checksum
    
    int frameLen = 14 + dataLen; // Header(14) + Data
    uint8_t checksum = 0;
    
    // ✅ MEGA: Hardware Serial1 kullanımı
    // Start delimiter
    xbeeSerial.write(0x7E);
    
    // Length (MSB, LSB)
    xbeeSerial.write((frameLen >> 8) & 0xFF);
    xbeeSerial.write(frameLen & 0xFF);
    
    // Frame Type (Transmit Request)
    xbeeSerial.write(0x10);
    checksum += 0x10;
    
    // Frame ID (İstasyon 2 Mega)
    xbeeSerial.write(0x02);
    checksum += 0x02;
    
    // 64-bit Destination Address
    uint8_t addr64[] = {
        (SATELLITE_ADDR_HIGH >> 24) & 0xFF,
        (SATELLITE_ADDR_HIGH >> 16) & 0xFF,
        (SATELLITE_ADDR_HIGH >> 8) & 0xFF,
        SATELLITE_ADDR_HIGH & 0xFF,
        (SATELLITE_ADDR_LOW >> 24) & 0xFF,
        (SATELLITE_ADDR_LOW >> 16) & 0xFF,
        (SATELLITE_ADDR_LOW >> 8) & 0xFF,
        SATELLITE_ADDR_LOW & 0xFF
    };
    
    for (int i = 0; i < 8; i++) {
        xbeeSerial.write(addr64[i]);
        checksum += addr64[i];
    }
    
    // 16-bit Network Address
    xbeeSerial.write((SATELLITE_ADDR_16 >> 8) & 0xFF);
    xbeeSerial.write(SATELLITE_ADDR_16 & 0xFF);
    checksum += (SATELLITE_ADDR_16 >> 8) & 0xFF;
    checksum += SATELLITE_ADDR_16 & 0xFF;
    
    // Broadcast Radius & Options
    xbeeSerial.write(0x00); // Broadcast radius
    xbeeSerial.write(0x00); // Options
    checksum += 0x00;
    checksum += 0x00;
    
    // Data
    for (int i = 0; i < dataLen; i++) {
        xbeeSerial.write(data[i]);
        checksum += data[i];
    }
    
    // Checksum
    checksum = 0xFF - checksum;
    xbeeSerial.write(checksum);
}

//************************** Checksum Hesaplama ***************************//
uint16_t calculateChecksum(uint8_t* data, int len) {
    uint16_t checksum = 0;
    for (int i = 0; i < len; i++) {
        checksum += data[i];
    }
    return checksum & 0xFFFF;
}

//************************** Durum Raporu (MEGA Enhanced) *******************//
void printStatus() {
    Serial.println("=== IoT İstasyonu #2 (MEGA+BMP280+XBee Pro) Durum ===");
    Serial.print("Platform: Arduino Mega 2560 - Hardware Serial1\n");
    Serial.print("Paket Sayısı: ");
    Serial.println(packetCounter);
    Serial.print("Sıcaklık: ");
    Serial.print(currentTemperature);
    Serial.println(" °C");
    Serial.print("Basınç: ");
    Serial.print(currentPressure / 100.0);
    Serial.println(" hPa");
    Serial.print("Pil Seviyesi (NCR18650B): ");
    Serial.print(batteryLevel);
    Serial.println("%");
    Serial.print("BMP280 Durumu: ");
    Serial.println(bmp280Ready ? "HAZIR" : "HATA");
    Serial.print("XBee Durumu: ");
    Serial.println(xbeeReady ? "HAZIR" : "HATA");
    Serial.println("Hedef Mesafe: 412-707m (Görev Yükü)");
    Serial.println("Kanal: 13 (2.413 GHz)");
    Serial.println("I2C: Pin 20/21 (MEGA)");
    Serial.println("XBee: Hardware Serial1 (Pin 18/19)");
    Serial.println("===============================================");
}