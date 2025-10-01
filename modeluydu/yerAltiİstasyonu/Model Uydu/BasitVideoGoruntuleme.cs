using System;
using System.Drawing;
using System.Windows.Forms;
using System.IO;

namespace ModelUydu
{
    /// <summary>
    /// Basit Video Görüntüleme Sistemi
    /// Donma problemlerini çözmek için sade ve güvenli video görüntüleme
    /// </summary>
    public class BasitVideoGoruntuleme
    {
        private PictureBox videoPanel;
        private Action<string, bool> logHata;
        private bool aktif = false;
        private int frameCount = 0;

        public BasitVideoGoruntuleme(PictureBox videoPanel, Action<string, bool> logHata)
        {
            this.videoPanel = videoPanel;
            this.logHata = logHata;
            
            // Panel ayarları
            if (videoPanel != null)
            {
                videoPanel.SizeMode = PictureBoxSizeMode.StretchImage;
                videoPanel.BackColor = Color.Black;
            }
            
            logHata?.Invoke("✅ Basit Video Görüntüleme sistemi başlatıldı", false);
        }

        /// <summary>
        /// Video sistemi aktif mi?
        /// </summary>
        public bool Aktif
        {
            get { return aktif; }
        }

        /// <summary>
        /// Video görüntülemeyi başlat
        /// </summary>
        public void Baslat()
        {
            try
            {
                aktif = true;
                frameCount = 0;
                
                if (videoPanel != null)
                {
                    videoPanel.BackColor = Color.DarkGreen;
                    
                    // Başlangıç mesajı göster
                    using (Graphics g = videoPanel.CreateGraphics())
                    {
                        g.Clear(Color.DarkGreen);
                        using (Font font = new Font("Arial", 12, FontStyle.Bold))
                        using (Brush brush = new SolidBrush(Color.White))
                        {
                            string mesaj = "📹 VIDEO SİSTEMİ HAZIR\nGörev yükünden video bekleniyor...";
                            g.DrawString(mesaj, font, brush, 10, 10);
                        }
                    }
                }
                
                logHata?.Invoke("🎬 Video görüntüleme başlatıldı", false);
            }
            catch (Exception ex)
            {
                logHata?.Invoke($"❌ Video başlatma hatası: {ex.Message}", true);
            }
        }

        /// <summary>
        /// Video frame işle ve görüntüle - ULTRA BASIT VERSİYON
        /// </summary>
        /// <param name="frameData">JPEG format video frame</param>
        public void FrameGoruntule(byte[] frameData)
        {
            if (!aktif || frameData == null || frameData.Length == 0 || videoPanel == null)
            {
                logHata?.Invoke($"❌ FrameGoruntule ATLANIYORR: aktif={aktif}, frameData={frameData?.Length}, panel={videoPanel != null}", true);
                Console.WriteLine($"❌ CONSOLE: FrameGoruntule ATLANIYOR - aktif={aktif}, frameData={frameData?.Length}, panel={videoPanel != null}");
                return;
            }

            try
            {
                // DEBUG LOG - ARTIRILDI
                logHata?.Invoke($"🎬 BASİT VİDEO: Frame işleniyor ({frameData.Length} bytes)", false);
                System.Diagnostics.Debug.WriteLine($"🎬 BASİT VİDEO: Frame işleniyor ({frameData.Length} bytes)");
                Console.WriteLine($"🎬 CONSOLE: BASİT VİDEO Frame işleniyor - {frameData.Length} bytes");
                // UI thread kontrolü
                if (videoPanel.InvokeRequired)
                {
                    videoPanel.BeginInvoke(new Action(() => FrameGoruntule(frameData)));
                    return;
                }

                // JPEG'i Bitmap'e çevir - EN BASIT YÖNTEM
                using (MemoryStream ms = new MemoryStream(frameData))
                {
                    try
                    {
                        using (Bitmap yeniFrame = new Bitmap(ms))
                        {
                            // Eski image'ı temizle
                            if (videoPanel.Image != null)
                            {
                                videoPanel.Image.Dispose();
                            }

                            // Yeni frame'i kopyala ve ayarla
                            videoPanel.Image = new Bitmap(yeniFrame);
                            
                            frameCount++;
                            
                            // Her 10 frame'de bir log (daha sık debug için)
                            if (frameCount % 10 == 0)
                            {
                                logHata?.Invoke($"📹 Video: {frameCount} frame görüntülendi ({frameData.Length} bytes)", false);
                                Console.WriteLine($"📹 CONSOLE: {frameCount} frame görüntülendi - {frameData.Length} bytes");
                            }
                        }
                    }
                    catch (ArgumentException)
                    {
                        // Geçersiz JPEG verisi - hata mesajı göster
                        GosterHataMesaji("Geçersiz JPEG verisi");
                    }
                }
            }
            catch (Exception ex)
            {
                logHata?.Invoke($"❌ Frame görüntüleme hatası: {ex.Message}", true);
                GosterHataMesaji($"Frame hatası: {ex.Message}");
            }
        }

        /// <summary>
        /// Video panelinde hata mesajı göster
        /// </summary>
        private void GosterHataMesaji(string hata)
        {
            try
            {
                if (videoPanel == null) return;

                // Kırmızı arka plan ile hata göster
                using (Bitmap hataBitmap = new Bitmap(videoPanel.Width, videoPanel.Height))
                using (Graphics g = Graphics.FromImage(hataBitmap))
                {
                    g.Clear(Color.DarkRed);
                    
                    using (Font font = new Font("Arial", 10, FontStyle.Bold))
                    using (Brush brush = new SolidBrush(Color.White))
                    {
                        string mesaj = $"❌ VIDEO HATASI\n{hata}\n\nFrame: {frameCount}";
                        g.DrawString(mesaj, font, brush, 10, 10);
                    }
                    
                    // Eski image'ı temizle
                    if (videoPanel.Image != null)
                    {
                        videoPanel.Image.Dispose();
                    }
                    
                    videoPanel.Image = new Bitmap(hataBitmap);
                }
            }
            catch
            {
                // Hata gösterirken hata olursa sessizce geç
            }
        }

        /// <summary>
        /// Test mesajı göster
        /// </summary>
        public void GosterTestMesaji()
        {
            try
            {
                if (videoPanel == null) return;

                using (Bitmap testBitmap = new Bitmap(videoPanel.Width, videoPanel.Height))
                using (Graphics g = Graphics.FromImage(testBitmap))
                {
                    g.Clear(Color.Blue);
                    
                    using (Font font = new Font("Arial", 14, FontStyle.Bold))
                    using (Brush brush = new SolidBrush(Color.White))
                    {
                        string mesaj = $"🧪 TEST MODU\nBasit Video Sistemi\n\nZaman: {DateTime.Now:HH:mm:ss}";
                        g.DrawString(mesaj, font, brush, 10, 10);
                    }
                    
                    if (videoPanel.Image != null)
                    {
                        videoPanel.Image.Dispose();
                    }
                    
                    videoPanel.Image = new Bitmap(testBitmap);
                }
                
                logHata?.Invoke("🧪 Test mesajı görüntülendi", false);
            }
            catch (Exception ex)
            {
                logHata?.Invoke($"❌ Test mesajı hatası: {ex.Message}", true);
            }
        }

        /// <summary>
        /// Video görüntülemeyi durdur
        /// </summary>
        public void Durdur()
        {
            try
            {
                aktif = false;
                
                if (videoPanel != null)
                {
                    if (videoPanel.Image != null)
                    {
                        videoPanel.Image.Dispose();
                        videoPanel.Image = null;
                    }
                    
                    videoPanel.BackColor = Color.Black;
                }
                
                logHata?.Invoke("⏹️ Video görüntüleme durduruldu", false);
            }
            catch (Exception ex)
            {
                logHata?.Invoke($"❌ Video durdurma hatası: {ex.Message}", true);
            }
        }

        /// <summary>
        /// Kaynakları temizle
        /// </summary>
        public void Dispose()
        {
            try
            {
                Durdur();
                logHata?.Invoke("🧹 Basit Video sistemi temizlendi", false);
            }
            catch
            {
                // Dispose sırasında hata olursa sessizce geç
            }
        }
    }
}
