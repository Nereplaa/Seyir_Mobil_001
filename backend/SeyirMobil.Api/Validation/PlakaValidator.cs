using System.Text.RegularExpressions;

namespace SeyirMobil.Api.Validation;

public static partial class PlakaValidator
{
    // Turkiye plaka formati: 01-81 il kodu + 1-3 harf + rakam (harf sayisina gore rakam sayisi degisir)
    // Harfler: A-Z hariç Q, W, X (Latin alfabesinde yok) ve Ç, Ş, İ, Ö, Ü, Ğ (plakada kullanilmiyor)
    [GeneratedRegex(@"^(0[1-9]|[1-7][0-9]|8[01])([A-PR-VYZ]\d{4,5}|[A-PR-VYZ]{2}\d{3,4}|[A-PR-VYZ]{3}\d{2,3})$")]
    private static partial Regex PlakaPattern();

    // Format() icin il kodu / harf / rakam bloklarini AYRI gruplar olarak yakalayan bir varyant -
    // PlakaPattern() harf+rakami tek grup olarak yakaliyor (uc alternatifi tek regex'te tutmak
    // icin), burada aralarina bosluk koyabilmek adina ayri gruplar lazim.
    [GeneratedRegex(@"^(0[1-9]|[1-7][0-9]|8[01])([A-PR-VYZ]{1,3})(\d{2,5})$")]
    private static partial Regex PlakaFormatPattern();

    public static string Normalize(string plaka) =>
        plaka.Trim().ToUpperInvariant().Replace(" ", "");

    public static bool IsValid(string plaka) => PlakaPattern().IsMatch(Normalize(plaka));

    // Gecerli bir plakayi standart "34 AB 141" bicimine (il kodu + harf + rakam, tek bosluklu)
    // cevirir - kullanici Excel'e boslukSUZ ya da kucuk harfle yazsa bile DB'ye hep ayni, tutarli
    // ve insan-okunur formatta yazilsin diye (gercek kullanici geri bildirimi: yeni eklenen
    // araclarin plakasi bitisik gorunuyordu, mevcut araclarinkiyle tutarsizdi).
    public static string Format(string plaka)
    {
        var normalized = Normalize(plaka);
        var match = PlakaFormatPattern().Match(normalized);
        return match.Success
            ? $"{match.Groups[1].Value} {match.Groups[2].Value} {match.Groups[3].Value}"
            : normalized;
    }
}
