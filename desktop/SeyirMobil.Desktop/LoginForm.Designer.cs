namespace SeyirMobil.Desktop;

partial class LoginForm
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

    private System.Windows.Forms.FlowLayoutPanel flowRoot;
    private System.Windows.Forms.Label lblBaslik;
    private System.Windows.Forms.Label lblKullaniciAdi;
    private System.Windows.Forms.TextBox txtKullaniciAdi;
    private System.Windows.Forms.Label lblSifre;
    private System.Windows.Forms.TextBox txtSifre;
    private System.Windows.Forms.CheckBox chkBeniHatirla;
    private System.Windows.Forms.Label lblHata;
    private System.Windows.Forms.Button btnGirisYap;

    private void InitializeComponent()
    {
        this.flowRoot = new System.Windows.Forms.FlowLayoutPanel();
        this.lblBaslik = new System.Windows.Forms.Label();
        this.lblKullaniciAdi = new System.Windows.Forms.Label();
        this.txtKullaniciAdi = new System.Windows.Forms.TextBox();
        this.lblSifre = new System.Windows.Forms.Label();
        this.txtSifre = new System.Windows.Forms.TextBox();
        this.chkBeniHatirla = new System.Windows.Forms.CheckBox();
        this.lblHata = new System.Windows.Forms.Label();
        this.btnGirisYap = new System.Windows.Forms.Button();
        this.flowRoot.SuspendLayout();
        this.SuspendLayout();
        //
        // flowRoot -- tek bir kok konteyner (Dock=Fill), formun TEK kontrolu - iki bagimsiz
        // layout motorunun cakismasi riski yok (bkz. api_patterns.md, AracHareketleriForm dersi).
        //
        this.flowRoot.Dock = System.Windows.Forms.DockStyle.Fill;
        this.flowRoot.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
        this.flowRoot.WrapContents = false;
        this.flowRoot.Padding = new System.Windows.Forms.Padding(24);
        this.flowRoot.Controls.Add(this.lblBaslik);
        this.flowRoot.Controls.Add(this.lblKullaniciAdi);
        this.flowRoot.Controls.Add(this.txtKullaniciAdi);
        this.flowRoot.Controls.Add(this.lblSifre);
        this.flowRoot.Controls.Add(this.txtSifre);
        this.flowRoot.Controls.Add(this.chkBeniHatirla);
        this.flowRoot.Controls.Add(this.lblHata);
        this.flowRoot.Controls.Add(this.btnGirisYap);
        this.flowRoot.Name = "flowRoot";
        //
        // lblBaslik
        //
        this.lblBaslik.AutoSize = true;
        this.lblBaslik.Font = new System.Drawing.Font(this.Font.FontFamily, 13F, System.Drawing.FontStyle.Bold);
        this.lblBaslik.Margin = new System.Windows.Forms.Padding(0, 0, 0, 16);
        this.lblBaslik.Name = "lblBaslik";
        this.lblBaslik.Text = "Seyir Mobil'e Giriş Yap";
        //
        // lblKullaniciAdi
        //
        this.lblKullaniciAdi.AutoSize = true;
        this.lblKullaniciAdi.Margin = new System.Windows.Forms.Padding(0, 0, 0, 4);
        this.lblKullaniciAdi.Name = "lblKullaniciAdi";
        this.lblKullaniciAdi.Text = "Kullanıcı Adı:";
        //
        // txtKullaniciAdi
        //
        this.txtKullaniciAdi.Margin = new System.Windows.Forms.Padding(0, 0, 0, 12);
        this.txtKullaniciAdi.Name = "txtKullaniciAdi";
        this.txtKullaniciAdi.Size = new System.Drawing.Size(260, 23);
        this.txtKullaniciAdi.TabIndex = 0;
        //
        // lblSifre
        //
        this.lblSifre.AutoSize = true;
        this.lblSifre.Margin = new System.Windows.Forms.Padding(0, 0, 0, 4);
        this.lblSifre.Name = "lblSifre";
        this.lblSifre.Text = "Şifre:";
        //
        // txtSifre
        //
        this.txtSifre.Margin = new System.Windows.Forms.Padding(0, 0, 0, 12);
        this.txtSifre.Name = "txtSifre";
        this.txtSifre.Size = new System.Drawing.Size(260, 23);
        this.txtSifre.TabIndex = 1;
        this.txtSifre.UseSystemPasswordChar = true;
        //
        // chkBeniHatirla
        //
        this.chkBeniHatirla.AutoSize = true;
        this.chkBeniHatirla.Margin = new System.Windows.Forms.Padding(0, 0, 0, 12);
        this.chkBeniHatirla.Name = "chkBeniHatirla";
        this.chkBeniHatirla.Text = "Beni Hatırla";
        this.chkBeniHatirla.TabIndex = 2;
        this.chkBeniHatirla.UseVisualStyleBackColor = true;
        //
        // lblHata
        //
        this.lblHata.AutoSize = true;
        this.lblHata.ForeColor = System.Drawing.Color.Firebrick;
        this.lblHata.Margin = new System.Windows.Forms.Padding(0, 0, 0, 8);
        this.lblHata.MaximumSize = new System.Drawing.Size(260, 0);
        this.lblHata.Name = "lblHata";
        this.lblHata.Text = "";
        //
        // btnGirisYap
        //
        this.btnGirisYap.Margin = new System.Windows.Forms.Padding(0);
        this.btnGirisYap.Name = "btnGirisYap";
        this.btnGirisYap.Size = new System.Drawing.Size(260, 32);
        this.btnGirisYap.TabIndex = 3;
        this.btnGirisYap.Text = "Giriş Yap";
        this.btnGirisYap.UseVisualStyleBackColor = true;
        this.btnGirisYap.Click += new System.EventHandler(this.btnGirisYap_Click);
        //
        // LoginForm
        //
        this.AcceptButton = this.btnGirisYap;
        this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
        this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
        this.ClientSize = new System.Drawing.Size(340, 340);
        this.Controls.Add(this.flowRoot);
        this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
        this.MaximizeBox = false;
        this.MinimizeBox = false;
        this.Name = "LoginForm";
        this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
        this.Text = "Seyir Mobil - Giriş";
        this.flowRoot.ResumeLayout(false);
        this.flowRoot.PerformLayout();
        this.ResumeLayout(false);
    }
}
