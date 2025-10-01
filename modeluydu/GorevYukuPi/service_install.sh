#!/bin/bash
# -*- coding: utf-8 -*-
"""
TÜRKSAT Model Uydu Yarışması - Görev Yükü Service Kurulum Scripti
Bu script gorevyuku.service'i sisteme kurar ve yapılandırır
"""

# Renk kodları
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

# Logo ve başlık
echo -e "${BLUE}======================================${NC}"
echo -e "${BLUE}🛰️  TÜRKSAT MODEL UYDU YARIŞMASI${NC}"
echo -e "${BLUE}📡 Görev Yükü Service Kurulum v6.0${NC}"
echo -e "${BLUE}======================================${NC}"

# Root kontrolü
if [[ $EUID -eq 0 ]]; then
   echo -e "${RED}❌ Bu scripti root olarak çalıştırmayın!${NC}"
   echo -e "${YELLOW}   Kullanım: ./service_install.sh${NC}"
   exit 1
fi

# Dosya varlık kontrolü
SERVICE_FILE="./gorevyuku.service"
if [ ! -f "$SERVICE_FILE" ]; then
    echo -e "${RED}❌ gorevyuku.service dosyası bulunamadı!${NC}"
    echo -e "${YELLOW}   Bu scripti GorevYukuPi/ klasöründe çalıştırın${NC}"
    exit 1
fi

echo -e "${YELLOW}🔍 Sistem kontrolleri yapılıyor...${NC}"

# Kullanıcı kontrolü
if [ "$USER" != "atugem" ]; then
    echo -e "${YELLOW}⚠️  Uyarı: Mevcut kullanıcı '$USER', beklenen 'atugem'${NC}"
    echo -e "${YELLOW}   Service dosyası 'atugem' kullanıcısı için yapılandırılmış${NC}"
    read -p "Devam etmek istiyor musunuz? (y/N): " -n 1 -r
    echo
    if [[ ! $REPLY =~ ^[Yy]$ ]]; then
        exit 1
    fi
fi

# Grup üyeliklerini kontrol et
echo -e "${BLUE}👥 Kullanıcı grup üyelikleri kontrol ediliyor...${NC}"
REQUIRED_GROUPS=("dialout" "gpio" "i2c" "spi" "video" "audio")
MISSING_GROUPS=()

for group in "${REQUIRED_GROUPS[@]}"; do
    if groups $USER | grep -q "\b$group\b"; then
        echo -e "${GREEN}✅ $group grubu - OK${NC}"
    else
        echo -e "${RED}❌ $group grubu - EKSİK${NC}"
        MISSING_GROUPS+=($group)
    fi
done

# Eksik grupları ekle
if [ ${#MISSING_GROUPS[@]} -ne 0 ]; then
    echo -e "${YELLOW}🔧 Eksik gruplar ekleniyor...${NC}"
    for group in "${MISSING_GROUPS[@]}"; do
        echo -e "${BLUE}   $group grubuna ekleniyor...${NC}"
        sudo usermod -a -G $group $USER
        if [ $? -eq 0 ]; then
            echo -e "${GREEN}   ✅ $group grubu eklendi${NC}"
        else
            echo -e "${RED}   ❌ $group grubu eklenemedi${NC}"
        fi
    done
    echo -e "${YELLOW}⚠️  Grup değişiklikleri için yeniden giriş yapın!${NC}"
fi

# Python virtual environment kontrolü
VENV_PATH="/home/atugem/TurkSatModelUydu/GorevYukuPi/venv"
echo -e "${BLUE}🐍 Python virtual environment kontrol ediliyor...${NC}"
if [ -d "$VENV_PATH" ]; then
    echo -e "${GREEN}✅ Virtual environment mevcut: $VENV_PATH${NC}"
    
    # Python executable kontrolü
    PYTHON_EXEC="$VENV_PATH/bin/python"
    if [ -x "$PYTHON_EXEC" ]; then
        PYTHON_VERSION=$($PYTHON_EXEC --version)
        echo -e "${GREEN}✅ Python sürümü: $PYTHON_VERSION${NC}"
    else
        echo -e "${RED}❌ Python executable bulunamadı: $PYTHON_EXEC${NC}"
        exit 1
    fi
else
    echo -e "${RED}❌ Virtual environment bulunamadı: $VENV_PATH${NC}"
    echo -e "${YELLOW}   Önce virtual environment oluşturun:${NC}"
    echo -e "${YELLOW}   cd /home/atugem/TurkSatModelUydu/GorevYukuPi${NC}"
    echo -e "${YELLOW}   python3 -m venv venv${NC}"
    echo -e "${YELLOW}   source venv/bin/activate${NC}"
    echo -e "${YELLOW}   pip install -r requirements.txt${NC}"
    exit 1
fi

# Ana program dosyası kontrolü
ANA_PROGRAM="/home/atugem/TurkSatModelUydu/GorevYukuPi/ana_program.py"
if [ -f "$ANA_PROGRAM" ]; then
    echo -e "${GREEN}✅ Ana program dosyası mevcut${NC}"
else
    echo -e "${RED}❌ Ana program dosyası bulunamadı: $ANA_PROGRAM${NC}"
    exit 1
fi

# Systemd service dizini kontrolü
SYSTEMD_DIR="/etc/systemd/system"
if [ ! -d "$SYSTEMD_DIR" ]; then
    echo -e "${RED}❌ Systemd service dizini bulunamadı: $SYSTEMD_DIR${NC}"
    exit 1
fi

# Mevcut service durumunu kontrol et
SERVICE_NAME="gorevyuku.service"
INSTALLED_SERVICE="$SYSTEMD_DIR/$SERVICE_NAME"

if systemctl is-active --quiet $SERVICE_NAME; then
    echo -e "${YELLOW}⚠️  Service şu anda aktif, durduruluyor...${NC}"
    sudo systemctl stop $SERVICE_NAME
fi

if systemctl is-enabled --quiet $SERVICE_NAME; then
    echo -e "${YELLOW}⚠️  Service aktif, devre dışı bırakılıyor...${NC}"
    sudo systemctl disable $SERVICE_NAME
fi

# Service dosyasını kopyala
echo -e "${BLUE}📋 Service dosyası kuruluyor...${NC}"
sudo cp "$SERVICE_FILE" "$INSTALLED_SERVICE"
if [ $? -eq 0 ]; then
    echo -e "${GREEN}✅ Service dosyası kopyalandı: $INSTALLED_SERVICE${NC}"
else
    echo -e "${RED}❌ Service dosyası kopyalanamadı${NC}"
    exit 1
fi

# Service dosyası izinlerini ayarla
sudo chmod 644 "$INSTALLED_SERVICE"
echo -e "${GREEN}✅ Service dosyası izinleri ayarlandı${NC}"

# Systemd daemon reload
echo -e "${BLUE}🔄 Systemd daemon reload yapılıyor...${NC}"
sudo systemd reload
if [ $? -eq 0 ]; then
    echo -e "${GREEN}✅ Systemd daemon reload tamamlandı${NC}"
else
    echo -e "${RED}❌ Systemd daemon reload başarısız${NC}"
    exit 1
fi

# Service'i etkinleştir
echo -e "${BLUE}🔧 Service etkinleştiriliyor...${NC}"
sudo systemctl enable $SERVICE_NAME
if [ $? -eq 0 ]; then
    echo -e "${GREEN}✅ Service etkinleştirildi (boot'ta otomatik başlayacak)${NC}"
else
    echo -e "${RED}❌ Service etkinleştirilemedi${NC}"
    exit 1
fi

# Service durumunu göster
echo -e "${BLUE}📊 Service durumu:${NC}"
systemctl status $SERVICE_NAME --no-pager -l

echo -e "${GREEN}======================================${NC}"
echo -e "${GREEN}🎉 SERVICE KURULUM TAMAMLANDI!${NC}"
echo -e "${GREEN}======================================${NC}"

echo -e "${YELLOW}📝 Service yönetim komutları:${NC}"
echo -e "${BLUE}   Service başlat:${NC}    sudo systemctl start gorevyuku"
echo -e "${BLUE}   Service durdur:${NC}    sudo systemctl stop gorevyuku"
echo -e "${BLUE}   Service yeniden başlat:${NC} sudo systemctl restart gorevyuku"
echo -e "${BLUE}   Service durumu:${NC}    systemctl status gorevyuku"
echo -e "${BLUE}   Service logları:${NC}   journalctl -u gorevyuku -f"
echo -e "${BLUE}   Boot'ta otomatik:${NC} sudo systemctl enable gorevyuku"
echo -e "${BLUE}   Boot'ta kapalı:${NC}   sudo systemctl disable gorevyuku"

echo -e "${YELLOW}🚀 Service'i şimdi başlatmak için:${NC}"
echo -e "${BLUE}   sudo systemctl start gorevyuku${NC}"

echo -e "${YELLOW}📡 Real-time log izlemek için:${NC}"
echo -e "${BLUE}   journalctl -u gorevyuku -f${NC}"

echo -e "${GREEN}✅ Kurulum başarıyla tamamlandı!${NC}"