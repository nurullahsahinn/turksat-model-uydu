using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Media;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ModelUydu
{
    /// <summary>
    /// ARAS (Arayüz Alarm Sistemi) sınıfı - Hata kodlarını işleme, sesli uyarıları yönetme ve hata panellerini güncelleme işlevlerine sahiptir
    /// </summary>
    public class AlarmSistemiYoneticisi
    {
        // Hata kodu ve durum değişkenleri
        private string hataKodu = "000000"; // Varsayılan olarak hata yok (6 haneli kod)
        private string oncekiHataKodu = "000000"; // Önceki hata kodunu takip etmek için
        
        // Modern ARAS hata ses sistemi
        private SoundPlayer[] arasSesleri = new SoundPlayer[6];
        private string[] arasSesDosyalari = {
            "aras_hata_hiz_model.wav",          // Hata 1: Model Uydu Hız
            "aras_hata_hiz_gorev.wav",          // Hata 2: Görev Yükü Hız
            "aras_hata_basinc.wav",             // Hata 3: Basınç Sensörü
            "aras_hata_gps.wav",                // Hata 4: GPS Sinyali
            "aras_hata_ayrilma.wav",            // Hata 5: Ayrılma Mekanizması
            "aras_hata_multispektral.wav"       // Hata 6: Multi-Spektral Sistem
        };
        private string[] arasHataAciklamalari = {
            "Model Uydu İniş Hızı Hatası",
            "Görev Yükü İniş Hızı Hatası", 
            "Taşıyıcı Basınç Verisi Hatası",
            "Görev Yükü Konum Verisi Hatası",
            "Ayrılma Durumu Hatası",
            "Multi-Spektral Sistem Hatası"
        };
        
        // Genel alarm sistemi kaldırıldı, artık her hatanın kendi özel sesi var.
        private bool sesliUyariAktif = false;

        // UI kontrolleri
        private Label labelHataKodu;
        private CheckBox checkBoxSesliUyari;

        // Hata panelleri (ana panel)
        private Panel panelHata1;
        private Panel panelHata2;
        private Panel panelHata3;
        private Panel panelHata4;
        private Panel panelHata5;
        private Panel panelHata6;

        // Alarm panelleri (özet panel)
        private Panel panelAlarm1;
        private Panel panelAlarm2;
        private Panel panelAlarm3;
        private Panel panelAlarm4;
        private Panel panelAlarm5;
        private Panel panelAlarm6;

        // Hata mesajları için callback fonksiyonu
        private Action<string, bool> logHataFunc;

        /// <summary>
        /// AlarmSistemiYoneticisi sınıfının yapıcı metodu
        /// </summary>
        /// <param name="labelHataKodu">Hata kodu etiketi</param>
        /// <param name="checkBoxSesliUyari">Sesli uyarı onay kutusu</param>
        /// <param name="panelHata1">Taşıyıcı İniş Hızı panel</param>
        /// <param name="panelHata2">Görev Yükü İniş Hızı panel</param>
        /// <param name="panelHata3">Taşıyıcı Basınç panel</param>
        /// <param name="panelHata4">Görev Yükü Konum panel</param>
        /// <param name="panelHata5">Ayrılma Durumu panel</param>
        /// <param name="panelHata6">Multi-Spektral Sistem panel</param>
        /// <param name="panelAlarm1">Taşıyıcı İniş Hızı alarm panel</param>
        /// <param name="panelAlarm2">Görev Yükü İniş Hızı alarm panel</param>
        /// <param name="panelAlarm3">Taşıyıcı Basınç alarm panel</param>
        /// <param name="panelAlarm4">Görev Yükü Konum alarm panel</param>
        /// <param name="panelAlarm5">Ayrılma Durumu alarm panel</param>
        /// <param name="panelAlarm6">Multi-Spektral Sistem alarm panel</param>
        /// <param name="logHataFunc">Hata loglama fonksiyonu</param>
        public AlarmSistemiYoneticisi(
            Label labelHataKodu,
            CheckBox checkBoxSesliUyari,
            Panel panelHata1, Panel panelHata2, Panel panelHata3, 
            Panel panelHata4, Panel panelHata5, Panel panelHata6,
            Panel panelAlarm1, Panel panelAlarm2, Panel panelAlarm3, 
            Panel panelAlarm4, Panel panelAlarm5, Panel panelAlarm6,
            Action<string, bool> logHataFunc)
        {
            this.labelHataKodu = labelHataKodu;
            this.checkBoxSesliUyari = checkBoxSesliUyari;
            
            // Hata panelleri
            this.panelHata1 = panelHata1;
            this.panelHata2 = panelHata2;
            this.panelHata3 = panelHata3;
            this.panelHata4 = panelHata4;
            this.panelHata5 = panelHata5;
            this.panelHata6 = panelHata6;
            
            // Alarm panelleri
            this.panelAlarm1 = panelAlarm1;
            this.panelAlarm2 = panelAlarm2;
            this.panelAlarm3 = panelAlarm3;
            this.panelAlarm4 = panelAlarm4;
            this.panelAlarm5 = panelAlarm5;
            this.panelAlarm6 = panelAlarm6;
            
            // Hata loglama fonksiyonu
            this.logHataFunc = logHataFunc;

            // Başlangıçta tüm hata panellerini yeşil yap
            SifirlaPaneller();

            // CheckBox olay işleyicisini ekle
            this.checkBoxSesliUyari.CheckedChanged += CheckBoxSesliUyari_CheckedChanged;
        }

        /// <summary>
        /// Hata panellerini başlangıç durumuna getir (tümü yeşil)
        /// </summary>
        public void SifirlaPaneller()
        {
            GuncelleHataPaneli(panelHata1, '0');
            GuncelleHataPaneli(panelHata2, '0');
            GuncelleHataPaneli(panelHata3, '0');
            GuncelleHataPaneli(panelHata4, '0');
            GuncelleHataPaneli(panelHata5, '0');
            GuncelleHataPaneli(panelHata6, '0');

            GuncelleHataPaneli(panelAlarm1, '0');
            GuncelleHataPaneli(panelAlarm2, '0');
            GuncelleHataPaneli(panelAlarm3, '0');
            GuncelleHataPaneli(panelAlarm4, '0');
            GuncelleHataPaneli(panelAlarm5, '0');
            GuncelleHataPaneli(panelAlarm6, '0');

            // Hata kodunu varsayılan değere ayarla ve etiketi güncelle
            hataKodu = "000000";
            labelHataKodu.Text = "<" + hataKodu + ">";
        }

        /// <summary>
        /// Sesli uyarı dosyalarını yükle (6 farklı ARAS hata sesi)
        /// </summary>
        private void YukleSesliUyari()
        {
            try
            {
                // Genel alarm sesi kaldırıldı.
                
                // 6 farklı ARAS hata sesini yükle
                for (int i = 0; i < 6; i++)
                {
                    try
                    {
                        if (System.IO.File.Exists(arasSesDosyalari[i]))
                        {
                            arasSesleri[i] = new SoundPlayer(arasSesDosyalari[i]);
                            logHataFunc($"✅ ARAS Ses {i + 1} yüklendi: {arasSesDosyalari[i]}", false);
                        }
                        else
                        {
                            logHataFunc($"Uyarı: ARAS ses dosyası bulunamadı: {arasSesDosyalari[i]}. Console.Beep kullanılacak.", false);
                            arasSesleri[i] = null; // null = Console.Beep kullanılacak
                        }
                    }
                    catch (Exception exAras)
                    {
                        logHataFunc($"❌ ARAS Ses {i + 1} yüklenirken hata: {exAras.Message}", true);
                        arasSesleri[i] = null;
                    }
                }
            }
            catch (Exception ex)
            {
                logHataFunc("Sesli uyarı sistemi başlatılamadı: " + ex.Message, true);
                for (int i = 0; i < 6; i++) arasSesleri[i] = null;
            }
        }

        /// <summary>
        /// Hata kodunu güncelleyen ve görsel/sesli uyarıları ayarlayan metod
        /// </summary>
        /// <param name="yeniHataKodu">Yeni hata kodu (6 haneli)</param>
        public void HataKoduGuncelle(string yeniHataKodu)
        {
            // Sadece hata kodu değiştiyse güncelleme yap
            if (hataKodu != yeniHataKodu)
            {
                oncekiHataKodu = hataKodu; // Önceki hata kodunu kaydet
                hataKodu = yeniHataKodu; // Yeni hata kodunu kaydet
                labelHataKodu.Text = "<" + hataKodu + ">"; // Hata kodu etiketini güncelle

                // 🔧 BIT POZİSYONLARI DÜZELTİLDİ - Arduino (YerAltiSistem.ino) ile tam uyumlu hale getirildi
                // Arduino Çıktısı: "{hata1}{hata2}{hata3}{hata4}{hata5}{hata6}"
                // String indeks:      [0]   [1]   [2]   [3]   [4]   [5]
                
                // Hata panellerini DOĞRU bit pozisyonları ile güncelle
                GuncelleHataPaneli(panelHata1, hataKodu[0]); // hata1 - Model uydu iniş hızı
                GuncelleHataPaneli(panelHata2, hataKodu[1]); // hata2 - Görev yükü iniş hızı
                GuncelleHataPaneli(panelHata3, hataKodu[2]); // hata3 - Basınç verisi
                GuncelleHataPaneli(panelHata4, hataKodu[3]); // hata4 - GPS verisi
                GuncelleHataPaneli(panelHata5, hataKodu[4]); // hata5 - Ayrılma durumu
                GuncelleHataPaneli(panelHata6, hataKodu[5]); // hata6 - Multi-spektral sistem

                // Alarm panellerini de DOĞRU bit pozisyonları ile güncelle
                GuncelleHataPaneli(panelAlarm1, hataKodu[0]); // hata1 - Model uydu iniş hızı
                GuncelleHataPaneli(panelAlarm2, hataKodu[1]); // hata2 - Görev yükü iniş hızı
                GuncelleHataPaneli(panelAlarm3, hataKodu[2]); // hata3 - Basınç verisi
                GuncelleHataPaneli(panelAlarm4, hataKodu[3]); // hata4 - GPS verisi
                GuncelleHataPaneli(panelAlarm5, hataKodu[4]); // hata5 - Ayrılma durumu
                GuncelleHataPaneli(panelAlarm6, hataKodu[5]); // hata6 - Multi-spektral sistem

                // 🔊 ARAS 6 Farklı Hata Sesi Sistemi - Yeni hatalar için ses çal
                if (checkBoxSesliUyari.Checked)
                {
                    CalArasHataSesleri(oncekiHataKodu, yeniHataKodu);
                }

                // 🔇 GENEL ALARM SİSTEMİ KALDIRILDI - Sadece ARAS özel sesleri çalacak
                // Artık her hata için farklı ses, genel alarm yok
                
                // Hata yoksa ARAS seslerinin de durmasını sağla
                if (!hataKodu.Contains('1'))
                {
                    // Tüm ARAS sesleri bittiğinde sessizlik
                    logHataFunc("✅ Tüm hatalar giderildi - Sesler durdu", false);
                }
            }
        }

        /// <summary>
        /// ARAS 6 farklı hata sesi çalan metod - Sadece yeni hatalar için ses çalar
        /// </summary>
        /// <param name="eskiHataKodu">Önceki hata kodu</param>
        /// <param name="yeniHataKodu">Yeni hata kodu</param>
        private void CalArasHataSesleri(string eskiHataKodu, string yeniHataKodu)
        {
            try
                {
                // 🔊 BIT POZİSYONLARI DÜZELTİLDİ - Arduino (YerAltiSistem.ino) ile tam uyumlu
                // Arduino Çıktısı: "{hata1}{hata2}{hata3}{hata4}{hata5}{hata6}"
                // String indeks:      [0]   [1]   [2]   [3]   [4]   [5]

                // Doğru bit pozisyonları ile hata seslerini kontrol et
                int[] bitPozisyonlari = { 0, 1, 2, 3, 4, 5 }; // hata1, hata2, hata3, hata4, hata5, hata6
                
                for (int i = 0; i < 6; i++)
                {
                    int bitIndex = bitPozisyonlari[i];

                    // Hata biti 0'dan 1'e geçmişse (yeni hata oluşmuşsa) ilgili sesi çal
                    if (eskiHataKodu[bitIndex] == '0' && yeniHataKodu[bitIndex] == '1')
                    {
                        // Hata tipi bilgisi ile log
                        string[] hataAciklamalari = { 
                            "Model Uydu İniş Hızı", "Görev Yükü İniş Hızı", "Basınç Verisi", 
                            "GPS Verisi", "Ayrılma Durumu", "Multi-Spektral Sistem" 
                        };
                        
                        logHataFunc($"🚨 YENİ ARAS HATASI {i+1}: {hataAciklamalari[i]} (Bit {bitIndex})", false);
                        CalArasHataSesi(i);
                    }
                }
            }
            catch (Exception ex)
                    {
                logHataFunc($"❌ ARAS hata sesleri çalınırken hata: {ex.Message}", false);
            }
        }

        /// <summary>
        /// Belirtilen hata indeksi için özel ARAS sesini çalar.
        /// </summary>
        /// <param name="hataIndeksi">Çalınacak sesin indeksi (0-5)</param>
        private void CalArasHataSesi(int hataIndeksi)
        {
            if (hataIndeksi < 0 || hataIndeksi >= 6) return;

            try
            {
                // Önce modern .wav dosyasını çalmayı dene
                if (arasSesleri[hataIndeksi] != null)
                {
                    arasSesleri[hataIndeksi].Play();
                    logHataFunc($"🔊 ARAS Uyarısı: {arasHataAciklamalari[hataIndeksi]}", false);
                    return; // Ses dosyası çalındı, işlemi bitir.
                }

                // .wav dosyası yoksa, modern BEEP desenini çal
                // Kritiklik 'false' yapıldı - artık pencere açmayacak
                logHataFunc($"🎵 ARAS Uyarısı (Beep): {arasHataAciklamalari[hataIndeksi]}", false);
                Task.Run(() =>
                {
                    try
                    {
                        switch (hataIndeksi)
                        {
                            case 0: // Model Uydu Hız Hatası - Ritmik ve Hızlı Uyarı
                                for (int i = 0; i < 4; i++) { Console.Beep(1200, 100); System.Threading.Thread.Sleep(50); }
                                break;
                                     
                            case 1: // Görev Yükü Hız Hatası - Daha Yumuşak Ritmik Uyarı
                                for (int i = 0; i < 3; i++) { Console.Beep(900, 150); System.Threading.Thread.Sleep(75); }
                                break;
                                     
                            case 2: // Basınç Veri Hatası - "Sonar Ping" efekti
                                Console.Beep(700, 400);
                                System.Threading.Thread.Sleep(200);
                                Console.Beep(700, 400);
                                break;
                                     
                            case 3: // GPS Sinyal Hatası - Klasik Sinyal Kaybı
                                Console.Beep(1500, 200);
                                System.Threading.Thread.Sleep(100);
                                Console.Beep(1500, 200);
                                System.Threading.Thread.Sleep(100);
                                Console.Beep(1500, 500);
                                break;
                                     
                            case 4: // Ayrılma Hatası - KRİTİK ALARM
                                for (int i = 0; i < 2; i++)
                                {
                                    Console.Beep(2000, 120);
                                    Console.Beep(1600, 120);
                                }
                                System.Threading.Thread.Sleep(100);
                                for (int i = 0; i < 2; i++)
                                {
                                    Console.Beep(2000, 120);
                                    Console.Beep(1600, 120);
                                }
                                break;
                                     
                            case 5: // Multi-Spektral Hata - Mekanik Arıza Sesi
                                Console.Beep(500, 80);
                                System.Threading.Thread.Sleep(50);
                                Console.Beep(500, 80);
                                System.Threading.Thread.Sleep(50);
                                Console.Beep(800, 300);
                                break;
                        }
                    }
                    catch (Exception ex)
                    {
                        // Kritiklik 'false' yapıldı - artık pencere açmayacak
                        logHataFunc($"Beep sesi çalınırken hata: {ex.Message}", false);
                    }
                });
            }
            catch (Exception ex)
            {
                // Kritiklik 'false' yapıldı - artık pencere açmayacak
                logHataFunc($"ARAS sesi çalınırken hata: {ex.Message}", false);
            }
        }

        /// <summary>
        /// Hata panelinin rengini güncelleyen metod
        /// </summary>
        /// <param name="panel">Güncellenecek panel</param>
        /// <param name="hataDurumu">Hata durumu ('0' veya '1')</param>
        private void GuncelleHataPaneli(Panel panel, char hataDurumu)
        {
            // Hata durumu '1' ise (hata var) panel kırmızı olur
            if (hataDurumu == '1')
            {
                panel.BackColor = Color.Red; // Hata durumu - kırmızı
            }
            // Hata durumu '0' ise (hata yok) panel yeşil olur
            else
            {
                panel.BackColor = Color.Green; // Normal durum - yeşil
            }
        }

        /// <summary>
        /// Mevcut hata kodunu döndürür
        /// </summary>
        /// <returns>6 haneli hata kodu</returns>
        public string GetHataKodu()
        {
            return hataKodu;
        }

        /// <summary>
        /// Herhangi bir hata durumu olup olmadığını kontrol eder
        /// </summary>
        /// <returns>Hata varsa true, yoksa false</returns>
        public bool HataVarMi()
        {
            return hataKodu.Contains('1');
        }

        /// <summary>
        /// Sesli uyarıyı durdurur
        /// </summary>
        public void DurdurSesliUyari()
        {
            sesliUyariAktif = false;
        }

        /// <summary>
        /// Sesli uyarı durumunu açık veya kapalı olarak güncelleyen metod
        /// </summary>
        /// <param name="aktifMi">Sesli uyarının aktif olup olmayacağı</param>
        public void SesliUyariDurumunuGuncelle(bool aktifMi)
        {
            // CheckBox'ın durumunu güncelle
            if (checkBoxSesliUyari.Checked != aktifMi)
            {
                checkBoxSesliUyari.Checked = aktifMi;
            }
            
            // Sesli uyarı kapatıldıysa ve aktif bir uyarı varsa
            if (!aktifMi && sesliUyariAktif)
            {
                // Ses dosyası varsa durdur
                sesliUyariAktif = false;
            }
            else if (aktifMi && hataKodu.Contains('1') && !sesliUyariAktif)
            {
                // Ses dosyası varsa çal, yoksa Console.Beep kullan
                sesliUyariAktif = true;
            }
        }

        /// <summary>
        /// Sesli uyarı onay kutusunun durumu değiştiğinde çalışan metod
        /// </summary>
        private void CheckBoxSesliUyari_CheckedChanged(object sender, EventArgs e)
        {
            // Checkbox durumunu kullanarak sesli uyarıyı güncelle
            SesliUyariDurumunuGuncelle(checkBoxSesliUyari.Checked);
        }

        /// <summary>
        /// Kaynakları temizler
        /// </summary>
        public void Dispose()
        {
            try
            {
                // 6 farklı ARAS sesini temizle
                for (int i = 0; i < 6; i++)
                {
                    if (arasSesleri[i] != null)
                    {
                        try
                        {
                            arasSesleri[i].Dispose();
                            arasSesleri[i] = null;
                        }
                        catch (Exception ex)
                        {
                            logHataFunc?.Invoke($"ARAS Ses {i + 1} temizlenirken hata: {ex.Message}", false);
                        }
                    }
                }

                // UI kontrol referanslarını temizle
                labelHataKodu = null;
                checkBoxSesliUyari = null;

                // Panel referanslarını temizle
                panelHata1 = null;
                panelHata2 = null;
                panelHata3 = null;
                panelHata4 = null;
                panelHata5 = null;
                panelHata6 = null;

                panelAlarm1 = null;
                panelAlarm2 = null;
                panelAlarm3 = null;
                panelAlarm4 = null;
                panelAlarm5 = null;
                panelAlarm6 = null;

                // Callback fonksiyonunu temizle
                logHataFunc = null;

                logHataFunc?.Invoke("🔊 ARAS 6 Farklı Hata Sesi Sistemi temizlendi.", false);
            }
            catch (Exception ex)
            {
                Console.WriteLine("AlarmSistemiYoneticisi Dispose hatası: " + ex.Message);
            }
        }
    }
} 