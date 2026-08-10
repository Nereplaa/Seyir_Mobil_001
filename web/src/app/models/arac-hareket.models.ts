export interface AracHareketDto {
  id: number;
  aracId: number;
  aracPlaka: string;
  veriTarihi: string; // yyyy-MM-dd
  hiz: number;
  kmSayaci: number;
}

export interface AracPlakaLookupDto {
  aracId: number;
  aracPlaka: string;
}

export interface AracHareketSinirlarDto {
  ayniTarihVarMi: boolean;
  oncekiTarih: string | null;
  oncekiKm: number | null;
  sonrakiTarih: string | null;
  sonrakiKm: number | null;
}

export interface CreateAracHareketRequestDto {
  aracId: number;
  aracPlaka: string;
  veriTarihi: string;
  hiz: number;
  kmSayaci: number;
}

export interface RaporTopluRequestDto {
  plakalar: string[];
  baslangic: string;
  bitis: string;
}

export interface AracRaporSonucuDto {
  aracPlaka: string;
  bulunduMu: boolean;
  baslangicTarihi: string | null;
  baslangicKm: number | null;
  bitisTarihi: string | null;
  bitisKm: number | null;
  yapilanKm: number | null;
}

export interface AracHareketDetayRaporSatiriDto {
  aracPlaka: string;
  veriTarihi: string;
  kmSayaci: number;
  artis: number | null;
}

// ---------- Excel'den toplu veri girişi (import) ----------

export interface ImportHamSatirDto {
  satirNo: number;
  aracPlaka: string;
  veriTarihi: string;
  hiz: number | null;
  kmSayaci: number | null;
}

export interface ImportSatiriSonucDto {
  satirNo: number;
  aracPlaka: string;
  kanonikAracPlaka: string;
  aracId: number | null;
  yeniAracMi: boolean;
  veriTarihi: string | null;
  hiz: number | null;
  kmSayaci: number | null;
  cakismaVarMi: boolean;
  mevcutHiz: number | null;
  mevcutKmSayaci: number | null;
  hatalar: string[];
  // Sadece grid'de kullanilan, backend'den gelmeyen alan - cakisan satirlar icin kullanicinin
  // sectigi aksiyon ("UzerineYaz" | "Atla" | henuz secilmediyse "").
  cakismaAksiyonu?: string;
}

export interface ImportOnizlemeYanitiDto {
  satirlar: ImportSatiriSonucDto[];
  dosyaHatasi?: string | null;
}

export interface ImportOnaylaSatiriDto {
  satirNo: number;
  aracPlaka: string;
  // "Atla" secilen bir satir hic veritabanina yazilmiyor (backend bunu hata kontrolunden
  // once eleyip atliyor) - o yuzden gecersiz/eksik (null) veri icerebilir.
  veriTarihi: string | null;
  hiz: number | null;
  kmSayaci: number | null;
  cakismaAksiyonu: string;
}

export interface ImportOnaylaSonucDto {
  eklenenSayisi: number;
  guncellenenSayisi: number;
  atlananSayisi: number;
}

