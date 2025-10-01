using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing;
using System.IO;
using OfficeOpenXml;
using OfficeOpenXml.Style;

namespace ModelUydu
{
    /// <summary>
    /// Modern Excel veri aktarma işlemlerini gerçekleştiren sınıf (EPPlus kullanarak).
    /// Microsoft.Office.Interop yerine hafif ve hızlı EPPlus kütüphanesi kullanır.
    /// </summary>
    public class ExcelVeriAktarmaYoneticisi
    {
        // Ana formun referansı (UI Thread kontrolü için)
        private Form anaForm;
        
        // Hata yönetici referansı
        private HataYoneticisi hataYoneticisi;

        // Telemetri yöneticisi referansı (ham veriler için)
        private TelemetriYoneticisi telemetriYoneticisi;

        /// <summary>
        /// ExcelVeriAktarmaYoneticisi sınıfı yapıcı metodu
        /// </summary>
        /// <param name="anaForm">Ana form referansı</param>
        /// <param name="hataYoneticisi">Hata yöneticisi referansı</param>
        /// <param name="telemetriYoneticisi">Telemetri yöneticisi referansı (ham veriler için)</param>
        public ExcelVeriAktarmaYoneticisi(Form anaForm, HataYoneticisi hataYoneticisi, TelemetriYoneticisi telemetriYoneticisi = null)
        {
            this.anaForm = anaForm;
            this.hataYoneticisi = hataYoneticisi;
            this.telemetriYoneticisi = telemetriYoneticisi;
            
            // EPPlus lisans ayarı (non-commercial use için)
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
        }

        /// <summary>
        /// DataGridView'den Excel'e veri aktarır (EPPlus kullanarak)
        /// </summary>
        /// <param name="dataGridView">Veri kaynağı DataGridView</param>
        /// <param name="baslik">Excel dosyasının başlığı</param>
        /// <returns>İşlem başarılı ise true, değilse false</returns>
        public bool DataGridViewVerileriniAktar(DataGridView dataGridView, string baslik = "Model Uydu Telemetri Verileri")
        {
            try
            {
                // Dosya kaydetme dialogu
                using (SaveFileDialog saveDialog = new SaveFileDialog())
                {
                    saveDialog.Filter = "Excel Files (*.xlsx)|*.xlsx|All Files (*.*)|*.*";
                    saveDialog.Title = "Excel Dosyasını Kaydet";
                    saveDialog.FileName = $"ModelUydu_Telemetri_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";

                    if (saveDialog.ShowDialog() != DialogResult.OK)
                        return false;

                    // EPPlus ile Excel dosyası oluştur
                    using (ExcelPackage excel = new ExcelPackage())
                    {
                        // Yeni worksheet ekle
                        ExcelWorksheet worksheet = excel.Workbook.Worksheets.Add("Telemetri Verileri");

                        // Başlık ekle (A1 hücresinde)
                        string basliktamami = baslik + " - " + DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss");
                        worksheet.Cells[1, 1].Value = basliktamami;
                        
                        // Başlık formatı
                        using (ExcelRange titleRange = worksheet.Cells[1, 1, 1, dataGridView.Columns.Count])
                        {
                            titleRange.Merge = true;
                            titleRange.Style.Font.Bold = true;
                            titleRange.Style.Font.Size = 14;
                            titleRange.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                            titleRange.Style.Fill.PatternType = ExcelFillStyle.Solid;
                            titleRange.Style.Fill.BackgroundColor.SetColor(Color.LightBlue);
                        }

                        // Sütun başlıklarını ekle (2. satır)
                        for (int i = 0; i < dataGridView.Columns.Count; i++)
                        {
                            worksheet.Cells[2, i + 1].Value = dataGridView.Columns[i].HeaderText;
                        }

                        // Başlık satırını formatla
                        using (ExcelRange headerRange = worksheet.Cells[2, 1, 2, dataGridView.Columns.Count])
                        {
                            headerRange.Style.Font.Bold = true;
                            headerRange.Style.Fill.PatternType = ExcelFillStyle.Solid;
                            headerRange.Style.Fill.BackgroundColor.SetColor(Color.LightGray);
                            headerRange.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                        }

                        // HAM VERİLERİ KULLAN (TelemetriYoneticisi'nden alınan veriler)
                        if (telemetriYoneticisi != null && telemetriYoneticisi.HamTelemetriVerileri != null)
                        {
                            var hamVeriler = telemetriYoneticisi.HamTelemetriVerileri;
                            
                            // Ham verileri Excel'e ekle (3. satırdan başlayarak)
                            for (int i = 0; i < hamVeriler.Count; i++)
                        {
                                var paket = hamVeriler[i];
                                if (paket != null && paket.Length >= 22) // TÜRKSAT formatı 22 alan
                            {
                                    // Her alanı formatına göre ekle
                                    for (int j = 0; j < Math.Min(paket.Length, dataGridView.Columns.Count); j++)
                                {
                                        var veri = paket[j];
                                    var headerText = dataGridView.Columns[j].HeaderText;

                                        EkselHucreFormatla(worksheet, i + 3, j + 1, veri, headerText, j);
                                    }
                                }
                            }
                                    }
                                    else
                                    {
                            // Fallback: DataGridView verilerini kullan (eski metod)
                            for (int i = 0; i < dataGridView.Rows.Count; i++)
                            {
                                for (int j = 0; j < dataGridView.Columns.Count; j++)
                                {
                                    if (dataGridView.Rows[i].Cells[j].Value != null)
                                    {
                                        var cellValue = dataGridView.Rows[i].Cells[j].Value.ToString();
                                        var headerText = dataGridView.Columns[j].HeaderText;
                                        
                                        EkselHucreFormatla(worksheet, i + 3, j + 1, cellValue, headerText, j);
                                    }
                                }
                            }
                        }

                        // Tüm verilere kenarlık ekle
                        using (ExcelRange dataRange = worksheet.Cells[1, 1, dataGridView.Rows.Count + 2, dataGridView.Columns.Count])
                        {
                            dataRange.Style.Border.Top.Style = ExcelBorderStyle.Thin;
                            dataRange.Style.Border.Left.Style = ExcelBorderStyle.Thin;
                            dataRange.Style.Border.Right.Style = ExcelBorderStyle.Thin;
                            dataRange.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                        }

                        // Sütun genişliklerini otomatik ayarla
                        worksheet.Cells.AutoFitColumns();

                        // Dosyayı kaydet
                        FileInfo fileInfo = new FileInfo(saveDialog.FileName);
                        excel.SaveAs(fileInfo);
                    }
                }

                // Kullanıcıya bilgi ver
                hataYoneticisi.BilgiGoster("Veriler Excel'e başarıyla aktarıldı!", "Başarılı");
                
                return true;
            }
            catch (Exception ex)
            {
                hataYoneticisi.LogException(ex, "DataGridViewVerileriniAktar", true);
                return false;
            }
        }

        /// <summary>
        /// Excel hücresini veri tipine göre formatlar
        /// </summary>
        /// <param name="worksheet">Excel worksheet</param>
        /// <param name="row">Satır numarası</param>
        /// <param name="col">Sütun numarası</param>
        /// <param name="veri">Ham veri</param>
        /// <param name="headerText">Sütun başlığı</param>
        /// <param name="columnIndex">Sütun indeksi</param>
        private void EkselHucreFormatla(ExcelWorksheet worksheet, int row, int col, string veri, string headerText, int columnIndex)
        {
            try
            {
                if (string.IsNullOrEmpty(veri))
                {
                    worksheet.Cells[row, col].Value = "";
                    return;
                }

                // TÜRKSAT Telemetri alanlarına göre formatla
                switch (columnIndex)
                {
                    case 0: // Paket Numarası - tam sayı
                        if (int.TryParse(veri, out int paketNo))
                        {
                            worksheet.Cells[row, col].Value = paketNo;
                            worksheet.Cells[row, col].Style.Numberformat.Format = "0";
                        }
                        else
                        {
                            worksheet.Cells[row, col].Value = veri;
                        }
                        break;

                    case 1: // Uydu Statüsü - orijinal kod değeri
                        worksheet.Cells[row, col].Value = veri; // Ham değeri koru (0,1,2,3,4,5)
                        break;

                    case 2: // Hata Kodu - 6 haneli string
                        worksheet.Cells[row, col].Value = veri; // Hata kodu string olarak kalmalı
                        break;

                    case 3: // Gönderme Saati - zaman formatı
                        worksheet.Cells[row, col].Value = veri;
                        break;

                    case 4: case 5: // Basınç1, Basınç2 - ondalıklı sayılar
                    case 6: case 7: // Yükseklik1, Yükseklik2
                    case 8: case 9: // İrtifa Farkı, İniş Hızı
                    case 11: // Pil Gerilimi
                    case 12: case 13: case 14: // GPS koordinatları
                    case 15: case 16: case 17: // Pitch, Roll, Yaw
                    case 18: // RHRH
                    case 19: case 20: // IoT S1, S2
                        if (double.TryParse(veri.Replace(',', '.'), System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out double numValue))
                        {
                            worksheet.Cells[row, col].Value = numValue;
                            
                            // Özel formatlar
                            if (columnIndex == 10) // Sıcaklık
                            {
                                worksheet.Cells[row, col].Style.Numberformat.Format = "0.0"; // 1 ondalık
                            }
                            else if (columnIndex == 11) // Pil Gerilimi
                            {
                                worksheet.Cells[row, col].Style.Numberformat.Format = "0.00"; // 2 ondalık
                            }
                            else if (columnIndex >= 12 && columnIndex <= 14) // GPS koordinatları
                            {
                                worksheet.Cells[row, col].Style.Numberformat.Format = "0.000000"; // 6 ondalık
                            }
                            else
                            {
                                worksheet.Cells[row, col].Style.Numberformat.Format = "0.0"; // Genel 1 ondalık
                            }
                        }
                        else
                        {
                            worksheet.Cells[row, col].Value = veri;
                        }
                        break;

                    case 10: // Sıcaklık - özel format
                        if (double.TryParse(veri.Replace(',', '.'), System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out double sicaklik))
                        {
                            worksheet.Cells[row, col].Value = sicaklik;
                            worksheet.Cells[row, col].Style.Numberformat.Format = "0.0"; // 1 ondalık yeterli
                        }
                        else
                        {
                            worksheet.Cells[row, col].Value = veri;
                        }
                        break;

                    case 21: // Takım No - tam sayı
                        if (int.TryParse(veri, out int takimNo))
                        {
                            worksheet.Cells[row, col].Value = takimNo;
                            worksheet.Cells[row, col].Style.Numberformat.Format = "0";
                        }
                        else
                        {
                            worksheet.Cells[row, col].Value = veri;
                        }
                        break;

                    default:
                        // Diğer alanlar için genel kural
                        if (double.TryParse(veri.Replace(',', '.'), System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out double genel))
                        {
                            worksheet.Cells[row, col].Value = genel;
                            worksheet.Cells[row, col].Style.Numberformat.Format = "0.0";
                        }
                        else
                        {
                            worksheet.Cells[row, col].Value = veri;
                        }
                        break;
                }
            }
            catch (Exception)
            {
                // Hata durumunda ham veriyi yaz
                worksheet.Cells[row, col].Value = veri;
            }
        }

        /// <summary>
        /// Metin tabanlı uydu statüsünü sayısal koduna çevirir.
        /// </summary>
        /// <param name="statusText">Metin olarak uydu statüsü</param>
        /// <returns>Sayısal uydu statü kodu</returns>
        private int GetUyduStatusuKod(string statusText)
        {
            switch (statusText.Trim().ToUpper())
            {
                case "HAZIR": return 0;
                case "YÜKSELME": return 1;
                case "M.U. İNİŞ": return 2;
                case "AYRILMA": return 3;
                case "G.Y. İNİŞ": return 4;
                case "KURTARMA": return 5;
                default: return -1; // Bilinmeyen durum
            }
        }

        /// <summary>
        /// IoT verilerini Excel'e aktarır (EPPlus kullanarak)
        /// </summary>
        /// <param name="iotVerileri">IoT verileri listesi</param>
        /// <returns>İşlem başarılı ise true, değilse false</returns>
        public bool IoTVerileriniAktar(List<IoTVeri> iotVerileri)
        {
            try
            {
                // Dosya kaydetme dialogu
                using (SaveFileDialog saveDialog = new SaveFileDialog())
                {
                    saveDialog.Filter = "Excel Files (*.xlsx)|*.xlsx|All Files (*.*)|*.*";
                    saveDialog.Title = "IoT Verilerini Excel'e Kaydet";
                    saveDialog.FileName = $"IoT_Verileri_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";

                    if (saveDialog.ShowDialog() != DialogResult.OK)
                        return false;

                    // EPPlus ile Excel dosyası oluştur
                    using (ExcelPackage excel = new ExcelPackage())
                    {
                        // Yeni worksheet ekle
                        ExcelWorksheet worksheet = excel.Workbook.Worksheets.Add("IoT Verileri");

                // Başlık ekle
                        worksheet.Cells[1, 1].Value = "IoT İstasyonu Verileri - " + DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss");
                        
                        // Başlık formatı
                        using (ExcelRange titleRange = worksheet.Cells[1, 1, 1, 5])
                        {
                            titleRange.Merge = true;
                            titleRange.Style.Font.Bold = true;
                            titleRange.Style.Font.Size = 14;
                            titleRange.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                            titleRange.Style.Fill.PatternType = ExcelFillStyle.Solid;
                            titleRange.Style.Fill.BackgroundColor.SetColor(Color.LightGreen);
                        }

                // Sütun başlıklarını ekle
                        worksheet.Cells[2, 1].Value = "Zaman";
                        worksheet.Cells[2, 2].Value = "İstasyon 1 Sıcaklık (°C)";
                        worksheet.Cells[2, 3].Value = "İstasyon 2 Sıcaklık (°C)";
                        worksheet.Cells[2, 4].Value = "Batarya Durumu (%)";
                        worksheet.Cells[2, 5].Value = "Bağlantı Durumu";

                        // Başlık satırını formatla
                        using (ExcelRange headerRange = worksheet.Cells[2, 1, 2, 5])
                        {
                            headerRange.Style.Font.Bold = true;
                            headerRange.Style.Fill.PatternType = ExcelFillStyle.Solid;
                            headerRange.Style.Fill.BackgroundColor.SetColor(Color.LightGray);
                        }

                // Verileri ekle
                for (int i = 0; i < iotVerileri.Count; i++)
                {
                    IoTVeri veri = iotVerileri[i];
                            int row = i + 3;
                            
                            worksheet.Cells[row, 1].Value = veri.Zaman.ToString("dd.MM.yyyy HH:mm:ss");
                            worksheet.Cells[row, 2].Value = veri.S1Sicaklik;
                            worksheet.Cells[row, 2].Style.Numberformat.Format = "0.0";
                            worksheet.Cells[row, 3].Value = veri.S2Sicaklik;
                            worksheet.Cells[row, 3].Style.Numberformat.Format = "0.0";
                            worksheet.Cells[row, 4].Value = veri.BataryaDurumu;
                            worksheet.Cells[row, 5].Value = veri.BaglantiDurumu ? "Bağlı" : "Bağlantı Yok";
                            
                            // Bağlantı durumuna göre renklendirme
                            if (!veri.BaglantiDurumu)
                            {
                                worksheet.Cells[row, 5].Style.Fill.PatternType = ExcelFillStyle.Solid;
                                worksheet.Cells[row, 5].Style.Fill.BackgroundColor.SetColor(Color.LightCoral);
                            }
                        }

                        // Tüm verilere kenarlık ekle
                        using (ExcelRange dataRange = worksheet.Cells[1, 1, iotVerileri.Count + 2, 5])
                        {
                            dataRange.Style.Border.Top.Style = ExcelBorderStyle.Thin;
                            dataRange.Style.Border.Left.Style = ExcelBorderStyle.Thin;
                            dataRange.Style.Border.Right.Style = ExcelBorderStyle.Thin;
                            dataRange.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                        }

                        // Sütun genişliklerini otomatik ayarla
                        worksheet.Cells.AutoFitColumns();

                        // Dosyayı kaydet
                        FileInfo fileInfo = new FileInfo(saveDialog.FileName);
                        excel.SaveAs(fileInfo);
                    }
                }

                // Kullanıcıya bilgi ver
                hataYoneticisi.BilgiGoster("IoT verileri Excel'e başarıyla aktarıldı!", "Başarılı");
                
                return true;
            }
            catch (Exception ex)
            {
                hataYoneticisi.LogException(ex, "IoTVerileriniAktar", true);
                return false;
            }
        }

        /// <summary>
        /// Hata log verilerini Excel'e aktarır (EPPlus kullanarak)
        /// </summary>
        /// <param name="logDosyaYolu">Log dosyasının yolu</param>
        /// <returns>İşlem başarılı ise true, değilse false</returns>
        public bool HataLoglariniAktar(string logDosyaYolu)
        {
            try
            {
                // Dosyanın var olup olmadığını kontrol et
                if (!File.Exists(logDosyaYolu))
                {
                    hataYoneticisi.LogHata("Log dosyası bulunamadı: " + logDosyaYolu);
                    return false;
                }

                // Log dosyasını oku
                string[] logSatirlari = File.ReadAllLines(logDosyaYolu);

                // Dosya kaydetme dialogu
                using (SaveFileDialog saveDialog = new SaveFileDialog())
                {
                    saveDialog.Filter = "Excel Files (*.xlsx)|*.xlsx|All Files (*.*)|*.*";
                    saveDialog.Title = "Hata Loglarını Excel'e Kaydet";
                    saveDialog.FileName = $"Hata_Loglari_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";

                    if (saveDialog.ShowDialog() != DialogResult.OK)
                    return false;

                    // EPPlus ile Excel dosyası oluştur
                    using (ExcelPackage excel = new ExcelPackage())
                    {
                        // Yeni worksheet ekle
                        ExcelWorksheet worksheet = excel.Workbook.Worksheets.Add("Hata Logları");

                        // Başlık ekle
                        worksheet.Cells[1, 1].Value = "Model Uydu Hata Logları - " + DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss");
                        
                        // Başlık formatı
                        using (ExcelRange titleRange = worksheet.Cells[1, 1, 1, 3])
                        {
                            titleRange.Merge = true;
                            titleRange.Style.Font.Bold = true;
                            titleRange.Style.Font.Size = 14;
                            titleRange.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                            titleRange.Style.Fill.PatternType = ExcelFillStyle.Solid;
                            titleRange.Style.Fill.BackgroundColor.SetColor(Color.LightCoral);
                        }

                        // Sütun başlıklarını ekle
                        worksheet.Cells[2, 1].Value = "Zaman";
                        worksheet.Cells[2, 2].Value = "Hata Seviyesi";
                        worksheet.Cells[2, 3].Value = "Hata Mesajı";

                        // Başlık satırını formatla
                        using (ExcelRange headerRange = worksheet.Cells[2, 1, 2, 3])
                        {
                            headerRange.Style.Font.Bold = true;
                            headerRange.Style.Fill.PatternType = ExcelFillStyle.Solid;
                            headerRange.Style.Fill.BackgroundColor.SetColor(Color.LightGray);
                        }

                        // Log verilerini işle ve ekle
                        for (int i = 0; i < logSatirlari.Length; i++)
                        {
                            string satir = logSatirlari[i];
                            if (!string.IsNullOrWhiteSpace(satir))
                            {
                                // Log satırını ayrıştır: "Zaman - Mesaj"
                                string[] parcalar = satir.Split(new[] { " - " }, 2, StringSplitOptions.None);
                                
                                int row = i + 3;
                                
                                if (parcalar.Length >= 2)
                                {
                                    worksheet.Cells[row, 1].Value = parcalar[0]; // Zaman
                                    
                                    // Hata seviyesini belirle
                                    string mesaj = parcalar[1];
                                    string seviye = "BİLGİ";
                                    Color rowColor = Color.White;
                                    
                                    if (mesaj.Contains("HATA"))
                                    {
                                        seviye = "HATA";
                                        rowColor = Color.LightCoral;
                                    }
                                    else if (mesaj.Contains("UYARI"))
                                    {
                                        seviye = "UYARI";
                                        rowColor = Color.LightYellow;
                                    }
                                    else if (mesaj.Contains("İSTİSNA"))
                                    {
                                        seviye = "İSTİSNA";
                                        rowColor = Color.LightPink;
                                    }
                                    
                                    worksheet.Cells[row, 2].Value = seviye;
                                    worksheet.Cells[row, 3].Value = mesaj;
                                    
                                    // Satır rengini ayarla
                                    if (rowColor != Color.White)
                                    {
                                        using (ExcelRange rowRange = worksheet.Cells[row, 1, row, 3])
                                        {
                                            rowRange.Style.Fill.PatternType = ExcelFillStyle.Solid;
                                            rowRange.Style.Fill.BackgroundColor.SetColor(rowColor);
                                        }
                                    }
                                }
                                else
                                {
                                    worksheet.Cells[row, 1].Value = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                                    worksheet.Cells[row, 2].Value = "BİLGİ";
                                    worksheet.Cells[row, 3].Value = satir;
                                }
                            }
                        }

                        // Kenarlık ekle
                        using (ExcelRange dataRange = worksheet.Cells[1, 1, logSatirlari.Length + 2, 3])
                        {
                            dataRange.Style.Border.Top.Style = ExcelBorderStyle.Thin;
                            dataRange.Style.Border.Left.Style = ExcelBorderStyle.Thin;
                            dataRange.Style.Border.Right.Style = ExcelBorderStyle.Thin;
                            dataRange.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                        }

                        // Sütun genişliklerini ayarla
                        worksheet.Column(1).Width = 20; // Zaman sütunu
                        worksheet.Column(2).Width = 15; // Seviye sütunu
                        worksheet.Column(3).Width = 50; // Mesaj sütunu

                        // Dosyayı kaydet
                        FileInfo fileInfo = new FileInfo(saveDialog.FileName);
                        excel.SaveAs(fileInfo);
                    }
                }

                // Kullanıcıya bilgi ver
                hataYoneticisi.BilgiGoster("Hata logları Excel'e başarıyla aktarıldı!", "Başarılı");
                
                return true;
            }
            catch (Exception ex)
            {
                hataYoneticisi.LogException(ex, "HataLoglariniAktar", true);
                return false;
            }
        }

        /// <summary>
        /// Özel rapor oluşturur (EPPlus kullanarak)
        /// </summary>
        /// <param name="baslik">Rapor başlığı</param>
        /// <param name="sutunBasliklari">Sütun başlıkları</param>
        /// <param name="veriler">Veri matrisi</param>
        /// <returns>İşlem başarılı ise true, değilse false</returns>
        public bool OzelRaporOlustur(string baslik, List<string> sutunBasliklari, List<List<string>> veriler)
        {
            try
            {
                // Dosya kaydetme dialogu
                using (SaveFileDialog saveDialog = new SaveFileDialog())
                {
                    saveDialog.Filter = "Excel Files (*.xlsx)|*.xlsx|All Files (*.*)|*.*";
                    saveDialog.Title = "Özel Raporu Kaydet";
                    saveDialog.FileName = $"Ozel_Rapor_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";

                    if (saveDialog.ShowDialog() != DialogResult.OK)
                        return false;

                    // EPPlus ile Excel dosyası oluştur
                    using (ExcelPackage excel = new ExcelPackage())
                    {
                        // Yeni worksheet ekle
                        ExcelWorksheet worksheet = excel.Workbook.Worksheets.Add("Özel Rapor");

                // Başlık ekle
                        worksheet.Cells[1, 1].Value = baslik + " - " + DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss");
                        
                        // Başlık formatı
                        using (ExcelRange titleRange = worksheet.Cells[1, 1, 1, sutunBasliklari.Count])
                        {
                            titleRange.Merge = true;
                            titleRange.Style.Font.Bold = true;
                            titleRange.Style.Font.Size = 14;
                            titleRange.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                            titleRange.Style.Fill.PatternType = ExcelFillStyle.Solid;
                            titleRange.Style.Fill.BackgroundColor.SetColor(Color.LightBlue);
                        }

                        // Sütun başlıklarını ekle
                for (int i = 0; i < sutunBasliklari.Count; i++)
                {
                            worksheet.Cells[2, i + 1].Value = sutunBasliklari[i];
                        }

                        // Başlık satırını formatla
                        using (ExcelRange headerRange = worksheet.Cells[2, 1, 2, sutunBasliklari.Count])
                        {
                            headerRange.Style.Font.Bold = true;
                            headerRange.Style.Fill.PatternType = ExcelFillStyle.Solid;
                            headerRange.Style.Fill.BackgroundColor.SetColor(Color.LightGray);
                        }

                        // Verileri ekle
                for (int i = 0; i < veriler.Count; i++)
                {
                            for (int j = 0; j < veriler[i].Count && j < sutunBasliklari.Count; j++)
                            {
                                worksheet.Cells[i + 3, j + 1].Value = veriler[i][j];
                            }
                        }

                        // Kenarlık ekle
                        using (ExcelRange dataRange = worksheet.Cells[1, 1, veriler.Count + 2, sutunBasliklari.Count])
                        {
                            dataRange.Style.Border.Top.Style = ExcelBorderStyle.Thin;
                            dataRange.Style.Border.Left.Style = ExcelBorderStyle.Thin;
                            dataRange.Style.Border.Right.Style = ExcelBorderStyle.Thin;
                            dataRange.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                        }

                        // Sütun genişliklerini otomatik ayarla
                        worksheet.Cells.AutoFitColumns();

                        // Dosyayı kaydet
                        FileInfo fileInfo = new FileInfo(saveDialog.FileName);
                        excel.SaveAs(fileInfo);
                    }
                }

                // Kullanıcıya bilgi ver
                hataYoneticisi.BilgiGoster("Özel rapor başarıyla oluşturuldu!", "Başarılı");
                
                return true;
            }
            catch (Exception ex)
            {
                hataYoneticisi.LogException(ex, "OzelRaporOlustur", true);
                return false;
            }
        }

        /// <summary>
        /// IoT veri sınıfı
        /// </summary>
        public class IoTVeri
        {
            public DateTime Zaman { get; set; }
            public double S1Sicaklik { get; set; }
            public double S2Sicaklik { get; set; }
            public int BataryaDurumu { get; set; }
            public bool BaglantiDurumu { get; set; }

            public IoTVeri(DateTime zaman, double s1Sicaklik, double s2Sicaklik, int bataryaDurumu, bool baglantiDurumu)
            {
                Zaman = zaman;
                S1Sicaklik = s1Sicaklik;
                S2Sicaklik = s2Sicaklik;
                BataryaDurumu = bataryaDurumu;
                BaglantiDurumu = baglantiDurumu;
            }
        }

        /// <summary>
        /// Kaynakları temizle
        /// </summary>
        public void Dispose()
        {
            // EPPlus için özel dispose işlemi gerekmez
            hataYoneticisi?.LogBilgi("ExcelVeriAktarmaYoneticisi kaynakları temizlendi.");
        }
    }
} 