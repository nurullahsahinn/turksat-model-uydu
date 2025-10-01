using System;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Collections.Generic;
using GMap.NET;
using GMap.NET.MapProviders;
using GMap.NET.WindowsForms;
using GMap.NET.WindowsForms.Markers;

namespace ModelUydu
{
    /// <summary>
    /// Harita işlemlerini yöneten sınıf
    /// </summary>
    public class HaritaYoneticisi
    {
        // Harita kontrol nesnesi
        private GMapControl haritaKontrol;

        // Harita ayarları
        private double enlem = 37.01;         // Başlangıç enlem değeri
        private double boylam = 36.5;         // Başlangıç boylam değeri
        private int zoom = 5;                 // Harita yakınlaştırma seviyesi
        private int maxZoom = 120;            // Maksimum zoom seviyesi
        private int minZoom = 1;              // Minimum zoom seviyesi
        private GMapProvider haritaSaglayici; // Harita sağlayıcı
        
        // Rota çizimi için değişkenler
        private GMapOverlay rotaKatmani;      // Rota katmanı
        private GMapOverlay isaretKatmani;    // İşaretleyici katmanı
        private List<PointLatLng> rotaNoktaListesi; // Rota nokta listesi
        private GMapRoute rota;               // Rota nesnesi
        
        // Hata bildirimi için delegasyon
        private Action<string, bool> logHataFonksiyonu;

        /// <summary>
        /// HaritaYoneticisi sınıfı yapıcı metodu
        /// </summary>
        /// <param name="haritaKontrol">GMapControl nesnesi</param>
        /// <param name="logHataFonksiyonu">Hata log fonksiyonu</param>
        public HaritaYoneticisi(GMapControl haritaKontrol, Action<string, bool> logHataFonksiyonu)
        {
            this.haritaKontrol = haritaKontrol;
            this.logHataFonksiyonu = logHataFonksiyonu;
            this.rotaNoktaListesi = new List<PointLatLng>();
            
            // Harita yapılandırması
            HaritaYapilandir();
        }

        /// <summary>
        /// Harita kontrolünü temel ayarlarla yapılandırır
        /// </summary>
        private void HaritaYapilandir()
        {
            try
            {
                // Harita sağlayıcısı ayarla
                haritaSaglayici = GMapProviders.GoogleMap;
                haritaKontrol.MapProvider = haritaSaglayici;
                
                // Konum ve zoom ayarlarını ayarla
                haritaKontrol.Position = new PointLatLng(enlem, boylam);
                haritaKontrol.MinZoom = minZoom;
                haritaKontrol.MaxZoom = maxZoom;
                haritaKontrol.Zoom = zoom;
                
                // Harita davranış ayarlarını yap
                haritaKontrol.DragButton = MouseButtons.Left; // Sol tıklama ile harita sürükleme
                haritaKontrol.MarkersEnabled = true;          // İşaretleyicileri etkinleştir
                haritaKontrol.PolygonsEnabled = true;         // Çokgenleri etkinleştir
                haritaKontrol.RoutesEnabled = true;           // Rotaları etkinleştir
                haritaKontrol.MouseWheelZoomEnabled = true;   // Fare tekerleği ile zoomu etkinleştir
                
                // Hata yakalamayı etkinleştir
                GMaps.Instance.Mode = AccessMode.ServerAndCache;
                
                // Rota ve işaret katmanları oluştur
                rotaKatmani = new GMapOverlay("rotaKatmani");
                isaretKatmani = new GMapOverlay("isaretKatmani");
                
                // Katmanları haritaya ekle
                haritaKontrol.Overlays.Add(rotaKatmani);
                haritaKontrol.Overlays.Add(isaretKatmani);
                
                // Başarıyla yapılandırma mesajı
                logHataFonksiyonu?.Invoke("Harita yapılandırması başarıyla tamamlandı.", false);
            }
            catch (Exception ex)
            {
                logHataFonksiyonu?.Invoke("Harita yapılandırılırken hata: " + ex.Message, true);
            }
        }

        /// <summary>
        /// Haritayı verilen koordinatlarla günceller
        /// </summary>
        /// <param name="enlem">Enlem değeri</param>
        /// <param name="boylam">Boylam değeri</param>
        /// <returns>İşlemin başarılı olup olmadığı</returns>
        public bool UpdateMap(double enlem, double boylam)
        {
            try
            {
                // Geçerli koordinat kontrolü
                if (enlem != 0 && boylam != 0)
                {
                    // Harita merkezini güncelle
                    haritaKontrol.Position = new PointLatLng(enlem, boylam);

                    // Haritayı yenile
                    haritaKontrol.Invalidate();

                    // Koordinatları sakla
                    this.enlem = enlem;
                    this.boylam = boylam;
                    
                    // Haritaya koordinat işaretleyicisi ekle
                    EkleKoordinatIsaretleyici(enlem, boylam);
                    
                    // Rota güncelle (eğer birden fazla nokta varsa)
                    if (rotaNoktaListesi.Count > 0)
                    {
                        GuncelleRota(enlem, boylam);
                    }
                    
                    return true;
                }
                else
                {
                    logHataFonksiyonu?.Invoke("Geçersiz koordinatlar: Enlem ve boylam sıfır olamaz.", false);
                    return false;
                }
            }
            catch (Exception ex)
            {
                logHataFonksiyonu?.Invoke($"Harita güncellenirken hata: {ex.Message}", false);
                return false;
            }
        }
        
        /// <summary>
        /// Haritaya koordinat işaretleyicisi ekler
        /// </summary>
        /// <param name="enlem">Enlem değeri</param>
        /// <param name="boylam">Boylam değeri</param>
        private void EkleKoordinatIsaretleyici(double enlem, double boylam)
        {
            try
            {
                // Yeni konuma işaretleyici ekle
                PointLatLng nokta = new PointLatLng(enlem, boylam);
                GMarkerGoogle isaretleyici = new GMarkerGoogle(nokta, GMarkerGoogleType.red_pushpin);
                isaretleyici.ToolTipText = $"Konum: {enlem}, {boylam}";
                
                // Eski işaretleyicileri temizle ve yeni işaretleyiciyi ekle
                isaretKatmani.Markers.Clear();
                isaretKatmani.Markers.Add(isaretleyici);
                
                // Rota listesine noktayı ekle
                if (rotaNoktaListesi.Count == 0 || 
                    HesaplaUzaklik(rotaNoktaListesi[rotaNoktaListesi.Count - 1], nokta) > 0.001) // Minimum uzaklık kontrolü
                {
                    rotaNoktaListesi.Add(nokta);
                }
            }
            catch (Exception ex)
            {
                logHataFonksiyonu?.Invoke($"İşaretleyici eklenirken hata: {ex.Message}", false);
            }
        }
        
        /// <summary>
        /// İki nokta arasındaki uzaklığı hesaplar
        /// </summary>
        /// <param name="nokta1">Birinci nokta</param>
        /// <param name="nokta2">İkinci nokta</param>
        /// <returns>Uzaklık (km)</returns>
        private double HesaplaUzaklik(PointLatLng nokta1, PointLatLng nokta2)
        {
            // Dünya yarıçapı (km)
            const double r = 6371;
            
            // Radyan dönüşümü
            double lat1 = nokta1.Lat * Math.PI / 180;
            double lat2 = nokta2.Lat * Math.PI / 180;
            double lon1 = nokta1.Lng * Math.PI / 180;
            double lon2 = nokta2.Lng * Math.PI / 180;
            
            // Haversine formülü
            double dLat = lat2 - lat1;
            double dLon = lon2 - lon1;
            double a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                       Math.Cos(lat1) * Math.Cos(lat2) * 
                       Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
            double c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
            double distance = r * c;
            
            return distance;
        }
        
        /// <summary>
        /// Rotayı günceller
        /// </summary>
        /// <param name="enlem">Yeni enlem</param>
        /// <param name="boylam">Yeni boylam</param>
        private void GuncelleRota(double enlem, double boylam)
        {
            try
            {
                // Eski rotayı temizle
                rotaKatmani.Routes.Clear();
                
                // Yeni rotayı oluştur
                if (rotaNoktaListesi.Count >= 2)
                {
                    rota = new GMapRoute(rotaNoktaListesi, "UyduRota");
                    rota.Stroke = new Pen(Color.Red, 2);
                    rotaKatmani.Routes.Add(rota);
                }
            }
            catch (Exception ex)
            {
                logHataFonksiyonu?.Invoke($"Rota güncellenirken hata: {ex.Message}", false);
            }
        }
        
        /// <summary>
        /// Zoom seviyesini artırır
        /// </summary>
        public void ZoomArtir()
        {
            if (zoom < maxZoom)
            {
                zoom += 1;
                haritaKontrol.Zoom = zoom;
            }
        }
        
        /// <summary>
        /// Zoom seviyesini azaltır
        /// </summary>
        public void ZoomAzalt()
        {
            if (zoom > minZoom)
            {
                zoom -= 1;
                haritaKontrol.Zoom = zoom;
            }
        }
        
        /// <summary>
        /// Haritayı belirtilen koordinatlar ve zoom seviyesi ile başlatır
        /// </summary>
        /// <param name="enlem">Başlangıç enlemi</param>
        /// <param name="boylam">Başlangıç boylamı</param>
        /// <param name="zoomSeviyesi">Başlangıç zoom seviyesi</param>
        /// <returns>İşlemin başarılı olup olmadığı</returns>
        public bool InitializeMap(double enlem, double boylam, int zoomSeviyesi)
        {
            try
            {
                // Geçerli koordinat kontrolü
                if (enlem != 0 && boylam != 0)
                {
                    // Değerleri güncelle
                    this.enlem = enlem;
                    this.boylam = boylam;
                    this.zoom = zoomSeviyesi;

                    // Harita pozisyonunu ve zoom seviyesini ayarla
                    haritaKontrol.Position = new PointLatLng(enlem, boylam);
                    haritaKontrol.Zoom = zoom;

                    // Haritayı yenile
                    haritaKontrol.Invalidate();
                    
                    return true;
                }
                else
                {
                    logHataFonksiyonu?.Invoke("Geçersiz koordinatlar: Enlem ve boylam sıfır olamaz.", false);
                    return false;
                }
            }
            catch (Exception ex)
            {
                logHataFonksiyonu?.Invoke($"Harita başlatılırken hata: {ex.Message}", true);
                return false;
            }
        }
        
        /// <summary>
        /// Haritayı merkezi konuma getiren metod
        /// </summary>
        public void MerkezKonumaGit()
        {
            haritaKontrol.Position = new PointLatLng(enlem, boylam);
            haritaKontrol.Zoom = 10;
        }
        
        /// <summary>
        /// Kalibrasyon için haritayı günceller (rota çizmeden)
        /// </summary>
        /// <param name="enlem">Enlem değeri</param>
        /// <param name="boylam">Boylam değeri</param>
        /// <returns>İşlemin başarılı olup olmadığı</returns>
        public bool UpdateMapForCalibration(double enlem, double boylam)
        {
            try
            {
                // Geçerli koordinat kontrolü
                if (enlem != 0 && boylam != 0)
                {
                    // Harita merkezini güncelle
                    haritaKontrol.Position = new PointLatLng(enlem, boylam);

                    // Haritayı yenile
                    haritaKontrol.Invalidate();

                    // Koordinatları sakla
                    this.enlem = enlem;
                    this.boylam = boylam;
                    
                    // Eski işaretleyicileri temizle
                    isaretKatmani.Markers.Clear();
                    
                    // Sadece yeni konum işaretleyicisi ekle (rota eklenmez)
                    PointLatLng nokta = new PointLatLng(enlem, boylam);
                    GMarkerGoogle isaretleyici = new GMarkerGoogle(nokta, GMarkerGoogleType.blue_dot);
                    isaretleyici.ToolTipText = $"Kalibrasyon Konumu: {enlem:F4}, {boylam:F4}";
                    isaretKatmani.Markers.Add(isaretleyici);
                    
                    // Zoom seviyesini şehir görünümü için ayarla
                    haritaKontrol.Zoom = 8;
                    
                    logHataFonksiyonu?.Invoke($"Kalibrasyon konumu güncellendi: {enlem:F4}, {boylam:F4}", false);
                    return true;
                }
                else
                {
                    logHataFonksiyonu?.Invoke("Geçersiz koordinatlar: Enlem ve boylam sıfır olamaz.", false);
                    return false;
                }
            }
            catch (Exception ex)
            {
                logHataFonksiyonu?.Invoke($"Kalibrasyon haritası güncellenirken hata: {ex.Message}", false);
                return false;
            }
        }
        
        /// <summary>
        /// Rotayı temizler
        /// </summary>
        public void RotayiTemizle()
        {
            rotaNoktaListesi.Clear();
            rotaKatmani.Routes.Clear();
            isaretKatmani.Markers.Clear();
            logHataFonksiyonu?.Invoke("Rota ve işaretleyiciler temizlendi.", false);
        }
        
        /// <summary>
        /// Harita sağlayıcısını değiştirir
        /// </summary>
        /// <param name="saglayiciTipi">Sağlayıcı tipi (1: Google, 2: Bing, 3: OpenStreetMap)</param>
        public void HaritaSaglayicisiniDegistir(int saglayiciTipi)
        {
            try
            {
                switch (saglayiciTipi)
                {
                    case 1:
                        haritaKontrol.MapProvider = GMapProviders.GoogleMap;
                        break;
                    case 2:
                        haritaKontrol.MapProvider = GMapProviders.BingMap;
                        break;
                    case 3:
                        haritaKontrol.MapProvider = GMapProviders.OpenStreetMap;
                        break;
                    default:
                        haritaKontrol.MapProvider = GMapProviders.GoogleMap;
                        break;
                }
                
                // Haritayı yenile
                haritaKontrol.Invalidate();
                
                logHataFonksiyonu?.Invoke($"Harita sağlayıcısı değiştirildi: {haritaKontrol.MapProvider.Name}", false);
            }
            catch (Exception ex)
            {
                logHataFonksiyonu?.Invoke($"Harita sağlayıcısı değiştirilirken hata: {ex.Message}", true);
            }
        }
        
        /// <summary>
        /// Güncel enlem değerini döndürür
        /// </summary>
        public double Enlem
        {
            get { return enlem; }
        }
        
        /// <summary>
        /// Güncel boylam değerini döndürür
        /// </summary>
        public double Boylam
        {
            get { return boylam; }
        }
        
        /// <summary>
        /// Güncel zoom seviyesini döndürür
        /// </summary>
        public int ZoomSeviyesi
        {
            get { return zoom; }
            set 
            { 
                if (value >= minZoom && value <= maxZoom)
                {
                    zoom = value;
                    haritaKontrol.Zoom = zoom;
                }
            }
        }
    }
}
        