using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using System.Threading.Tasks; // Added for Task

namespace ModelUydu
{
    /// <summary>
    /// Multi-spektral filtreleme işlemlerini yöneten sınıf
    /// </summary>
    public class MultiSpektralFiltreYoneticisi
    {
        // Multi-spektral UI bileşenleri
        private Label labelMultiSpektralDurum;
        private Label labelMultiSpektralKomut;
        private Label labelMultiSpektralAciklama;
        private TextBox textBoxMultiSpektralKomut;
        
        // Seri port nesnesi
        private System.IO.Ports.SerialPort serialPort;
        
        // Bağlantı yöneticisi referansı (test modu desteği için)
        private BaglantiYoneticisi baglantiYoneticisi;
        
        // Multi-spektral filtreleme için değişkenler
        private string multiSpektralKomut = "";
        private string logDosyaYolu = "komut_log.txt";
        
        // Hata loglama için callback
        private Action<string, bool> logHata;
        
        // Ayrılma durumu kontrolü için değişkenler
        private bool ayrilmaDurumuGerceklesti = false;
        
        // Alarm sistemi referansı
        private AlarmSistemiYoneticisi alarmSistemiYoneticisi;
        
        // Filtreleme durumu
        private bool filtrelemeAktif = false;
        
        // Kalan zaman bilgisi için değişkenler
        private Timer kalanZamanTimer;
        private int kalanZamanSaniye = 0;
        private DateTime filtrelemeBaslangicZamani;
        
        // 🔧 KRİTİK DÜZELTME: N harfinin iki farklı anlamı:
        // 1. Şartname Tablo 7: N = Navy Blue (Blue + Blue) ✅
        // 2. Standart konum: (N) = Normal/Filtresiz ✅
        
        // Standart filtre değeri - Normal/Filtresiz konum için
        // NOT: Bu sadece başlangıç/bitiş için kullanılır, komut olarak değil!
        private const char STANDART_FILTRE_INTERNAL = 'N'; // İç kullanım için
        
        // Maksimum filtreleme geçiş süresi (ms)
        private const int MAKSIMUM_GECIS_SURESI_MS = 2000; // 2 saniye
        
        // Toplam filtreleme süresi (saniye)
        private const int TOPLAM_FILTRELEME_SURESI = 15;
        
        /// <summary>
        /// MultiSpektralFiltreYoneticisi yapıcı metodu
        /// </summary>
        /// <param name="labelMultiSpektralDurum">Filtre durumunu gösteren etiket</param>
        /// <param name="labelMultiSpektralKomut">Aktif komutu gösteren etiket</param>
        /// <param name="labelMultiSpektralAciklama">Filtre açıklamasını gösteren etiket</param>
        /// <param name="textBoxMultiSpektralKomut">Komut giriş metin kutusu</param>
        /// <param name="serialPort">Seri port nesnesi</param>
        /// <param name="logHata">Hata loglama fonksiyonu</param>
        public MultiSpektralFiltreYoneticisi(
            Label labelMultiSpektralDurum,
            Label labelMultiSpektralKomut,
            Label labelMultiSpektralAciklama,
            TextBox textBoxMultiSpektralKomut,
            System.IO.Ports.SerialPort serialPort,
            Action<string, bool> logHata)
        {
            this.labelMultiSpektralDurum = labelMultiSpektralDurum;
            this.labelMultiSpektralKomut = labelMultiSpektralKomut;
            this.labelMultiSpektralAciklama = labelMultiSpektralAciklama;
            this.textBoxMultiSpektralKomut = textBoxMultiSpektralKomut;
            this.serialPort = serialPort;
            this.logHata = logHata;
            
            // Başlangıç durumunu ayarla
            ResetDurumEtiketi();
        }
        
        /// <summary>
        /// Alarm Sistemi Yöneticisini ayarlar
        /// </summary>
        /// <param name="alarmSistemiYoneticisi">Alarm sistemi yöneticisi referansı</param>
        public void SetAlarmSistemiYoneticisi(AlarmSistemiYoneticisi alarmSistemiYoneticisi)
        {
            this.alarmSistemiYoneticisi = alarmSistemiYoneticisi;
            logHata?.Invoke("Multi-Spektral Filtre Yöneticisi: Alarm sistemi referansı ayarlandı.", false);
        }
        
        /// <summary>
        /// Bağlantı Yöneticisini ayarlar (test modu desteği için)
        /// </summary>
        /// <param name="baglantiYoneticisi">Bağlantı yöneticisi referansı</param>
        public void SetBaglantiYoneticisi(BaglantiYoneticisi baglantiYoneticisi)
        {
            this.baglantiYoneticisi = baglantiYoneticisi;
            logHata?.Invoke("Multi-Spektral Filtre Yöneticisi: Bağlantı yöneticisi referansı ayarlandı. Test modu desteği aktif.", false);
        }
        
        /// <summary>
        /// Kullanıcının girdiği komutu günceller
        /// </summary>
        /// <param name="yeniKomut">Yeni multi-spektral komut</param>
        public void UpdateKomut(string yeniKomut)
        {
            multiSpektralKomut = yeniKomut?.Trim().ToUpper() ?? "";
            
            // UI'yi güncelle
            if (!string.IsNullOrEmpty(multiSpektralKomut))
            {
                labelMultiSpektralKomut.Text = $"Aktif Komut: {multiSpektralKomut}";
                
                // Komut açıklamasını güncelle
                string aciklama = KomutAciklamasiOlustur(multiSpektralKomut);
                labelMultiSpektralAciklama.Text = aciklama;
            }
            else
            {
                labelMultiSpektralKomut.Text = "Aktif Komut: -";
                labelMultiSpektralAciklama.Text = "Açıklama: Komut giriniz";
            }
        }
        
        /// <summary>
        /// Durum etiketini başlangıç değerine sıfırlar
        /// </summary>
        public void ResetDurumEtiketi()
        {
            // Kalan zaman timer'ını durdur
            DurdurKalanZamanTimer();
            
            labelMultiSpektralDurum.Text = "Durum: Bekleniyor";
            labelMultiSpektralDurum.ForeColor = Color.Gray;
            filtrelemeAktif = false;
            
            // Alarm sistemini güncelle (Multi-spektral sistem çalışmıyor - 6. hata kodu)
            GuncelleAlarmSistemi();
        }
        
        /// <summary>
        /// Multi-spektral filtreleme komutunu gönderir
        /// </summary>
        /// <param name="komut">Opsiyonel özel komut (null ise pendingKomut kullanılır)</param>
        /// <returns>Başarılıysa true</returns>
        public async Task<bool> KomutGonder(string komut = null)
        {
            try
            {
                string gonderilecekKomut = komut ?? multiSpektralKomut; // Changed from pendingKomut to multiSpektralKomut
                
                if (string.IsNullOrEmpty(gonderilecekKomut))
            {
                    logHata?.Invoke("Gönderilecek komut bulunamadı", true);
                return false;
            }
            
                // Komutu format kontrolü yap
                if (!KomutFormatDogrula(gonderilecekKomut)) // Changed from IsValidKomut to KomutFormatDogrula
            {
                    logHata?.Invoke($"Geçersiz komut formatı: {gonderilecekKomut}", true);
                return false;
            }

                logHata?.Invoke($"Multi-spektral komut gönderiliyor: {gonderilecekKomut}", false);
                
                // 🔧 AYRILMA KONTROLÜ: Multi-spektral komutlar sadece ayrılma sonrası gönderilebilir
                if (!ayrilmaDurumuGerceklesti)
                {
                    logHata?.Invoke("❌ AYRILMA BEKLENİYOR: Görev yükü taşıyıcıdan henüz ayrılmadı! Önce 'Manuel Ayrılma' butonuna basın.", true);
                return false;
            }

                // 🔧 DÜZELTME: İlk komut için sistem hazır olmayabilir, ama aktif filtreleme varsa engelle
                // Sistem hazır mı kontrol et - SADECE AKTIF FILTRELEME SIRASINDA ENGELLE
                // if (!filtrelemeAktif) 
                // {
                //     logHata?.Invoke("Filtre sistemi hazır değil", true);
                //     return false;
                // }
                
                // Yeni mantık: Sadece zaten aktif filtreleme varsa engelle
                if (filtrelemeAktif)
                {
                    logHata?.Invoke("⚠️ Bir filtreleme zaten aktif! Önce mevcut filtrelemeyi bekleyin.", true);
                    return false;
                }
                
                // Komut gönder
                bool sonuc = await ExecuteKomut(gonderilecekKomut); // Changed from ExecuteKomut to ExecuteKomut
                
                if (sonuc)
                {
                    // Başarılı komut sonrası
                    logHata?.Invoke($"Multi-spektral komut başarıyla gönderildi: {gonderilecekKomut}", false);
                    
                    // Pending komut sıfırla
                    if (komut == null) // Sadece pending komut kullanıldıysa
                    {
                        multiSpektralKomut = null; // Changed from pendingKomut to multiSpektralKomut
                    }
                }
                else
                {
                    logHata?.Invoke($"Multi-spektral komut gönderilemedi: {gonderilecekKomut}", true);
                    } 
                
                return sonuc;
            }
            catch (Exception ex)
            {
                logHata?.Invoke($"Multi-spektral komut gönderim hatası: {ex.Message}", true);
                return false;
            }
        }
        
        /// <summary>
        /// Multi-spektral komut formatını doğrulayan metod
        /// </summary>
        /// <param name="komut">Doğrulanacak komut</param>
        /// <returns>Komutun geçerli olup olmadığı</returns>
        public bool KomutFormatDogrula(string komut)
        {
            try
            {
                // Boş veya 4 karakterden farklı uzunluktaki komutları reddet
                if (string.IsNullOrEmpty(komut) || komut.Length != 4)
                {
                    logHata?.Invoke("Geçersiz komut uzunluğu. Komut 4 karakter olmalıdır.", false);
                    return false;
                }

                // İlk ve üçüncü karakterler 6-9 arasında rakam olmalı
                if (!char.IsDigit(komut[0]) || !char.IsDigit(komut[2]))
                {
                    logHata?.Invoke("İlk ve üçüncü karakterler rakam olmalıdır.", false);
                    return false;
                }

                int duration1 = int.Parse(komut[0].ToString());
                int duration2 = int.Parse(komut[2].ToString());

                // Rakamların 6 ile 9 arasında olduğunu kontrol et
                if (duration1 < 6 || duration1 > 9 || duration2 < 6 || duration2 > 9)
                {
                    logHata?.Invoke("Geçersiz komut: Filtreleme süreleri 6 ile 9 saniye arasında olmalıdır!", true);
                    return false;
                }

                // İkinci ve dördüncü karakterler geçerli filtre harfleri olmalı
                char filter1 = komut[1];
                char filter2 = komut[3];

                // Toplam filtreleme süresini kontrol et (tam 15 saniye olmalı)
                int toplamSure = duration1 + duration2;
                if (toplamSure != 15)
                {
                    logHata?.Invoke($"Geçersiz komut: Toplam filtreleme süresi tam 15 saniye olmalı! (Mevcut: {toplamSure})", true);
                    return false;
                }

                // Filtre türlerinin geçerli olduğunu kontrol et
                string gecerliFiltreler = "RGBCFNMPY";
                if (gecerliFiltreler.IndexOf(filter1) == -1 || gecerliFiltreler.IndexOf(filter2) == -1)
                {
                    logHata?.Invoke("Geçersiz komut: Filtre türü harfleri (R,G,B,C,F,N,M,P,Y) arasında olmalıdır!", true);
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                logHata?.Invoke($"Komut doğrulama sırasında hata: {ex.Message}", false);
                return false;
            }
        }
        
        /// <summary>
        /// İki ayrı servo/step motor kontrolü için komut oluşturan metod
        /// </summary>
        /// <param name="komut">Orijinal 4 haneli komut</param>
        /// <returns>Servo/step motor kontrolü için düzenlenmiş komut</returns>
        private string OlusturMotorKontrolKomutu(string komut)
        {
            // Komut formatı: !RHRH! (R: Rakam, H: Harf) -> İki disk için motor kontrolü
            if (komut.Length != 4)
                return $"!{komut}!"; // Geçersiz komut durumunda basit format gönder
            
            // Filtreleme süreleri
            int duration1 = int.Parse(komut[0].ToString());
            int duration2 = int.Parse(komut[2].ToString());
            
            // Filtre tipleri
            char filter1 = komut[1];
            char filter2 = komut[3];
            
            // Motor1 ve Motor2 için komut oluştur
            // Format: !M1:süre1:filtre1;M2:süre2:filtre2!
            // Gecikme parametresi olarak maksimum geçiş süresini ekle (ms cinsinden)
            string motorKomutu = $"!M1:{duration1}:{filter1}:{MAKSIMUM_GECIS_SURESI_MS};M2:{duration2}:{filter2}:{MAKSIMUM_GECIS_SURESI_MS}!";
            
            logHata?.Invoke($"Oluşturulan motor kontrol komutu: {motorKomutu}", false);
            return motorKomutu;
        }
        
        /// <summary>
        /// Toplam filtreleme süresini hesaplar
        /// </summary>
        /// <param name="komut">4 haneli komut</param>
        /// <returns>Toplam filtreleme süresi (saniye)</returns>
        private int HesaplaToplamFiltrelemeSuresi(string komut)
        {
            if (komut.Length != 4 || !char.IsDigit(komut[0]) || !char.IsDigit(komut[2]))
                return TOPLAM_FILTRELEME_SURESI; // Varsayılan süre
                
            int duration1 = int.Parse(komut[0].ToString());
            int duration2 = int.Parse(komut[2].ToString());
            
            return duration1 + duration2;
        }
        
        /// <summary>
        /// Gönderilen komutu log dosyasına kaydeden metod
        /// </summary>
        /// <param name="komut">Loglanacak komut</param>
        private void KomutLogla(string komut)
        {
            try
            {
                // Log satırını oluştur (tarih saat - komut)
                string logSatiri = $"{DateTime.Now} - Multi-Spektral Komut: {komut}";

                // Dosya yoksa oluştur, varsa sonuna ekle (append modu)
                using (StreamWriter sw = new StreamWriter(logDosyaYolu, true))
                {
                    sw.WriteLine(logSatiri); // Log satırını dosyaya yaz
                }
            }
            catch (Exception ex)
            {
                // Hata durumunda konsola mesaj yaz
                Console.WriteLine("Komut loglanırken hata: " + ex.Message);
                // Loglama hatası kritik olmadığından kullanıcıya bildirilmiyor
            }
        }
        
        /// <summary>
        /// Komut açıklaması oluşturan fonksiyon
        /// </summary>
        /// <param name="komut">Açıklaması oluşturulacak komut</param>
        /// <returns>Komut açıklaması</returns>
        public string KomutAciklamasiOlustur(string komut)
        {
            if (komut.Length != 4)
                return "Geçersiz komut formatı";

            int duration1 = int.Parse(komut[0].ToString());
            char filter1 = komut[1];
            int duration2 = int.Parse(komut[2].ToString());
            char filter2 = komut[3];

            // 🔧 KRİTİK DÜZELTME: Kullanıcı komutu verirken N = Navy Blue!
            // Standart konum parametresi FALSE çünkü bu ŞARTNAME TABLO 7 komutu
            string filterDesc1 = FiltreTuruAciklamasi(filter1, isStandartKonum: false);
            string filterDesc2 = FiltreTuruAciklamasi(filter2, isStandartKonum: false);

            return $"{duration1} saniye {filterDesc1}, {duration2} saniye {filterDesc2}\nToplam süre: {duration1 + duration2} saniye";
        }
        
        /// <summary>
        /// Filtre türü açıklaması
        /// </summary>
        /// <param name="filterType">Filtre türü karakteri</param>
        /// <param name="isStandartKonum">True ise standart konum açıklaması, false ise Navy Blue</param>
        /// <returns>Filtre açıklaması</returns>
        public string FiltreTuruAciklamasi(char filterType, bool isStandartKonum = false)
        {
            switch (filterType)
            {
                case 'R': return "Light Red";
                case 'G': return "Light Green";
                case 'B': return "Light Blue";
                case 'C': return "Cyan, Turquoise";
                case 'F': return "Forest/Dark Green";
                case 'N': 
                    if (isStandartKonum)
                        return "Standart Konum (Filtresiz)";
                    else
                        return "Navy Blue (Blue + Blue)";
                case 'M': return "Maroon Red";
                case 'P': return "Purple, Pink";
                case 'Y': return "Yellow, Brown";
                default: return "Bilinmeyen Filtre";
            }
        }
        
        /// <summary>
        /// Ayrılma durumunu günceller
        /// </summary>
        /// <param name="ayrilmaDurumu">Ayrılma durumu (true: ayrıldı, false: ayrılmadı)</param>
        public void GuncelleAyrilmaDurumu(bool ayrilmaDurumu)
        {
            // Eğer durum değiştiyse güncelle
            if (this.ayrilmaDurumuGerceklesti != ayrilmaDurumu)
            {
                this.ayrilmaDurumuGerceklesti = ayrilmaDurumu;
                
                if (ayrilmaDurumu)
                {
                    logHata?.Invoke("Görev yükü taşıyıcıdan ayrıldı! Multi-spektral filtreleme komutlarına hazır.", false);
                    labelMultiSpektralDurum.Text = "Durum: Komut Bekleniyor";
                    labelMultiSpektralDurum.ForeColor = Color.Blue;
                }
                else
                {
                    logHata?.Invoke("Görev yükü taşıyıcıdan henüz ayrılmadı! Multi-spektral filtreleme komutları gönderilemez.", false);
                    labelMultiSpektralDurum.Text = "Durum: Ayrılma Bekleniyor";
                    labelMultiSpektralDurum.ForeColor = Color.Orange;
                    
                    // Filtreleme durumunu sıfırla
                    filtrelemeAktif = false;
                }
                
                // Alarm sistemini güncelle (Multi-spektral sistem durumu güncellendi)
                GuncelleAlarmSistemi();
            }
        }
        
        /// <summary>
        /// Ayrılma durumunu günceller
        /// </summary>
        public void AyrilmaDurumuGuncelle(bool ayrilmaDurumu)
        {
            GuncelleAyrilmaDurumu(ayrilmaDurumu);
        }
        
        /// <summary>
        /// Alarm sistemini günceller
        /// </summary>
        private void GuncelleAlarmSistemi()
        {
            if (alarmSistemiYoneticisi != null)
            {
                // Mevcut hata kodunu al
                string mevcutHataKodu = alarmSistemiYoneticisi.GetHataKodu();
                
                if (mevcutHataKodu.Length >= 6)
                {
                    // Hata kodunun 6. karakterini (multi-spektral sistem durumu) güncelle
                    char[] hataKoduArray = mevcutHataKodu.ToCharArray();
                    hataKoduArray[5] = filtrelemeAktif ? '0' : '1'; // 0: Çalışıyor, 1: Çalışmıyor
                    
                    // Güncellenmiş hata kodunu oluştur
                    string yeniHataKodu = new string(hataKoduArray);
                    
                    // Alarm sistemini güncelle
                    alarmSistemiYoneticisi.HataKoduGuncelle(yeniHataKodu);
                }
            }
        }
        
        /// <summary>
        /// Aktif komut
        /// </summary>
        public string AktifKomut
        {
            get { return multiSpektralKomut; }
        }
        
        /// <summary>
        /// Log dosya yolu
        /// </summary>
        public string LogDosyaYolu
        {
            get { return logDosyaYolu; }
            set { logDosyaYolu = value; }
        }
        
        /// <summary>
        /// Ayrılma durumu
        /// </summary>
        public bool AyrilmaDurumu
        {
            get { return ayrilmaDurumuGerceklesti; }
        }
        
        /// <summary>
        /// Filtreleme aktif mi
        /// </summary>
        public bool FiltrelemeAktif
        {
            get { return filtrelemeAktif; }
        }
        
        /// <summary>
        /// Kalan zaman timer'ını başlatır
        /// </summary>
        /// <param name="toplamSureSaniye">Toplam filtreleme süresi (saniye)</param>
        private void BaslatKalanZamanTimer(int toplamSureSaniye)
        {
            try
            {
                // Mevcut timer'ı durdur
                DurdurKalanZamanTimer();
                
                // Kalan zaman değişkenlerini ayarla
                kalanZamanSaniye = toplamSureSaniye;
                filtrelemeBaslangicZamani = DateTime.Now;
                
                // Timer'ı oluştur ve başlat
                kalanZamanTimer = new Timer();
                kalanZamanTimer.Interval = 1000; // 1 saniye
                kalanZamanTimer.Tick += KalanZamanTimer_Tick;
                kalanZamanTimer.Start();
                
                // İlk durumu göster
                GuncelleKalanZamanGosterimi();
                
                logHata?.Invoke($"⏱️ Kalan zaman timer'ı başlatıldı: {toplamSureSaniye} saniye", false);
            }
            catch (Exception ex)
            {
                logHata?.Invoke($"Kalan zaman timer'ı başlatılırken hata: {ex.Message}", true);
            }
        }
        
        /// <summary>
        /// Kalan zaman timer'ını durdurur
        /// </summary>
        private void DurdurKalanZamanTimer()
        {
            try
            {
                if (kalanZamanTimer != null)
                {
                    kalanZamanTimer.Stop();
                    kalanZamanTimer.Dispose();
                    kalanZamanTimer = null;
                }
                
                kalanZamanSaniye = 0;
            }
            catch (Exception ex)
            {
                logHata?.Invoke($"Kalan zaman timer'ı durdurulurken hata: {ex.Message}", true);
            }
        }
        
        /// <summary>
        /// Kalan zaman timer'ının tick event'i
        /// </summary>
        private void KalanZamanTimer_Tick(object sender, EventArgs e)
        {
            try
            {
                // Kalan zamanı 1 saniye azalt
                kalanZamanSaniye = Math.Max(0, kalanZamanSaniye - 1);
                
                // Eğer süre dolmuşsa timer'ı durdur
                if (kalanZamanSaniye <= 0)
                {
                    kalanZamanSaniye = 0;
                    DurdurKalanZamanTimer();
                }
                
                // Gösterimi güncelle
                GuncelleKalanZamanGosterimi();
            }
            catch (Exception ex)
            {
                logHata?.Invoke($"Kalan zaman timer tick'inde hata: {ex.Message}", true);
            }
        }
        
        /// <summary>
        /// Kalan zaman gösterimini günceller
        /// </summary>
        private void GuncelleKalanZamanGosterimi()
        {
            try
            {
                if (kalanZamanSaniye > 0)
                {
                    labelMultiSpektralDurum.Text = $"Durum: Filtreleme Aktif - Kalan: {kalanZamanSaniye}s";
                    labelMultiSpektralDurum.ForeColor = Color.Green;
                }
                else
                {
                    labelMultiSpektralDurum.Text = "Durum: Filtreleme Tamamlandı";
                    labelMultiSpektralDurum.ForeColor = Color.Blue;
                }
            }
            catch (Exception ex)
            {
                logHata?.Invoke($"Kalan zaman gösterimi güncellenirken hata: {ex.Message}", true);
            }
        }

        /// <summary>
        /// Komutu gerçek olarak execute eder
        /// </summary>
        /// <param name="komut">Gönderilecek komut</param>
        /// <returns>Başarılıysa true</returns>
        private async Task<bool> ExecuteKomut(string komut)
        {
            try
            {
                // Test modu desteği için BaglantiYoneticisi kullan
                // 🔧 STANDART KONUM: Normal/Filtresiz konum için özel komut formatı
                string baslangicKomutu = $"!M1:1:{STANDART_FILTRE_INTERNAL};M2:1:{STANDART_FILTRE_INTERNAL}!";
                
                if (baglantiYoneticisi != null)
                {
                    // BaglantiYoneticisi üzerinden gönder (test/gerçek mod otomatik kontrolü)
                    if (!await baglantiYoneticisi.KomutGonder(baslangicKomutu, manuelKomutMu: true))
                    {
                        throw new Exception("Başlangıç komutu gönderilemedi");
                    }
                }
                else
                {
                    // Fallback: Direkt seri port kullan
                    if (!serialPort.IsOpen) serialPort.Open();
                    serialPort.Write(baslangicKomutu);
                }
                
                logHata?.Invoke("Filtreleme öncesi diskler standart (N) konumuna getiriliyor.", false);
                await Task.Delay(MAKSIMUM_GECIS_SURESI_MS); // Thread.Sleep yerine async delay
                
                string gonderilecekKomut = OlusturMotorKontrolKomutu(komut);
                
                if (baglantiYoneticisi != null)
                {
                    // Test modundaysa TestVeriGonderici'ye orijinal komutu gönder (video efektleri için)
                    if (baglantiYoneticisi.TestModuAktifMi)
                    {
                        baglantiYoneticisi.TestKomutGonder(komut);
                    }
                    
                    // BaglantiYoneticisi üzerinden gönder (test/gerçek mod otomatik kontrolü)
                    if (!await baglantiYoneticisi.KomutGonder(gonderilecekKomut, manuelKomutMu: true))
                    {
                        throw new Exception("Multi-spektral komut gönderilemedi");
                    }
                }
                else
                {
                    // Fallback: Direkt seri port kullan
                    serialPort.Write(gonderilecekKomut);
                }
                
                // UI güncellemeleri
                KomutLogla(komut);
                labelMultiSpektralKomut.Text = "Aktif Komut: " + komut;
                filtrelemeAktif = true;
                labelMultiSpektralAciklama.Text = KomutAciklamasiOlustur(komut);
                GuncelleAlarmSistemi();
                
                int toplamSure = HesaplaToplamFiltrelemeSuresi(komut);
                
                // Kalan zaman timer'ını başlat
                BaslatKalanZamanTimer(toplamSure);
                
                // 🔧 DÜZELTME: Timer kaldırıldı - Görev Yükü otomatik standart konuma döndürüyor
                // Yer İstasyonu sadece komut gönderir, bitiş işlemini Görev Yükü halleder
                // StartCommandTimer(toplamSure); // KALDIRILDI: Çakışma önlemi
                
                return true;
            }
            catch (Exception ex)
            {
                logHata?.Invoke($"Komut execute hatası: {ex.Message}", true);
                return false;
            }
        }

        /// <summary>
        /// 🔧 KALDIRILDI: Komut timer'ı artık kullanılmıyor
        /// Görev Yükü otomatik olarak standart konuma döndürüyor, çakışmayı önlemek için bu metod devre dışı
        /// </summary>
        /// <param name="toplamSure">Toplam filtreleme süresi</param>
        private void StartCommandTimer_DEVREDISKI(int toplamSure)
        {
            // 🚨 BU METOD ARTIK KULLANILMIYOR!
            // Çakışma sorunu: Hem Yer İstasyonu hem Görev Yükü aynı anda standart konuma döndürme komutu gönderiyordu
            // Çözüm: Sadece Görev Yükü otomatik döndürür, Yer İstasyonu sadece başlangıç komutunu gönderir
            
            // Orijinal kod yorum olarak korundu:
            /*
            System.Windows.Forms.Timer timer = new System.Windows.Forms.Timer();
            timer.Interval = toplamSure * 1000;
            timer.Tick += async (sender, e) =>
            {
                string bitisKomutu = $"!M1:1:{STANDART_FILTRE_INTERNAL};M2:1:{STANDART_FILTRE_INTERNAL}!";
                try 
                {
                    if (baglantiYoneticisi != null)
                    {
                        await baglantiYoneticisi.KomutGonder(bitisKomutu, manuelKomutMu: true);
                    }
                    else
                    {
                        if (serialPort.IsOpen) serialPort.Write(bitisKomutu);
                    }
                    logHata?.Invoke("Filtreleme tamamlandı, diskler standart (N) konumuna getiriliyor.", false);
                } 
                catch (Exception ex) 
                {
                    logHata?.Invoke($"Standart konuma dönerken hata: {ex.Message}", true);
                }
                
                DurdurKalanZamanTimer();
                labelMultiSpektralDurum.Text = "Durum: Tamamlandı";
                labelMultiSpektralDurum.ForeColor = Color.Blue;
                filtrelemeAktif = false;
                GuncelleAlarmSistemi();
                
                timer.Stop();
                timer.Dispose();
            };
            timer.Start();
            */
        }
        
        /// <summary>
        /// Filtreleme tamamlandığında UI'yi günceller (Görev Yükü'nden gelen onay sonrası)
        /// </summary>
        public void FiltrelemeKompletOlarakIsaretle()
        {
            // Kalan zaman timer'ını durdur
            DurdurKalanZamanTimer();
            
            labelMultiSpektralDurum.Text = "Durum: Tamamlandı";
            labelMultiSpektralDurum.ForeColor = Color.Blue;
            filtrelemeAktif = false;
            GuncelleAlarmSistemi();
            
            logHata?.Invoke("Filtreleme Görev Yükü tarafından tamamlandı.", false);
        }
    }
} 