# -*- coding: utf-8 -*-
"""
Birleşik XBee Alıcı Modülü

Bu modül, görev yükünde tek XBee ile tüm haberleşmeyi yönetir:
1. Yer istasyonuna telemetri gönder
2. Yer istasyonundan komut al  
3. Taşıyıcıdan BASINÇ2 al (ayrılma sonrası)
4. IoT#1 ve IoT#2'den sıcaklık al
"""

import serial
import threading
import time
import logging
import struct
import base64
import json
from moduller.yapilandirma import SERIAL_PORT_XBEE, SERIAL_BAUD_XBEE, XBEE_PAN_ID, IS_RASPBERRY_PI

class BirlesikXBeeAlici:
    def __init__(self, command_callback=None, debug=True, simulate=not IS_RASPBERRY_PI):
        """
        Birleşik XBee alıcı başlatma
        
        Args:
            command_callback: Komut geldiğinde çağrılacak fonksiyon
            debug: Debug modu aktif/pasif
            simulate: Simülasyon modu
        """
        self.xbee_port = SERIAL_PORT_XBEE
        self.baud_rate = SERIAL_BAUD_XBEE
        self.debug = debug
        # Eğer Raspberry Pi ise simülasyonu zorla False yap
        self.simulate = False if IS_RASPBERRY_PI else simulate
        self.command_callback = command_callback
        
        # XBee bağlantısı
        self.xbee_serial = None
        self.is_connected = False
        
        # Thread safety için lock
        self._data_lock = threading.Lock()
        
        # Paylaşılan veriler (thread-safe erişim gerekli)
        self.basinc2_veri = {
            'value': 0.0,
            'son_guncelleme': 0
        }
        
        self.iot_verileri = {
            1: {'sicaklik': 0.0, 'son_guncelleme': 0},
            2: {'sicaklik': 0.0, 'son_guncelleme': 0}
        }
        
        # Threading kontrol
        self.running = False
        self.receive_thread = None
        
        # Logger kurulum
        self.setup_logger()
        
    def setup_logger(self):
        """Logger konfigürasyonu"""
        self.logger = logging.getLogger('BirlesikXBeeAlici')
        if not self.logger.handlers:
            handler = logging.StreamHandler()
            formatter = logging.Formatter(
                '%(asctime)s - %(name)s - %(levelname)s - %(message)s'
            )
            handler.setFormatter(formatter)
            self.logger.addHandler(handler)
            self.logger.setLevel(logging.DEBUG if self.debug else logging.INFO)
    
    def connect_xbee(self) -> bool:
        """XBee modüle bağlanma"""
        if self.simulate:
            self.logger.info("Birleşik XBee simülasyon modu - Port bağlantısı atlanıyor")
            self.is_connected = True
            return True
            
        try:
            self.logger.info(f"Birleşik XBee'ye bağlanılıyor: {self.xbee_port}")
            
            self.xbee_serial = serial.Serial(
                port=self.xbee_port,
                baudrate=self.baud_rate,
                timeout=1,
                parity=serial.PARITY_NONE,
                stopbits=serial.STOPBITS_ONE,
                bytesize=serial.EIGHTBITS
            )
            
            if self.xbee_serial.is_open:
                self.is_connected = True
                self.logger.info("✅ Birleşik XBee bağlantısı başarılı!")
                return True
            else:
                self.logger.error("❌ Birleşik XBee port açılamadı!")
                return False
                
        except Exception as e:
            self.logger.error(f"Birleşik XBee bağlantı hatası: {e}")
            self.is_connected = False
            return False
    
    def start_listening(self):
        """Veri dinlemeye başla"""
        if not self.is_connected and not self.simulate:
            self.logger.error("XBee bağlantısı yok, dinleme başlatılamadı")
            return False
        
        try:
            self.running = True
            self.receive_thread = threading.Thread(target=self._listen_xbee_data, daemon=True)
            self.receive_thread.start()
            self.logger.info("✅ Birleşik XBee veri dinleyicisi başlatıldı")
            return True
        except Exception as e:
            self.logger.error(f"XBee dinleyici başlatma hatası: {e}")
            return False
    
    def stop_listening(self):
        """Veri dinlemeyi durdur"""
        try:
            self.running = False
            if self.receive_thread and self.receive_thread.is_alive():
                # 🔧 Thread timeout artırıldı (Analiz4.txt düzeltmesi)
                self.receive_thread.join(timeout=8.0)  # 2s → 8s
            
            if self.xbee_serial and self.xbee_serial.is_open:
                self.xbee_serial.close()
            
            self.logger.info("Birleşik XBee veri dinleyicisi durduruldu")
        except Exception as e:
            self.logger.error(f"XBee dinleyici durdurma hatası: {e}")
    
    def _listen_xbee_data(self):
        """XBee'den gelen verileri dinleyen ana döngü"""
        buffer = ""
        binary_buffer = bytearray()  # API frame'ler için binary buffer
        MAX_BUFFER_SIZE = 4096  # Buffer boyut sınırı (DoS koruması)
        MAX_BINARY_BUFFER_SIZE = 8192  # Binary buffer sınırı
        
        while self.running:
            try:
                if self.simulate:
                    # Simülasyon verileri
                    time.sleep(2.0)
                    
                    # Test BASINÇ2
                    if time.time() % 10 < 5:
                        self._process_message("SAHA:BASINC2:1012.45")
                    
                    # Test IoT (varyasyonlu veriler)
                    import random
                    temp1 = 25.0 + random.uniform(-5.0, 5.0)
                    temp2 = 23.0 + random.uniform(-3.0, 7.0)
                    self._process_message(f"IOT:1:{temp1:.1f}")
                    self._process_message(f"IOT:2:{temp2:.1f}")
                    continue
                
                if self.xbee_serial and self.xbee_serial.in_waiting > 0:
                    data = self.xbee_serial.read(self.xbee_serial.in_waiting)
                    
                    if self.debug and data:
                        print(f"🔍 DEBUG: XBee'den ham veri alındı: {len(data)} bytes")
                        # İlk birkaç byte'a bak
                        if len(data) > 0:
                            if data[0] == 0x7E:
                                print(f"🔍 DEBUG: API frame tespit edildi (0x7E ile başlıyor)")
                            else:
                                try:
                                    print(f"🔍 DEBUG: Text veri: {data.decode('utf-8', errors='ignore')[:100]}")
                                except:
                                    print(f"🔍 DEBUG: Binary veri: {data.hex()[:100]}")
                    
                    # Binary buffer'a ekle (API frame kontrolü için)
                    binary_buffer.extend(data)
                    if len(binary_buffer) > MAX_BINARY_BUFFER_SIZE:
                        binary_buffer = binary_buffer[-MAX_BINARY_BUFFER_SIZE//2:]
                        self.logger.warning("Binary buffer overflow koruması")
                    
                    # API frame kontrolü
                    self._process_binary_buffer(binary_buffer)
                    
                    # Text buffer işleme (transparent mode için)
                    try:
                        decoded_data = data.decode('utf-8', errors='ignore')
                        buffer += decoded_data
                        if len(buffer) > MAX_BUFFER_SIZE:
                            buffer = buffer[-MAX_BUFFER_SIZE//2:]
                            self.logger.warning("Text buffer overflow koruması")
                        
                        # Satır sonu karakterine göre mesajları ayır
                        lines = buffer.split('\n')
                        buffer = lines[-1]  # Son tamamlanmamış satırı sakla
                        
                        # Tamamlanmış satırları işle
                        for line in lines[:-1]:
                            line = line.strip()
                            if line:
                                if self.debug:
                                    print(f"🔍 DEBUG: Text satır işleniyor: '{line}'")
                                self._process_message(line)
                    except Exception as e:
                        if self.debug:
                            print(f"🔍 DEBUG: Text decode hatası: {e}")
                
                time.sleep(0.1)
                
            except UnicodeDecodeError as e:
                self.logger.warning(f"XBee veri decode hatası: {e}")
                buffer = ""  # Buffer'ı temizle
                time.sleep(0.5)
            except serial.SerialException as e:
                self.logger.error(f"XBee seri port hatası: {e}")
                time.sleep(1)
            except Exception as e:
                self.logger.error(f"XBee veri dinleme hatası: {e}")
                time.sleep(1)
    
    def _process_message(self, message):
        """Gelen mesajı işle"""
        try:
            if isinstance(message, bytes):
                # XBee API frame kontrolü (0x7E ile başlar)
                if len(message) > 0 and message[0] == 0x7E:
                    if self.debug:
                        print(f"🔍 DEBUG: XBee API frame tespit edildi: {len(message)} bytes")
                    self._process_api_frame(message)
                    return
                # Binary veri (IoT)
                elif self._is_binary_packet(message):
                    self._process_binary_message(message)
                    return
                else:
                    # Binary'yi string'e çevirmeyi dene
                    try:
                        decoded = message.decode('utf-8', errors='ignore').strip()
                        if decoded:
                            if self.debug:
                                print(f"🔍 DEBUG: Binary string'e çevrildi: '{decoded}'")
                            self._process_message(decoded)  # Recursive call with string
                        return
                    except:
                        if self.debug:
                            print(f"🔍 DEBUG: Binary veri decode edilemedi: {message.hex()}")
                        return
            
            # String mesaj işleme
            if isinstance(message, str):
                message = message.strip()
                if not message:
                    return
            
            # SAHA basınç verisi: "SAHA:BASINC2:1012.45"
            if message.startswith("SAHA:BASINC2:"):
                try:
                    basinc2_deger = float(message.split(":")[2])
                
                    # Thread-safe veri güncellemesi
                    with self._data_lock:
                        self.basinc2_veri = {
                            'value': basinc2_deger,
                            'son_guncelleme': time.time()
                        }
                
                    if self.debug:
                        print(f"📡 SAHA BASINÇ2 alındı: {basinc2_deger} hPa")
                except (IndexError, ValueError) as e:
                    self.logger.error(f"SAHA:BASINC2 parse hatası: {e}")
            
            # IoT string verisi: "IOT:1:25.3"
            elif message.startswith("IOT:"):
                try:
                    parts = message.split(":")
                    if len(parts) >= 3:
                        istasyon_id = int(parts[1])
                        sicaklik = float(parts[2])
                        
                        if istasyon_id in [1, 2]:
                            # Thread-safe veri güncellemesi
                            with self._data_lock:
                                self.iot_verileri[istasyon_id] = {
                                    'sicaklik': sicaklik,
                                    'son_guncelleme': time.time()
                                }
                            
                            if self.debug:
                                print(f"📡 IoT{istasyon_id} sıcaklık alındı: {sicaklik}°C")
                except (IndexError, ValueError) as e:
                    self.logger.error(f"IoT string parse hatası: {e}")
            
            # Komut mesajları
            elif message.startswith("!") and message.endswith("!"):
                if self.debug:
                    print(f"🔍 DEBUG: XBee'den komut alındı: {message}")
                if self.command_callback:
                    self.command_callback(message)
                else:
                    print("⚠️ UYARI: command_callback tanımlanmamış!")
            
        except Exception as e:
            self.logger.error(f"Mesaj işleme hatası: {e}")
    
    def _process_api_frame(self, frame_data: bytes):
        """XBee API frame'ini işle"""
        try:
            if len(frame_data) < 4:
                if self.debug:
                    print("🔍 DEBUG: API frame çok kısa")
                return
            
            # API frame yapısı: 0x7E | Length MSB | Length LSB | Frame Data | Checksum
            if frame_data[0] != 0x7E:
                if self.debug:
                    print("🔍 DEBUG: Geçersiz API frame başlangıcı")
                return
            
            # Frame uzunluğu
            length = (frame_data[1] << 8) | frame_data[2]
            if self.debug:
                print(f"🔍 DEBUG: API frame uzunluğu: {length}")
            
            if len(frame_data) < length + 4:
                if self.debug:
                    print("🔍 DEBUG: API frame eksik")
                return
            
            # Frame tipi
            frame_type = frame_data[3]
            if self.debug:
                print(f"🔍 DEBUG: API frame tipi: 0x{frame_type:02X}")
            
            # RX Indicator (0x90) - Received data
            if frame_type == 0x90:
                # 64-bit source address (8 bytes) + 16-bit source address (2 bytes) + options (1 byte) = 11 bytes
                if length >= 12:  # En az 11 byte header + 1 byte data
                    payload_start = 14  # 3 (header) + 11 (addressing)
                    payload_end = 3 + length - 1  # Checksum hariç
                    
                    if payload_end > payload_start and payload_end < len(frame_data):
                        payload = frame_data[payload_start:payload_end]
                        
                        if self.debug:
                            print(f"🔍 DEBUG: API frame payload: {len(payload)} bytes")
                            try:
                                payload_str = payload.decode('utf-8', errors='ignore')
                                print(f"🔍 DEBUG: API frame içeriği: '{payload_str}'")
                            except:
                                print(f"🔍 DEBUG: API frame binary: {payload.hex()}")
                        
                        # Payload'u string olarak işle
                        try:
                            message = payload.decode('utf-8', errors='ignore').strip()
                            if message:
                                if self.debug:
                                    print(f"🔍 DEBUG: API frame'den mesaj çıkarıldı: '{message}'")
                                self._process_message(message)  # Recursive call
                        except Exception as e:
                            if self.debug:
                                print(f"🔍 DEBUG: API frame payload decode hatası: {e}")
            
            # TX Status (0x8B) - Transmission status
            elif frame_type == 0x8B:
                if self.debug:
                    print("🔍 DEBUG: TX Status frame alındı")
            
            else:
                if self.debug:
                    print(f"🔍 DEBUG: Bilinmeyen API frame tipi: 0x{frame_type:02X}")
                    
        except Exception as e:
            self.logger.error(f"API frame işleme hatası: {e}")
            if self.debug:
                print(f"🔍 DEBUG: API frame işleme hatası: {e}")
    
    def _process_binary_buffer(self, buffer: bytearray):
        """Binary buffer'da API frame'leri ara ve işle"""
        try:
            i = 0
            while i < len(buffer):
                # API frame başlangıcı ara (0x7E)
                if buffer[i] == 0x7E:
                    if i + 3 < len(buffer):  # En az header için yeterli byte var mı?
                        # Frame uzunluğunu oku
                        length = (buffer[i+1] << 8) | buffer[i+2]
                        frame_end = i + 3 + length + 1  # Header(3) + Data(length) + Checksum(1)
                        
                        if frame_end <= len(buffer):
                            # Tam frame mevcut
                            frame_data = buffer[i:frame_end]
                            if self.debug:
                                print(f"🔍 DEBUG: Tam API frame bulundu: {len(frame_data)} bytes")
                            self._process_api_frame(bytes(frame_data))
                            
                            # İşlenen frame'i buffer'dan çıkar
                            del buffer[i:frame_end]
                            continue
                        else:
                            # Frame henüz tam değil, daha fazla veri bekle
                            if self.debug:
                                print(f"🔍 DEBUG: Eksik API frame, bekleniyor: {frame_end - len(buffer)} bytes")
                            break
                    else:
                        # Header bile tam değil
                        break
                else:
                    # 0x7E değil, sonraki byte'a geç
                    i += 1
                    
        except Exception as e:
            if self.debug:
                print(f"🔍 DEBUG: Binary buffer işleme hatası: {e}")
    
    def _process_binary_message(self, data: bytes):
        """Binary IoT mesajını ayrıştır"""
        try:
            # 16-byte IoT paketi format: <StationID><PacketNum><Temp><Battery><Timestamp><Checksum>
            if len(data) != 16:
                if self.debug:
                    print(f"⚠️ Binary paket boyut hatası: {len(data)} (16 olmalı)")
                return
            
            # Struct ile ayrıştır
            station_id, packet_num, temperature, battery, timestamp, checksum = struct.unpack('<BIfBIH', data)
            
            # Checksum doğrulama
            calculated_checksum = sum(data[:14]) & 0xFFFF
            if calculated_checksum != checksum:
                if self.debug:
                    print(f"⚠️ Binary paket checksum hatası: {calculated_checksum} != {checksum}")
                return
            
            # Station ID kontrolü
            if station_id not in [1, 2]:
                if self.debug:
                    print(f"⚠️ Geçersiz station ID: {station_id}")
                return
            
            # Thread-safe veri güncellemesi
            with self._data_lock:
                self.iot_verileri[station_id] = {
                    'sicaklik': temperature,
                    'son_guncelleme': time.time()
                }
            
            if self.debug:
                print(f"📡 IoT{station_id} binary: {temperature:.1f}°C (paket #{packet_num})")
            
        except struct.error as e:
            self.logger.error(f"Binary paket struct hatası: {e}")
        except Exception as e:
            self.logger.error(f"Binary mesaj işleme hatası: {e}")
    
    def _is_binary_packet(self, data: bytes) -> bool:
        """
        Gelen verinin binary IoT paketi olup olmadığını kontrol eder.
        Güvenlik için magic number ve boyut kontrolü yapar.
        """
        try:
            # Boyut kontrolü: tam 16 byte olmalı
            if len(data) != 16:
                return False
            
            # İlk byte station ID olmalı (1 veya 2)
            station_id = data[0]
            if station_id not in [1, 2]:
                return False
            
            # Magic number kontrolü: son 4 byte'ta özel pattern arayabiliriz
            # Veya checksum validation yapabiliriz
            try:
                # Checksum validation test
                calculated_checksum = sum(data[:14]) & 0xFFFF
                received_checksum = struct.unpack('<H', data[14:16])[0]
                
                # Checksum eşleşmesi binary paket olma ihtimalini artırır
                if calculated_checksum == received_checksum:
                    return True
                    
            except:
                pass
            
            # Fallback: yazdırılabilir karakter kontrolü
            printable_count = sum(1 for b in data[:8] if 32 <= b <= 126)
            
            # Binary data genellikle az yazdırılabilir karakter içerir
            if printable_count <= 4:  # Daha katı kontrol
                return True
                
            return False
            
        except Exception:
            return False
    
    def send_telemetry(self, telemetry_data):
        """TIMEOUT DÜZELTMESİ - SD kaydını asla engelleme"""
        # String/bytes dönüşüm güvenliği
        if isinstance(telemetry_data, bytes):
            telemetry_data = telemetry_data.decode('utf-8', errors='ignore')
        elif not isinstance(telemetry_data, str):
            telemetry_data = str(telemetry_data)
        
        if self.simulate:
            print(f"SİMÜLASYON - Telemetri: {telemetry_data[:50]}...")
            return True
        
        # 🔥 KRİTİK: XBee problemi SD kaydını engellememesin
        if not self.xbee_serial or not self.xbee_serial.is_open:
            return True  # ✅ SD kaydet, XBee gönderme
        
        try:
            data_to_send = telemetry_data + '\n'
            self.xbee_serial.write(data_to_send.encode('utf-8'))
            return True
        except:
            # HER TÜRLÜ HATA: SD kaydını engellememe
            return True  # ✅ XBee hatası olsa da SD kaydet
    
    # SAHA (BASINÇ2) verileri için getter'lar
    def is_basinc2_available(self) -> bool:
        """BASINÇ2 verisi mevcut mu?"""
        with self._data_lock:
            if self.basinc2_veri['son_guncelleme'] is None:
                return False
        
        # Son 10 saniye içinde güncelleme var mı?
        return (time.time() - self.basinc2_veri['son_guncelleme']) < 10.0
    
    def get_basinc2_value(self) -> float:
        """BASINÇ2 değerini al (hPa)"""
        with self._data_lock:
            if self.is_basinc2_available():
                return self.basinc2_veri['value']
            else:
                return 0.0
    
    # IoT verileri için getter'lar
    def get_iot_data(self, istasyon_id: int) -> float:
        """IoT istasyon sıcaklık verisini al"""
        with self._data_lock:
            if istasyon_id not in [1, 2]:
                return 0.0
        
        veri = self.iot_verileri[istasyon_id]
        if veri['son_guncelleme'] is None:
            return 0.0
        
        # Son 30 saniye içinde güncelleme var mı?
        if (time.time() - veri['son_guncelleme']) > 30.0:
            return 0.0
        
        return veri['sicaklik']
    
    def get_iot_temperatures(self):
        """
        IoT istasyonlarının sıcaklık verilerini tuple olarak döndürür
        Ana programda kullanılan method (compatibility için)
        """
        with self._data_lock:
            return (self.get_iot_data(1), self.get_iot_data(2))
    
    def get_status(self) -> dict:
        """Birleşik XBee durumunu al"""
        with self._data_lock:
            return {
                'baglanti': self.is_connected,
                'dinleme': self.running,
                'basinc2_mevcut': self.is_basinc2_available(),
                'basinc2_deger': self.get_basinc2_value(),
                'iot1_sicaklik': self.get_iot_data(1),
                'iot2_sicaklik': self.get_iot_data(2),
                'iot1_aktif': (time.time() - self.iot_verileri[1].get('son_guncelleme', 0)) < 30,
                'iot2_aktif': (time.time() - self.iot_verileri[2].get('son_guncelleme', 0)) < 30
            }

# Test için örnek kullanım
if __name__ == '__main__':
    def test_command_handler(command):
        print(f"Test komut işleniyor: {command}")
    
    print("Birleşik XBee Alıcı modülü test ediliyor...")
    
    xbee = BirlesikXBeeAlici(
        command_callback=test_command_handler,
        debug=True, 
        simulate=True
    )
    
    if xbee.connect_xbee():
        xbee.start_listening()
        
        # 60 saniye test et
        try:
            for i in range(60):
                time.sleep(1)
                status = xbee.get_status()
                print(f"Durum: BASINÇ2={status['basinc2_deger']:.2f}, "
                      f"IoT1={status['iot1_sicaklik']:.1f}°C, "
                      f"IoT2={status['iot2_sicaklik']:.1f}°C")
                
        except KeyboardInterrupt:
            print("Test durduruldu")
        finally:
            xbee.stop_listening()
            print("Test tamamlandı.")
    else:
        print("XBee bağlantısı başarısız.") 