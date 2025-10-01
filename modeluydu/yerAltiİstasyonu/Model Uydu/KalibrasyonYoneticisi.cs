using System;
using System.Drawing;
using System.Windows.Forms;
using System.IO.Ports;

namespace ModelUydu
{
    /// <summary>
    /// Kalibrasyon işlemlerini yöneten sınıf
    /// TÜRKSAT Model Uydu Yarışması için farklı şehirlerde kalibrasyon desteği
    /// </summary>
    public class KalibrasyonYoneticisi
    {
        // UI Kontrolleri
        private ComboBox comboBoxSehir;
        private Button buttonBasincKalibre;
        private Button buttonGPSReferans;
        private Button buttonGyroSifirla;
        private Label labelKalibrasyonDurum;
        private Label labelSehirSecimi;
        
        // Sistem referansları
        private BaglantiYoneticisi baglantiYoneticisi;
        private HaritaYoneticisi haritaYoneticisi;
        private SerialPort serialPort1;
        private Action<string, bool> logMetodu;
        
        // Kalibrasyon durumu
        private string secilenSehir = "";
        private double lat = 37.01;
        private double longt = 36.5;

        /// <summary>
        /// KalibrasyonYoneticisi sınıfı yapıcı metodu
        /// </summary>
        public KalibrasyonYoneticisi(
            ComboBox comboBoxSehir,
            Button buttonBasincKalibre,
            Button buttonGPSReferans,
            Button buttonGyroSifirla,
            Label labelKalibrasyonDurum,
            Label labelSehirSecimi,
            BaglantiYoneticisi baglantiYoneticisi,
            HaritaYoneticisi haritaYoneticisi,
            SerialPort serialPort1,
            Action<string, bool> logMetodu)
        {
            this.comboBoxSehir = comboBoxSehir;
            this.buttonBasincKalibre = buttonBasincKalibre;
            this.buttonGPSReferans = buttonGPSReferans;
            this.buttonGyroSifirla = buttonGyroSifirla;
            this.labelKalibrasyonDurum = labelKalibrasyonDurum;
            this.labelSehirSecimi = labelSehirSecimi;
            this.baglantiYoneticisi = baglantiYoneticisi;
            this.haritaYoneticisi = haritaYoneticisi;
            this.serialPort1 = serialPort1;
            this.logMetodu = logMetodu;

            // Event handler'ları bağla
            BaglaEventHandlerlar();
            
            // Başlangıç durumu ayarla
            labelKalibrasyonDurum.Text = "Durum: Şehir seçiniz";
            labelKalibrasyonDurum.ForeColor = Color.Gray;
        }

        /// <summary>
        /// Kalibrasyon event handler'larını bağlar
        /// </summary>
        private void BaglaEventHandlerlar()
        {
            comboBoxSehir.SelectedIndexChanged += ComboBoxSehir_SelectedIndexChanged;
            buttonBasincKalibre.Click += ButtonBasincKalibre_Click;
            buttonGPSReferans.Click += ButtonGPSReferans_Click;
            buttonGyroSifirla.Click += ButtonGyroSifirla_Click;
        }

        /// <summary>
        /// Şehir seçim ComboBox event handler'ı
        /// </summary>
        private void ComboBoxSehir_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                if (comboBoxSehir.SelectedIndex >= 0)
                {
                    secilenSehir = comboBoxSehir.SelectedItem.ToString();
                    labelKalibrasyonDurum.Text = $"✅ {secilenSehir} seçildi - Kalibrasyon butonları aktif";
                    labelKalibrasyonDurum.ForeColor = Color.Green;
                    
                    // Şehir koordinatlarını güncelle
                    GuncelleSehirKoordinatlari(secilenSehir);
                    
                    logMetodu?.Invoke($"Kalibrasyon şehri seçildi: {secilenSehir}", false);
                }
            }
            catch (Exception ex)
            {
                logMetodu?.Invoke($"Şehir seçim hatası: {ex.Message}", true);
                labelKalibrasyonDurum.Text = "❌ Şehir seçim hatası";
                labelKalibrasyonDurum.ForeColor = Color.Red;
            }
        }

        /// <summary>
        /// Basınç kalibrasyonu butonu event handler'ı
        /// </summary>
        private void ButtonBasincKalibre_Click(object sender, EventArgs e)
        {
            try
            {
                if (comboBoxSehir.SelectedIndex < 0)
                {
                    // MessageBox.Show("Önce bir şehir seçiniz!", "Kalibrasyon Uyarısı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            System.Diagnostics.Debug.WriteLine("UYARI: Önce bir şehir seçiniz!");
                    return;
                }

                labelKalibrasyonDurum.Text = "🔧 Basınç kalibrasyonu yapılıyor...";
                labelKalibrasyonDurum.ForeColor = Color.Orange;
                buttonBasincKalibre.Enabled = false;

                // Kalibrasyon değerlerini hesapla
                var kalibrasyonVerisi = HesaplaKalibrasyonDegerleri(secilenSehir);
                
                // Arduino'ya basınç kalibrasyon komutu gönder
                string komut = $"#CALIB_PRESSURE:{kalibrasyonVerisi.DenizSeviyesiBasinc:F2}:{kalibrasyonVerisi.Rakim}#";
                KomutGonderArduino(komut);

                labelKalibrasyonDurum.Text = $"✅ Basınç kalibre edildi ({kalibrasyonVerisi.DenizSeviyesiBasinc:F1} hPa, {kalibrasyonVerisi.Rakim}m)";
                labelKalibrasyonDurum.ForeColor = Color.Green;
                
                logMetodu?.Invoke($"Basınç kalibrasyonu tamamlandı - {secilenSehir}: {kalibrasyonVerisi.DenizSeviyesiBasinc:F2} hPa", false);
                
                // 3 saniye sonra butonu tekrar aktif et
                System.Threading.Tasks.Task.Delay(3000).ContinueWith(t => 
                {
                    if (buttonBasincKalibre.InvokeRequired)
                        buttonBasincKalibre.Invoke(new Action(() => { buttonBasincKalibre.Enabled = true; }));
                    else
                        buttonBasincKalibre.Enabled = true;
                });
            }
            catch (Exception ex)
            {
                logMetodu?.Invoke($"Basınç kalibrasyon hatası: {ex.Message}", true);
                labelKalibrasyonDurum.Text = "❌ Basınç kalibrasyon hatası";
                labelKalibrasyonDurum.ForeColor = Color.Red;
                buttonBasincKalibre.Enabled = true;
            }
        }

        /// <summary>
        /// GPS referans butonu event handler'ı
        /// </summary>
        private void ButtonGPSReferans_Click(object sender, EventArgs e)
        {
            try
            {
                if (comboBoxSehir.SelectedIndex < 0)
                {
                    // MessageBox.Show("Önce bir şehir seçiniz!", "Kalibrasyon Uyarısı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            System.Diagnostics.Debug.WriteLine("UYARI: Önce bir şehir seçiniz!");
                    return;
                }

                labelKalibrasyonDurum.Text = "📍 GPS referans ayarlanıyor...";
                labelKalibrasyonDurum.ForeColor = Color.Orange;
                buttonGPSReferans.Enabled = false;

                // Şehre göre GPS koordinatları al
                var koordinatlar = GetSehirKoordinatlari(secilenSehir);
                
                // Arduino'ya GPS referans komutu gönder
                string komut = $"#CALIB_GPS:{koordinatlar.lat:F6}:{koordinatlar.lng:F6}#";
                KomutGonderArduino(komut);

                // Haritayı seçilen şehrin konumuna güncelle (kalibrasyon modu)
                if (haritaYoneticisi != null)
                {
                    haritaYoneticisi.UpdateMapForCalibration(koordinatlar.lat, koordinatlar.lng);
                    haritaYoneticisi.MerkezKonumaGit();
                }

                labelKalibrasyonDurum.Text = $"✅ GPS referans ayarlandı ({secilenSehir})";
                labelKalibrasyonDurum.ForeColor = Color.Green;
                
                logMetodu?.Invoke($"GPS referans ayarlandı: {secilenSehir}, Lat: {koordinatlar.lat:F6}, Lng: {koordinatlar.lng:F6}", false);
                
                // 3 saniye sonra butonu tekrar aktif et
                System.Threading.Tasks.Task.Delay(3000).ContinueWith(t => 
                {
                    if (buttonGPSReferans.InvokeRequired)
                        buttonGPSReferans.Invoke(new Action(() => { buttonGPSReferans.Enabled = true; }));
                    else
                        buttonGPSReferans.Enabled = true;
                });
            }
            catch (Exception ex)
            {
                logMetodu?.Invoke($"GPS referans ayarlama hatası: {ex.Message}", true);
                labelKalibrasyonDurum.Text = "❌ GPS referans hatası";
                labelKalibrasyonDurum.ForeColor = Color.Red;
                buttonGPSReferans.Enabled = true;
            }
        }

        /// <summary>
        /// Gyro sıfırlama butonu event handler'ı
        /// </summary>
        private void ButtonGyroSifirla_Click(object sender, EventArgs e)
        {
            try
            {
                labelKalibrasyonDurum.Text = "🔄 Gyro sıfırlanıyor... (10 saniye)";
                labelKalibrasyonDurum.ForeColor = Color.Orange;
                buttonGyroSifirla.Enabled = false;

                // Arduino'ya gyro sıfırlama komutu gönder
                string komut = "#CALIB_GYRO:RESET#";
                KomutGonderArduino(komut);

                // 10 saniye bekleme için countdown
                int countdown = 10;
                var timer = new System.Windows.Forms.Timer();
                timer.Interval = 1000;
                timer.Tick += (s, args) =>
                {
                    countdown--;
                    labelKalibrasyonDurum.Text = $"🔄 Gyro kalibre ediliyor... ({countdown})";
                    
                    if (countdown <= 0)
                    {
                        timer.Stop();
                        timer.Dispose();
                        labelKalibrasyonDurum.Text = "✅ Gyro sıfırlama tamamlandı";
                        labelKalibrasyonDurum.ForeColor = Color.Green;
                        buttonGyroSifirla.Enabled = true;
                        logMetodu?.Invoke("Gyro kalibrasyon tamamlandı", false);
                    }
                };
                timer.Start();
            }
            catch (Exception ex)
            {
                logMetodu?.Invoke($"Gyro sıfırlama hatası: {ex.Message}", true);
                labelKalibrasyonDurum.Text = "❌ Gyro sıfırlama hatası";
                labelKalibrasyonDurum.ForeColor = Color.Red;
                buttonGyroSifirla.Enabled = true;
            }
        }

        // 🔧 KALİBRASYON YARDIMCI METODLARI

        /// <summary>
        /// Şehir koordinatlarını günceller (kalibrasyon modunda - rota çizmez)
        /// </summary>
        private void GuncelleSehirKoordinatlari(string sehir)
        {
            try
            {
                var koordinatlar = GetSehirKoordinatlari(sehir);
                lat = koordinatlar.lat;
                longt = koordinatlar.lng;
                
                if (haritaYoneticisi != null)
                {
                    // Kalibrasyon için özel metod kullan (kırmızı çizgi çizmez)
                    haritaYoneticisi.UpdateMapForCalibration(lat, longt);
                }
            }
            catch (Exception ex)
            {
                logMetodu?.Invoke($"Şehir koordinat güncelleme hatası: {ex.Message}", true);
            }
        }

        /// <summary>
        /// Şehir koordinatlarını döndürür
        /// </summary>
        private (double lat, double lng) GetSehirKoordinatlari(string sehir)
        {
            switch (sehir)
            {
                case "Ankara (850m)":
                    return (39.9334, 32.8597);  // Ankara merkez
                case "İstanbul (40m)":
                    return (41.0082, 28.9784);  // İstanbul merkez
                case "İzmir (25m)":
                    return (38.4237, 27.1428);  // İzmir merkez
                case "Antalya (50m)":
                    return (36.8969, 30.7133);  // Antalya merkez
                case "Kayseri (1054m)":
                    return (38.7312, 35.4787);  // Kayseri merkez
                case "Bursa (100m)":
                    return (40.1826, 29.0669);  // Bursa merkez
                default:
                    return (39.9334, 32.8597);  // Varsayılan Ankara
            }
        }

        /// <summary>
        /// Şehre göre kalibrasyon değerlerini hesaplar
        /// </summary>
        private (double DenizSeviyesiBasinc, int Rakim) HesaplaKalibrasyonDegerleri(string sehir)
        {
            // Şehir rakımları (metre)
            int rakim;
            switch (sehir)
            {
                case "Ankara (850m)":
                    rakim = 850;
                    break;
                case "İstanbul (40m)":
                    rakim = 40;
                    break;
                case "İzmir (25m)":
                    rakim = 25;
                    break;
                case "Antalya (50m)":
                    rakim = 50;
                    break;
                case "Kayseri (1054m)":
                    rakim = 1054;
                    break;
                case "Bursa (100m)":
                    rakim = 100;
                    break;
                default:
                    rakim = 850; // Varsayılan Ankara
                    break;
            }

            // Standart atmosfer basıncı (deniz seviyesi: 1013.25 hPa)
            // Rakım artışıyla basınç azalması: her 8.3m için yaklaşık 1 hPa azalma
            double denizSeviyesiBasinc = 1013.25 - (rakim / 8.3);

            return (Math.Round(denizSeviyesiBasinc, 2), rakim);
        }

        /// <summary>
        /// Arduino'ya kalibrasyon komutu gönderir
        /// </summary>
        private async void KomutGonderArduino(string komut)
        {
            try
            {
                if (baglantiYoneticisi != null && baglantiYoneticisi.BaglantiAcikMi)
                {
                    // BaglantiYoneticisi üzerinden manuel komut gönder (test verisi başlatmaz)
                    bool basarili = await baglantiYoneticisi.KomutGonder(komut, manuelKomutMu: true);
                    
                    if (basarili)
                    {
                    logMetodu?.Invoke($"Arduino kalibrasyon komutu gönderildi: {komut}", false);
                    }
                    else
                    {
                        logMetodu?.Invoke($"Arduino kalibrasyon komutu gönderilemedi: {komut}", true);
                    }
                }
                else
                {
                    logMetodu?.Invoke("Kalibrasyon komutu gönderilemiyor: Bağlantı kapalı", true);
                }
            }
            catch (Exception ex)
            {
                logMetodu?.Invoke($"Kalibrasyon komut gönderim hatası: {ex.Message}", true);
            }
        }

        /// <summary>
        /// Seçilen şehri döndürür
        /// </summary>
        public string SecilenSehir => secilenSehir;

        /// <summary>
        /// Kalibrasyon sistemi aktif mi kontrolü
        /// </summary>
        public bool KalibrasyonAktifMi => comboBoxSehir.SelectedIndex >= 0;

        /// <summary>
        /// Manuel kalibrasyon durumu ayarlama
        /// </summary>
        public void SetKalibrasyonDurumu(string durum, Color renk)
        {
            labelKalibrasyonDurum.Text = durum;
            labelKalibrasyonDurum.ForeColor = renk;
        }
    }
} 