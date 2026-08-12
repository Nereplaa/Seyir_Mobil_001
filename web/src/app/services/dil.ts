import { Injectable, inject } from '@angular/core';
import { TranslateService } from '@ngx-translate/core';

const DIL_ANAHTARI = 'seyir_dil';

// Coklu dil altyapisi (feedback_001, 2026-08-11 - Eren bey: "yavas yavas dil ekleyebiliriz").
// KADEMELI bir surec olarak tasarlandi: bu servis + public/i18n/*.json altyapisi HAZIR, ama
// su an sadece header/nav metinleri gercekten cevrilmis durumda (bkz. app.html) - geri kalan
// ekranlar zaman icinde AYNI desenle (TranslatePipe, "sayfa.anahtar" seklinde iic-ici JSON
// anahtarlari) tek tek tasinacak. Yeni bir dil eklemek icin: public/i18n/ altina yeni bir
// <kod>.json dosyasi + asagidaki DESTEKLENEN_DILLER listesine eklemek yeterli.
export const DESTEKLENEN_DILLER = [
  { kod: 'tr', etiket: 'TR' },
  { kod: 'en', etiket: 'EN' },
];

@Injectable({ providedIn: 'root' })
export class Dil {
  private readonly translate = inject(TranslateService);

  baslat(): void {
    const kayitliDil = localStorage.getItem(DIL_ANAHTARI);
    const gecerliKodlar = DESTEKLENEN_DILLER.map((d) => d.kod);
    const baslangicDili = kayitliDil && gecerliKodlar.includes(kayitliDil) ? kayitliDil : 'tr';
    this.translate.use(baslangicDili);
  }

  degistir(kod: string): void {
    this.translate.use(kod);
    localStorage.setItem(DIL_ANAHTARI, kod);
  }

  get guncelDil(): string {
    return this.translate.currentLang() ?? 'tr';
  }
}
