using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using OpenTK; // 3D grafik işlemleri için OpenTK kütüphanesi
using OpenTK.Graphics.OpenGL; // OpenGL grafik işlemleri için gerekli namespace

namespace ModelUydu
{
    /// <summary>
    /// 3D uydu görselleştirme, OpenGL işlemleri, animasyon ve çizim işlemlerini içeren sınıf
    /// </summary>
    public class UyduGorsellestime
    {
        // 3D koordinat değişkenleri
        private int x = 0, y = 0, z = 0;

        // Koordinat değişim bayrakları
        private bool cx = false, cy = false, cz = false;

        // Timer değişkeni
        private System.Windows.Forms.Timer timerXYZ;

        // OpenGL kontrol
        private OpenTK.GLControl glControl;

        // Form1'den referanslar
        private Label label3, label4, label5; // Koordinat etiketleri
        private TextBox textBox6; // Uydu durumu
        
        // Hata loglamak için delegate
        private Action<string, bool> logHataFunc;

        /// <summary>
        /// UyduGorsellestime sınıfı yapıcı metodu
        /// </summary>
        /// <param name="glControl">OpenGL kontrol referansı</param>
        /// <param name="label3">X(Pitch) değeri etiketi</param>
        /// <param name="label4">Y(Roll) değeri etiketi</param>
        /// <param name="label5">Z(Yaw) değeri etiketi</param>
        /// <param name="textBox6">Uydu durumu textbox'ı</param>
        /// <param name="logHataFunc">Hata loglama fonksiyonu</param>
        public UyduGorsellestime(
            OpenTK.GLControl glControl,
            Label label3, Label label4, Label label5,
            TextBox textBox6,
            Action<string, bool> logHataFunc)
        {
            this.glControl = glControl;
            this.label3 = label3;
            this.label4 = label4;
            this.label5 = label5;
            this.textBox6 = textBox6;
            this.logHataFunc = logHataFunc;

            // Timer'ı başlat
            timerXYZ = new System.Windows.Forms.Timer();
            timerXYZ.Interval = 100; // 100 ms
            timerXYZ.Tick += TimerXYZ_Tick;

            // OpenGL kontrol olaylarını tanımla
            this.glControl.Load += OnGLControlLoad;
            this.glControl.Paint += OnGLControlPaint;
        }

        /// <summary>
        /// 3D model animasyonu için x, y, z değerlerini ayarlar
        /// </summary>
        /// <param name="pitch">X ekseni açısı (decimal değer)</param>
        /// <param name="roll">Y ekseni açısı (decimal değer)</param>
        /// <param name="yaw">Z ekseni açısı (decimal değer)</param>
        public void SetGyroValues(double pitch, double roll, double yaw)
        {
            this.x = (int)Math.Round(pitch);
            this.y = (int)Math.Round(roll);
            this.z = (int)Math.Round(yaw);
            
            // Değerleri etiketlere yaz (decimal formatında)
            if (label3 != null) label3.Text = pitch.ToString("F1");
            if (label4 != null) label4.Text = roll.ToString("F1");
            if (label5 != null) label5.Text = yaw.ToString("F1");
            
            // 3D görüntüyü yenile
            glControl.Invalidate();
        }

        /// <summary>
        /// OpenGL kontrol yüklendiğinde çalışan metod
        /// </summary>
        private void OnGLControlLoad(object sender, EventArgs e)
        {
            // 🎨 Modern uzay temalı gradyan arka plan
            GL.ClearColor(0.05f, 0.1f, 0.2f, 1.0f); // Derin uzay mavisi
            GL.Enable(EnableCap.DepthTest); // Derinlik testini etkinleştir
            GL.Enable(EnableCap.Blend); // Blending etkinleştir
            GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
        }

        /// <summary>
        /// OpenGL kontrol çizildiğinde çalışan metod (3D model çizimi)
        /// </summary>
        private void OnGLControlPaint(object sender, PaintEventArgs e)
        {
            float step = 1.0f; // Adım değeri
            float topla = step; // Adım artış değeri
            float radius = 5.0f; // Yarıçap
            float dikey1 = radius, dikey2 = -radius; // Dikey koordinatlar
            
            // 🌌 Uzay atmosferi arka planı çiz
            CizUzayArkaPlan();
            
            GL.Clear(ClearBufferMask.DepthBufferBit); // Sadece derinlik tamponunu temizle

            // Perspektif ve bakış matrisleri oluştur - optimum görünüm
            Matrix4 perspective = Matrix4.CreatePerspectiveFieldOfView(1.15f, 4 / 3, 1, 10000);
            // Optimum kamera mesafesi (z değeri 50 - paraşüt görünür ama çok uzak değil)
            Matrix4 lookat = Matrix4.LookAt(50, 0, 0, 0, 0, 0, 0, 1, 0);
            GL.MatrixMode(MatrixMode.Projection); // Projeksiyon matris modunu ayarla
            GL.LoadIdentity(); // Birim matris yükle
            GL.LoadMatrix(ref perspective); // Perspektif matrisi yükle
            GL.MatrixMode(MatrixMode.Modelview); // Model görünüm matris modunu ayarla
            GL.LoadIdentity(); // Birim matris yükle
            GL.LoadMatrix(ref lookat); // Bakış matrisini yükle
            GL.Viewport(0, 0, glControl.Width, glControl.Height); // Görüntü alanını ayarla
            GL.Enable(EnableCap.DepthTest); // Derinlik testini etkinleştir
            GL.DepthFunc(DepthFunction.Less); // Derinlik test fonksiyonunu ayarla

            // Model dönüşlerini uygula (gyro verilerine göre)
            GL.Rotate(x, 1.0, 0.0, 0.0); // X ekseni etrafında döndür (pitch)
            GL.Rotate(z, 0.0, 1.0, 0.0); // Z ekseni etrafında döndür (yaw)
            GL.Rotate(y, 0.0, 0.0, 1.0); // Y ekseni etrafında döndür (roll)

            // Model parçalarını çiz
            // Modern uydu silindir gövdesi
            ModernSilindir(radius, 8, -8);

            // Modern paraşüt çizimi
            ModernParasut(18.0f, 15.0f, 16);

            // Uydu durum göstergesini çiz
            CizUyduDurumPanel();

            // 🎯 Modern koordinat eksenleri çiz
            CizModernKoordinatEksenleri();

            // Çizimi göster
            glControl.SwapBuffers();
        }

        /// <summary>
        /// Uydu durum paneli (3D model üzerinde gösterge)
        /// </summary>
        private void CizUyduDurumPanel()
        {
            // Uydu statüsüne göre renk belirle
            Color durumRengi;

            // Uydu statüsüne göre renk seç
            if (textBox6 != null)
            {
                switch (textBox6.Text.Trim().ToUpper())
                {
                    case "HAZIR":
                        durumRengi = Color.FromArgb(255, 150, 0); // Turuncu
                        break;
                    case "YÜKSELME":
                        durumRengi = Color.FromArgb(0, 180, 0); // Yeşil
                        break;
                    case "M.U. İNİŞ":
                        durumRengi = Color.FromArgb(0, 100, 255); // Mavi
                        break;
                    case "KURTARMA":
                        durumRengi = Color.FromArgb(100, 0, 255); // Mor
                        break;
                    case "AYRILMA":
                        durumRengi = Color.FromArgb(255, 0, 0); // Kırmızı
                        break;
                    case "G.Y. İNİŞ":
                        durumRengi = Color.FromArgb(0, 180, 180); // Turkuaz
                        break;
                    default:
                        durumRengi = Color.FromArgb(80, 80, 80); // Gri
                        break;
                }
            }
            else
            {
                durumRengi = Color.FromArgb(80, 80, 80); // Gri (varsayılan)
            }

            // Silindir üzerinde durum paneli çiz (ön tarafta)
            GL.Begin(PrimitiveType.Quads);
            GL.Color3(durumRengi);

            float panelGenislik = 3.0f;
            float panelYukseklik = 2.0f;
            float panelDerinlik = 5.1f; // Silindirden biraz dışarı çıksın

            // Panel yüzeyi (ön - z+)
            GL.Vertex3(-panelGenislik / 2, 0 + panelYukseklik / 2, panelDerinlik);
            GL.Vertex3(panelGenislik / 2, 0 + panelYukseklik / 2, panelDerinlik);
            GL.Vertex3(panelGenislik / 2, 0 - panelYukseklik / 2, panelDerinlik);
            GL.Vertex3(-panelGenislik / 2, 0 - panelYukseklik / 2, panelDerinlik);

            // Panel kenarı için çerçeve (daha koyu renkli)
            GL.Color3(Color.FromArgb(
                (int)(durumRengi.R * 0.7f),
                (int)(durumRengi.G * 0.7f),
                (int)(durumRengi.B * 0.7f)));

            float kenarKalinlik = 0.1f;

            // Üst kenar
            GL.Vertex3(-panelGenislik / 2 - kenarKalinlik, 0 + panelYukseklik / 2 + kenarKalinlik, panelDerinlik);
            GL.Vertex3(panelGenislik / 2 + kenarKalinlik, 0 + panelYukseklik / 2 + kenarKalinlik, panelDerinlik);
            GL.Vertex3(panelGenislik / 2, 0 + panelYukseklik / 2, panelDerinlik);
            GL.Vertex3(-panelGenislik / 2, 0 + panelYukseklik / 2, panelDerinlik);

            // Alt kenar
            GL.Vertex3(-panelGenislik / 2, 0 - panelYukseklik / 2, panelDerinlik);
            GL.Vertex3(panelGenislik / 2, 0 - panelYukseklik / 2, panelDerinlik);
            GL.Vertex3(panelGenislik / 2 + kenarKalinlik, 0 - panelYukseklik / 2 - kenarKalinlik, panelDerinlik);
            GL.Vertex3(-panelGenislik / 2 - kenarKalinlik, 0 - panelYukseklik / 2 - kenarKalinlik, panelDerinlik);

            // Sol kenar
            GL.Vertex3(-panelGenislik / 2 - kenarKalinlik, 0 + panelYukseklik / 2 + kenarKalinlik, panelDerinlik);
            GL.Vertex3(-panelGenislik / 2, 0 + panelYukseklik / 2, panelDerinlik);
            GL.Vertex3(-panelGenislik / 2, 0 - panelYukseklik / 2, panelDerinlik);
            GL.Vertex3(-panelGenislik / 2 - kenarKalinlik, 0 - panelYukseklik / 2 - kenarKalinlik, panelDerinlik);

            // Sağ kenar
            GL.Vertex3(panelGenislik / 2, 0 + panelYukseklik / 2, panelDerinlik);
            GL.Vertex3(panelGenislik / 2 + kenarKalinlik, 0 + panelYukseklik / 2 + kenarKalinlik, panelDerinlik);
            GL.Vertex3(panelGenislik / 2 + kenarKalinlik, 0 - panelYukseklik / 2 - kenarKalinlik, panelDerinlik);
            GL.Vertex3(panelGenislik / 2, 0 - panelYukseklik / 2, panelDerinlik);

            GL.End();

            // Durum ışıkları (LED göstergeler)
            GL.Begin(PrimitiveType.Quads);

            // Durum gösterge ışığı (LED) - sağ alt köşede
            GL.Color3(Color.FromArgb(0, 255, 0)); // Aktif yeşil LED
            float ledBoyut = 0.3f;
            GL.Vertex3(panelGenislik / 2 - ledBoyut - 0.2f, -panelYukseklik / 2 + ledBoyut + 0.2f, panelDerinlik + 0.01f);
            GL.Vertex3(panelGenislik / 2 - 0.2f, -panelYukseklik / 2 + ledBoyut + 0.2f, panelDerinlik + 0.01f);
            GL.Vertex3(panelGenislik / 2 - 0.2f, -panelYukseklik / 2 + 0.2f, panelDerinlik + 0.01f);
            GL.Vertex3(panelGenislik / 2 - ledBoyut - 0.2f, -panelYukseklik / 2 + 0.2f, panelDerinlik + 0.01f);

            GL.End();
        }

        /// <summary>
        /// Modern paraşüt çizim metodu
        /// </summary>
        /// <param name="yukseklik">Paraşüt yüksekliği</param>
        /// <param name="yaricap">Paraşüt yarıçapı</param>
        /// <param name="ipSayisi">İp sayısı</param>
        private void ModernParasut(float yukseklik, float yaricap, int ipSayisi)
        {
            float step = 3.0f; // Daha pürüzsüz görünüm için daha küçük adım
            float theta;
            float x, y, z;

            // Paraşüt rengi (modern turuncu)
            Color parasutRengi = Color.FromArgb(255, 120, 0); // Ana turuncu
            Color parasutAcikRengi = Color.FromArgb(255, 150, 30); // Açık turuncu
            Color parasutKoyuRengi = Color.FromArgb(220, 90, 0);  // Koyu turuncu

            // Paraşüt dilimlerini çiz
            GL.Begin(PrimitiveType.Triangles);

            // Daha fazla dilimli paraşüt
            int dilimSayisi = 16;
            float dilimGenisligi = 360.0f / dilimSayisi;

            for (int dilim = 0; dilim < dilimSayisi; dilim++)
            {
                // Her dilim için başlangıç ve bitiş açıları
                float baslangicAci = dilim * dilimGenisligi;
                float bitisAci = (dilim + 1) * dilimGenisligi;

                // Dilim rengi (alternatif dilimler için farklı renkler - modern görünüm için daha ince geçişler)
                if (dilim % 3 == 0)
                    GL.Color3(parasutRengi); // Ana turuncu
                else if (dilim % 3 == 1)
                    GL.Color3(parasutAcikRengi); // Açık turuncu
                else
                    GL.Color3(parasutKoyuRengi); // Koyu turuncu

                // Dilim tepe noktası (daha yukarıda - daha zarif paraşüt şekli)
                float merkez_x = 0;
                float merkez_y = yukseklik + yaricap * 0.9f; // Tepe noktası yüksekliği (biraz daha yukarıda)
                float merkez_z = 0;

                // Her dilim için ince bir geçiş kullan, daha pürüzsüz
                for (theta = baslangicAci; theta < bitisAci; theta += step)
                {
                    // Çağdaş eğimli şekil - paraşütün simetrik eğriliği için sinüs dalgası
                    float cevre_x1 = yaricap * (float)Math.Cos(theta * Math.PI / 180);
                    // Daha düzgün eğimli kenar için sinüs dalgası kullan
                    float cevre_y1 = yukseklik + yaricap * 0.2f * (float)Math.Sin((theta - baslangicAci) * Math.PI / dilimGenisligi);
                    float cevre_z1 = yaricap * (float)Math.Sin(theta * Math.PI / 180);

                    float cevre_x2 = yaricap * (float)Math.Cos((theta + step) * Math.PI / 180);
                    float cevre_y2 = yukseklik + yaricap * 0.2f * (float)Math.Sin(((theta + step) - baslangicAci) * Math.PI / dilimGenisligi);
                    float cevre_z2 = yaricap * (float)Math.Sin((theta + step) * Math.PI / 180);

                    // Dilimi oluşturan üçgen
                    GL.Vertex3(merkez_x, merkez_y, merkez_z); // Tepe noktası
                    GL.Vertex3(cevre_x1, cevre_y1, cevre_z1); // Kenar 1
                    GL.Vertex3(cevre_x2, cevre_y2, cevre_z2); // Kenar 2
                }
            }
            GL.End();

            // Paraşüt alt kenarı - daha ince ve zarif
            GL.Begin(PrimitiveType.Quads);

            // Kenar renkleri
            for (int dilim = 0; dilim < dilimSayisi; dilim++)
            {
                float baslangicAci = dilim * dilimGenisligi;
                float bitisAci = (dilim + 1) * dilimGenisligi;

                if (dilim % 3 == 0)
                    GL.Color3(Color.FromArgb(200, 100, 0)); // Koyu turuncu
                else if (dilim % 3 == 1)
                    GL.Color3(Color.FromArgb(220, 110, 10)); // Orta turuncu
                else
                    GL.Color3(Color.FromArgb(180, 90, 0)); // Daha koyu turuncu

                for (theta = baslangicAci; theta < bitisAci; theta += step)
                {
                    // İnce kenar - daha zarif görünüm
                    float kenar_x1 = yaricap * (float)Math.Cos(theta * Math.PI / 180);
                    float kenar_y1 = yukseklik + yaricap * 0.2f * (float)Math.Sin((theta - baslangicAci) * Math.PI / dilimGenisligi);
                    float kenar_z1 = yaricap * (float)Math.Sin(theta * Math.PI / 180);

                    float kenar_x2 = yaricap * (float)Math.Cos((theta + step) * Math.PI / 180);
                    float kenar_y2 = yukseklik + yaricap * 0.2f * (float)Math.Sin(((theta + step) - baslangicAci) * Math.PI / dilimGenisligi);
                    float kenar_z2 = yaricap * (float)Math.Sin((theta + step) * Math.PI / 180);

                    // Daha ince kenar (% 1.5 daha küçük)
                    float ic_kenar_x1 = 0.985f * yaricap * (float)Math.Cos(theta * Math.PI / 180);
                    float ic_kenar_y1 = kenar_y1 - 0.15f; // Biraz aşağıda
                    float ic_kenar_z1 = 0.985f * yaricap * (float)Math.Sin(theta * Math.PI / 180);

                    float ic_kenar_x2 = 0.985f * yaricap * (float)Math.Cos((theta + step) * Math.PI / 180);
                    float ic_kenar_y2 = kenar_y2 - 0.15f; // Biraz aşağıda
                    float ic_kenar_z2 = 0.985f * yaricap * (float)Math.Sin((theta + step) * Math.PI / 180);

                    // Kenar dörtgeni
                    GL.Vertex3(kenar_x1, kenar_y1, kenar_z1); // Dış kenar 1
                    GL.Vertex3(kenar_x2, kenar_y2, kenar_z2); // Dış kenar 2
                    GL.Vertex3(ic_kenar_x2, ic_kenar_y2, ic_kenar_z2); // İç kenar 2
                    GL.Vertex3(ic_kenar_x1, ic_kenar_y1, ic_kenar_z1); // İç kenar 1
                }
            }
            GL.End();

            // Modern ip sistemi - ince ve çift ipler
            GL.Begin(PrimitiveType.Lines);
            GL.Color3(Color.WhiteSmoke); // Modern beyaz

            float parachuteBottom = yukseklik;

            // Ana gruplar halinde ipler
            int ipGrupSayisi = 8;
            float grupAcisi = 360.0f / ipGrupSayisi;

            // Her grup içinde ipler
            for (int grup = 0; grup < ipGrupSayisi; grup++)
            {
                float grupMerkezAci = grup * grupAcisi;

                // Her grupta 2 ip
                for (int ipIndex = 0; ipIndex < 2; ipIndex++)
                {
                    float ipAciOfset = (ipIndex - 0.5f) * 5.0f; // -2.5 ve 2.5 derece ofset
                    float ipAci = grupMerkezAci + ipAciOfset;

                    // Paraşüt kenarından ipin başlangıç noktası
                    x = yaricap * (float)Math.Cos(ipAci * Math.PI / 180);
                    y = parachuteBottom;
                    z = yaricap * (float)Math.Sin(ipAci * Math.PI / 180);
                    GL.Vertex3(x, y, z);

                    // İpin uydunun üst kısmına bağlantı noktası - hafif dağıtılmış
                    GL.Vertex3(ipIndex * 0.3f, 8, ipIndex * 0.3f);
                }
            }
            GL.End();

            // İplerin merkez bağlantı noktası - daha zarif, yuvarlak
            GL.Begin(PrimitiveType.Quads);
            GL.Color3(Color.DarkGray);

            float connectorSize = 0.8f; // Daha küçük, zarif bağlantı
            // Üst yüz
            GL.Vertex3(-connectorSize, 8, -connectorSize);
            GL.Vertex3(connectorSize, 8, -connectorSize);
            GL.Vertex3(connectorSize, 8, connectorSize);
            GL.Vertex3(-connectorSize, 8, connectorSize);

            // Alt yüz
            GL.Vertex3(-connectorSize, 8 - connectorSize / 2, -connectorSize);
            GL.Vertex3(connectorSize, 8 - connectorSize / 2, -connectorSize);
            GL.Vertex3(connectorSize, 8 - connectorSize / 2, connectorSize);
            GL.Vertex3(-connectorSize, 8 - connectorSize / 2, connectorSize);

            // Yan yüzler
            GL.Vertex3(-connectorSize, 8, -connectorSize);
            GL.Vertex3(-connectorSize, 8 - connectorSize / 2, -connectorSize);
            GL.Vertex3(connectorSize, 8 - connectorSize / 2, -connectorSize);
            GL.Vertex3(connectorSize, 8, -connectorSize);

            GL.Vertex3(connectorSize, 8, -connectorSize);
            GL.Vertex3(connectorSize, 8 - connectorSize / 2, -connectorSize);
            GL.Vertex3(connectorSize, 8 - connectorSize / 2, connectorSize);
            GL.Vertex3(connectorSize, 8, connectorSize);

            GL.Vertex3(connectorSize, 8, connectorSize);
            GL.Vertex3(connectorSize, 8 - connectorSize / 2, connectorSize);
            GL.Vertex3(-connectorSize, 8 - connectorSize / 2, connectorSize);
            GL.Vertex3(-connectorSize, 8, connectorSize);

            GL.Vertex3(-connectorSize, 8, connectorSize);
            GL.Vertex3(-connectorSize, 8 - connectorSize / 2, connectorSize);
            GL.Vertex3(-connectorSize, 8 - connectorSize / 2, -connectorSize);
            GL.Vertex3(-connectorSize, 8, -connectorSize);

            GL.End();

            // Dekoratif detaylar - bağlantı noktası üzerinde
            GL.Begin(PrimitiveType.Triangles);
            GL.Color3(Color.LightGray);

            float detayBoyutu = 0.4f;
            // Üstte küçük üçgen detaylar
            for (int i = 0; i < 4; i++)
            {
                float detayAci = i * 90.0f;
                float dx1 = (float)Math.Cos(detayAci * Math.PI / 180.0f) * detayBoyutu;
                float dz1 = (float)Math.Sin(detayAci * Math.PI / 180.0f) * detayBoyutu;

                float dx2 = (float)Math.Cos((detayAci + 45.0f) * Math.PI / 180.0f) * detayBoyutu;
                float dz2 = (float)Math.Sin((detayAci + 45.0f) * Math.PI / 180.0f) * detayBoyutu;

                // Merkez
                GL.Vertex3(0, 8 + 0.1f, 0);
                // Kenar 1
                GL.Vertex3(dx1, 8, dz1);
                // Kenar 2
                GL.Vertex3(dx2, 8, dz2);
            }
            GL.End();
        }

        /// <summary>
        /// Modern silindir çizim metodu (görev yükü)
        /// </summary>
        /// <param name="radius">Silindir yarıçapı</param>
        /// <param name="dikey1">Üst nokta</param>
        /// <param name="dikey2">Alt nokta</param>
        private void ModernSilindir(float radius, float dikey1, float dikey2)
        {
            // Daha yüksek çözünürlüklü silindir için adım boyutunu küçült
            float step = 0.72f; // 360/500 yaklaşık değer, 500 köşeli bir silindir için

            // Silindir gövdesi
            GL.Begin(PrimitiveType.Quads);
            for (float angle = 0; angle <= 360; angle += step)
            {
                // 🛰️ Modern uydu renkleri - metalik görünüm
                if (angle < 180)
                    GL.Color3(Color.FromArgb(45, 85, 170)); // Koyu uzay mavisi
                else
                    GL.Color3(Color.FromArgb(220, 225, 235)); // Metalik beyaz

                // Silindir yüzey noktalarını hesaplama ve çizme
                float ciz1_x = (float)(radius * Math.Cos(angle * Math.PI / 180F));
                float ciz1_y = (float)(radius * Math.Sin(angle * Math.PI / 180F));
                GL.Vertex3(ciz1_x, dikey1, ciz1_y); // İlk nokta

                float ciz2_x = (float)(radius * Math.Cos((angle + step) * Math.PI / 180F));
                float ciz2_y = (float)(radius * Math.Sin((angle + step) * Math.PI / 180F));
                GL.Vertex3(ciz2_x, dikey1, ciz2_y); // İkinci nokta

                GL.Vertex3(ciz2_x, dikey2, ciz2_y); // Üçüncü nokta (alt kısım)
                GL.Vertex3(ciz1_x, dikey2, ciz1_y); // Dördüncü nokta (alt kısım)
            }
            GL.End();

            // Üst yüzey - modern metalik görünüm
            GL.Begin(PrimitiveType.TriangleFan);
            GL.Color3(Color.FromArgb(120, 140, 190)); // Merkez metalik mavi
            GL.Vertex3(0, dikey1, 0); // Merkez nokta

            for (float angle = 0; angle <= 360; angle += step)
            {
                // 🌟 Gradyan metalik yüzey
                if (angle < 180)
                    GL.Color3(Color.FromArgb(85, 115, 200)); // Metalik mavi 
                else
                    GL.Color3(Color.FromArgb(200, 210, 230)); // Metalik gümüş

                float ciz_x = (float)(radius * Math.Cos(angle * Math.PI / 180F));
                float ciz_y = (float)(radius * Math.Sin(angle * Math.PI / 180F));
                GL.Vertex3(ciz_x, dikey1, ciz_y);
            }
            GL.End();

            // Alt yüzey
            GL.Begin(PrimitiveType.TriangleFan);
            GL.Color3(Color.FromArgb(100, 130, 230)); // Açık mavi
            GL.Vertex3(0, dikey2, 0); // Merkez nokta

            for (float angle = 0; angle <= 360; angle += step)
            {
                // Yarım daire mavi, yarım daire beyaz
                if (angle < 180)
                    GL.Color3(Color.FromArgb(70, 100, 220)); // Açık mavi
                else
                    GL.Color3(Color.FromArgb(230, 230, 235)); // Açık beyaz

                float ciz_x = (float)(radius * Math.Cos(angle * Math.PI / 180F));
                float ciz_y = (float)(radius * Math.Sin(angle * Math.PI / 180F));
                GL.Vertex3(ciz_x, dikey2, ciz_y);
            }
            GL.End();

            // Modern detay çizgileri - yatay şeritler
            GL.Begin(PrimitiveType.Lines);
            GL.Color3(Color.FromArgb(30, 30, 30)); // Koyu gri
            GL.LineWidth(1.0f);

            // Yatay şeritler - 4 adet
            int seritSayisi = 4;
            float yukseklikFarki = (dikey1 - dikey2) / (seritSayisi + 1);

            for (int i = 1; i <= seritSayisi; i++)
            {
                float seritY = dikey1 - i * yukseklikFarki;

                // Tam çevre çizgisi
                for (float angle = 0; angle < 360; angle += 10)
                {
                    float baslangic_x = (float)(radius * 1.01f * Math.Cos(angle * Math.PI / 180F));
                    float baslangic_z = (float)(radius * 1.01f * Math.Sin(angle * Math.PI / 180F));

                    float bitis_x = (float)(radius * 1.01f * Math.Cos((angle + 10) * Math.PI / 180F));
                    float bitis_z = (float)(radius * 1.01f * Math.Sin((angle + 10) * Math.PI / 180F));

                    GL.Vertex3(baslangic_x, seritY, baslangic_z);
                    GL.Vertex3(bitis_x, seritY, bitis_z);
                }
            }

            GL.End();
        }

        /// <summary>
        /// 3D model animasyonu için zamanlayıcı
        /// </summary>
        private void TimerXYZ_Tick(object sender, EventArgs e)
        {
            // X ekseni dönüşü (pitch)
            if (cx == true)
            {
                if (x < 360)
                    x += 5; // 5 derece artır
                else
                    x = 0; // 360 dereceyi geçince sıfırla
                
                if (label3 != null)
                    label3.Text = x.ToString(); // X değerini etikette göster
            }
            
            // Y ekseni dönüşü (roll)
            if (cy == true)
            {
                if (y < 360)
                    y += 5; // 5 derece artır
                else
                    y = 0; // 360 dereceyi geçince sıfırla
                
                if (label4 != null)
                    label4.Text = y.ToString(); // Y değerini etikette göster
            }
            
            // Z ekseni dönüşü (yaw)
            if (cz == true)
            {
                if (z < 360)
                    z += 5; // 5 derece artır
                else
                    z = 0; // 360 dereceyi geçince sıfırla
                
                if (label5 != null)
                    label5.Text = z.ToString(); // Z değerini etikette göster
            }
            
            // 3D görüntüyü yenile
            glControl.Invalidate();
        }

        /// <summary>
        /// X ekseni dönüşünü başlat/durdur
        /// </summary>
        public void ToggleX()
        {
            cx = !cx;
            timerXYZ.Start();
        }

        /// <summary>
        /// Y ekseni dönüşünü başlat/durdur
        /// </summary>
        public void ToggleY()
        {
            cy = !cy;
            timerXYZ.Start();
        }

        /// <summary>
        /// Z ekseni dönüşünü başlat/durdur
        /// </summary>
        public void ToggleZ()
        {
            cz = !cz;
            timerXYZ.Start();
        }

        /// <summary>
        /// OpenGL kaynaklarını temizle ve timer'ı durdur
        /// </summary>
        public void Dispose()
        {
            if (timerXYZ != null)
            {
                timerXYZ.Stop();
                timerXYZ.Dispose();
            }
        }

        /// <summary>
        /// 🌌 Modern uzay atmosferi arka planı çizer
        /// </summary>
        private void CizUzayArkaPlan()
        {
            GL.Clear(ClearBufferMask.ColorBufferBit);
            
            // Arka plan için projeksiyon ayarları
            GL.MatrixMode(MatrixMode.Projection);
            GL.PushMatrix();
            GL.LoadIdentity();
            GL.Ortho(0, glControl.Width, 0, glControl.Height, -1, 1);
            
            GL.MatrixMode(MatrixMode.Modelview);
            GL.PushMatrix();
            GL.LoadIdentity();
            
            // Derinlik testini geçici olarak kapat
            GL.Disable(EnableCap.DepthTest);
            
            // 🌌 Uzay gradyanı (üstten alta)
            GL.Begin(PrimitiveType.Quads);
            
            // Üst kısım - derin uzay mavisi
            GL.Color3(0.02f, 0.05f, 0.15f);
            GL.Vertex2(0, glControl.Height);
            GL.Vertex2(glControl.Width, glControl.Height);
            
            // Alt kısım - atmosfer mavisi
            GL.Color3(0.08f, 0.15f, 0.25f);
            GL.Vertex2(glControl.Width, 0);
            GL.Vertex2(0, 0);
            
            GL.End();
            
            // ✨ Yıldızlar ekle
            CizYildizlar();
            
            // Derinlik testini tekrar aç
            GL.Enable(EnableCap.DepthTest);
            
            // Matrix'leri geri yükle
            GL.PopMatrix();
            GL.MatrixMode(MatrixMode.Projection);
            GL.PopMatrix();
            GL.MatrixMode(MatrixMode.Modelview);
        }

        /// <summary>
        /// ✨ Dinamik yıldızlar çizer
        /// </summary>
        private void CizYildizlar()
        {
            Random rand = new Random(42); // Sabit seed ile tutarlı yıldızlar
            
            GL.Begin(PrimitiveType.Points);
            GL.PointSize(2.0f);
            
            // 100 yıldız çiz
            for (int i = 0; i < 100; i++)
            {
                float x = rand.Next(0, glControl.Width);
                float y = rand.Next(0, glControl.Height);
                
                // Yıldız parlaklığı (alpha değeri)
                float brightness = 0.3f + (float)(rand.NextDouble() * 0.7f);
                GL.Color4(1.0f, 1.0f, 0.9f, brightness);
                
                GL.Vertex2(x, y);
            }
            
            GL.End();
        }

        /// <summary>
        /// 🎯 Modern koordinat eksenleri çizer
        /// </summary>
        private void CizModernKoordinatEksenleri()
        {
            GL.LineWidth(1.5f); // Daha ince çizgiler
            GL.Begin(PrimitiveType.Lines);

            // X ekseni (kırmızı) - Pitch
            GL.Color4(1.0f, 0.2f, 0.2f, 0.8f);
            GL.Vertex3(-20.0, 0.0, 0.0);
            GL.Vertex3(20.0, 0.0, 0.0);

            // Y ekseni (yeşil) - Roll  
            GL.Color4(0.2f, 1.0f, 0.2f, 0.8f);
            GL.Vertex3(0.0, 20.0, 0.0);
            GL.Vertex3(0.0, -20.0, 0.0);

            // Z ekseni (mavi) - Yaw
            GL.Color4(0.2f, 0.5f, 1.0f, 0.8f);
            GL.Vertex3(0.0, 0.0, 20.0);
            GL.Vertex3(0.0, 0.0, -20.0);

            GL.End();
            
            // Eksen uçlarına küçük oklar çiz
            CizEksenOklari();
            
            GL.LineWidth(1.0f); // Varsayılan kalınlığa dön
        }

        /// <summary>
        /// ➤ Koordinat ekseni uçlarına oklar çizer
        /// </summary>
        private void CizEksenOklari()
        {
            float okUzunlugu = 3.0f;
            
            GL.Begin(PrimitiveType.Triangles);
            
            // X ekseni oku (kırmızı)
            GL.Color4(1.0f, 0.2f, 0.2f, 0.9f);
            GL.Vertex3(20.0f, 0.0f, 0.0f);
            GL.Vertex3(20.0f - okUzunlugu, 1.0f, 0.0f);
            GL.Vertex3(20.0f - okUzunlugu, -1.0f, 0.0f);
            
            // Y ekseni oku (yeşil)
            GL.Color4(0.2f, 1.0f, 0.2f, 0.9f);
            GL.Vertex3(0.0f, 20.0f, 0.0f);
            GL.Vertex3(1.0f, 20.0f - okUzunlugu, 0.0f);
            GL.Vertex3(-1.0f, 20.0f - okUzunlugu, 0.0f);
            
            // Z ekseni oku (mavi)
            GL.Color4(0.2f, 0.5f, 1.0f, 0.9f);
            GL.Vertex3(0.0f, 0.0f, 20.0f);
            GL.Vertex3(0.0f, 1.0f, 20.0f - okUzunlugu);
            GL.Vertex3(0.0f, -1.0f, 20.0f - okUzunlugu);
            
            GL.End();
        }
    }
} 