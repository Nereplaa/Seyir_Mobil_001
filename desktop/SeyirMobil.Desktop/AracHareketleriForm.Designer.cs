namespace SeyirMobil.Desktop;

partial class AracHareketleriForm
{
    private System.ComponentModel.IContainer components = null;

    protected override void Dispose(bool disposing)
    {
        if (disposing && (components != null))
        {
            components.Dispose();
        }
        base.Dispose(disposing);
    }

    private System.Windows.Forms.TableLayoutPanel tableRoot;
    private System.Windows.Forms.FlowLayoutPanel flowUst;

    private System.Windows.Forms.FlowLayoutPanel groupPlaka;
    private System.Windows.Forms.Label lblPlaka;
    private System.Windows.Forms.ComboBox cmbPlaka;

    private System.Windows.Forms.FlowLayoutPanel groupTarih;
    private System.Windows.Forms.Label lblTarih;
    private System.Windows.Forms.DateTimePicker dtpTarih;
    private System.Windows.Forms.Button btnTarihOnayla;

    private System.Windows.Forms.FlowLayoutPanel groupHiz;
    private System.Windows.Forms.Label lblHiz;
    private System.Windows.Forms.NumericUpDown nudHiz;

    private System.Windows.Forms.FlowLayoutPanel groupKm;
    private System.Windows.Forms.Label lblKm;
    private System.Windows.Forms.NumericUpDown nudKm;
    private System.Windows.Forms.Label lblSinirBilgisi;

    private System.Windows.Forms.Button btnEkle;
    private System.Windows.Forms.Button btnSil;
    private System.Windows.Forms.Button btnYenile;
    private System.Windows.Forms.Button btnHareketRaporu;
    private System.Windows.Forms.Button btnExcelAktar;
    private System.Windows.Forms.Button btnExcelIceAktar;
    private System.Windows.Forms.Button btnCikisYap;

    private System.Windows.Forms.FlowLayoutPanel flowFiltre;
    private System.Windows.Forms.Label lblFiltreBaslik;
    private System.Windows.Forms.ComboBox cmbFiltrePlaka;
    private System.Windows.Forms.CheckBox chkFiltreTarih;
    private System.Windows.Forms.DateTimePicker dtpFiltreTarih;
    private System.Windows.Forms.Label lblFiltreHiz;
    private System.Windows.Forms.TextBox txtFiltreHiz;
    private System.Windows.Forms.Label lblFiltreKm;
    private System.Windows.Forms.TextBox txtFiltreKm;
    private System.Windows.Forms.Button btnFiltreUygula;
    private System.Windows.Forms.Button btnFiltreTemizle;

    private System.Windows.Forms.DataGridView dgvHareketler;

    private System.Windows.Forms.FlowLayoutPanel flowSayfalama;
    private System.Windows.Forms.Label lblSayfaBoyutu;
    private System.Windows.Forms.ComboBox cmbSayfaBoyutu;
    private System.Windows.Forms.Button btnOncekiSayfa;
    private System.Windows.Forms.Label lblSayfaGostergesi;
    private System.Windows.Forms.Button btnSonrakiSayfa;

    private System.Windows.Forms.Label lblStatus;

    private void InitializeComponent()
    {
        this.tableRoot = new System.Windows.Forms.TableLayoutPanel();
        this.flowUst = new System.Windows.Forms.FlowLayoutPanel();
        this.groupPlaka = new System.Windows.Forms.FlowLayoutPanel();
        this.lblPlaka = new System.Windows.Forms.Label();
        this.cmbPlaka = new System.Windows.Forms.ComboBox();
        this.groupTarih = new System.Windows.Forms.FlowLayoutPanel();
        this.lblTarih = new System.Windows.Forms.Label();
        this.dtpTarih = new System.Windows.Forms.DateTimePicker();
        this.btnTarihOnayla = new System.Windows.Forms.Button();
        this.groupHiz = new System.Windows.Forms.FlowLayoutPanel();
        this.lblHiz = new System.Windows.Forms.Label();
        this.nudHiz = new System.Windows.Forms.NumericUpDown();
        this.groupKm = new System.Windows.Forms.FlowLayoutPanel();
        this.lblKm = new System.Windows.Forms.Label();
        this.nudKm = new System.Windows.Forms.NumericUpDown();
        this.lblSinirBilgisi = new System.Windows.Forms.Label();
        this.btnEkle = new System.Windows.Forms.Button();
        this.btnSil = new System.Windows.Forms.Button();
        this.btnYenile = new System.Windows.Forms.Button();
        this.btnHareketRaporu = new System.Windows.Forms.Button();
        this.btnExcelAktar = new System.Windows.Forms.Button();
        this.btnExcelIceAktar = new System.Windows.Forms.Button();
        this.btnCikisYap = new System.Windows.Forms.Button();
        this.flowFiltre = new System.Windows.Forms.FlowLayoutPanel();
        this.lblFiltreBaslik = new System.Windows.Forms.Label();
        this.cmbFiltrePlaka = new System.Windows.Forms.ComboBox();
        this.chkFiltreTarih = new System.Windows.Forms.CheckBox();
        this.dtpFiltreTarih = new System.Windows.Forms.DateTimePicker();
        this.lblFiltreHiz = new System.Windows.Forms.Label();
        this.txtFiltreHiz = new System.Windows.Forms.TextBox();
        this.lblFiltreKm = new System.Windows.Forms.Label();
        this.txtFiltreKm = new System.Windows.Forms.TextBox();
        this.btnFiltreUygula = new System.Windows.Forms.Button();
        this.btnFiltreTemizle = new System.Windows.Forms.Button();
        this.dgvHareketler = new System.Windows.Forms.DataGridView();
        this.flowSayfalama = new System.Windows.Forms.FlowLayoutPanel();
        this.lblSayfaBoyutu = new System.Windows.Forms.Label();
        this.cmbSayfaBoyutu = new System.Windows.Forms.ComboBox();
        this.btnOncekiSayfa = new System.Windows.Forms.Button();
        this.lblSayfaGostergesi = new System.Windows.Forms.Label();
        this.btnSonrakiSayfa = new System.Windows.Forms.Button();
        this.lblStatus = new System.Windows.Forms.Label();
        this.tableRoot.SuspendLayout();
        this.flowUst.SuspendLayout();
        this.groupPlaka.SuspendLayout();
        this.groupTarih.SuspendLayout();
        this.groupHiz.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)(this.nudHiz)).BeginInit();
        this.groupKm.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)(this.nudKm)).BeginInit();
        this.flowFiltre.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)(this.dgvHareketler)).BeginInit();
        this.flowSayfalama.SuspendLayout();
        this.SuspendLayout();
        //
        // tableRoot -- ust (degisken boy) sihirbaz + alt (kalani doldur) grid icin 2 satirli
        // duzen. FlowLayoutPanel(Dock=Top) + DataGridView(Dock=Fill) kardes kontrol kombinasyonu
        // calisma zamaninda (gruplar acilip kapaninca) grid'in yeniden hizalanmasini guvenilir
        // yapmiyordu (basliklar/ust satirlar eski konumda "takili" kalabiliyordu) - TableLayoutPanel
        // bu senaryo icin WinForms'un onerilen, daha guvenilir cozumu.
        //
        this.tableRoot.ColumnCount = 1;
        this.tableRoot.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
        this.tableRoot.Dock = System.Windows.Forms.DockStyle.Fill;
        // NOT (2026-08-04, 3. tur duzeltme): Onceki iki tur (form boyutu buyutme, DoubleBuffered,
        // flowSayfalama'yi Absolute yukseklik yapma) sorunu TAM cozemedi - cunku kok neden hicbiri
        // degildi: lblStatus, tableRoot'un DISINDA, Form'a ayrica Dock=Bottom olarak ekleniyordu.
        // IKI BAGIMSIZ layout motoru (Form'un kendi Dock cozumlemesi + tableRoot'un kendi
        // satir/hucre hesaplamasi) ayni dikey alani PAYLASMAYA calisiyordu - ikisi senkron
        // olmayinca (ozellikle tableRoot'a yeni bir Absolute satir eklenince) lblStatus'un
        // rezerve ettigi alanla tableRoot'un SON satirinin (flowSayfalama) hesapladigi alan
        // CAKISIYORDU (lblStatus, flowSayfalama'nin ALTINDA/ARKASINDA kalip goze
        // gorunmuyordu/ustune biniliyordu). KESIN COZUM: lblStatus da tableRoot'un kendi
        // SON satiri yapildi - artik TEK bir layout motoru (tableRoot) formun TUM dikey
        // yerlesimine karar veriyor, iki ayri Dock hesaplamasi arasinda cakisma imkani kalmadi.
        this.tableRoot.RowCount = 5;
        this.tableRoot.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
        this.tableRoot.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
        this.tableRoot.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
        this.tableRoot.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 46F));
        this.tableRoot.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 28F));
        this.tableRoot.Controls.Add(this.flowUst, 0, 0);
        this.tableRoot.Controls.Add(this.flowFiltre, 0, 1);
        this.tableRoot.Controls.Add(this.dgvHareketler, 0, 2);
        this.tableRoot.Controls.Add(this.flowSayfalama, 0, 3);
        this.tableRoot.Controls.Add(this.lblStatus, 0, 4);
        this.tableRoot.Name = "tableRoot";
        //
        // flowUst  -- pencere daralinca gruplar/butonlar alt satira kayar (responsive)
        //
        this.flowUst.AutoSize = true;
        this.flowUst.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
        this.flowUst.Dock = System.Windows.Forms.DockStyle.Fill;
        this.flowUst.FlowDirection = System.Windows.Forms.FlowDirection.LeftToRight;
        this.flowUst.WrapContents = true;
        this.flowUst.Padding = new System.Windows.Forms.Padding(10);
        this.flowUst.Controls.Add(this.groupPlaka);
        this.flowUst.Controls.Add(this.groupTarih);
        this.flowUst.Controls.Add(this.groupHiz);
        this.flowUst.Controls.Add(this.groupKm);
        this.flowUst.Controls.Add(this.btnEkle);
        this.flowUst.Controls.Add(this.btnSil);
        this.flowUst.Controls.Add(this.btnYenile);
        this.flowUst.Controls.Add(this.btnHareketRaporu);
        this.flowUst.Controls.Add(this.btnExcelAktar);
        this.flowUst.Controls.Add(this.btnExcelIceAktar);
        this.flowUst.Controls.Add(this.btnCikisYap);
        this.flowUst.Name = "flowUst";
        this.flowUst.TabIndex = 0;
        //
        // groupPlaka  -- Adim 1: plaka secimi (mevcut araclardan, lookup)
        //
        this.groupPlaka.AutoSize = true;
        this.groupPlaka.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
        this.groupPlaka.Margin = new System.Windows.Forms.Padding(3, 3, 20, 3);
        this.groupPlaka.Controls.Add(this.lblPlaka);
        this.groupPlaka.Controls.Add(this.cmbPlaka);
        this.groupPlaka.Name = "groupPlaka";
        this.groupPlaka.TabIndex = 0;
        //
        // lblPlaka
        //
        this.lblPlaka.AutoSize = true;
        this.lblPlaka.Margin = new System.Windows.Forms.Padding(0, 0, 0, 4);
        this.lblPlaka.Name = "lblPlaka";
        this.lblPlaka.Text = "1) Plaka:";
        //
        // cmbPlaka
        //
        this.cmbPlaka.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
        this.cmbPlaka.Margin = new System.Windows.Forms.Padding(0);
        this.cmbPlaka.Name = "cmbPlaka";
        this.cmbPlaka.Size = new System.Drawing.Size(160, 23);
        this.cmbPlaka.TabIndex = 0;
        this.cmbPlaka.SelectedIndexChanged += new System.EventHandler(this.cmbPlaka_SelectedIndexChanged);
        //
        // groupTarih  -- Adim 2: veri tarihi (varsayilan bugun, degistirilebilir)
        //
        this.groupTarih.AutoSize = true;
        this.groupTarih.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
        this.groupTarih.Margin = new System.Windows.Forms.Padding(3, 3, 20, 3);
        this.groupTarih.Visible = false;
        this.groupTarih.Controls.Add(this.lblTarih);
        this.groupTarih.Controls.Add(this.dtpTarih);
        this.groupTarih.Controls.Add(this.btnTarihOnayla);
        this.groupTarih.Name = "groupTarih";
        this.groupTarih.TabIndex = 1;
        //
        // lblTarih
        //
        this.lblTarih.AutoSize = true;
        this.lblTarih.Margin = new System.Windows.Forms.Padding(0, 0, 0, 4);
        this.lblTarih.Name = "lblTarih";
        this.lblTarih.Text = "2) Veri Tarihi:";
        //
        // dtpTarih
        //
        this.dtpTarih.Format = System.Windows.Forms.DateTimePickerFormat.Short;
        this.dtpTarih.Margin = new System.Windows.Forms.Padding(0, 0, 0, 6);
        this.dtpTarih.Name = "dtpTarih";
        this.dtpTarih.Size = new System.Drawing.Size(160, 23);
        this.dtpTarih.TabIndex = 0;
        this.dtpTarih.ValueChanged += new System.EventHandler(this.dtpTarih_ValueChanged);
        //
        // btnTarihOnayla  -- tarih kesinlesmeden (bu butona basilmadan) sonraki adimlar acilmaz
        //
        this.btnTarihOnayla.Margin = new System.Windows.Forms.Padding(0);
        this.btnTarihOnayla.Name = "btnTarihOnayla";
        this.btnTarihOnayla.Size = new System.Drawing.Size(160, 27);
        this.btnTarihOnayla.TabIndex = 1;
        this.btnTarihOnayla.Text = "Tarihi Onayla →";
        this.btnTarihOnayla.UseVisualStyleBackColor = true;
        this.btnTarihOnayla.Click += new System.EventHandler(this.btnTarihOnayla_Click);
        //
        // groupHiz  -- Adim 3: hiz (NumericUpDown kendi sinirini zaten uyguluyor -> "dogrulanmis")
        //
        this.groupHiz.AutoSize = true;
        this.groupHiz.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
        this.groupHiz.Margin = new System.Windows.Forms.Padding(3, 3, 20, 3);
        this.groupHiz.Visible = false;
        this.groupHiz.Controls.Add(this.lblHiz);
        this.groupHiz.Controls.Add(this.nudHiz);
        this.groupHiz.Name = "groupHiz";
        this.groupHiz.TabIndex = 2;
        //
        // lblHiz
        //
        this.lblHiz.AutoSize = true;
        this.lblHiz.Margin = new System.Windows.Forms.Padding(0, 0, 0, 4);
        this.lblHiz.Name = "lblHiz";
        this.lblHiz.Text = "3) Hız (km/s):";
        //
        // nudHiz
        //
        this.nudHiz.Margin = new System.Windows.Forms.Padding(0);
        this.nudHiz.Maximum = new decimal(new int[] { 300, 0, 0, 0 });
        this.nudHiz.Name = "nudHiz";
        this.nudHiz.Size = new System.Drawing.Size(160, 23);
        this.nudHiz.TabIndex = 0;
        this.nudHiz.ValueChanged += new System.EventHandler(this.nudHiz_ValueChanged);
        //
        // groupKm  -- Adim 4: km sayaci, min/max onceki/sonraki okumaya gore CANLI hesaplaniyor
        //
        this.groupKm.AutoSize = true;
        this.groupKm.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
        this.groupKm.Margin = new System.Windows.Forms.Padding(3, 3, 20, 3);
        this.groupKm.Visible = false;
        this.groupKm.Controls.Add(this.lblKm);
        this.groupKm.Controls.Add(this.nudKm);
        this.groupKm.Controls.Add(this.lblSinirBilgisi);
        this.groupKm.Name = "groupKm";
        this.groupKm.TabIndex = 3;
        //
        // lblKm
        //
        this.lblKm.AutoSize = true;
        this.lblKm.Margin = new System.Windows.Forms.Padding(0, 0, 0, 4);
        this.lblKm.Name = "lblKm";
        this.lblKm.Text = "4) Km Sayacı:";
        //
        // nudKm
        //
        this.nudKm.DecimalPlaces = 2;
        this.nudKm.Margin = new System.Windows.Forms.Padding(0, 0, 0, 4);
        this.nudKm.Maximum = new decimal(new int[] { 99999999, 0, 0, 0 });
        this.nudKm.Name = "nudKm";
        this.nudKm.Size = new System.Drawing.Size(200, 23);
        this.nudKm.TabIndex = 0;
        //
        // lblSinirBilgisi
        //
        this.lblSinirBilgisi.AutoSize = true;
        this.lblSinirBilgisi.ForeColor = System.Drawing.Color.SteelBlue;
        this.lblSinirBilgisi.Margin = new System.Windows.Forms.Padding(0);
        this.lblSinirBilgisi.MaximumSize = new System.Drawing.Size(220, 0);
        this.lblSinirBilgisi.Name = "lblSinirBilgisi";
        this.lblSinirBilgisi.Text = "";
        //
        // btnEkle
        //
        this.btnEkle.Enabled = false;
        this.btnEkle.Margin = new System.Windows.Forms.Padding(3, 26, 3, 3);
        this.btnEkle.Name = "btnEkle";
        this.btnEkle.Size = new System.Drawing.Size(90, 30);
        this.btnEkle.TabIndex = 4;
        this.btnEkle.Text = "Ekle";
        this.btnEkle.UseVisualStyleBackColor = true;
        this.btnEkle.Click += new System.EventHandler(this.btnEkle_Click);
        //
        // btnSil
        //
        this.btnSil.Enabled = false;
        this.btnSil.Margin = new System.Windows.Forms.Padding(3, 26, 10, 3);
        this.btnSil.Name = "btnSil";
        this.btnSil.Size = new System.Drawing.Size(90, 30);
        this.btnSil.TabIndex = 5;
        this.btnSil.Text = "Sil";
        this.btnSil.UseVisualStyleBackColor = true;
        this.btnSil.Click += new System.EventHandler(this.btnSil_Click);
        //
        // btnYenile
        //
        this.btnYenile.Margin = new System.Windows.Forms.Padding(3, 26, 3, 3);
        this.btnYenile.Name = "btnYenile";
        this.btnYenile.Size = new System.Drawing.Size(90, 30);
        this.btnYenile.TabIndex = 6;
        this.btnYenile.Text = "Yenile";
        this.btnYenile.UseVisualStyleBackColor = true;
        this.btnYenile.Click += new System.EventHandler(this.btnYenile_Click);
        //
        // btnHareketRaporu
        //
        this.btnHareketRaporu.Margin = new System.Windows.Forms.Padding(3, 26, 3, 3);
        this.btnHareketRaporu.Name = "btnHareketRaporu";
        this.btnHareketRaporu.Size = new System.Drawing.Size(200, 30);
        this.btnHareketRaporu.TabIndex = 7;
        this.btnHareketRaporu.Text = "Araç Hareket Raporu...";
        this.btnHareketRaporu.UseVisualStyleBackColor = true;
        this.btnHareketRaporu.Click += new System.EventHandler(this.btnHareketRaporu_Click);
        //
        // btnExcelAktar  -- o an gridde GORUNEN (filtreli olabilir) satirlari .xlsx olarak indirir
        //
        this.btnExcelAktar.Margin = new System.Windows.Forms.Padding(3, 26, 3, 3);
        this.btnExcelAktar.Name = "btnExcelAktar";
        this.btnExcelAktar.Size = new System.Drawing.Size(120, 30);
        this.btnExcelAktar.TabIndex = 8;
        this.btnExcelAktar.Text = "Excel'e Aktar";
        this.btnExcelAktar.UseVisualStyleBackColor = true;
        this.btnExcelAktar.Click += new System.EventHandler(this.btnExcelAktar_Click);
        //
        // btnExcelIceAktar
        //
        this.btnExcelIceAktar.Margin = new System.Windows.Forms.Padding(3, 26, 3, 3);
        this.btnExcelIceAktar.Name = "btnExcelIceAktar";
        this.btnExcelIceAktar.Size = new System.Drawing.Size(140, 30);
        this.btnExcelIceAktar.TabIndex = 10;
        this.btnExcelIceAktar.Text = "Excel'den İçe Aktar...";
        this.btnExcelIceAktar.UseVisualStyleBackColor = true;
        this.btnExcelIceAktar.Click += new System.EventHandler(this.btnExcelIceAktar_Click);
        //
        // btnCikisYap
        //
        this.btnCikisYap.Margin = new System.Windows.Forms.Padding(3, 26, 3, 3);
        this.btnCikisYap.Name = "btnCikisYap";
        this.btnCikisYap.Size = new System.Drawing.Size(100, 30);
        this.btnCikisYap.TabIndex = 9;
        this.btnCikisYap.Text = "Çıkış Yap";
        this.btnCikisYap.UseVisualStyleBackColor = true;
        this.btnCikisYap.Click += new System.EventHandler(this.btnCikisYap_Click);
        //
        // flowFiltre -- grid'in uzerindeki filtre seridi: doldurulan her alan, o alana TAM
        // ESIT olan satirlari gosterir (bos birakilan alanlar filtreye dahil edilmez).
        //
        this.flowFiltre.AutoSize = true;
        this.flowFiltre.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
        this.flowFiltre.Dock = System.Windows.Forms.DockStyle.Fill;
        this.flowFiltre.WrapContents = true;
        this.flowFiltre.Padding = new System.Windows.Forms.Padding(10, 4, 10, 4);
        this.flowFiltre.Controls.Add(this.lblFiltreBaslik);
        this.flowFiltre.Controls.Add(this.cmbFiltrePlaka);
        this.flowFiltre.Controls.Add(this.chkFiltreTarih);
        this.flowFiltre.Controls.Add(this.dtpFiltreTarih);
        this.flowFiltre.Controls.Add(this.lblFiltreHiz);
        this.flowFiltre.Controls.Add(this.txtFiltreHiz);
        this.flowFiltre.Controls.Add(this.lblFiltreKm);
        this.flowFiltre.Controls.Add(this.txtFiltreKm);
        this.flowFiltre.Controls.Add(this.btnFiltreUygula);
        this.flowFiltre.Controls.Add(this.btnFiltreTemizle);
        this.flowFiltre.Name = "flowFiltre";
        this.flowFiltre.TabIndex = 1;
        //
        // lblFiltreBaslik
        //
        this.lblFiltreBaslik.AutoSize = true;
        this.lblFiltreBaslik.Font = new System.Drawing.Font(this.Font, System.Drawing.FontStyle.Bold);
        this.lblFiltreBaslik.Margin = new System.Windows.Forms.Padding(3, 6, 12, 3);
        this.lblFiltreBaslik.Name = "lblFiltreBaslik";
        this.lblFiltreBaslik.Text = "Filtrele:";
        //
        // cmbFiltrePlaka
        //
        this.cmbFiltrePlaka.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
        this.cmbFiltrePlaka.Margin = new System.Windows.Forms.Padding(3, 3, 12, 3);
        this.cmbFiltrePlaka.Name = "cmbFiltrePlaka";
        this.cmbFiltrePlaka.Size = new System.Drawing.Size(140, 23);
        this.cmbFiltrePlaka.TabIndex = 0;
        //
        // chkFiltreTarih
        //
        this.chkFiltreTarih.AutoSize = true;
        this.chkFiltreTarih.Margin = new System.Windows.Forms.Padding(3, 5, 3, 3);
        this.chkFiltreTarih.Name = "chkFiltreTarih";
        this.chkFiltreTarih.Text = "Tarih:";
        this.chkFiltreTarih.UseVisualStyleBackColor = true;
        this.chkFiltreTarih.CheckedChanged += new System.EventHandler(this.chkFiltreTarih_CheckedChanged);
        //
        // dtpFiltreTarih
        //
        this.dtpFiltreTarih.Enabled = false;
        this.dtpFiltreTarih.Format = System.Windows.Forms.DateTimePickerFormat.Short;
        this.dtpFiltreTarih.Margin = new System.Windows.Forms.Padding(3, 3, 12, 3);
        this.dtpFiltreTarih.Name = "dtpFiltreTarih";
        this.dtpFiltreTarih.Size = new System.Drawing.Size(120, 23);
        this.dtpFiltreTarih.TabIndex = 1;
        //
        // lblFiltreHiz
        //
        this.lblFiltreHiz.AutoSize = true;
        this.lblFiltreHiz.Margin = new System.Windows.Forms.Padding(3, 6, 3, 3);
        this.lblFiltreHiz.Name = "lblFiltreHiz";
        this.lblFiltreHiz.Text = "Hız:";
        //
        // txtFiltreHiz
        //
        this.txtFiltreHiz.Margin = new System.Windows.Forms.Padding(3, 3, 12, 3);
        this.txtFiltreHiz.Name = "txtFiltreHiz";
        this.txtFiltreHiz.Size = new System.Drawing.Size(60, 23);
        this.txtFiltreHiz.TabIndex = 2;
        //
        // lblFiltreKm
        //
        this.lblFiltreKm.AutoSize = true;
        this.lblFiltreKm.Margin = new System.Windows.Forms.Padding(3, 6, 3, 3);
        this.lblFiltreKm.Name = "lblFiltreKm";
        this.lblFiltreKm.Text = "Km Sayacı:";
        //
        // txtFiltreKm
        //
        this.txtFiltreKm.Margin = new System.Windows.Forms.Padding(3, 3, 12, 3);
        this.txtFiltreKm.Name = "txtFiltreKm";
        this.txtFiltreKm.Size = new System.Drawing.Size(100, 23);
        this.txtFiltreKm.TabIndex = 3;
        //
        // btnFiltreUygula
        //
        this.btnFiltreUygula.Margin = new System.Windows.Forms.Padding(3, 3, 3, 3);
        this.btnFiltreUygula.Name = "btnFiltreUygula";
        this.btnFiltreUygula.Size = new System.Drawing.Size(90, 26);
        this.btnFiltreUygula.TabIndex = 4;
        this.btnFiltreUygula.Text = "Filtrele";
        this.btnFiltreUygula.UseVisualStyleBackColor = true;
        this.btnFiltreUygula.Click += new System.EventHandler(this.btnFiltreUygula_Click);
        //
        // btnFiltreTemizle
        //
        this.btnFiltreTemizle.Margin = new System.Windows.Forms.Padding(3, 3, 3, 3);
        this.btnFiltreTemizle.Name = "btnFiltreTemizle";
        this.btnFiltreTemizle.Size = new System.Drawing.Size(90, 26);
        this.btnFiltreTemizle.TabIndex = 5;
        this.btnFiltreTemizle.Text = "Temizle";
        this.btnFiltreTemizle.UseVisualStyleBackColor = true;
        this.btnFiltreTemizle.Click += new System.EventHandler(this.btnFiltreTemizle_Click);
        //
        // dgvHareketler
        //
        this.dgvHareketler.AllowUserToAddRows = false;
        this.dgvHareketler.AllowUserToDeleteRows = false;
        this.dgvHareketler.AutoGenerateColumns = false;
        this.dgvHareketler.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
        this.dgvHareketler.ColumnHeadersHeight = 32;
        this.dgvHareketler.Dock = System.Windows.Forms.DockStyle.Fill;
        this.dgvHareketler.ReadOnly = true;
        this.dgvHareketler.RowHeadersVisible = false;
        this.dgvHareketler.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
        this.dgvHareketler.Name = "dgvHareketler";
        this.dgvHareketler.TabIndex = 1;
        this.dgvHareketler.SelectionChanged += new System.EventHandler(this.dgvHareketler_SelectionChanged);
        //
        // flowSayfalama -- grid'in hemen altinda, sayfa boyutu + onceki/sonraki
        //
        this.flowSayfalama.AutoSize = true;
        this.flowSayfalama.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
        this.flowSayfalama.Dock = System.Windows.Forms.DockStyle.Fill;
        this.flowSayfalama.WrapContents = true;
        this.flowSayfalama.Padding = new System.Windows.Forms.Padding(10, 6, 10, 6);
        this.flowSayfalama.Controls.Add(this.lblSayfaBoyutu);
        this.flowSayfalama.Controls.Add(this.cmbSayfaBoyutu);
        this.flowSayfalama.Controls.Add(this.btnOncekiSayfa);
        this.flowSayfalama.Controls.Add(this.lblSayfaGostergesi);
        this.flowSayfalama.Controls.Add(this.btnSonrakiSayfa);
        this.flowSayfalama.Name = "flowSayfalama";
        this.flowSayfalama.TabIndex = 2;
        //
        // lblSayfaBoyutu
        //
        this.lblSayfaBoyutu.AutoSize = true;
        this.lblSayfaBoyutu.Margin = new System.Windows.Forms.Padding(3, 6, 6, 3);
        this.lblSayfaBoyutu.Name = "lblSayfaBoyutu";
        this.lblSayfaBoyutu.Text = "Sayfa başına:";
        //
        // cmbSayfaBoyutu
        //
        this.cmbSayfaBoyutu.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
        this.cmbSayfaBoyutu.Margin = new System.Windows.Forms.Padding(3, 3, 20, 3);
        this.cmbSayfaBoyutu.Name = "cmbSayfaBoyutu";
        this.cmbSayfaBoyutu.Size = new System.Drawing.Size(70, 23);
        this.cmbSayfaBoyutu.TabIndex = 0;
        this.cmbSayfaBoyutu.SelectedIndexChanged += new System.EventHandler(this.cmbSayfaBoyutu_SelectedIndexChanged);
        //
        // btnOncekiSayfa
        //
        this.btnOncekiSayfa.Margin = new System.Windows.Forms.Padding(3);
        this.btnOncekiSayfa.Name = "btnOncekiSayfa";
        this.btnOncekiSayfa.Size = new System.Drawing.Size(90, 26);
        this.btnOncekiSayfa.TabIndex = 1;
        this.btnOncekiSayfa.Text = "‹ Önceki";
        this.btnOncekiSayfa.UseVisualStyleBackColor = true;
        this.btnOncekiSayfa.Click += new System.EventHandler(this.btnOncekiSayfa_Click);
        //
        // lblSayfaGostergesi
        //
        this.lblSayfaGostergesi.AutoSize = true;
        this.lblSayfaGostergesi.Margin = new System.Windows.Forms.Padding(10, 6, 10, 3);
        this.lblSayfaGostergesi.Name = "lblSayfaGostergesi";
        this.lblSayfaGostergesi.Text = "Sayfa 1 / 1";
        //
        // btnSonrakiSayfa
        //
        this.btnSonrakiSayfa.Margin = new System.Windows.Forms.Padding(3);
        this.btnSonrakiSayfa.Name = "btnSonrakiSayfa";
        this.btnSonrakiSayfa.Size = new System.Drawing.Size(90, 26);
        this.btnSonrakiSayfa.TabIndex = 2;
        this.btnSonrakiSayfa.Text = "Sonraki ›";
        this.btnSonrakiSayfa.UseVisualStyleBackColor = true;
        this.btnSonrakiSayfa.Click += new System.EventHandler(this.btnSonrakiSayfa_Click);
        //
        // lblStatus  -- artik tableRoot'un KENDI son satiri (Form'a ayrica Dock=Bottom
        // EKLENMIYOR - bkz. tableRoot yorumu yukarida)
        //
        this.lblStatus.AutoSize = false;
        this.lblStatus.Dock = System.Windows.Forms.DockStyle.Fill;
        this.lblStatus.ForeColor = System.Drawing.Color.DimGray;
        this.lblStatus.Padding = new System.Windows.Forms.Padding(10, 0, 10, 0);
        this.lblStatus.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
        this.lblStatus.Name = "lblStatus";
        this.lblStatus.TabIndex = 3;
        //
        // AracHareketleriForm
        //
        this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
        this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
        this.ClientSize = new System.Drawing.Size(1000, 660);
        this.Controls.Add(this.tableRoot);
        this.MinimumSize = new System.Drawing.Size(420, 420);
        this.Name = "AracHareketleriForm";
        this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
        this.Text = "Seyir Mobil - Araç Hareketleri";
        this.Load += new System.EventHandler(this.AracHareketleriForm_Load);
        this.tableRoot.ResumeLayout(false);
        this.tableRoot.PerformLayout();
        this.flowUst.ResumeLayout(false);
        this.flowUst.PerformLayout();
        this.groupPlaka.ResumeLayout(false);
        this.groupPlaka.PerformLayout();
        this.groupTarih.ResumeLayout(false);
        this.groupTarih.PerformLayout();
        this.groupHiz.ResumeLayout(false);
        this.groupHiz.PerformLayout();
        ((System.ComponentModel.ISupportInitialize)(this.nudHiz)).EndInit();
        this.groupKm.ResumeLayout(false);
        this.groupKm.PerformLayout();
        ((System.ComponentModel.ISupportInitialize)(this.nudKm)).EndInit();
        this.flowFiltre.ResumeLayout(false);
        this.flowFiltre.PerformLayout();
        ((System.ComponentModel.ISupportInitialize)(this.dgvHareketler)).EndInit();
        this.flowSayfalama.ResumeLayout(false);
        this.flowSayfalama.PerformLayout();
        this.ResumeLayout(false);
    }
}
