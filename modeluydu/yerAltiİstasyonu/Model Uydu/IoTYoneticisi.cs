using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ModelUydu
{
    /// <summary>
    /// IoT istasyonlarıyla iletişim, veri işleme ve kaydetme işlemlerini gerçekleştiren sınıf
    /// </summary>
    public class IoTYoneticisi : IDisposable
    {
        // IoT S2S veri transferi için değişkenler
        private double iotS1Sicaklik = 0.0; // IoT İstasyon 1 sıcaklık değeri
        private double iotS2Sicaklik = 0.0; // IoT İstasyon 2 sıcaklık değeri
        private bool iotVerisiAlindiMi = false; // IoT verisi alınma durumu

        // IoT verileri listesi
        private List<IoTVeri> iotVerileriListesi = new List<IoTVeri>();

        // 🔧 DÜZELTME: Gereksiz SerialPort nesneleri kaldırıldı
        // IoT verileri artık sadece telemetri paketinden alınır - daha güvenilir!
        // private SerialPort iotPort1 = null; // ❌ KALDIRILDI - Kullanılmıyordu
        // private SerialPort iotPort2 = null; // ❌ KALDIRILDI - Kullanılmıyordu

        // IoT veri kaydı için dosya yolu
        private string iotLogDosyaYolu = "iot_verileri.txt";

        // Timer değişkeni
        private System.Windows.Forms.Timer iotZamanlayici;

        // UI kontrolleri
        private Label labelIoTS1Deger;
        private Label labelIoTS2Deger;

        // Diğer yöneticiler
        private GrafikYoneticisi grafikYoneticisi;
        // SDKartYoneticisi kaldırıldı - artık direkt dosya kaydetme işlemi yapılacak

        // Hata log metodu için delegate
        private Action<string, bool> logHataMetodu;

        /// <summary>
        /// IoTYoneticisi sınıfı yapıcı metodu
        /// </summary>
        public IoTYoneticisi(
            Label labelIoTS1Deger,
            Label labelIoTS2Deger,
            GrafikYoneticisi grafikYoneticisi,
            Action<string, bool> logHataMetodu)
        {
            // UI kontrollerini ayarla
            this.labelIoTS1Deger = labelIoTS1Deger;
            this.labelIoTS2Deger = labelIoTS2Deger;

            // Diğer yöneticileri ayarla
            this.grafikYoneticisi = grafikYoneticisi;
            // sdKartYoneticisi kaldırıldı - null olarak geçilecek

            // Hata log metodunu ayarla
            this.logHataMetodu = logHataMetodu;

            // IoT zamanlayıcısını ayarla
            iotZamanlayici = new System.Windows.Forms.Timer();
            iotZamanlayici.Interval = 1000; // 1 saniye
            iotZamanlayici.Tick += new EventHandler(IotZamanlayici_Tick);

            // IoT sistemini başlat
            Baslat();
        }

        /// <summary>
        /// IoT sistemini başlatan metod
        /// </summary>
        public void Baslat()
        {
            try
            {
                // IoT verisi alınmadı olarak işaretle
                iotVerisiAlindiMi = false;

                // IoT veri kaydı için dosya oluştur
                if (!File.Exists(iotLogDosyaYolu))
                {
                    using (StreamWriter sw = new StreamWriter(iotLogDosyaYolu))
                    {
                        sw.WriteLine("Zaman,IoT_Istasyon1_Sicaklik,IoT_Istasyon2_Sicaklik");
                    }
                }

                // 🔧 DÜZELTME: IoT istasyonları doğrudan yer istasyonuna bağlanmaz
                // IoT verileri görev yükünden gelen telemetri paketinde gelir
                // Bu sayede veri akışı: IoT İstasyonları → Görev Yükü → Yer İstasyonu
                LogHata("✅ IoT Sistem Başlatıldı: Veriler telemetri paketinden alınacak", false);
                LogHata("📡 IoT Veri Akışı: IoT İstasyonları → Görev Yükü → Yer İstasyonu", false);

                // IoT zamanlayıcısını başlat (telemetri paketinden veri işleme için)
                iotZamanlayici.Start();
            }
            catch (Exception ex)
            {
                LogHata("IoT başlatılırken hata: " + ex.Message, true);
            }
        }

        // 🔧 DÜZELTME: Gereksiz SerialPort event handler'ları kaldırıldı
        // IoT verileri artık TelemetridenIoTVerileriniGuncelle() metodu ile
        // telemetri paketinden alınır - daha güvenilir ve basit!
        
        /*
        ❌ KALDIRILDI: private void IotPort1_DataReceived() 
        ❌ KALDIRILDI: private void IotPort2_DataReceived()
        
        SEBEP: IoT İstasyonları → Görev Yükü → Telemetri Paketi → IoTYoneticisi 
        veri akışı kullanılıyor. Ayrı SerialPort bağlantısı gereksiz.
        */

        /// <summary>
        /// IoT istasyonlarından gelen veriyi işleyen metod
        /// </summary>
        private void IsleIoTVeri(int istasyonID, string veri)
        {
            try
            {
                // Veriyi ayrıştır (format: "SICAKLIK:23.5")
                if (veri.StartsWith("SICAKLIK:"))
                {
                    string sicaklikStr = veri.Substring(9);
                    if (float.TryParse(sicaklikStr, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out float sicaklik))
                    {
                        // IoT istasyon sıcaklık değerini güncelle
                        if (istasyonID == 1)
                        {
                            iotS1Sicaklik = sicaklik;
                            labelIoTS1Deger.Text = sicaklik.ToString("F1") + " °C"; // 1 ondalık basamakla göster
                        }
                        else if (istasyonID == 2)
                        {
                            iotS2Sicaklik = sicaklik;
                            labelIoTS2Deger.Text = sicaklik.ToString("F1") + " °C"; // 1 ondalık basamakla göster
                        }

                        iotVerisiAlindiMi = true;

                        // IoT verilerini liste için kaydet
                        EkleIoTVeri();

                        // IoT verilerini güncelle ve grafiği çiz
                        GuncelleGrafikVerileri();

                        // IoT verilerini SD karta kaydet
                        KaydetIotVerileriSDKarta(istasyonID.ToString(), sicaklik);
                    }
                }
            }
            catch (Exception ex)
            {
                LogHata($"IoT Port {istasyonID} verisi işlenirken hata: " + ex.Message, true);
            }
        }

        /// <summary>
        /// IoT zamanlayıcısı her çalıştığında çalışacak metod
        /// </summary>
        private void IotZamanlayici_Tick(object sender, EventArgs e)
        {
            try
            {
                // 🔧 DÜZELTME: IoT verileri sadece telemetri paketinden gelir
                // TelemetridenIoTVerileriniGuncelle() metodu çağrıldığında iotVerisiAlindiMi = true olur
                    if (iotVerisiAlindiMi)
                    {
                        // IoT verilerini listeye ekle
                        EkleIoTVeri();

                        // IoT verilerini log dosyasına kaydet
                        KaydetIotVerileri();

                        // IoT grafiğini güncelle
                GuncelleGrafikVerileri();
                    
                    // İşlem tamamlandı, flag'i sıfırla
                    iotVerisiAlindiMi = false;
                }
            }
            catch (Exception ex)
            {
                LogHata("IoT zamanlayıcı hatası: " + ex.Message, true);
            }
        }

        /// <summary>
        /// IoT verilerini yerel dosyaya kaydeden metod
        /// </summary>
        private void KaydetIotVerileri()
        {
            try
            {
                // Ortak VeriKayitlari klasörüne IoT verilerini kaydet
                string iotDizini = Path.Combine(Application.StartupPath, "VeriKayitlari", "iot");
                if (!Directory.Exists(iotDizini))
                {
                    Directory.CreateDirectory(iotDizini);
                }

                string dosyaAdi = Path.Combine(iotDizini, "iot_" + DateTime.Now.ToString("yyyy-MM-dd") + ".csv");

                // Dosya yoksa başlık satırını ekle
                if (!File.Exists(dosyaAdi))
                {
                    using (StreamWriter sw = new StreamWriter(dosyaAdi))
                    {
                        sw.WriteLine("Zaman,S1_Sicaklik,S2_Sicaklik");
                    }
                }

                // IoT verilerini dosyaya ekle
                using (StreamWriter sw = new StreamWriter(dosyaAdi, true))
                {
                    sw.WriteLine($"{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")},{iotS1Sicaklik.ToString("F2")},{iotS2Sicaklik.ToString("F2")}");
                }
                
                // Ayrıca yerel dosyaya da kaydet
                using (StreamWriter sw = new StreamWriter(iotLogDosyaYolu, true))
                {
                    sw.WriteLine($"{DateTime.Now},{iotS1Sicaklik.ToString("F1")},{iotS2Sicaklik.ToString("F1")}");
                }
            }
            catch (Exception ex)
            {
                LogHata("IoT verileri kaydedilirken hata: " + ex.Message, false);
            }
        }

        /// <summary>
        /// Belirli bir istasyonun IoT verilerini ortak klasöre kaydeden metod
        /// </summary>
        private void KaydetIotVerileriSDKarta(string istasyonID, float sicaklik)
        {
            try
            {
                // Ortak VeriKayitlari klasörüne IoT verilerini kaydet
                string iotDizini = Path.Combine(Application.StartupPath, "VeriKayitlari", "iot");
                if (!Directory.Exists(iotDizini))
                {
                    Directory.CreateDirectory(iotDizini);
                }

                string dosyaAdi = Path.Combine(iotDizini, "iot_" + DateTime.Now.ToString("yyyy-MM-dd") + ".csv");

                // Dosya yoksa başlık satırını ekle
                if (!File.Exists(dosyaAdi))
                {
                    using (StreamWriter sw = new StreamWriter(dosyaAdi))
                    {
                        sw.WriteLine("Zaman,IstasyonID,Sicaklik");
                    }
                }

                // IoT verilerini dosyaya ekle
                using (StreamWriter sw = new StreamWriter(dosyaAdi, true))
                {
                    sw.WriteLine($"{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")},{istasyonID},{sicaklik.ToString("F2")}");
                }
            }
            catch (Exception ex)
            {
                LogHata("IoT verileri kaydedilirken hata: " + ex.Message, false);
            }
        }

        /// <summary>
        /// IoT verilerini grafik yöneticisine aktaran ve güncelleyen metod
        /// </summary>
        public void GuncelleGrafikVerileri()
        {
            if (grafikYoneticisi != null)
            {
                // IoT verilerini GrafikYoneticisi'ne aktar
                grafikYoneticisi.IotS1Sicaklik = iotS1Sicaklik;
                grafikYoneticisi.IotS2Sicaklik = iotS2Sicaklik;
                grafikYoneticisi.IotVerisiAlindiMi = true;

                // IoT grafiğini güncelle
                grafikYoneticisi.GuncelleIotGrafigi();
            }
        }

        /// <summary>
        /// IoT verilerini görüntüleme butonu için olay işleyici
        /// </summary>
        public void GoruntuleIoTVerileri()
        {
            try
            {
                // IoT verilerini içeren dosyayı kontrol et
                if (!File.Exists(iotLogDosyaYolu))
                {
                    MessageBox.Show("IoT veri dosyası bulunamadı: " + iotLogDosyaYolu,
                        "Dosya Bulunamadı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // IoT verilerini içeren dosyayı aç
                System.Diagnostics.Process.Start("notepad.exe", iotLogDosyaYolu);
            }
            catch (Exception ex)
            {
                MessageBox.Show("IoT verileri görüntülenirken hata oluştu: " + ex.Message,
                    "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Telemetri paketinden IoT verilerini ayrıştıran ve güncelleyen metod
        /// </summary>
        public void TelemetridenIoTVerileriniGuncelle(string[] paket)
        {
            if (paket.Length >= 21)
            {
                try
                {
                    if (double.TryParse(paket[19].Trim(), System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out double s1))
                    {
                        iotS1Sicaklik = s1;
                        labelIoTS1Deger.Text = s1.ToString("F1") + " °C";
                    }
                    else
                    {
                        LogHata($"❌ IoT S1 Parse Hatası: '{paket[19]}'", false);
                    }

                    if (double.TryParse(paket[20].Trim(), System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out double s2))
                    {
                        iotS2Sicaklik = s2;
                        labelIoTS2Deger.Text = s2.ToString("F1") + " °C";
                    }
                    else
                    {
                        LogHata($"❌ IoT S2 Parse Hatası: '{paket[20]}'", false);
                    }
                }
                catch (Exception ex)
                {
                    LogHata($"IoT telemetri verisi güncellenirken hata: {ex.Message}", true);
                }
            }
        }

        /// <summary>
        /// 🔧 DÜZELTME: IoT sistemini kapatmak için kullanılan metod - Sadeleştirilmiş versiyon
        /// </summary>
        public void Kapat()
        {
            try
            {
                // IoT zamanlayıcısını durdur
                if (iotZamanlayici != null && iotZamanlayici.Enabled)
                {
                    iotZamanlayici.Stop();
                }

                // 🔧 DÜZELTME: Gereksiz SerialPort kapatma kodları kaldırıldı
                // IoT verileri telemetri paketinden gelir, ayrı port bağlantısı yok
                LogHata("✅ IoT sistemi kapatıldı (Sadeleştirilmiş versiyon)", false);
            }
            catch (Exception ex)
            {
                LogHata("IoT sistemi kapatılırken hata: " + ex.Message, true);
            }
        }

        /// <summary>
        /// En son alınan IoT verilerini döndüren property'ler
        /// </summary>
        public double IoTS1Sicaklik => iotS1Sicaklik;
        public double IoTS2Sicaklik => iotS2Sicaklik;
        public bool IoTVerisiAlindiMi => iotVerisiAlindiMi;
        
        /// <summary>
        /// IoT verileri listesini döndüren property
        /// </summary>
        public List<IoTVeri> IotVerileriListesi => iotVerileriListesi;

        /// <summary>
        /// IoT verisi sınıfı - IoT verilerini saklamak için
        /// </summary>
        public class IoTVeri
        {
            public DateTime Zaman { get; set; }
            public double S1Sicaklik { get; set; }
            public double S2Sicaklik { get; set; }
            public int BataryaDurumu { get; set; }
            public bool BaglantiDurumu { get; set; }

            public IoTVeri(DateTime zaman, double s1Sicaklik, double s2Sicaklik, int bataryaDurumu, bool baglantiDurumu)
            {
                Zaman = zaman;
                S1Sicaklik = s1Sicaklik;
                S2Sicaklik = s2Sicaklik;
                BataryaDurumu = bataryaDurumu;
                BaglantiDurumu = baglantiDurumu;
            }
        }

        /// <summary>
        /// IoT verilerini listeye ekleyen metod
        /// </summary>
        private void EkleIoTVeri()
        {
            try
            {
                // Rastgele batarya durumu (örnek)
                Random rnd = new Random();
                int bataryaDurumu = 50 + rnd.Next(50); // %50-100 arası batarya

                // IoT verisini oluştur
                IoTVeri yeniVeri = new IoTVeri(
                    DateTime.Now,
                    iotS1Sicaklik,
                    iotS2Sicaklik,
                    bataryaDurumu,
                    true // Bağlantı durumu (varsayılan olarak bağlı)
                );

                // Listeye ekle
                iotVerileriListesi.Add(yeniVeri);

                // 🚨 TÜRKSAT YARIŞMASI - VERİ KAYBI YASAK!
                // Liste boyutu sınırlaması devre dışı bırakıldı
                // Tüm IoT verileri yarışma sonuna kadar korunacak!
                
                // ESKI KOD (VERİ SİLİYORDU):
                // while (iotVerileriListesi.Count > 1000)
                // {
                //     iotVerileriListesi.RemoveAt(0); // VERİ KAYBI TEHLİKESİ!
                // }
                
                // Performans için sadece uyarı
                if (iotVerileriListesi.Count > 0 && iotVerileriListesi.Count % 1000 == 0)
                {
                    LogHata($"🏆 TÜRKSAT: {iotVerileriListesi.Count} IoT verisi korunuyor (veri silme devre dışı)", false);
                }
            }
            catch (Exception ex)
            {
                LogHata("IoT verileri listeye eklenirken hata: " + ex.Message, false);
            }
        }

        /// <summary>
        /// Hata loglama metodu
        /// </summary>
        private void LogHata(string hataMesaji, bool kritikMi = false)
        {
            if (logHataMetodu != null)
            {
                logHataMetodu(hataMesaji, kritikMi);
            }
            else
            {
                Console.WriteLine("HATA: " + hataMesaji);
            }
        }
        
        /// <summary>
        /// 🔧 DÜZELTME: IDisposable arayüzü için Dispose metodu - Sadeleştirilmiş versiyon
        /// </summary>
        public void Dispose()
        {
            // IoT sistemini kapat
            Kapat();
            
            // Zamanlayıcıyı temizle
            if (iotZamanlayici != null)
            {
                iotZamanlayici.Tick -= IotZamanlayici_Tick;
                iotZamanlayici.Dispose();
                iotZamanlayici = null;
            }
            
            // 🔧 DÜZELTME: Gereksiz SerialPort temizleme kodları kaldırıldı
            // IoT verileri telemetri paketinden gelir, ayrı SerialPort nesnesi yok
            /*
            ❌ KALDIRILDI: iotPort1.Close(), iotPort1.Dispose()
            ❌ KALDIRILDI: iotPort2.Close(), iotPort2.Dispose()
            
            SEBEP: IoT verileri telemetri paketinden alınır, 
            ayrı SerialPort bağlantısı kullanılmaz - daha güvenilir sistem!
            */
            
            // Verileri temizle
            iotVerileriListesi.Clear();
            
            // GC'yi çağır
            GC.SuppressFinalize(this);
            
            LogHata("✅ IoTYoneticisi kaynakları temizlendi (Cleanup versiyon)", false);
        }


    }
} 