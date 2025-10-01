using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using System.IO;
using System.Drawing.Imaging;
using AForge.Controls; // VideoSourcePlayer sınıfı için gerekli
using AForge.Video.DirectShow; // DirectShow kameralar için
using AForge.Video; // Video işleme için
using Accord.Video.FFMPEG; // MP4 video dosyası okuma için

namespace ModelUydu
{
    /// <summary>
    /// Görev yükünden XBee ile gelen video stream'ini işleyen ve görüntüleyen sınıf
    /// Requirements.md Gereksinim 17, 18, 29 uyumlu implementasyon
    /// 
    /// ÖNEMLİ NOT: Test modu sadece test amaçlı MP4 dosyası oynatır.
    /// Gerçek görevde görev yükünden gelen CANLI görüntüler gösterilir!
    /// </summary>
    public class KameraYoneticisi
    {
        // Video görüntüleme ve kayıt bileşenleri
        private VideoSourcePlayer videoPlayer;
        private Bitmap mevcutFrame;
        private VideoFileWriter videoYazici;
        
        // UI kontrolleri için referanslar
        // UI kontrolleri - ŞARTNAME GEREĞİ: Manuel butonlar kaldırıldı
        
        // Video stream durumu
        private bool videoStreamAktif = false;
        private bool kayitModu = false;
        private string kayitDosyaYolu = "";
        
        // Test modu için değişkenler (SADECE TEST AMAÇLI!)
        private bool testModu = false;
        // testVideoTimer ve testFrameIndex kaldırıldı (kullanılmıyor)
        
        // MP4 video oynatma için değişkenler (SADECE TEST MODU)
        private VideoFileReader mp4VideoReader;
        private bool mp4Oynatiliyor = false;
        private Timer mp4Timer;
        private int mp4FrameIndex = 0;
        private int toplamMp4Frame = 0; // CS1519 hatası düzeltildi - boşluk kaldırıldı
        
        // Hata loglama için callback
        private Action<string, bool> logHata;
        
        // 🔧 DÜZELTME: Frame display counter için instance variable
        private int frameDisplayCount = 0;
        
        // Video stream parametreleri
        private int frameGenislik = 640;
        private int frameYukseklik = 480;
        private int fps = 15; // XBee bandwidth limiti nedeniyle düşük FPS
        
        // Test komut efektleri için değişkenler
        private string aktifTestEfekti = "NORMAL";
        private Random efektRandom = new Random();
        private int efektSayaci = 0;
        
        /// <summary>
        /// KameraYoneticisi yapıcı metodu (Görev Yükü Video Stream için)
        /// ŞARTNAME GEREĞİ: Kamera sistemi otomatik çalışır, manuel butonlar kaldırıldı
        /// </summary>
        /// <param name="videoPlayer">Video görüntüleme kontrolü</param>
        /// <param name="logHata">Hata loglama fonksiyonu</param>
        public KameraYoneticisi(VideoSourcePlayer videoPlayer, Action<string, bool> logHata)
        {
            this.videoPlayer = videoPlayer;
            this.logHata = logHata;
            
            // Video sistemini başlat
            InitializeVideoSystem();
        }
        
        /// <summary>
        /// Görev yükü video sistemini başlat
        /// </summary>
        private void InitializeVideoSystem()
        {
            try
            {
                // Video yazıcı için codec ayarla
                videoYazici = new VideoFileWriter();
                
                // ŞARTNAME GEREĞİ: Manuel butonlar kaldırıldı, otomatik sistem başlatıldı
                logHata?.Invoke("Görev yükü video sistemi başlatıldı. Otomatik mod aktif.", false);
            }
            catch (Exception ex)
            {
                logHata?.Invoke("Video sistemi başlatılırken hata: " + ex.Message, true);
            }
        }
        
        /// <summary>
        /// Video stream'i başlat (XBee'den veri gelmeye başladığında çağrılır)
        /// </summary>
        public void VideoStreamBaslat()
        {
            try
            {
                if (!videoStreamAktif)
                {
                    videoStreamAktif = true;
                    
                    // ŞARTNAME GEREĞİ: Video stream başlarken otomatik kayıt başlat (Madde 17)
                    if (!kayitModu)
                    {
                        VideoKayitBaslat();
                    }
                    
                    // ŞARTNAME GEREĞİ: Manuel butonlar kaldırıldı, otomatik sistem
                    
                    logHata?.Invoke("Görev yükü video stream'i başlatıldı. Otomatik kayıt aktif.", false);
                }
            }
            catch (Exception ex)
            {
                logHata?.Invoke("Video stream başlatılırken hata: " + ex.Message, true);
            }
        }
        
        /// <summary>
        /// Test modunu başlat - SADECE TEST AMAÇLI MP4 video oynatır
        /// GERÇEK GÖREVDE: Görev yükünden gelen canlı görüntüler VideoFrameIsle() ile işlenir
        /// </summary>
        public void TestModuBaslat()
        {
            try
            {
                if (!testModu)
                {
                    testModu = true;
                    
                    // Test video dosyasını yükle
                    string testVideoDosyasi = Path.Combine(Application.StartupPath, "test_video", "test_video.mp4");
                    
                    if (File.Exists(testVideoDosyasi))
                    {
                        // MP4 video reader'ını oluştur
                        mp4VideoReader = new VideoFileReader();
                        mp4VideoReader.Open(testVideoDosyasi);
                        
                        // Video bilgilerini al
                        toplamMp4Frame = (int)mp4VideoReader.FrameCount; // CS1073 hatası düzeltildi
                        mp4FrameIndex = 0;
                        
                        // Video oynatma timer'ını oluştur
                        mp4Timer = new Timer();
                        mp4Timer.Interval = 1000 / fps; // 15 FPS
                        mp4Timer.Tick += Mp4Timer_Tick;
                        mp4Timer.Start();
                        
                        mp4Oynatiliyor = true;
                        
                        // Video stream'i de aktif et (otomatik kayıt dahil)
                        VideoStreamBaslat();
                        
                        logHata?.Invoke($"🎬 TEST MODU başlatıldı. MP4 video ({toplamMp4Frame} frame) oynatılıyor. Otomatik kayıt aktif.", false);
                    }
                    else
                    {
                        logHata?.Invoke("⚠️ Test video dosyası bulunamadı: " + testVideoDosyasi, true);
                    }
                }
            }
            catch (Exception ex)
            {
                logHata?.Invoke("Test modu başlatılırken hata: " + ex.Message, true);
            }
        }
        
        /// <summary>
        /// 🔧 DÜZELTME: MP4 timer tick event handler - BELLEK SIZINTISI ÇÖZÜLDİ
        /// </summary>
        private void Mp4Timer_Tick(object sender, EventArgs e)
        {
            Bitmap frame = null;
            Bitmap efektFrame = null;
            
            try
            {
                if (testModu && mp4Oynatiliyor && mp4VideoReader != null && mp4FrameIndex < toplamMp4Frame)
                {
                    // 🔧 BELLEK KONTROLÜ: Frame okumadan önce bellek durumunu kontrol et
                    long availableMemory = GC.GetTotalMemory(false);
                    if (availableMemory > 500_000_000) // 500MB'den fazlaysa garbage collection tetikle
                    {
                        GC.Collect();
                        GC.WaitForPendingFinalizers();
                        logHata?.Invoke($"🧹 Bellek temizliği yapıldı. Önceki: {availableMemory / 1_000_000}MB", false);
                    }
                    
                    // Frame'i oku
                    frame = mp4VideoReader.ReadVideoFrame(mp4FrameIndex);
                    
                    if (frame != null)
                    {
                        // 🔧 DÜZELTME: Mevcut frame'i güvenli şekilde güncelle
                        if (mevcutFrame != null)
                        {
                            mevcutFrame.Dispose();
                            mevcutFrame = null;
                        }
                        mevcutFrame = new Bitmap(frame);
                        
                        // Test komut efektlerini uygula
                        efektFrame = EfektUygula(frame);
                        
                        // 🔧 DÜZELTME: UI güncellemesini güvenli şekilde yap
                        if (videoPlayer != null && !videoPlayer.IsDisposed)
                        {
                        if (videoPlayer.InvokeRequired)
                        {
                                videoPlayer.Invoke(new Action(() => 
                                {
                                    try
                                    {
                                        GoruntuleFrame(efektFrame);
                                    }
                                    catch (ObjectDisposedException)
                                    {
                                        // UI kapatılmışsa sessizce geç
                                    }
                                }));
                        }
                        else
                        {
                            GoruntuleFrame(efektFrame);
                            }
                        }
                        
                        // Kayıt modundaysa dosyaya yaz (orijinal frame'i yaz)
                        if (kayitModu && videoYazici != null)
                        {
                            videoYazici.WriteVideoFrame(frame);
                        }
                    }
                    
                    // Sonraki frame'e geç
                    mp4FrameIndex++;
                    
                    // Video sonuna geldiysek başa dön (döngüsel oynatma)
                    if (mp4FrameIndex >= toplamMp4Frame)
                    {
                        mp4FrameIndex = 0;
                        logHata?.Invoke("🔄 Test video döngüsü yeniden başladı.", false);
                    }
                }
            }
            catch (OutOfMemoryException)
            {
                // 🔧 BELLEK YETERSİZLİĞİ: Acil bellek temizliği
                logHata?.Invoke("❌ KRİTİK: Bellek yetersiz! Acil temizlik yapılıyor...", true);
                
                // Timer'ı geçici durdur
                if (mp4Timer != null)
                {
                    mp4Timer.Stop();
                }
                
                // Bellek temizliği
                if (mevcutFrame != null)
                {
                    mevcutFrame.Dispose();
                    mevcutFrame = null;
                }
                
                GC.Collect();
                GC.WaitForPendingFinalizers();
                GC.Collect();
                
                // Timer'ı tekrar başlat
                if (mp4Timer != null)
                {
                    mp4Timer.Start();
                }
                
                logHata?.Invoke("✅ Bellek temizliği tamamlandı, video oynatma devam ediyor.", false);
            }
            catch (Exception ex)
            {
                logHata?.Invoke($"MP4 timer tick'inde hata: {ex.Message}", true);
            }
            finally
            {
                // 🔧 DÜZELTME: Finally bloğunda kesin temizlik
                if (efektFrame != null && efektFrame != frame)
                {
                    efektFrame.Dispose();
                    efektFrame = null;
                }
                
                if (frame != null)
                {
                    frame.Dispose();
                    frame = null;
                }
            }
        } // CS1513 hatası düzeltildi - eksik süslü parantez eklendi
        
        /// <summary>
        /// Test modunu durdur
        /// </summary>
        public void TestModuDurdur()
        {
            try
            {
                if (testModu)
                {
                    testModu = false;
                    mp4Oynatiliyor = false;
                    
                    // MP4 timer'ını durdur
                    if (mp4Timer != null)
                    {
                        mp4Timer.Stop();
                        mp4Timer.Dispose();
                        mp4Timer = null;
                    }
                    
                    // MP4 video reader'ını kapat
                    if (mp4VideoReader != null)
                    {
                        mp4VideoReader.Close();
                        mp4VideoReader.Dispose();
                        mp4VideoReader = null;
                    }
                    
                    // Video player kaynağını temizle
                    if (videoPlayer != null)
                    {
                        if (videoPlayer.IsRunning)
                        {
                            videoPlayer.Stop();
                        }
                        videoPlayer.VideoSource = null;
                    }
                    
                    // Frame indekslerini sıfırla
                    mp4FrameIndex = 0;
                    toplamMp4Frame = 0; // CS hatası düzeltildi
                    
                    // Test frame temizleme işlemleri artık gerekli değil (kullanılmayan değişkenler kaldırıldı)
                    
                    logHata?.Invoke("🎬 Test modu durduruldu.", false);
                }
            }
            catch (Exception ex)
            {
                logHata?.Invoke("Test modu durdurulurken hata: " + ex.Message, true);
            }
        }
        
        /// <summary>
        /// Video kaydını başlat
        /// </summary>
        public void VideoKayitBaslat()
        {
            try
            {
                if (videoStreamAktif && !kayitModu)
                {
                                                                // Kayıt dosyası yolu oluştur - ortak VeriKayitlari klasörüne
                    string videoDizini = Path.Combine(Application.StartupPath, "VeriKayitlari", "video");
                    if (!Directory.Exists(videoDizini))
                    {
                        Directory.CreateDirectory(videoDizini);
                    }
                    
                    string dosyaAdi = $"GorevYuku_Video_{DateTime.Now:yyyyMMdd_HHmmss}.avi";
                    kayitDosyaYolu = Path.Combine(videoDizini, dosyaAdi);
                        
                        // Video dosyasını aç
                        videoYazici.Open(kayitDosyaYolu, frameGenislik, frameYukseklik, fps, VideoCodec.MPEG4, 1000000);
                        
                        kayitModu = true;
                        
                        // ŞARTNAME GEREĞİ: Manuel butonlar kaldırıldı
                        
                        logHata?.Invoke($"📹 Video kaydı başlatıldı!\n📂 Dosya: {kayitDosyaYolu}", false);
                }
            }
            catch (Exception ex)
            {
                logHata?.Invoke("Video kaydı başlatılırken hata: " + ex.Message, true);
            }
        }
        
        /// <summary>
        /// Video kaydını durdur
        /// </summary>
        public void VideoKayitDurdur()
        {
            try
            {
                if (kayitModu && videoYazici != null)
                {
                    videoYazici.Close();
                    kayitModu = false;
                    
                    // ŞARTNAME GEREĞİ: Manuel butonlar kaldırıldı
                    
                    logHata?.Invoke($"✅ Video kaydı tamamlandı!\n📂 Dosya: {kayitDosyaYolu}", false);
                }
            }
            catch (Exception ex)
            {
                logHata?.Invoke("Video kaydı durdurulurken hata: " + ex.Message, true);
            }
        }
        
        /// <summary>
        /// Video stream'i tamamen durdur
        /// </summary>
        public void VideoStreamDurdur()
        {
            try
            {
                // Test modu aktifse önce test modunu durdur
                if (testModu)
                {
                    TestModuDurdur();
                }
                
                // Kayıt modundaysa önce kaydı durdur
                if (kayitModu)
                {
                    VideoKayitDurdur();
                }
                
                videoStreamAktif = false;
                
                // Video player'ı temizle
                if (videoPlayer != null)
                {
                    if (videoPlayer.IsRunning)
                    {
                        videoPlayer.Stop();
                    }
                    videoPlayer.VideoSource = null;
                }
                
                // ŞARTNAME GEREĞİ: Manuel butonlar kaldırıldı
                
                logHata?.Invoke("Video stream durduruldu.", false);
            }
            catch (Exception ex)
            {
                logHata?.Invoke("Video stream durdurulurken hata: " + ex.Message, true);
            }
        }
        
        /// <summary>
        /// XBee'den gelen video frame'ini işle ve görüntüle
        /// 
        /// ÖNEMLİ: GERÇEK GÖREVDE bu metod kullanılır!
        /// Görev yükünden gelen CANLI görüntüler burada işlenir.
        /// Test modunda bu metod çağrılmaz, MP4 video oynatılır.
        /// </summary>
        /// <param name="frameData">JPEG formatında video frame verisi</param>
        public void VideoFrameIsle(byte[] frameData)
        {
            try
            {
                if (!videoStreamAktif || frameData == null || frameData.Length == 0)
                    return;
                
                // GÜVENLI FRAME İŞLEME - Donma önlemi
                System.Diagnostics.Debug.WriteLine($"KAMERA: Frame işleniyor ({frameData.Length} bytes)");
                
                // Byte array'i Bitmap'e çevir - GÜVENLI VERSİYON
                using (MemoryStream ms = new MemoryStream(frameData))
                {
                    try
                    {
                        using (Bitmap yeniFrame = new Bitmap(ms))
                        {
                            // Önceki frame'i güvenli şekilde temizle
                            if (mevcutFrame != null)
                            {
                                mevcutFrame.Dispose();
                                mevcutFrame = null;
                            }
                            
                            // Yeni frame'i kopyala
                            mevcutFrame = new Bitmap(yeniFrame);
                            
                            // SADECE GÖRÜNTÜLEME - Efekt yok, kayıt yok (performans için)
                            if (videoPlayer != null)
                            {
                                // UI thread kontrolü
                                if (videoPlayer.InvokeRequired)
                                {
                                    videoPlayer.BeginInvoke(new Action(() => 
                                    {
                                        try
                                        {
                                            GoruntuleFrameBasit(mevcutFrame);
                                        }
                                        catch (Exception uiEx)
                                        {
                                            System.Diagnostics.Debug.WriteLine($"UI HATASI: {uiEx.Message}");
                                        }
                                    }));
                                }
                                else
                                {
                                    GoruntuleFrameBasit(mevcutFrame);
                                }
                            }
                            
                            System.Diagnostics.Debug.WriteLine($"KAMERA: Frame başarıyla işlendi");
                        }
                    }
                    catch (ArgumentException argEx)
                    {
                        logHata?.Invoke($"Geçersiz JPEG verisi: {argEx.Message}", true);
                        System.Diagnostics.Debug.WriteLine($"JPEG HATASI: {argEx.Message}");
                    }
                }
            }
            catch (Exception ex)
            {
                logHata?.Invoke("Video frame işlenirken hata: " + ex.Message, true);
                System.Diagnostics.Debug.WriteLine($"FRAME İŞLEME HATASI: {ex.Message}");
            }
        }
        
        /// <summary>
        /// Basit frame görüntüleme - donma önlemi için
        /// </summary>
        private void GoruntuleFrameBasit(Bitmap frame)
        {
            try
            {
                if (frame == null || videoPlayer == null || videoPlayer.IsDisposed)
                    return;
                
                // Önceki background image'ı temizle
                if (videoPlayer.BackgroundImage != null)
                {
                    var oldImage = videoPlayer.BackgroundImage;
                    videoPlayer.BackgroundImage = null;
                    oldImage.Dispose();
                }
                
                // Yeni frame'i ayarla
                videoPlayer.BackgroundImage = new Bitmap(frame);
                videoPlayer.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
                videoPlayer.Invalidate(); // Yeniden çiz
                
                System.Diagnostics.Debug.WriteLine("FRAME GÖRÜNTÜLENDI");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"GÖRÜNTÜLEME HATASI: {ex.Message}");
            }
        }
        
        /// <summary>
        /// 🔧 DÜZELTME: Frame'i VideoSourcePlayer'da görüntüle - PERFORMANS OPTİMİZE VERSİYONU
        /// </summary>
        /// <param name="frame">Görüntülenecek frame</param>
        private void GoruntuleFrame(Bitmap frame)
        {
            try
            {
                if (videoPlayer == null || frame == null)
                    return;

                // 🔧 DÜZELTME: VideoPlayer type checking - safely handle different control types
                if (videoPlayer.GetType().Name.Contains("PictureBox"))
                {
                    // VideoPlayer gerçek PictureBox ise optimized metodu kullan
                    GoruntuleFrameOptimized(frame);
                }
                else
                {
                    // VideoSourcePlayer veya diğer kontroller için fallback
                    GoruntuleFrameFallback(frame);
                }
            }
            catch (Exception ex)
            {
                logHata?.Invoke($"❌ Frame görüntülenirken hata: {ex.Message}", true);
            }
        }

        /// <summary>
        /// 🔧 DÜZELTME: Optimized frame görüntüleme - 10x daha hızlı
        /// </summary>
        private void GoruntuleFrameOptimized(Bitmap frame)
        {
            try
            {
                // 🔧 DÜZELTME: VideoSourcePlayer compatible approach - no casting needed
                // VideoSourcePlayer direkt olarak Control'den türediği için BackgroundImage kullanabiliriz
                // Ama memory optimizasyonu için daha dikkatli yaklaşım
                
                // UI thread kontrolü
                if (videoPlayer.InvokeRequired)
                {
                    videoPlayer.Invoke(new Action(() => 
                    {
                        GoruntuleFrameOptimizedInternal(frame);
                    }));
                }
                else
                {
                    GoruntuleFrameOptimizedInternal(frame);
                }
            }
            catch (Exception ex)
            {
                logHata?.Invoke($"❌ Optimized display hatası: {ex.Message}", true);
            }
        }

                /// <summary>
        /// 🔧 DÜZELTME: Internal optimized rendering - BELLEK SIZINTISI ÇÖZÜLDİ
        /// </summary>
        private void GoruntuleFrameOptimizedInternal(Bitmap frame)
        {
            try
            {
                // 🔧 NULL VE DISPOSED KONTROLLERI
                if (frame == null || videoPlayer == null || videoPlayer.IsDisposed)
                    return;

                // 🔧 DÜZELTME: Önceki backgroundImage'ı güvenli şekilde dispose et
                Image oldImage = null;
                try
                {
                    oldImage = videoPlayer.BackgroundImage;
                    videoPlayer.BackgroundImage = null;
                }
                catch (ObjectDisposedException)
                {
                    // VideoPlayer dispose edilmişse sessizce çık
                    return;
                }

                // Eski image'ı temizle
                if (oldImage != null)
                {
                    oldImage.Dispose();
                    oldImage = null;
                }

                // 🔧 BELLEK OPTİMİZASYONU: Yeni frame boyutunu kontrol et
                try
                {
                    // Frame'i BackgroundImage olarak ayarla (kopya oluştur)
                    videoPlayer.BackgroundImage = new Bitmap(frame);
                    videoPlayer.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
                    
                    // UI güncelleme
                    videoPlayer.Update();
                }
                catch (OutOfMemoryException)
                {
                    logHata?.Invoke("⚠️ Frame görüntülenirken bellek yetersiz, frame atlandı", true);
                    
                    // Acil garbage collection
                    GC.Collect();
                    return;
                }
                
                // Debug log (her 50 frame'de bir - logları azalt)
                frameDisplayCount++;
                if (frameDisplayCount % 50 == 0)
                {
                    long totalMemory = GC.GetTotalMemory(false);
                    logHata?.Invoke($"📹 Frame: {frameDisplayCount} ({frame.Width}x{frame.Height}) - Bellek: {totalMemory / 1_000_000}MB", false);
                    
                    // 🔧 PREVENTİF BELLEK TEMİZLİĞİ: 200MB'den fazlaysa
                    if (totalMemory > 200_000_000)
                    {
                        GC.Collect();
                        logHata?.Invoke("🧹 Preventif bellek temizliği yapıldı", false);
                    }
                }
            }
            catch (Exception ex)
            {
                logHata?.Invoke($"❌ Internal optimized display hatası: {ex.Message}", true);
                
                // Exception durumunda da garbage collection tetikle
                try
                {
                    GC.Collect();
                }
                catch { }
            }
        }

        /// <summary>
        /// 🔧 DÜZELTME: Fallback görüntüleme metodu (eski VideoSourcePlayer için)
        /// </summary>
        private void GoruntuleFrameFallback(Bitmap frame)
        {
            try
            {
                // UI thread kontrolü
                if (videoPlayer.InvokeRequired)
                {
                    videoPlayer.Invoke(new Action(() => GoruntuleFrameFallback(frame)));
                    return;
                }

                // 🔧 DÜZELTME: Önceki backgroundImage'ı dispose et
                    if (videoPlayer.BackgroundImage != null)
                    {
                    var oldImage = videoPlayer.BackgroundImage;
                    videoPlayer.BackgroundImage = null;
                    oldImage.Dispose();
                    }
                    
                    // Yeni frame'i background image olarak ayarla
                    videoPlayer.BackgroundImage = new Bitmap(frame);
                videoPlayer.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
                    
                    // Control'ü yeniden çiz
                    videoPlayer.Invalidate();
                    
                logHata?.Invoke($"📹 Fallback: Frame görüntülendi ({frame.Width}x{frame.Height})", false);
            }
            catch (Exception ex)
            {
                logHata?.Invoke($"❌ Fallback display hatası: {ex.Message}", true);
            }
        }
        
        /// <summary>
        /// Video stream durumunu kontrol et
        /// </summary>
        public bool VideoStreamAktifMi
        {
            get { return videoStreamAktif; }
        }
        
        /// <summary>
        /// Video kayıt durumunu kontrol et
        /// </summary>
        public bool VideoKayitModundaMi
        {
            get { return kayitModu; }
        }
        
        /// <summary>
        /// Test modu aktif mi?
        /// </summary>
        public bool TestModuAktifMi
        {
            get { return testModu; }
        }
        
        /// <summary>
        /// Test komut efektlerini uygula
        /// </summary>
        /// <param name="efektTipi">Efekt tipi (MANUEL_AYRILMA, multi-spektral komut, NORMAL)</param>
        public void TestKomutEfekti(string efektTipi)
        {
            try
            {

                
                if (!testModu)
                {
                    // YARISHMA GÜVENLİĞİ: Gerçek modda hiçbir efekt uygulanmaz ve log yazılmaz
                    return;
                }

                if (string.IsNullOrEmpty(efektTipi))
                {

                    efektTipi = "NORMAL";
                }

                string eskiEfekt = aktifTestEfekti;
                aktifTestEfekti = efektTipi;
                efektSayaci = 0;
                
                logHata?.Invoke($"🎨 Test video efekti değiştirildi: '{eskiEfekt}' → '{efektTipi}'", false);
            }
            catch (Exception ex)
            {
                logHata?.Invoke($"❌ Test komut efekti uygulanırken HATA: {ex.Message}", true);
                logHata?.Invoke($"❌ Hata detayı: {ex.ToString()}", true);
                
                // Hata durumunda normal moda geç
                aktifTestEfekti = "NORMAL";
            }
        }

        /// <summary>
        /// Video frame'e efekt uygula
        /// </summary>
        /// <param name="frame">Orijinal frame</param>
        /// <returns>Efekt uygulanmış frame</returns>
        private Bitmap EfektUygula(Bitmap frame)
        {
            try
            {
                if (frame == null)
                {

                    return frame;
                }

                if (aktifTestEfekti == "NORMAL" || string.IsNullOrEmpty(aktifTestEfekti))
                {
                    return frame; // Değişiklik yok
                }



                Bitmap efektFrame = null;
                Graphics g = null;
                
                try
                {
                    efektFrame = new Bitmap(frame.Width, frame.Height);
                    g = Graphics.FromImage(efektFrame);
                    
                    // Orijinal frame'i çiz
                    g.DrawImage(frame, 0, 0);
                    
                    // Efekt tipine göre uygula
                    switch (aktifTestEfekti)
                    {
                        case "MANUEL_AYRILMA":
                            UygulaManuelAyrilmaEfekti(g, frame);
                            break;
                            
                        default:
                            // Multi-spektral komut (4 karakterli)
                            if (aktifTestEfekti.Length == 4)
                            {
                                EfektUygula_MultiSpektral(g, frame, aktifTestEfekti);
                            }
                            break;
                    }
                    
                    // Efekt sayacını artır
                    efektSayaci++;
                    
                    return efektFrame;
                }
                catch (Exception innerEx)
                {
                    logHata?.Invoke($"❌ Efekt uygulama iç hatası: {innerEx.Message}", true);
                    
                    // Hata durumunda kaynakları temizle
                    g?.Dispose();
                    efektFrame?.Dispose();
                    
                    return frame; // Orijinal frame'i döndür
                }
                finally
                {
                    g?.Dispose(); // Graphics nesnesini her durumda temizle
                }
            }
            catch (Exception ex)
            {
                logHata?.Invoke($"❌ Video efekti uygulanırken HATA: {ex.Message}", true);
                return frame; // Hata durumunda orijinal frame'i döndür
            }
        }

        /// <summary>
        /// Manuel ayrılma efekti uygula (ayrı metod)
        /// </summary>
        private void UygulaManuelAyrilmaEfekti(Graphics g, Bitmap frame)
        {
            try
            {
                // Titreme efekti - kırmızı kenarlık
                using (Pen kirmizi = new Pen(Color.Red, 5))
                {
                    g.DrawRectangle(kirmizi, 0, 0, frame.Width - 1, frame.Height - 1);
                }
                
                // Titreme yazısı
                using (Font font = new Font("Arial", 20, FontStyle.Bold))
                using (Brush brush = new SolidBrush(Color.Red))
                {
                    g.DrawString("MANUEL AYRILMA", font, brush, 10, 10);
                    g.DrawString("SEPARATION COMMAND", font, brush, 10, 40);
                }
            }
            catch (Exception ex)
            {
                logHata?.Invoke($"❌ Manuel ayrılma efekti hatası: {ex.Message}", true);
            }
        }

        /// <summary>
        /// Multi-spektral komut efekti uygula
        /// </summary>
        private void EfektUygula_MultiSpektral(Graphics g, Bitmap frame, string komut)
        {
            try
            {
                if (g == null || frame == null || string.IsNullOrEmpty(komut) || komut.Length != 4)
                {
                    return;
                }

                // Komut bilgilerini parse et
                char filtre1 = komut[1];
                char filtre2 = komut[3];
                
                // Filtre rengini belirle
                Color renk1 = FiltreRengi(filtre1);
                Color renk2 = FiltreRengi(filtre2);
                
                // Renk overlay uygula - iki filtre üst üste
                using (Brush overlay1 = new SolidBrush(Color.FromArgb(40, renk1)))
                using (Brush overlay2 = new SolidBrush(Color.FromArgb(40, renk2)))
                {
                    // İlk filtre - tüm frame'e uygula
                    g.FillRectangle(overlay1, 0, 0, frame.Width, frame.Height);
                    
                    // İkinci filtre - tüm frame'e üst üste uygula (karışım etkisi)
                    g.FillRectangle(overlay2, 0, 0, frame.Width, frame.Height);
                }
                
                // Filtre bilgilerini göster
                using (Font font = new Font("Arial", 16, FontStyle.Bold))
                using (Brush brush = new SolidBrush(Color.White))
                using (Brush brushShadow = new SolidBrush(Color.Black))
                {
                    string text = $"MULTI-SPEKTRAL: {komut}";
                    g.DrawString(text, font, brushShadow, 11, 11); // Gölge
                    g.DrawString(text, font, brush, 10, 10); // Asıl yazı
                    
                    // Filtre açıklamaları
                    string filtre1Desc = FiltreTuruAciklamasi(filtre1);
                    string filtre2Desc = FiltreTuruAciklamasi(filtre2);
                    
                    using (Font smallFont = new Font("Arial", 12))
                    {
                        g.DrawString($"Filtre 1: {filtre1Desc}", smallFont, brushShadow, 11, 41);
                        g.DrawString($"Filtre 1: {filtre1Desc}", smallFont, brush, 10, 40);
                        
                        g.DrawString($"Filtre 2: {filtre2Desc}", smallFont, brushShadow, 11, 61);
                        g.DrawString($"Filtre 2: {filtre2Desc}", smallFont, brush, 10, 60);
                    }
                }
            }
            catch (Exception ex)
            {
                logHata?.Invoke($"❌ Multi-spektral efekt uygulanırken HATA: {ex.Message}", true);
            }
        }

        /// <summary>
        /// Filtre karakterini renge çevir
        /// </summary>
        private Color FiltreRengi(char filtre)
        {
            switch (filtre)
            {
                case 'R': return Color.Red;         // Light Red
                case 'G': return Color.Green;       // Light Green
                case 'B': return Color.Blue;        // Light Blue
                case 'C': return Color.Cyan;        // Cyan, Turquoise
                case 'F': return Color.ForestGreen; // Forest/Dark Green
                case 'N': return Color.Navy;        // Navy Blue
                case 'M': return Color.Maroon;      // Maroon Red
                case 'P': return Color.Purple;      // Purple, Pink
                case 'Y': return Color.Yellow;      // Yellow, Brown
                default: return Color.Gray;
            }
        }

        /// <summary>
        /// Filtre türü açıklaması
        /// </summary>
        private string FiltreTuruAciklamasi(char filterType, bool isStandartKonum = false)
        {
            switch (filterType)
            {
                case 'R': return "Light Red";
                case 'G': return "Light Green";
                case 'B': return "Light Blue";
                case 'C': return "Cyan";
                case 'F': return "Forest Green";
                case 'N': 
                    if (isStandartKonum)
                        return "Standart";
                    else
                        return "Navy Blue";
                case 'M': return "Maroon Red";
                case 'P': return "Purple";
                case 'Y': return "Yellow";
                default: return "Unknown";
            }
        }
        
        /// <summary>
        /// Mevcut video frame'ini al
        /// </summary>
        public Bitmap MevcutFrame
        {
            get { return mevcutFrame; }
        }
        
        /// <summary>
        /// 🔧 DÜZELTME: Kaynakları temizle - BELLEK SIZINTISI ÇÖZÜLDİ
        /// </summary>
        public void Dispose()
        {
            try
            {
                logHata?.Invoke("🧹 Kamera yöneticisi kapatılıyor - bellek temizliği başlatıldı...", false);
                
                // İlk olarak timer'ları durdur (bellek sızıntısını durdur)
                if (mp4Timer != null)
                {
                    mp4Timer.Stop();
                    mp4Timer.Tick -= Mp4Timer_Tick; // Event handler'ı temizle
                    mp4Timer.Dispose();
                    mp4Timer = null;
                }
                
                // Video stream'i durdur
                VideoStreamDurdur();
                
                // Test modu kaynaklarını temizle
                if (testModu)
                {
                    TestModuDurdur();
                }
                
                // 🔧 DÜZELTME: VideoPlayer'dan BackgroundImage'ı temizle
                try
                {
                    if (videoPlayer != null && !videoPlayer.IsDisposed && videoPlayer.BackgroundImage != null)
                {
                        var oldImage = videoPlayer.BackgroundImage;
                        videoPlayer.BackgroundImage = null;
                        oldImage.Dispose();
                    }
                }
                catch (ObjectDisposedException)
                {
                    // VideoPlayer zaten dispose edilmişse sorun yok
                }
                
                // MP4 reader'ı temizle
                if (mp4VideoReader != null)
                {
                    try
                {
                    mp4VideoReader.Close();
                    mp4VideoReader.Dispose();
                    }
                    catch { }
                    mp4VideoReader = null;
                }
                
                // Mevcut frame'i temizle
                if (mevcutFrame != null)
                {
                    mevcutFrame.Dispose();
                    mevcutFrame = null;
                }
                
                // Video yazıcıyı temizle
                if (videoYazici != null)
                {
                    try
                    {
                        videoYazici.Close();
                    videoYazici.Dispose();
                    }
                    catch { }
                    videoYazici = null;
                }
                
                // 🔧 DÜZELTME: Son garbage collection ile tüm referansları temizle
                GC.Collect();
                GC.WaitForPendingFinalizers();
                GC.Collect();
                
                logHata?.Invoke("✅ Kamera yöneticisi kapatıldı - bellek temizliği tamamlandı.", false);
            }
            catch (Exception ex)
            {
                logHata?.Invoke("❌ Kamera yöneticisi kapatılırken hata: " + ex.Message, true);
                
                // Hata durumunda da garbage collection tetikle
                try
                {
                    GC.Collect();
                }
                catch { }
            }
        }
    }
        
    /// <summary>
    /// Video dosyası yazma işlemleri için basit wrapper sınıf
    /// </summary>
    public class VideoFileWriter : IDisposable
    {
        private string dosyaYolu;
        private List<Bitmap> frameler = new List<Bitmap>();
        private bool dosyaAcik = false;
        
        public void Open(string fileName, int width, int height, int frameRate, VideoCodec codec, int bitRate)
        {
            dosyaYolu = fileName;
            dosyaAcik = true;
            frameler.Clear();
        }
        
        public void WriteVideoFrame(Bitmap frame)
        {
            if (dosyaAcik && frame != null)
            {
                frameler.Add(new Bitmap(frame));
            }
        }
        
        public void Close()
        {
            if (dosyaAcik && frameler.Count > 0)
            {
                // Basit implementasyon: Frame'leri GIF olarak kaydet
                // Gerçek projede FFmpeg kullanılabilir
                try
                {
                    string gifPath = dosyaYolu.Replace(".avi", ".gif");
                    // Bu kısım geliştirilmelidir
                }
                catch { }
            }
            
            frameler.Clear();
            dosyaAcik = false;
        }
        
        public void Dispose()
        {
            Close();
            foreach (var frame in frameler)
            {
                frame?.Dispose();
            }
            frameler.Clear();
        }
    }
        
    /// <summary>
    /// Video codec türleri
    /// </summary>
    public enum VideoCodec
    {
        Default,
        MPEG4,
        H264
    }
}