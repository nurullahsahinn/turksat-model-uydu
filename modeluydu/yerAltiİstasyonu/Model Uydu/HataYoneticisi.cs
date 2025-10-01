using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ModelUydu
{
    /// <summary>
    /// Uygulama genelinde hata yönetimi, loglama ve bildirim işlemlerini gerçekleştiren sınıf.
    /// </summary>
    public class HataYoneticisi
    {
        // Hata mesajlarını saklamak için kuyruk yapısı
        private Queue<string> hataBildirimKuyrugu = new Queue<string>();
        
        // Zamanlayıcı - hata mesajlarını göstermek için
        private System.Windows.Forms.Timer hataBildirimZamanlayici;
        
        // Hata log dosyasının yolu
        private string hataLogDosyaYolu = "hata_log.txt";
        
        // Form referansı (UI thread'inde mesaj göstermek için)
        private Form anaForm;

        /// <summary>
        /// HataYoneticisi sınıfı yapıcı metodu
        /// </summary>
        /// <param name="anaForm">Ana form referansı</param>
        /// <param name="logDosyaYolu">Hata log dosyasının yolu (opsiyonel)</param>
        public HataYoneticisi(Form anaForm, string logDosyaYolu = null)
        {
            this.anaForm = anaForm;
            
            // Log dosya yolunu ayarla
            if (!string.IsNullOrEmpty(logDosyaYolu))
            {
                this.hataLogDosyaYolu = logDosyaYolu;
            }

            // Zamanlayıcıyı başlat
            hataBildirimZamanlayici = new System.Windows.Forms.Timer();
            hataBildirimZamanlayici.Interval = 100; // 100 ms
            hataBildirimZamanlayici.Tick += new EventHandler(HataBildirimZamanlayici_Tick);
            
            // Log dosyasının var olup olmadığını kontrol et
            if (!File.Exists(hataLogDosyaYolu))
            {
                try
                {
                    // Dosya yoksa oluştur
                    using (StreamWriter sw = new StreamWriter(hataLogDosyaYolu))
                    {
                        sw.WriteLine("Zaman - Hata Bilgisi");
                    }
                    
                    Log("Hata log dosyası oluşturuldu: " + hataLogDosyaYolu, false);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Hata log dosyası oluşturulurken hata: " + ex.Message);
                }
            }
        }

        /// <summary>
        /// Hata bildirim zamanlayıcısı tick olayı - mesaj kuyruğundan hataları gösterir
        /// </summary>
        private void HataBildirimZamanlayici_Tick(object sender, EventArgs e)
        {
            // 🔧 POPUP DEVRE DIŞI - Sadece log'a yaz, popup gösterme
            if (hataBildirimKuyrugu.Count > 0)
            {
                string hataMesaji = hataBildirimKuyrugu.Dequeue();
                // TÜM POPUP'LAR TAMAMEN KAPALI - Sadece log
                System.Diagnostics.Debug.WriteLine($"HATA: {hataMesaji}");
            }
            else
            {
                // Kuyruk boşsa zamanlayıcıyı durdur
                hataBildirimZamanlayici.Stop();
            }
        }

        /// <summary>
        /// Hata mesajını loglayan ve isteğe bağlı olarak kullanıcıya bildiren metod
        /// </summary>
        /// <param name="hataMesaji">Hata mesajı</param>
        /// <param name="kritikMi">Kritik hata ise kullanıcıya bildirilir</param>
        public void Log(string hataMesaji, bool kritikMi = false)
        {
            try
            {
                // Hata zamanını al
                string zaman = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                
                // Hata mesajını oluştur
                string logMesaji = $"{zaman} - {hataMesaji}";
                
                // Konsola yaz
                Console.WriteLine(logMesaji);
                
                // Log dosyasına kaydet
                using (StreamWriter sw = new StreamWriter(hataLogDosyaYolu, true))
                {
                    sw.WriteLine(logMesaji);
                }
                
                // Kritik hataysa kullanıcıya bildir
                if (kritikMi)
                {
                    // UI thread'inde çalışmıyorsak Invoke kullan
                    if (anaForm.InvokeRequired)
                    {
                        anaForm.Invoke(new Action(() =>
                        {
                            hataBildirimKuyrugu.Enqueue(hataMesaji);
                            if (!hataBildirimZamanlayici.Enabled)
                            {
                                hataBildirimZamanlayici.Start();
                            }
                        }));
                    }
                    else
                    {
                        hataBildirimKuyrugu.Enqueue(hataMesaji);
                        if (!hataBildirimZamanlayici.Enabled)
                        {
                            hataBildirimZamanlayici.Start();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Loglama sırasında hata oluşursa konsola yaz
                Console.WriteLine("Hata loglanırken hata oluştu: " + ex.Message);
            }
        }

        /// <summary>
        /// Bilgi mesajı loglar (kritik olmayan log)
        /// </summary>
        /// <param name="bilgiMesaji">Bilgi mesajı</param>
        public void LogBilgi(string bilgiMesaji)
        {
            Log("BİLGİ: " + bilgiMesaji, false);
        }

        /// <summary>
        /// Uyarı mesajı loglar (kritik olmayan ama dikkat edilmesi gereken durumlar)
        /// </summary>
        /// <param name="uyariMesaji">Uyarı mesajı</param>
        public void LogUyari(string uyariMesaji)
        {
            Log("UYARI: " + uyariMesaji, false);
        }

        /// <summary>
        /// Hata mesajı loglar ve kullanıcıya bildirir (kritik hatalar)
        /// </summary>
        /// <param name="hataMesaji">Hata mesajı</param>
        public void LogHata(string hataMesaji)
        {
            Log("HATA: " + hataMesaji, true);
        }

        /// <summary>
        /// İstisnai durum loglar (beklenmeyen hatalar, exception durumları)
        /// </summary>
        /// <param name="ex">Exception nesnesi</param>
        /// <param name="kaynakMetod">Hatanın oluştuğu metod adı</param>
        /// <param name="kritikMi">Kritik hata ise kullanıcıya bildirilir</param>
        public void LogException(Exception ex, string kaynakMetod, bool kritikMi = true)
        {
            string hataMesaji = $"İSTİSNA [{kaynakMetod}]: {ex.Message}";
            
            // İç hata (InnerException) varsa ekle
            if (ex.InnerException != null)
            {
                hataMesaji += $" | İç Hata: {ex.InnerException.Message}";
            }
            
            Log(hataMesaji, kritikMi);
        }
        
        /// <summary>
        /// Kullanıcı işlemini loglar (önemli kullanıcı eylemleri)
        /// </summary>
        /// <param name="islemMesaji">İşlem mesajı</param>
        public void LogKullaniciIslemi(string islemMesaji)
        {
            Log("KULLANICI İŞLEMİ: " + islemMesaji, false);
        }
        
        /// <summary>
        /// Hata kayıt dosyasının mevcut içeriğini gösterir (sistemin varsayılan metin düzenleyicisinde açar)
        /// </summary>
        public void HataLogDosyasiniGoruntule()
        {
            try
            {
                if (File.Exists(hataLogDosyaYolu))
                {
                    System.Diagnostics.Process.Start("notepad.exe", hataLogDosyaYolu);
                }
                else
                {
                    MessageBox.Show(anaForm, "Hata log dosyası bulunamadı: " + hataLogDosyaYolu, 
                        "Dosya Bulunamadı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            catch (Exception ex)
            {
                Log("Hata log dosyası açılırken hata oluştu: " + ex.Message, true);
            }
        }
        
        /// <summary>
        /// Hata log dosyasını temizler (yedekleyerek)
        /// </summary>
        public void HataLogDosyasiniTemizle()
        {
            try
            {
                if (File.Exists(hataLogDosyaYolu))
                {
                    // Eski log dosyasını tarih eklenerek yedekle
                    string yedekDosyaAdi = Path.GetFileNameWithoutExtension(hataLogDosyaYolu) + 
                        "_" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + Path.GetExtension(hataLogDosyaYolu);
                    
                    string yedekDosyaYolu = Path.Combine(Path.GetDirectoryName(hataLogDosyaYolu), yedekDosyaAdi);
                    
                    // Dosyayı yedekle
                    File.Copy(hataLogDosyaYolu, yedekDosyaYolu);
                    
                    // Log dosyasını temizle ve başlık satırını ekle
                    using (StreamWriter sw = new StreamWriter(hataLogDosyaYolu, false))
                    {
                        sw.WriteLine("Zaman - Hata Bilgisi");
                    }
                    
                    Log("Hata log dosyası temizlendi. Yedek: " + yedekDosyaAdi, false);
                }
            }
            catch (Exception ex)
            {
                Log("Hata log dosyası temizlenirken hata: " + ex.Message, true);
            }
        }

        /// <summary>
        /// Kullanıcıya bilgi mesajı gösterir (MessageBox ile)
        /// </summary>
        /// <param name="mesaj">Gösterilecek mesaj</param>
        /// <param name="baslik">Mesaj kutusu başlığı</param>
        public void BilgiGoster(string mesaj, string baslik = "Bilgi")
        {
            try
            {
                // UI thread'inde çalışmıyorsak Invoke kullan
                if (anaForm.InvokeRequired)
                {
                    anaForm.Invoke(new Action(() =>
                    {
                        MessageBox.Show(anaForm, mesaj, baslik, MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }));
                }
                else
                {
                    MessageBox.Show(anaForm, mesaj, baslik, MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                Log("Bilgi mesajı gösterilirken hata: " + ex.Message, false);
            }
        }

        /// <summary>
        /// Kullanıcıya uyarı mesajı gösterir (MessageBox ile)
        /// </summary>
        /// <param name="mesaj">Gösterilecek mesaj</param>
        /// <param name="baslik">Mesaj kutusu başlığı</param>
        public void UyariGoster(string mesaj, string baslik = "Uyarı")
        {
            try
            {
                // UI thread'inde çalışmıyorsak Invoke kullan
                if (anaForm.InvokeRequired)
                {
                    anaForm.Invoke(new Action(() =>
                    {
                        MessageBox.Show(anaForm, mesaj, baslik, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }));
                }
                else
                {
                    MessageBox.Show(anaForm, mesaj, baslik, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            catch (Exception ex)
            {
                Log("Uyarı mesajı gösterilirken hata: " + ex.Message, false);
            }
        }

        /// <summary>
        /// Kullanıcıya hata mesajı gösterir (MessageBox ile)
        /// </summary>
        /// <param name="mesaj">Gösterilecek mesaj</param>
        /// <param name="baslik">Mesaj kutusu başlığı</param>
        public void HataGoster(string mesaj, string baslik = "Hata")
        {
            try
            {
                // UI thread'inde çalışmıyorsak Invoke kullan
                if (anaForm.InvokeRequired)
                {
                    anaForm.Invoke(new Action(() =>
                    {
                        MessageBox.Show(anaForm, mesaj, baslik, MessageBoxButtons.OK, MessageBoxIcon.Error);
                        System.Diagnostics.Debug.WriteLine($"HATA: {mesaj}");
                    }));
                }
                else
                {
                    MessageBox.Show(anaForm, mesaj, baslik, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    System.Diagnostics.Debug.WriteLine($"HATA: {mesaj}");
                }
            }
            catch (Exception ex)
            {
                Log("Hata mesajı gösterilirken hata: " + ex.Message, false);
            }
        }
        
        /// <summary>
        /// Kullanıcıya onay sorusu sorar (MessageBox ile)
        /// </summary>
        /// <param name="mesaj">Soru mesajı</param>
        /// <param name="baslik">Mesaj kutusu başlığı</param>
        /// <returns>Kullanıcı Evet'e tıklarsa true, aksi halde false</returns>
        public bool OnayIste(string mesaj, string baslik = "Onay")
        {
            try
            {
                // UI thread'inde çalışıyorsak direkt sor
                if (!anaForm.InvokeRequired)
                {
                    return MessageBox.Show(anaForm, mesaj, baslik, 
                        MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes;
                }
                
                // UI thread'inde çalışmıyorsak Invoke kullan
                bool sonuc = false;
                anaForm.Invoke(new Action(() =>
                {
                    sonuc = MessageBox.Show(anaForm, mesaj, baslik, 
                        MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes;
                }));
                
                return sonuc;
            }
            catch (Exception ex)
            {
                Log("Onay istenirken hata: " + ex.Message, false);
                return false;
            }
        }
        
        /// <summary>
        /// Kaynakları serbest bırakır
        /// </summary>
        public void Dispose()
        {
            // Zamanlayıcıyı durdur ve serbest bırak
            if (hataBildirimZamanlayici != null)
            {
                hataBildirimZamanlayici.Stop();
                hataBildirimZamanlayici.Dispose();
                hataBildirimZamanlayici = null;
            }
            
            // Mesaj kuyruğunu temizle
            hataBildirimKuyrugu.Clear();
        }
    }
} 