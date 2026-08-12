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
    // SIRA ONEMLI (2026-08-12 bug fix): once localStorage, SONRA translate.use(). Aksi durumda
    // (eski sira: once use() sonra localStorage) translate.use()'in tetikledigi onLangChange
    // aboneleri (bkz. import.ts - dil degisince otomatik yenidenDogrula()) SENKRON calisiyor ve
    // authInterceptor o anda localStorage'i OKUYOR - henuz yazilmamis oldugu icin BIR ONCEKI
    // dili gonderiyordu (backend'e hep "bir adim geride" bir Accept-Language gidiyordu, gercek
    // kullanici testinde yakalandi: hizli TR/EN degistirmede eski dildeki mesajlar goruluyordu).
    localStorage.setItem(DIL_ANAHTARI, kod);
    this.translate.use(kod);
  }

  get guncelDil(): string {
    return this.translate.currentLang() ?? 'tr';
  }
}
