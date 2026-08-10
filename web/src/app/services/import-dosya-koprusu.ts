import { Injectable } from '@angular/core';

// Liste sayfasindaki "Excel'den Veri Aktar" kisayolu, dosyayi burada gecici olarak
// tutup /import route'una yonlendiriyor - Import sayfasi acilinca burayi kontrol edip
// bekleyen bir dosya varsa dogrudan islemeye baslıyor (kullanicinin Import sayfasina
// gidip AYRICA "Excel Dosyası Seç" demesine gerek kalmadan).
@Injectable({ providedIn: 'root' })
export class ImportDosyaKoprusu {
  bekleyenDosya: File | null = null;
}
