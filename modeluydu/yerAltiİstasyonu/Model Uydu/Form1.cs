using System; // Temel C# sınıfları ve veri tipleri için gerekli namespace
using System.Collections.Generic; // List, Dictionary gibi koleksiyon sınıfları için gerekli namespace
using System.ComponentModel; // Component sınıfı ve ilgili özellikler için gerekli namespace
using System.Data; // Veri işleme sınıfları için gerekli namespace
using System.Drawing; // Grafik işlemleri için gerekli namespace
using System.Linq; // LINQ sorguları için gerekli namespace
using System.Text; // Metin işleme sınıfları için gerekli namespace
using System.Threading.Tasks; // Asenkron programlama için gerekli namespace
using System.Windows.Forms; // Windows Forms uygulamaları için gerekli namespace
using System.IO.Ports; // Seri port iletişimi için gerekli namespace
// Microsoft.Office.Interop.Excel referansları kaldırıldı - artık EPPlus kullanıyoruz
using OpenTK; // 3D grafik işlemleri için OpenTK kütüphanesi
using OpenTK.Graphics.OpenGL; // OpenGL grafik işlemleri için gerekli namespace
using GMap.NET; // Harita işlemleri için GMap kütüphanesi
using GMap.NET.MapProviders; // Harita sağlayıcıları için gerekli namespace
using AForge.Video.DirectShow; // Kamera erişimi için DirectShow kütüphanesi
using AForge.Video; // Video işleme için AForge kütüphanesi
using System.Diagnostics; // Performans ölçümü ve hata ayıklama için gerekli namespace
using AForge; // AForge temel kütüphanesi
//RECORD
using Accord.Video.FFMPEG; // Video kodlama/çözme için FFMPEG kütüphanesi
using Accord.Video.VFW; // Video for Windows kütüphanesi
using System.IO; // Dosya işlemleri için gerekli namespace
using System.Text.RegularExpressions; // Düzenli ifadeler için gerekli namespace
using System.Media; // Ses çalma işlemleri için gerekli namespace, SoundPlayer için
using WinForms = System.Windows.Forms;
using SystemTextBox = System.Windows.Forms.TextBox; // Forms TextBox için alias
// Excel referansları kaldırıldı - artık EPPlus kullanıyoruz
using System.Windows.Forms.DataVisualization.Charting;  // Chart kontrolü için
using System.Threading;
using GMap.NET.WindowsForms;
using GMap.NET.WindowsForms.Markers;
using SystemAction = System.Action; // System.Action için kısaltma tanımı
using SystemPoint = System.Drawing.Point; // System.Drawing.Point için kısaltma tanımı
using SystemFont = System.Drawing.Font; // System.Drawing.Font için kısaltma tanımı
using SystemRectangle = System.Drawing.Rectangle; // System.Drawing.Rectangle için kısaltma tanımı
using System.Drawing.Drawing2D; // GradientBrush için

namespace ModelUydu // Proje namespace'i
{
    public partial class Form1 : Form // Form1 sınıfı, Form sınıfından türetilmiş
    {
        // Veri değişkenleri
        public static double lat = 37.01, longt = 36.5; // Başlangıç enlem ve boylam değerleri
        int zoom = 5; // Harita yakınlaştırma seviyesi
        
        // Timer değişkenleri
        private System.Windows.Forms.Timer Zamanlayici;
        // hataBildirimZamanlayici artık HataYoneticisi sınıfına taşındı
        private System.Windows.Forms.Timer sdKartTimer;

        // Multi-spektral filtreleme için değişkenler -> MultiSpektralFiltreYoneticisi sınıfına taşındı

        // ARAS (Arayüz Alarm Sistemi) değişkenleri AlarmSistemiYoneticisi sınıfına taşındı

        // IoT ile ilgili değişkenler artık IoTYoneticisi sınıfına taşındı

        // Hata yönetimi için değişkenler artık HataYoneticisi sınıfına taşındı
                    // SD kart yönetimi için değişkenler artık SDKartYoneticisi sınıfına taşındı

        // Form yapıcı metodu
        public Form1()
        {
            InitializeComponent(); // Form bileşenlerini başlat

            // Timer'ları başlat
            // TimerXYZ artık UyduGorsellestime sınıfı tarafından yönetiliyor
            Zamanlayici = new System.Windows.Forms.Timer();
            sdKartTimer = new System.Windows.Forms.Timer();

            // Timer ayarları
            Zamanlayici.Interval = 1000; // 1 saniye
            sdKartTimer.Interval = 10000;

            // Timer event handler'ları
            Zamanlayici.Tick += Zamanlayici_Tick;
            // SD kart işlemleri artık SDKartYoneticisi tarafından yönetiliyor

            // Multi-spektral durum etiketini başlangıçta ayarla
            labelMultiSpektralDurum.Text = "Durum: Bekleniyor";
            labelMultiSpektralDurum.ForeColor = Color.Gray;

            // Uydu statüsünü başlangıçta ayarla
            labelUyduStatu.Text = "BEKLEMEDE";
            labelUyduStatu.ForeColor = Color.White;
            labelUyduStatu.BackColor = Color.FromArgb(255, 150, 0); // Turuncu
            textBox6.Text = "BEKLEMEDE";
        }

        // SD kart işlemleri artık SDKartYoneticisi sınıfı tarafından yönetiliyor

        // Not: Kamera işlemleri artık KameraYoneticisi sınıfı tarafından yönetiliyor

        // OpenGL kontrol yüklendiğinde çalışan metod
        private void glControl1_Load(object sender, EventArgs e)
        {
            // UyduGorsellestime sınıfı kendi OpenGL yükleme işlemlerini yapıyor
            // Burada ek bir işlem yapmaya gerek yok
        }

        // OpenGL kontrol çizildiğinde çalışan metod (3D model çizimi)
        private void glControl1_Paint(object sender, PaintEventArgs e)
        {
            // UyduGorsellestime sınıfı kendi OpenGL çizim işlemlerini yapıyor
            // Burada ek bir işlem yapmaya gerek yok
        }

        // X ekseni dönüşünü başlat/durdur butonu
        private void button1_Click_1(object sender, EventArgs e)
        {
            // UyduGorsellestime sınıfının X dönüşünü kontrol eden metodunu çağır
            uyduGorsellestime.ToggleX();
        }

        // Y ekseni dönüşünü başlat/durdur butonu
        private void button2_Click(object sender, EventArgs e)
        {
            // UyduGorsellestime sınıfının Y dönüşünü kontrol eden metodunu çağır
            uyduGorsellestime.ToggleY();
        }

        // Z ekseni dönüşünü başlat/durdur butonu
        private void button3_Click(object sender, EventArgs e)
        {
            // UyduGorsellestime sınıfının Z dönüşünü kontrol eden metodunu çağır
            uyduGorsellestime.ToggleZ();
        }

        // Label3 tıklama olayı
        private void label3_Click(object sender, EventArgs e)
        {
            // Label3 tıklama işlemleri buraya eklenebilir
        }

        

        // DataGridView hücre tıklama olayı
        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            // Hücre tıklama işlemleri buraya eklenebilir
        }

        // Not: OpenVideoSource ve CloseCurrentVideoSource metotları KameraYoneticisi sınıfına taşındı

        // TextBox3 metin değişikliği olayı
        private void textBox3_TextChanged(object sender, EventArgs e)
        {
            // TextBox3 metin değişikliği işlemleri buraya eklenebilir
        }

        // TextBox2 metin değişikliği olayı
        private void textBox2_TextChanged(object sender, EventArgs e)
        {
            // TextBox2 metin değişikliği işlemleri buraya eklenebilir
        }

        // Form yüklendiğinde çalışan metod
        private void Form1_Load(object sender, EventArgs e)
        {
            // 🔧 İnitial UI state ayarları
            kes.Enabled = false;  // Başlangıçta disconnect button disabled
            baglan.Text = "Bağlan";
            baglan.Enabled = true;
            
            // 🔧 Default baud rate ayarı (tüm sistem 57600 kullanıyor)
            if (string.IsNullOrEmpty(texBaundRate.Text))
            {
                texBaundRate.Text = "57600"; // XBee default baud rate
            }
            
            // 🔧 Baud rate için tooltip ekle
            ToolTip baudRateTooltip = new ToolTip();
            baudRateTooltip.SetToolTip(texBaundRate, "XBee iletişim hızı (57600 önerilen)\nTüm sistem 57600 baud kullanmaktadır");
            
            // 🔧 Label'ı daha açıklayıcı yap
            if (label1.Text == "Baudrate")
            {
                label1.Text = "Baud Rate (57600)";
            }
            
            // Form ana renk ayarları - açık mavi-yeşil karışımı arka plan
            this.BackColor = System.Drawing.Color.FromArgb(240, 250, 255); // Çok açık mavi-yeşil karışımı
            this.ForeColor = System.Drawing.SystemColors.ControlText;

            // DataGridView stil ayarları artık EkleDataGridViewTestVeri() metodunda yapılıyor

            // HataYoneticisi nesnesini oluştur
            hataYoneticisi = new HataYoneticisi(this, "hata_log.txt");

            // GrafikYoneticisi nesnesini oluştur
            grafikYoneticisi = new GrafikYoneticisi(
                chartSicaklik,
                chartYukseklik,
                chartBasinc,
                chartHiz,
                chartPilGerilimi,
                chartIoT,
                LogHata);

            // Tüm grafikleri yapılandır
            grafikYoneticisi.ConfigureAllCharts();

            // UyduGorsellestime nesnesini oluştur - 3D görselleştirme için
            uyduGorsellestime = new UyduGorsellestime(
                glControl1,
                label3, label4, label5,
                textBox6,
                Log // Artık LogHata yerine HataYoneticisi.Log kullanılıyor
            );

            // HaritaYoneticisi nesnesini oluştur
            haritaYoneticisi = new HaritaYoneticisi(
                gMapControl1,
                Log // Artık LogHata yerine HataYoneticisi.Log kullanılıyor
            );

            // KameraYoneticisi nesnesini oluştur (Görev Yükü Video Stream için)
            // ŞARTNAME GEREĞİ: Kamera sistemi otomatik çalışır, manuel butonlar kaldırıldı
            kameraYoneticisi = new KameraYoneticisi(
                videoSourcePlayer,
                Log // Hata loglama fonksiyonu
            );

            // SDKartYoneticisi kaldırıldı - artık direkt dosya kaydetme işlemi yapılacak

            // MultiSpektralFiltreYoneticisi nesnesini oluştur
            multiSpektralFiltreYoneticisi = new MultiSpektralFiltreYoneticisi(
                labelMultiSpektralDurum,
                labelMultiSpektralKomut,
                labelMultiSpektralAciklama,
                textBoxMultiSpektralKomut,
                serialPort1,
                Log // Artık LogHata yerine HataYoneticisi.Log kullanılıyor
            );

            // AlarmSistemiYoneticisi (ARAS) nesnesini oluştur
            alarmSistemiYoneticisi = new AlarmSistemiYoneticisi(
                labelHataKodu,
                checkBoxSesliUyari,
                panelHata1, panelHata2, panelHata3, 
                panelHata4, panelHata5, panelHata6,
                panelAlarm1, panelAlarm2, panelAlarm3, 
                panelAlarm4, panelAlarm5, panelAlarm6,
                Log // Artık LogHata yerine HataYoneticisi.Log kullanılıyor
            );
            
            // MultiSpektralFiltreYoneticisi'ne AlarmSistemiYoneticisi referansını ayarla
            multiSpektralFiltreYoneticisi.SetAlarmSistemiYoneticisi(alarmSistemiYoneticisi);
            
            // Başlangıçta ayrılma durumunu false olarak ayarla
            multiSpektralFiltreYoneticisi.GuncelleAyrilmaDurumu(false);

            // Seri port veri alma olayını tanımla
            OkumaNesnesi.DataReceived += new SerialDataReceivedEventHandler(SerialPort1_DataReceived);

            // IoTYoneticisi nesnesini oluştur
            iotYoneticisi = new IoTYoneticisi(
                labelIoTS1Deger,
                labelIoTS2Deger,
                grafikYoneticisi,
                Log // Artık LogHata yerine HataYoneticisi.Log kullanılıyor
            );
            
            // IoT sistemi - sadece gerçek bağlantıdan veri alır
            // iotYoneticisi.SimuleIoTVeri(); // IoT simülasyonu kullanılmaz - Sadece gerçek veriler
            
            // UI'ı temiz başlat - gerçek telemetri için hazır
            labelIoTS1Deger.Text = "Bekleniyor...";
            labelIoTS2Deger.Text = "Bekleniyor...";
            
            // Tüm telemetri textbox'larını temizle
            textBox1.Text = ""; // GPS Latitude
            textBox2.Text = ""; // GPS Longitude
            textBox3.Text = ""; // GPS Altitude
            textBox4.Text = ""; // Basınç 1
            textBox5.Text = ""; // Basınç 2
            textBox7.Text = ""; // Hata Kodu
            textBox8.Text = ""; // Gönderme Saati
            textBox9.Text = ""; // Yükseklik 1
            textBox10.Text = ""; // Yükseklik 2
            textBox11.Text = ""; // İrtifa Farkı
            textBox12.Text = ""; // İniş Hızı
            textBox13.Text = ""; // Pitch
            textBox14.Text = ""; // Roll
            textBox15.Text = ""; // Paket No
            textBox16.Text = ""; // Sıcaklık
            textBox17.Text = ""; // Pil Gerilimi
            textBox18.Text = ""; // Yaw
            textBox19.Text = ""; // RHRH
            textBox20.Text = ""; // IoT S1 Sıcaklık
            textBox21.Text = ""; // IoT S2 Sıcaklık
            textBox22.Text = ""; // Takım No

            // 🌌 Modern uzay temalı arka plan
            GL.ClearColor(Color.FromArgb(13, 26, 51)); // Derin uzay mavisi (#0D1A33)

            // DataGridView stil ayarları ve test verileri artık EkleDataGridViewTestVeri() metodunda yapılıyor

            // Kamera cihazları artık KameraYoneticisi tarafından yönetiliyor

            // Log dosyasının var olup olmadığını kontrol et
            if (!File.Exists(logDosyaYolu))
            {
                // Dosya yoksa, başlık satırı ile oluştur
                using (StreamWriter sw = new StreamWriter(logDosyaYolu))
                {
                    sw.WriteLine("Zaman - Komut Bilgisi");
                }
            }

            // Sesli uyarı işlemleri artık AlarmSistemiYoneticisi tarafından yönetiliyor

            // Uygulama başlatıldığında yapılacak işlemler
            try
            {
                // Hata kodunu ve panelleri ayarlama işlemleri artık AlarmSistemiYoneticisi tarafından yönetiliyor

                // Hata etiketlerini ayarla
                labelHata1.Text = "Taşıyıcı İniş Hızı";
                labelHata2.Text = "Görev Yükü İniş Hızı";
                labelHata3.Text = "Taşıyıcı Basınç";
                labelHata4.Text = "Görev Yükü Konum";
                labelHata5.Text = "Ayrılma Durumu";
                labelHata6.Text = "Multi-Spektral Sistem";

                // Küçük alarm panel etiketlerini ayarla
                label14.Text = "T-Hız";
                label15.Text = "G-Hız";
                label16.Text = "Basınç";
                label17.Text = "Konum";
                label18.Text = "Ayrılma";
                label19.Text = "M-Spek";
            }
            catch (Exception ex)
            {
                Log("Form yüklenirken hata: " + ex.Message, true);
            }

            // BaglantiYoneticisi nesnesini oluştur
            baglantiYoneticisi = new BaglantiYoneticisi(
                OkumaNesnesi,
                comboBox1,
                texBaundRate,
                baglan,
                kes,
                label1,
                Log // Log metodunu kullan
            );

            // Telemetri verisi alındığında çalışacak metod
            baglantiYoneticisi.TelemetriVerisiAlindi += TelemetriVerisiAlindi;
            
            // 🔧 YENİ: Binary video verisi alındığında çalışacak metod
            baglantiYoneticisi.BinaryVeriAlindi += BinaryVideoVerisiAlindi;
            
            // Kamera yöneticisi referansını BaglantiYoneticisi'ne aktar (test modu için)
            baglantiYoneticisi.KameraYoneticisiAyarla(kameraYoneticisi);
            
            // Bağlantı yöneticisi referansını MultiSpektralFiltreYoneticisi'ne aktar (test modu için)
            multiSpektralFiltreYoneticisi.SetBaglantiYoneticisi(baglantiYoneticisi);

            // TextBox kontrollerini içeren sözlük oluştur - telemetri yöneticisi için
            Dictionary<string, SystemTextBox> telemetriKontrolleri = new Dictionary<string, SystemTextBox>
            {
                { "textBox1", textBox1 }, // GPS Latitude
                { "textBox2", textBox2 }, // GPS Longitude
                { "textBox3", textBox3 }, // GPS Altitude
                { "textBox4", textBox4 }, // Basınç 1
                { "textBox5", textBox5 }, // Basınç 2
                { "textBox6", textBox6 }, // Uydu Statüsü
                { "textBox7", textBox7 }, // Hata Kodu
                { "textBox8", textBox8 }, // Gönderme Saati
                { "textBox9", textBox9 }, // Yükseklik 1
                { "textBox10", textBox10 }, // Yükseklik 2
                { "textBox11", textBox11 }, // İrtifa Farkı
                { "textBox12", textBox12 }, // İniş Hızı
                { "textBox13", textBox13 }, // Pitch
                { "textBox14", textBox14 }, // Roll
                { "textBox15", textBox15 }, // Paket No
                { "textBox16", textBox16 }, // Sıcaklık
                { "textBox17", textBox17 }, // Pil Gerilimi
                { "textBox18", textBox18 }, // Yaw
                { "textBox19", textBox19 }, // RHRH
                { "textBox20", textBox20 }, // IoT S1 Sıcaklık
                { "textBox21", textBox21 }, // IoT S2 Sıcaklık
                { "textBox22", textBox22 }  // Takım No
            };

            // TelemetriYoneticisi nesnesini oluştur
            telemetriYoneticisi = new TelemetriYoneticisi(
                dataGridView1,
                telemetriKontrolleri,
                grafikYoneticisi,
                uyduGorsellestime,
                haritaYoneticisi,
                iotYoneticisi,
                alarmSistemiYoneticisi,
                UyduDurumuGuncelle, // uyduDurumuGuncelleDelegesi
                Log, // logHataFonksiyonu
                labelTakimNumarasi,
                alarmSistemiYoneticisi.HataKoduGuncelle,
                multiSpektralFiltreYoneticisi,
                kameraYoneticisi,
                labelUyduStatu);

            // 📊 ExcelVeriAktarmaYoneticisi nesnesini oluştur (TelemetriYoneticisi'nden sonra)
            excelVeriAktarmaYoneticisi = new ExcelVeriAktarmaYoneticisi(this, hataYoneticisi, telemetriYoneticisi);

            // 🔧 KalibrasyonYoneticisi nesnesini oluştur (TÜRKSAT Yarışma Şehirleri için)
            kalibrasyonYoneticisi = new KalibrasyonYoneticisi(
                comboBoxSehir,
                buttonBasincKalibre,
                buttonGPSReferans,
                buttonGyroSifirla,
                labelKalibrasyonDurum,
                labelSehirSecimi,
                baglantiYoneticisi,
                haritaYoneticisi,
                serialPort1,
                Log // Log metodunu kalibrasyon işlemleri için kullan
            );

            // 📊 DataGridView extreme kompakt konfigürasyonu
            ConfigureDataGridViewKompakt();
            
            // 📊 DataGridView hazır - gerçek telemetri için bekleniyor
        }

        /// <summary>
        /// DataGridView'i extreme kompakt tasarım ile konfigüre eder
        /// </summary>
        private void ConfigureDataGridViewKompakt()
        {
            try
            {
                // DataGridView'i temizle ve emoji'li başlıkları ayarla
                dataGridView1.Columns.Clear();
                
                // 📦 Emoji'li sütunları ekle
                dataGridView1.Columns.Add("PaketNo", "📋 Paket No");
                dataGridView1.Columns.Add("UyduStatusu", "🛰️ Uydu Statüsü");
                dataGridView1.Columns.Add("HataKodu", "⚠️ Hata Kodu");
                dataGridView1.Columns.Add("GondermeSaati", "⏰ Gönderme Saati");
                dataGridView1.Columns.Add("Basinc1", "🌀 Basınç 1");
                dataGridView1.Columns.Add("Basinc2", "🌀 Basınç 2");
                dataGridView1.Columns.Add("Yukseklik1", "📏 Yükseklik 1");
                dataGridView1.Columns.Add("Yukseklik2", "📏 Yükseklik 2");
                dataGridView1.Columns.Add("IrtifaFarki", "📐 İrtifa Farkı");
                dataGridView1.Columns.Add("InisHizi", "🏃 İniş Hızı");
                dataGridView1.Columns.Add("Sicaklik", "🌡️ Sıcaklık");
                dataGridView1.Columns.Add("PilGerilimi", "🔋 Pil Gerilimi");
                dataGridView1.Columns.Add("GPSLatitude", "🌍 GPS Latitude");
                dataGridView1.Columns.Add("GPSLongitude", "🌎 GPS Longitude");
                dataGridView1.Columns.Add("GPSAltitude", "📍 GPS Altitude");
                dataGridView1.Columns.Add("Pitch", "↕️ Pitch");
                dataGridView1.Columns.Add("Roll", "↔️ Roll");
                dataGridView1.Columns.Add("Yaw", "🔄 Yaw");
                dataGridView1.Columns.Add("RHRH", "🌈 RHRH");
                dataGridView1.Columns.Add("IoTS1", "🏠 IoT S1");
                dataGridView1.Columns.Add("IoTS2", "🏭 IoT S2");
                dataGridView1.Columns.Add("TakimNo", "🏷️ Takım No");

                // 📐 EXTREME KOMPAKT sütun genişlik ayarları (tüm sütunlar görünecek)
                dataGridView1.Columns["PaketNo"].Width = 40;           // Paket numarası
                dataGridView1.Columns["UyduStatusu"].Width = 55;        // KALKIŞ, APOGEE vs.
                dataGridView1.Columns["HataKodu"].Width = 35;           // 0-255 hata kodu
                dataGridView1.Columns["GondermeSaati"].Width = 50;      // HH:MM:SS
                dataGridView1.Columns["Basinc1"].Width = 45;            // Basınç değeri
                dataGridView1.Columns["Basinc2"].Width = 45;            // Basınç değeri
                dataGridView1.Columns["Yukseklik1"].Width = 45;         // Yükseklik metre
                dataGridView1.Columns["Yukseklik2"].Width = 45;         // Yükseklik metre
                dataGridView1.Columns["IrtifaFarki"].Width = 40;        // Fark değeri
                dataGridView1.Columns["InisHizi"].Width = 40;           // Hız m/s
                dataGridView1.Columns["Sicaklik"].Width = 40;           // Sıcaklık °C
                dataGridView1.Columns["PilGerilimi"].Width = 45;        // Gerilim V
                dataGridView1.Columns["GPSLatitude"].Width = 55;        // GPS koordinat
                dataGridView1.Columns["GPSLongitude"].Width = 55;       // GPS koordinat
                dataGridView1.Columns["GPSAltitude"].Width = 45;        // GPS yükseklik
                dataGridView1.Columns["Pitch"].Width = 35;              // Gyro derece
                dataGridView1.Columns["Roll"].Width = 35;               // Gyro derece
                dataGridView1.Columns["Yaw"].Width = 35;                // Gyro derece
                dataGridView1.Columns["RHRH"].Width = 35;               // R/H değeri
                dataGridView1.Columns["IoTS1"].Width = 40;              // IoT sıcaklık
                dataGridView1.Columns["IoTS2"].Width = 40;              // IoT sıcaklık
                dataGridView1.Columns["TakimNo"].Width = 45;            // TÜRKSAT takım

                // 🎨 DataGridView EXTREME KOMPAKT görsel iyileştirmeleri
                dataGridView1.ColumnHeadersDefaultCellStyle.BackColor = System.Drawing.Color.FromArgb(52, 58, 64);
                dataGridView1.ColumnHeadersDefaultCellStyle.ForeColor = System.Drawing.Color.White;
                dataGridView1.ColumnHeadersDefaultCellStyle.Font = new System.Drawing.Font("Arial", 7, System.Drawing.FontStyle.Bold); // Ultra küçük başlık
                dataGridView1.AlternatingRowsDefaultCellStyle.BackColor = System.Drawing.Color.FromArgb(248, 249, 250);
                dataGridView1.DefaultCellStyle.SelectionBackColor = System.Drawing.Color.LightBlue;
                dataGridView1.DefaultCellStyle.SelectionForeColor = System.Drawing.Color.Black;
                dataGridView1.DefaultCellStyle.Font = new System.Drawing.Font("Arial", 7); // Ultra küçük font
                dataGridView1.BorderStyle = BorderStyle.Fixed3D;
                dataGridView1.GridColor = System.Drawing.Color.LightGray;
                dataGridView1.BackgroundColor = System.Drawing.Color.White;

                // 📏 Extreme kompakt satır ve başlık ayarları (maksimum küçültme)
                dataGridView1.RowTemplate.Height = 16;                  // En küçük satır yüksekliği
                dataGridView1.ColumnHeadersHeight = 16;                 // En küçük başlık yüksekliği
                
                // 📜 Gelişmiş scroll özellikleri
                dataGridView1.ScrollBars = ScrollBars.Both;              // Hem yatay hem dikey scroll
                dataGridView1.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None; // Manuel genişlik
                dataGridView1.AllowUserToResizeColumns = true;          // Kullanıcı sütun genişliği ayarlayabilir
                dataGridView1.AllowUserToResizeRows = false;            // Satır yüksekliği sabit
                dataGridView1.RowHeadersVisible = false;                // Sol taraftaki satır başlıklarını gizle
                dataGridView1.MultiSelect = false;                      // Tek seçim modu
                dataGridView1.SelectionMode = DataGridViewSelectionMode.FullRowSelect; // Tam satır seçimi

                Log("📊 DataGridView extreme kompakt konfigürasyonu tamamlandı (22 sütun emoji ile)", false);
            }
            catch (Exception ex)
            {
                Log($"DataGridView konfigürasyonu yapılırken hata: {ex.Message}", true);
            }
        }

        /// <summary>
        /// DataGridView'e tek satır telemetri verisi ekler
        /// </summary>
        private void EkleDataGridViewSatiri(string[] paket, System.Drawing.Color arkaplanRengi)
        {
            try
            {
                if (paket.Length >= 22)
                {
                    int satir = dataGridView1.Rows.Add();
                    for (int i = 0; i < 22; i++)
                    {
                        dataGridView1.Rows[satir].Cells[i].Value = paket[i];
                    }
                    
                    // Özel renklendirme (ultra kompakt)
                    dataGridView1.Rows[satir].DefaultCellStyle.BackColor = arkaplanRengi;
                    dataGridView1.Rows[satir].DefaultCellStyle.Font = new System.Drawing.Font("Arial", 7, System.Drawing.FontStyle.Italic);
                }
            }
            catch (Exception ex)
            {
                Log($"DataGridView satırı eklenirken hata: {ex.Message}", false);
            }
        }

        #region Grafik İşlemleri

        // GrafikYoneticisi nesnesi
        private GrafikYoneticisi grafikYoneticisi;

        #endregion

        // UyduGorsellestime nesnesi - 3D görselleştirme için
        private UyduGorsellestime uyduGorsellestime;
        
        // HaritaYoneticisi nesnesi - Harita işlemleri için
        private HaritaYoneticisi haritaYoneticisi;
        
        // KameraYoneticisi nesnesi - Kamera ve video işlemleri için
        private KameraYoneticisi kameraYoneticisi;
        
        // SDKartYoneticisi kaldırıldı - artık direkt dosya kaydetme işlemi yapılacak

        // MultiSpektralFiltreYoneticisi nesnesi - Multi-spektral filtre işlemleri için
        private MultiSpektralFiltreYoneticisi multiSpektralFiltreYoneticisi;

        // AlarmSistemiYoneticisi nesnesi - ARAS (Arayüz Alarm Sistemi) için
        private AlarmSistemiYoneticisi alarmSistemiYoneticisi;
        
        // IoTYoneticisi nesnesi - IoT istasyonu iletişimi ve veri işlemleri için
        private IoTYoneticisi iotYoneticisi;

        // HataYoneticisi nesnesi - Hata yönetimi, loglama ve bildirimler için
        private HataYoneticisi hataYoneticisi;

        // ExcelVeriAktarmaYoneticisi nesnesi - Excel raporlama işlemleri için
        private ExcelVeriAktarmaYoneticisi excelVeriAktarmaYoneticisi;

        // BaglantiYoneticisi nesnesi - Seri port bağlantıları için
        private BaglantiYoneticisi baglantiYoneticisi;

        // TelemetriYoneticisi nesnesi - Telemetri verilerinin işlenmesi için
        private TelemetriYoneticisi telemetriYoneticisi;

        // Zamanlayıcı her tetiklendiğinde çalışan metod (telemetri verilerini işler)
        private void Zamanlayici_Tick(object sender, EventArgs e)
        {
            try
            {
                // 🔧 UI DONMA SORUNU ÇÖZÜMü - ReadLine() yerine ReadExisting() kullan
                if (OkumaNesnesi.BytesToRead > 0) 
                {
                    string sonuc = OkumaNesnesi.ReadExisting(); // Non-blocking okuma
                    
                    // Satır bazında işleme için split et
                    string[] satirlar = sonuc.Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
                    
                    foreach (string satir in satirlar)
                    {
                        if (!string.IsNullOrWhiteSpace(satir))
                        {
                            // Her satırı telemetri olarak işle
                            IsleVeGuncelleTelemetriVerisi(satir.Trim());

                            // 🔧 Veri alındığında zamanı güncelle
                            sonVeriZamani = DateTime.Now;
                        }
                    }
                }
                
                // 🔧 Veri timeout kontrolü
                TimeSpan gecenSure = DateTime.Now - sonVeriZamani;
                if (gecenSure.TotalSeconds > VERI_TIMEOUT_SANIYE)
                {
                    // Status bar veya title'da göster
                    this.Text = $"Model Uydu Yer İstasyonu - Bağlı (Veri Bekleniyor: {gecenSure.TotalSeconds:F0}s)";
                }
                else
                {
                    this.Text = "Model Uydu Yer İstasyonu - Bağlı (Veri Alınıyor)";
                }
                
                // Veri yoksa hiçbir şey yapmadan devam et (UI donmasını önler)
            }
            catch (Exception ex)
            {
                Console.WriteLine("Telemetri verileri işlenirken hata: " + ex.Message);
            }
        }

        // Telemetri verilerini SD karta kaydetme
        private void KaydetTelemetriVerileri(string telemetriVerisi)
        {
            try
            {
                // Ortak VeriKayitlari klasörüne telemetri verilerini kaydet
                string telemetriDizini = Path.Combine(Application.StartupPath, "VeriKayitlari", "telemetri");
                if (!Directory.Exists(telemetriDizini))
                {
                    Directory.CreateDirectory(telemetriDizini);
                }

                string dosyaAdi = Path.Combine(telemetriDizini, "telemetri_" + DateTime.Now.ToString("yyyy-MM-dd") + ".csv");

                // Dosya yoksa başlık satırını ekle
                if (!File.Exists(dosyaAdi))
                {
                    string csvBaslik = "Paket_Numarasi,Uydu_Statusu,Hata_Kodu,Gonderme_Saati," +
                                    "Basinc1,Basinc2,Yukseklik1,Yukseklik2,Irtifa_Farki," +
                                    "Inis_Hizi,Sicaklik,Pil_Gerilimi,GPS1_Latitude,GPS1_Longitude,GPS1_Altitude," +
                                    "Pitch,Roll,Yaw,RHRH,IoT_S1_Data,IoT_S2_Data,Takim_No";

                    File.AppendAllText(dosyaAdi, csvBaslik + Environment.NewLine);
                }

                // Telemetri verilerini dosyaya ekle
                File.AppendAllText(dosyaAdi, telemetriVerisi + Environment.NewLine);
            }
            catch (Exception ex)
            {
                Log("Telemetri verileri kaydedilirken hata: " + ex.Message, true);
            }
        }

        // Not: Video yakalama değişkenleri KameraYoneticisi sınıfına taşındı

        private void button4_Click(object sender, EventArgs e)
        {
            // Haritayı merkeze konumlandır
            haritaYoneticisi.MerkezKonumaGit();
        }

        private void textBox6_TextChanged(object sender, EventArgs e)
        {

        }

        private void button5_Click(object sender, EventArgs e)
        {
            // Haritayı uzaklaştır
            haritaYoneticisi.ZoomAzalt();
            zoom = haritaYoneticisi.ZoomSeviyesi; // Eski global değişkeni güncelle
        }

        private void Yakınlaştır_Click(object sender, EventArgs e)
        {
            // Haritayı yakınlaştır
            haritaYoneticisi.ZoomArtir();
            zoom = haritaYoneticisi.ZoomSeviyesi; // Eski global değişkeni güncelle
        }

        private void groupBox3_Enter(object sender, EventArgs e)
        {

        }

        private void groupBox2_Enter(object sender, EventArgs e)
        {

        }

        private void comboBox2_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void textBox8_TextChanged(object sender, EventArgs e)
        {
            // TextBox8 metin değişikliği işlemleri buraya eklenebilir
        }

        private void SerialPort1_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            try
            {
                SerialPort sp = (SerialPort)sender;
                string telemetriVerisi = sp.ReadLine();

                // Veriyi işle ve UI'yi güncelle - thread-safe bir şekilde
                this.BeginInvoke(new EventHandler(delegate
                {
                    IsleVeGuncelleTelemetriVerisi(telemetriVerisi);
                }));
            }
            catch (Exception ex)
            {
                Log($"Seri port veri okuma hatası: {ex.Message}", true);
            }
        }

        // Telemetri verisini işleyip UI'yi güncelleyen ortak metod
        private void IsleVeGuncelleTelemetriVerisi(string telemetriVerisi)
        {
            try
            {
                // TelemetriYoneticisi ile telemetri verisini işle
                telemetriYoneticisi.TelemetriVerisiniIsle(telemetriVerisi);
                
                // Zaman bilgisini güncelle
                DateTime myDateValue = DateTime.Now;
                label2.Text = myDateValue.ToString();
            }
            catch (Exception ex)
            {
                Log($"Telemetri verisi işlenirken hata: {ex.Message}", true);
            }
        }

        private async void manuelAyrilma_Click(object sender, EventArgs e)
        {
            // Manuel ayrılma komutunu gönder (async)
            if (await baglantiYoneticisi.KomutGonder("!xT!", true))
            {
            LogHata("Manuel ayrılma komutu gönderildi: !xT!", false);
                
                // 🔧 DÜZELTME: MultiSpektralFiltreYoneticisi'na ayrılma durumunu bildir
                if (multiSpektralFiltreYoneticisi != null)
                {
                    multiSpektralFiltreYoneticisi.GuncelleAyrilmaDurumu(true);
                    LogHata("Multi-spektral sistem aktifleştirildi - Komutlar gönderilebilir.", false);
                }
            }
            else
            {
                LogHata("Manuel ayrılma komutu gönderilemedi: Bağlantı yok!", true);
            }
        }

        // Video buton event handler'ları kaldırıldı - ŞARTNAME GEREĞİ: Kamera sistemi otomatik çalışır

        // VideoSourcePlayer_Click metodu KameraYoneticisi sınıfına taşındı

        private void button4_Click_1(object sender, EventArgs e)
        {
            // ❌ ESKİ KOD: Doğrudan COM1'e erişim (hata veriyordu)
            // serialPort1.Open(); // Seri portu aç
            // string veriler = serialPort1.ReadExisting(); // Mevcut verileri oku
            // MessageBox.Show("veriler gösteriliyor " + veriler); // Verileri mesaj kutusunda göster
            
            // ✅ YENİ KOD: Modern BaglantiYoneticisi ile entegre sistem
            try
            {
                if (baglantiYoneticisi != null)
                {
                    if (baglantiYoneticisi.BaglantiAcikMi)
                    {
                        // Bağlantı açıksa mevcut telemetri verilerini göster
                        StringBuilder veriler = new StringBuilder();
                        veriler.AppendLine("🛰️ TÜRKSAT Model Uydu - Mevcut Telemetri Verileri");
                        veriler.AppendLine("=" + new string('=', 50));
                        veriler.AppendLine();
                        
                        // TelemetriYoneticisi'nden son verileri al
                        if (telemetriYoneticisi != null && telemetriYoneticisi.SonTelemetriVerileri != null)
                        {
                            veriler.AppendLine($"📡 Bağlantı Durumu: AÇIK");
                            veriler.AppendLine($"📦 Toplam Paket Sayısı: {telemetriYoneticisi.ToplamPaketSayisi}");
                            veriler.AppendLine($"⏰ Son Güncelleme: {DateTime.Now:HH:mm:ss}");
                            veriler.AppendLine();
                            veriler.AppendLine("📋 Son Telemetri Paketi:");
                            veriler.AppendLine(telemetriYoneticisi.SonTelemetriFormatli);
                        }
                        else
                        {
                            veriler.AppendLine("⚠️ Henüz telemetri verisi alınmadı.");
                        }
                        
                        MessageBox.Show(veriler.ToString(), "Telemetri Verileri", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    else
                    {
                        Log("❌ Bağlantı kapalı!\n\nÖnce 'Test Verisi' seçerek bağlantıyı açın.", true);
                    }
                }
                else
                {
                    Log("⚠️ Bağlantı yöneticisi başlatılmamış!", true);
                }
            }
            catch (Exception ex)
            {
                Log($"Veri gösterme sırasında hata: {ex.Message}", true);
            }
        }

        private void gMapControl1_Load(object sender, EventArgs e)
        {
            try
            {
                // haritaYoneticisi null kontrolü ekle
                if (haritaYoneticisi != null)
                {
                    // HaritaYoneticisi tarafından yönetiliyor
                    haritaYoneticisi.InitializeMap(lat, longt, zoom);
                }
                else
                {
                    // Eğer haritaYoneticisi henüz oluşturulmadıysa basit bir başlangıç yapılandırması yap
                    GMapControl mapControl = (GMapControl)sender;
                    
                    // Google harita sağlayıcısını kullan
                    mapControl.MapProvider = GMapProviders.GoogleMap;
                    GMaps.Instance.Mode = AccessMode.ServerAndCache;
                    
                    // Başlangıç konumunu ve zoom seviyesini ayarla
                    mapControl.Position = new PointLatLng(lat, longt);
                    mapControl.MinZoom = 1;
                    mapControl.MaxZoom = 120;
                    mapControl.Zoom = zoom;
                    
                    // Harita davranış ayarlarını yap
                    mapControl.DragButton = MouseButtons.Left;
                    mapControl.MarkersEnabled = true;
                    mapControl.PolygonsEnabled = true;
                    mapControl.RoutesEnabled = true;
                    mapControl.MouseWheelZoomEnabled = true;
                    
                    Log("Harita doğrudan yapılandırıldı, HaritaYoneticisi henüz başlatılmamış.", false);
                }
            }
            catch (Exception ex)
            {
                Log("Harita yüklenirken hata: " + ex.Message, true);
            }
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            // Uygulama kapatılırken tüm kaynakları serbest bırak
            try
            {
                baglantiYoneticisi?.Dispose();
                kameraYoneticisi?.Dispose();
                // sdKartYoneticisi kaldırıldı
                uyduGorsellestime?.Dispose();
                hataYoneticisi?.Dispose();
                
                // Zamanlayıcıları durdur ve serbest bırak
                if (Zamanlayici != null)
                {
                    Zamanlayici.Stop();
                    Zamanlayici.Dispose();
                }
                
                if (sdKartTimer != null)
                {
                    sdKartTimer.Stop();
                    sdKartTimer.Dispose();
                }

                // Excel nesnelerini serbest bırak
                excelVeriAktarmaYoneticisi?.Dispose();

                // Diğer IDisposable nesneler varsa burada dispose edilmeli
            }
            catch (Exception ex)
            {
                // Kapatma sırasında oluşabilecek hataları logla
                MessageBox.Show($"Uygulama kapatılırken bir hata oluştu: {ex.Message}", "Kapatma Hatası", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async void button1_Click(object sender, EventArgs e)
        {
            try 
            {
                // 🔧 UI DONMA SORUNU ÇÖZÜMü - Kullanıcıya feedback ver
                baglan.Enabled = false;  // Button'ı disable et
                baglan.Text = "Bağlanıyor...";
                
                // BaglantiYoneticisi üzerinden bağlantıyı aç (async)
                bool sonuc = await baglantiYoneticisi.BaglantiyiAc();
                
                if (sonuc)
            {
                // Bağlantı başarılı ise zamanlayıcıyı başlat
                Zamanlayici.Start();
                    baglan.Text = "Bağlandı";
                    kes.Enabled = true;
                }
                else 
                {
                    baglan.Text = "Bağlan";
                    baglan.Enabled = true;
                }
            }
            catch (Exception ex)
            {
                // Hata durumunda UI'ı restore et
                baglan.Text = "Bağlan";
                baglan.Enabled = true;
                
                // 🔧 Port erişim hatası için özel mesaj
                if (ex.Message.Contains("erişim reddedildi") || ex.Message.Contains("access") || ex.Message.Contains("COM"))
                {
                    MessageBox.Show(
                        $"❌ PORT ERİŞİM HATASI!\n\n" +
                        $"COM port başka program tarafından kullanılıyor.\n\n" +
                        $"💡 ÇÖZÜM:\n" +
                        $"• XCTU programını kapatın\n" +
                        $"• Terminal/Serial monitor programlarını kapatın\n" +
                        $"• 5-10 saniye bekleyip tekrar deneyin\n\n" +
                        $"📍 Port: {comboBox1.Text}\n" +
                        $"🔍 Detay: {ex.Message}", 
                        "Port Erişim Hatası", 
                        MessageBoxButtons.OK, 
                        MessageBoxIcon.Warning);
                }
                else
                {
                    MessageBox.Show($"Bağlantı hatası: {ex.Message}", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void kes_Click(object sender, EventArgs e)
        {
            try 
        {
            // BaglantiYoneticisi üzerinden bağlantıyı kapat
            if (baglantiYoneticisi.BaglantiyiKapat())
            {
                // Bağlantı kapandı ise zamanlayıcıyı durdur
                Zamanlayici.Stop();
                }
                
                // 🔧 UI durumunu restore et
                baglan.Text = "Bağlan";
                baglan.Enabled = true;
                kes.Enabled = false;
                
                // 🔧 Window title'ı güncelle
                this.Text = "Model Uydu Yer İstasyonu - Bağlantı Kesildi";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Bağlantı kesme hatası: {ex.Message}", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void ExlAktar_Click(object sender, EventArgs e)
        {
            try
            {
                // ExcelVeriAktarmaYoneticisi kullanarak Excel'e veri aktar
                bool basarili = excelVeriAktarmaYoneticisi.DataGridViewVerileriniAktar(
                    dataGridView1, 
                    "Model Uydu Telemetri Verileri"
                );

                if (!basarili)
                {
                    MessageBox.Show("Excel'e veri aktarımı sırasında bir hata oluştu.", 
                        "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                Log("Excel'e aktarma sırasında hata: " + ex.Message, true);
            }
        }

        private void displayData_event(object sender, EventArgs e)
        {
            // Veri gösterme işlemleri buraya eklenebilir
        }

        private async void buttonMultiSpektralGonder_Click(object sender, EventArgs e)
        {
            if (multiSpektralFiltreYoneticisi != null)
            {
                // 🔧 DÜZELTME: TextBox'tan komutu oku ve KomutGonder'e parametre olarak geç
                string kullaniciKomutu = textBoxMultiSpektralKomut.Text?.Trim().ToUpper();
                
                if (string.IsNullOrEmpty(kullaniciKomutu))
                {
                    Log("❌ Lütfen bir multi-spektral komut girin (örn: 6R7G)", true);
                    return;
                }
                
                // Komut gönder ve sonucu kontrol et (async)
                bool basarili = await multiSpektralFiltreYoneticisi.KomutGonder(kullaniciKomutu);

                // Hata mesajı gösterme sorumluluğu artık MultiSpektralFiltreYoneticisi'nde.
                // Bu yüzden buradaki MessageBox kaldırıldı.
            }
            else
            {
                Log("MultiSpektralFiltreYoneticisi başlatılmamış.", true);
            }
        }

        private void textBoxMultiSpektralKomut_TextChanged(object sender, EventArgs e)
        {
            // 🔧 DÜZELTME: TextBox değişikliğini MultiSpektralFiltreYoneticisi'ne bildir
            if (multiSpektralFiltreYoneticisi != null)
            {
                string kullaniciKomutu = textBoxMultiSpektralKomut.Text?.Trim().ToUpper();
                // MultiSpektralFiltreYoneticisi'ne komut güncelleme metodu eklenecek
                multiSpektralFiltreYoneticisi.UpdateKomut(kullaniciKomutu);
            }
        }

        private void textBox14_TextChanged(object sender, EventArgs e)
        {
            // TextBox14 metin değişikliği işlemleri buraya eklenebilir
        }

        private void panelHata1_Paint(object sender, PaintEventArgs e)
        {
            // Panel1 çizim işlemleri buraya eklenebilir
        }

        // Multi-spektral komut formatı doğrulama metodu artık MultiSpektralFiltreYoneticisi sınıfına taşındı

        // Komut loglama metodu artık MultiSpektralFiltreYoneticisi sınıfına taşındı

        // ARAS (Arayüz Alarm Sistemi) fonksiyonları artık AlarmSistemiYoneticisi sınıfına taşındı

        // Not: IoT S2S veri transferi için başlatma işlemleri artık IoTYoneticisi sınıfında yapılıyor

        // Not: IoT istasyonlarından veri alma işlemleri artık IoTYoneticisi sınıfı tarafından yönetiliyor

        // IoT verilerini Excel'e aktarmak için yeni method
        private void buttonIotExcelAktar_Click(object sender, EventArgs e)
        {
            try
            {
                if (iotYoneticisi != null && iotYoneticisi.IotVerileriListesi != null && iotYoneticisi.IotVerileriListesi.Count > 0)
                {
                    // IoT verilerini ExcelVeriAktarmaYoneticisi.IoTVeri sınıfına dönüştür
                    List<ExcelVeriAktarmaYoneticisi.IoTVeri> excelIoTVerileri = new List<ExcelVeriAktarmaYoneticisi.IoTVeri>();
                    
                    foreach (var veri in iotYoneticisi.IotVerileriListesi)
                    {
                        excelIoTVerileri.Add(new ExcelVeriAktarmaYoneticisi.IoTVeri(
                            veri.Zaman,
                            veri.S1Sicaklik,
                            veri.S2Sicaklik,
                            veri.BataryaDurumu,
                            veri.BaglantiDurumu
                        ));
                    }
                    
                    // Excel'e aktar
                    bool basarili = excelVeriAktarmaYoneticisi.IoTVerileriniAktar(excelIoTVerileri);
                    
                    if (!basarili)
                    {
                        MessageBox.Show("IoT verilerinin Excel'e aktarımı sırasında bir hata oluştu.", 
                            "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
                else
                {
                    MessageBox.Show("Aktarılacak IoT verisi bulunamadı.", 
                        "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            catch (Exception ex)
            {
                Log("IoT verilerini Excel'e aktarma sırasında hata: " + ex.Message, true);
            }
        }

        // Hata loglarını Excel'e aktarmak için yeni method
        private void buttonHataLogExcelAktar_Click(object sender, EventArgs e)
        {
            try
            {
                // Hata log dosyasının yolu
                string logDosyaYolu = "hata_log.txt";
                
                // Excel'e aktar
                bool basarili = excelVeriAktarmaYoneticisi.HataLoglariniAktar(logDosyaYolu);
                
                if (!basarili)
                {
                    MessageBox.Show("Hata loglarının Excel'e aktarımı sırasında bir sorun oluştu.", 
                        "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            catch (Exception ex)
            {
                Log("Hata loglarını Excel'e aktarma sırasında hata: " + ex.Message, true);
            }
        }

        // IoT verilerini görüntüleme butonu için olay işleyicisi
        private void buttonIotGoruntule_Click(object sender, EventArgs e)
        {
            // IoTYoneticisi üzerinden IoT verilerini görüntüle
            if (iotYoneticisi != null)
            {
                iotYoneticisi.GoruntuleIoTVerileri();
            }
        }

        // LogHata metodunu HataYoneticisi.Log metoduna yönlendirme
        private void Log(string hataMesaji, bool kritikMi = false)
        {
            if (hataYoneticisi != null)
            {
                hataYoneticisi.Log(hataMesaji, kritikMi);
            }
            else
            {
                // HataYoneticisi henüz oluşturulmamışsa konsola yaz
                Console.WriteLine($"HATA: {hataMesaji} (HataYoneticisi henüz başlatılmadı)");
            }
        }

        // SD kart işlemleri artık SDKartYoneticisi sınıfı tarafından yönetiliyor

        // Not: IoT verilerini SD karta kaydetme işlemi artık IoTYoneticisi sınıfında yapılıyor

        private void progressBarSDKart_Click(object sender, EventArgs e)
        {

        }

        private void textBox10_TextChanged(object sender, EventArgs e)
        {

        }

        private void textBox11_TextChanged(object sender, EventArgs e)
        {

        }

        // ConfigureChart2 metodu GrafikYoneticisi sınıfına taşındı
        // ConfigureChart3 metodu GrafikYoneticisi sınıfına taşındı

        // ConfigureChart4 metodu GrafikYoneticisi sınıfına taşındı

        // Komut açıklaması oluşturan fonksiyon artık MultiSpektralFiltreYoneticisi sınıfına taşındı

        // Filtre türü açıklaması fonksiyonu artık MultiSpektralFiltreYoneticisi sınıfına taşındı

        // Telemetri verisi alındığında çalışacak metod
        private void TelemetriVerisiAlindi(object sender, string telemetriVerisi)
        {
            // UI thread kontrolü
            this.BeginInvoke(new EventHandler(delegate
            {
                IsleVeGuncelleTelemetriVerisi(telemetriVerisi);
            }));
        }

        /// <summary>
        /// 🔧 YENİ: Binary video verisi alındığında çalışacak metod
        /// DEADBEEF + JPEG + CAFEBABE formatındaki video frame'leri işler
        /// </summary>
        private void BinaryVideoVerisiAlindi(object sender, byte[] binaryVeri)
        {
            try
            {
                // UI thread kontrolü
                this.BeginInvoke(new Action(() =>
                {
                    // TelemetriYoneticisi'ne binary veri gönder
                    if (telemetriYoneticisi != null)
                    {
                        telemetriYoneticisi.BinaryVideoVerisiniIsle(binaryVeri);
                    }
                    else
                    {
                        Log("TelemetriYoneticisi bulunamadı, binary video verisi işlenemedi", true);
                    }
                }));
            }
            catch (Exception ex)
            {
                Log($"Binary video verisi işlenirken hata: {ex.Message}", true);
            }
        }

        private void chart3_Click(object sender, EventArgs e)
        {

        }

        // Sesli uyarı durumunu güncelleyen olay işleyicisi
        private void checkBoxSesliUyari_CheckedChanged(object sender, EventArgs e)
        {
            // AlarmSistemiYoneticisi üzerinden sesli uyarı durumunu güncelle
            if (alarmSistemiYoneticisi != null)
            {
                bool yeniDurum = checkBoxSesliUyari.Checked;
                alarmSistemiYoneticisi.SesliUyariDurumunuGuncelle(yeniDurum);
                
                if (yeniDurum)
                {
                    Log("Sesli uyarı sistemi aktifleştirildi", false);
                }
                else
                {
                    Log("Sesli uyarı sistemi devre dışı bırakıldı", false);
                }
            }
        }

        private void chartYukseklik_Click(object sender, EventArgs e)
        {

        }

        // Log dosya yolları ve hatalar
        private string logDosyaYolu = "komut_log.txt"; // Komut logları için dosya yolu

        // 🔧 XBee veri timeout tracking için değişkenler
        private DateTime sonVeriZamani = DateTime.Now;
        private const int VERI_TIMEOUT_SANIYE = 10; // 10 saniye veri gelmezse timeout

        // 🔧 KALİBRASYON SİSTEMİ EVENT HANDLER'LARI
        
        /// <summary>
        /// Şehir seçimi değiştiğinde çalışan olay handler'ı - KalibrasyonYoneticisi tarafından yönetiliyor
        /// </summary>
        private void comboBoxSehir_SelectedIndexChanged(object sender, EventArgs e)
        {
            // Bu event artık KalibrasyonYoneticisi tarafından yönetiliyor
        }

        /// <summary>
        /// Basınç kalibrasyon butonu - KalibrasyonYoneticisi tarafından yönetiliyor
        /// </summary>
        private void buttonBasincKalibre_Click(object sender, EventArgs e)
        {
            // Bu event artık KalibrasyonYoneticisi tarafından yönetiliyor
        }

        /// <summary>
        /// GPS referans butonu - KalibrasyonYoneticisi tarafından yönetiliyor
        /// </summary>
        private void buttonGPSReferans_Click(object sender, EventArgs e)
        {
            // Bu event artık KalibrasyonYoneticisi tarafından yönetiliyor
        }

        /// <summary>
        /// Gyro sıfırlama butonu - KalibrasyonYoneticisi tarafından yönetiliyor
        /// </summary>
        private void buttonGyroSifirla_Click(object sender, EventArgs e)
        {
            // Bu event artık KalibrasyonYoneticisi tarafından yönetiliyor
        }

        // 🔧 KALİBRASYON YARDIMCI METODLARI - KalibrasyonYoneticisi sınıfına taşındı

        // Kalibrasyon yöneticisi referansı
        private KalibrasyonYoneticisi kalibrasyonYoneticisi;

        /// <summary>
        /// Hata mesajını loglar ve UI'da gösterir
        /// </summary>
        private void LogHata(string hataMesaji, bool kritikMi = false)
        {
            Log(hataMesaji, kritikMi); // Log metodunu çağır
        }

        // Uydu durumu güncelleme metodu
        private void UyduDurumuGuncelle(string durum)
        {
            // Uydu durumunu güncelle
            if (labelUyduStatu.InvokeRequired)
            {
                labelUyduStatu.Invoke(new Action(() => {
                    labelUyduStatu.Text = durum;
                }));
            }
            else
            {
                labelUyduStatu.Text = durum;
            }
        }
    }
}
