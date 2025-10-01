#!/usr/bin/env python3
"""
BASIT XBEE TEST - Sadece test mesajı gönder
"""

import serial
import time
import base64

def test_xbee_connection():
    """XBee bağlantısını test et"""
    try:
        print("🔧 XBee Test Başlatılıyor...")
        
        # XBee bağlantısını aç
        ser = serial.Serial('/dev/serial0', 57600, timeout=1)
        print("✅ XBee bağlantısı açıldı")
        
        # Test mesajları gönder
        for i in range(10):
            # Test JPEG data (sahte)
            test_data = b"TEST_JPEG_DATA_" + str(i).encode()
            base64_data = base64.b64encode(test_data).decode('utf-8')
            message = f"#VIDEO:{base64_data}#\n"
            
            # Gönder
            ser.write(message.encode('utf-8'))
            ser.flush()
            
            print(f"📡 Test mesajı {i+1} gönderildi: {len(message)} karakter")
            time.sleep(2)  # 2 saniye bekle
        
        ser.close()
        print("✅ Test tamamlandı")
        
    except Exception as e:
        print(f"❌ Test hatası: {e}")

if __name__ == "__main__":
    test_xbee_connection()

