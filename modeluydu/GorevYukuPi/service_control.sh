#!/bin/bash
# -*- coding: utf-8 -*-
"""
TÜRKSAT Model Uydu Yarışması - Service Kontrol Scripti
Görev yükü service'ini yönetmek için pratik komutlar
"""

# Renk kodları
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
CYAN='\033[0;36m'
NC='\033[0m' # No Color

SERVICE_NAME="gorevyuku.service"

# Başlık
echo -e "${CYAN}🛰️  TÜRKSAT Görev Yükü Service Kontrolü${NC}"
echo -e "${CYAN}======================================${NC}"

# Fonksiyonlar
show_status() {
    echo -e "${BLUE}📊 Service Durumu:${NC}"
    systemctl status $SERVICE_NAME --no-pager -l
}

show_logs() {
    echo -e "${BLUE}📝 Son 50 Log Satırı:${NC}"
    journalctl -u $SERVICE_NAME -n 50 --no-pager
}

follow_logs() {
    echo -e "${BLUE}📡 Real-time Log İzleme (CTRL+C ile çıkış):${NC}"
    journalctl -u $SERVICE_NAME -f
}

start_service() {
    echo -e "${YELLOW}🚀 Service başlatılıyor...${NC}"
    sudo systemctl start $SERVICE_NAME
    if [ $? -eq 0 ]; then
        echo -e "${GREEN}✅ Service başlatıldı${NC}"
    else
        echo -e "${RED}❌ Service başlatılamadı${NC}"
    fi
    show_status
}

stop_service() {
    echo -e "${YELLOW}🛑 Service durduruluyor...${NC}"
    sudo systemctl stop $SERVICE_NAME
    if [ $? -eq 0 ]; then
        echo -e "${GREEN}✅ Service durduruldu${NC}"
    else
        echo -e "${RED}❌ Service durdurulamadı${NC}"
    fi
    show_status
}

restart_service() {
    echo -e "${YELLOW}🔄 Service yeniden başlatılıyor...${NC}"
    sudo systemctl restart $SERVICE_NAME
    if [ $? -eq 0 ]; then
        echo -e "${GREEN}✅ Service yeniden başlatıldı${NC}"
    else
        echo -e "${RED}❌ Service yeniden başlatılamadı${NC}"
    fi
    show_status
}

enable_service() {
    echo -e "${YELLOW}⚡ Service boot'ta otomatik başlatma aktif ediliyor...${NC}"
    sudo systemctl enable $SERVICE_NAME
    if [ $? -eq 0 ]; then
        echo -e "${GREEN}✅ Service boot'ta otomatik başlayacak${NC}"
    else
        echo -e "${RED}❌ Service etkinleştirilemedi${NC}"
    fi
}

disable_service() {
    echo -e "${YELLOW}⏸️  Service boot'ta otomatik başlatma devre dışı bırakılıyor...${NC}"
    sudo systemctl disable $SERVICE_NAME
    if [ $? -eq 0 ]; then
        echo -e "${GREEN}✅ Service boot'ta başlamayacak${NC}"
    else
        echo -e "${RED}❌ Service devre dışı bırakılamadı${NC}"
    fi
}

show_system_info() {
    echo -e "${BLUE}🖥️  Sistem Bilgileri:${NC}"
    echo -e "${CYAN}Uptime:${NC} $(uptime)"
    echo -e "${CYAN}Memory:${NC} $(free -h | grep Mem | awk '{print $3"/"$2" ("$3/$2*100"%)"}')"
    echo -e "${CYAN}CPU Sıcaklık:${NC} $(($(cat /sys/class/thermal/thermal_zone0/temp)/1000))°C"
    echo -e "${CYAN}Disk Kullanımı:${NC} $(df -h / | awk 'NR==2{print $3"/"$2" ("$5")"}')"
    
    echo -e "\n${BLUE}📡 XBee Port Durumu:${NC}"
    for port in /dev/ttyUSB* /dev/ttyACM* /dev/ttyAMA0; do
        if [ -e "$port" ]; then
            echo -e "${GREEN}✅ $port mevcut${NC}"
        fi
    done
    
    echo -e "\n${BLUE}🎥 Kamera Durumu:${NC}"
    if [ -e "/dev/video0" ]; then
        echo -e "${GREEN}✅ /dev/video0 mevcut${NC}"
    else
        echo -e "${RED}❌ Kamera bulunamadı${NC}"
    fi
}

show_help() {
    echo -e "${YELLOW}📚 Kullanılabilir Komutlar:${NC}"
    echo -e "${BLUE}   ./service_control.sh start${NC}     - Service'i başlat"
    echo -e "${BLUE}   ./service_control.sh stop${NC}      - Service'i durdur"
    echo -e "${BLUE}   ./service_control.sh restart${NC}   - Service'i yeniden başlat"
    echo -e "${BLUE}   ./service_control.sh status${NC}    - Service durumunu göster"
    echo -e "${BLUE}   ./service_control.sh logs${NC}      - Son logları göster"
    echo -e "${BLUE}   ./service_control.sh follow${NC}    - Real-time log izle"
    echo -e "${BLUE}   ./service_control.sh enable${NC}    - Boot'ta otomatik başlat"
    echo -e "${BLUE}   ./service_control.sh disable${NC}   - Boot'ta başlatma"
    echo -e "${BLUE}   ./service_control.sh info${NC}      - Sistem bilgileri"
    echo -e "${BLUE}   ./service_control.sh help${NC}      - Bu yardım menüsü"
}

# Ana komut işleme
case "${1:-status}" in
    "start")
        start_service
        ;;
    "stop")
        stop_service
        ;;
    "restart")
        restart_service
        ;;
    "status")
        show_status
        ;;
    "logs")
        show_logs
        ;;
    "follow")
        follow_logs
        ;;
    "enable")
        enable_service
        ;;
    "disable")
        disable_service
        ;;
    "info")
        show_system_info
        ;;
    "help")
        show_help
        ;;
    *)
        echo -e "${RED}❌ Bilinmeyen komut: $1${NC}"
        show_help
        exit 1
        ;;
esac

echo -e "${CYAN}======================================${NC}"