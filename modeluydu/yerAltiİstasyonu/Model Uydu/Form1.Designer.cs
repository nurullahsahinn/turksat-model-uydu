namespace ModelUydu
{
    partial class Form1
    {
        /// <summary>
        ///Gerekli tasarımcı değişkeni.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///Kullanılan tüm kaynakları temizleyin.
        /// </summary>
        ///<param name="disposing">yönetilen kaynaklar dispose edilmeliyse doğru; aksi halde yanlış.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer üretilen kod

        /// <summary>
        /// Tasarımcı desteği için gerekli metot - bu metodun 
        ///içeriğini kod düzenleyici ile değiştirmeyin.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.Windows.Forms.DataVisualization.Charting.ChartArea chartArea1 = new System.Windows.Forms.DataVisualization.Charting.ChartArea();
            System.Windows.Forms.DataVisualization.Charting.Series series1 = new System.Windows.Forms.DataVisualization.Charting.Series();
            System.Windows.Forms.DataVisualization.Charting.Title title1 = new System.Windows.Forms.DataVisualization.Charting.Title();
            System.Windows.Forms.DataVisualization.Charting.ChartArea chartArea2 = new System.Windows.Forms.DataVisualization.Charting.ChartArea();
            System.Windows.Forms.DataVisualization.Charting.Series series2 = new System.Windows.Forms.DataVisualization.Charting.Series();
            System.Windows.Forms.DataVisualization.Charting.Title title2 = new System.Windows.Forms.DataVisualization.Charting.Title();
            System.Windows.Forms.DataVisualization.Charting.ChartArea chartArea3 = new System.Windows.Forms.DataVisualization.Charting.ChartArea();
            System.Windows.Forms.DataVisualization.Charting.Series series3 = new System.Windows.Forms.DataVisualization.Charting.Series();
            System.Windows.Forms.DataVisualization.Charting.Title title3 = new System.Windows.Forms.DataVisualization.Charting.Title();
            System.Windows.Forms.DataVisualization.Charting.ChartArea chartArea4 = new System.Windows.Forms.DataVisualization.Charting.ChartArea();
            System.Windows.Forms.DataVisualization.Charting.Series series4 = new System.Windows.Forms.DataVisualization.Charting.Series();
            System.Windows.Forms.DataVisualization.Charting.Title title4 = new System.Windows.Forms.DataVisualization.Charting.Title();
            System.Windows.Forms.DataVisualization.Charting.ChartArea chartArea5 = new System.Windows.Forms.DataVisualization.Charting.ChartArea();
            System.Windows.Forms.DataVisualization.Charting.Series series5 = new System.Windows.Forms.DataVisualization.Charting.Series();
            System.Windows.Forms.DataVisualization.Charting.Title title5 = new System.Windows.Forms.DataVisualization.Charting.Title();
            System.Windows.Forms.DataVisualization.Charting.ChartArea chartArea6 = new System.Windows.Forms.DataVisualization.Charting.ChartArea();
            System.Windows.Forms.DataVisualization.Charting.Series series6 = new System.Windows.Forms.DataVisualization.Charting.Series();
            System.Windows.Forms.DataVisualization.Charting.Title title6 = new System.Windows.Forms.DataVisualization.Charting.Title();
            System.Windows.Forms.DataVisualization.Charting.ChartArea chartArea7 = new System.Windows.Forms.DataVisualization.Charting.ChartArea();
            System.Windows.Forms.DataVisualization.Charting.Series series7 = new System.Windows.Forms.DataVisualization.Charting.Series();
            System.Windows.Forms.DataVisualization.Charting.Title title7 = new System.Windows.Forms.DataVisualization.Charting.Title();
            System.Windows.Forms.DataVisualization.Charting.ChartArea chartArea8 = new System.Windows.Forms.DataVisualization.Charting.ChartArea();
            System.Windows.Forms.DataVisualization.Charting.Series series8 = new System.Windows.Forms.DataVisualization.Charting.Series();
            System.Windows.Forms.DataVisualization.Charting.Title title8 = new System.Windows.Forms.DataVisualization.Charting.Title();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.textBox2 = new System.Windows.Forms.TextBox();
            this.textBox3 = new System.Windows.Forms.TextBox();
            this.OkumaNesnesi = new System.IO.Ports.SerialPort(this.components);
            this.glControl1 = new OpenTK.GLControl();
            this.zamanlayici = new System.Windows.Forms.Timer(this.components);
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.button1 = new System.Windows.Forms.Button();
            this.button2 = new System.Windows.Forms.Button();
            this.button3 = new System.Windows.Forms.Button();
            this.gMapControl1 = new GMap.NET.WindowsForms.GMapControl();
            this.Bul = new System.Windows.Forms.Button();
            this.label6 = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.Yakınlaştır = new System.Windows.Forms.Button();
            this.Uzaklaştır = new System.Windows.Forms.Button();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.label8 = new System.Windows.Forms.Label();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.groupBoxMultiSpektral = new System.Windows.Forms.GroupBox();
            this.labelMultiSpektralDurum = new System.Windows.Forms.Label();
            this.labelMultiSpektralAciklama = new System.Windows.Forms.Label();
            this.labelMultiSpektralKomut = new System.Windows.Forms.Label();
            this.buttonMultiSpektralGonder = new System.Windows.Forms.Button();
            this.textBoxMultiSpektralKomut = new System.Windows.Forms.TextBox();
            this.serialPort1 = new System.IO.Ports.SerialPort(this.components);
            this.videoSourcePlayer = new AForge.Controls.VideoSourcePlayer();
            this.groupBoxARAS = new System.Windows.Forms.GroupBox();
            this.tableLayoutPanelAlarm = new System.Windows.Forms.TableLayoutPanel();
            this.panelAlarm1 = new System.Windows.Forms.Panel();
            this.panelAlarm2 = new System.Windows.Forms.Panel();
            this.panelAlarm3 = new System.Windows.Forms.Panel();
            this.panelAlarm4 = new System.Windows.Forms.Panel();
            this.panelAlarm5 = new System.Windows.Forms.Panel();
            this.panelAlarm6 = new System.Windows.Forms.Panel();
            this.label14 = new System.Windows.Forms.Label();
            this.label15 = new System.Windows.Forms.Label();
            this.label16 = new System.Windows.Forms.Label();
            this.label17 = new System.Windows.Forms.Label();
            this.label18 = new System.Windows.Forms.Label();
            this.label19 = new System.Windows.Forms.Label();
            this.checkBoxSesliUyari = new System.Windows.Forms.CheckBox();
            this.labelHata6 = new System.Windows.Forms.Label();
            this.labelHata5 = new System.Windows.Forms.Label();
            this.labelHata4 = new System.Windows.Forms.Label();
            this.labelHata3 = new System.Windows.Forms.Label();
            this.labelHata2 = new System.Windows.Forms.Label();
            this.labelHata1 = new System.Windows.Forms.Label();
            this.panelHata6 = new System.Windows.Forms.Panel();
            this.panelHata5 = new System.Windows.Forms.Panel();
            this.panelHata4 = new System.Windows.Forms.Panel();
            this.panelHata3 = new System.Windows.Forms.Panel();
            this.panelHata2 = new System.Windows.Forms.Panel();
            this.panelHata1 = new System.Windows.Forms.Panel();
            this.labelHataKoduBaslik = new System.Windows.Forms.Label();
            this.labelHataKodu = new System.Windows.Forms.Label();
            this.groupBoxKalibrasyon = new System.Windows.Forms.GroupBox();
            this.buttonGyroSifirla = new System.Windows.Forms.Button();
            this.buttonGPSReferans = new System.Windows.Forms.Button();
            this.buttonBasincKalibre = new System.Windows.Forms.Button();
            this.comboBoxSehir = new System.Windows.Forms.ComboBox();
            this.labelSehirSecimi = new System.Windows.Forms.Label();
            this.labelKalibrasyonDurum = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.chart3 = new System.Windows.Forms.DataVisualization.Charting.Chart();
            this.chart4 = new System.Windows.Forms.DataVisualization.Charting.Chart();
            this.chart5 = new System.Windows.Forms.DataVisualization.Charting.Chart();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.ExlAktar = new System.Windows.Forms.Button();
            this.labelTakimNumarasi = new System.Windows.Forms.Label();
            this.label36 = new System.Windows.Forms.Label();
            this.label35 = new System.Windows.Forms.Label();
            this.label34 = new System.Windows.Forms.Label();
            this.label33 = new System.Windows.Forms.Label();
            this.label32 = new System.Windows.Forms.Label();
            this.label26 = new System.Windows.Forms.Label();
            this.label20 = new System.Windows.Forms.Label();
            this.textBox22 = new System.Windows.Forms.TextBox();
            this.textBox21 = new System.Windows.Forms.TextBox();
            this.textBox20 = new System.Windows.Forms.TextBox();
            this.textBox19 = new System.Windows.Forms.TextBox();
            this.textBox18 = new System.Windows.Forms.TextBox();
            this.textBox17 = new System.Windows.Forms.TextBox();
            this.textBox16 = new System.Windows.Forms.TextBox();
            this.label31 = new System.Windows.Forms.Label();
            this.label30 = new System.Windows.Forms.Label();
            this.label29 = new System.Windows.Forms.Label();
            this.label28 = new System.Windows.Forms.Label();
            this.label27 = new System.Windows.Forms.Label();
            this.label25 = new System.Windows.Forms.Label();
            this.label24 = new System.Windows.Forms.Label();
            this.label23 = new System.Windows.Forms.Label();
            this.label22 = new System.Windows.Forms.Label();
            this.label21 = new System.Windows.Forms.Label();
            this.groupBoxIoT = new System.Windows.Forms.GroupBox();
            this.buttonIotGoruntule = new System.Windows.Forms.Button();
            this.labelIoTS1 = new System.Windows.Forms.Label();
            this.labelIoTS2 = new System.Windows.Forms.Label();
            this.labelIoTS1Deger = new System.Windows.Forms.Label();
            this.labelIoTS2Deger = new System.Windows.Forms.Label();
            this.chartIoT = new System.Windows.Forms.DataVisualization.Charting.Chart();
            this.textBox8 = new System.Windows.Forms.TextBox();
            this.label13 = new System.Windows.Forms.Label();
            this.label12 = new System.Windows.Forms.Label();
            this.textBox12 = new System.Windows.Forms.TextBox();
            this.label11 = new System.Windows.Forms.Label();
            this.manuelAyrilma = new System.Windows.Forms.Button();
            this.button4 = new System.Windows.Forms.Button();
            this.textBox7 = new System.Windows.Forms.TextBox();
            this.textBox6 = new System.Windows.Forms.TextBox();
            this.groupBoxUyduDurum = new System.Windows.Forms.GroupBox();
            this.labelUyduStatu = new System.Windows.Forms.Label();
            this.textBox15 = new System.Windows.Forms.TextBox();
            this.label10 = new System.Windows.Forms.Label();
            this.label9 = new System.Windows.Forms.Label();
            this.textBox14 = new System.Windows.Forms.TextBox();
            this.textBox13 = new System.Windows.Forms.TextBox();
            this.textBox11 = new System.Windows.Forms.TextBox();
            this.textBox10 = new System.Windows.Forms.TextBox();
            this.textBox9 = new System.Windows.Forms.TextBox();
            this.comboBox1 = new System.Windows.Forms.ComboBox();
            this.dataGridView1 = new System.Windows.Forms.DataGridView();
            this.Column17 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Column19 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Column20 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Column7 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Column2 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Column21 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Column3 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Column14 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Column22 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Hız = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Column1 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Column11 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Column12 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Column13 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Column24 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Pitch = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Column15 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Column16 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Column23 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Column25 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Column26 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Column10 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.baglan = new System.Windows.Forms.Button();
            this.texBaundRate = new System.Windows.Forms.TextBox();
            this.kes = new System.Windows.Forms.Button();
            this.chartYukseklik = new System.Windows.Forms.DataVisualization.Charting.Chart();
            this.chartSicaklik = new System.Windows.Forms.DataVisualization.Charting.Chart();
            this.chartBasinc = new System.Windows.Forms.DataVisualization.Charting.Chart();
            this.chartHiz = new System.Windows.Forms.DataVisualization.Charting.Chart();
            this.chartPilGerilimi = new System.Windows.Forms.DataVisualization.Charting.Chart();
            this.label1 = new System.Windows.Forms.Label();
            this.textBox5 = new System.Windows.Forms.TextBox();
            this.textBox4 = new System.Windows.Forms.TextBox();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.groupBoxMultiSpektral.SuspendLayout();
            this.groupBoxARAS.SuspendLayout();
            this.tableLayoutPanelAlarm.SuspendLayout();
            this.groupBoxKalibrasyon.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.chart3)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.chart4)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.chart5)).BeginInit();
            this.groupBox3.SuspendLayout();
            this.groupBoxIoT.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.chartIoT)).BeginInit();
            this.groupBoxUyduDurum.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.chartYukseklik)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.chartSicaklik)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.chartBasinc)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.chartHiz)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.chartPilGerilimi)).BeginInit();
            this.SuspendLayout();
            // 
            // textBox1
            // 
            this.textBox1.Location = new System.Drawing.Point(111, 308);
            this.textBox1.Margin = new System.Windows.Forms.Padding(4);
            this.textBox1.Name = "textBox1";
            this.textBox1.Size = new System.Drawing.Size(124, 22);
            this.textBox1.TabIndex = 6;
            // 
            // textBox2
            // 
            this.textBox2.Location = new System.Drawing.Point(244, 308);
            this.textBox2.Margin = new System.Windows.Forms.Padding(4);
            this.textBox2.Name = "textBox2";
            this.textBox2.Size = new System.Drawing.Size(117, 22);
            this.textBox2.TabIndex = 7;
            this.textBox2.TextChanged += new System.EventHandler(this.textBox2_TextChanged);
            // 
            // textBox3
            // 
            this.textBox3.Location = new System.Drawing.Point(157, 372);
            this.textBox3.Margin = new System.Windows.Forms.Padding(4);
            this.textBox3.Name = "textBox3";
            this.textBox3.Size = new System.Drawing.Size(157, 22);
            this.textBox3.TabIndex = 8;
            this.textBox3.TextChanged += new System.EventHandler(this.textBox3_TextChanged);
            // 
            // glControl1
            // 
            this.glControl1.BackColor = System.Drawing.Color.Black;
            this.glControl1.Location = new System.Drawing.Point(5, 24);
            this.glControl1.Margin = new System.Windows.Forms.Padding(5);
            this.glControl1.Name = "glControl1";
            this.glControl1.Size = new System.Drawing.Size(277, 242);
            this.glControl1.TabIndex = 14;
            this.glControl1.VSync = false;
            this.glControl1.Load += new System.EventHandler(this.glControl1_Load);
            this.glControl1.Paint += new System.Windows.Forms.PaintEventHandler(this.glControl1_Paint);
            // 
            // zamanlayici
            // 
            this.zamanlayici.Interval = 1000;
            this.zamanlayici.Tick += new System.EventHandler(this.Zamanlayici_Tick);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("Microsoft Sans Serif", 20.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(162)));
            this.label3.Location = new System.Drawing.Point(37, 282);
            this.label3.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(40, 39);
            this.label3.TabIndex = 15;
            this.label3.Text = "X";
            this.label3.Click += new System.EventHandler(this.label3_Click);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Font = new System.Drawing.Font("Microsoft Sans Serif", 20.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(162)));
            this.label4.Location = new System.Drawing.Point(151, 282);
            this.label4.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(39, 39);
            this.label4.TabIndex = 16;
            this.label4.Text = "Y";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Font = new System.Drawing.Font("Microsoft Sans Serif", 20.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(162)));
            this.label5.Location = new System.Drawing.Point(244, 282);
            this.label5.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(38, 39);
            this.label5.TabIndex = 17;
            this.label5.Text = "Z";
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(0, 324);
            this.button1.Margin = new System.Windows.Forms.Padding(4);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(100, 28);
            this.button1.TabIndex = 18;
            this.button1.Text = "button1";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click_1);
            // 
            // button2
            // 
            this.button2.Location = new System.Drawing.Point(108, 325);
            this.button2.Margin = new System.Windows.Forms.Padding(4);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(100, 28);
            this.button2.TabIndex = 19;
            this.button2.Text = "button2";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.button2_Click);
            // 
            // button3
            // 
            this.button3.Location = new System.Drawing.Point(227, 324);
            this.button3.Margin = new System.Windows.Forms.Padding(4);
            this.button3.Name = "button3";
            this.button3.Size = new System.Drawing.Size(100, 28);
            this.button3.TabIndex = 20;
            this.button3.Text = "button3";
            this.button3.UseVisualStyleBackColor = true;
            this.button3.Click += new System.EventHandler(this.button3_Click);
            // 
            // gMapControl1
            // 
            this.gMapControl1.Bearing = 0F;
            this.gMapControl1.CanDragMap = true;
            this.gMapControl1.EmptyTileColor = System.Drawing.Color.Navy;
            this.gMapControl1.GrayScaleMode = false;
            this.gMapControl1.HelperLineOption = GMap.NET.WindowsForms.HelperLineOptions.DontShow;
            this.gMapControl1.LevelsKeepInMemory = 5;
            this.gMapControl1.Location = new System.Drawing.Point(8, 21);
            this.gMapControl1.Margin = new System.Windows.Forms.Padding(4);
            this.gMapControl1.MarkersEnabled = true;
            this.gMapControl1.MaxZoom = 2;
            this.gMapControl1.MinZoom = 2;
            this.gMapControl1.MouseWheelZoomEnabled = true;
            this.gMapControl1.MouseWheelZoomType = GMap.NET.MouseWheelZoomType.MousePositionAndCenter;
            this.gMapControl1.Name = "gMapControl1";
            this.gMapControl1.NegativeMode = false;
            this.gMapControl1.PolygonsEnabled = true;
            this.gMapControl1.RetryLoadTile = 0;
            this.gMapControl1.RoutesEnabled = true;
            this.gMapControl1.ScaleMode = GMap.NET.WindowsForms.ScaleModes.Integer;
            this.gMapControl1.SelectedAreaFillColor = System.Drawing.Color.FromArgb(((int)(((byte)(33)))), ((int)(((byte)(65)))), ((int)(((byte)(105)))), ((int)(((byte)(225)))));
            this.gMapControl1.ShowTileGridLines = false;
            this.gMapControl1.Size = new System.Drawing.Size(407, 262);
            this.gMapControl1.TabIndex = 22;
            this.gMapControl1.Zoom = 0D;
            this.gMapControl1.Load += new System.EventHandler(this.gMapControl1_Load);
            // 
            // Bul
            // 
            this.Bul.Location = new System.Drawing.Point(8, 302);
            this.Bul.Margin = new System.Windows.Forms.Padding(4);
            this.Bul.Name = "Bul";
            this.Bul.Size = new System.Drawing.Size(88, 36);
            this.Bul.TabIndex = 23;
            this.Bul.Text = "Bul";
            this.Bul.UseVisualStyleBackColor = true;
            this.Bul.Click += new System.EventHandler(this.button4_Click);
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(137, 291);
            this.label6.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(57, 16);
            this.label6.TabIndex = 26;
            this.label6.Text = "🌍Enlem";
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(261, 287);
            this.label7.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(65, 16);
            this.label7.TabIndex = 27;
            this.label7.Text = "🌎Boylam";
            // 
            // Yakınlaştır
            // 
            this.Yakınlaştır.Location = new System.Drawing.Point(8, 342);
            this.Yakınlaştır.Margin = new System.Windows.Forms.Padding(4);
            this.Yakınlaştır.Name = "Yakınlaştır";
            this.Yakınlaştır.Size = new System.Drawing.Size(89, 38);
            this.Yakınlaştır.TabIndex = 28;
            this.Yakınlaştır.Text = "Yakınlaştır";
            this.Yakınlaştır.UseVisualStyleBackColor = true;
            this.Yakınlaştır.Click += new System.EventHandler(this.Yakınlaştır_Click);
            // 
            // Uzaklaştır
            // 
            this.Uzaklaştır.Location = new System.Drawing.Point(8, 385);
            this.Uzaklaştır.Margin = new System.Windows.Forms.Padding(4);
            this.Uzaklaştır.Name = "Uzaklaştır";
            this.Uzaklaştır.Size = new System.Drawing.Size(89, 34);
            this.Uzaklaştır.TabIndex = 29;
            this.Uzaklaştır.Text = "Uzaklaştır";
            this.Uzaklaştır.UseVisualStyleBackColor = true;
            this.Uzaklaştır.Click += new System.EventHandler(this.button5_Click);
            // 
            // groupBox1
            // 
            this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox1.Controls.Add(this.label8);
            this.groupBox1.Controls.Add(this.label6);
            this.groupBox1.Controls.Add(this.Uzaklaştır);
            this.groupBox1.Controls.Add(this.gMapControl1);
            this.groupBox1.Controls.Add(this.Yakınlaştır);
            this.groupBox1.Controls.Add(this.Bul);
            this.groupBox1.Controls.Add(this.label7);
            this.groupBox1.Controls.Add(this.textBox1);
            this.groupBox1.Controls.Add(this.textBox2);
            this.groupBox1.Controls.Add(this.textBox3);
            this.groupBox1.Location = new System.Drawing.Point(1189, 267);
            this.groupBox1.Margin = new System.Windows.Forms.Padding(4);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Padding = new System.Windows.Forms.Padding(4);
            this.groupBox1.Size = new System.Drawing.Size(419, 428);
            this.groupBox1.TabIndex = 30;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Konum";
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(181, 351);
            this.label8.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(105, 16);
            this.label8.TabIndex = 31;
            this.label8.Text = "📍GPSYükseklik";
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.label3);
            this.groupBox2.Controls.Add(this.glControl1);
            this.groupBox2.Controls.Add(this.button3);
            this.groupBox2.Controls.Add(this.label4);
            this.groupBox2.Controls.Add(this.button1);
            this.groupBox2.Controls.Add(this.label5);
            this.groupBox2.Controls.Add(this.button2);
            this.groupBox2.Location = new System.Drawing.Point(1731, 279);
            this.groupBox2.Margin = new System.Windows.Forms.Padding(4);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Padding = new System.Windows.Forms.Padding(4);
            this.groupBox2.Size = new System.Drawing.Size(309, 428);
            this.groupBox2.TabIndex = 31;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Uydu Pozisyonu";
            this.groupBox2.Enter += new System.EventHandler(this.groupBox2_Enter);
            // 
            // groupBoxMultiSpektral
            // 
            this.groupBoxMultiSpektral.Controls.Add(this.labelMultiSpektralDurum);
            this.groupBoxMultiSpektral.Controls.Add(this.labelMultiSpektralAciklama);
            this.groupBoxMultiSpektral.Controls.Add(this.labelMultiSpektralKomut);
            this.groupBoxMultiSpektral.Controls.Add(this.buttonMultiSpektralGonder);
            this.groupBoxMultiSpektral.Controls.Add(this.textBoxMultiSpektralKomut);
            this.groupBoxMultiSpektral.Location = new System.Drawing.Point(101, 480);
            this.groupBoxMultiSpektral.Margin = new System.Windows.Forms.Padding(4);
            this.groupBoxMultiSpektral.Name = "groupBoxMultiSpektral";
            this.groupBoxMultiSpektral.Padding = new System.Windows.Forms.Padding(4);
            this.groupBoxMultiSpektral.Size = new System.Drawing.Size(381, 148);
            this.groupBoxMultiSpektral.TabIndex = 40;
            this.groupBoxMultiSpektral.TabStop = false;
            this.groupBoxMultiSpektral.Text = "Multi-Spektral Filtreleme";
            // 
            // labelMultiSpektralDurum
            // 
            this.labelMultiSpektralDurum.AutoSize = true;
            this.labelMultiSpektralDurum.Location = new System.Drawing.Point(13, 105);
            this.labelMultiSpektralDurum.Name = "labelMultiSpektralDurum";
            this.labelMultiSpektralDurum.Size = new System.Drawing.Size(130, 16);
            this.labelMultiSpektralDurum.TabIndex = 44;
            this.labelMultiSpektralDurum.Text = "Multi-Spektral Durum";
            // 
            // labelMultiSpektralAciklama
            // 
            this.labelMultiSpektralAciklama.AutoSize = true;
            this.labelMultiSpektralAciklama.Location = new System.Drawing.Point(13, 31);
            this.labelMultiSpektralAciklama.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.labelMultiSpektralAciklama.Name = "labelMultiSpektralAciklama";
            this.labelMultiSpektralAciklama.Size = new System.Drawing.Size(222, 16);
            this.labelMultiSpektralAciklama.TabIndex = 3;
            this.labelMultiSpektralAciklama.Text = "Ör: 6G9R (Rakam-Harf-Rakam-Harf)";
            // 
            // labelMultiSpektralKomut
            // 
            this.labelMultiSpektralKomut.AutoSize = true;
            this.labelMultiSpektralKomut.Location = new System.Drawing.Point(13, 65);
            this.labelMultiSpektralKomut.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.labelMultiSpektralKomut.Name = "labelMultiSpektralKomut";
            this.labelMultiSpektralKomut.Size = new System.Drawing.Size(47, 16);
            this.labelMultiSpektralKomut.TabIndex = 2;
            this.labelMultiSpektralKomut.Text = "Komut:";
            // 
            // buttonMultiSpektralGonder
            // 
            this.buttonMultiSpektralGonder.Location = new System.Drawing.Point(285, 94);
            this.buttonMultiSpektralGonder.Margin = new System.Windows.Forms.Padding(4);
            this.buttonMultiSpektralGonder.Name = "buttonMultiSpektralGonder";
            this.buttonMultiSpektralGonder.Size = new System.Drawing.Size(96, 31);
            this.buttonMultiSpektralGonder.TabIndex = 1;
            this.buttonMultiSpektralGonder.Text = "Gönder";
            this.buttonMultiSpektralGonder.UseVisualStyleBackColor = true;
            this.buttonMultiSpektralGonder.Click += new System.EventHandler(this.buttonMultiSpektralGonder_Click);
            // 
            // textBoxMultiSpektralKomut
            // 
            this.textBoxMultiSpektralKomut.Location = new System.Drawing.Point(147, 68);
            this.textBoxMultiSpektralKomut.Margin = new System.Windows.Forms.Padding(4);
            this.textBoxMultiSpektralKomut.MaxLength = 4;
            this.textBoxMultiSpektralKomut.Name = "textBoxMultiSpektralKomut";
            this.textBoxMultiSpektralKomut.Size = new System.Drawing.Size(108, 22);
            this.textBoxMultiSpektralKomut.TabIndex = 0;
            this.textBoxMultiSpektralKomut.TextChanged += new System.EventHandler(this.textBoxMultiSpektralKomut_TextChanged);
            // 
            // videoSourcePlayer
            // 
            this.videoSourcePlayer.BackColor = System.Drawing.Color.Gray;
            this.videoSourcePlayer.Location = new System.Drawing.Point(1660, 4);
            this.videoSourcePlayer.Margin = new System.Windows.Forms.Padding(4);
            this.videoSourcePlayer.Name = "videoSourcePlayer";
            this.videoSourcePlayer.Size = new System.Drawing.Size(357, 261);
            this.videoSourcePlayer.TabIndex = 35;
            this.videoSourcePlayer.Text = "Kamera Göürnümü";
            this.videoSourcePlayer.VideoSource = null;
            // 
            // groupBoxARAS
            // 
            this.groupBoxARAS.Controls.Add(this.tableLayoutPanelAlarm);
            this.groupBoxARAS.Controls.Add(this.checkBoxSesliUyari);
            this.groupBoxARAS.Controls.Add(this.labelHata6);
            this.groupBoxARAS.Controls.Add(this.labelHata5);
            this.groupBoxARAS.Controls.Add(this.labelHata4);
            this.groupBoxARAS.Controls.Add(this.labelHata3);
            this.groupBoxARAS.Controls.Add(this.labelHata2);
            this.groupBoxARAS.Controls.Add(this.labelHata1);
            this.groupBoxARAS.Controls.Add(this.panelHata6);
            this.groupBoxARAS.Controls.Add(this.panelHata5);
            this.groupBoxARAS.Controls.Add(this.panelHata4);
            this.groupBoxARAS.Controls.Add(this.panelHata3);
            this.groupBoxARAS.Controls.Add(this.panelHata2);
            this.groupBoxARAS.Controls.Add(this.panelHata1);
            this.groupBoxARAS.Controls.Add(this.labelHataKoduBaslik);
            this.groupBoxARAS.Controls.Add(this.labelHataKodu);
            this.groupBoxARAS.Location = new System.Drawing.Point(1412, 705);
            this.groupBoxARAS.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.groupBoxARAS.Name = "groupBoxARAS";
            this.groupBoxARAS.Padding = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.groupBoxARAS.Size = new System.Drawing.Size(419, 305);
            this.groupBoxARAS.TabIndex = 0;
            this.groupBoxARAS.TabStop = false;
            this.groupBoxARAS.Text = "Alarm Sistemi";
            // 
            // tableLayoutPanelAlarm
            // 
            this.tableLayoutPanelAlarm.ColumnCount = 6;
            this.tableLayoutPanelAlarm.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 16.66667F));
            this.tableLayoutPanelAlarm.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 16.66667F));
            this.tableLayoutPanelAlarm.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 16.66667F));
            this.tableLayoutPanelAlarm.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 16.66667F));
            this.tableLayoutPanelAlarm.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 16.66667F));
            this.tableLayoutPanelAlarm.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 16.66667F));
            this.tableLayoutPanelAlarm.Controls.Add(this.panelAlarm1, 0, 0);
            this.tableLayoutPanelAlarm.Controls.Add(this.panelAlarm2, 1, 0);
            this.tableLayoutPanelAlarm.Controls.Add(this.panelAlarm3, 2, 0);
            this.tableLayoutPanelAlarm.Controls.Add(this.panelAlarm4, 3, 0);
            this.tableLayoutPanelAlarm.Controls.Add(this.panelAlarm5, 4, 0);
            this.tableLayoutPanelAlarm.Controls.Add(this.panelAlarm6, 5, 0);
            this.tableLayoutPanelAlarm.Controls.Add(this.label14, 0, 1);
            this.tableLayoutPanelAlarm.Controls.Add(this.label15, 1, 1);
            this.tableLayoutPanelAlarm.Controls.Add(this.label16, 2, 1);
            this.tableLayoutPanelAlarm.Controls.Add(this.label17, 3, 1);
            this.tableLayoutPanelAlarm.Controls.Add(this.label18, 4, 1);
            this.tableLayoutPanelAlarm.Controls.Add(this.label19, 5, 1);
            this.tableLayoutPanelAlarm.Location = new System.Drawing.Point(17, 192);
            this.tableLayoutPanelAlarm.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.tableLayoutPanelAlarm.Name = "tableLayoutPanelAlarm";
            this.tableLayoutPanelAlarm.RowCount = 2;
            this.tableLayoutPanelAlarm.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 70F));
            this.tableLayoutPanelAlarm.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 30F));
            this.tableLayoutPanelAlarm.Size = new System.Drawing.Size(380, 78);
            this.tableLayoutPanelAlarm.TabIndex = 16;
            // 
            // panelAlarm1
            // 
            this.panelAlarm1.BackColor = System.Drawing.Color.Green;
            this.panelAlarm1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelAlarm1.Location = new System.Drawing.Point(3, 2);
            this.panelAlarm1.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.panelAlarm1.Name = "panelAlarm1";
            this.panelAlarm1.Size = new System.Drawing.Size(57, 50);
            this.panelAlarm1.TabIndex = 0;
            // 
            // panelAlarm2
            // 
            this.panelAlarm2.BackColor = System.Drawing.Color.Green;
            this.panelAlarm2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelAlarm2.Location = new System.Drawing.Point(66, 2);
            this.panelAlarm2.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.panelAlarm2.Name = "panelAlarm2";
            this.panelAlarm2.Size = new System.Drawing.Size(57, 50);
            this.panelAlarm2.TabIndex = 1;
            // 
            // panelAlarm3
            // 
            this.panelAlarm3.BackColor = System.Drawing.Color.Green;
            this.panelAlarm3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelAlarm3.Location = new System.Drawing.Point(129, 2);
            this.panelAlarm3.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.panelAlarm3.Name = "panelAlarm3";
            this.panelAlarm3.Size = new System.Drawing.Size(57, 50);
            this.panelAlarm3.TabIndex = 2;
            // 
            // panelAlarm4
            // 
            this.panelAlarm4.BackColor = System.Drawing.Color.Green;
            this.panelAlarm4.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelAlarm4.Location = new System.Drawing.Point(192, 2);
            this.panelAlarm4.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.panelAlarm4.Name = "panelAlarm4";
            this.panelAlarm4.Size = new System.Drawing.Size(57, 50);
            this.panelAlarm4.TabIndex = 3;
            // 
            // panelAlarm5
            // 
            this.panelAlarm5.BackColor = System.Drawing.Color.Green;
            this.panelAlarm5.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelAlarm5.Location = new System.Drawing.Point(255, 2);
            this.panelAlarm5.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.panelAlarm5.Name = "panelAlarm5";
            this.panelAlarm5.Size = new System.Drawing.Size(57, 50);
            this.panelAlarm5.TabIndex = 4;
            // 
            // panelAlarm6
            // 
            this.panelAlarm6.BackColor = System.Drawing.Color.Green;
            this.panelAlarm6.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelAlarm6.Location = new System.Drawing.Point(318, 2);
            this.panelAlarm6.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.panelAlarm6.Name = "panelAlarm6";
            this.panelAlarm6.Size = new System.Drawing.Size(59, 50);
            this.panelAlarm6.TabIndex = 5;
            // 
            // label14
            // 
            this.label14.AutoSize = true;
            this.label14.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label14.Location = new System.Drawing.Point(3, 54);
            this.label14.Name = "label14";
            this.label14.Size = new System.Drawing.Size(57, 24);
            this.label14.TabIndex = 6;
            this.label14.Text = "1";
            this.label14.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // label15
            // 
            this.label15.AutoSize = true;
            this.label15.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label15.Location = new System.Drawing.Point(66, 54);
            this.label15.Name = "label15";
            this.label15.Size = new System.Drawing.Size(57, 24);
            this.label15.TabIndex = 7;
            this.label15.Text = "2";
            this.label15.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // label16
            // 
            this.label16.AutoSize = true;
            this.label16.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label16.Location = new System.Drawing.Point(129, 54);
            this.label16.Name = "label16";
            this.label16.Size = new System.Drawing.Size(57, 24);
            this.label16.TabIndex = 8;
            this.label16.Text = "3";
            this.label16.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // label17
            // 
            this.label17.AutoSize = true;
            this.label17.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label17.Location = new System.Drawing.Point(192, 54);
            this.label17.Name = "label17";
            this.label17.Size = new System.Drawing.Size(57, 24);
            this.label17.TabIndex = 9;
            this.label17.Text = "4";
            this.label17.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // label18
            // 
            this.label18.AutoSize = true;
            this.label18.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label18.Location = new System.Drawing.Point(255, 54);
            this.label18.Name = "label18";
            this.label18.Size = new System.Drawing.Size(57, 24);
            this.label18.TabIndex = 10;
            this.label18.Text = "5";
            this.label18.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // label19
            // 
            this.label19.AutoSize = true;
            this.label19.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label19.Location = new System.Drawing.Point(318, 54);
            this.label19.Name = "label19";
            this.label19.Size = new System.Drawing.Size(59, 24);
            this.label19.TabIndex = 11;
            this.label19.Text = "6";
            this.label19.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // checkBoxSesliUyari
            // 
            this.checkBoxSesliUyari.AutoSize = true;
            this.checkBoxSesliUyari.Checked = true;
            this.checkBoxSesliUyari.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBoxSesliUyari.Location = new System.Drawing.Point(17, 274);
            this.checkBoxSesliUyari.Margin = new System.Windows.Forms.Padding(4);
            this.checkBoxSesliUyari.Name = "checkBoxSesliUyari";
            this.checkBoxSesliUyari.Size = new System.Drawing.Size(122, 20);
            this.checkBoxSesliUyari.TabIndex = 14;
            this.checkBoxSesliUyari.Text = "Sesli Uyarı Aktif";
            this.checkBoxSesliUyari.UseVisualStyleBackColor = true;
            this.checkBoxSesliUyari.CheckedChanged += new System.EventHandler(this.checkBoxSesliUyari_CheckedChanged);
            // 
            // labelHata6
            // 
            this.labelHata6.AutoSize = true;
            this.labelHata6.Location = new System.Drawing.Point(276, 148);
            this.labelHata6.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.labelHata6.Name = "labelHata6";
            this.labelHata6.Size = new System.Drawing.Size(132, 16);
            this.labelHata6.TabIndex = 13;
            this.labelHata6.Text = "Multi-Spektral Sistem";
            // 
            // labelHata5
            // 
            this.labelHata5.AutoSize = true;
            this.labelHata5.Location = new System.Drawing.Point(276, 111);
            this.labelHata5.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.labelHata5.Name = "labelHata5";
            this.labelHata5.Size = new System.Drawing.Size(101, 16);
            this.labelHata5.TabIndex = 12;
            this.labelHata5.Text = "Ayrılma Durumu";
            // 
            // labelHata4
            // 
            this.labelHata4.AutoSize = true;
            this.labelHata4.Location = new System.Drawing.Point(276, 74);
            this.labelHata4.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.labelHata4.Name = "labelHata4";
            this.labelHata4.Size = new System.Drawing.Size(121, 16);
            this.labelHata4.TabIndex = 11;
            this.labelHata4.Text = "Görev Yükü Konum";
            // 
            // labelHata3
            // 
            this.labelHata3.AutoSize = true;
            this.labelHata3.Location = new System.Drawing.Point(53, 148);
            this.labelHata3.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.labelHata3.Name = "labelHata3";
            this.labelHata3.Size = new System.Drawing.Size(135, 16);
            this.labelHata3.TabIndex = 10;
            this.labelHata3.Text = "Taşıyıcı Basınç Verisi";
            // 
            // labelHata2
            // 
            this.labelHata2.AutoSize = true;
            this.labelHata2.Location = new System.Drawing.Point(53, 111);
            this.labelHata2.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.labelHata2.Name = "labelHata2";
            this.labelHata2.Size = new System.Drawing.Size(125, 16);
            this.labelHata2.TabIndex = 9;
            this.labelHata2.Text = "Görev Yükü İniş Hızı";
            // 
            // labelHata1
            // 
            this.labelHata1.AutoSize = true;
            this.labelHata1.Location = new System.Drawing.Point(53, 74);
            this.labelHata1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.labelHata1.Name = "labelHata1";
            this.labelHata1.Size = new System.Drawing.Size(102, 16);
            this.labelHata1.TabIndex = 8;
            this.labelHata1.Text = "Taşıyıcı İniş Hızı";
            // 
            // panelHata6
            // 
            this.panelHata6.BackColor = System.Drawing.Color.Green;
            this.panelHata6.Location = new System.Drawing.Point(240, 148);
            this.panelHata6.Margin = new System.Windows.Forms.Padding(4);
            this.panelHata6.Name = "panelHata6";
            this.panelHata6.Size = new System.Drawing.Size(27, 25);
            this.panelHata6.TabIndex = 7;
            // 
            // panelHata5
            // 
            this.panelHata5.BackColor = System.Drawing.Color.Green;
            this.panelHata5.Location = new System.Drawing.Point(240, 111);
            this.panelHata5.Margin = new System.Windows.Forms.Padding(4);
            this.panelHata5.Name = "panelHata5";
            this.panelHata5.Size = new System.Drawing.Size(27, 25);
            this.panelHata5.TabIndex = 6;
            // 
            // panelHata4
            // 
            this.panelHata4.BackColor = System.Drawing.Color.Green;
            this.panelHata4.Location = new System.Drawing.Point(240, 74);
            this.panelHata4.Margin = new System.Windows.Forms.Padding(4);
            this.panelHata4.Name = "panelHata4";
            this.panelHata4.Size = new System.Drawing.Size(27, 25);
            this.panelHata4.TabIndex = 5;
            // 
            // panelHata3
            // 
            this.panelHata3.BackColor = System.Drawing.Color.Green;
            this.panelHata3.Location = new System.Drawing.Point(17, 148);
            this.panelHata3.Margin = new System.Windows.Forms.Padding(4);
            this.panelHata3.Name = "panelHata3";
            this.panelHata3.Size = new System.Drawing.Size(27, 25);
            this.panelHata3.TabIndex = 4;
            // 
            // panelHata2
            // 
            this.panelHata2.BackColor = System.Drawing.Color.Green;
            this.panelHata2.Location = new System.Drawing.Point(17, 111);
            this.panelHata2.Margin = new System.Windows.Forms.Padding(4);
            this.panelHata2.Name = "panelHata2";
            this.panelHata2.Size = new System.Drawing.Size(27, 25);
            this.panelHata2.TabIndex = 3;
            // 
            // panelHata1
            // 
            this.panelHata1.BackColor = System.Drawing.Color.Green;
            this.panelHata1.Location = new System.Drawing.Point(17, 74);
            this.panelHata1.Margin = new System.Windows.Forms.Padding(4);
            this.panelHata1.Name = "panelHata1";
            this.panelHata1.Size = new System.Drawing.Size(27, 25);
            this.panelHata1.TabIndex = 2;
            this.panelHata1.Paint += new System.Windows.Forms.PaintEventHandler(this.panelHata1_Paint);
            // 
            // labelHataKoduBaslik
            // 
            this.labelHataKoduBaslik.AutoSize = true;
            this.labelHataKoduBaslik.Location = new System.Drawing.Point(13, 31);
            this.labelHataKoduBaslik.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.labelHataKoduBaslik.Name = "labelHataKoduBaslik";
            this.labelHataKoduBaslik.Size = new System.Drawing.Size(118, 16);
            this.labelHataKoduBaslik.TabIndex = 1;
            this.labelHataKoduBaslik.Text = "Güncel Hata Kodu:";
            // 
            // labelHataKodu
            // 
            this.labelHataKodu.AutoSize = true;
            this.labelHataKodu.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(162)));
            this.labelHataKodu.Location = new System.Drawing.Point(187, 31);
            this.labelHataKodu.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.labelHataKodu.Name = "labelHataKodu";
            this.labelHataKodu.Size = new System.Drawing.Size(110, 25);
            this.labelHataKodu.TabIndex = 0;
            this.labelHataKodu.Text = "<000000>";
            // 
            // groupBoxKalibrasyon
            // 
            this.groupBoxKalibrasyon.Controls.Add(this.buttonGyroSifirla);
            this.groupBoxKalibrasyon.Controls.Add(this.buttonGPSReferans);
            this.groupBoxKalibrasyon.Controls.Add(this.buttonBasincKalibre);
            this.groupBoxKalibrasyon.Controls.Add(this.comboBoxSehir);
            this.groupBoxKalibrasyon.Controls.Add(this.labelSehirSecimi);
            this.groupBoxKalibrasyon.Controls.Add(this.labelKalibrasyonDurum);
            this.groupBoxKalibrasyon.Location = new System.Drawing.Point(1303, 14);
            this.groupBoxKalibrasyon.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.groupBoxKalibrasyon.Name = "groupBoxKalibrasyon";
            this.groupBoxKalibrasyon.Padding = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.groupBoxKalibrasyon.Size = new System.Drawing.Size(345, 108);
            this.groupBoxKalibrasyon.TabIndex = 49;
            this.groupBoxKalibrasyon.TabStop = false;
            this.groupBoxKalibrasyon.Text = "🔧 Kalibrasyon Sistemi";
            // 
            // buttonGyroSifirla
            // 
            this.buttonGyroSifirla.BackColor = System.Drawing.Color.Blue;
            this.buttonGyroSifirla.Font = new System.Drawing.Font("Microsoft Sans Serif", 7F, System.Drawing.FontStyle.Bold);
            this.buttonGyroSifirla.ForeColor = System.Drawing.Color.White;
            this.buttonGyroSifirla.Location = new System.Drawing.Point(231, 46);
            this.buttonGyroSifirla.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.buttonGyroSifirla.Name = "buttonGyroSifirla";
            this.buttonGyroSifirla.Size = new System.Drawing.Size(105, 25);
            this.buttonGyroSifirla.TabIndex = 3;
            this.buttonGyroSifirla.Text = "🔄 Gyro Sıfırlama";
            this.buttonGyroSifirla.UseVisualStyleBackColor = false;
            this.buttonGyroSifirla.Click += new System.EventHandler(this.buttonGyroSifirla_Click);
            // 
            // buttonGPSReferans
            // 
            this.buttonGPSReferans.BackColor = System.Drawing.Color.Green;
            this.buttonGPSReferans.Font = new System.Drawing.Font("Microsoft Sans Serif", 7F, System.Drawing.FontStyle.Bold);
            this.buttonGPSReferans.ForeColor = System.Drawing.Color.White;
            this.buttonGPSReferans.Location = new System.Drawing.Point(125, 46);
            this.buttonGPSReferans.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.buttonGPSReferans.Name = "buttonGPSReferans";
            this.buttonGPSReferans.Size = new System.Drawing.Size(100, 25);
            this.buttonGPSReferans.TabIndex = 2;
            this.buttonGPSReferans.Text = "📍 GPS Referans";
            this.buttonGPSReferans.UseVisualStyleBackColor = false;
            this.buttonGPSReferans.Click += new System.EventHandler(this.buttonGPSReferans_Click);
            // 
            // buttonBasincKalibre
            // 
            this.buttonBasincKalibre.BackColor = System.Drawing.Color.Orange;
            this.buttonBasincKalibre.Font = new System.Drawing.Font("Microsoft Sans Serif", 7F, System.Drawing.FontStyle.Bold);
            this.buttonBasincKalibre.ForeColor = System.Drawing.Color.White;
            this.buttonBasincKalibre.Location = new System.Drawing.Point(9, 46);
            this.buttonBasincKalibre.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.buttonBasincKalibre.Name = "buttonBasincKalibre";
            this.buttonBasincKalibre.Size = new System.Drawing.Size(109, 25);
            this.buttonBasincKalibre.TabIndex = 1;
            this.buttonBasincKalibre.Text = "🔧 Basınç Kalibrasyonu";
            this.buttonBasincKalibre.UseVisualStyleBackColor = false;
            this.buttonBasincKalibre.Click += new System.EventHandler(this.buttonBasincKalibre_Click);
            // 
            // comboBoxSehir
            // 
            this.comboBoxSehir.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxSehir.FormattingEnabled = true;
            this.comboBoxSehir.Items.AddRange(new object[] {
            "Ankara (850m)",
            "İstanbul (40m)",
            "İzmir (25m)",
            "Antalya (50m)",
            "Kayseri (1054m)",
            "Bursa (100m)"});
            this.comboBoxSehir.Location = new System.Drawing.Point(85, 18);
            this.comboBoxSehir.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.comboBoxSehir.Name = "comboBoxSehir";
            this.comboBoxSehir.Size = new System.Drawing.Size(140, 24);
            this.comboBoxSehir.TabIndex = 0;
            this.comboBoxSehir.SelectedIndexChanged += new System.EventHandler(this.comboBoxSehir_SelectedIndexChanged);
            // 
            // labelSehirSecimi
            // 
            this.labelSehirSecimi.AutoSize = true;
            this.labelSehirSecimi.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F, System.Drawing.FontStyle.Bold);
            this.labelSehirSecimi.Location = new System.Drawing.Point(5, 21);
            this.labelSehirSecimi.Name = "labelSehirSecimi";
            this.labelSehirSecimi.Size = new System.Drawing.Size(73, 17);
            this.labelSehirSecimi.TabIndex = 5;
            this.labelSehirSecimi.Text = "🏙️ Şehir:";
            // 
            // labelKalibrasyonDurum
            // 
            this.labelKalibrasyonDurum.AutoSize = true;
            this.labelKalibrasyonDurum.Font = new System.Drawing.Font("Microsoft Sans Serif", 7F);
            this.labelKalibrasyonDurum.ForeColor = System.Drawing.Color.Gray;
            this.labelKalibrasyonDurum.Location = new System.Drawing.Point(5, 78);
            this.labelKalibrasyonDurum.Name = "labelKalibrasyonDurum";
            this.labelKalibrasyonDurum.Size = new System.Drawing.Size(175, 15);
            this.labelKalibrasyonDurum.TabIndex = 4;
            this.labelKalibrasyonDurum.Text = "Durum: Kalibrasyon Bekleniyor";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(-71, 331);
            this.label2.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(44, 16);
            this.label2.TabIndex = 5;
            this.label2.Text = "label2";
            // 
            // chart3
            // 
            chartArea1.Name = "ChartAreaPilGerilimi";
            this.chart3.ChartAreas.Add(chartArea1);
            this.chart3.Location = new System.Drawing.Point(-1000, -999);
            this.chart3.Margin = new System.Windows.Forms.Padding(4);
            this.chart3.Name = "chart3";
            series1.ChartArea = "ChartAreaPilGerilimi";
            series1.ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Line;
            series1.Name = "Pil Gerilimi";
            this.chart3.Series.Add(series1);
            this.chart3.Size = new System.Drawing.Size(1, 1);
            this.chart3.TabIndex = 37;
            this.chart3.Text = "chart3";
            title1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(162)));
            title1.Name = "Title1";
            title1.Text = "Basınç(Pa/saniye)";
            this.chart3.Titles.Add(title1);
            this.chart3.Visible = false;
            this.chart3.Click += new System.EventHandler(this.chart3_Click);
            // 
            // chart4
            // 
            chartArea2.Name = "ChartAreaBasinc";
            this.chart4.ChartAreas.Add(chartArea2);
            this.chart4.Location = new System.Drawing.Point(-1000, -999);
            this.chart4.Margin = new System.Windows.Forms.Padding(4);
            this.chart4.Name = "chart4";
            series2.ChartArea = "ChartAreaBasinc";
            series2.ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Line;
            series2.Name = "Basınç(Pascal)";
            this.chart4.Series.Add(series2);
            this.chart4.Size = new System.Drawing.Size(1, 1);
            this.chart4.TabIndex = 38;
            this.chart4.Text = "chart4";
            title2.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(162)));
            title2.Name = "Title1";
            title2.Text = "Basınç(Pa/S)";
            this.chart4.Titles.Add(title2);
            this.chart4.Visible = false;
            // 
            // chart5
            // 
            chartArea3.Name = "ChartAreaChart5";
            this.chart5.ChartAreas.Add(chartArea3);
            this.chart5.Location = new System.Drawing.Point(-1000, -999);
            this.chart5.Margin = new System.Windows.Forms.Padding(4);
            this.chart5.Name = "chart5";
            series3.ChartArea = "ChartAreaChart5";
            series3.ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Line;
            series3.Name = "Hız(m/s)";
            this.chart5.Series.Add(series3);
            this.chart5.Size = new System.Drawing.Size(1, 1);
            this.chart5.TabIndex = 43;
            this.chart5.Text = "chart5";
            title3.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(162)));
            title3.Name = "Title1";
            title3.Text = "Pil Gerilimi(V/saniye)";
            this.chart5.Titles.Add(title3);
            this.chart5.Visible = false;
            // 
            // groupBox3
            // 
            this.groupBox3.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox3.Controls.Add(this.ExlAktar);
            this.groupBox3.Controls.Add(this.labelTakimNumarasi);
            this.groupBox3.Controls.Add(this.label36);
            this.groupBox3.Controls.Add(this.label35);
            this.groupBox3.Controls.Add(this.label34);
            this.groupBox3.Controls.Add(this.label33);
            this.groupBox3.Controls.Add(this.label32);
            this.groupBox3.Controls.Add(this.label26);
            this.groupBox3.Controls.Add(this.label20);
            this.groupBox3.Controls.Add(this.textBox22);
            this.groupBox3.Controls.Add(this.textBox21);
            this.groupBox3.Controls.Add(this.textBox20);
            this.groupBox3.Controls.Add(this.textBox19);
            this.groupBox3.Controls.Add(this.textBox18);
            this.groupBox3.Controls.Add(this.textBox17);
            this.groupBox3.Controls.Add(this.textBox16);
            this.groupBox3.Controls.Add(this.label31);
            this.groupBox3.Controls.Add(this.label30);
            this.groupBox3.Controls.Add(this.label29);
            this.groupBox3.Controls.Add(this.label28);
            this.groupBox3.Controls.Add(this.label27);
            this.groupBox3.Controls.Add(this.label25);
            this.groupBox3.Controls.Add(this.label24);
            this.groupBox3.Controls.Add(this.label23);
            this.groupBox3.Controls.Add(this.label22);
            this.groupBox3.Controls.Add(this.label21);
            this.groupBox3.Controls.Add(this.groupBoxIoT);
            this.groupBox3.Controls.Add(this.textBox8);
            this.groupBox3.Controls.Add(this.chart5);
            this.groupBox3.Controls.Add(this.label13);
            this.groupBox3.Controls.Add(this.label12);
            this.groupBox3.Controls.Add(this.groupBoxMultiSpektral);
            this.groupBox3.Controls.Add(this.textBox12);
            this.groupBox3.Controls.Add(this.label11);
            this.groupBox3.Controls.Add(this.manuelAyrilma);
            this.groupBox3.Controls.Add(this.button4);
            this.groupBox3.Controls.Add(this.textBox7);
            this.groupBox3.Controls.Add(this.textBox6);
            this.groupBox3.Controls.Add(this.groupBoxUyduDurum);
            this.groupBox3.Controls.Add(this.textBox15);
            this.groupBox3.Controls.Add(this.label10);
            this.groupBox3.Controls.Add(this.label9);
            this.groupBox3.Controls.Add(this.textBox14);
            this.groupBox3.Controls.Add(this.textBox13);
            this.groupBox3.Controls.Add(this.textBox11);
            this.groupBox3.Controls.Add(this.textBox10);
            this.groupBox3.Controls.Add(this.textBox9);
            this.groupBox3.Controls.Add(this.comboBox1);
            this.groupBox3.Controls.Add(this.dataGridView1);
            this.groupBox3.Controls.Add(this.baglan);
            this.groupBox3.Controls.Add(this.texBaundRate);
            this.groupBox3.Controls.Add(this.kes);
            this.groupBox3.Controls.Add(this.chartYukseklik);
            this.groupBox3.Controls.Add(this.chartSicaklik);
            this.groupBox3.Controls.Add(this.chartBasinc);
            this.groupBox3.Controls.Add(this.chartHiz);
            this.groupBox3.Controls.Add(this.chartPilGerilimi);
            this.groupBox3.Controls.Add(this.chart4);
            this.groupBox3.Controls.Add(this.chart3);
            this.groupBox3.Controls.Add(this.label1);
            this.groupBox3.Controls.Add(this.label2);
            this.groupBox3.Controls.Add(this.textBox5);
            this.groupBox3.Controls.Add(this.textBox4);
            this.groupBox3.Location = new System.Drawing.Point(13, 14);
            this.groupBox3.Margin = new System.Windows.Forms.Padding(4);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Padding = new System.Windows.Forms.Padding(4);
            this.groupBox3.Size = new System.Drawing.Size(1169, 1268);
            this.groupBox3.TabIndex = 32;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "Veriler";
            this.groupBox3.Enter += new System.EventHandler(this.groupBox3_Enter);
            // 
            // ExlAktar
            // 
            this.ExlAktar.Location = new System.Drawing.Point(1063, 798);
            this.ExlAktar.Margin = new System.Windows.Forms.Padding(4);
            this.ExlAktar.Name = "ExlAktar";
            this.ExlAktar.Size = new System.Drawing.Size(99, 25);
            this.ExlAktar.TabIndex = 3;
            this.ExlAktar.Text = "📊 Excel";
            this.ExlAktar.UseVisualStyleBackColor = true;
            this.ExlAktar.Click += new System.EventHandler(this.ExlAktar_Click);
            // 
            // labelTakimNumarasi
            // 
            this.labelTakimNumarasi.AutoSize = true;
            this.labelTakimNumarasi.Font = new System.Drawing.Font("Segoe UI", 14F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelTakimNumarasi.ForeColor = System.Drawing.Color.DarkMagenta;
            this.labelTakimNumarasi.Location = new System.Drawing.Point(1027, 46);
            this.labelTakimNumarasi.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.labelTakimNumarasi.Name = "labelTakimNumarasi";
            this.labelTakimNumarasi.Size = new System.Drawing.Size(98, 32);
            this.labelTakimNumarasi.TabIndex = 50;
            this.labelTakimNumarasi.Text = "286570";
            // 
            // label36
            // 
            this.label36.AutoSize = true;
            this.label36.Location = new System.Drawing.Point(427, 808);
            this.label36.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label36.Name = "label36";
            this.label36.Size = new System.Drawing.Size(81, 16);
            this.label36.TabIndex = 75;
            this.label36.Text = "🏷️ Takım No";
            // 
            // label35
            // 
            this.label35.AutoSize = true;
            this.label35.Location = new System.Drawing.Point(471, 647);
            this.label35.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label35.Name = "label35";
            this.label35.Size = new System.Drawing.Size(43, 16);
            this.label35.TabIndex = 74;
            this.label35.Text = "↕️Pitch";
            // 
            // label34
            // 
            this.label34.AutoSize = true;
            this.label34.Location = new System.Drawing.Point(473, 669);
            this.label34.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label34.Name = "label34";
            this.label34.Size = new System.Drawing.Size(44, 16);
            this.label34.TabIndex = 73;
            this.label34.Text = "↔️Roll";
            // 
            // label33
            // 
            this.label33.AutoSize = true;
            this.label33.Location = new System.Drawing.Point(473, 695);
            this.label33.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label33.Name = "label33";
            this.label33.Size = new System.Drawing.Size(48, 16);
            this.label33.TabIndex = 72;
            this.label33.Text = "🔄 Yaw";
            // 
            // label32
            // 
            this.label32.AutoSize = true;
            this.label32.Location = new System.Drawing.Point(404, 754);
            this.label32.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label32.Name = "label32";
            this.label32.Size = new System.Drawing.Size(111, 16);
            this.label32.TabIndex = 71;
            this.label32.Text = "🌡️ IoT S1 Sıcaklık";
            // 
            // label26
            // 
            this.label26.AutoSize = true;
            this.label26.Location = new System.Drawing.Point(452, 722);
            this.label26.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label26.Name = "label26";
            this.label26.Size = new System.Drawing.Size(62, 16);
            this.label26.TabIndex = 70;
            this.label26.Text = "📡 RHRH";
            // 
            // label20
            // 
            this.label20.AutoSize = true;
            this.label20.Location = new System.Drawing.Point(404, 781);
            this.label20.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label20.Name = "label20";
            this.label20.Size = new System.Drawing.Size(111, 16);
            this.label20.TabIndex = 69;
            this.label20.Text = "🌡️ IoT S2 Sıcaklık";
            // 
            // textBox22
            // 
            this.textBox22.Location = new System.Drawing.Point(529, 803);
            this.textBox22.Margin = new System.Windows.Forms.Padding(4);
            this.textBox22.Name = "textBox22";
            this.textBox22.Size = new System.Drawing.Size(69, 22);
            this.textBox22.TabIndex = 68;
            // 
            // textBox21
            // 
            this.textBox21.Location = new System.Drawing.Point(529, 778);
            this.textBox21.Margin = new System.Windows.Forms.Padding(4);
            this.textBox21.Name = "textBox21";
            this.textBox21.Size = new System.Drawing.Size(73, 22);
            this.textBox21.TabIndex = 67;
            // 
            // textBox20
            // 
            this.textBox20.Location = new System.Drawing.Point(529, 748);
            this.textBox20.Margin = new System.Windows.Forms.Padding(4);
            this.textBox20.Name = "textBox20";
            this.textBox20.Size = new System.Drawing.Size(69, 22);
            this.textBox20.TabIndex = 66;
            // 
            // textBox19
            // 
            this.textBox19.Location = new System.Drawing.Point(529, 722);
            this.textBox19.Margin = new System.Windows.Forms.Padding(4);
            this.textBox19.Name = "textBox19";
            this.textBox19.Size = new System.Drawing.Size(69, 22);
            this.textBox19.TabIndex = 65;
            // 
            // textBox18
            // 
            this.textBox18.Location = new System.Drawing.Point(529, 695);
            this.textBox18.Margin = new System.Windows.Forms.Padding(4);
            this.textBox18.Name = "textBox18";
            this.textBox18.Size = new System.Drawing.Size(69, 22);
            this.textBox18.TabIndex = 64;
            // 
            // textBox17
            // 
            this.textBox17.Location = new System.Drawing.Point(303, 792);
            this.textBox17.Margin = new System.Windows.Forms.Padding(4);
            this.textBox17.Name = "textBox17";
            this.textBox17.Size = new System.Drawing.Size(79, 22);
            this.textBox17.TabIndex = 63;
            // 
            // textBox16
            // 
            this.textBox16.Location = new System.Drawing.Point(303, 765);
            this.textBox16.Margin = new System.Windows.Forms.Padding(4);
            this.textBox16.Name = "textBox16";
            this.textBox16.Size = new System.Drawing.Size(79, 22);
            this.textBox16.TabIndex = 62;
            // 
            // label31
            // 
            this.label31.AutoSize = true;
            this.label31.Location = new System.Drawing.Point(216, 653);
            this.label31.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label31.Name = "label31";
            this.label31.Size = new System.Drawing.Size(87, 16);
            this.label31.TabIndex = 61;
            this.label31.Text = "📏Yükseklik 1";
            // 
            // label30
            // 
            this.label30.AutoSize = true;
            this.label30.Location = new System.Drawing.Point(216, 683);
            this.label30.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label30.Name = "label30";
            this.label30.Size = new System.Drawing.Size(87, 16);
            this.label30.TabIndex = 60;
            this.label30.Text = "📏Yükseklik 2";
            // 
            // label29
            // 
            this.label29.AutoSize = true;
            this.label29.Location = new System.Drawing.Point(221, 710);
            this.label29.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label29.Name = "label29";
            this.label29.Size = new System.Drawing.Size(76, 16);
            this.label29.TabIndex = 59;
            this.label29.Text = "📐İrtifa Farkı";
            // 
            // label28
            // 
            this.label28.AutoSize = true;
            this.label28.Location = new System.Drawing.Point(231, 740);
            this.label28.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label28.Name = "label28";
            this.label28.Size = new System.Drawing.Size(64, 16);
            this.label28.TabIndex = 58;
            this.label28.Text = "🏃İniş Hızı";
            // 
            // label27
            // 
            this.label27.AutoSize = true;
            this.label27.Location = new System.Drawing.Point(217, 796);
            this.label27.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label27.Name = "label27";
            this.label27.Size = new System.Drawing.Size(85, 16);
            this.label27.TabIndex = 57;
            this.label27.Text = "🔋 Pil Gerilimi";
            // 
            // label25
            // 
            this.label25.AutoSize = true;
            this.label25.Location = new System.Drawing.Point(11, 677);
            this.label25.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label25.Name = "label25";
            this.label25.Size = new System.Drawing.Size(98, 16);
            this.label25.TabIndex = 55;
            this.label25.Text = "🛰️Uydu Statüsü";
            // 
            // label24
            // 
            this.label24.AutoSize = true;
            this.label24.Location = new System.Drawing.Point(27, 706);
            this.label24.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label24.Name = "label24";
            this.label24.Size = new System.Drawing.Size(82, 16);
            this.label24.TabIndex = 54;
            this.label24.Text = "⚠Hata Kodu";
            // 
            // label23
            // 
            this.label23.AutoSize = true;
            this.label23.Location = new System.Drawing.Point(-4, 743);
            this.label23.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label23.Name = "label23";
            this.label23.Size = new System.Drawing.Size(117, 16);
            this.label23.TabIndex = 53;
            this.label23.Text = "⏰Gönderme Saati";
            // 
            // label22
            // 
            this.label22.AutoSize = true;
            this.label22.Location = new System.Drawing.Point(43, 770);
            this.label22.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label22.Name = "label22";
            this.label22.Size = new System.Drawing.Size(70, 16);
            this.label22.TabIndex = 52;
            this.label22.Text = "🌀Basınç 1";
            // 
            // label21
            // 
            this.label21.AutoSize = true;
            this.label21.Location = new System.Drawing.Point(43, 798);
            this.label21.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label21.Name = "label21";
            this.label21.Size = new System.Drawing.Size(70, 16);
            this.label21.TabIndex = 51;
            this.label21.Text = "🌀Basınç 2";
            // 
            // groupBoxIoT
            // 
            this.groupBoxIoT.Controls.Add(this.buttonIotGoruntule);
            this.groupBoxIoT.Controls.Add(this.labelIoTS1);
            this.groupBoxIoT.Controls.Add(this.labelIoTS2);
            this.groupBoxIoT.Controls.Add(this.labelIoTS1Deger);
            this.groupBoxIoT.Controls.Add(this.labelIoTS2Deger);
            this.groupBoxIoT.Controls.Add(this.chartIoT);
            this.groupBoxIoT.Location = new System.Drawing.Point(627, 495);
            this.groupBoxIoT.Margin = new System.Windows.Forms.Padding(4);
            this.groupBoxIoT.Name = "groupBoxIoT";
            this.groupBoxIoT.Padding = new System.Windows.Forms.Padding(4);
            this.groupBoxIoT.Size = new System.Drawing.Size(665, 283);
            this.groupBoxIoT.TabIndex = 42;
            this.groupBoxIoT.TabStop = false;
            this.groupBoxIoT.Text = "IoT S2S Veri Transferi";
            // 
            // buttonIotGoruntule
            // 
            this.buttonIotGoruntule.Location = new System.Drawing.Point(11, 121);
            this.buttonIotGoruntule.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.buttonIotGoruntule.Name = "buttonIotGoruntule";
            this.buttonIotGoruntule.Size = new System.Drawing.Size(123, 58);
            this.buttonIotGoruntule.TabIndex = 25;
            this.buttonIotGoruntule.Text = "IoT Verilerini Göster";
            this.buttonIotGoruntule.UseVisualStyleBackColor = true;
            this.buttonIotGoruntule.Click += new System.EventHandler(this.buttonIotGoruntule_Click);
            // 
            // labelIoTS1
            // 
            this.labelIoTS1.AutoSize = true;
            this.labelIoTS1.Location = new System.Drawing.Point(13, 31);
            this.labelIoTS1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.labelIoTS1.Name = "labelIoTS1";
            this.labelIoTS1.Size = new System.Drawing.Size(117, 16);
            this.labelIoTS1.TabIndex = 0;
            this.labelIoTS1.Text = "IoT İstasyon 1 (°C):";
            // 
            // labelIoTS2
            // 
            this.labelIoTS2.AutoSize = true;
            this.labelIoTS2.Location = new System.Drawing.Point(13, 62);
            this.labelIoTS2.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.labelIoTS2.Name = "labelIoTS2";
            this.labelIoTS2.Size = new System.Drawing.Size(117, 16);
            this.labelIoTS2.TabIndex = 1;
            this.labelIoTS2.Text = "IoT İstasyon 2 (°C):";
            // 
            // labelIoTS1Deger
            // 
            this.labelIoTS1Deger.AutoSize = true;
            this.labelIoTS1Deger.Location = new System.Drawing.Point(140, 31);
            this.labelIoTS1Deger.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.labelIoTS1Deger.Name = "labelIoTS1Deger";
            this.labelIoTS1Deger.Size = new System.Drawing.Size(24, 16);
            this.labelIoTS1Deger.TabIndex = 2;
            this.labelIoTS1Deger.Text = "0.0";
            // 
            // labelIoTS2Deger
            // 
            this.labelIoTS2Deger.AutoSize = true;
            this.labelIoTS2Deger.Location = new System.Drawing.Point(140, 60);
            this.labelIoTS2Deger.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.labelIoTS2Deger.Name = "labelIoTS2Deger";
            this.labelIoTS2Deger.Size = new System.Drawing.Size(24, 16);
            this.labelIoTS2Deger.TabIndex = 3;
            this.labelIoTS2Deger.Text = "0.0";
            // 
            // chartIoT
            // 
            this.chartIoT.BackColor = System.Drawing.Color.WhiteSmoke;
            this.chartIoT.BorderlineColor = System.Drawing.Color.LightGray;
            this.chartIoT.BorderlineDashStyle = System.Windows.Forms.DataVisualization.Charting.ChartDashStyle.Solid;
            this.chartIoT.Location = new System.Drawing.Point(235, 7);
            this.chartIoT.Margin = new System.Windows.Forms.Padding(4);
            this.chartIoT.Name = "chartIoT";
            this.chartIoT.Size = new System.Drawing.Size(424, 246);
            this.chartIoT.TabIndex = 0;
            this.chartIoT.Text = "IoT Sıcaklık Verileri";
            // 
            // textBox8
            // 
            this.textBox8.Location = new System.Drawing.Point(128, 737);
            this.textBox8.Margin = new System.Windows.Forms.Padding(4);
            this.textBox8.Name = "textBox8";
            this.textBox8.Size = new System.Drawing.Size(76, 22);
            this.textBox8.TabIndex = 30;
            this.textBox8.TextChanged += new System.EventHandler(this.textBox8_TextChanged);
            // 
            // label13
            // 
            this.label13.AutoSize = true;
            this.label13.Font = new System.Drawing.Font("OCR A Extended", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label13.Location = new System.Drawing.Point(851, 49);
            this.label13.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label13.Name = "label13";
            this.label13.Size = new System.Drawing.Size(162, 25);
            this.label13.TabIndex = 42;
            this.label13.Text = "MODEL UYDU";
            // 
            // label12
            // 
            this.label12.AutoSize = true;
            this.label12.Font = new System.Drawing.Font("OCR A Extended", 21.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label12.Location = new System.Drawing.Point(868, 10);
            this.label12.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label12.Name = "label12";
            this.label12.Size = new System.Drawing.Size(247, 39);
            this.label12.TabIndex = 41;
            this.label12.Text = "NONGRAVITY";
            // 
            // textBox12
            // 
            this.textBox12.Location = new System.Drawing.Point(303, 737);
            this.textBox12.Margin = new System.Windows.Forms.Padding(4);
            this.textBox12.Name = "textBox12";
            this.textBox12.Size = new System.Drawing.Size(79, 22);
            this.textBox12.TabIndex = 25;
            // 
            // label11
            // 
            this.label11.AutoSize = true;
            this.label11.Location = new System.Drawing.Point(8, 21);
            this.label11.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(58, 16);
            this.label11.TabIndex = 40;
            this.label11.Text = "Seri Port";
            // 
            // manuelAyrilma
            // 
            this.manuelAyrilma.BackColor = System.Drawing.Color.Red;
            this.manuelAyrilma.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.manuelAyrilma.ForeColor = System.Drawing.SystemColors.ButtonFace;
            this.manuelAyrilma.Location = new System.Drawing.Point(529, 23);
            this.manuelAyrilma.Margin = new System.Windows.Forms.Padding(4);
            this.manuelAyrilma.Name = "manuelAyrilma";
            this.manuelAyrilma.Size = new System.Drawing.Size(132, 46);
            this.manuelAyrilma.TabIndex = 34;
            this.manuelAyrilma.Text = "Manuel Ayır";
            this.manuelAyrilma.UseVisualStyleBackColor = false;
            this.manuelAyrilma.Click += new System.EventHandler(this.manuelAyrilma_Click);
            // 
            // button4
            // 
            this.button4.Location = new System.Drawing.Point(421, 23);
            this.button4.Margin = new System.Windows.Forms.Padding(4);
            this.button4.Name = "button4";
            this.button4.Size = new System.Drawing.Size(100, 54);
            this.button4.TabIndex = 39;
            this.button4.Text = "veri göster";
            this.button4.UseVisualStyleBackColor = true;
            this.button4.Click += new System.EventHandler(this.button4_Click_1);
            // 
            // textBox7
            // 
            this.textBox7.Location = new System.Drawing.Point(128, 705);
            this.textBox7.Margin = new System.Windows.Forms.Padding(4);
            this.textBox7.Name = "textBox7";
            this.textBox7.Size = new System.Drawing.Size(76, 22);
            this.textBox7.TabIndex = 25;
            // 
            // textBox6
            // 
            this.textBox6.Location = new System.Drawing.Point(128, 674);
            this.textBox6.Margin = new System.Windows.Forms.Padding(4);
            this.textBox6.Name = "textBox6";
            this.textBox6.Size = new System.Drawing.Size(76, 22);
            this.textBox6.TabIndex = 24;
            this.textBox6.TextChanged += new System.EventHandler(this.textBox6_TextChanged);
            // 
            // groupBoxUyduDurum
            // 
            this.groupBoxUyduDurum.Controls.Add(this.labelUyduStatu);
            this.groupBoxUyduDurum.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(162)));
            this.groupBoxUyduDurum.Location = new System.Drawing.Point(668, 10);
            this.groupBoxUyduDurum.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.groupBoxUyduDurum.Name = "groupBoxUyduDurum";
            this.groupBoxUyduDurum.Padding = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.groupBoxUyduDurum.Size = new System.Drawing.Size(175, 65);
            this.groupBoxUyduDurum.TabIndex = 49;
            this.groupBoxUyduDurum.TabStop = false;
            this.groupBoxUyduDurum.Text = "Uydu Statüsü";
            // 
            // labelUyduStatu
            // 
            this.labelUyduStatu.AutoSize = true;
            this.labelUyduStatu.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(162)));
            this.labelUyduStatu.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(64)))), ((int)(((byte)(0)))));
            this.labelUyduStatu.Location = new System.Drawing.Point(11, 31);
            this.labelUyduStatu.Name = "labelUyduStatu";
            this.labelUyduStatu.Size = new System.Drawing.Size(142, 25);
            this.labelUyduStatu.TabIndex = 0;
            this.labelUyduStatu.Text = "BEKLEMEDE";
            // 
            // textBox15
            // 
            this.textBox15.Location = new System.Drawing.Point(128, 647);
            this.textBox15.Margin = new System.Windows.Forms.Padding(4);
            this.textBox15.Name = "textBox15";
            this.textBox15.Size = new System.Drawing.Size(76, 22);
            this.textBox15.TabIndex = 30;
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Location = new System.Drawing.Point(35, 650);
            this.label10.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(75, 16);
            this.label10.TabIndex = 29;
            this.label10.Text = "📋Paket No";
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(228, 768);
            this.label9.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(69, 16);
            this.label9.TabIndex = 28;
            this.label9.Text = "\t🌡️ Sıcaklık";
            // 
            // textBox14
            // 
            this.textBox14.Location = new System.Drawing.Point(529, 666);
            this.textBox14.Margin = new System.Windows.Forms.Padding(4);
            this.textBox14.Name = "textBox14";
            this.textBox14.Size = new System.Drawing.Size(73, 22);
            this.textBox14.TabIndex = 27;
            this.textBox14.TextChanged += new System.EventHandler(this.textBox14_TextChanged);
            // 
            // textBox13
            // 
            this.textBox13.Location = new System.Drawing.Point(529, 641);
            this.textBox13.Margin = new System.Windows.Forms.Padding(4);
            this.textBox13.Name = "textBox13";
            this.textBox13.Size = new System.Drawing.Size(73, 22);
            this.textBox13.TabIndex = 26;
            // 
            // textBox11
            // 
            this.textBox11.Location = new System.Drawing.Point(303, 708);
            this.textBox11.Margin = new System.Windows.Forms.Padding(4);
            this.textBox11.Name = "textBox11";
            this.textBox11.Size = new System.Drawing.Size(79, 22);
            this.textBox11.TabIndex = 24;
            this.textBox11.TextChanged += new System.EventHandler(this.textBox11_TextChanged);
            // 
            // textBox10
            // 
            this.textBox10.Location = new System.Drawing.Point(303, 680);
            this.textBox10.Margin = new System.Windows.Forms.Padding(4);
            this.textBox10.Name = "textBox10";
            this.textBox10.Size = new System.Drawing.Size(79, 22);
            this.textBox10.TabIndex = 23;
            this.textBox10.TextChanged += new System.EventHandler(this.textBox10_TextChanged);
            // 
            // textBox9
            // 
            this.textBox9.Location = new System.Drawing.Point(303, 650);
            this.textBox9.Margin = new System.Windows.Forms.Padding(4);
            this.textBox9.Name = "textBox9";
            this.textBox9.Size = new System.Drawing.Size(79, 22);
            this.textBox9.TabIndex = 22;
            // 
            // comboBox1
            // 
            this.comboBox1.FormattingEnabled = true;
            this.comboBox1.Location = new System.Drawing.Point(75, 18);
            this.comboBox1.Margin = new System.Windows.Forms.Padding(4);
            this.comboBox1.Name = "comboBox1";
            this.comboBox1.Size = new System.Drawing.Size(105, 24);
            this.comboBox1.TabIndex = 11;
            // 
            // dataGridView1
            // 
            this.dataGridView1.AllowUserToAddRows = false;
            this.dataGridView1.AllowUserToDeleteRows = false;
            this.dataGridView1.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.dataGridView1.BackgroundColor = System.Drawing.Color.White;
            this.dataGridView1.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridView1.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.Column17,
            this.Column19,
            this.Column20,
            this.Column7,
            this.Column2,
            this.Column21,
            this.Column3,
            this.Column14,
            this.Column22,
            this.Hız,
            this.Column1,
            this.Column11,
            this.Column12,
            this.Column13,
            this.Column24,
            this.Pitch,
            this.Column15,
            this.Column16,
            this.Column23,
            this.Column25,
            this.Column26,
            this.Column10});
            this.dataGridView1.GridColor = System.Drawing.SystemColors.ActiveCaption;
            this.dataGridView1.Location = new System.Drawing.Point(0, 832);
            this.dataGridView1.Margin = new System.Windows.Forms.Padding(4);
            this.dataGridView1.Name = "dataGridView1";
            this.dataGridView1.ReadOnly = true;
            this.dataGridView1.RowHeadersWidth = 51;
            this.dataGridView1.Size = new System.Drawing.Size(1255, 217);
            this.dataGridView1.TabIndex = 0;
            this.dataGridView1.CellContentClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dataGridView1_CellContentClick);
            // 
            // Column17
            // 
            this.Column17.HeaderText = "Paket No";
            this.Column17.MinimumWidth = 6;
            this.Column17.Name = "Column17";
            this.Column17.ReadOnly = true;
            // 
            // Column19
            // 
            this.Column19.HeaderText = "Uydu Statüsü";
            this.Column19.MinimumWidth = 6;
            this.Column19.Name = "Column19";
            this.Column19.ReadOnly = true;
            // 
            // Column20
            // 
            this.Column20.HeaderText = "Hata Kodu";
            this.Column20.MinimumWidth = 6;
            this.Column20.Name = "Column20";
            this.Column20.ReadOnly = true;
            // 
            // Column7
            // 
            this.Column7.HeaderText = "Gönderme Saati";
            this.Column7.MinimumWidth = 6;
            this.Column7.Name = "Column7";
            this.Column7.ReadOnly = true;
            // 
            // Column2
            // 
            this.Column2.HeaderText = "Basınç1";
            this.Column2.MinimumWidth = 6;
            this.Column2.Name = "Column2";
            this.Column2.ReadOnly = true;
            // 
            // Column21
            // 
            this.Column21.HeaderText = "Basınç2";
            this.Column21.MinimumWidth = 6;
            this.Column21.Name = "Column21";
            this.Column21.ReadOnly = true;
            // 
            // Column3
            // 
            this.Column3.FillWeight = 115F;
            this.Column3.HeaderText = "Yükseklik1";
            this.Column3.MinimumWidth = 6;
            this.Column3.Name = "Column3";
            this.Column3.ReadOnly = true;
            // 
            // Column14
            // 
            this.Column14.HeaderText = "Yükseklik2";
            this.Column14.MinimumWidth = 6;
            this.Column14.Name = "Column14";
            this.Column14.ReadOnly = true;
            // 
            // Column22
            // 
            this.Column22.HeaderText = "İrtifa Farkı";
            this.Column22.MinimumWidth = 6;
            this.Column22.Name = "Column22";
            this.Column22.ReadOnly = true;
            // 
            // Hız
            // 
            this.Hız.HeaderText = "İniş Hızı";
            this.Hız.MinimumWidth = 6;
            this.Hız.Name = "Hız";
            this.Hız.ReadOnly = true;
            // 
            // Column1
            // 
            this.Column1.FillWeight = 110F;
            this.Column1.HeaderText = "Sıcaklık";
            this.Column1.MinimumWidth = 6;
            this.Column1.Name = "Column1";
            this.Column1.ReadOnly = true;
            // 
            // Column11
            // 
            this.Column11.HeaderText = "Pil Gerilimi";
            this.Column11.MinimumWidth = 6;
            this.Column11.Name = "Column11";
            this.Column11.ReadOnly = true;
            // 
            // Column12
            // 
            this.Column12.HeaderText = "GPS1 Latitude";
            this.Column12.MinimumWidth = 6;
            this.Column12.Name = "Column12";
            this.Column12.ReadOnly = true;
            // 
            // Column13
            // 
            this.Column13.HeaderText = "GPS1 Longitude";
            this.Column13.MinimumWidth = 6;
            this.Column13.Name = "Column13";
            this.Column13.ReadOnly = true;
            // 
            // Column24
            // 
            this.Column24.HeaderText = "GPS1 Altitude";
            this.Column24.MinimumWidth = 6;
            this.Column24.Name = "Column24";
            this.Column24.ReadOnly = true;
            // 
            // Pitch
            // 
            this.Pitch.HeaderText = "Pitch";
            this.Pitch.MinimumWidth = 6;
            this.Pitch.Name = "Pitch";
            this.Pitch.ReadOnly = true;
            // 
            // Column15
            // 
            this.Column15.HeaderText = "Roll";
            this.Column15.MinimumWidth = 6;
            this.Column15.Name = "Column15";
            this.Column15.ReadOnly = true;
            // 
            // Column16
            // 
            this.Column16.HeaderText = "Yaw";
            this.Column16.MinimumWidth = 6;
            this.Column16.Name = "Column16";
            this.Column16.ReadOnly = true;
            // 
            // Column23
            // 
            this.Column23.HeaderText = "RHRH";
            this.Column23.MinimumWidth = 6;
            this.Column23.Name = "Column23";
            this.Column23.ReadOnly = true;
            // 
            // Column25
            // 
            this.Column25.HeaderText = "IoT S1 Data";
            this.Column25.MinimumWidth = 6;
            this.Column25.Name = "Column25";
            this.Column25.ReadOnly = true;
            // 
            // Column26
            // 
            this.Column26.HeaderText = "IoT S2 Data";
            this.Column26.MinimumWidth = 6;
            this.Column26.Name = "Column26";
            this.Column26.ReadOnly = true;
            // 
            // Column10
            // 
            this.Column10.HeaderText = "Takım No";
            this.Column10.MinimumWidth = 6;
            this.Column10.Name = "Column10";
            this.Column10.ReadOnly = true;
            // 
            // baglan
            // 
            this.baglan.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(255)))), ((int)(((byte)(192)))));
            this.baglan.Location = new System.Drawing.Point(205, 18);
            this.baglan.Margin = new System.Windows.Forms.Padding(4);
            this.baglan.Name = "baglan";
            this.baglan.Size = new System.Drawing.Size(100, 54);
            this.baglan.TabIndex = 1;
            this.baglan.Text = "bağlan";
            this.baglan.UseVisualStyleBackColor = false;
            this.baglan.Click += new System.EventHandler(this.button1_Click);
            // 
            // texBaundRate
            // 
            this.texBaundRate.Location = new System.Drawing.Point(136, 50);
            this.texBaundRate.Margin = new System.Windows.Forms.Padding(4);
            this.texBaundRate.Name = "texBaundRate";
            this.texBaundRate.Size = new System.Drawing.Size(71, 22);
            this.texBaundRate.TabIndex = 21;
            // 
            // kes
            // 
            this.kes.BackColor = System.Drawing.Color.LightCoral;
            this.kes.Location = new System.Drawing.Point(313, 21);
            this.kes.Margin = new System.Windows.Forms.Padding(4);
            this.kes.Name = "kes";
            this.kes.Size = new System.Drawing.Size(100, 54);
            this.kes.TabIndex = 2;
            this.kes.Text = "kes";
            this.kes.UseVisualStyleBackColor = false;
            this.kes.Click += new System.EventHandler(this.kes_Click);
            // 
            // chartYukseklik
            // 
            chartArea4.Name = "ChartAreaYukseklik";
            this.chartYukseklik.ChartAreas.Add(chartArea4);
            this.chartYukseklik.Location = new System.Drawing.Point(497, 82);
            this.chartYukseklik.Margin = new System.Windows.Forms.Padding(4);
            this.chartYukseklik.Name = "chartYukseklik";
            series4.ChartArea = "ChartAreaYukseklik";
            series4.ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Line;
            series4.Name = "Yükseklik(m)";
            this.chartYukseklik.Series.Add(series4);
            this.chartYukseklik.Size = new System.Drawing.Size(247, 385);
            this.chartYukseklik.TabIndex = 13;
            this.chartYukseklik.Text = "chartYukseklik";
            title4.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(162)));
            title4.Name = "Title1";
            title4.Text = "Yükseklik(m/saniye)";
            this.chartYukseklik.Titles.Add(title4);
            // 
            // chartSicaklik
            // 
            chartArea5.Name = "ChartAreaSicaklik";
            this.chartSicaklik.ChartAreas.Add(chartArea5);
            this.chartSicaklik.Location = new System.Drawing.Point(248, 82);
            this.chartSicaklik.Margin = new System.Windows.Forms.Padding(4);
            this.chartSicaklik.Name = "chartSicaklik";
            this.chartSicaklik.Palette = System.Windows.Forms.DataVisualization.Charting.ChartColorPalette.Fire;
            series5.ChartArea = "ChartAreaSicaklik";
            series5.ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Spline;
            series5.Font = new System.Drawing.Font("Microsoft Sans Serif", 6F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(162)));
            series5.Name = "Sıcaklık(°C)";
            this.chartSicaklik.Series.Add(series5);
            this.chartSicaklik.Size = new System.Drawing.Size(241, 385);
            this.chartSicaklik.TabIndex = 13;
            this.chartSicaklik.Text = "chartSicaklik";
            title5.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(162)));
            title5.Name = "Title1";
            title5.Text = "Sıcaklık(°C/saniye)";
            this.chartSicaklik.Titles.Add(title5);
            // 
            // chartBasinc
            // 
            chartArea6.InnerPlotPosition.Auto = false;
            chartArea6.InnerPlotPosition.Height = 80.17548F;
            chartArea6.InnerPlotPosition.Width = 77.5357F;
            chartArea6.InnerPlotPosition.X = 19.11325F;
            chartArea6.InnerPlotPosition.Y = 5.63645F;
            chartArea6.Name = "ChartAreaBasinc";
            chartArea6.Position.Auto = false;
            chartArea6.Position.Height = 83.82925F;
            chartArea6.Position.Width = 94F;
            chartArea6.Position.X = 3F;
            chartArea6.Position.Y = 13.17074F;
            this.chartBasinc.ChartAreas.Add(chartArea6);
            this.chartBasinc.Location = new System.Drawing.Point(752, 80);
            this.chartBasinc.Margin = new System.Windows.Forms.Padding(4);
            this.chartBasinc.Name = "chartBasinc";
            series6.ChartArea = "ChartAreaBasinc";
            series6.ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Line;
            series6.Name = "Basınç(Pascal)";
            this.chartBasinc.Series.Add(series6);
            this.chartBasinc.Size = new System.Drawing.Size(273, 386);
            this.chartBasinc.TabIndex = 37;
            this.chartBasinc.Text = "chartBasinc";
            title6.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(162)));
            title6.Name = "Title1";
            title6.Text = "Basınç(Pascal)";
            this.chartBasinc.Titles.Add(title6);
            // 
            // chartHiz
            // 
            chartArea7.Name = "ChartAreaHiz";
            this.chartHiz.ChartAreas.Add(chartArea7);
            this.chartHiz.Location = new System.Drawing.Point(8, 82);
            this.chartHiz.Margin = new System.Windows.Forms.Padding(4);
            this.chartHiz.Name = "chartHiz";
            series7.ChartArea = "ChartAreaHiz";
            series7.ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Line;
            series7.Name = "Hız(m/s)";
            this.chartHiz.Series.Add(series7);
            this.chartHiz.Size = new System.Drawing.Size(232, 383);
            this.chartHiz.TabIndex = 38;
            this.chartHiz.Text = "chartHiz";
            title7.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(162)));
            title7.Name = "TitleHiz";
            title7.Text = "Hız(m/s)";
            this.chartHiz.Titles.Add(title7);
            // 
            // chartPilGerilimi
            // 
            chartArea8.InnerPlotPosition.Auto = false;
            chartArea8.InnerPlotPosition.Height = 80.17548F;
            chartArea8.InnerPlotPosition.Width = 77.5434F;
            chartArea8.InnerPlotPosition.X = 19.10554F;
            chartArea8.InnerPlotPosition.Y = 5.63645F;
            chartArea8.Name = "ChartAreaPilGerilimi";
            chartArea8.Position.Auto = false;
            chartArea8.Position.Height = 83.82925F;
            chartArea8.Position.Width = 94F;
            chartArea8.Position.X = 3F;
            chartArea8.Position.Y = 13.17074F;
            this.chartPilGerilimi.ChartAreas.Add(chartArea8);
            this.chartPilGerilimi.Location = new System.Drawing.Point(1033, 82);
            this.chartPilGerilimi.Margin = new System.Windows.Forms.Padding(4);
            this.chartPilGerilimi.Name = "chartPilGerilimi";
            series8.ChartArea = "ChartAreaPilGerilimi";
            series8.ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Line;
            series8.Name = "Pil Gerilimi";
            this.chartPilGerilimi.Series.Add(series8);
            this.chartPilGerilimi.Size = new System.Drawing.Size(259, 383);
            this.chartPilGerilimi.TabIndex = 43;
            this.chartPilGerilimi.Text = "chartPilGerilimi";
            title8.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(162)));
            title8.Name = "Title1";
            title8.Text = "Pil Gerilimi(V/saniye)";
            this.chartPilGerilimi.Titles.Add(title8);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(8, 50);
            this.label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(62, 16);
            this.label1.TabIndex = 4;
            this.label1.Text = "Baudrate";
            // 
            // textBox5
            // 
            this.textBox5.Location = new System.Drawing.Point(128, 796);
            this.textBox5.Margin = new System.Windows.Forms.Padding(4);
            this.textBox5.Name = "textBox5";
            this.textBox5.Size = new System.Drawing.Size(76, 22);
            this.textBox5.TabIndex = 10;
            // 
            // textBox4
            // 
            this.textBox4.Location = new System.Drawing.Point(128, 767);
            this.textBox4.Margin = new System.Windows.Forms.Padding(4);
            this.textBox4.Name = "textBox4";
            this.textBox4.Size = new System.Drawing.Size(76, 22);
            this.textBox4.TabIndex = 9;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.Silver;
            this.ClientSize = new System.Drawing.Size(1924, 1055);
            this.Controls.Add(this.groupBoxKalibrasyon);
            this.Controls.Add(this.groupBoxARAS);
            this.Controls.Add(this.groupBox3);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.videoSourcePlayer);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(4);
            this.Name = "Form1";
            this.Text = "NONGRAVITY Multi-Spektral Mekanik Filtreleme Modülü";
            this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Form1_FormClosing);
            this.Load += new System.EventHandler(this.Form1_Load);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.groupBoxMultiSpektral.ResumeLayout(false);
            this.groupBoxMultiSpektral.PerformLayout();
            this.groupBoxARAS.ResumeLayout(false);
            this.groupBoxARAS.PerformLayout();
            this.tableLayoutPanelAlarm.ResumeLayout(false);
            this.tableLayoutPanelAlarm.PerformLayout();
            this.groupBoxKalibrasyon.ResumeLayout(false);
            this.groupBoxKalibrasyon.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.chart3)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.chart4)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.chart5)).EndInit();
            this.groupBox3.ResumeLayout(false);
            this.groupBox3.PerformLayout();
            this.groupBoxIoT.ResumeLayout(false);
            this.groupBoxIoT.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.chartIoT)).EndInit();
            this.groupBoxUyduDurum.ResumeLayout(false);
            this.groupBoxUyduDurum.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.chartYukseklik)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.chartSicaklik)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.chartBasinc)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.chartHiz)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.chartPilGerilimi)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.TextBox textBox1;
        private System.Windows.Forms.TextBox textBox2;
        private System.Windows.Forms.TextBox textBox3;
        private System.IO.Ports.SerialPort OkumaNesnesi;
        private OpenTK.GLControl glControl1;
        private System.Windows.Forms.Timer zamanlayici;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.Button button3;
        private GMap.NET.WindowsForms.GMapControl gMapControl1;
        private System.Windows.Forms.Button Bul;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Button Yakınlaştır;
        private System.Windows.Forms.Button Uzaklaştır;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.IO.Ports.SerialPort serialPort1;
        private AForge.Controls.VideoSourcePlayer videoSourcePlayer;
        private System.Windows.Forms.GroupBox groupBoxMultiSpektral;
        private System.Windows.Forms.TextBox textBoxMultiSpektralKomut;
        private System.Windows.Forms.Button buttonMultiSpektralGonder;
        private System.Windows.Forms.Label labelMultiSpektralKomut;
        private System.Windows.Forms.Label labelMultiSpektralAciklama;
        private System.Windows.Forms.GroupBox groupBoxARAS;
        private System.Windows.Forms.Label labelHataKodu;
        private System.Windows.Forms.Label labelHataKoduBaslik;
        private System.Windows.Forms.Panel panelHata1;
        private System.Windows.Forms.Panel panelHata2;
        private System.Windows.Forms.Panel panelHata3;
        private System.Windows.Forms.Panel panelHata4;
        private System.Windows.Forms.Panel panelHata5;
        private System.Windows.Forms.Panel panelHata6;
        private System.Windows.Forms.Label labelHata1;
        private System.Windows.Forms.Label labelHata2;
        private System.Windows.Forms.Label labelHata3;
        private System.Windows.Forms.Label labelHata4;
        private System.Windows.Forms.Label labelHata5;
        private System.Windows.Forms.Label labelHata6;
        private System.Windows.Forms.CheckBox checkBoxSesliUyari;
        private System.Windows.Forms.DataVisualization.Charting.Chart chart2;
        private System.Windows.Forms.Label labelMultiSpektralDurum;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanelAlarm;
        private System.Windows.Forms.Panel panelAlarm1;
        private System.Windows.Forms.Panel panelAlarm2;
        private System.Windows.Forms.Panel panelAlarm3;
        private System.Windows.Forms.Panel panelAlarm4;
        private System.Windows.Forms.Panel panelAlarm5;
        private System.Windows.Forms.Panel panelAlarm6;
        private System.Windows.Forms.Label label14;
        private System.Windows.Forms.Label label15;
        private System.Windows.Forms.Label label16;
        private System.Windows.Forms.Label label17;
        private System.Windows.Forms.Label label18;
        private System.Windows.Forms.Label label19;
        
        // 🔧 Kalibrasyon Sistemi Kontrolleri
        private System.Windows.Forms.GroupBox groupBoxKalibrasyon;
        private System.Windows.Forms.ComboBox comboBoxSehir;
        private System.Windows.Forms.Button buttonBasincKalibre;
        private System.Windows.Forms.Button buttonGPSReferans;
        private System.Windows.Forms.Button buttonGyroSifirla;
        private System.Windows.Forms.Label labelKalibrasyonDurum;
        private System.Windows.Forms.Label labelSehirSecimi;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.DataVisualization.Charting.Chart chart3;
        private System.Windows.Forms.DataVisualization.Charting.Chart chart4;
        private System.Windows.Forms.DataVisualization.Charting.Chart chart5;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.Button ExlAktar;
        private System.Windows.Forms.Label labelTakimNumarasi;
        private System.Windows.Forms.Label label36;
        private System.Windows.Forms.Label label35;
        private System.Windows.Forms.Label label34;
        private System.Windows.Forms.Label label33;
        private System.Windows.Forms.Label label32;
        private System.Windows.Forms.Label label26;
        private System.Windows.Forms.Label label20;
        private System.Windows.Forms.TextBox textBox22;
        private System.Windows.Forms.TextBox textBox21;
        private System.Windows.Forms.TextBox textBox20;
        private System.Windows.Forms.TextBox textBox19;
        private System.Windows.Forms.TextBox textBox18;
        private System.Windows.Forms.TextBox textBox17;
        private System.Windows.Forms.TextBox textBox16;
        private System.Windows.Forms.Label label31;
        private System.Windows.Forms.Label label30;
        private System.Windows.Forms.Label label29;
        private System.Windows.Forms.Label label28;
        private System.Windows.Forms.Label label27;
        private System.Windows.Forms.Label label25;
        private System.Windows.Forms.Label label24;
        private System.Windows.Forms.Label label23;
        private System.Windows.Forms.Label label22;
        private System.Windows.Forms.Label label21;
        private System.Windows.Forms.GroupBox groupBoxIoT;
        private System.Windows.Forms.Button buttonIotGoruntule;
        private System.Windows.Forms.Label labelIoTS1;
        private System.Windows.Forms.Label labelIoTS2;
        private System.Windows.Forms.Label labelIoTS1Deger;
        private System.Windows.Forms.Label labelIoTS2Deger;
        private System.Windows.Forms.DataVisualization.Charting.Chart chartIoT;
        private System.Windows.Forms.TextBox textBox8;
        private System.Windows.Forms.Label label13;
        private System.Windows.Forms.Label label12;
        private System.Windows.Forms.TextBox textBox12;
        private System.Windows.Forms.Label label11;
        private System.Windows.Forms.Button manuelAyrilma;
        private System.Windows.Forms.Button button4;
        private System.Windows.Forms.TextBox textBox7;
        private System.Windows.Forms.TextBox textBox6;
        private System.Windows.Forms.GroupBox groupBoxUyduDurum;
        private System.Windows.Forms.Label labelUyduStatu;
        private System.Windows.Forms.TextBox textBox15;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.TextBox textBox14;
        private System.Windows.Forms.TextBox textBox13;
        private System.Windows.Forms.TextBox textBox11;
        private System.Windows.Forms.TextBox textBox10;
        private System.Windows.Forms.TextBox textBox9;
        private System.Windows.Forms.ComboBox comboBox1;
        private System.Windows.Forms.DataGridView dataGridView1;
        private System.Windows.Forms.DataGridViewTextBoxColumn Column17;
        private System.Windows.Forms.DataGridViewTextBoxColumn Column19;
        private System.Windows.Forms.DataGridViewTextBoxColumn Column20;
        private System.Windows.Forms.DataGridViewTextBoxColumn Column7;
        private System.Windows.Forms.DataGridViewTextBoxColumn Column2;
        private System.Windows.Forms.DataGridViewTextBoxColumn Column21;
        private System.Windows.Forms.DataGridViewTextBoxColumn Column3;
        private System.Windows.Forms.DataGridViewTextBoxColumn Column14;
        private System.Windows.Forms.DataGridViewTextBoxColumn Column22;
        private System.Windows.Forms.DataGridViewTextBoxColumn Hız;
        private System.Windows.Forms.DataGridViewTextBoxColumn Column1;
        private System.Windows.Forms.DataGridViewTextBoxColumn Column11;
        private System.Windows.Forms.DataGridViewTextBoxColumn Column12;
        private System.Windows.Forms.DataGridViewTextBoxColumn Column13;
        private System.Windows.Forms.DataGridViewTextBoxColumn Column24;
        private System.Windows.Forms.DataGridViewTextBoxColumn Pitch;
        private System.Windows.Forms.DataGridViewTextBoxColumn Column15;
        private System.Windows.Forms.DataGridViewTextBoxColumn Column16;
        private System.Windows.Forms.DataGridViewTextBoxColumn Column23;
        private System.Windows.Forms.DataGridViewTextBoxColumn Column25;
        private System.Windows.Forms.DataGridViewTextBoxColumn Column26;
        private System.Windows.Forms.DataGridViewTextBoxColumn Column10;
        private System.Windows.Forms.Button baglan;
        private System.Windows.Forms.TextBox texBaundRate;
        private System.Windows.Forms.Button kes;
        private System.Windows.Forms.DataVisualization.Charting.Chart chartYukseklik;
        private System.Windows.Forms.DataVisualization.Charting.Chart chartSicaklik;
        private System.Windows.Forms.DataVisualization.Charting.Chart chartBasinc;
        private System.Windows.Forms.DataVisualization.Charting.Chart chartHiz;
        private System.Windows.Forms.DataVisualization.Charting.Chart chartPilGerilimi;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox textBox5;
        private System.Windows.Forms.TextBox textBox4;
    }
}

