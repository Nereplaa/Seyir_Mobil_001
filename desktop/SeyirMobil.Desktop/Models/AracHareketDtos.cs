using System.Text.Json.Serialization;

namespace SeyirMobil.Desktop.Models;

public record AracHareketDto(
    [property: JsonPropertyName("id")] int Id,
    [property: JsonPropertyName("aracId")] int AracId,
    [property: JsonPropertyName("aracPlaka")] string AracPlaka,
    [property: JsonPropertyName("veriTarihi")] DateOnly VeriTarihi,
    [property: JsonPropertyName("hiz")] int Hiz,
    [property: JsonPropertyName("kmSayaci")] decimal KmSayaci);

public record AracHareketSinirlarDto(
    [property: JsonPropertyName("ayniTarihVarMi")] bool AyniTarihVarMi,
    [property: JsonPropertyName("oncekiTarih")] DateOnly? OncekiTarih,
    [property: JsonPropertyName("oncekiKm")] decimal? OncekiKm,
    [property: JsonPropertyName("sonrakiTarih")] DateOnly? SonrakiTarih,
    [property: JsonPropertyName("sonrakiKm")] decimal? SonrakiKm);

public record CreateAracHareketRequestDto(
    [property: JsonPropertyName("aracId")] int AracId,
    [property: JsonPropertyName("aracPlaka")] string AracPlaka,
    [property: JsonPropertyName("veriTarihi")] DateOnly VeriTarihi,
    [property: JsonPropertyName("hiz")] int Hiz,
    [property: JsonPropertyName("kmSayaci")] decimal KmSayaci);

public record AracPlakaLookupDto(
    [property: JsonPropertyName("aracId")] int AracId,
    [property: JsonPropertyName("aracPlaka")] string AracPlaka);

public record RaporTopluRequestDto(
    [property: JsonPropertyName("plakalar")] List<string> Plakalar,
    [property: JsonPropertyName("baslangic")] DateOnly Baslangic,
    [property: JsonPropertyName("bitis")] DateOnly Bitis);

public record AracRaporSonucuDto(
    [property: JsonPropertyName("aracPlaka")] string AracPlaka,
    [property: JsonPropertyName("bulunduMu")] bool BulunduMu,
    [property: JsonPropertyName("baslangicTarihi")] DateOnly? BaslangicTarihi,
    [property: JsonPropertyName("baslangicKm")] decimal? BaslangicKm,
    [property: JsonPropertyName("bitisTarihi")] DateOnly? BitisTarihi,
    [property: JsonPropertyName("bitisKm")] decimal? BitisKm,
    [property: JsonPropertyName("yapilanKm")] decimal? YapilanKm);

public record AracHareketDetayRaporSatiriDto(
    [property: JsonPropertyName("aracPlaka")] string AracPlaka,
    [property: JsonPropertyName("veriTarihi")] DateOnly VeriTarihi,
    [property: JsonPropertyName("kmSayaci")] decimal KmSayaci,
    [property: JsonPropertyName("artis")] decimal? Artis);

public record RaporExportRequestDto(
    [property: JsonPropertyName("plakalar")] List<string> Plakalar,
    [property: JsonPropertyName("baslangic")] DateOnly Baslangic,
    [property: JsonPropertyName("bitis")] DateOnly Bitis,
    [property: JsonPropertyName("detayliMi")] bool DetayliMi,
    [property: JsonPropertyName("ayriPlakaBazliMi")] bool AyriPlakaBazliMi);

// ---------- Excel'den toplu veri girisi (import) ----------

public record ImportHamSatirDto(
    [property: JsonPropertyName("satirNo")] int SatirNo,
    [property: JsonPropertyName("aracPlaka")] string AracPlaka,
    [property: JsonPropertyName("veriTarihi")] string VeriTarihi,
    [property: JsonPropertyName("hiz")] int? Hiz,
    [property: JsonPropertyName("kmSayaci")] decimal? KmSayaci);

public record ImportSatiriSonucDto(
    [property: JsonPropertyName("satirNo")] int SatirNo,
    [property: JsonPropertyName("aracPlaka")] string AracPlaka,
    [property: JsonPropertyName("kanonikAracPlaka")] string KanonikAracPlaka,
    [property: JsonPropertyName("aracId")] int? AracId,
    [property: JsonPropertyName("yeniAracMi")] bool YeniAracMi,
    [property: JsonPropertyName("veriTarihi")] string? VeriTarihi,
    [property: JsonPropertyName("hiz")] int? Hiz,
    [property: JsonPropertyName("kmSayaci")] decimal? KmSayaci,
    [property: JsonPropertyName("cakismaVarMi")] bool CakismaVarMi,
    [property: JsonPropertyName("mevcutHiz")] int? MevcutHiz,
    [property: JsonPropertyName("mevcutKmSayaci")] decimal? MevcutKmSayaci,
    [property: JsonPropertyName("hatalar")] List<string> Hatalar);

public record ImportOnizlemeYanitiDto(
    [property: JsonPropertyName("satirlar")] List<ImportSatiriSonucDto> Satirlar);

public record ImportYenidenDogrulaRequestDto(
    [property: JsonPropertyName("satirlar")] List<ImportHamSatirDto> Satirlar);

public record ImportOnaylaSatiriDto(
    [property: JsonPropertyName("satirNo")] int SatirNo,
    [property: JsonPropertyName("aracPlaka")] string AracPlaka,
    [property: JsonPropertyName("veriTarihi")] string VeriTarihi,
    [property: JsonPropertyName("hiz")] int Hiz,
    [property: JsonPropertyName("kmSayaci")] decimal KmSayaci,
    [property: JsonPropertyName("cakismaAksiyonu")] string CakismaAksiyonu);

public record ImportOnaylaRequestDto(
    [property: JsonPropertyName("satirlar")] List<ImportOnaylaSatiriDto> Satirlar);

public record ImportOnaylaSonucDto(
    [property: JsonPropertyName("eklenenSayisi")] int EklenenSayisi,
    [property: JsonPropertyName("guncellenenSayisi")] int GuncellenenSayisi,
    [property: JsonPropertyName("atlananSayisi")] int AtlananSayisi);
