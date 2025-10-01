using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ModelUydu
{
    /// <summary>
            /// Test modunda telemetri ve IoT verilerini gönderen sınıf.
    /// Bu sınıf gerçek Arduino bağlantısı olmadan sistemi test etmek için kullanılır.
    /// </summary>
    public class TestVeriGonderici
    {
        // TÜRKSAT Yarışması sabit takım numarası - değişmez!
        private const int TAKIM_NUMARASI = 286570;
        
        // Test verisi gönderme timer'ı
        private Timer testTimer;
        
        // Test verisi alma olayı için delegate
        public delegate void TestVerisiAlmaHandler(object sender, string testVerisi);
        public event TestVerisiAlmaHandler TestVerisiAlindi;

        // Komut alma olayı için delegate
        public delegate void KomutAlinaHandler(object sender, string komut);
        public event KomutAlinaHandler KomutAlindi;

        // Hata loglama için delegate
        private Action<string, bool> logHataMetodu;

        // Test modu durumu
        private bool testModuAktif = false;

        // Test verileri için sayaçlar ve rastgele sayı üretici
        private Random rnd = new Random();
        private int testSayac = 0;
        
        // Uçuş profili değişkenleri
        private double tepeIrtifa; // 500-700m arası rastgele tepe irtifa
        private UcusFazi mevcutFaz;

        // Test telemetri verileri için değişkenler
        private double testYukseklik = 2.0;
        private double testSicaklik = 25.0;
        private double testBasinc = 1013.25;
        private double testPilGerilimi = 12.0;
        private double testHiz = 0.0;
        private double testLatitude = 41.2753; // İstanbul Havalimanı
        private double testLongitude = 28.7519; // İstanbul Havalimanı

        // Komut durumu takibi
        private bool manuelAyrilmaKomutuAlindi = false;
        private string aktifMultiSpektralKomut = "";
        private DateTime komutBaslangicZamani = DateTime.Now;
        
        // Faz geçişleri için zaman kontrolü (paket sayacı resetlenmediği için)
        private DateTime ayrilmaBaslangicZamani = DateTime.Now;

        // Uçuş fazlarını tanımlayan enum
        private enum UcusFazi
        {
            UCUSA_HAZIR,
            TIRMANIS,
            MODEL_UYDU_INIS,
            AYRILMA,
            GOREV_YUKU_INIS,
            KURTARMA
        }

        /// <summary>
        /// TestVeriGonderici sınıfı yapıcı metodu
        /// </summary>
        /// <param name="logHataMetodu">Hata loglama metodu</param>
        public TestVeriGonderici(Action<string, bool> logHataMetodu)
        {
            this.logHataMetodu = logHataMetodu;
            
            // Timer'ı oluştur
            testTimer = new Timer();
            testTimer.Interval = 2000; // 2 saniyede bir veri gönder
            testTimer.Tick += TestTimer_Tick;
        }

        /// <summary>
        /// Test modunu başlatır
        /// </summary>
        public bool TestModuBaslat()
        {
            try
            {
                testModuAktif = true;
                testSayac = 1; // TÜRKSAT şartnamesine uygun: Paket numarası 1'den başlar
                mevcutFaz = UcusFazi.UCUSA_HAZIR;
                testYukseklik = 2.0; // Yer seviyesi
                testHiz = 0.0;
                testPilGerilimi = 12.0; // Tam dolu pil
                
                // Şartnameye uygun rastgele tepe irtifası belirle (500-700m)
                tepeIrtifa = rnd.Next(500, 701);
                LogHata($"🚀 Yeni Test Uçuşu Başlatıldı. Hedef Tepe İrtifa: {tepeIrtifa}m", false);
                
                // Timer'ı başlat
                testTimer.Start();
                
                LogHata("Test modu başlatıldı. Simülasyon veriler gönderiliyor...", false);
                
                return true;
            }
            catch (Exception ex)
            {
                LogHata("Test modu başlatılırken hata: " + ex.Message, true);
                return false;
            }
        }

        /// <summary>
        /// Test modunu durdurur
        /// </summary>
        public bool TestModuDurdur()
        {
            try
            {
                testModuAktif = false;
                
                // Timer'ı durdur
                testTimer.Stop();
                
                LogHata("Test modu durduruldu.", false);
                
                return true;
            }
            catch (Exception ex)
            {
                LogHata("Test modu durdurulurken hata: " + ex.Message, true);
                return false;
            }
        }

        /// <summary>
        /// Test modunda komut alma fonksiyonu
        /// </summary>
        /// <param name="komut">Alınan komut</param>
        /// <returns>Komutun başarıyla işlenip işlenmediği</returns>
        public bool KomutAl(string komut)
        {
            try
            {
                if (!testModuAktif)
                {
                    LogHata("Test modu aktif değil, komut işlenemez: " + komut, true);
                    return false;
                }

                LogHata($"🎮 Test modu komut alındı: {komut}", false);

                // Manuel ayrılma komutu kontrolü
                if (komut == "!xT!")
                {
                    return ManuelAyrilmaKomutuIsle();
                }
                
                // Multi-spektral komut kontrolü (4 karakterli komutlar)
                if (komut.Length == 4 && char.IsDigit(komut[0]) && char.IsDigit(komut[2]))
                {
                    return MultiSpektralKomutuIsle(komut);
                }
                
                // Motor kontrol komutları (Multi-spektral filtreleme için)
                if (komut.StartsWith("!M1:") || komut.StartsWith("!M2:") || komut.Contains("M1:") || komut.Contains("M2:"))
                {
                    return MotorKontrolKomutuIsle(komut);
                }

                // Bilinmeyen komut
                LogHata($"⚠️ Bilinmeyen test komut formatı: {komut}", false);
                return false;
            }
            catch (Exception ex)
            {
                LogHata($"Test komut işleme hatası: {ex.Message}", true);
                return false;
            }
        }

        /// <summary>
        /// Manuel ayrılma komutu işleme
        /// </summary>
        private bool ManuelAyrilmaKomutuIsle()
        {
            try
            {
                if (manuelAyrilmaKomutuAlindi)
                {
                    LogHata("⚠️ Manuel ayrılma komutu daha önce alınmış!", false);
                    return false;
                }

                manuelAyrilmaKomutuAlindi = true;
                komutBaslangicZamani = DateTime.Now;
                
                LogHata("🚀 TEST: Manuel ayrılma komutu işlendi! Video efektleri başlatılıyor...", false);
                
                // Ayrılma fazına geçiş yap (eğer uygun fazda ise)
                if (mevcutFaz == UcusFazi.MODEL_UYDU_INIS && testYukseklik <= 450)
                {
                    mevcutFaz = UcusFazi.AYRILMA;
                    LogHata("📡 TEST: Faz değişimi - Ayrılma fazına geçildi", false);
                }
                
                // Komut event'ini tetikle (video efektleri için)
                KomutAlindi?.Invoke(this, "MANUEL_AYRILMA");
                
                return true;
            }
            catch (Exception ex)
            {
                LogHata($"Manuel ayrılma komutu işlenirken hata: {ex.Message}", true);
                return false;
            }
        }

        /// <summary>
        /// Motor kontrol komutları işleme (Multi-spektral filtreleme için)
        /// </summary>
        private bool MotorKontrolKomutuIsle(string komut)
        {
            try
            {
                LogHata($"🔧 TEST: Motor kontrol komutu işlendi: {komut}", false);
                
                // Motor kontrol komutları test modunda başarılı olarak simüle edilir
                // Gerçek motor hareketi yerine sadece log kaydı yapılır
                
                if (komut.Contains("N"))
                {
                    LogHata("🔧 TEST: Filtre diskleri standart (N) konumuna getiriliyor...", false);
                }
                else
                {
                    LogHata("🔧 TEST: Filtre diskleri belirlenen konumlara getiriliyor...", false);
                }
                
                return true;
            }
            catch (Exception ex)
            {
                LogHata($"Motor kontrol komutu işlenirken hata: {ex.Message}", true);
                return false;
            }
        }

        /// <summary>
        /// Multi-spektral komut işleme
        /// </summary>
        private bool MultiSpektralKomutuIsle(string komut)
        {
            try
            {
                if (!string.IsNullOrEmpty(aktifMultiSpektralKomut))
                {
                    LogHata($"⚠️ Aktif multi-spektral komut var: {aktifMultiSpektralKomut}", false);
                    return false;
                }

                // Komut formatı doğrulama (basit kontrol)
                if (!KomutFormatDogrula(komut))
                {
                    LogHata($"❌ Geçersiz multi-spektral komut formatı: {komut}", true);
                    return false;
                }

                // TEST MODU için otomatik faz geçişi
                if (mevcutFaz != UcusFazi.GOREV_YUKU_INIS)
                {
                    LogHata($"🔄 TEST MODU: Multi-spektral komut için otomatik faz geçişi yapılıyor. Mevcut faz: {mevcutFaz} → GOREV_YUKU_INIS", false);
                    
                    // Test modunda direkt görev yükü iniş fazına geç
                    mevcutFaz = UcusFazi.GOREV_YUKU_INIS;
                    testYukseklik = 350.0; // Görev yükü iniş yüksekliği
                    testHiz = 7.0; // Görev yükü iniş hızı (6-8 m/s)
                    
                    LogHata($"✈️ TEST MODU: Faz değişimi tamamlandı - Görev yükü iniş fazı aktif. Multi-spektral komut işlenebilir.", false);
                }

                aktifMultiSpektralKomut = komut;
                komutBaslangicZamani = DateTime.Now;
                
                LogHata($"🌈 TEST: Multi-spektral komut işlendi: {komut} - Video efektleri başlatılıyor...", false);
                
                // Komut event'ini tetikle (video efektleri için)
                KomutAlindi?.Invoke(this, $"MULTISPEKTRAL_{komut}");
                
                // 15 saniye sonra komutu bitir
                System.Windows.Forms.Timer komutTimer = new System.Windows.Forms.Timer();
                komutTimer.Interval = 15000; // 15 saniye
                komutTimer.Tick += (sender, e) =>
                {
                    aktifMultiSpektralKomut = "";
                    LogHata($"🌈 TEST: Multi-spektral komut tamamlandı: {komut}", false);
                    KomutAlindi?.Invoke(this, "MULTISPEKTRAL_BITTI");
                    komutTimer.Stop();
                    komutTimer.Dispose();
                };
                komutTimer.Start();
                
                return true;
            }
            catch (Exception ex)
            {
                LogHata($"Multi-spektral komut işlenirken hata: {ex.Message}", true);
                return false;
            }
        }

        /// <summary>
        /// Komut formatı doğrulama (basit versiyon)
        /// </summary>
        private bool KomutFormatDogrula(string komut)
        {
            try
            {
                if (string.IsNullOrEmpty(komut) || komut.Length != 4)
                {
                    return false;
                }

                // İlk ve üçüncü karakterler rakam olmalı
                if (!char.IsDigit(komut[0]) || !char.IsDigit(komut[2]))
                {
                    return false;
                }

                // Sürelerin 6-9 arasında olması
                int duration1 = int.Parse(komut[0].ToString());
                int duration2 = int.Parse(komut[2].ToString());
                
                if (duration1 < 6 || duration1 > 9 || duration2 < 6 || duration2 > 9)
                {
                    return false;
                }

                // Toplam süre 15 saniye olmalı
                if (duration1 + duration2 != 15)
                {
                    return false;
                }

                // Filtre karakterleri geçerli olmalı
                string gecerliFiltreler = "RGBCFNMPY";
                if (gecerliFiltreler.IndexOf(komut[1]) == -1 || gecerliFiltreler.IndexOf(komut[3]) == -1)
                {
                    return false;
                }

                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Checksum hesaplama metodu (TelemetriYoneticisi ile aynı algoritma)
        /// </summary>
        private byte HesaplaChecksum(string packet)
        {
            byte checksum = 0;
            foreach (char c in packet)
            {
                checksum ^= (byte)c;
            }
            return checksum;
        }

        /// <summary>
        /// TÜRKSAT şartnamesi uydu statüsü üretir (gerçekçi senaryo)
        /// </summary>
        private string GetTestUyduStatusu()
        {
            // Mevcut faza göre statü kodu döndür
            return ((int)mevcutFaz).ToString();
        }

        /// <summary>
        /// TÜRKSAT şartnamesi ARAS hata kodu üretir (6 haneli binary)
        /// Ayrılma hatası çok nadir, diğer hatalar daha dengeli
        /// </summary>
        private string GetTestHataKodu()
        {
            // ARAS Alarm Sistemi - 6 bit hata kodu
            int hata1 = 0; // Model uydu iniş hızı (12-14 m/s dışında = 1)
            int hata2 = 0; // Görev yükü iniş hızı (6-8 m/s dışında = 1)
            int hata3 = 0; // Taşıyıcı basınç verisi alınamaması = 1
            int hata4 = 0; // Görev yükü konum verisi alınamaması = 1
            int hata5 = 0; // Ayrılmanın gerçekleşmemesi = 1 (ÇOK NADİR)
            int hata6 = 0; // Multi-spektral sistem çalışmaması = 1

            // Gerçekçi hata senaryoları
            
            // Faz bazlı hız kontrolleri
            // Model uydu iniş hızı kontrolü (şartname 12-14 m/s)
            if (mevcutFaz == UcusFazi.MODEL_UYDU_INIS && (testHiz < 12 || testHiz > 14))
            {
                hata1 = 1; // Model uydu iniş hızı problemi
                LogHata($"🚨 ARAS Hata1: Model uydu iniş hızı hatası! Hız={testHiz:F1}m/s (12-14 olmalı)", false);
            }
            
            // Görev yükü iniş hızı kontrolü (şartname 6-8 m/s)
            if (mevcutFaz == UcusFazi.GOREV_YUKU_INIS && (testHiz < 6 || testHiz > 8))
            {
                hata2 = 1; // Görev yükü iniş hızı problemi
                LogHata($"🚨 ARAS Hata2: Görev yükü iniş hızı hatası! Hız={testHiz:F1}m/s (6-8 olmalı)", false);
            }

            // Diğer sistem hataları
            if (rnd.NextDouble() < 0.08) hata3 = 1; // Basınç sensör hatası (%8)
            if (rnd.NextDouble() < 0.06) hata4 = 1; // GPS konum hatası (%6)
            
            // Ayrılma hatası - Sadece ayrılma fazında ve çok nadir (%1)
            if (mevcutFaz == UcusFazi.AYRILMA && rnd.NextDouble() < 0.01) 
            {
                hata5 = 1; // Ayrılma sorunu (ULTRA NADİR %1!)
                LogHata($"🚨 ARAS Hata5: Ayrılma hatası tetiklendi!", false);
            }
            
            // Multi-spektral sistem hatası (görev yükü indikten sonra)
            if (mevcutFaz == UcusFazi.GOREV_YUKU_INIS && rnd.NextDouble() < 0.07) hata6 = 1;

            // ARAS hata kodu oluştur (Binary string: bit5 bit4 bit3 bit2 bit1 bit0)
            string arasKodu = $"{hata6}{hata5}{hata4}{hata3}{hata2}{hata1}";
            
            // Debug log (her 10 pakette bir veya hata varsa)
            if (testSayac % 10 == 0 || arasKodu != "000000")
            {
                LogHata($"🔍 ARAS (Paket {testSayac}): Kod={arasKodu}, Faz={mevcutFaz}, Yükseklik={testYukseklik:F0}m, Hız={testHiz:F1}m/s", false);
            }

            return arasKodu;
        }

        /// <summary>
        /// Timer'ın tick olayında çalışan metod - test verilerini gönderir
        /// </summary>
        private void TestTimer_Tick(object sender, EventArgs e)
        {
            try
            {
                if (!testModuAktif) return;

                testSayac++;

                // TÜRKSAT Şartnamesine uygun gerçekçi telemetri verisi oluşturur ve uçuş fazını günceller.
                GuncelleUcusFaziVeVerileri();

                // TAM 22 ALAN ile telemetri verisi formatı (TelemetriYoneticisi.cs ile uyumlu):
                // PaketNo,UyduStatusu,HataKodu,GondermeSaati,Basinc1,Basinc2,Yukseklik1,Yukseklik2,IrtifaFarki,InisHizi,Sicaklik,PilGerilimi,GpsLatitude,GpsLongitude,GpsAltitude,Pitch,Roll,Yaw,RHRH,IoTS1Data,IoTS2Data,TakimNo
                string uyduStatusu = GetTestUyduStatusu();
                string hataKodu = GetTestHataKodu();
                double basinc2 = testBasinc + (rnd.NextDouble() * 200 - 100); // Basınç 2 (hafif farklı, Pascal cinsinden)
                double yukseklik2 = testYukseklik + rnd.NextDouble() * 4 - 2; // Yükseklik 2 (hafif farklı)
                double irtifaFarki = Math.Abs(testYukseklik - yukseklik2); // İrtifa farkı
                double gpsAltitude = testYukseklik + rnd.NextDouble() * 10 - 5; // GPS altitude
                int pitch = rnd.Next(-90, 91); // Pitch açısı
                int roll = rnd.Next(-90, 91); // Roll açısı  
                int yaw = rnd.Next(-180, 181); // Yaw açısı
                int rhrh = rnd.Next(0, 256); // RHRH değeri
                double iotS1 = 15 + rnd.NextDouble() * 20; // IoT S1 sıcaklık
                double iotS2 = 10 + rnd.NextDouble() * 25; // IoT S2 sıcaklık

                string veriKismi = $"{testSayac},{uyduStatusu},{hataKodu},{DateTime.Now:HH:mm:ss}," +
                                   $"{testBasinc.ToString("F2", System.Globalization.CultureInfo.InvariantCulture)},{basinc2.ToString("F2", System.Globalization.CultureInfo.InvariantCulture)},{testYukseklik.ToString("F1", System.Globalization.CultureInfo.InvariantCulture)},{yukseklik2.ToString("F1", System.Globalization.CultureInfo.InvariantCulture)}," +
                                   $"{irtifaFarki.ToString("F1", System.Globalization.CultureInfo.InvariantCulture)},{testHiz.ToString("F1", System.Globalization.CultureInfo.InvariantCulture)},{testSicaklik.ToString("F1", System.Globalization.CultureInfo.InvariantCulture)},{testPilGerilimi.ToString("F2", System.Globalization.CultureInfo.InvariantCulture)}," +
                                   $"{testLatitude.ToString("F6", System.Globalization.CultureInfo.InvariantCulture)},{testLongitude.ToString("F6", System.Globalization.CultureInfo.InvariantCulture)},{gpsAltitude.ToString("F1", System.Globalization.CultureInfo.InvariantCulture)}," +
                                   $"{pitch},{roll},{yaw},{rhrh}," +
                                   $"{iotS1.ToString("F1", System.Globalization.CultureInfo.InvariantCulture)},{iotS2.ToString("F1", System.Globalization.CultureInfo.InvariantCulture)},{TAKIM_NUMARASI}";

                // Checksum hesapla
                byte checksum = HesaplaChecksum(veriKismi);
                
                // Doğru format ile telemetri verisi oluştur: $veriler*checksum
                string testTelemetri = $"${veriKismi}*{checksum:X2}";

                // Telemetri verisini gönder
                TestVerisiAlindi?.Invoke(this, testTelemetri);

                // IoT test verisi ayrı göndermiyoruz - zaten telemetri paketinde IoTS1Data ve IoTS2Data var
                // Bu sayede format hatası oluşmuyor

                // Video test verisi oluştur (her 5. gönderimde)
                if (testSayac % 5 == 0)
                {
                    GenerateVideoTestData();
                }
            }
            catch (Exception ex)
            {
                LogHata("Test verisi gönderilirken hata oluştu: " + ex.Message, true);
            }
        }

        /// <summary>
        /// TÜRKSAT Model Uydu Yarışması şartnamesine tam uyumlu, gerçekçi bir uçuş senaryosu oluşturur.
        /// Uçuş fazlarını yönetir ve telemetri verilerini her faz için uygun şekilde günceller.
        /// </summary>
        private void GuncelleUcusFaziVeVerileri()
        {
            // Uçuş Fazına Göre Mantık
            switch (mevcutFaz)
            {
                case UcusFazi.UCUSA_HAZIR:
                    testHiz = 0;
                    testYukseklik = 2.0 + (rnd.NextDouble() - 0.5); // Yer seviyesinde hafif dalgalanma
                    if (testSayac > 5) // 5 paket sonra tırmanışa geç
                    {
                        mevcutFaz = UcusFazi.TIRMANIS;
                        LogHata($"✈️ Faz Değişimi: {mevcutFaz}. {tepeIrtifa}m hedefine tırmanış başladı.", false);
            }
                    break;

                case UcusFazi.TIRMANIS:
                    // Roket motoru ateşlenmiş gibi hızla yükselme
                    testHiz += 5.0 + (rnd.NextDouble() * 2); // Hızlanarak yükselir
                    if (testHiz > 80) testHiz = 80; // Maksimum tırmanış hızı
                    testYukseklik += testHiz; // Yükselme
                    
                    if (testYukseklik >= tepeIrtifa)
                    {
                        testYukseklik = tepeIrtifa;
                        mevcutFaz = UcusFazi.MODEL_UYDU_INIS;
                        LogHata($"✈️ Faz Değişimi: {mevcutFaz}. Tepe irtifaya ({tepeIrtifa}m) ulaşıldı. Pasif iniş başlıyor.", false);
                    }
                    break;

                case UcusFazi.MODEL_UYDU_INIS:
                    // Şartname: 12-14 m/s hızla pasif iniş
                    testHiz = 13.0 + (rnd.NextDouble() * 2 - 1.0); // 12.0 ile 14.0 arası
                    testYukseklik = Math.Max(400.0, testYukseklik - testHiz); // 400m'ye kadar kontrollü iniş

                    if (testYukseklik <= 400.0)
                    {
                        mevcutFaz = UcusFazi.AYRILMA;
                        ayrilmaBaslangicZamani = DateTime.Now; // Ayrılma fazı başlangıç zamanını kaydet
                        LogHata($"✈️ Faz Değişimi: {mevcutFaz}. 400m irtifaya ulaşıldı. Ayrılma prosedürü başlıyor.", false);
                        // TÜRKSAT şartnamesine uygun: Paket numarası hiçbir zaman resetlenmez
                    }
                    break;

                case UcusFazi.AYRILMA:
                    // Ayrılma birkaç saniye sürer, bu sırada hız biraz düşebilir
                    testHiz *= 0.95; 
                    testYukseklik = Math.Max(350.0, testYukseklik - testHiz); // Ayrılma sırasında da kontrollü iniş
                    
                    // Ayrılma fazı yaklaşık 6-8 saniye sürer (paket sayacı resetlenmediği için zaman kontrol)
                    double gecenSure = (DateTime.Now - ayrilmaBaslangicZamani).TotalSeconds;
                    if (gecenSure > 7.0) // 7 saniye sonra görev yükü inişine geç
                    {
                        mevcutFaz = UcusFazi.GOREV_YUKU_INIS;
                        LogHata($"✈️ Faz Değişimi: {mevcutFaz}. Ayrılma tamamlandı ({gecenSure:F1}s). Görev yükü inişi başlıyor.", false);
                    }
                    break;

                case UcusFazi.GOREV_YUKU_INIS:
                    // Şartname: 6-8 m/s hızla görev yükü inişi
                    testHiz = 7.0 + (rnd.NextDouble() * 2 - 1.0); // 6.0 ile 8.0 arası
                    testYukseklik = Math.Max(1.0, testYukseklik - testHiz); // Yere kadar kontrollü iniş

                    if (testYukseklik <= 10.0)
                    {
                        mevcutFaz = UcusFazi.KURTARMA;
                        LogHata($"✈️ Faz Değişimi: {mevcutFaz}. Görev yükü yere indi. Kurtarma modu aktif.", false);
                    }
                    break;

                case UcusFazi.KURTARMA:
                    testHiz = 0;
                    testYukseklik = 1.0 + (rnd.NextDouble() * 0.5); // Yerde duruyor
                    // Bu fazda kalır
                    break;
            }

            // Fiziksel modellere göre diğer verileri hesapla
            // Standart Atmosfer Modeli (yaklaşık) - Pascal cinsine çevir
            double basincHPa = 1013.25 * Math.Pow(1 - 0.0065 * testYukseklik / 288.15, 5.255);
            testBasinc = basincHPa * 100.0; // hPa'dan Pascal'a çevir (1 hPa = 100 Pa)
            testSicaklik = 15.0 - (testYukseklik / 150.0) + (rnd.NextDouble() * 2 - 1); // Yükseldikçe hava soğur
            
            // Genel Değişkenler
            testPilGerilimi -= 0.015; // Pil her saniye biraz azalır
            if (testPilGerilimi < 9.0) testPilGerilimi = 9.0;

            // İstanbul Havalimanı ve çevresinde rüzgar etkisiyle hafif sapmalar
            testLatitude += (rnd.NextDouble() - 0.5) * 0.0003;  // Yaklaşık ±150m sapma
            testLongitude += (rnd.NextDouble() - 0.5) * 0.0003;
        }

        /// <summary>
        /// Video test verisi oluşturur ve gönderir
        /// </summary>
        private void GenerateVideoTestData()
        {
            try
            {
                // Test modu aktifse video frame bilgisi gönder
                if (testModuAktif)
                {
                    LogHata($"🎬 Test video frame {testSayac} gönderildi.", false);
                    
                    // Video frame event'i tetikleme (eğer gerekirse)
                    // Bu kısım KameraYoneticisi tarafından otomatik olarak yönetiliyor
                }
                else
                {
                    LogHata("Video frame gönderimi - gerçek mod aktif değil.", false);
                }
            }
            catch (Exception ex)
            {
                // Video hatası kritik değil, sadece logla
                LogHata("Video test verisi oluşturulurken hata: " + ex.Message, false);
            }
        }

        /// <summary>
        /// Test modu aktif mi?
        /// </summary>
        public bool TestModuAktifMi
        {
            get { return testModuAktif; }
        }

        /// <summary>
        /// Kaynakları temizler
        /// </summary>
        public void Dispose()
        {
            if (testTimer != null)
            {
                testTimer.Stop();
                testTimer.Dispose();
            }
        }

        /// <summary>
        /// Hata loglama metodu
        /// </summary>
        private void LogHata(string hataMesaji, bool kritikMi = false)
        {
            logHataMetodu?.Invoke(hataMesaji, kritikMi);
        }
    }
} 