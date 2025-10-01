#!/bin/bash
echo "🚀 TÜRKSAT Model Uydu Servis Kurulumu"
echo "======================================"

# Gerekli dizinleri kontrol et
if [ ! -d "/home/atugem/TurkSatModelUydu/GorevYukuPi" ]; then
    echo "❌ Proje dizini bulunamadı!"
    exit 1
fi

cd /home/atugem/TurkSatModelUydu/GorevYukuPi

echo "1. Servis dosyasını sistem dizinine kopyalıyor..."
sudo cp gorevyuku.service /etc/systemd/system/

echo "2. Sistem daemon'unu yeniden yüklüyor..."
sudo systemctl daemon-reload

echo "3. Servisi etkinleştiriyor (otomatik başlatma)..."
sudo systemctl enable gorevyuku.service

echo "4. pigpiod servisini etkinleştiriyor..."
sudo systemctl enable pigpiod

echo "5. Servisleri başlatıyor..."
sudo systemctl start pigpiod
sleep 2
sudo systemctl start gorevyuku.service

echo ""
echo "✅ Kurulum tamamlandı!"
echo ""
echo "📋 Kullanım komutları:"
echo "   Durum kontrol: sudo systemctl status gorevyuku"
echo "   Başlat:        sudo systemctl start gorevyuku"
echo "   Durdur:        sudo systemctl stop gorevyuku"
echo "   Logları gör:   sudo journalctl -u gorevyuku -f"
echo "   Yeniden başlat: sudo systemctl restart gorevyuku"
echo ""
echo "🔄 Sistem yeniden başlatıldığında otomatik çalışacak!"