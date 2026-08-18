import { HttpInterceptorFn } from '@angular/common/http';
import { inject, Injector } from '@angular/core';
import { Router } from '@angular/router';
import { TranslateService } from '@ngx-translate/core';
import { catchError, throwError } from 'rxjs';
import { Auth } from '../services/auth';
import { Bildirim } from '../services/bildirim';

// Login istegi (henuz token yokken cagrilir) haric, tum isteklere Authorization header'i
// ekler. 401 donen bir istek (idle-timeout veya baska bir nedenle oturum gecersiz oldu)
// yerel oturumu temizleyip kullaniciyi login ekranina yonlendirir - istemcinin baska hicbir
// yerde 401'i ayrica kontrol etmesine gerek kalmaz.
const TOKEN_GEREKMEYEN_YOLLAR = ['/api/auth/login'];

// Backend mesajlarinin (dogrulama/hata metinleri) TR/EN cevirisi icin secili uygulama dilini
// her istekte bildiriyoruz (2026-08-12). DOGRUDAN localStorage'dan okunuyor, `Dil` servisi
// inject EDILMIYOR - Dil servisi TranslateService'e bagimli, TranslateService KENDI ceviri
// dosyasini yuklerken bu interceptor'dan gecen bir HTTP istegi atiyor; interceptor icinden
// Dil/TranslateService inject etmeye calismak NG0200 dongusel bagimlilik hatasina yol aciyordu
// (ayni sebep Bildirim.ts'te de yasandi, bkz. o dosyanin yorumu) - localStorage'a dogrudan
// erismek bu dongudeki HICBIR servise bagimli olmadigi icin sorunu tamamen atlatiyor.
const DIL_ANAHTARI = 'seyir_dil';

export const authInterceptor: HttpInterceptorFn = (req, next) => {
  const auth = inject(Auth);
  const router = inject(Router);
  const bildirim = inject(Bildirim);
  // TranslateService, Bildirim.ts'teki AYNI sebeple (NG0200 dongusel bagimlilik - yukaridaki
  // yorum) constructor'da DEGIL, Injector uzerinden TEMBEL aliniyor; catchError icinde (gercek
  // bir 401 olustugunda, uygulama tam kurulduktan cok sonra) cagrildigi icin dongu kirilmis olur.
  const injector = inject(Injector);

  const tokenGerekmiyor = TOKEN_GEREKMEYEN_YOLLAR.some((yol) => req.url.includes(yol));
  const token = auth.token();
  const dil = localStorage.getItem(DIL_ANAHTARI) ?? 'tr';

  const baslikliIstek = req.clone({ setHeaders: { 'Accept-Language': dil } });
  const istek =
    token && !tokenGerekmiyor
      ? baslikliIstek.clone({ setHeaders: { Authorization: `Bearer ${token}` } })
      : baslikliIstek;

  return next(istek).pipe(
    catchError((hata) => {
      if (hata.status === 401 && !tokenGerekmiyor) {
        // Ayni anda birden fazla istek 401 alirsa (ör. sayfa acilisinda paralel cagrilar),
        // sadece ILK yakalayan uyari gostersin - token zaten temizlenmisse (token() artik null)
        // digerleri sessizce gecer, tekrar tekrar "oturum sona erdi" penceresi acilmaz.
        const oturumZatenAcikti = !!auth.token();
        auth.oturumuTemizle();
        if (oturumZatenAcikti) {
          // translate.instant() DEGIL translate.get() - sayfa YENI acilmisken (ör. token zaten
          // gecersizken F5 yapilmasi) ceviri dosyasi henuz yuklenmeden bu kod calisirsa instant()
          // henuz olmayan degeri DEGIL, anahtarin kendisini ("common.oturumSuresiDoldu" duz metin
          // olarak) dondurur - Chrome'da bu senaryo simule edilip gercekten yakalandi (2026-08-18).
          // get() ceviri dosyasi yuklenene kadar bekleyen bir Observable dondurdugu icin bu
          // yarisa girmiyor.
          const translate = injector.get(TranslateService);
          translate.get('common.oturumSuresiDoldu').subscribe((mesaj) => bildirim.bilgi(mesaj));
        }
        router.navigate(['/login']);
      }
      return throwError(() => hata);
    })
  );
};
