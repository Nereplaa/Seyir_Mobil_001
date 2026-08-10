namespace SeyirMobil.Api.Models;

// Excel'den okunan (veya kullanicinin grid'de duzenledigi) HAM bir satir - hem ilk yukleme
// hem "yeniden dogrula" AYNI tipi kullanir, boylece iki ayri validasyon kodu yazilmiyor.
public record ImportHamSatir(int SatirNo, string AracPlaka, string VeriTarihi, int? Hiz, decimal? KmSayaci);

// Bir satirin dogrulama SONUCU - hem basariliysa (Hatalar bos) hem basarisizsa (Hatalar dolu)
// AYNI tip donuyor, boylece web/masaustu tek bir grid modeliyle her iki durumu da gosterebiliyor.
public record ImportSatiriSonuc(
    int SatirNo,
    string AracPlaka,          // kullanicinin YAZDIGI ham deger (grid'de gosterilir/duzenlenir)
    string KanonikAracPlaka,   // onayla'da veritabanina YAZILACAK gercek deger
    int? AracId,
    bool YeniAracMi,
    string? VeriTarihi,        // "yyyy-MM-dd", parse edilemediyse null
    int? Hiz,
    decimal? KmSayaci,
    bool CakismaVarMi,         // ayni plaka+tarih zaten DB'de var
    int? MevcutHiz,
    decimal? MevcutKmSayaci,
    List<string> Hatalar);

// DosyaHatasi dolu ise Satirlar BOS demektir - dosyanin sutun basliklari beklenen formatla
// hic eslesmedi (ör. rapor export'u yanlislikla import'a yuklendi), istemci bunu satir bazli
// hatalar yerine TEK bir belirgin uyari olarak gostermeli.
public record ImportOnizlemeYaniti(List<ImportSatiriSonuc> Satirlar, string? DosyaHatasi = null);

public record ImportYenidenDogrulaRequest(List<ImportHamSatir> Satirlar);

// VeriTarihi/Hiz/KmSayaci NULLABLE - CakismaAksiyonu "Atla" ise bu satir zaten hic
// veritabanina yazilmiyor (bkz. Program.cs import-onayla), o yuzden gecersiz/eksik veri
// icerebilir; non-nullable olsalar JSON'da null gelince TUM istek deserialize edilemezdi
// (kullanici geri bildirimi: hatali+atlanan bir satir yuzunden butun ice aktarma patliyordu).
public record ImportOnaylaSatiri(
    int SatirNo,
    string AracPlaka,
    string? VeriTarihi,
    int? Hiz,
    decimal? KmSayaci,
    string CakismaAksiyonu); // "UzerineYaz" | "Atla" | "" (cakisma yoksa onemsiz)

public record ImportOnaylaRequest(List<ImportOnaylaSatiri> Satirlar);

public record ImportOnaylaSonuc(int EklenenSayisi, int GuncellenenSayisi, int AtlananSayisi);
