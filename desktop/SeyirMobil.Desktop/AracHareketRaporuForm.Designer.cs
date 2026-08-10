namespace SeyirMobil.Desktop;

partial class AracHareketRaporuForm
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
    private System.Windows.Forms.FlowLayoutPanel groupPlakalar;
    private System.Windows.Forms.Label lblPlakalar;
    private System.Windows.Forms.CheckedListBox cblPlakalar;
    private System.Windows.Forms.FlowLayoutPanel groupBaslangic;
    private System.Windows.Forms.Label lblBaslangic;
    private System.Windows.Forms.DateTimePicker dtpBaslangic;
    private System.Windows.Forms.FlowLayoutPanel groupBitis;
    private System.Windows.Forms.Label lblBitis;
    private System.Windows.Forms.DateTimePicker dtpBitis;
    private System.Windows.Forms.CheckBox chkDetayliRapor;
    private System.Windows.Forms.Button btnRaporOlustur;
    private System.Windows.Forms.ComboBox cmbExportModu;
    private System.Windows.Forms.Button btnExcelAktar;
    private System.Windows.Forms.Button btnGeri;
    private System.Windows.Forms.DataGridView dgvRapor;
    private System.Windows.Forms.Label lblStatus;

    private void InitializeComponent()
    {
        this.tableRoot = new System.Windows.Forms.TableLayoutPanel();
        this.flowUst = new System.Windows.Forms.FlowLayoutPanel();
        this.groupPlakalar = new System.Windows.Forms.FlowLayoutPanel();
        this.lblPlakalar = new System.Windows.Forms.Label();
        this.cblPlakalar = new System.Windows.Forms.CheckedListBox();
        this.groupBaslangic = new System.Windows.Forms.FlowLayoutPanel();
        this.lblBaslangic = new System.Windows.Forms.Label();
        this.dtpBaslangic = new System.Windows.Forms.DateTimePicker();
        this.groupBitis = new System.Windows.Forms.FlowLayoutPanel();
        this.lblBitis = new System.Windows.Forms.Label();
        this.dtpBitis = new System.Windows.Forms.DateTimePicker();
        this.chkDetayliRapor = new System.Windows.Forms.CheckBox();
        this.btnRaporOlustur = new System.Windows.Forms.Button();
        this.cmbExportModu = new System.Windows.Forms.ComboBox();
        this.btnExcelAktar = new System.Windows.Forms.Button();
        this.btnGeri = new System.Windows.Forms.Button();
        this.dgvRapor = new System.Windows.Forms.DataGridView();
        this.lblStatus = new System.Windows.Forms.Label();
        this.tableRoot.SuspendLayout();
        this.flowUst.SuspendLayout();
        this.groupPlakalar.SuspendLayout();
        this.groupBaslangic.SuspendLayout();
        this.groupBitis.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)(this.dgvRapor)).BeginInit();
        this.SuspendLayout();
        //
        // tableRoot -- ust (degisken boy) sihirbaz + alt (kalani doldur) grid icin 2 satirli
        // duzen (bkz. AracHareketleriForm'daki ayni gerekce - Dock=Top/Dock=Fill kardes
        // kombinasyonu grid'in yeniden hizalanmasinda guvenilir degildi).
        //
        this.tableRoot.ColumnCount = 1;
        this.tableRoot.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
        this.tableRoot.Dock = System.Windows.Forms.DockStyle.Fill;
        this.tableRoot.RowCount = 2;
        this.tableRoot.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
        this.tableRoot.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
        this.tableRoot.Controls.Add(this.flowUst, 0, 0);
        this.tableRoot.Controls.Add(this.dgvRapor, 0, 1);
        this.tableRoot.Name = "tableRoot";
        //
        // flowUst -- pencere daralinca gruplar alt satira kayar (responsive)
        //
        this.flowUst.AutoSize = true;
        this.flowUst.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
        this.flowUst.Dock = System.Windows.Forms.DockStyle.Fill;
        this.flowUst.WrapContents = true;
        this.flowUst.Padding = new System.Windows.Forms.Padding(10);
        this.flowUst.Controls.Add(this.groupPlakalar);
        this.flowUst.Controls.Add(this.groupBaslangic);
        this.flowUst.Controls.Add(this.groupBitis);
        this.flowUst.Controls.Add(this.chkDetayliRapor);
        this.flowUst.Controls.Add(this.btnRaporOlustur);
        this.flowUst.Controls.Add(this.cmbExportModu);
        this.flowUst.Controls.Add(this.btnExcelAktar);
        this.flowUst.Controls.Add(this.btnGeri);
        this.flowUst.Name = "flowUst";
        this.flowUst.TabIndex = 0;
        //
        // groupPlakalar
        //
        this.groupPlakalar.AutoSize = true;
        this.groupPlakalar.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
        this.groupPlakalar.Margin = new System.Windows.Forms.Padding(3, 3, 20, 3);
        this.groupPlakalar.Controls.Add(this.lblPlakalar);
        this.groupPlakalar.Controls.Add(this.cblPlakalar);
        this.groupPlakalar.Name = "groupPlakalar";
        this.groupPlakalar.TabIndex = 0;
        //
        // lblPlakalar
        //
        this.lblPlakalar.AutoSize = true;
        this.lblPlakalar.Margin = new System.Windows.Forms.Padding(0, 0, 0, 4);
        this.lblPlakalar.Name = "lblPlakalar";
        this.lblPlakalar.Text = "Araçlar (birden fazla seçebilirsiniz):";
        //
        // cblPlakalar
        //
        this.cblPlakalar.CheckOnClick = true;
        this.cblPlakalar.Margin = new System.Windows.Forms.Padding(0);
        this.cblPlakalar.Name = "cblPlakalar";
        this.cblPlakalar.Size = new System.Drawing.Size(200, 154);
        this.cblPlakalar.TabIndex = 0;
        this.cblPlakalar.ItemCheck += new System.Windows.Forms.ItemCheckEventHandler(this.cblPlakalar_ItemCheck);
        //
        // groupBaslangic
        //
        this.groupBaslangic.AutoSize = true;
        this.groupBaslangic.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
        this.groupBaslangic.Margin = new System.Windows.Forms.Padding(3, 3, 20, 3);
        this.groupBaslangic.Controls.Add(this.lblBaslangic);
        this.groupBaslangic.Controls.Add(this.dtpBaslangic);
        this.groupBaslangic.Name = "groupBaslangic";
        this.groupBaslangic.TabIndex = 1;
        //
        // lblBaslangic
        //
        this.lblBaslangic.AutoSize = true;
        this.lblBaslangic.Margin = new System.Windows.Forms.Padding(0, 0, 0, 4);
        this.lblBaslangic.Name = "lblBaslangic";
        this.lblBaslangic.Text = "Başlangıç Tarihi:";
        //
        // dtpBaslangic
        //
        this.dtpBaslangic.Format = System.Windows.Forms.DateTimePickerFormat.Short;
        this.dtpBaslangic.Margin = new System.Windows.Forms.Padding(0);
        this.dtpBaslangic.Name = "dtpBaslangic";
        this.dtpBaslangic.Size = new System.Drawing.Size(160, 23);
        this.dtpBaslangic.TabIndex = 0;
        this.dtpBaslangic.ValueChanged += new System.EventHandler(this.dtpBaslangic_ValueChanged);
        //
        // groupBitis  -- baslangic secilene kadar tum grup gizli
        //
        this.groupBitis.AutoSize = true;
        this.groupBitis.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
        this.groupBitis.Margin = new System.Windows.Forms.Padding(3, 3, 20, 3);
        this.groupBitis.Visible = false;
        this.groupBitis.Controls.Add(this.lblBitis);
        this.groupBitis.Controls.Add(this.dtpBitis);
        this.groupBitis.Name = "groupBitis";
        this.groupBitis.TabIndex = 2;
        //
        // lblBitis
        //
        this.lblBitis.AutoSize = true;
        this.lblBitis.Margin = new System.Windows.Forms.Padding(0, 0, 0, 4);
        this.lblBitis.Name = "lblBitis";
        this.lblBitis.Text = "Bitiş Tarihi:";
        //
        // dtpBitis
        //
        this.dtpBitis.Format = System.Windows.Forms.DateTimePickerFormat.Short;
        this.dtpBitis.Margin = new System.Windows.Forms.Padding(0);
        this.dtpBitis.Name = "dtpBitis";
        this.dtpBitis.Size = new System.Drawing.Size(160, 23);
        this.dtpBitis.TabIndex = 0;
        this.dtpBitis.ValueChanged += new System.EventHandler(this.dtpBitis_ValueChanged);
        //
        // chkDetayliRapor -- isaretlenirse gun gun (her gercek okuma) detay raporu uretilir
        //
        this.chkDetayliRapor.AutoSize = true;
        this.chkDetayliRapor.Margin = new System.Windows.Forms.Padding(3, 29, 20, 3);
        this.chkDetayliRapor.Name = "chkDetayliRapor";
        this.chkDetayliRapor.Text = "Detaylı Rapor (gün gün)";
        this.chkDetayliRapor.UseVisualStyleBackColor = true;
        this.chkDetayliRapor.TabIndex = 3;
        //
        // btnRaporOlustur
        //
        this.btnRaporOlustur.Enabled = false;
        this.btnRaporOlustur.Margin = new System.Windows.Forms.Padding(3, 26, 10, 3);
        this.btnRaporOlustur.Name = "btnRaporOlustur";
        this.btnRaporOlustur.Size = new System.Drawing.Size(160, 32);
        this.btnRaporOlustur.TabIndex = 4;
        this.btnRaporOlustur.Text = "Rapor Oluştur";
        this.btnRaporOlustur.UseVisualStyleBackColor = true;
        this.btnRaporOlustur.Click += new System.EventHandler(this.btnRaporOlustur_Click);
        //
        // cmbExportModu -- Excel'e Aktar hangi yapida (ayri bolum / tek tablo) uretilecek
        //
        this.cmbExportModu.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
        this.cmbExportModu.Margin = new System.Windows.Forms.Padding(20, 26, 3, 3);
        this.cmbExportModu.Name = "cmbExportModu";
        this.cmbExportModu.Size = new System.Drawing.Size(210, 23);
        this.cmbExportModu.TabIndex = 5;
        this.cmbExportModu.Items.AddRange(new object[] { "Her plaka için ayrı bölüm", "Tüm plakalar tek tabloda" });
        this.cmbExportModu.SelectedIndex = 0;
        //
        // btnExcelAktar
        //
        this.btnExcelAktar.Margin = new System.Windows.Forms.Padding(3, 26, 10, 3);
        this.btnExcelAktar.Name = "btnExcelAktar";
        this.btnExcelAktar.Size = new System.Drawing.Size(120, 32);
        this.btnExcelAktar.TabIndex = 6;
        this.btnExcelAktar.Text = "Excel'e Aktar";
        this.btnExcelAktar.UseVisualStyleBackColor = true;
        this.btnExcelAktar.Click += new System.EventHandler(this.btnExcelAktar_Click);
        //
        // btnGeri
        //
        this.btnGeri.Margin = new System.Windows.Forms.Padding(3, 26, 3, 3);
        this.btnGeri.Name = "btnGeri";
        this.btnGeri.Size = new System.Drawing.Size(92, 32);
        this.btnGeri.TabIndex = 7;
        this.btnGeri.Text = "← Geri";
        this.btnGeri.UseVisualStyleBackColor = true;
        this.btnGeri.Click += new System.EventHandler(this.btnGeri_Click);
        //
        // dgvRapor
        //
        this.dgvRapor.AllowUserToAddRows = false;
        this.dgvRapor.AllowUserToDeleteRows = false;
        this.dgvRapor.AutoGenerateColumns = false;
        this.dgvRapor.Dock = System.Windows.Forms.DockStyle.Fill;
        this.dgvRapor.ReadOnly = true;
        this.dgvRapor.RowHeadersVisible = false;
        this.dgvRapor.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
        this.dgvRapor.Name = "dgvRapor";
        this.dgvRapor.TabIndex = 1;
        //
        // lblStatus
        //
        this.lblStatus.AutoSize = false;
        this.lblStatus.Dock = System.Windows.Forms.DockStyle.Bottom;
        this.lblStatus.ForeColor = System.Drawing.Color.DimGray;
        this.lblStatus.Height = 28;
        this.lblStatus.Padding = new System.Windows.Forms.Padding(10, 0, 10, 0);
        this.lblStatus.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
        this.lblStatus.Name = "lblStatus";
        this.lblStatus.TabIndex = 2;
        //
        // AracHareketRaporuForm
        //
        this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
        this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
        this.ClientSize = new System.Drawing.Size(820, 561);
        this.Controls.Add(this.lblStatus);
        this.Controls.Add(this.tableRoot);
        this.MinimumSize = new System.Drawing.Size(420, 380);
        this.Name = "AracHareketRaporuForm";
        this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
        this.Text = "Araç Hareket Raporu";
        this.Load += new System.EventHandler(this.AracHareketRaporuForm_Load);
        this.tableRoot.ResumeLayout(false);
        this.tableRoot.PerformLayout();
        this.flowUst.ResumeLayout(false);
        this.flowUst.PerformLayout();
        this.groupPlakalar.ResumeLayout(false);
        this.groupPlakalar.PerformLayout();
        this.groupBaslangic.ResumeLayout(false);
        this.groupBaslangic.PerformLayout();
        this.groupBitis.ResumeLayout(false);
        this.groupBitis.PerformLayout();
        ((System.ComponentModel.ISupportInitialize)(this.dgvRapor)).EndInit();
        this.ResumeLayout(false);
    }
}
