namespace SeyirMobil.Api.Models;

public record AracHareketSinirlar(
    bool AyniTarihVarMi,
    DateOnly? OncekiTarih,
    decimal? OncekiKm,
    DateOnly? SonrakiTarih,
    decimal? SonrakiKm);

public record CreateAracHareketRequest(int AracId, string AracPlaka, DateOnly VeriTarihi, int Hiz, decimal KmSayaci);
