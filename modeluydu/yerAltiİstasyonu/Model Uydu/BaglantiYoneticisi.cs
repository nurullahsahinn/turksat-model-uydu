using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO.Ports;
using System.Windows.Forms;

namespace ModelUydu
{
    /// <summary>
    /// Bağlantı hatası detaylarını tutan sınıf - Error context preservation için
    /// </summary>
    public class BaglantiHataDetayi
    {
        public string HataTuru { get; set; }
        public string HataMessaji { get; set; }
        public string KullaniciDostu { get; set; }
        public string Port { get; set; }
        public DateTime Zaman { get; set; }
        public string InnerException { get; set; }
    }

    /// <summary>
    /// Seri port bağlantılarını yöneten sınıf.
    /// Bu sınıf port listelerini doldurma, bağlantı açma/kapama ve bağlantı durumu takibi işlemlerini gerçekleştirir.
    /// </summary>
    public class BaglantiYoneticisi
    {
        // Seri port nesnesi
        private SerialPort seriPort;

        // UI kontrollerinin referansları
        private ComboBox portComboBox;
        private TextBox baudRateTextBox;
        private Button baglanButon;
        private Button kesButon;
        private Label durumLabel;

        // Telemetri verisi alma olayı için delegate
        public delegate void TelemetriVerisiAlmaHandler(object sender, string telemetriVerisi);
        public event TelemetriVerisiAlmaHandler TelemetriVerisiAlindi;

        // 🔧 YENİ: Binary video verisi alma olayı için delegate
        public delegate void BinaryVeriAlmaHandler(object sender, byte[] binaryVeri);
        public event BinaryVeriAlmaHandler BinaryVeriAlindi;

        // Hata loglama için delegate
        private Action<string, bool> logHataMetodu;

        // Bağlantı durumu
        private bool baglantiDurumu = false;

        // Seri porttan gelen verileri biriktirmek için arabellek
        private StringBuilder _serialBuffer = new StringBuilder();

        // 🔧 YENİ: Binary video frame işleme için buffer
        private List<byte> _binaryBuffer = new List<byte>();

        // Test verisi gönderici
        private TestVeriGonderici testVeriGonderici;

        // Kamera yöneticisi referansı (test modu için)
        private KameraYoneticisi kameraYoneticisi;

        // Son bağlantı hatası detayları (error context preservation)
        public BaglantiHataDetayi SonBaglantiHatasi { get; private set; }

        /// <summary>
        /// BaglantiYoneticisi sınıfı yapıcı metodu
        /// </summary>
        /// <param name="seriPort">SerialPort nesnesi</param>
        /// <param name="portComboBox">Port seçim ComboBox'ı</param>
        /// <param name="baudRateTextBox">Baud rate ayarı için TextBox</param>
        /// <param name="baglanButon">Bağlan butonu</param>
        /// <param name="kesButon">Bağlantıyı kes butonu</param>
        /// <param name="durumLabel">Durum bilgisi etiketi</param>
        /// <param name="logHataMetodu">Hata loglama metodu</param>
        public BaglantiYoneticisi(
            SerialPort seriPort,
            ComboBox portComboBox,
            TextBox baudRateTextBox,
            Button baglanButon,
            Button kesButon,
            Label durumLabel,
            Action<string, bool> logHataMetodu)
        {
            this.seriPort = seriPort;
            this.portComboBox = portComboBox;
            this.baudRateTextBox = baudRateTextBox;
            this.baglanButon = baglanButon;
            this.kesButon = kesButon;
            this.durumLabel = durumLabel;
            this.logHataMetodu = logHataMetodu;

            // Seri port veri alma olayını tanımla
            this.seriPort.DataReceived += new SerialDataReceivedEventHandler(SerialPort_DataReceived);

            // Test verisi gönderici oluştur
            testVeriGonderici = new TestVeriGonderici(this.logHataMetodu);
            testVeriGonderici.TestVerisiAlindi += TestVeriGonderici_TestVerisiAlindi;
            testVeriGonderici.KomutAlindi += TestVeriGonderici_KomutAlindi;

            // Kullanıcı arayüzünü güncelle
            GuncelleBaglantiDurumu();

            // Başlangıçta portları doldur
            PortlariListele();
            
            // 🚀 VIDEO İÇİN OPTİMİZE EDİLMİŞ BAUD RATE - Otomatik ayarla
            if (string.IsNullOrEmpty(baudRateTextBox.Text))
            {
                baudRateTextBox.Text = "57600";
            }
        }

        /// <summary>
        /// Kamera yöneticisi referansını ayarlar (test modu için gerekli)
        /// </summary>
        /// <param name="kameraYoneticisi">Kamera yöneticisi referansı</param>
        public void KameraYoneticisiAyarla(KameraYoneticisi kameraYoneticisi)
        {
            this.kameraYoneticisi = kameraYoneticisi;
        }

        /// <summary>
        /// Mevcut portları listeler ve ComboBox'a ekler
        /// </summary>
        public void PortlariListele()
        {
            try
            {
                // ComboBox'ı temizle
                portComboBox.Items.Clear();

                // Önce Test Verisi seçeneğini ekle
                portComboBox.Items.Add("Test Verisi");

                // Mevcut portları al
                string[] portlar = SerialPort.GetPortNames();
                
                // ComboBox'a ekle
                foreach (string port in portlar)
                {
                    portComboBox.Items.Add(port);
                }

                // Eğer Test Verisi de dahil en az bir item varsa, ilkini seç (Test Verisi)
                if (portComboBox.Items.Count > 0)
                {
                    portComboBox.SelectedIndex = 0;
                }
                else
                {
                    LogHata("Kullanılabilir seri port bulunamadı.", false);
                }
            }
            catch (Exception ex)
            {
                LogHata("Port listesi alınırken hata: " + ex.Message, true);
            }
        }

        /// <summary>
        /// Bağlantıyı açar
        /// </summary>
        /// <returns>Başarılıysa true, değilse false</returns>
        public async Task<bool> BaglantiyiAc()
        {
            try
            {
                // Test verisi seçiliyse test modunu başlat
                if (portComboBox.Text == "Test Verisi")
                {
                    // Önce mevcut bağlantıları kapat
                    if (seriPort.IsOpen)
                    {
                        seriPort.Close();
                    }

                    // Test modunu başlat
                    bool testBasarili = testVeriGonderici.TestModuBaslat();
                    
                    if (testBasarili)
                    {
                        // Kamera test modunu da başlat
                        if (kameraYoneticisi != null)
                        {
                            kameraYoneticisi.TestModuBaslat();
                            LogHata("Kamera simülasyon modu başlatıldı.", false);
                        }
                        else
                        {
                            LogHata("⚠️ Kamera yöneticisi bulunamadı. Video test modu başlatılamadı.", false);
                        }
                        
                        // Bağlantı durumunu güncelle
                        baglantiDurumu = true;
                        GuncelleBaglantiDurumu();
                        
                        // Test modu başarılı mesajı
                        LogHata("Simülasyon modu başlatıldı.", false);
                        
                        return true;
                    }
                    else
                    {
                        // Test modu başlatılamadı
                        baglantiDurumu = false;
                        GuncelleBaglantiDurumu();
                        // MessageBox.Show("TEST MODU BAŞLATILAMADI", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        System.Diagnostics.Debug.WriteLine("HATA: TEST MODU BAŞLATILAMADI");
                        return false;
                    }
                }
                else
                {
                    // Normal seri port bağlantısı
                    
                    // Test modu aktifse durdur
                    if (testVeriGonderici.TestModuAktifMi)
                    {
                        testVeriGonderici.TestModuDurdur();
                    }

                // Bağlantı zaten açıksa kapat
                if (seriPort.IsOpen)
                {
                    seriPort.Close();
                }

                // Bağlantı ayarlarını yap
                seriPort.PortName = portComboBox.Text;
                
                // DÜZELTME: Timeout ayarları ekle
                seriPort.ReadTimeout = 5000;    // 5 saniye okuma timeout
                seriPort.WriteTimeout = 5000;   // 5 saniye yazma timeout
                seriPort.ReceivedBytesThreshold = 1;  // 1 byte gelince tetikle
                
                // Baud rate ayarını yap
                if (!string.IsNullOrEmpty(baudRateTextBox.Text))
                {
                    seriPort.BaudRate = Convert.ToInt32(baudRateTextBox.Text);
                }
                else
                {
                    seriPort.BaudRate = 57600; // 🚀 VIDEO İÇİN OPTİMİZE EDİLMİŞ BAUD RATE
                }

                // 🔧 UI DONMA SORUNU ÇÖZÜMü (Additional feedback and timeout)
                logHataMetodu?.Invoke("Seri port açılıyor...", false);
                
                // UI'ı güncelle
                await Task.Delay(50); // UI thread'e nefes alma fırsatı ver
                
                await Task.Run(() => {
                    // Seri port timeout'larını ayarla
                    seriPort.ReadTimeout = 3000;  // 3 saniye okuma timeout
                    seriPort.WriteTimeout = 3000; // 3 saniye yazma timeout
                    
                    // Port'u aç
                    seriPort.Open();
                });
                
                // Bağlantı sonrası stabilizasyon
                await Task.Delay(100);
                
                logHataMetodu?.Invoke("✅ Seri port başarıyla açıldı", false);
                
                // Bağlantı durumunu güncelle
                baglantiDurumu = true;
                GuncelleBaglantiDurumu();
                
                // ŞARTNAME GEREĞİ: Gerçek port bağlantısında kamera otomatik başlat
                if (kameraYoneticisi != null)
                {
                    kameraYoneticisi.VideoStreamBaslat(); // Canlı video stream
                                            LogHata("Kamera canlı video stream'i başlatıldı.", false);
                }
                else
                {
                    LogHata("⚠️ Kamera yöneticisi bulunamadı. Video stream başlatılamadı.", false);
                }
                
                // Bağlantı başarılı mesajı
                LogHata($"'{seriPort.PortName}' portuna bağlantı başarılı. Baud Rate: {seriPort.BaudRate}", false);
                
                return true;
                }
            }
            catch (UnauthorizedAccessException ex)
            {
                // 🔧 Port erişim hatası - genelde başka program kullanıyor
                logHataMetodu?.Invoke($"❌ PORT ERİŞİM HATASI: {seriPort.PortName} başka program tarafından kullanılıyor!", true);
                logHataMetodu?.Invoke("💡 ÇÖZÜM: XCTU veya başka terminal programını kapatın", false);
                logHataMetodu?.Invoke($"🔍 Detay: {ex.Message}", false);
                return false;
            }
            catch (Exception ex)
            {
                // Hata mesajı (daha detaylı)
                string hataDetay = "";
                if (ex is UnauthorizedAccessException)
                {
                    hataDetay = "Port başka bir uygulama tarafından kullanılıyor.";
                }
                else if (ex is ArgumentException)
                {
                    hataDetay = "Geçersiz port adı veya ayarları.";
                }
                else if (ex is System.IO.IOException)
                {
                    hataDetay = "Port bulunamadı veya erişilemez durumda.";
                }
                else if (ex is TimeoutException)
                {
                    hataDetay = "Bağlantı zaman aşımına uğradı (5 saniye).";
                }
                else
                {
                    hataDetay = ex.Message;
                }
                
                LogHata($"Bağlantı açılırken hata: {hataDetay}", true);
                
                // 🔧 ERROR PROPAGATION FIX: Detailed error context preservation
                SonBaglantiHatasi = new BaglantiHataDetayi
                {
                    HataTuru = ex.GetType().Name,
                    HataMessaji = ex.Message,
                    KullaniciDostu = hataDetay,
                    Port = seriPort?.PortName ?? "Bilinmiyor",
                    Zaman = DateTime.Now,
                    InnerException = ex.InnerException?.Message
                };
                
                // Kullanıcı arayüzünde detaylı uyarı göster - POPUP AKTİF
                MessageBox.Show($"BAĞLANTI KURULAMADI\n\n" +
                                $"Port: {portComboBox.Text}\n" +
                                $"Hata: {hataDetay}\n\n" +
                                $"Çözüm: 'Test Verisi' seçeneğini deneyin.",
                                "Bağlantı Hatası", 
                                MessageBoxButtons.OK, 
                                MessageBoxIcon.Warning);
                System.Diagnostics.Debug.WriteLine($"BAĞLANTI HATASI: {hataDetay}");
                
                // Bağlantı durumunu güncelle
                baglantiDurumu = false;
                GuncelleBaglantiDurumu();
                
                return false;
            }
        }

        /// <summary>
        /// Bağlantıyı kapatır
        /// </summary>
        /// <returns>Başarılıysa true, değilse false</returns>
        public bool BaglantiyiKapat()
        {
            try
            {
                // Test modu aktifse durdur
                if (testVeriGonderici.TestModuAktifMi)
                {
                    testVeriGonderici.TestModuDurdur();
                    
                    // Kamera test modunu da durdur
                    if (kameraYoneticisi != null && kameraYoneticisi.TestModuAktifMi)
                    {
                        kameraYoneticisi.TestModuDurdur();
                        LogHata("Kamera simülasyon modu durduruldu.", false);
                    }
                }

                // Bağlantı açıksa kapat
                if (seriPort.IsOpen)
                {
                    seriPort.Close();
                    
                    // ŞARTNAME GEREĞİ: Gerçek port bağlantısı kesildiğinde kamera durdur
                    if (kameraYoneticisi != null && kameraYoneticisi.VideoStreamAktifMi)
                    {
                        kameraYoneticisi.VideoStreamDurdur();
                        LogHata("Kamera canlı video stream'i durduruldu.", false);
                    }
                }
                
                // Bağlantı durumunu güncelle
                baglantiDurumu = false;
                GuncelleBaglantiDurumu();
                
                // Bağlantı kapandı mesajı
                LogHata("Bağlantı kapatıldı.", false);
                
                return true;
            }
            catch (Exception ex)
            {
                // Hata mesajı
                LogHata("Bağlantı kapatılırken hata: " + ex.Message, true);

                // Kullanıcı arayüzünde uyarı göster - POPUP AKTİF
                MessageBox.Show(ex.Message, "Bağlantı Kapatma Hatası", MessageBoxButtons.OK, MessageBoxIcon.Error);
                System.Diagnostics.Debug.WriteLine($"BAĞLANTI KAPATMA HATASI: {ex.Message}");
                
                return false;
            }
        }

        /// <summary>
        /// Test verisi alındığında çalışan metod
        /// </summary>
        private void TestVeriGonderici_TestVerisiAlindi(object sender, string testVerisi)
        {
            try
            {
                // Test verisini normal telemetri verisi gibi işle
                TelemetriVerisiAlindi?.Invoke(this, testVerisi);
            }
            catch (Exception ex)
            {
                LogHata("Test verisi işlenirken hata: " + ex.Message, true);
            }
        }

        /// <summary>
        /// Test verisi alındığında çalışan metod
        /// </summary>
        private void TestVeriGonderici_KomutAlindi(object sender, string komut)
        {
            try
            {
                // Kamera yöneticisi varsa video efektleri başlat
                if (kameraYoneticisi == null || !kameraYoneticisi.TestModuAktifMi || string.IsNullOrEmpty(komut))
                {
                    return;
                }

                // YARISHMA GÜVENLİĞİ: Bu event SADECE test modunda tetiklenir
                // Gerçek modda bu kod hiç çalışmaz
                if (komut == "MANUEL_AYRILMA")
                {
                    kameraYoneticisi.TestKomutEfekti("MANUEL_AYRILMA");
                }
                else if (komut.StartsWith("MULTISPEKTRAL_"))
                {
                    string spektralKomut = komut.Replace("MULTISPEKTRAL_", "");
                    if (!string.IsNullOrEmpty(spektralKomut))
                    {
                        kameraYoneticisi.TestKomutEfekti(spektralKomut);
                    }
                }
                else if (komut == "MULTISPEKTRAL_BITTI")
                {
                    kameraYoneticisi.TestKomutEfekti("NORMAL");
                }
            }
            catch (Exception ex)
            {
                LogHata($"❌ Test komut event'i işlenirken HATA: {ex.Message}", true);
            }
        }

        /// <summary>
        /// Bağlantı durumuna göre UI kontrollerini günceller
        /// </summary>
        private void GuncelleBaglantiDurumu()
        {
            try
            {
                // Bağlantı durumuna göre butonların etkinliğini ve durum etiketini güncelle
                if (baglantiDurumu)
                {
                    // Bağlantı açık
                    baglanButon.Enabled = false;
                    kesButon.Enabled = true;
                    
                    // Test modu aktif mi kontrol et
                    if (testVeriGonderici != null && testVeriGonderici.TestModuAktifMi)
                    {
                        durumLabel.Text = "Simülasyon Modu";
                        durumLabel.ForeColor = System.Drawing.Color.Orange;
                    }
                    else
                    {
                        // 🔧 FIX: Communication quality indicator
                        string qualityInfo = GetConnectionQualityStatus();
                        durumLabel.Text = "Bağlantı açık: " + seriPort.PortName + qualityInfo;
                        
                        // Color based on communication errors
                        if (_communicationErrorCount > 50)
                            durumLabel.ForeColor = System.Drawing.Color.Red;
                        else if (_communicationErrorCount > 10)
                            durumLabel.ForeColor = System.Drawing.Color.Orange;
                        else
                            durumLabel.ForeColor = System.Drawing.Color.Green;
                    }
                }
                else
                {
                    // Bağlantı kapalı
                    baglanButon.Enabled = true;
                    kesButon.Enabled = false;
                    durumLabel.Text = "Bağlantı kapalı";
                    durumLabel.ForeColor = System.Drawing.Color.Red;
                }
            }
            catch (Exception ex)
            {
                LogHata("Bağlantı durumu güncellenirken hata: " + ex.Message, false);
            }
        }

        /// <summary>
        /// 🔧 FIX: Connection quality status for UI display
        /// </summary>
        private string GetConnectionQualityStatus()
        {
            if (_communicationErrorCount == 0)
                return " ✅";
            else if (_communicationErrorCount < 10)
                return $" ⚠️({_communicationErrorCount})";
            else
                return $" ❌({_communicationErrorCount})";
        }

        /// <summary>
        /// Seri porttan veri alındığında tetiklenen olay - GÜNCELLENMIŞ: Binary video frame desteği
        /// Gelen veriyi bir arabellekte biriktirir ve tam satırları işler.
        /// </summary>
        private void SerialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            try
            {
                SerialPort sp = (SerialPort)sender;
                
                // 🔧 SÜPER DEBUG: Port durumunu kontrol et
                if (!sp.IsOpen)
                {
                    LogHata("❌ SÜPER DEBUG: Seri port kapalı!", true);
                    return;
                }
                
                // 🔧 SÜPER DEBUG: Bytes in buffer
                int bytesAvailable = sp.BytesToRead;
                LogHata($"📡 SÜPER DEBUG: {bytesAvailable} bytes mevcut", false);
                
                if (bytesAvailable == 0)
                {
                    LogHata("⚠️ SÜPER DEBUG: Hiç byte yok!", true);
                    return;
                }
                
                // 🔧 YENİ: Raw bytes al (hem text hem binary için)
                byte[] rawData = new byte[sp.BytesToRead];
                int bytesRead = sp.Read(rawData, 0, rawData.Length);
                
                // 🔧 SÜPER DEBUG: Okunan veri bilgisi
                LogHata($"📨 SÜPER DEBUG: {bytesRead} bytes okundu", false);
                
                // 🔧 SÜPER DEBUG: Raw bytes'ı hex olarak göster
                string hexData = BitConverter.ToString(rawData, 0, Math.Min(50, rawData.Length));
                LogHata($"🔢 SÜPER DEBUG: Raw bytes (hex): {hexData}", false);
                
                // 🔧 SÜPER DEBUG: ASCII olarak dene
                try
                {
                    string asciiData = System.Text.Encoding.ASCII.GetString(rawData, 0, Math.Min(100, rawData.Length));
                    LogHata($"🔤 SÜPER DEBUG: ASCII: {asciiData}", false);
                }
                catch
                {
                    LogHata($"🔤 SÜPER DEBUG: ASCII decode başarısız", false);
                }
                
                // Binary buffer'a ekle
                _binaryBuffer.AddRange(rawData);
                
                // 🔧 SÜPER DEBUG: Buffer durumu
                LogHata($"📦 SÜPER DEBUG: Binary buffer boyutu: {_binaryBuffer.Count}", false);
                
                // 🔧 FIX: Buffer overflow koruması - çok büyük buffer'ları temizle
                if (_binaryBuffer.Count > 10000) // 10KB limit
                {
                    LogHata("⚠️ Binary buffer çok büyük, temizleniyor...", true);
                    _binaryBuffer.Clear();
                    _serialBuffer.Clear();
                }
                
                // 1. Binary video frame kontrolü yap
                CheckForBinaryVideoFrames();
                
                // 2. Text data işleme
                ProcessTextData(rawData);
            }
            catch (Exception ex)
            {
                LogHata("❌ Veri okunurken hata: " + ex.Message, true);
                // 🔧 SÜPER DEBUG: Exception detayları
                LogHata($"🔍 SÜPER DEBUG: Exception türü: {ex.GetType().Name}", true);
                
                // 🔧 FIX: Communication error handling
                HandleCommunicationError();
            }
        }

        /// <summary>
        /// 🔧 YENİ: Binary video frame'leri tespit eder ve işler
        /// DEADBEEF + JPEG + CAFEBABE formatını arar
        /// </summary>
        private void CheckForBinaryVideoFrames()
        {
            try
            {
                byte[] startMarker = { 0xDE, 0xAD, 0xBE, 0xEF }; // DEADBEEF
                byte[] endMarker = { 0xCA, 0xFE, 0xBA, 0xBE };   // CAFEBABE
                
                while (true)
                {
                    // Başlangıç marker'ını ara
                    int startIndex = FindByteSequence(_binaryBuffer.ToArray(), startMarker);
                    if (startIndex == -1) break; // Başlangıç bulunamadı
                    
                    // Bitiş marker'ını ara (başlangıçtan sonra)
                    int searchStartIndex = startIndex + startMarker.Length;
                    if (searchStartIndex >= _binaryBuffer.Count) break;
                    
                    byte[] searchArray = new byte[_binaryBuffer.Count - searchStartIndex];
                    _binaryBuffer.CopyTo(searchStartIndex, searchArray, 0, searchArray.Length);
                    
                    int endIndex = FindByteSequence(searchArray, endMarker);
                    if (endIndex == -1) break; // Bitiş bulunamadı
                    
                    // Frame'in toplam uzunluğu
                    int frameLength = searchStartIndex + endIndex + endMarker.Length;
                    
                    // Frame'i çıkar
                    byte[] frameData = new byte[frameLength];
                    _binaryBuffer.CopyTo(0, frameData, 0, frameLength);
                    
                    // Buffer'dan frame'i kaldır
                    _binaryBuffer.RemoveRange(0, frameLength);
                    
                    // Event'i tetikle
                    BinaryVeriAlindi?.Invoke(this, frameData);
                }
            }
            catch (Exception ex)
            {
                LogHata("Binary video frame işlenirken hata: " + ex.Message, true);
            }
        }

        /// <summary>
        /// 🔧 YENİ: Byte dizisinde belirli bir sequence arar
        /// </summary>
        private int FindByteSequence(byte[] source, byte[] pattern)
        {
            for (int i = 0; i <= source.Length - pattern.Length; i++)
            {
                bool found = true;
                for (int j = 0; j < pattern.Length; j++)
                {
                    if (source[i + j] != pattern[j])
                    {
                        found = false;
                        break;
                    }
                }
                if (found) return i;
            }
            return -1;
        }

        /// <summary>
        /// 🔧 FIX: Binary/corrupted data kontrolü - sadece valid text data kabul et
        /// </summary>
        private bool IsValidTextData(byte[] data)
        {
            if (data == null || data.Length == 0) return false;
            
            int validCharCount = 0;
            int totalChars = data.Length;
            
            foreach (byte b in data)
            {
                // Printable ASCII range (32-126) + common control chars (9=TAB, 10=LF, 13=CR)
                if ((b >= 32 && b <= 126) || b == 9 || b == 10 || b == 13)
                {
                    validCharCount++;
                }
                // Null bytes are often signs of binary corruption
                else if (b == 0)
                {
                    return false;
                }
            }
            
            // At least 80% of bytes should be valid text characters
            double validRatio = (double)validCharCount / totalChars;
            return validRatio >= 0.80;
        }

        /// <summary>
        /// 🔧 FIX: Packet synchronization - corrupted data'dan sonra valid packet'ları bul
        /// </summary>
        private void SynchronizePackets()
        {
            try
            {
                string bufferContent = _serialBuffer.ToString();
                
                // Buffer çok büyükse (5KB+) ve valid packet yoksa temizle
                if (bufferContent.Length > 5000 && !bufferContent.Contains("$") && !bufferContent.Contains("#VIDEO:"))
                {
                    LogHata("⚠️ Buffer çok büyük ve valid packet yok, temizleniyor...", true);
                    _serialBuffer.Clear();
                    return;
                }
                
                // Valid packet start marker'larını ara
                int dollarIndex = bufferContent.IndexOf('$');
                int videoIndex = bufferContent.IndexOf("#VIDEO:");
                
                // En yakın valid marker'ı bul
                int validStartIndex = -1;
                if (dollarIndex >= 0 && videoIndex >= 0)
                {
                    validStartIndex = Math.Min(dollarIndex, videoIndex);
                }
                else if (dollarIndex >= 0)
                {
                    validStartIndex = dollarIndex;
                }
                else if (videoIndex >= 0)
                {
                    validStartIndex = videoIndex;
                }
                
                // Valid marker'dan önce corrupted data varsa temizle
                if (validStartIndex > 0)
                {
                    string discardedData = bufferContent.Substring(0, validStartIndex);
                    LogHata($"🧹 Corrupted data temizlendi: {discardedData.Length} karakter", false);
                    _serialBuffer.Remove(0, validStartIndex);
                }
            }
            catch (Exception ex)
            {
                LogHata($"Packet synchronization hatası: {ex.Message}", true);
            }
        }

        private int _communicationErrorCount = 0;
        private DateTime _lastErrorTime = DateTime.MinValue;

        /// <summary>
        /// 🔧 FIX: Communication error handling and diagnostics
        /// </summary>
        private void HandleCommunicationError()
        {
            _communicationErrorCount++;
            _lastErrorTime = DateTime.Now;
            
            // Her 10 hatada bir diagnostic mesaj göster
            if (_communicationErrorCount % 10 == 0)
            {
                LogHata("", true);
                LogHata("🔧 =================== COMMUNICATION DIAGNOSTIC ===================", true);
                LogHata($"📊 Toplam communication error sayısı: {_communicationErrorCount}", true);
                LogHata("", true);
                LogHata("🔍 MUHTEMEL NEDENLER:", true);
                LogHata("   1. ❌ Baud rate uyumsuzluğu (gönderici-alıcı farklı)", true);
                LogHata("   2. ❌ XBee konfigürasyon hatası", true);
                LogHata("   3. ❌ Elektriksel parazit/interference", true);
                LogHata("   4. ❌ Kablo bağlantı sorunu", true);
                LogHata("", true);
                LogHata("🛠️ ÖNERİLEN ÇÖZÜMLER:", true);
                LogHata("   1. ✅ XBee baud rate'ini kontrol edin (9600/57600)", true);
                LogHata("   2. ✅ XBee konfigürasyonunu sıfırlayın", true);
                LogHata("   3. ✅ Kablo bağlantılarını kontrol edin", true);
                LogHata("   4. ✅ Antenlerin düzgün bağlı olduğunu kontrol edin", true);
                LogHata("   5. ✅ Güç kaynağını kontrol edin", true);
                LogHata("=================================================================", true);
                LogHata("", true);
            }
        }

        /// <summary>
        /// 🔧 YENİ: Text data işleme (eski string-based telemetri için)
        /// </summary>
        private void ProcessTextData(byte[] rawData)
        {
            try
            {
                // 🔧 FIX: Binary corruption kontrolü - sadece printable ASCII karakterleri kabul et
                if (!IsValidTextData(rawData))
                {
                    LogHata($"⚠️ Binary/corrupted data detected and skipped ({rawData.Length} bytes)", false);
                    return;
                }
                
                // Raw data'yı string'e çevir
                string textData = System.Text.Encoding.UTF8.GetString(rawData);
                
                // 🔧 SÜPER DEBUG: Text data bilgisi
                LogHata($"🔤 SÜPER DEBUG: Text data ({textData.Length} kar): {textData.Substring(0, Math.Min(100, textData.Length))}...", false);
                
                _serialBuffer.Append(textData);

                // 🔧 FIX: Packet synchronization - try to find valid packet start markers
                SynchronizePackets();

                // Tamponda tam bir satır ('\n' ile biten) olup olmadığını kontrol et
                string bufferContent = _serialBuffer.ToString();
                int newlineIndex;
                while ((newlineIndex = bufferContent.IndexOf('\n')) >= 0)
                {
                    // Satırı al (yeni satır karakteri hariç)
                    string line = bufferContent.Substring(0, newlineIndex).Trim();
                    
                    // İşlenen satırı tampondan kaldır
                    bufferContent = bufferContent.Substring(newlineIndex + 1);

                    // Eğer satır boş değilse ve video frame değilse, olayı tetikle
                    if (!string.IsNullOrEmpty(line))
                    {
                        // VIDEO FRAME KONTROLÜ - Video frame'leri direkt işle
                        if (line.StartsWith("#VIDEO:") && line.EndsWith("#"))
                        {
                            // Video frame - direkt TelemetriVerisiAlindi event'ini tetikle
                            System.Diagnostics.Debug.WriteLine($"🎥 VIDEO FRAME BULUNDU: {line.Length} karakter");
                            LogHata($"🎥 VIDEO FRAME ALINDI: {line.Length} karakter", false);
                            Console.WriteLine($"🎥 CONSOLE: VIDEO FRAME BULUNDU - {line.Length} karakter");
                            TelemetriVerisiAlindi?.Invoke(this, line);
                            continue; // Bu satırı işledik, döngüye devam
                        }
                        
                        // TÜM VERİLERİ LOGLA (DEBUG İÇİN)
                        if (line.Length > 10) // Kısa veriler hariç
                        {
                            System.Diagnostics.Debug.WriteLine($"📡 GELEN VERİ: [{line.Length} kar] {line.Substring(0, Math.Min(50, line.Length))}...");
                            LogHata($"📡 GELEN VERİ: [{line.Length} kar] {line.Substring(0, Math.Min(50, line.Length))}...", false);
                        }
                        
                        // Normal telemetri için filtre (video frame değilse)
                        // ÖNEMLİ: Video frame'leri zaten yukarıda işlendi, bu sadece normal telemetri için
                        if (line.Length > 200 || 
                            (line.Length > 50 && line.Contains("==")))
                        {
                            // Video frame değilse uzun veri - atla
                            if (!line.StartsWith("#VIDEO:"))
                            {
                                System.Diagnostics.Debug.WriteLine($"Bağlantı yöneticisi: Uzun veri atlandı ({line.Length} karakter)");
                                continue;
                            }
                            else
                            {
                                // Video frame ama yukarıda yakalanmadı - tekrar dene
                                System.Diagnostics.Debug.WriteLine($"🔄 VIDEO FRAME İKİNCİ ŞANS: {line.Length} karakter");
                                LogHata($"🔄 VIDEO FRAME İKİNCİ ŞANS: {line.Length} karakter", false);
                                TelemetriVerisiAlindi?.Invoke(this, line);
                                continue;
                            }
                        }
                        
                        TelemetriVerisiAlindi?.Invoke(this, line);
                    }
                }
                
                // Tamponu güncellenmiş haliyle tekrar ata
                _serialBuffer.Clear();
                _serialBuffer.Append(bufferContent);
            }
            catch (Exception ex)
            {
                LogHata("Text data işlenirken hata: " + ex.Message, true);
            }
        }

        /// <summary>
        /// Seri port üzerinden komut gönderir
        /// </summary>
        /// <param name="komut">Gönderilecek komut</param>
        /// <param name="manuelKomutMu">Manuel komut ise true (otomatik bağlantı açmaz)</param>
        /// <returns>Başarılıysa true, değilse false</returns>
        public async Task<bool> KomutGonder(string komut, bool manuelKomutMu = false)
        {
            try
            {
                // Test modu kontrol et
                if (testVeriGonderici != null && testVeriGonderici.TestModuAktifMi)
                {
                    // TEST MODU: TestVeriGonderici'ye komut gönder (LOG YOK - YARISHMA GÜVENLİĞİ)
                    bool testKomutBasarili = testVeriGonderici.KomutAl(komut);
                    return testKomutBasarili;
                }
                else
                {
                    // GERÇEK MOD: Seri porta komut gönder
                    
                    // Manuel komut ise bağlantı kontrolü yap ama otomatik açma
                    if (manuelKomutMu)
                    {
                        if (!seriPort.IsOpen)
                        {
                            LogHata("Manuel komut gönderilirken hata: Bağlantı noktası kapalı.", true);
                            return false;
                        }
                    }
                    else
                    {
                        // Normal komut - bağlantı açık değilse açmayı dene
                        if (!seriPort.IsOpen)
                        {
                            if (!await BaglantiyiAc())
                            {
                                return false;
                            }
                        }
                    }
                    
                    // Komutu seri porta gönder
                    seriPort.Write(komut);
                    
                    return true;
                }
            }
            catch (Exception ex)
            {
                // Hata mesajı
                LogHata("Komut gönderilirken hata: " + ex.Message, true);
                return false;
            }
        }

        /// <summary>
        /// Giriş tamponunu temizler
        /// </summary>
        public void TemizleGirisTamponu()
        {
            try
            {
                if (seriPort.IsOpen)
                {
                    seriPort.DiscardInBuffer();
                }
            }
            catch (Exception ex)
            {
                LogHata("Giriş tamponu temizlenirken hata: " + ex.Message, false);
            }
        }

        /// <summary>
        /// Çıkış tamponunu temizler
        /// </summary>
        public void TemizleCikisTamponu()
        {
            try
            {
                if (seriPort.IsOpen)
                {
                    seriPort.DiscardOutBuffer();
                }
            }
            catch (Exception ex)
            {
                LogHata("Çıkış tamponu temizlenirken hata: " + ex.Message, false);
            }
        }

        /// <summary>
        /// Bağlantının açık olup olmadığını döndürür
        /// </summary>
        public bool BaglantiAcikMi
        {
            get { return baglantiDurumu; }
        }

        /// <summary>
        /// Seri port nesnesini döndürür
        /// </summary>
        public SerialPort SeriPort
        {
            get { return seriPort; }
        }
        
        /// <summary>
        /// Test modu aktif mi?
        /// </summary>
        public bool TestModuAktifMi
        {
            get { return testVeriGonderici != null && testVeriGonderici.TestModuAktifMi; }
        }
        
        /// <summary>
        /// Test modundayken TestVeriGonderici'ye komut gönder (video efektleri için)
        /// </summary>
        /// <param name="komut">Gönderilecek komut</param>
        /// <returns>Başarılı olup olmadığı</returns>
        public bool TestKomutGonder(string komut)
        {
            try
            {
                if (testVeriGonderici != null && testVeriGonderici.TestModuAktifMi)
                {
                    return testVeriGonderici.KomutAl(komut);
                }
                return false;
            }
            catch (Exception ex)
            {
                LogHata($"Test komutu gönderilirken hata: {ex.Message}", true);
                return false;
            }
        }

        /// <summary>
        /// Kaynakları serbest bırakır
        /// </summary>
        public void Dispose()
        {
            try
            {
                // Test verisi göndericiyi temizle
                if (testVeriGonderici != null)
                {
                    testVeriGonderici.Dispose();
                }

                // Bağlantıyı kapat
                if (seriPort != null && seriPort.IsOpen)
                {
                    seriPort.Close();
                }
            }
            catch (Exception ex)
            {
                LogHata("BaglantiYoneticisi kapatılırken hata: " + ex.Message, false);
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
    }
} 