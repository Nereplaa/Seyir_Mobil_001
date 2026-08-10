using System.Net;

namespace SeyirMobil.Desktop.Services;

public static class HataYardimcisi
{
    // 401 (oturum gecersiz/suresi dolmus) hatasi icin form-bazli catch bloklarinin KENDI
    // genel "Backend API calisiyor mu?" mesajini gostermesini engellemek icin kullanilir -
    // bu durumda zaten OturumHandler + AracHareketleriForm'daki merkezi handler net bir
    // "Oturum Sona Erdi" mesaji gosterip kullaniciyi login ekranina donduruyor, ikinci/celiskili
    // bir hata kutusuna gerek yok.
    public static bool OturumSuresiDolduMu(Exception ex) =>
        ex is HttpRequestException http && http.StatusCode == HttpStatusCode.Unauthorized;
}
