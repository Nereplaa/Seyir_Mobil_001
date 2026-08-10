namespace SeyirMobil.Api.Models;

public record AracPlakaLookup(int AracId, string AracPlaka);

public record RaporTopluRequest(List<string> Plakalar, DateOnly Baslangic, DateOnly Bitis);

public record AracRaporSonucu(
    string AracPlaka,
    bool BulunduMu,
    DateOnly? BaslangicTarihi,
    decimal? BaslangicKm,
    DateOnly? BitisTarihi,
    decimal? BitisKm,
    decimal? YapilanKm);

// Detayli rapor: secilen araliktaki HER gercek okuma, bir onceki okumaya gore
// km farkiyla birlikte. Ilk okumanin Artis'i null (ondan onceki bir okuma
// aralik icinde yok).
public record AracHareketDetayRaporSatiri(
    string AracPlaka,
    DateOnly VeriTarihi,
    decimal KmSayaci,
    decimal? Artis);

// Excel export istekleri (2026-08-04). AyriPlakaBazliMi: true ise dosyada her
// plaka icin ayri bir baslik+veri blogu, false ise tum plakalar tek bir
// tablo altinda birlesik.
public record RaporExportRequest(
    List<string> Plakalar,
    DateOnly Baslangic,
    DateOnly Bitis,
    bool DetayliMi,
    bool AyriPlakaBazliMi);

// Ana liste (arac-hareketleri) export'u icin - istemci (web/masaustu) o an
// EKRANDA GOSTERDIGI (filtreli olabilir) satirlari oldugu gibi gonderir,
// backend sadece Excel'e bicimlendirir - filtreleme mantigi istemcide kalir.
public record AracHareketExportSatiri(
    int AracId,
    string AracPlaka,
    DateOnly VeriTarihi,
    int Hiz,
    decimal KmSayaci);
