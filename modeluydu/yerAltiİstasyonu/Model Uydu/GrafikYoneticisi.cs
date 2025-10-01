using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing; 
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using System.IO;

namespace ModelUydu
{
    public class GrafikYoneticisi
    {
        // Grafik kontrolleri
        private Chart chartSicaklik;
        private Chart chartYukseklik;
        private Chart chartBasinc;
        private Chart chartHiz;
        private Chart chartPilGerilimi;
        private Chart chartIoT;
        // Chart2 field kaldırıldı - gizlenen chart'lar için null kontrolü eklendi
        // private Chart chart2;

        // IoT değişkenleri
        public double IotS1Sicaklik { get; set; } = 0.0;
        public double IotS2Sicaklik { get; set; } = 0.0;
        public bool IotVerisiAlindiMi { get; set; } = false;
        private string iotLogDosyaYolu = "iot_verileri.txt";

        // Hız analizi değişkenleri
        private List<double> hizGecmisi = new List<double>();
        // Kullanılmayan field'lar kaldırıldı (CS0414 uyarısı giderildi)

        // Yükseklik analizi değişkenleri
        private List<double> yukseklikGecmisi = new List<double>();
        private double toplamYukseklik = 0.0;
        private int yukseklikSayaci = 0;
        private double oncekiYukseklik = 0.0;

        // IoT analizi değişkenleri
        private List<double> iotS1Gecmisi = new List<double>();
        private List<double> iotS2Gecmisi = new List<double>();
        // Kullanılmayan field'lar kaldırıldı (CS0414 uyarısı giderildi)

        // Pil gerilimi analizi değişkenleri
        private List<double> pilGerilimiGecmisi = new List<double>();
        private double maxPilGerilimi = double.MinValue;
        private double minPilGerilimi = double.MaxValue;
        private double oncekiPilGerilimi = 0.0;

        // Log metodu için delegate
        private Action<string, bool> logHataMetodu;

        /// <summary>
        /// GrafikYoneticisi sınıfı yapıcı metodu
        /// </summary>
        public GrafikYoneticisi(Chart sicaklik, Chart yukseklik, Chart basinc, Chart hiz, Chart pilGerilimi, 
            Chart iot, Action<string, bool> logHataMetodu)
        {
            this.chartSicaklik = sicaklik;
            this.chartYukseklik = yukseklik;
            this.chartBasinc = basinc;
            this.chartHiz = hiz;
            this.chartPilGerilimi = pilGerilimi;
            this.chartIoT = iot;
            this.logHataMetodu = logHataMetodu;
        }

        /// <summary>
        /// Tüm grafikleri yapılandıran metod
        /// </summary>
        public void ConfigureAllCharts()
        {
            try
            {
                // Tüm grafikleri yapılandır (chart2 hariç - gizli durumda)
                Chart[] charts =
                {
                    chartSicaklik,
                    chartYukseklik,
                    chartBasinc,
                    chartHiz,
                    chartPilGerilimi,
                    chartIoT
                    // chart2 gizli olduğu için array'den çıkarıldı
                    // chart3 ve chart4 ayrıca yapılandırılacak
                };

                foreach (Chart chart in charts)
                {
                    // Null kontrolü ekleyelim
                    if (chart != null)
                    {
                        // ChartArea yok ise ekleyelim
                        if (chart.ChartAreas.Count == 0)
                        {
                            chart.ChartAreas.Add(new ChartArea());
                        }

                        // Temel ayarlar
                        chart.ChartAreas[0].AxisX.MajorGrid.LineColor = Color.LightGray;
                        chart.ChartAreas[0].AxisY.MajorGrid.LineColor = Color.LightGray;
                        chart.ChartAreas[0].BackColor = Color.WhiteSmoke;
                        chart.BackColor = Color.White;
                        chart.BorderlineColor = Color.LightGray;
                        chart.BorderlineDashStyle = ChartDashStyle.Solid;
                        chart.BorderlineWidth = 1;
                        chart.Palette = ChartColorPalette.BrightPastel;
                    }
                }

                // Özel grafik yapılandırmaları
                ConfigureChartSicaklik();
                ConfigureChartYukseklik();
                ConfigureChartBasinc();
                ConfigureChartHiz();
                ConfigureChartPilGerilimi();
                ConfigureChartIoT();
                // ConfigureChart2(); // DEVRE DIŞI: Chart2 gizli durumda (index hatası önlenir)
                // ConfigureChart3 ve ConfigureChart4 kaldırıldı - gereksiz tekrar
            }
            catch (Exception ex)
            {
                LogHata("Grafikler yapılandırılamadı: " + ex.Message);
                Console.WriteLine("Hata - ConfigureAllCharts: " + ex.Message);
            }
        }

        /// <summary>
        /// Sıcaklık grafiğini yapılandıran metod
        /// </summary>
        private void ConfigureChartSicaklik()
        {
            try
            {
                var chart = chartSicaklik;

                // ChartArea ayarları
                chart.ChartAreas[0].AxisX.Minimum = 0;
                chart.ChartAreas[0].AxisX.Maximum = 100;
                chart.ChartAreas[0].AxisX.Interval = 10;
                chart.ChartAreas[0].AxisX.MajorGrid.LineColor = Color.LightGray;

                // Otomatik ölçeklendirme etkinleştir
                chart.ChartAreas[0].AxisY.Minimum = Double.NaN;
                chart.ChartAreas[0].AxisY.Maximum = Double.NaN;
                chart.ChartAreas[0].AxisY.IsStartedFromZero = false;
                chart.ChartAreas[0].AxisY.MajorGrid.LineColor = Color.LightGray;
                chart.ChartAreas[0].AxisY.LabelStyle.Format = "0.#";
                chart.ChartAreas[0].RecalculateAxesScale();

                // Seri ayarları
                if (chart.Series.Count == 0)
                {
                    chart.Series.Add(new Series("Sıcaklık(°C)"));
                }

                chart.Series[0].ChartType = SeriesChartType.Line;
                chart.Series[0].Color = Color.Red;
                chart.Series[0].BorderWidth = 2;
                chart.Series[0].MarkerStyle = MarkerStyle.Circle;
                chart.Series[0].MarkerSize = 5;

                // Başlık
                if (chart.Titles.Count == 0)
                {
                    chart.Titles.Add(new Title("Sıcaklık(°C)", Docking.Top));
                }
                else
                {
                    chart.Titles[0].Text = "Sıcaklık(°C)";
                }
                
                // Scrollbar görünürlüğü aktif et
                chart.ChartAreas[0].AxisX.ScrollBar.IsPositionedInside = true;
                chart.ChartAreas[0].AxisX.ScrollBar.Enabled = true;
                chart.ChartAreas[0].AxisX.ScaleView.Zoomable = true;
                chart.ChartAreas[0].CursorX.AutoScroll = true;
            }
            catch (Exception ex)
            {
                LogHata("Sıcaklık grafiği yapılandırılamadı: " + ex.Message);
                Console.WriteLine("Hata - ConfigureChartSicaklik: " + ex.Message);
            }
        }

        /// <summary>
        /// Yükseklik grafiğini akıllı irtifa sistemi ile yapılandıran metod
        /// </summary>
        private void ConfigureChartYukseklik()
        {
            try
            {
                var chart = chartYukseklik;

                // ChartArea ayarları
                chart.ChartAreas[0].AxisX.Minimum = 0;
                chart.ChartAreas[0].AxisX.Maximum = 100;
                chart.ChartAreas[0].AxisX.Interval = 10;
                chart.ChartAreas[0].AxisX.MajorGrid.LineColor = Color.LightGray;
                // X ekseni başlığı kaldırıldı - temiz görünüm için

                // Y ekseni dinamik ölçeklendirme
                chart.ChartAreas[0].AxisY.Minimum = 0;
                chart.ChartAreas[0].AxisY.Maximum = 1200;
                chart.ChartAreas[0].AxisY.Interval = 100;
                chart.ChartAreas[0].AxisY.MajorGrid.LineColor = Color.LightGray;
                // Y ekseni başlığı kaldırıldı - temiz görünüm için
                chart.ChartAreas[0].AxisY.Title = "";

                // Kritik irtifa uyarı çizgileri (Strip Lines)
                chart.ChartAreas[0].AxisY.StripLines.Clear();
                
                // İniş Alanı (0-100m) - Yeşil arka plan
                StripLine inisAlani = new StripLine();
                inisAlani.IntervalOffset = 0;
                inisAlani.StripWidth = 100;
                inisAlani.BackColor = Color.FromArgb(50, Color.Green);
                inisAlani.Text = "İniş Alanı";
                inisAlani.ForeColor = Color.DarkGreen;
                chart.ChartAreas[0].AxisY.StripLines.Add(inisAlani);

                // Düşük İrtifa (100-300m) - Mavi arka plan
                StripLine dusukIrtifa = new StripLine();
                dusukIrtifa.IntervalOffset = 100;
                dusukIrtifa.StripWidth = 200;
                dusukIrtifa.BackColor = Color.FromArgb(50, Color.Blue);
                dusukIrtifa.Text = "Düşük İrtifa";
                dusukIrtifa.ForeColor = Color.DarkBlue;
                chart.ChartAreas[0].AxisY.StripLines.Add(dusukIrtifa);

                // Orta İrtifa (300-600m) - Sarı arka plan
                StripLine ortaIrtifa = new StripLine();
                ortaIrtifa.IntervalOffset = 300;
                ortaIrtifa.StripWidth = 300;
                ortaIrtifa.BackColor = Color.FromArgb(50, Color.Yellow);
                ortaIrtifa.Text = "Orta İrtifa";
                ortaIrtifa.ForeColor = Color.DarkGoldenrod;
                chart.ChartAreas[0].AxisY.StripLines.Add(ortaIrtifa);

                // Yüksek İrtifa (600m+) - Turuncu arka plan
                StripLine yuksekIrtifa = new StripLine();
                yuksekIrtifa.IntervalOffset = 600;
                yuksekIrtifa.StripWidth = 600;
                yuksekIrtifa.BackColor = Color.FromArgb(50, Color.Orange);
                yuksekIrtifa.Text = "Yüksek İrtifa";
                yuksekIrtifa.ForeColor = Color.DarkOrange;
                chart.ChartAreas[0].AxisY.StripLines.Add(yuksekIrtifa);

                // Kritik çizgiler
                StripLine kritikCizgi1 = new StripLine();
                kritikCizgi1.IntervalOffset = 100;
                kritikCizgi1.StripWidth = 0;
                kritikCizgi1.BorderColor = Color.Green;
                kritikCizgi1.BorderWidth = 2;
                kritikCizgi1.BorderDashStyle = ChartDashStyle.Dash;
                chart.ChartAreas[0].AxisY.StripLines.Add(kritikCizgi1);

                StripLine kritikCizgi2 = new StripLine();
                kritikCizgi2.IntervalOffset = 300;
                kritikCizgi2.StripWidth = 0;
                kritikCizgi2.BorderColor = Color.Blue;
                kritikCizgi2.BorderWidth = 2;
                kritikCizgi2.BorderDashStyle = ChartDashStyle.Dash;
                chart.ChartAreas[0].AxisY.StripLines.Add(kritikCizgi2);

                StripLine kritikCizgi3 = new StripLine();
                kritikCizgi3.IntervalOffset = 600;
                kritikCizgi3.StripWidth = 0;
                kritikCizgi3.BorderColor = Color.Orange;
                kritikCizgi3.BorderWidth = 2;
                kritikCizgi3.BorderDashStyle = ChartDashStyle.Dash;
                chart.ChartAreas[0].AxisY.StripLines.Add(kritikCizgi3);

                StripLine kritikCizgi4 = new StripLine();
                kritikCizgi4.IntervalOffset = 1000;
                kritikCizgi4.StripWidth = 0;
                kritikCizgi4.BorderColor = Color.Red;
                kritikCizgi4.BorderWidth = 3;
                kritikCizgi4.BorderDashStyle = ChartDashStyle.Solid;
                chart.ChartAreas[0].AxisY.StripLines.Add(kritikCizgi4);

                // Serileri temizle ve yeniden oluştur
                chart.Series.Clear();

                // Ana yükseklik serisi
                Series yukseklikSeries = new Series("Yükseklik(m)");
                yukseklikSeries.ChartType = SeriesChartType.Line;
                yukseklikSeries.Color = Color.Blue;
                yukseklikSeries.BorderWidth = 3;
                yukseklikSeries.MarkerStyle = MarkerStyle.Circle;
                yukseklikSeries.MarkerSize = 6;
                chart.Series.Add(yukseklikSeries);

                // Legend kapatıldı - temiz görünüm için
                if (chart.Legends.Count > 0)
                {
                    chart.Legends[0].Enabled = false;
                }

                // Başlık
                if (chart.Titles.Count == 0)
                {
                    chart.Titles.Add(new Title("İrtifa Analizi (m)", Docking.Top));
                }
                else
                {
                    chart.Titles[0].Text = "İrtifa Analizi (m)";
                    chart.Titles[0].Font = new Font("Arial", 10, FontStyle.Bold);
                }
                
                // Zoom ve scroll ayarları
                chart.ChartAreas[0].AxisX.ScrollBar.IsPositionedInside = true;
                chart.ChartAreas[0].AxisX.ScrollBar.Enabled = true;
                chart.ChartAreas[0].AxisX.ScaleView.Zoomable = true;
                chart.ChartAreas[0].CursorX.AutoScroll = true;
            }
            catch (Exception ex)
            {
                LogHata("Yükseklik grafiği yapılandırılamadı: " + ex.Message);
                Console.WriteLine("Hata - ConfigureChartYukseklik: " + ex.Message);
            }
        }

        /// <summary>
        /// Basınç grafiğini yapılandıran metod
        /// </summary>
        private void ConfigureChartBasinc()
        {
            try
            {
                var chart = chartBasinc;

                // ChartArea ayarları
                chart.ChartAreas[0].AxisX.Minimum = 0;
                chart.ChartAreas[0].AxisX.Maximum = 100;
                chart.ChartAreas[0].AxisX.Interval = 10;
                chart.ChartAreas[0].AxisX.MajorGrid.LineColor = Color.LightGray;

                chart.ChartAreas[0].AxisY.Minimum = 85000;  // 850 hPa
                chart.ChartAreas[0].AxisY.Maximum = 105000; // 1050 hPa
                chart.ChartAreas[0].AxisY.Interval = 2500;  // 25 hPa aralıklarla
                chart.ChartAreas[0].AxisY.MajorGrid.LineColor = Color.LightGray;
                chart.ChartAreas[0].AxisY.Title = "Basınç (Pa)";
                chart.ChartAreas[0].AxisY.LabelStyle.Format = "N0"; // Binlik ayırıcı ekle

                // Seri ayarları
                if (chart.Series.Count == 0)
                {
                    chart.Series.Add(new Series("Basınç(Pa)"));
                }

                chart.Series[0].ChartType = SeriesChartType.Line;
                chart.Series[0].Color = Color.Green;
                chart.Series[0].BorderWidth = 2;

                // Başlık
                if (chart.Titles.Count == 0)
                {
                    chart.Titles.Add(new Title("Basınç(Pa)", Docking.Top));
                }
                else
                {
                    chart.Titles[0].Text = "Basınç(Pa)";
                }
            }
            catch (Exception ex)
            {
                LogHata("Basınç grafiği yapılandırılamadı: " + ex.Message);
                Console.WriteLine("Hata - ConfigureChartBasinc: " + ex.Message);
            }
        }

        /// <summary>
        /// Hız grafiğini akıllı renklendirme sistemi ile yapılandıran metod
        /// </summary>
        private void ConfigureChartHiz()
        {
            try
            {
                var chart = chartHiz;

                // ChartArea ayarları
                chart.ChartAreas[0].AxisX.Minimum = 0;
                chart.ChartAreas[0].AxisX.Maximum = 100;
                chart.ChartAreas[0].AxisX.Interval = 10;
                chart.ChartAreas[0].AxisX.MajorGrid.LineColor = Color.LightGray;
                // X ekseni başlığı kaldırıldı - temiz görünüm için

                // Y ekseni otomatik ölçeklendirme
                chart.ChartAreas[0].AxisY.Minimum = 0;
                chart.ChartAreas[0].AxisY.Maximum = 25;
                chart.ChartAreas[0].AxisY.Interval = 5;
                chart.ChartAreas[0].AxisY.MajorGrid.LineColor = Color.LightGray;
                // Y ekseni başlığı kaldırıldı - temiz görünüm için
                chart.ChartAreas[0].AxisY.Title = "";

                // Kritik hız uyarı çizgileri (Strip Lines)
                chart.ChartAreas[0].AxisY.StripLines.Clear();
                
                // Güvenli İniş Alanı (0-5 m/s) - Yeşil arka plan
                StripLine guvenliAlan = new StripLine();
                guvenliAlan.IntervalOffset = 0;
                guvenliAlan.StripWidth = 5;
                guvenliAlan.BackColor = Color.FromArgb(50, Color.Green);
                guvenliAlan.Text = "Güvenli İniş";
                guvenliAlan.ForeColor = Color.DarkGreen;
                chart.ChartAreas[0].AxisY.StripLines.Add(guvenliAlan);

                // Normal Hız Alanı (5-15 m/s) - Sarı arka plan
                StripLine normalAlan = new StripLine();
                normalAlan.IntervalOffset = 5;
                normalAlan.StripWidth = 10;
                normalAlan.BackColor = Color.FromArgb(50, Color.Yellow);
                normalAlan.Text = "Normal";
                normalAlan.ForeColor = Color.DarkGoldenrod;
                chart.ChartAreas[0].AxisY.StripLines.Add(normalAlan);

                // Dikkat Alanı (15-25 m/s) - Turuncu arka plan
                StripLine dikkatAlan = new StripLine();
                dikkatAlan.IntervalOffset = 15;
                dikkatAlan.StripWidth = 10;
                dikkatAlan.BackColor = Color.FromArgb(50, Color.Orange);
                dikkatAlan.Text = "Dikkat";
                dikkatAlan.ForeColor = Color.DarkOrange;
                chart.ChartAreas[0].AxisY.StripLines.Add(dikkatAlan);

                // Tehlike Alanı (25+ m/s) - Kırmızı arka plan
                StripLine tehlikeAlan = new StripLine();
                tehlikeAlan.IntervalOffset = 25;
                tehlikeAlan.StripWidth = 15;
                tehlikeAlan.BackColor = Color.FromArgb(50, Color.Red);
                tehlikeAlan.Text = "Tehlike";
                tehlikeAlan.ForeColor = Color.DarkRed;
                chart.ChartAreas[0].AxisY.StripLines.Add(tehlikeAlan);

                // Kritik çizgiler
                StripLine kritikCizgi1 = new StripLine();
                kritikCizgi1.IntervalOffset = 5;
                kritikCizgi1.StripWidth = 0;
                kritikCizgi1.BorderColor = Color.Green;
                kritikCizgi1.BorderWidth = 2;
                kritikCizgi1.BorderDashStyle = ChartDashStyle.Dash;
                chart.ChartAreas[0].AxisY.StripLines.Add(kritikCizgi1);

                StripLine kritikCizgi2 = new StripLine();
                kritikCizgi2.IntervalOffset = 20;
                kritikCizgi2.StripWidth = 0;
                kritikCizgi2.BorderColor = Color.Orange;
                kritikCizgi2.BorderWidth = 2;
                kritikCizgi2.BorderDashStyle = ChartDashStyle.Dash;
                chart.ChartAreas[0].AxisY.StripLines.Add(kritikCizgi2);

                StripLine kritikCizgi3 = new StripLine();
                kritikCizgi3.IntervalOffset = 25;
                kritikCizgi3.StripWidth = 0;
                kritikCizgi3.BorderColor = Color.Red;
                kritikCizgi3.BorderWidth = 3;
                kritikCizgi3.BorderDashStyle = ChartDashStyle.Solid;
                chart.ChartAreas[0].AxisY.StripLines.Add(kritikCizgi3);

                // Serileri temizle ve yeniden oluştur
                chart.Series.Clear();

                // Ana hız serisi - Mavi çizgi, sadece hız verisi
                Series hizSeries = new Series("Hız");
                hizSeries.ChartType = SeriesChartType.Line;
                hizSeries.Color = Color.Blue;
                hizSeries.BorderWidth = 3;
                hizSeries.MarkerStyle = MarkerStyle.None;  // Noktalar kapatıldı - temiz çizgi görünümü
                chart.Series.Add(hizSeries);

                // Legend kapatıldı - temiz görünüm için
                if (chart.Legends.Count > 0)
                {
                    chart.Legends[0].Enabled = false;
                }

                // Başlık
                if (chart.Titles.Count == 0)
                {
                    chart.Titles.Add(new Title("Hız Analizi (m/s)", Docking.Top));
                }
                else
                {
                    chart.Titles[0].Text = "Hız Analizi (m/s)";
                    chart.Titles[0].Font = new Font("Arial", 10, FontStyle.Bold);
                }

                // Zoom ve scroll ayarları
                chart.ChartAreas[0].AxisX.ScrollBar.IsPositionedInside = true;
                chart.ChartAreas[0].AxisX.ScrollBar.Enabled = true;
                chart.ChartAreas[0].AxisX.ScaleView.Zoomable = true;
                chart.ChartAreas[0].CursorX.AutoScroll = true;
            }
            catch (Exception ex)
            {
                LogHata("Hız grafiği yapılandırılamadı: " + ex.Message);
                Console.WriteLine("Hata - ConfigureChartHiz: " + ex.Message);
            }
        }

        /// <summary>
        /// Pil gerilimi grafiğini akıllı batarya sistemi ile yapılandıran metod
        /// </summary>
        private void ConfigureChartPilGerilimi()
        {
            try
            {
                var chart = chartPilGerilimi;

                // ChartArea ayarları
                chart.ChartAreas[0].AxisX.Minimum = 0;
                chart.ChartAreas[0].AxisX.Maximum = 100;
                chart.ChartAreas[0].AxisX.Interval = 10;
                chart.ChartAreas[0].AxisX.MajorGrid.LineColor = Color.LightGray;
                // X ekseni başlığı kaldırıldı - temiz görünüm için

                // Y ekseni pil gerilim aralığı (yazı taşması için genişletildi)
                chart.ChartAreas[0].AxisY.Minimum = 5;
                chart.ChartAreas[0].AxisY.Maximum = 14;
                chart.ChartAreas[0].AxisY.Interval = 1;
                chart.ChartAreas[0].AxisY.MajorGrid.LineColor = Color.LightGray;
                chart.ChartAreas[0].AxisY.Title = "Gerilim (V)";

                // Kritik pil seviyesi uyarı alanları (Strip Lines)
                chart.ChartAreas[0].AxisY.StripLines.Clear();
                
                // Kritik Düşük Alan (5-6V) - Kırmızı arka plan
                StripLine kritikAlan = new StripLine();
                kritikAlan.IntervalOffset = 5;
                kritikAlan.StripWidth = 1;
                kritikAlan.BackColor = Color.FromArgb(50, Color.Red);
                chart.ChartAreas[0].AxisY.StripLines.Add(kritikAlan);

                // Düşük Pil Alanı (6-9V) - Turuncu arka plan
                StripLine dusukAlan = new StripLine();
                dusukAlan.IntervalOffset = 6;
                dusukAlan.StripWidth = 3;
                dusukAlan.BackColor = Color.FromArgb(50, Color.Orange);
                chart.ChartAreas[0].AxisY.StripLines.Add(dusukAlan);

                // Normal Pil Alanı (9-11V) - Yeşil arka plan
                StripLine normalAlan = new StripLine();
                normalAlan.IntervalOffset = 9;
                normalAlan.StripWidth = 2;
                normalAlan.BackColor = Color.FromArgb(50, Color.Green);
                chart.ChartAreas[0].AxisY.StripLines.Add(normalAlan);

                // Tam Pil Alanı (11-12V) - Mavi arka plan
                StripLine tamAlan = new StripLine();
                tamAlan.IntervalOffset = 11;
                tamAlan.StripWidth = 1;
                tamAlan.BackColor = Color.FromArgb(50, Color.Blue);
                chart.ChartAreas[0].AxisY.StripLines.Add(tamAlan);

                // Aşırı Şarj Alanı (12-14V) - Mor arka plan
                StripLine asiriAlan = new StripLine();
                asiriAlan.IntervalOffset = 12;
                asiriAlan.StripWidth = 2;
                asiriAlan.BackColor = Color.FromArgb(50, Color.Purple);
                chart.ChartAreas[0].AxisY.StripLines.Add(asiriAlan);

                // Kritik çizgiler
                StripLine kritikCizgi = new StripLine();
                kritikCizgi.IntervalOffset = 6;
                kritikCizgi.StripWidth = 0;
                kritikCizgi.BorderColor = Color.Red;
                kritikCizgi.BorderWidth = 3;
                kritikCizgi.BorderDashStyle = ChartDashStyle.Solid;
                chart.ChartAreas[0].AxisY.StripLines.Add(kritikCizgi);

                StripLine dusukCizgi = new StripLine();
                dusukCizgi.IntervalOffset = 9;
                dusukCizgi.StripWidth = 0;
                dusukCizgi.BorderColor = Color.Orange;
                dusukCizgi.BorderWidth = 2;
                dusukCizgi.BorderDashStyle = ChartDashStyle.Dash;
                chart.ChartAreas[0].AxisY.StripLines.Add(dusukCizgi);

                StripLine tamCizgi = new StripLine();
                tamCizgi.IntervalOffset = 11;
                tamCizgi.StripWidth = 0;
                tamCizgi.BorderColor = Color.Blue;
                tamCizgi.BorderWidth = 2;
                tamCizgi.BorderDashStyle = ChartDashStyle.Dash;
                chart.ChartAreas[0].AxisY.StripLines.Add(tamCizgi);

                // Serileri temizle ve yeniden oluştur
                chart.Series.Clear();

                // Ana pil gerilimi serisi - sadece pil verisi
                Series pilSeries = new Series("Pil Gerilimi");
                pilSeries.ChartType = SeriesChartType.Line;
                pilSeries.Color = Color.DarkGreen;
                pilSeries.BorderWidth = 3;
                pilSeries.MarkerStyle = MarkerStyle.None;  // Noktalar kapatıldı - temiz çizgi
                chart.Series.Add(pilSeries);

                // Legend kapatıldı - temiz görünüm için
                if (chart.Legends.Count > 0)
                {
                    chart.Legends[0].Enabled = false;
                }

                // Başlık - kompakt ve taşmayacak
                if (chart.Titles.Count == 0)
                {
                    chart.Titles.Add(new Title("Pil Gerilimi (V)", Docking.Top));
                }
                else
                {
                    chart.Titles[0].Text = "Pil Gerilimi (V)";
                    chart.Titles[0].Font = new Font("Arial", 9, FontStyle.Bold);
                    chart.Titles[0].ForeColor = Color.DarkGreen;
                }

                // Zoom ve scroll ayarları
                chart.ChartAreas[0].AxisX.ScrollBar.IsPositionedInside = true;
                chart.ChartAreas[0].AxisX.ScrollBar.Enabled = true;
                chart.ChartAreas[0].AxisX.ScaleView.Zoomable = true;
                chart.ChartAreas[0].CursorX.AutoScroll = true;
            }
            catch (Exception ex)
            {
                LogHata("Pil Gerilimi grafiği yapılandırılamadı: " + ex.Message);
                Console.WriteLine("Hata - ConfigureChartPilGerilimi: " + ex.Message);
            }
        }

        /// <summary>
        /// IoT grafiğini basit ve temiz şekilde yapılandıran metod
        /// </summary>
        private void ConfigureChartIoT()
        {
            try
            {
                var chart = chartIoT;

                // ChartArea ayarları - temiz ve modern
                chart.ChartAreas[0].AxisX.Minimum = 0;
                chart.ChartAreas[0].AxisX.Maximum = 30;  // 30 nokta göster
                chart.ChartAreas[0].AxisX.Interval = 5;
                chart.ChartAreas[0].AxisX.MajorGrid.LineColor = Color.LightGray;
                // X ekseni başlığı kaldırıldı - temiz görünüm için

                // Y ekseni - sadece gerekli aralık (0-40°C)
                chart.ChartAreas[0].AxisY.Minimum = 0;
                chart.ChartAreas[0].AxisY.Maximum = 40;
                chart.ChartAreas[0].AxisY.Interval = 5;
                chart.ChartAreas[0].AxisY.MajorGrid.LineColor = Color.LightGray;
                chart.ChartAreas[0].AxisY.Title = "Sıcaklık (°C)";

                // SADECE 1 ADET kritik çizgi - 35°C (çok sıcak uyarısı)
                chart.ChartAreas[0].AxisY.StripLines.Clear();
                StripLine kritikCizgi = new StripLine();
                kritikCizgi.IntervalOffset = 35;
                kritikCizgi.StripWidth = 0;
                kritikCizgi.BorderColor = Color.OrangeRed;
                kritikCizgi.BorderWidth = 1;
                kritikCizgi.BorderDashStyle = ChartDashStyle.Dot;
                chart.ChartAreas[0].AxisY.StripLines.Add(kritikCizgi);

                // Serileri temizle ve yeniden oluştur
                chart.Series.Clear();

                // IoT İstasyon 1 serisi
                Series s1Series = new Series("İstasyon 1");
                s1Series.ChartType = SeriesChartType.Line;
                s1Series.Color = Color.FromArgb(231, 76, 60);  // Parlak kırmızı (#E74C3C)
                s1Series.BorderWidth = 2;
                s1Series.MarkerStyle = MarkerStyle.None;
                chart.Series.Add(s1Series);

                // IoT İstasyon 2 serisi
                Series s2Series = new Series("İstasyon 2");
                s2Series.ChartType = SeriesChartType.Line;
                s2Series.Color = Color.FromArgb(241, 196, 15);  // Parlak sarı (#F1C40F)
                s2Series.BorderWidth = 2;
                s2Series.MarkerStyle = MarkerStyle.None;
                chart.Series.Add(s2Series);

                // Legend aktif ve özelleştirildi
                if (chart.Legends.Count == 0)
                {
                    chart.Legends.Add(new Legend("IoTLegend"));
                }
                chart.Legends[0].Enabled = true;
                chart.Legends[0].Docking = Docking.Top;
                chart.Legends[0].Alignment = StringAlignment.Center;
                chart.Legends[0].Font = new Font("Arial", 8, FontStyle.Regular);
                chart.Legends[0].BackColor = Color.Transparent;
                chart.Legends[0].BorderColor = Color.Transparent;

                // Başlık - modern ve kısa
                if (chart.Titles.Count == 0)
                {
                    chart.Titles.Add(new Title("IoT Sıcaklık İzleme", Docking.Top));
                }
                else
                {
                    chart.Titles[0].Text = "IoT Sıcaklık İzleme";
                    chart.Titles[0].Font = new Font("Arial", 10, FontStyle.Bold);
                    chart.Titles[0].ForeColor = Color.FromArgb(52, 73, 94);  // Koyu gri
                }

                // Chart background - temiz beyaz
                chart.BackColor = Color.White;
                chart.ChartAreas[0].BackColor = Color.White;
            }
            catch (Exception ex)
            {
                LogHata("IoT grafiği yapılandırılamadı: " + ex.Message);
                Console.WriteLine("Hata - ConfigureChartIoT: " + ex.Message);
            }
        }

        /// <summary>
        /// Telemetri paketinden gelen verileri grafiklere ekleyen metod
        /// </summary>
        public void UpdateCharts(string[] paket)
        {
            try
            {
                // Gerekli verileri çıkar ve dönüştür
                if (double.TryParse(paket[4].Trim(), out double basinc1) &&
                    double.TryParse(paket[6].Trim(), out double yukseklik1) &&
                    double.TryParse(paket[7].Trim(), out double yukseklik2) &&
                    double.TryParse(paket[9].Trim(), out double inisHizi) &&
                    double.TryParse(paket[10].Trim(), out double sicaklik) &&
                    double.TryParse(paket[11].Trim(), out double pilGerilimi) &&
                    double.TryParse(paket[15].Trim(), out double pitch) &&
                    double.TryParse(paket[16].Trim(), out double roll) &&
                    double.TryParse(paket[17].Trim(), out double yaw))
                {
                    // Sıcaklık grafiği (chartSicaklik)
                    chartSicaklik.Series["Sıcaklık(°C)"].Points.AddY(sicaklik);

                    // 🚨 TÜRKSAT YARIŞMASI - VERİ GÜVENLİK MODU
                    // Grafik performansı için son 50 veri gösterilir (eskiler chart'tan silinir)
                    // ANCAK tüm veriler arka planda saklanır ve SD karta kaydedilir!
                    if (chartSicaklik.Series["Sıcaklık(°C)"].Points.Count > 50)
                    {
                        chartSicaklik.Series["Sıcaklık(°C)"].Points.RemoveAt(0);
                        // NOT: Bu sadece görsel chart'tan siliyor, veri kaybı YOK!
                        // Tüm veriler SD karta ve Excel'e tam olarak kaydediliyor.
                    }

                    // Yükseklik grafiği - Gelişmiş akıllı irtifa sistemi
                    UpdateYukseklikGrafigi(yukseklik1);

                    // Basınç grafiği (chartBasinc) - DÜZELTME: Gerçek basınç verisi
                    chartBasinc.Series["Basınç(Pa)"].Points.AddY(basinc1);

                    // 🚨 TÜRKSAT YARIŞMASI - VERİ GÜVENLİK MODU
                    // Grafik performansı için son 50 veri gösterilir
                    if (chartBasinc.Series["Basınç(Pa)"].Points.Count > 50)
                    {
                        chartBasinc.Series["Basınç(Pa)"].Points.RemoveAt(0);
                        // NOT: Sadece görsel chart'tan siliyor, tüm veriler SD karta kaydediliyor!
                    }

                    // Hız grafiği - Gelişmiş akıllı renklendirme sistemi
                    UpdateHizGrafigi(inisHizi);

                    // Pil gerilimi grafiği - Gelişmiş akıllı batarya sistemi
                    UpdatePilGerilimiGrafigi(pilGerilimi);

                    // Grafikleri yenile
                    chartSicaklik.Invalidate();
                    chartYukseklik.Invalidate();
                    chartBasinc.Invalidate();
                    chartHiz.Invalidate();
                    chartPilGerilimi.Invalidate();
                }
            }
            catch (Exception ex)
            {
                LogHata($"Grafikler güncellenirken hata: {ex.Message}", false);
            }
        }

        /// <summary>
        /// Tüm grafikleri doğrudan değerlerle güncelleyen metod
        /// </summary>
        public void UpdateAllCharts(DateTime zaman, string sicaklik, string yukseklik1, string basinc1, string inisHizi, string pilGerilimi, string iotS1Sicaklik = "0", string iotS2Sicaklik = "0")
        {
            try
            {
                // Grafik güncelleme başlangıcı
                
                // Gelen verileri güvenli bir şekilde double'a çevir
                if (!double.TryParse(sicaklik, out double sicaklikValue))
                {
                    LogHata($"Geçersiz sıcaklık değeri: {sicaklik}", true);
                    sicaklikValue = 0; // Hata durumunda varsayılan değer
                }

                if (!double.TryParse(yukseklik1, out double yukseklik1Value))
                {
                    LogHata($"Geçersiz yükseklik değeri: {yukseklik1}", true);
                    yukseklik1Value = 0; // Hata durumunda varsayılan değer
                }

                if (!double.TryParse(basinc1, out double basinc1Value))
                {
                    LogHata($"Geçersiz basınç değeri: {basinc1}", true);
                    basinc1Value = 0; // Hata durumunda varsayılan değer
                }
                
                if (!double.TryParse(inisHizi, out double inisHiziValue))
                {
                    LogHata($"Geçersiz iniş hızı değeri: {inisHizi}", true);
                    inisHiziValue = 0; // Hata durumunda varsayılan değer
                }

                if (!double.TryParse(pilGerilimi, out double pilGerilimiValue))
                {
                    LogHata($"Geçersiz pil gerilimi değeri: {pilGerilimi}", true);
                    pilGerilimiValue = 0; // Hata durumunda varsayılan değer
                }

                // IoT verilerini güvenli bir şekilde double'a çevir
                if (!double.TryParse(iotS1Sicaklik, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out double iotS1Value))
                {
                    iotS1Value = 0; // Hata durumunda varsayılan değer
                }

                if (!double.TryParse(iotS2Sicaklik, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out double iotS2Value))
                {
                    iotS2Value = 0; // Hata durumunda varsayılan değer
                }
                
                // Grafikleri akıllı sistemlerle güncelle (güvenli erişim ile)
                if (chartSicaklik?.Series?.Count > 0)
                {
                    // Zaman olarak X değeri için simple integer kullan (DateTime yerine)
                    int zamanIndex = chartSicaklik.Series[0].Points.Count;
                    chartSicaklik.Series[0].Points.AddXY(zamanIndex, sicaklikValue);
                    
                    // Chart refresh sistemi
                    chartSicaklik.Invalidate();
                }
                else
                    LogHata("Sıcaklık grafiği series'i mevcut değil", true);
                    
                UpdateYukseklikGrafigi(yukseklik1Value);
                
                if (chartBasinc?.Series?.Count > 0)
                {
                    // Zaman olarak X değeri için simple integer kullan
                    int zamanIndex = chartBasinc.Series[0].Points.Count;
                    chartBasinc.Series[0].Points.AddXY(zamanIndex, basinc1Value);
                    
                    // Chart refresh sistemi  
                    chartBasinc.Invalidate();
                }
                else
                    LogHata("Basınç grafiği series'i mevcut değil", true);
                    
                UpdateHizGrafigi(inisHiziValue);
                UpdatePilGerilimiGrafigi(pilGerilimiValue);
                
                // IoT grafiğini her zaman güncelle (0 değeri de kabul et)
                UpdateIoTGrafigi(iotS1Value, iotS2Value);
            }
            catch (Exception ex)
            {
                LogHata("Grafikler güncellenirken hata: " + ex.Message, false);
            }
        }

        /// <summary>
        /// IoT verilerini akıllı analiz sistemi ile grafiğe ekleyen metod
        /// </summary>
        public void GuncelleIotGrafigi()
        {
            try
            {
                // IoT verisi alınmamışsa güncelleme yapma
                if (!IotVerisiAlindiMi)
                {
                    return;
                }

                // IoT verilerini dosyaya kaydet
                string iotVerisi = $"{DateTime.Now},{IotS1Sicaklik},{IotS2Sicaklik}\n";
                File.AppendAllText(iotLogDosyaYolu, iotVerisi);

                // Akıllı IoT grafik güncelleme sistemi
                UpdateIoTGrafigi(IotS1Sicaklik, IotS2Sicaklik);
            }
            catch (Exception ex)
            {
                LogHata($"IoT grafik güncellenirken hata: {ex.Message}", false);
            }
        }

        /// <summary>
        /// Hız değerine göre renk belirleyen metod
        /// </summary>
        private Color GetHizRengi(double hiz)
        {
            if (hiz <= 5)
                return Color.Green;          // Güvenli İniş (0-5 m/s)
            else if (hiz <= 15)
                return Color.Goldenrod;      // Normal Hız (5-15 m/s)
            else if (hiz <= 25)
                return Color.Orange;         // Dikkat (15-25 m/s)
            else
                return Color.Red;            // Tehlike (25+ m/s)
        }

        /// <summary>
        /// Hız durumunu text olarak döndüren metod
        /// </summary>
        private string GetHizDurumu(double hiz)
        {
            if (hiz <= 5)
                return "Güvenli";
            else if (hiz <= 15)
                return "Normal";
            else if (hiz <= 25)
                return "Dikkat";
            else
                return "Tehlike";
        }

        /// <summary>
        /// Gelişmiş hız grafiği güncelleme metodu
        /// </summary>
        private void UpdateHizGrafigi(double inisHizi)
        {
            try
            {
                // Chart ve Series güvenlik kontrolü
                if (chartHiz == null)
                {
                    LogHata("chartHiz null - UpdateHizGrafigi çağrılamıyor", false);
                    return;
                }

                if (chartHiz.Series.Count == 0 || chartHiz.Series["Hız"] == null)
                {
                    LogHata("chartHiz Series[\"Hız\"] bulunamadı - Grafik yapılandırılmamış", false);
                    return;
                }

                // Sadece hız verisi - ortalama hesaplama kaldırıldı

                // Ana hız serisini güncelle
                var hizSeries = chartHiz.Series["Hız"];
                hizSeries.Points.AddY(inisHizi);
                
                // Son eklenen DataPoint'e erişim
                var dataPoint = hizSeries.Points[hizSeries.Points.Count - 1];
                
                // Sabit mavi renk - dinamik renklendirme kapatıldı
                
                // Tooltip ekle
                dataPoint.ToolTip = $"Hız: {inisHizi:F1} m/s\nDurum: {GetHizDurumu(inisHizi)}";

                // 🚨 TÜRKSAT YARIŞMASI - VERİ GÜVENLİK MODU
                // Grafik performansı için son 50 veriyi göster
                if (hizSeries.Points.Count > 50)
                {
                    hizSeries.Points.RemoveAt(0);
                    // NOT: Sadece görsel chart'tan siliyor, tüm veriler korunuyor!
                }

                // Ortalama serisi kaldırıldı - sadece hız verisi gösteriliyor

                // Grafiğin başlığını dinamik güncelle (güvenlik kontrolü ile)
                if (chartHiz.Titles.Count > 0)
                {
                    string durumText = GetHizDurumu(inisHizi);
                    Color durumRengi = GetHizRengi(inisHizi);
                    chartHiz.Titles[0].Text = $"Hız Analizi - {durumText} ({inisHizi:F1} m/s)";
                    chartHiz.Titles[0].ForeColor = durumRengi;
                }
                else
                {
                    LogHata("chartHiz.Titles[0] bulunamadı", false);
                }

                // Kritik durumlarda arka plan rengini değiştir
                if (inisHizi > 25)
                {
                    chartHiz.BackColor = Color.FromArgb(30, Color.Red);
                }
                else if (inisHizi > 15)
                {
                    chartHiz.BackColor = Color.FromArgb(20, Color.Orange);
                }
                else
                {
                    chartHiz.BackColor = Color.White;
                }

                // Grafiği zorla yenile
                chartHiz.Invalidate();
                chartHiz.Update();
                chartHiz.Refresh();
            }
            catch (Exception ex)
            {
                LogHata($"Hız grafiği güncellenirken hata: {ex.Message}", false);
            }
        }

        /// <summary>
        /// Yükseklik değerine göre renk belirleyen metod
        /// </summary>
        private Color GetYukseklikRengi(double yukseklik)
        {
            if (yukseklik <= 100)
                return Color.Green;          // İniş Alanı (0-100m)
            else if (yukseklik <= 300)
                return Color.Blue;           // Düşük İrtifa (100-300m)
            else if (yukseklik <= 600)
                return Color.Goldenrod;      // Orta İrtifa (300-600m)
            else
                return Color.Orange;         // Yüksek İrtifa (600m+)
        }

        /// <summary>
        /// Yükseklik ve trend analizi ile uçuş fazını belirleyen metod
        /// </summary>
        private string GetUcusFazi(double yukseklik, double trend)
        {
            if (yukseklik <= 50)
            {
                if (trend > 0) return "Kalkış";
                else return "İniş";
            }
            else if (yukseklik <= 100)
            {
                return "Alçak Uçuş";
            }
            else if (yukseklik <= 300)
            {
                if (trend > 2) return "Tırmanış";
                else if (trend < -2) return "İniş";
                else return "Düz Uçuş";
            }
            else if (yukseklik <= 600)
            {
                if (trend > 1) return "Yükselme";
                else if (trend < -1) return "Alçalma";
                else return "Seyir";
            }
            else
            {
                return "Yüksek İrtifa";
            }
        }

        /// <summary>
        /// Gelişmiş yükseklik grafiği güncelleme metodu
        /// </summary>
        private void UpdateYukseklikGrafigi(double yukseklik)
        {
            try
            {
                // Chart ve Series güvenlik kontrolü
                if (chartYukseklik == null)
                {
                    LogHata("chartYukseklik null - UpdateYukseklikGrafigi çağrılamıyor", false);
                    return;
                }

                if (chartYukseklik.Series.Count == 0 || chartYukseklik.Series["Yükseklik(m)"] == null)
                {
                    LogHata("chartYukseklik Series[\"Yükseklik(m)\"] bulunamadı - Grafik yapılandırılmamış", false);
                    return;
                }

                // Yükseklik geçmişini güncelle
                yukseklikGecmisi.Add(yukseklik);
                toplamYukseklik += yukseklik;
                yukseklikSayaci++;

                // 🚨 TÜRKSAT YARIŞMASI - VERİ GÜVENLİK MODU
                // Grafik performansı için son 50 veriyi tut (ortalama hesaplama için)
                if (yukseklikGecmisi.Count > 50)
                {
                    toplamYukseklik -= yukseklikGecmisi[0];
                    yukseklikGecmisi.RemoveAt(0);
                    yukseklikSayaci--;
                    // NOT: Bu sadece grafik ortalama hesaplama için, tüm veriler SD karta kaydediliyor!
                }

                // Trend hesaplama (yükselme/alçalma oranı)
                double trend = yukseklik - oncekiYukseklik;
                oncekiYukseklik = yukseklik;

                // Ortalama yüksekliği hesapla
                double ortalamaYukseklik = yukseklikSayaci > 0 ? toplamYukseklik / yukseklikSayaci : 0;

                // Ana yükseklik serisini güncelle
                var yukseklikSeries = chartYukseklik.Series["Yükseklik(m)"];
                yukseklikSeries.Points.AddY(yukseklik);
                
                // Son eklenen DataPoint'e erişim
                var dataPoint = yukseklikSeries.Points[yukseklikSeries.Points.Count - 1];
                
                // Yükseklik değerine göre nokta rengini ayarla
                dataPoint.Color = GetYukseklikRengi(yukseklik);
                dataPoint.MarkerColor = GetYukseklikRengi(yukseklik);
                
                // Tooltip ekle
                string ucusFazi = GetUcusFazi(yukseklik, trend);
                dataPoint.ToolTip = $"Yükseklik: {yukseklik:F1}m\nFaz: {ucusFazi}\nMax: {ortalamaYukseklik:F1}m\nTrend: {trend:+0.0;-0.0;0.0}m/s";

                // 🚨 TÜRKSAT YARIŞMASI - VERİ GÜVENLİK MODU
                // Grafik performansı için son 50 veriyi göster
                if (yukseklikSeries.Points.Count > 50)
                {
                    yukseklikSeries.Points.RemoveAt(0);
                    // NOT: Sadece görsel chart'tan siliyor, tüm veriler korunuyor!
                }

                // Grafiğin başlığını dinamik güncelle (güvenlik kontrolü ile)
                if (chartYukseklik.Titles.Count > 0)
                {
                    string fazText = GetUcusFazi(yukseklik, trend);
                    Color fazRengi = GetYukseklikRengi(yukseklik);
                    chartYukseklik.Titles[0].Text = $"İrtifa Analizi - {fazText} ({yukseklik:F1}m)";
                    chartYukseklik.Titles[0].ForeColor = fazRengi;
                }
                else
                {
                    LogHata("chartYukseklik.Titles[0] bulunamadı", false);
                }

                // Kritik durumlarda arka plan rengini değiştir
                if (yukseklik < 50 && trend < -5)
                {
                    chartYukseklik.BackColor = Color.FromArgb(30, Color.Red); // Hızlı iniş uyarısı
                }
                else if (yukseklik > 800)
                {
                    chartYukseklik.BackColor = Color.FromArgb(20, Color.Orange); // Yüksek irtifa uyarısı
                }
                else
                {
                    chartYukseklik.BackColor = Color.White;
                }
            }
            catch (Exception ex)
            {
                LogHata($"Yükseklik grafiği güncellenirken hata: {ex.Message}", false);
            }
        }

        /// <summary>
        /// Sıcaklık değerine göre basit renk belirleyen metod
        /// </summary>
        private Color GetSicaklikRengi(double sicaklik)
        {
            if (sicaklik < 0)
                return Color.Blue;           // Soğuk (0°C altı)
            else if (sicaklik <= 35)
                return Color.Green;          // Normal (0°C - 35°C)
            else
                return Color.Red;            // Kritik Sıcak (35°C+)
        }

        /// <summary>
        /// Sıcaklık durumunu text olarak döndüren metod
        /// </summary>
        private string GetSicaklikDurumu(double sicaklik)
        {
            if (sicaklik < -20)
                return "Donma";
            else if (sicaklik < 0)
                return "Soğuk";
            else if (sicaklik <= 25)
                return "Normal";
            else if (sicaklik <= 40)
                return "Sıcak";
            else
                return "Kritik Sıcak";
        }

        /// <summary>
        /// İki istasyon arasındaki sıcaklık farkını analiz eden metod
        /// </summary>
        private string GetIstasyonKarsilastirma(double s1, double s2)
        {
            double fark = Math.Abs(s1 - s2);
            if (fark > 15)
                return "Büyük Fark";
            else if (fark > 10)
                return "Orta Fark";
            else if (fark > 5)
                return "Küçük Fark";
            else
                return "Benzer";
        }

        /// <summary>
        /// Basit IoT grafiği güncelleme metodu
        /// </summary>
        private void UpdateIoTGrafigi(double s1Sicaklik, double s2Sicaklik)
        {
            try
            {
                // Chart ve Series güvenlik kontrolü
                if (chartIoT == null || chartIoT.Series.Count == 0 || 
                    chartIoT.Series["İstasyon 1"] == null || chartIoT.Series["İstasyon 2"] == null)
                {
                    return;
                }

                // S1 İstasyon serisini güncelle - sabit modern mavi renk
                var s1Series = chartIoT.Series["İstasyon 1"];
                s1Series.Points.AddY(s1Sicaklik);
                
                // Son eklenen S1 DataPoint - sabit renk (karmaşa yok)
                var s1DataPoint = s1Series.Points[s1Series.Points.Count - 1];
                s1DataPoint.ToolTip = $"İstasyon 1: {s1Sicaklik:F1}°C";

                // S2 İstasyon serisini güncelle - sabit modern yeşil renk
                var s2Series = chartIoT.Series["İstasyon 2"];
                s2Series.Points.AddY(s2Sicaklik);
                
                // Son eklenen S2 DataPoint - sabit renk (karmaşa yok)
                var s2DataPoint = s2Series.Points[s2Series.Points.Count - 1];
                s2DataPoint.ToolTip = $"İstasyon 2: {s2Sicaklik:F1}°C";

                // Grafik performansı için son 30 veriyi göster
                if (s1Series.Points.Count > 30)
                {
                    s1Series.Points.RemoveAt(0);
                }

                if (s2Series.Points.Count > 30)
                {
                    s2Series.Points.RemoveAt(0);
                }

                // Başlık - sadece kritik durumlarda değiştir (SADELEŞTİRİLDİ)
                if (chartIoT.Titles.Count > 0)
                {
                    double maxSicaklik = Math.Max(s1Sicaklik, s2Sicaklik);
                    if (maxSicaklik > 35)
                    {
                        chartIoT.Titles[0].Text = $"IoT Sıcaklık İzleme - YÜKSEK ({maxSicaklik:F0}°C)";
                        chartIoT.Titles[0].ForeColor = Color.OrangeRed;
                    }
                    else
                    {
                        chartIoT.Titles[0].Text = "IoT Sıcaklık İzleme";
                        chartIoT.Titles[0].ForeColor = Color.FromArgb(52, 73, 94);  // Normal koyu gri
                    }
                }

                // Grafik alanını yenile
                chartIoT.Invalidate();
                chartIoT.Update();
                chartIoT.Refresh();
            }
            catch (Exception ex)
            {
                LogHata($"❌ IoT grafiği güncellenirken hata: {ex.Message}", false);
            }
        }

        /// <summary>
        /// Pil gerilimi değerine göre renk belirleyen metod
        /// </summary>
        private Color GetPilRengi(double gerilim)
        {
            if (gerilim < 6)
                return Color.Red;            // Kritik Düşük (0-6V)
            else if (gerilim < 9)
                return Color.Orange;         // Düşük Pil (6-9V)
            else if (gerilim < 11)
                return Color.Green;          // Normal Pil (9-11V)
            else if (gerilim <= 12)
                return Color.Blue;           // Tam Pil (11-12V)
            else
                return Color.Purple;         // Aşırı Şarj (12V+)
        }

        /// <summary>
        /// Pil gerilimi durumunu text olarak döndüren metod
        /// </summary>
        private string GetPilDurumu(double gerilim)
        {
            if (gerilim < 6)
                return "Kritik Düşük";
            else if (gerilim < 9)
                return "Düşük Pil";
            else if (gerilim < 11)
                return "Normal";
            else if (gerilim <= 12)
                return "Tam Pil";
            else
                return "Aşırı Şarj";
        }

        /// <summary>
        /// Pil yüzdesi hesaplayan metod (9V-12V arası = %0-100)
        /// </summary>
        private double GetPilYuzdesi(double gerilim)
        {
            // 9V = %0, 12V = %100 olarak hesapla
            if (gerilim < 9) return 0;
            if (gerilim > 12) return 100;
            return ((gerilim - 9) / 3) * 100;
        }

        /// <summary>
        /// Gelişmiş pil gerilimi grafiği güncelleme metodu
        /// </summary>
        private void UpdatePilGerilimiGrafigi(double pilGerilimi)
        {
            try
            {
                // Chart ve Series güvenlik kontrolü
                if (chartPilGerilimi == null)
                {
                    LogHata("chartPilGerilimi null - UpdatePilGerilimiGrafigi çağrılamıyor", false);
                    return;
                }

                if (chartPilGerilimi.Series.Count == 0 || chartPilGerilimi.Series["Pil Gerilimi"] == null)
                {
                    LogHata("chartPilGerilimi Series[\"Pil Gerilimi\"] bulunamadı - Grafik yapılandırılmamış", false);
                    return;
                }

                // Sadece pil verisi - ortalama hesaplama kaldırıldı
                
                // Min/Max değerleri güncelle
                if (pilGerilimi > maxPilGerilimi) maxPilGerilimi = pilGerilimi;
                if (pilGerilimi < minPilGerilimi) minPilGerilimi = pilGerilimi;

                // Şarj/Deşarj trend analizi
                double trend = pilGerilimi - oncekiPilGerilimi;
                string trendText = trend > 0.1 ? "↗ Şarj" : (trend < -0.1 ? "↘ Deşarj" : "→ Sabit");
                oncekiPilGerilimi = pilGerilimi;

                // Ana pil serisi güncelle
                var pilSeries = chartPilGerilimi.Series["Pil Gerilimi"];
                pilSeries.Points.AddY(pilGerilimi);
                
                // Son eklenen DataPoint'e erişim
                var dataPoint = pilSeries.Points[pilSeries.Points.Count - 1];
                
                // Sabit yeşil renk - dinamik renklendirme kapatıldı
                
                // Detaylı tooltip ekle (ortalama kaldırıldı)
                string pilDurumu = GetPilDurumu(pilGerilimi);
                double pilYuzdesi = GetPilYuzdesi(pilGerilimi);
                dataPoint.ToolTip = $"Pil: {pilGerilimi:F2}V ({pilYuzdesi:F0}%)\nDurum: {pilDurumu}\nTrend: {trendText}\nMax: {maxPilGerilimi:F2}V\nMin: {minPilGerilimi:F2}V";

                // 🚨 TÜRKSAT YARIŞMASI - VERİ GÜVENLİK MODU
                // Grafik performansı için son 50 veriyi göster
                if (pilSeries.Points.Count > 50)
                {
                    pilSeries.Points.RemoveAt(0);
                    // NOT: Sadece görsel chart'tan siliyor, tüm veriler korunuyor!
                }

                // Ortalama serisi kaldırıldı - sadece pil gerilimi gösteriliyor

                // Grafiğin başlığını dinamik güncelle (güvenlik kontrolü ile) - kompakt
                if (chartPilGerilimi.Titles.Count > 0)
                {
                    Color durumRengi = GetPilRengi(pilGerilimi);
                    chartPilGerilimi.Titles[0].Text = $"Pil ({pilGerilimi:F1}V)";
                    chartPilGerilimi.Titles[0].ForeColor = durumRengi;
                }
                else
                {
                    LogHata("chartPilGerilimi.Titles[0] bulunamadı", false);
                }

                // Kritik durumlarda arka plan rengini değiştir
                if (pilGerilimi < 6)
                {
                    chartPilGerilimi.BackColor = Color.FromArgb(30, Color.Red);
                }
                else if (pilGerilimi < 9)
                {
                    chartPilGerilimi.BackColor = Color.FromArgb(20, Color.Orange);
                }
                else
                {
                    chartPilGerilimi.BackColor = Color.White;
                }

                // Grafiği zorla yenile
                chartPilGerilimi.Invalidate();
                chartPilGerilimi.Update();
                chartPilGerilimi.Refresh();
            }
            catch (Exception ex)
            {
                LogHata($"Pil gerilimi grafiği güncellenirken hata: {ex.Message}", false);
            }
        }

        /// <summary>
        /// Hata log metodu
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