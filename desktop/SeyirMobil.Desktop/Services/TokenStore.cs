using System.Security.Cryptography;
using System.Text;

namespace SeyirMobil.Desktop.Services;

// O anki oturum token'ini bellekte tutar, "beni hatirla" secilirse DPAPI (Windows'un kendi
// kullanici bazli sifreleme API'si) ile yerel bir dosyaya sifreli olarak kaydeder - token asla
// duz metin olarak diske yazilmaz.
public static class TokenStore
{
    private static readonly string DosyaYolu = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "SeyirMobil", "oturum.dat");

    public static string? Token { get; private set; }

    // API cagrilari 401 aldiginda (idle-timeout veya baska bir nedenle oturum gecersiz
    // oldugunda) OturumHandler tarafindan tetiklenir - UI katmani bunu dinleyip kullaniciyi
    // login ekranina geri gonderir.
    public static event Action? OturumGecersizOldu;

    public static void OturumBaslat(string token, bool beniHatirla)
    {
        Token = token;
        if (beniHatirla)
        {
            Kaydet(token);
        }
        else
        {
            SilKayitliOturum();
        }
    }

    public static void OturumBitir()
    {
        Token = null;
        SilKayitliOturum();
    }

    public static void GecersizOldu()
    {
        Token = null;
        SilKayitliOturum();
        OturumGecersizOldu?.Invoke();
    }

    public static string? KayitliTokeniYukle()
    {
        try
        {
            if (!File.Exists(DosyaYolu))
            {
                return null;
            }
            var sifreliVeri = File.ReadAllBytes(DosyaYolu);
            var veri = ProtectedData.Unprotect(sifreliVeri, null, DataProtectionScope.CurrentUser);
            return Encoding.UTF8.GetString(veri);
        }
        catch
        {
            // Dosya bozuksa/baska bir kullaniciya aitse sessizce yok say - normal login akisina dusulur.
            return null;
        }
    }

    private static void Kaydet(string token)
    {
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(DosyaYolu)!);
            var sifreliVeri = ProtectedData.Protect(Encoding.UTF8.GetBytes(token), null, DataProtectionScope.CurrentUser);
            File.WriteAllBytes(DosyaYolu, sifreliVeri);
        }
        catch
        {
            // Kalici saklama basarisiz olsa bile oturum bellekte calismaya devam eder.
        }
    }

    private static void SilKayitliOturum()
    {
        try
        {
            if (File.Exists(DosyaYolu))
            {
                File.Delete(DosyaYolu);
            }
        }
        catch
        {
            // Silme basarisiz olsa bile onemli degil - bir sonraki KayitliTokeniYukle zaten dogrulayacak.
        }
    }
}
