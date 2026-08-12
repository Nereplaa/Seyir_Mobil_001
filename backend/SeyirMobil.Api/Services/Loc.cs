using System.Globalization;

namespace SeyirMobil.Api.Services;

// Backend'in kullaniciya gosterilen mesajlarinin TR/EN cevirisi (2026-08-12, kullanici karari:
// web'deki i18n tum ekranlara yayildiktan sonra "backend mesajlarini da cevirelim" dendi).
// Frontend'deki tr.json/en.json ile AYNI ruhta ama ayri/basit bir mekanizma - .resx/IStringLocalizer
// gibi agir bir altyapiya gerek yok, sadece bir kac dusun sabit dogrulama/hata metni var.
//
// Dil, istemcinin HER istekte gonderdigi Accept-Language header'indan okunur - web tarafinda bu,
// kullanicinin secili uygulama diline (localStorage "seyir_dil") gore auth-interceptor.ts
// tarafindan otomatik ekleniyor, tarayicinin kendi varsayilan Accept-Language'inden BAGIMSIZ
// (kullanici arayuzde TR secip tarayicisi EN olsa bile backend mesajlari TR gelir - tutarlilik
// icin dogru olan bu, "sistem dili" degil "uygulamada secili dil" esas aliniyor).
public static class Loc
{
    private static readonly Dictionary<string, (string Tr, string En)> Metinler = new()
    {
        ["veriBulunamadi"] = ("Bu plaka ve tarih aralığında veri bulunamadı.", "No data found for this plate and date range."),
        ["hizAraligi"] = ("Hız 0-300 aralığında olmalı.", "Speed must be between 0-300."),
        ["ayniTarihKaydiVar"] = ("Bu plaka için bu tarihte zaten bir kayıt var.", "A record for this plate already exists on this date."),
        ["kmBuyukOlmali"] = ("Km sayacı, {0} tarihli {1:N2} km değerinden büyük olmalı.", "Odometer must be greater than the {1:N2} km reading dated {0}."),
        ["kmKucukOlmali"] = ("Km sayacı, {0} tarihli {1:N2} km değerinden küçük olmalı.", "Odometer must be less than the {1:N2} km reading dated {0}."),
        ["kmNegatifOlamaz"] = ("Km sayacı negatif olamaz.", "Odometer cannot be negative."),
        ["importBaziSatirlarBasarisiz"] = ("Bazı satırlar içe aktarılamadı, düzeltip tekrar deneyin.", "Some rows could not be imported, fix them and try again."),
        ["sifremiUnuttumYanit"] = ("Eğer bu e-posta adresi sistemde kayıtlıysa, şifre sıfırlama bağlantısı gönderildi.", "If this email address is registered, a password reset link has been sent."),
        ["sifreKisa"] = ("Şifre en az 6 karakter olmalı.", "Password must be at least 6 characters."),
        ["sifirlamaGecersiz"] = ("Sıfırlama bağlantısı geçersiz veya süresi dolmuş. Yeniden \"Şifremi Unuttum\" isteğinde bulunun.", "The reset link is invalid or has expired. Please request \"Forgot Password\" again."),
        ["sifreGuncellendi"] = ("Şifreniz güncellendi, şimdi giriş yapabilirsiniz.", "Your password has been updated, you can now sign in."),
        ["kullaniciAdiBos"] = ("Kullanıcı adı boş olamaz.", "Username cannot be empty."),
        ["rolGecersiz"] = ("Rol 'Admin' veya 'Viewer' olmalı.", "Role must be 'Admin' or 'Viewer'."),
        ["emailGecersiz"] = ("Geçerli bir e-posta adresi girilmeli.", "A valid email address must be entered."),
        ["kullaniciAdiKayitli"] = ("Bu kullanıcı adı zaten kayıtlı.", "This username is already registered."),
        ["emailKayitli"] = ("Bu e-posta adresi zaten kayıtlı.", "This email address is already registered."),
        ["importSatirHata"] = ("Satır {0}: {1}", "Row {0}: {1}"),
        ["importCakismaSecilmedi"] = ("Satır {0}: Çakışma için \"üzerine yaz\" veya \"atla\" seçilmedi.", "Row {0}: No action (\"overwrite\" or \"skip\") selected for the conflict."),
        ["plakaFormatGecersiz"] = (
            "Geçersiz plaka formatı. İl kodu 01-81 arasında olmalı. Ardından en fazla 3 harf gelir (Q, W, X harfleri kullanılmaz), şu kalıplardan biriyle devam eder: \"99 X 9999\", \"99 X 99999\", \"99 XX 999\", \"99 XX 9999\", \"99 XXX 99\" veya \"99 XXX 999\". Örnek: \"34 AB 141\".",
            "Invalid plate format. The province code must be 01-81. It's followed by up to 3 letters (Q, W, X are not used), continuing with one of these patterns: \"99 X 9999\", \"99 X 99999\", \"99 XX 999\", \"99 XX 9999\", \"99 XXX 99\" or \"99 XXX 999\". Example: \"34 AB 141\"."),
        ["tarihGecersizBos"] = ("Geçersiz veya boş tarih.", "Invalid or empty date."),
        ["kmNegatifVeyaBos"] = ("Km sayacı negatif olamaz veya boş.", "Odometer cannot be negative or empty."),
        ["dosyaIciTekrar"] = ("Bu dosyada aynı plaka + tarih için birden fazla satır var.", "This file has more than one row for the same plate + date."),
        ["basliklarEslesmiyor"] = (
            "Bu dosyanın sütun başlıkları beklenen formatla eşleşmiyor. Beklenen: {0}. Bulunan: {1}. Aşağıdaki \"Şablon İndir\" ile doğru formatta bir dosya indirip kullanabilirsiniz.",
            "This file's column headers don't match the expected format. Expected: {0}. Found: {1}. You can download a correctly formatted file below using \"Download Template\"."),
        ["bos"] = ("(boş)", "(empty)"),
    };

    public static string DilKodu(HttpRequest request)
    {
        var header = request.Headers.AcceptLanguage.ToString();
        return header.StartsWith("en", StringComparison.OrdinalIgnoreCase) ? "en" : "tr";
    }

    public static string T(string dil, string anahtar, params object?[] args)
    {
        var (tr, en) = Metinler[anahtar];
        var format = dil == "en" ? en : tr;
        return args.Length > 0 ? string.Format(CultureInfo.InvariantCulture, format, args) : format;
    }

    // Tarih formati dile gore degisir (TR: noktali dd.MM.yyyy, EN: M/d/yyyy - frontend'in
    // en-US locale'iyle tutarli, bkz. web/src/app/pages/admin-paneli/admin-paneli.ts formatTarih).
    public static string Tarih(string dil, DateOnly tarih) =>
        dil == "en" ? tarih.ToString("M/d/yyyy", CultureInfo.InvariantCulture) : tarih.ToString("dd.MM.yyyy", CultureInfo.InvariantCulture);
}
