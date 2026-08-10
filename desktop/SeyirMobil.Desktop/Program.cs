using System.Net.Http.Headers;
using SeyirMobil.Desktop.Services;

namespace SeyirMobil.Desktop;

static class Program
{
    /// <summary>
    ///  The main entry point for the application.
    /// </summary>
    [STAThread]
    static async Task Main()
    {
        // To customize application configuration such as set high DPI settings or default font,
        // see https://aka.ms/applicationconfiguration.
        ApplicationConfiguration.Initialize();

        // Login <-> ana ekran dongusu: normal kapatma (X butonu) programdan tamamen cikar,
        // "Cikis Yap" veya oturumun gecersiz olmasi (401/idle-timeout) ise login ekranina
        // geri doner (AracHareketleriForm.OturumSonlandirildiMi bu ikisini ayirt eder).
        while (true)
        {
            if (!await KayitliOturumGecerliMiAsync())
            {
                using var loginForm = new LoginForm();
                if (loginForm.ShowDialog() != DialogResult.OK)
                {
                    return;
                }
            }

            using var anaForm = new AracHareketleriForm();
            Application.Run(anaForm);

            if (!AracHareketleriForm.OturumSonlandirildiMi)
            {
                return;
            }
            AracHareketleriForm.OturumSonlandirildiMi = false;
        }
    }

    // "Beni Hatirla" ile kaydedilmis bir token varsa gecerliligini backend'e sorup dogrular -
    // gecerliyse kullaniciyi login ekranindan hic gecirmeden dogrudan ana ekrana gonderir.
    private static async Task<bool> KayitliOturumGecerliMiAsync()
    {
        var kayitliToken = TokenStore.KayitliTokeniYukle();
        if (kayitliToken is null)
        {
            return false;
        }

        using var http = new HttpClient { BaseAddress = new Uri("http://localhost:5080/") };
        http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", kayitliToken);
        try
        {
            var yanit = await http.GetAsync("api/auth/me");
            if (yanit.IsSuccessStatusCode)
            {
                TokenStore.OturumBaslat(kayitliToken, beniHatirla: true);
                return true;
            }
        }
        catch
        {
            // Backend'e ulasilamadi - normal login akisina dusulur.
        }
        return false;
    }
}
