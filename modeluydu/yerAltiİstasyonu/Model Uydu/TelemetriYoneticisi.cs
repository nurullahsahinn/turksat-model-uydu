using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO.Ports;
using System.Diagnostics;
using System.IO;
using SystemTextBox = System.Windows.Forms.TextBox;
using System.Drawing.Drawing2D; // LinearGradientBrush için

namespace ModelUydu
{
    /// <summary>
    /// Telemetri verilerinin alınması, işlenmesi ve görüntülenmesi işlevlerini yöneten sınıf
    /// </summary>
    public class TelemetriYoneticisi : IDisposable
    {
        #region Özellikler ve Değişkenler
        
        // Telemetri verilerinin tutulduğu liste
        private List<string[]> telemetriVerileri = new List<string[]>();
        
        // UI bileşenleri
        private DataGridView dataGridView;
        private Dictionary<string, SystemTextBox> veriKontrolleri = new Dictionary<string, SystemTextBox>();
        private Label labelTakimNumarasi;
        private Label labelUyduStatu;
        
        // Diğer yöneticilere referanslar
        private GrafikYoneticisi grafikYoneticisi;
        private UyduGorsellestime uyduGorsellestime;
        private HaritaYoneticisi haritaYoneticisi;
        private IoTYoneticisi iotYoneticisi;
        private AlarmSistemiYoneticisi alarmSistemiYoneticisi;
        // SDKartYoneticisi kaldırıldı - artık direkt dosya kaydetme işlemi yapılacak
        private MultiSpektralFiltreYoneticisi multiSpektralFiltreYoneticisi;
        private KameraYoneticisi kameraYoneticisi;
        
        // Basit Video Görüntüleme sistemi (donma önlemi)
        private BasitVideoGoruntuleme basitVideo;
        
        // Uydu durumu güncelleme delegesi
        private Action<string> uyduDurumuGuncelleDelegesi;
        
        // Hata log işlemi için delege
        private Action<string, bool> logHataFonksiyonu;
        
        // Beklenen telemetri paketi uzunluğu
        private const int BeklenenPaketUzunlugu = 22;
        
        // Ayrılma durumu takibi için değişken
        private bool ayrilmaDurumuGerceklesti = false;
        
        // Son telemetri verilerini tutmak için değişkenler
        private string[] sonTelemetriVerileri = null;
        private int toplamPaketSayisi = 0;
        
        // Telemetri format açıklaması:
        // <PAKET NUMARASI>, <UYDU STATÜSÜ>, <HATA KODU>, <GÖNDERME SAATI>, <BASINÇ1>, <BASINÇ2>, <YÜKSEKLIK1>, <YÜKSEKLIK2>, 
        // <IRTIFA FARKI>, <INIŞ HIZI>, <SICAKLIK>, <PIL GERILIMI>, <GPS1 LATITUDE>, <GPS1 LONGITUDE>, <GPS1 ALTITUDE>, 
        // <PITCH>, <ROLL>, <YAW>, <RHRH>, <IOT S1 DATA>, <IOT S2 DATA>, <TAKIM NO>
        
        // Telemetri veri alanları indeksleri
        public enum TelemetriAlani
        {
            PaketNo = 0,
            UyduStatusu = 1,
            HataKodu = 2,
            GondermeSaati = 3,
            Basinc1 = 4,
            Basinc2 = 5,
            Yukseklik1 = 6,
            Yukseklik2 = 7,
            IrtifaFarki = 8,
            InisHizi = 9,
            Sicaklik = 10,
            PilGerilimi = 11,
            GpsLatitude = 12,
            GpsLongitude = 13,
            GpsAltitude = 14,
            Pitch = 15,
            Roll = 16,
            Yaw = 17,
            RHRH = 18,
            IoTS1Data = 19,
            IoTS2Data = 20,
            TakimNo = 21
        }
        
        #endregion
        
        #region Yapıcı Metot
        
        /// <summary>
        /// TelemetriYoneticisi sınıfı yapıcı metodu
        /// </summary>
        public TelemetriYoneticisi(
            DataGridView dataGridView,
            Dictionary<string, SystemTextBox> veriKontrolleri,
            GrafikYoneticisi grafikYoneticisi,
            UyduGorsellestime uyduGorsellestime,
            HaritaYoneticisi haritaYoneticisi,
            IoTYoneticisi iotYoneticisi,
            AlarmSistemiYoneticisi alarmSistemiYoneticisi,
            Action<string> uyduDurumuGuncelleDelegesi,
            Action<string, bool> logHataFonksiyonu,
            Label labelTakimNumarasi = null,
            Action<string> hataKoduGuncelle = null,
            MultiSpektralFiltreYoneticisi multiSpektralFiltreYoneticisi = null,
            KameraYoneticisi kameraYoneticisi = null,
            Label labelUyduStatu = null)
        {
            this.dataGridView = dataGridView;
            this.veriKontrolleri = veriKontrolleri;
            this.grafikYoneticisi = grafikYoneticisi;
            this.uyduGorsellestime = uyduGorsellestime;
            this.haritaYoneticisi = haritaYoneticisi;
            this.iotYoneticisi = iotYoneticisi;
            this.alarmSistemiYoneticisi = alarmSistemiYoneticisi;
            // sdKartYoneticisi kaldırıldı - null olarak geçilecek
            this.uyduDurumuGuncelleDelegesi = uyduDurumuGuncelleDelegesi;
            this.logHataFonksiyonu = logHataFonksiyonu;
            this.labelTakimNumarasi = labelTakimNumarasi;
            this.labelUyduStatu = labelUyduStatu;
            this.multiSpektralFiltreYoneticisi = multiSpektralFiltreYoneticisi;
            this.kameraYoneticisi = kameraYoneticisi;
            
            // Basit Video Sistemi başlat (donma önlemi)
            try
            {
                if (kameraYoneticisi != null)
                {
                    Log("🔧 Basit Video sistemi başlatılıyor...", false);
                    // Video panel'i reflection ile al
                    var videoPlayerField = kameraYoneticisi.GetType().GetField("videoPlayer", 
                        System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                    
                    if (videoPlayerField != null)
                    {
                        var videoPlayerObj = videoPlayerField.GetValue(kameraYoneticisi);
                        Log($"🔧 Video player objesi: {videoPlayerObj?.GetType().Name}", false);
                        
                        // VideoSourcePlayer'ı PictureBox olarak kullan
                        if (videoPlayerObj is System.Windows.Forms.Control videoControl)
                        {
                            // Geçici PictureBox oluştur (basit video için)
                            var tempPictureBox = new PictureBox
                            {
                                Size = videoControl.Size,
                                Location = videoControl.Location,
                                SizeMode = PictureBoxSizeMode.StretchImage,
                                BackColor = Color.Black
                            };
                            
                            // Ana forma ekle
                            videoControl.Parent?.Controls.Add(tempPictureBox);
                            tempPictureBox.BringToFront();
                            
                            basitVideo = new BasitVideoGoruntuleme(tempPictureBox, Log);
                            basitVideo.Baslat();
                            Log("✅ Basit Video sistemi başlatıldı (overlay PictureBox)", false);
                        }
                        else
                        {
                            Log("⚠️ Video control uyumlu değil, basit video sistemi devre dışı", true);
                        }
                    }
                    else
                    {
                        Log("⚠️ Video player field bulunamadı", true);
                    }
                }
            }
            catch (Exception ex)
            {
                Log($"❌ Basit video sistemi başlatma hatası: {ex.Message}", true);
            }
            
            // DataGridView için sütunları oluştur (ilk defa oluşturuluyorsa)
            if (dataGridView != null && dataGridView.Columns.Count == 0)
            {
                OlusturDataGridViewSutunlari();
            }
            
            Log("Telemetri Yöneticisi başlatıldı", false);
        }
        
        #endregion
        
        #region Telemetri ve Video İşleme Metodları
        
        /// <summary>
        /// Gelen telemetri dizesi için 8-bit XOR checksum hesaplar
        /// </summary>
        /// <param name="packet">Checksum'ı hesaplanacak veri paketi</param>
        /// <returns>Hesaplanan checksum değeri</returns>
        private byte HesaplaChecksum(string packet)
        {
            byte checksum = 0;
            foreach (char ch in packet)
            {
                checksum ^= (byte)ch;
            }
            return checksum;
        }

        /// <summary>
        /// Görev yükünden gelen video frame'ini işler
        /// 🔧 DÜZELTME: DEADBEEF/CAFEBABE binary format desteği eklendi
        /// Hem eski (#VIDEO:base64#) hem yeni (DEADBEEF+JPEG+CAFEBABE) formatları destekler
        /// </summary>
        /// <param name="videoData">Video frame verisi</param>
        private void VideoFrameIsle(string videoData)
        {
            try
            {
                // ÖNEMLİ DEBUG LOG - ARTIRILDI
                Log($"🎬 VideoFrameIsle ÇAĞRILDI: {videoData?.Length} karakter", false);
                System.Diagnostics.Debug.WriteLine($"🎬 VideoFrameIsle: {videoData?.Length} karakter");
                Console.WriteLine($"🎬 CONSOLE DEBUG: VideoFrameIsle çağrıldı - {videoData?.Length} karakter");
                
                // Video stream aktif değilse başlat
                if (kameraYoneticisi != null && !kameraYoneticisi.VideoStreamAktifMi)
                {
                    kameraYoneticisi.VideoStreamBaslat();
                    Log("Video stream otomatik başlatıldı (veri alındı)", false);
                }

                // Format kontrolü: Base64 string format (#VIDEO:...#)
                if (!string.IsNullOrEmpty(videoData) && videoData.StartsWith("#VIDEO:") && videoData.EndsWith("#"))
                {
                    Log($"🎥 Video frame alındı: {videoData.Length} karakter", false);
                    System.Diagnostics.Debug.WriteLine($"VIDEO ALINDI: {videoData.Length} karakter, ilk 100: {videoData.Substring(0, Math.Min(100, videoData.Length))}...");
                    Console.WriteLine($"🎥 CONSOLE: Video frame alındı - {videoData.Length} karakter");
                    
                    // ESKİ FORMAT: #VIDEO:base64_encoded_jpeg_data#
                    string base64Data = videoData.Substring(7, videoData.Length - 8); // #VIDEO: (7 karakter) ve son # (1 karakter) kaldır
                    
                    // Base64 verisi geçerli mi kontrol et
                    if (string.IsNullOrEmpty(base64Data))
                    {
                        Log("Boş video frame verisi alındı", true);
                        return;
                    }

                    // Base64'ü byte array'e çevir
                    byte[] frameBytes = Convert.FromBase64String(base64Data);
                    
                    // BASIT VIDEO SİSTEMİ ile frame'i göster (donma önlemi)
                    if (basitVideo != null && basitVideo.Aktif)
                    {
                        Console.WriteLine($"🎬 CONSOLE: Basit video sistemi kullanılıyor - {frameBytes.Length} bytes");
                        basitVideo.FrameGoruntule(frameBytes);
                        Log($"✅ Video frame işlendi (Basit sistem: {frameBytes.Length} bytes)", false);
                        
                        // Debug için detaylı log
                        System.Diagnostics.Debug.WriteLine($"BASİT VIDEO: Frame boyutu: {frameBytes.Length} bytes");
                        System.Diagnostics.Debug.WriteLine($"BASİT VIDEO: Base64 uzunluğu: {base64Data.Length} karakter");
                        Console.WriteLine($"✅ CONSOLE: Basit video frame işlendi - {frameBytes.Length} bytes");
                    }
                    else if (kameraYoneticisi != null)
                    {
                        // Fallback: Eski sistem (sadece basit video yoksa)
                        Console.WriteLine($"🎬 CONSOLE: Eski kamera sistemi kullanılıyor - {frameBytes.Length} bytes");
                        kameraYoneticisi.VideoFrameIsle(frameBytes);
                        Log($"✅ Video frame işlendi (Eski sistem: {frameBytes.Length} bytes)", false);
                        Console.WriteLine($"✅ CONSOLE: Eski sistem frame işlendi - {frameBytes.Length} bytes");
                    }
                    else
                    {
                        Log("❌ Hiçbir video sistemi yok - frame işlenemedi", true);
                        Console.WriteLine("❌ CONSOLE: Hiçbir video sistemi yok!");
                    }
                }
                else
                {
                    // Diğer formatlar için binary processing gerekebilir
                    Log("Video frame formatı tanınmadı - binary processing gerekli", true);
                }
            }
            catch (FormatException ex)
            {
                Log($"❌ Video frame formatı geçersiz: {ex.Message}", true);
                System.Diagnostics.Debug.WriteLine($"FORMAT HATASI: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"PROBLEM VERİSİ: {videoData?.Substring(0, Math.Min(200, videoData?.Length ?? 0))}");
                
                // Popup kaldırıldı - donmaya sebep oluyor
                // System.Windows.Forms.MessageBox.Show($"Video Frame Format Hatası:\n\n{ex.Message}\n\nVeri: {videoData?.Substring(0, Math.Min(100, videoData?.Length ?? 0))}", 
                //     "Video Format Hatası", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                Log($"❌ Video frame işlenirken hata: {ex.Message}", true);
                System.Diagnostics.Debug.WriteLine($"VIDEO İŞLEME HATASI: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"STACK TRACE: {ex.StackTrace}");
                
                // Popup kaldırıldı - donmaya sebep oluyor
                // System.Windows.Forms.MessageBox.Show($"Video İşleme Hatası:\n\n{ex.Message}\n\nDetay: {ex.StackTrace?.Substring(0, Math.Min(200, ex.StackTrace?.Length ?? 0))}", 
                //     "Video İşleme Hatası", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// 🔧 YENİ: Binary video frame verilerini işleyen metod
        /// DEADBEEF + JPEG + CAFEBABE formatını destekler
        /// </summary>
        /// <param name="binaryData">Raw binary video frame verisi</param>
        private void VideoFrameIsleBinary(byte[] binaryData)
        {
            try
            {
                if (binaryData == null || binaryData.Length < 8)
                {
                    Log("Geçersiz binary video verisi - çok küçük", true);
                    return;
                }

                // Video frame delimiter'ları
                byte[] frameStart = { 0xDE, 0xAD, 0xBE, 0xEF }; // DEADBEEF
                byte[] frameEnd = { 0xCA, 0xFE, 0xBA, 0xBE };   // CAFEBABE

                // Başlangıç delimiter'ını kontrol et
                bool startFound = true;
                for (int i = 0; i < frameStart.Length; i++)
                {
                    if (binaryData[i] != frameStart[i])
                    {
                        startFound = false;
                        break;
                    }
                }

                if (!startFound)
                {
                    Log("Video frame başlangıç delimiter'ı (DEADBEEF) bulunamadı", true);
                    return;
                }

                // Bitiş delimiter'ını ara
                int endIndex = -1;
                for (int i = binaryData.Length - frameEnd.Length; i >= frameStart.Length; i--)
                {
                    bool endFound = true;
                    for (int j = 0; j < frameEnd.Length; j++)
                    {
                        if (binaryData[i + j] != frameEnd[j])
                        {
                            endFound = false;
                            break;
                        }
                    }
                    if (endFound)
                    {
                        endIndex = i;
                        break;
                    }
                }

                if (endIndex == -1)
                {
                    Log("Video frame bitiş delimiter'ı (CAFEBABE) bulunamadı", true);
                    return;
                }

                // JPEG verilerini çıkar (delimiter'lar arasındaki kısım)
                int jpegLength = endIndex - frameStart.Length;
                if (jpegLength <= 0)
                {
                    Log("Geçersiz JPEG veri uzunluğu", true);
                    return;
                }

                byte[] jpegData = new byte[jpegLength];
                Array.Copy(binaryData, frameStart.Length, jpegData, 0, jpegLength);

                // Video stream aktif değilse başlat
                if (kameraYoneticisi != null && !kameraYoneticisi.VideoStreamAktifMi)
                {
                    kameraYoneticisi.VideoStreamBaslat();
                    Log("Video stream otomatik başlatıldı (binary veri alındı)", false);
                }

                // KameraYoneticisi'ne frame'i gönder
                if (kameraYoneticisi != null)
                {
                    kameraYoneticisi.VideoFrameIsle(jpegData);
                    Log($"Binary video frame işlendi ({jpegData.Length} bytes JPEG)", false);
                }
                else
                {
                    Log("KameraYoneticisi bulunamadı, binary video frame işlenemedi", true);
                }
            }
            catch (Exception ex)
            {
                Log($"Binary video frame işlenirken hata: {ex.Message}", true);
            }
        }

        /// <summary>
        /// 🔧 YENİ: Binary video verisini işleyen public metod
        /// DEADBEEF + JPEG + CAFEBABE formatını destekler
        /// </summary>
        /// <param name="binaryVeri">Raw binary video frame verisi</param>
        public void BinaryVideoVerisiniIsle(byte[] binaryVeri)
        {
            VideoFrameIsleBinary(binaryVeri);
        }

        /// <summary>
        /// Telemetri verisini işleyen ana metod
        /// </summary>
        public void TelemetriVerisiniIsle(string telemetriVerisi)
        {
            try
            {
                // Video frame kontrolü - #VIDEO: ile başlayıp # ile bitiyorsa
                if (!string.IsNullOrEmpty(telemetriVerisi) && telemetriVerisi.StartsWith("#VIDEO:") && telemetriVerisi.EndsWith("#"))
                {
                    Log($"🎯 VIDEO FRAME TESPİT EDİLDİ: {telemetriVerisi.Length} karakter", false);
                    System.Diagnostics.Debug.WriteLine($"🎯 VIDEO FRAME TESPİT EDİLDİ: {telemetriVerisi.Length} karakter");
                    Console.WriteLine($"🎯 CONSOLE: VIDEO FRAME TESPİT EDİLDİ - {telemetriVerisi.Length} karakter");
                    VideoFrameIsle(telemetriVerisi);
                    return;
                }
                
                // GÜÇLÜ BINARY/VIDEO FRAME FİLTRESİ
                if (!string.IsNullOrEmpty(telemetriVerisi))
                {
                    // 1. Çok uzun veriler (>200 karakter) - muhtemelen video
                    if (telemetriVerisi.Length > 200)
                    {
                        // Log($"Uzun veri atlandı ({telemetriVerisi.Length} karakter) - muhtemelen video frame", false);
                        return;
                    }
                    
                    // 2. Base64 pattern kontrolü
                    if (telemetriVerisi.Length > 50 && 
                        (telemetriVerisi.Contains("==") || telemetriVerisi.Contains("=") && 
                         telemetriVerisi.Count(c => char.IsLetterOrDigit(c) || c == '+' || c == '/') > telemetriVerisi.Length * 0.8))
                    {
                        // Log("Base64 video frame atlandı", false);
                        return;
                    }
                    
                    // 3. Binary data pattern (çok fazla büyük/küçük harf karışımı)
                    if (telemetriVerisi.Length > 100)
                    {
                        int upperCount = telemetriVerisi.Count(char.IsUpper);
                        int lowerCount = telemetriVerisi.Count(char.IsLower);
                        int digitCount = telemetriVerisi.Count(char.IsDigit);
                        
                        if (upperCount > 20 && lowerCount > 20 && digitCount > 10)
                        {
                            // Log("Binary pattern video frame atlandı", false);
                            return;
                        }
                    }
                }
                
                // Paket formatını doğrula: '$' ile başlamalı ve '*' içermeli
                if (string.IsNullOrEmpty(telemetriVerisi) || !telemetriVerisi.StartsWith("$") || !telemetriVerisi.Contains("*"))
                {
                    Log($"Geçersiz paket formatı (Başlangıç veya Checksum ayracı eksik): {telemetriVerisi}", true);
                    return;
                }

                // Paketi veri ve checksum olarak ayır
                int checksumIndex = telemetriVerisi.LastIndexOf('*');
                string veriKismi = telemetriVerisi.Substring(1, checksumIndex - 1); // '$' sonrası ve '*' öncesi
                string gelenChecksumHex = telemetriVerisi.Substring(checksumIndex + 1).Trim();

                // Gelen checksum'ı byte'a çevir
                byte gelenChecksum;
                try
                {
                    gelenChecksum = Convert.ToByte(gelenChecksumHex, 16);
                }
                catch (Exception ex)
                {
                    Log($"Geçersiz checksum formatı: {gelenChecksumHex}. Hata: {ex.Message}", true);
                    return;
                }

                // Veri kısmı için checksum'ı yeniden hesapla
                byte hesaplananChecksum = HesaplaChecksum(veriKismi);

                // Checksum'ları karşılaştır
                if (gelenChecksum != hesaplananChecksum)
                {
                    Log($"Checksum hatası! Gelen: {gelenChecksumHex}, Hesaplanması gereken: {hesaplananChecksum:X2}. Paket atlandı: {telemetriVerisi}", true);
                    return; // Checksum uyuşmuyorsa paketi işleme
                }
                
                // Checksum doğrulandı, veriyi işle
                Log($"Checksum doğrulandı. Paket: {veriKismi}", false);

                // 🔧 KALİBRASYON ONAY MESAJLARI KONTROLÜ
                if (veriKismi.StartsWith("GYRO_CALIB_OK"))
                {
                    Log("✅ Gyro kalibrasyonu başarıyla tamamlandı", false);
                    return;
                }
                else if (veriKismi.StartsWith("GYRO_CALIB_ERROR"))
                {
                    Log("❌ Gyro kalibrasyonu başarısız oldu", true);
                    return;
                }
                else if (veriKismi.StartsWith("PRESSURE_CALIB_OK"))
                {
                    string[] parts = veriKismi.Split(':');
                    if (parts.Length >= 2)
                    {
                        Log($"✅ Basınç kalibrasyonu başarıyla güncellendi: {parts[1]} hPa", false);
                    }
                    else
                    {
                        Log("✅ Basınç kalibrasyonu başarıyla güncellendi", false);
                    }
                    return;
                }
                else if (veriKismi.StartsWith("PRESSURE_CALIB_ERROR"))
                {
                    Log("❌ Basınç kalibrasyonu başarısız oldu", true);
                    return;
                }
                else if (veriKismi.StartsWith("MULTISPEKTRAL_COMPLETE"))
                {
                    Log("✅ Multi-spektral filtreleme Görev Yükü tarafından tamamlandı!", false);
                    
                    // Multi-spektral filtre yöneticisine bildir
                    if (multiSpektralFiltreYoneticisi != null)
                    {
                        multiSpektralFiltreYoneticisi.FiltrelemeKompletOlarakIsaretle();
                    }
                    return;
                }
                else if (veriKismi.StartsWith("AYIRMA_KOMUTU"))
                {
                    Log("🚀 Görev yükü tarafından ayrılma komutu gönderildi", false);
                    return;
                }

                // Gelen veriyi parçalara ayır
                string[] paket = veriKismi.Split(',');
                
                // Paket formatının doğruluğunu kontrol et
                if (paket.Length < BeklenenPaketUzunlugu) // TÜRKSAT formatı: 22 alan
                {
                    Log($"Geçersiz telemetri paketi (beklenen: {BeklenenPaketUzunlugu}, gelen: {paket.Length}): {telemetriVerisi}", true);
                    return;
                }
                
                // Telemetri verisini listeye ekle
                telemetriVerileri.Add(paket);
                
                // Son telemetri verilerini güncelle (VERİLERİ GÖSTER butonu için)
                sonTelemetriVerileri = paket;
                toplamPaketSayisi++;
                
                // Telemetri verisini görüntüye aktar
                GoruntuleVeriler(paket);
                
                // Telemetri verilerini DataGridView'e ekle
                DataGridViewEkle(paket);
                
                // Grafikleri güncelle
                GuncelleGrafikler(paket);
                
                // 3D modeli güncelle
                Guncelle3DModel(paket);
                
                // Haritayı güncelle
                GuncelleHarita(paket);
                
                // IoT verilerini günceller
                GuncelleIoTVerileri(paket);
                
                // Alarm sistemini kontrol et
                KontrolAlarmSistemi(paket);
                
                // SD Karta kaydet (artık checksum ile birlikte)
                KaydetSDKarta(telemetriVerisi.Substring(1)); // '$' karakteri olmadan kaydet
                
                // Uydu durumunu güncelle
                if (paket.Length > (int)TelemetriAlani.UyduStatusu)
                {
                    // Ham durum kodunu al
                    string durumKodu = paket[(int)TelemetriAlani.UyduStatusu].Trim();
                    
                    // İç mantık için ham kodu kullan
                    KontrolAyrilmaDurumu(durumKodu, paket);
                    
                    // Sadece arayüzü güncellemek için metne çevir
                    string durumMetni = GetUyduStatusuText(durumKodu);
                    uyduDurumuGuncelleDelegesi?.Invoke(durumMetni);
                }
            }
            catch (Exception ex)
            {
                Log($"Telemetri verisi işlenirken hata: {ex.Message}", true);
            }
        }
        
        /// <summary>
        /// Ayrılma durumunu kontrol eder ve MultiSpektralFiltreYoneticisi'ni günceller
        /// </summary>
        private void KontrolAyrilmaDurumu(string durumKodu, string[] paket)
        {
            try
            {
                // Statü 3: Ayrılma
                if (durumKodu == "3" && !ayrilmaDurumuGerceklesti)
                {
                    ayrilmaDurumuGerceklesti = true;
                    
                    // Kullanıcıyı bilgilendirici bir mesaj kutusu ile uyar (Hata olarak değil!)
                    string bilgiMesaji = "🚀 AYRILMA GERÇEKLEŞTİ!\n\nModel uydu ve görev yükü başarıyla ayrıldı.";
                    System.Windows.Forms.MessageBox.Show(
                        bilgiMesaji, 
                        "Uçuş Durumu Bilgisi", 
                        System.Windows.Forms.MessageBoxButtons.OK, 
                        System.Windows.Forms.MessageBoxIcon.Information
                    );

                    // Olayı log dosyasına normal bir bilgi olarak kaydet
                    Log(bilgiMesaji.Replace("\n\n", " "), false);
                
                    // MultiSpektralFiltreYoneticisi'ni güncelle
                    if (multiSpektralFiltreYoneticisi != null)
                    {
                        multiSpektralFiltreYoneticisi.AyrilmaDurumuGuncelle(true);
                    }
                }
            }
            catch (Exception ex)
            {
                Log($"Ayrılma durumu kontrolünde hata: {ex.Message}", true);
            }
        }
        
        /// <summary>
        /// Alarm sistemini kontrol eder
        /// </summary>
        private void KontrolAlarmSistemi(string[] paket)
        {
            try
            {
                if (alarmSistemiYoneticisi != null && paket.Length > 2)
                {
                    // Hata kodunu al
                    string hataKodu = paket[2].Trim();
                    
                    // Alarm sistemini güncelle - AlarmSistemiYoneticisi sınıfında bulunan HataKoduGuncelle metodunu çağır
                    alarmSistemiYoneticisi.HataKoduGuncelle(hataKodu);
                }
            }
            catch (Exception ex)
            {
                Log($"Alarm sistemi kontrol edilirken hata: {ex.Message}", false);
            }
        }
        
        /// <summary>
        /// SD Karta kaydetme işlemini tetikler
        /// </summary>
        /// <param name="telemetriVerisi">Kaydedilecek ham telemetri verisi</param>
        private void KaydetSDKarta(string telemetriVerisi)
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

                // Telemetri verilerini dosyaya ekle ($ karakteri olmadan)
                File.AppendAllText(dosyaAdi, telemetriVerisi + Environment.NewLine);
            }
            catch (Exception ex)
            {
                Log($"Telemetri verisi kaydedilirken hata: {ex.Message}", false);
            }
        }
        
        #endregion
        
        #region Yardımcı Metodlar
        
        /// <summary>
        /// DataGridView için sütunları oluşturur
        /// </summary>
        private void OlusturDataGridViewSutunlari()
        {
            try
            {
                // Önceki sütunları temizle
                dataGridView.Columns.Clear();
                
                // 📦 Emoji'li sütunları ekle (daha anlaşılır görünüm için)
                dataGridView.Columns.Add("PaketNo", "📋 Paket No");
                dataGridView.Columns.Add("UyduStatusu", "🛰️ Uydu Statüsü");
                dataGridView.Columns.Add("HataKodu", "⚠️ Hata Kodu");
                dataGridView.Columns.Add("GondermeSaati", "⏰ Gönderme Saati");
                dataGridView.Columns.Add("Basinc1", "🌀 Basınç 1");
                dataGridView.Columns.Add("Basinc2", "🌀 Basınç 2");
                dataGridView.Columns.Add("Yukseklik1", "📏 Yükseklik 1");
                dataGridView.Columns.Add("Yukseklik2", "📏 Yükseklik 2");
                dataGridView.Columns.Add("IrtifaFarki", "📐 İrtifa Farkı");
                dataGridView.Columns.Add("InisHizi", "🏃 İniş Hızı");
                dataGridView.Columns.Add("Sicaklik", "🌡️ Sıcaklık");
                dataGridView.Columns.Add("PilGerilimi", "🔋 Pil Gerilimi");
                dataGridView.Columns.Add("GPSLatitude", "🌍 GPS Latitude");
                dataGridView.Columns.Add("GPSLongitude", "🌎 GPS Longitude");
                dataGridView.Columns.Add("GPSAltitude", "📍 GPS Altitude");
                dataGridView.Columns.Add("Pitch", "↕️ Pitch");
                dataGridView.Columns.Add("Roll", "↔️ Roll");
                dataGridView.Columns.Add("Yaw", "🔄 Yaw");
                dataGridView.Columns.Add("RHRH", "🌈 RHRH");
                dataGridView.Columns.Add("IoTS1", "🏠 IoT S1");
                dataGridView.Columns.Add("IoTS2", "🏭 IoT S2");
                dataGridView.Columns.Add("TakimNo", "🏷️ Takım No");
                
                // 📐 EXTREME KOMPAKT sütun genişlik ayarları (tüm sütunlar görünecek)
                dataGridView.Columns["PaketNo"].Width = 40;           // Paket numarası
                dataGridView.Columns["UyduStatusu"].Width = 55;       // KALKIŞ, APOGEE vs.
                dataGridView.Columns["HataKodu"].Width = 35;          // 0-255 hata kodu
                dataGridView.Columns["GondermeSaati"].Width = 50;     // HH:MM:SS
                dataGridView.Columns["Basinc1"].Width = 45;           // Basınç değeri
                dataGridView.Columns["Basinc2"].Width = 45;           // Basınç değeri
                dataGridView.Columns["Yukseklik1"].Width = 45;        // Yükseklik metre
                dataGridView.Columns["Yukseklik2"].Width = 45;        // Yükseklik metre
                dataGridView.Columns["IrtifaFarki"].Width = 40;       // Fark değeri
                dataGridView.Columns["InisHizi"].Width = 40;          // Hız m/s
                dataGridView.Columns["Sicaklik"].Width = 40;          // Sıcaklık °C
                dataGridView.Columns["PilGerilimi"].Width = 45;       // Gerilim V
                dataGridView.Columns["GPSLatitude"].Width = 55;       // GPS koordinat
                dataGridView.Columns["GPSLongitude"].Width = 55;      // GPS koordinat
                dataGridView.Columns["GPSAltitude"].Width = 45;       // GPS yükseklik
                dataGridView.Columns["Pitch"].Width = 35;             // Gyro derece
                dataGridView.Columns["Roll"].Width = 35;              // Gyro derece
                dataGridView.Columns["Yaw"].Width = 35;               // Gyro derece
                dataGridView.Columns["RHRH"].Width = 35;              // R/H değeri
                dataGridView.Columns["IoTS1"].Width = 40;             // IoT sıcaklık
                dataGridView.Columns["IoTS2"].Width = 40;             // IoT sıcaklık
                dataGridView.Columns["TakimNo"].Width = 45;           // TÜRKSAT takım

                // 🎨 DataGridView ULTRA KOMPAKT görsel iyileştirmeleri
                dataGridView.EnableHeadersVisualStyles = true;
                dataGridView.BackgroundColor = System.Drawing.Color.White;
                dataGridView.DefaultCellStyle.BackColor = System.Drawing.Color.White;
                dataGridView.DefaultCellStyle.ForeColor = System.Drawing.Color.Black;
                dataGridView.DefaultCellStyle.SelectionBackColor = System.Drawing.Color.LightBlue;
                dataGridView.DefaultCellStyle.SelectionForeColor = System.Drawing.Color.Black;
                dataGridView.DefaultCellStyle.Font = new System.Drawing.Font("Arial", 7); // Ultra küçük font
                dataGridView.AlternatingRowsDefaultCellStyle.BackColor = System.Drawing.Color.FromArgb(248, 249, 250);
                dataGridView.ColumnHeadersDefaultCellStyle.BackColor = System.Drawing.Color.FromArgb(52, 58, 64);
                dataGridView.ColumnHeadersDefaultCellStyle.ForeColor = System.Drawing.Color.White;
                dataGridView.ColumnHeadersDefaultCellStyle.Font = new System.Drawing.Font("Arial", 7, System.Drawing.FontStyle.Bold); // Ultra küçük başlık
                dataGridView.BorderStyle = BorderStyle.Fixed3D;
                dataGridView.GridColor = System.Drawing.Color.LightGray;
                
                // 📏 Extreme kompakt satır ve başlık ayarları (maksimum küçültme)
                dataGridView.RowTemplate.Height = 16;                  // En küçük satır yüksekliği
                dataGridView.ColumnHeadersHeight = 16;                 // En küçük başlık yüksekliği
                
                // 📜 Gelişmiş scroll özellikleri
                dataGridView.ScrollBars = ScrollBars.Both;              // Hem yatay hem dikey scroll
                dataGridView.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None; // Manuel genişlik
                dataGridView.AllowUserToResizeColumns = true;          // Kullanıcı genişlik ayarlayabilir
                dataGridView.AllowUserToResizeRows = false;            // Satır yüksekliği sabit
                dataGridView.RowHeadersVisible = false;                // Sol taraftaki satır numaralarını gizle
                dataGridView.MultiSelect = false;                      // Tek seçim modu
                dataGridView.SelectionMode = DataGridViewSelectionMode.FullRowSelect; // Tam satır seçimi
                
                // 📊 DataGridView hazır - gerçek telemetri verisi bekleniyor
                
                Log("DataGridView sütunları başarıyla oluşturuldu (22 sütun + emoji'ler)", false);
            }
            catch (Exception ex)
            {
                Log($"DataGridView sütunları oluşturulurken hata: {ex.Message}", true);
            }
        }
        

        
        /// <summary>
        /// TextBox kontrollerini telemetri verisiyle günceller
        /// </summary>
        private void GoruntuleVeriler(string[] paket)
        {
            try
            {
                // GPS Latitude
                if (paket.Length > 12 && veriKontrolleri.ContainsKey("textBox1"))
                    veriKontrolleri["textBox1"].Text = paket[(int)TelemetriAlani.GpsLatitude]; // GPS Latitude
                
                // GPS Longitude
                if (paket.Length > 13 && veriKontrolleri.ContainsKey("textBox2"))
                    veriKontrolleri["textBox2"].Text = paket[(int)TelemetriAlani.GpsLongitude]; // GPS Longitude
                
                // GPS Altitude
                if (paket.Length > 14 && veriKontrolleri.ContainsKey("textBox3"))
                    veriKontrolleri["textBox3"].Text = paket[(int)TelemetriAlani.GpsAltitude]; // GPS Altitude
                
                // Basınç 1
                if (paket.Length > 4 && veriKontrolleri.ContainsKey("textBox4"))
                    veriKontrolleri["textBox4"].Text = paket[(int)TelemetriAlani.Basinc1]; // Basınç 1
                
                // Basınç 2
                if (paket.Length > 5 && veriKontrolleri.ContainsKey("textBox5"))
                    veriKontrolleri["textBox5"].Text = paket[(int)TelemetriAlani.Basinc2]; // Basınç 2
                
                // Uydu Statüsü
                string uyduStatusu = GetUyduStatusuText(paket[(int)TelemetriAlani.UyduStatusu]);
                if (paket.Length > 1 && veriKontrolleri.ContainsKey("textBox6"))
                    veriKontrolleri["textBox6"].Text = uyduStatusu; // Uydu Statüsü
                
                // labelUyduStatu güncellemesi
                if (labelUyduStatu != null)
                {
                    if (labelUyduStatu.InvokeRequired)
                    {
                        labelUyduStatu.Invoke(new Action(() => {
                            GuncelleUyduStatuUI(uyduStatusu);
                        }));
                    }
                    else
                    {
                        GuncelleUyduStatuUI(uyduStatusu);
                    }
                }
                
                if (paket.Length > 2 && veriKontrolleri.ContainsKey("textBox7"))
                    veriKontrolleri["textBox7"].Text = paket[2]; // Hata Kodu
                
                if (paket.Length > 3 && veriKontrolleri.ContainsKey("textBox8"))
                    veriKontrolleri["textBox8"].Text = paket[3]; // Gönderme Saati
                
                if (paket.Length > 6 && veriKontrolleri.ContainsKey("textBox9"))
                    veriKontrolleri["textBox9"].Text = paket[6]; // Yükseklik 1
                
                if (paket.Length > 7 && veriKontrolleri.ContainsKey("textBox10"))
                    veriKontrolleri["textBox10"].Text = paket[7]; // Yükseklik 2
                
                if (paket.Length > 8 && veriKontrolleri.ContainsKey("textBox11"))
                    veriKontrolleri["textBox11"].Text = paket[8]; // İrtifa Farkı
                
                if (paket.Length > 9 && veriKontrolleri.ContainsKey("textBox12"))
                    veriKontrolleri["textBox12"].Text = paket[9]; // İniş Hızı
                
                if (paket.Length > 15 && veriKontrolleri.ContainsKey("textBox13"))
                    veriKontrolleri["textBox13"].Text = paket[15]; // Pitch
                
                if (paket.Length > 16 && veriKontrolleri.ContainsKey("textBox14"))
                    veriKontrolleri["textBox14"].Text = paket[16]; // Roll
                
                if (paket.Length > 0 && veriKontrolleri.ContainsKey("textBox15"))
                    veriKontrolleri["textBox15"].Text = paket[0]; // Paket No

                // Sıcaklık verisi (textBox16 - yeni)
                if (paket.Length > 10 && veriKontrolleri.ContainsKey("textBox16"))
                    veriKontrolleri["textBox16"].Text = paket[10]; // Sıcaklık
                
                // Pil Gerilimi verisi (textBox17 - yeni)
                if (paket.Length > 11 && veriKontrolleri.ContainsKey("textBox17"))
                    veriKontrolleri["textBox17"].Text = paket[11]; // Pil Gerilimi
                
                // Yaw verisi (textBox18 - yeni)
                if (paket.Length > 17 && veriKontrolleri.ContainsKey("textBox18"))
                    veriKontrolleri["textBox18"].Text = paket[17]; // Yaw
                
                // RHRH Multi-spektral filtre durumu (textBox19 - yeni)
                if (paket.Length > 18 && veriKontrolleri.ContainsKey("textBox19"))
                    veriKontrolleri["textBox19"].Text = paket[18]; // RHRH
                
                // IoT İstasyon 1 Sıcaklık (textBox20 - yeni)
                if (paket.Length > 19 && veriKontrolleri.ContainsKey("textBox20"))
                    veriKontrolleri["textBox20"].Text = paket[19]; // IoT S1 Sıcaklık
                
                // IoT İstasyon 2 Sıcaklık (textBox21 - yeni)
                if (paket.Length > 20 && veriKontrolleri.ContainsKey("textBox21"))
                    veriKontrolleri["textBox21"].Text = paket[20]; // IoT S2 Sıcaklık
                
                // Takım No (textBox22 - yeni)
                if (paket.Length > 21 && veriKontrolleri.ContainsKey("textBox22"))
                    veriKontrolleri["textBox22"].Text = paket[21]; // Takım No
                
                // Takım Numarası Label'ını güncelle
                if (paket.Length > 21 && labelTakimNumarasi != null)
                {
                    string takimNo = paket[21].Trim();
                    if (labelTakimNumarasi.InvokeRequired)
                    {
                        labelTakimNumarasi.Invoke(new Action(() =>
                        {
                            labelTakimNumarasi.Text = $"Takım: {takimNo}";
                        }));
                    }
                    else
                    {
                        labelTakimNumarasi.Text = $"Takım: {takimNo}";
                    }
                }
            }
            catch (Exception ex)
            {
                Log($"TextBox'lar güncellenirken hata: {ex.Message}", true);
            }
        }
        
        private void GuncelleUyduStatuUI(string durum)
        {
            // Duruma göre renk belirle
            Color statusRengi;
            Color statusRengiAcik;
            Color statusRengiKoyu;

            // Durumu büyük harfe çevir
            durum = durum.Trim().ToUpper();

            // Duruma göre renk seç
            switch (durum)
            {
                case "HAZIR":
                    statusRengi = Color.FromArgb(255, 150, 0); // Turuncu
                    statusRengiAcik = Color.FromArgb(255, 180, 30);
                    statusRengiKoyu = Color.FromArgb(200, 100, 0);
                    break;
                case "YÜKSELME":
                    statusRengi = Color.FromArgb(0, 180, 0); // Yeşil
                    statusRengiAcik = Color.FromArgb(30, 200, 30);
                    statusRengiKoyu = Color.FromArgb(0, 130, 0);
                    break;
                case "M.U. İNİŞ":
                    statusRengi = Color.FromArgb(0, 100, 255); // Mavi
                    statusRengiAcik = Color.FromArgb(30, 150, 255);
                    statusRengiKoyu = Color.FromArgb(0, 70, 200);
                    break;
                case "KURTARMA":
                    statusRengi = Color.FromArgb(100, 0, 255); // Mor
                    statusRengiAcik = Color.FromArgb(150, 30, 255);
                    statusRengiKoyu = Color.FromArgb(80, 0, 200);
                    break;
                case "AYRILMA":
                    statusRengi = Color.FromArgb(255, 0, 0); // Kırmızı
                    statusRengiAcik = Color.FromArgb(255, 30, 30);
                    statusRengiKoyu = Color.FromArgb(200, 0, 0);
                    break;
                case "G.Y. İNİŞ":
                    statusRengi = Color.FromArgb(0, 180, 180); // Turkuaz
                    statusRengiAcik = Color.FromArgb(30, 200, 200);
                    statusRengiKoyu = Color.FromArgb(0, 130, 130);
                    break;
                default:
                    statusRengi = Color.FromArgb(80, 80, 80); // Gri
                    statusRengiAcik = Color.FromArgb(150, 150, 150);
                    statusRengiKoyu = Color.FromArgb(50, 50, 50);
                    durum = "BİLİNMİYOR";
                    break;
            }

            // Yazıyı ayarla
            labelUyduStatu.Text = durum;
            labelUyduStatu.ForeColor = Color.White;
            labelUyduStatu.BackColor = Color.Transparent;

            // Yazıyı koyu renkle gölgele
            labelUyduStatu.Font = new Font("Segoe UI", 12, FontStyle.Bold); // Font boyutunu 14'ten 12'ye düşürdük
            labelUyduStatu.AutoSize = true; // Label'ın içeriğe göre boyutlanmasını sağla

            // GroupBox'a dokulu arka plan oluştur
            var groupBox = (GroupBox)labelUyduStatu.Parent;
            if (groupBox != null)
            {
                groupBox.BackColor = Color.FromArgb(40, statusRengi);

                // Panel dokusunu oluşturmak için bitmap
                Bitmap bmp = new Bitmap(groupBox.Width, groupBox.Height);
                using (Graphics g = Graphics.FromImage(bmp))
                {
                    // Gradient arka plan
                    using (LinearGradientBrush gradientBrush = new LinearGradientBrush(
                        new Point(0, 0),
                        new Point(0, groupBox.Height),
                        statusRengiKoyu,
                        statusRengiAcik))
                    {
                        g.FillRectangle(gradientBrush, 0, 0, groupBox.Width, groupBox.Height);
                    }

                    // Doku efekti
                    for (int i = 0; i < groupBox.Height; i += 4)
                    {
                        g.DrawLine(new Pen(Color.FromArgb(10, Color.White), 1), 0, i, groupBox.Width, i);
                    }
                }

                // GroupBox'a bitmap'i arka plan olarak ata
                groupBox.BackgroundImage = bmp;
                groupBox.BackgroundImageLayout = ImageLayout.Stretch;

                // GroupBox başlığını güncelle
                groupBox.Text = "Uydu Statüsü";
                if (veriKontrolleri.ContainsKey("textBox3") && !string.IsNullOrEmpty(veriKontrolleri["textBox3"].Text))
                {
                    groupBox.Text += " - Yükseklik: " + veriKontrolleri["textBox3"].Text + " m";
                }

                groupBox.ForeColor = Color.White;
                groupBox.Font = new Font("Segoe UI", 9, FontStyle.Bold);
            }
        }
        
        /// <summary>
        /// Telemetri verilerini DataGridView'e ekler
        /// </summary>
        private void DataGridViewEkle(string[] paket)
        {
            try
            {
                // Check if we have enough data in the packet
                if (paket.Length < 19) // RHRH dahil minimum uzunluk
                {
                    Log($"Eksik telemetri paketi alındı. Beklenen: minimum 19, Alınan: {paket.Length}", false);
                    return;
                }

                // Parse data from the packet
                string paketNo = paket[0].Trim();
                string uyduStatusu = paket[1].Trim();
                string hataKodu = paket[2].Trim();
                string gondermeSaati = paket[3].Trim();
                string basinc1 = paket[4].Trim();
                string basinc2 = paket[5].Trim();
                string yukseklik1 = paket[6].Trim();
                string yukseklik2 = paket[7].Trim();
                string irtifaFarki = paket[8].Trim();
                string inisHizi = paket[9].Trim();
                string sicaklik = paket[10].Trim();
                string pilGerilimi = paket[11].Trim();
                string gpsLatitude = paket[12].Trim();
                string gpsLongitude = paket[13].Trim();
                string gpsAltitude = paket[14].Trim();
                string pitch = paket[15].Trim();
                string roll = paket[16].Trim();
                string yaw = paket[17].Trim();
                string rhrh = paket[18].Trim();

                // Takım No
                string takimNo = paket.Length >= 22 ? paket[21].Trim() : "14534";

                // Add row to DataGridView
                int satir = dataGridView.Rows.Add();
                dataGridView.Rows[satir].Cells[0].Value = paketNo;             // Paket No
                dataGridView.Rows[satir].Cells[1].Value = GetUyduStatusuText(uyduStatusu);         // Uydu Statüsü
                dataGridView.Rows[satir].Cells[2].Value = hataKodu;            // Hata Kodu
                dataGridView.Rows[satir].Cells[3].Value = gondermeSaati;       // Gönderme Saati
                dataGridView.Rows[satir].Cells[4].Value = basinc1;             // Basınç1
                dataGridView.Rows[satir].Cells[5].Value = basinc2;             // Basınç2
                dataGridView.Rows[satir].Cells[6].Value = yukseklik1;          // Yükseklik1
                dataGridView.Rows[satir].Cells[7].Value = yukseklik2;          // Yükseklik2
                dataGridView.Rows[satir].Cells[8].Value = irtifaFarki;         // İrtifa Farkı
                dataGridView.Rows[satir].Cells[9].Value = inisHizi;            // İniş Hızı
                dataGridView.Rows[satir].Cells[10].Value = sicaklik;           // Sıcaklık
                dataGridView.Rows[satir].Cells[11].Value = pilGerilimi;        // Pil Gerilimi
                dataGridView.Rows[satir].Cells[12].Value = gpsLatitude;        // GPS1 Latitude
                dataGridView.Rows[satir].Cells[13].Value = gpsLongitude;       // GPS1 Longitude
                dataGridView.Rows[satir].Cells[14].Value = gpsAltitude;        // GPS1 Altitude
                dataGridView.Rows[satir].Cells[15].Value = pitch;              // Pitch
                dataGridView.Rows[satir].Cells[16].Value = roll;               // Roll
                dataGridView.Rows[satir].Cells[17].Value = yaw;                // Yaw
                dataGridView.Rows[satir].Cells[18].Value = rhrh;               // RHRH
                dataGridView.Rows[satir].Cells[19].Value = iotYoneticisi.IoTS1Sicaklik;      // IoT S1 Data
                dataGridView.Rows[satir].Cells[20].Value = iotYoneticisi.IoTS2Sicaklik;      // IoT S2 Data
                dataGridView.Rows[satir].Cells[21].Value = takimNo;            // Takım No

                // 📊 TÜRKSAT Yarışması - VERİ KAYBI YASAK! 
                // Not: Tüm veriler SD karta ve Excel'e kaydediliyor, DataGridView sadece görsel
                // Performans için sadece uyarı veriyoruz, veri silmiyoruz
                const int performansUyariSiniri = 5000; // 5000 satır uyarısı
                if (dataGridView.Rows.Count > performansUyariSiniri && dataGridView.Rows.Count % 1000 == 0)
                {
                    Log($"📊 TÜRKSAT Uyarı: {dataGridView.Rows.Count} satır var, performans yavaşlayabilir. Tüm veriler SD karta kaydediliyor.", false);
                }

                // 📜 Otomatik scroll - yeni veriye odaklan
                if (dataGridView.Rows.Count > 0)
                {
                    // Son satırı seç ve görünür yap
                    int sonSatir = dataGridView.Rows.Count - 1;
                    dataGridView.CurrentCell = dataGridView.Rows[sonSatir].Cells[0];
                    dataGridView.FirstDisplayedScrollingRowIndex = sonSatir;
                    
                    // Son satırı vurgula (yeni veri indicator)
                    dataGridView.Rows[sonSatir].DefaultCellStyle.BackColor = System.Drawing.Color.FromArgb(255, 255, 200); // Açık sarı
                    
                    // Önceki satırın rengini normale döndür
                    if (sonSatir > 0)
                    {
                        dataGridView.Rows[sonSatir - 1].DefaultCellStyle.BackColor = System.Drawing.Color.White;
                    }
                }
            }
            catch (Exception ex)
            {
                Log($"DataGridView'e veri eklerken hata: {ex.Message}", true);
            }
        }
        
        /// <summary>
        /// Grafikleri günceller
        /// </summary>
        private void GuncelleGrafikler(string[] paket)
        {
            try
            {
                if (grafikYoneticisi != null)
                {
                    // GrafikYoneticisi sınıfında bulunan UpdateCharts metodunu çağır (Doğru alan indeksleri ile)
                    if (paket.Length > (int)TelemetriAlani.Sicaklik && double.TryParse(paket[(int)TelemetriAlani.Sicaklik], System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out double sicaklik) &&
                        paket.Length > (int)TelemetriAlani.Yukseklik1 && double.TryParse(paket[(int)TelemetriAlani.Yukseklik1], System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out double yukseklik) &&
                        paket.Length > (int)TelemetriAlani.Basinc1 && double.TryParse(paket[(int)TelemetriAlani.Basinc1], System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out double basinc1) &&
                        paket.Length > (int)TelemetriAlani.InisHizi && double.TryParse(paket[(int)TelemetriAlani.InisHizi], System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out double inisHizi) &&
                        paket.Length > (int)TelemetriAlani.PilGerilimi && double.TryParse(paket[(int)TelemetriAlani.PilGerilimi], System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out double pilGerilimi))
                    {
                        // IoT verilerini de al (güvenli parse ile)
                        string iotS1Data = "0";
                        string iotS2Data = "0";
                        
                        if (paket.Length > (int)TelemetriAlani.IoTS1Data)
                        {
                            iotS1Data = paket[(int)TelemetriAlani.IoTS1Data];
                        }
                        
                        if (paket.Length > (int)TelemetriAlani.IoTS2Data)
                        {
                            iotS2Data = paket[(int)TelemetriAlani.IoTS2Data];
                        }

                        // DateTime şu anki zamanı kullanarak grafiği güncelle (IoT verileri dahil)
                        grafikYoneticisi.UpdateAllCharts(
                            DateTime.Now, 
                            sicaklik.ToString(), 
                            yukseklik.ToString(), 
                            basinc1.ToString(), 
                            inisHizi.ToString(), 
                            pilGerilimi.ToString(),
                            iotS1Data,
                            iotS2Data
                        );
                    }
                }
            }
            catch (Exception ex)
            {
                Log($"Grafikler güncellenirken hata: {ex.Message}", false);
            }
        }
        
        /// <summary>
        /// 3D modeli, gelen gyro verilerine göre günceller
        /// </summary>
        private void Guncelle3DModel(string[] paket)
        {
            try
            {
                if (uyduGorsellestime != null && paket.Length > 17)
                {
                    // Pitch, Roll ve Yaw değerlerini güvenli bir şekilde al (decimal değerler)
                    if (!double.TryParse(paket[(int)TelemetriAlani.Pitch], System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out double pitch))
                    {
                        Log($"Geçersiz Pitch değeri: {paket[(int)TelemetriAlani.Pitch]}", true);
                        pitch = 0; // Hata durumunda varsayılan değer
                    }

                    if (!double.TryParse(paket[(int)TelemetriAlani.Roll], System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out double roll))
                    {
                        Log($"Geçersiz Roll değeri: {paket[(int)TelemetriAlani.Roll]}", true);
                        roll = 0; // Hata durumunda varsayılan değer
                    }

                    if (!double.TryParse(paket[(int)TelemetriAlani.Yaw], System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out double yaw))
                    {
                        Log($"Geçersiz Yaw değeri: {paket[(int)TelemetriAlani.Yaw]}", true);
                        yaw = 0; // Hata durumunda varsayılan değer
                    }
                    
                    // Debug: IMU değerlerini logla
                    Log($"3D Model güncelleniyor: P={pitch:F1}° R={roll:F1}° Y={yaw:F1}°", false);
                    
                    uyduGorsellestime.SetGyroValues(pitch, roll, yaw);
                }
            }
            catch (Exception ex)
            {
                Log($"3D model güncellenirken hata: {ex.Message}", true);
            }
        }
        
        /// <summary>
        /// Harita üzerindeki uydu konumunu günceller
        /// </summary>
        private void GuncelleHarita(string[] paket)
        {
            try
            {
                if (haritaYoneticisi != null && paket.Length > 13)
                {
                    if (double.TryParse(paket[(int)TelemetriAlani.GpsLatitude], System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out double lat) &&
                        double.TryParse(paket[(int)TelemetriAlani.GpsLongitude], System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out double lon))
                    {
                        // Sadece geçerli koordinatlar ise haritayı güncelle
                        if (lat != 0 && lon != 0)
                        {
                            haritaYoneticisi.UpdateMap(lat, lon);
                        }
                    }
                    else
                    {
                        Log($"Geçersiz GPS koordinat değeri: Lat={paket[(int)TelemetriAlani.GpsLatitude]}, Lon={paket[(int)TelemetriAlani.GpsLongitude]}", true);
                    }
                }
            }
            catch (Exception ex)
            {
                Log($"Harita güncellenirken hata: {ex.Message}", true);
            }
        }
        
        /// <summary>
        /// IoT verilerini günceller
        /// </summary>
        private void GuncelleIoTVerileri(string[] paket)
        {
            try
            {
                if (iotYoneticisi != null)
                {
                    // IoT verilerini güncelle
                    iotYoneticisi.TelemetridenIoTVerileriniGuncelle(paket);
                }
            }
            catch (Exception ex)
            {
                Log($"IoT verileri güncellenirken hata: {ex.Message}", false);
            }
        }
        
        /// <summary>
        /// Hata log işlemi
        /// </summary>
        private void Log(string mesaj, bool kritikMi = false)
        {
            logHataFonksiyonu?.Invoke(mesaj, kritikMi);
        }
        
        /// <summary>
        /// Uydu statüsü sayısal değerini anlamlı metne çevirir
        /// </summary>
        private string GetUyduStatusuText(string statusKodu)
        {
            try
            {
                // Boş veya null kontrolü
                if (string.IsNullOrEmpty(statusKodu))
                {
                    Log("Uydu statüsü boş geldi!", true);
                    return "BİLİNMİYOR";
                }

                // Trim yapıp sayıya çevirmeyi dene
                statusKodu = statusKodu.Trim();
                
                // TÜRKSAT şartnamesi uydu statüsü kodları
                switch (statusKodu)
                {
                    case "0": return "HAZIR";
                    case "1": return "YÜKSELME";
                    case "2": return "M.U. İNİŞ"; // Model Uydu İniş kısaltması
                    case "3": return "AYRILMA";
                    case "4": return "G.Y. İNİŞ"; // Görev Yükü İniş kısaltması
                    case "5": return "KURTARMA";
                    default:
                        Log($"Bilinmeyen uydu statüsü kodu: {statusKodu}", true);
                        return "BİLİNMİYOR";
                }
            }
            catch (Exception ex)
            {
                Log($"Uydu statüsü çevrilirken hata: {ex.Message}", true);
                return "BİLİNMİYOR";
            }
        }
        
        #endregion
        
        #region IDisposable Implementation
        
        /// <summary>
        /// Kaynakları temizler
        /// </summary>
        public void Dispose()
        {
            try
            {
                // Veri listelerini temizle
                telemetriVerileri.Clear();
                telemetriVerileri = null;
                
                // Referansları temizle
                dataGridView = null;
                veriKontrolleri = null;
                grafikYoneticisi = null;
                uyduGorsellestime = null;
                haritaYoneticisi = null;
                iotYoneticisi = null;
                alarmSistemiYoneticisi = null;
                uyduDurumuGuncelleDelegesi = null;
                logHataFonksiyonu = null;
                multiSpektralFiltreYoneticisi = null;
                
                Log("Telemetri Yöneticisi kaynakları temizlendi.", false);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Telemetri Yöneticisi kaynakları temizlenirken hata: {ex.Message}");
            }
        }
        
        #endregion
        
        #region Public Properties (VERİLERİ GÖSTER butonu için)
        
        /// <summary>
        /// Son alınan telemetri verilerini döndürür
        /// </summary>
        public string[] SonTelemetriVerileri
        {
            get { return sonTelemetriVerileri; }
        }
        
        /// <summary>
        /// Tüm ham telemetri verilerini döndürür (Excel aktarımı için)
        /// </summary>
        public List<string[]> HamTelemetriVerileri
        {
            get { return telemetriVerileri; }
        }
        
        /// <summary>
        /// Toplam alınan paket sayısını döndürür
        /// </summary>
        public int ToplamPaketSayisi
        {
            get { return toplamPaketSayisi; }
        }
        
        /// <summary>
        /// Son telemetri paketini formatlanmış şekilde döndürür
        /// </summary>
        public string SonTelemetriFormatli
        {
            get
            {
                if (sonTelemetriVerileri == null || sonTelemetriVerileri.Length < BeklenenPaketUzunlugu)
                {
                    return "Henüz telemetri verisi alınmadı.";
                }
                
                StringBuilder sb = new StringBuilder();
                try
                {
                    sb.AppendLine($"📦 Paket No: {sonTelemetriVerileri[0]}");
                    sb.AppendLine($"🛰️ Uydu Statüsü: {GetUyduStatusuText(sonTelemetriVerileri[1])}");
                    sb.AppendLine($"⚠️ Hata Kodu: {sonTelemetriVerileri[2]}");
                    sb.AppendLine($"⏰ Gönderme Saati: {sonTelemetriVerileri[3]}");
                    sb.AppendLine($"🌡️ Sıcaklık: {sonTelemetriVerileri[10]}°C");
                    sb.AppendLine($"🔋 Pil Gerilimi: {sonTelemetriVerileri[11]}V");
                    sb.AppendLine($"📏 Yükseklik: {sonTelemetriVerileri[6]}m");
                    sb.AppendLine($"🏃 İniş Hızı: {sonTelemetriVerileri[9]}m/s");
                    sb.AppendLine($"📍 GPS: {sonTelemetriVerileri[12]}, {sonTelemetriVerileri[13]}");
                    sb.AppendLine($"🎯 Takım No: {sonTelemetriVerileri[21]}");
                }
                catch (Exception)
                {
                    return "Telemetri verisi formatlanırken hata oluştu.";
                }
                
                return sb.ToString();
            }
        }
        
        #endregion
    }
} 