# -*- coding: utf-8 -*-
"""
GÖREV YÜKÜ MODÜLLERİ - TEK XBee KONFİGÜRASYONU
TÜRKSAT Model Uydu Yarışması
"""

from .sensorler import SensorManager
from .aktuatorler import AktuatorYoneticisi  
from .kamera import KameraYoneticisi
from .sd_kayitci import SDKayitci
from .telemetri_isleyici import TelemetryHandler
# from .gps_isleyici import GPSProcessor  # Henüz yok
from .birlesik_xbee_alici import BirlesikXBeeAlici  # 🔥 TEK XBee modülü
from .guc_yoneticisi import GucYoneticisi

# ESKİ MODÜLLER (Artık tek XBee kullanıldığı için gerekli değil)
# from .saha_alici import SahaAlici
# from .iot_xbee_alici import IoTXBeeAlici
# from .haberlesme import Communication

__all__ = [
    'SensorManager',
    'AktuatorYoneticisi', 
    'KameraYoneticisi',
    'SDKayitci',
    'TelemetryHandler',
    # 'GPSProcessor',  # Henüz yok
    'BirlesikXBeeAlici',  # 🔥 TEK XBee 
    'GucYoneticisi'
]

