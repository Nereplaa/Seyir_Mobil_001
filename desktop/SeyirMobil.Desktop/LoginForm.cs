using SeyirMobil.Desktop.Services;

namespace SeyirMobil.Desktop;

public partial class LoginForm : Form
{
    private readonly AracHareketApiClient _apiClient = new();

    public LoginForm()
    {
        InitializeComponent();
    }

    private async void btnGirisYap_Click(object? sender, EventArgs e)
    {
        lblHata.Text = "";
        if (string.IsNullOrWhiteSpace(txtKullaniciAdi.Text) || txtSifre.Text.Length == 0)
        {
            lblHata.Text = "Kullanıcı adı ve şifre gerekli.";
            return;
        }

        btnGirisYap.Enabled = false;
        Cursor = Cursors.WaitCursor;
        try
        {
            var yanit = await _apiClient.LoginAsync(txtKullaniciAdi.Text.Trim(), txtSifre.Text);
            TokenStore.OturumBaslat(yanit.Token, chkBeniHatirla.Checked);
            DialogResult = DialogResult.OK;
            Close();
        }
        catch (Exception ex)
        {
            lblHata.Text = ex.Message;
        }
        finally
        {
            btnGirisYap.Enabled = true;
            Cursor = Cursors.Default;
        }
    }
}
