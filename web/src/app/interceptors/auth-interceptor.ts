import { HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { Router } from '@angular/router';
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
          bildirim.bilgi('Oturum süreniz doldu veya geçersiz. Lütfen tekrar giriş yapın.');
        }
        router.navigate(['/login']);
      }
      return throwError(() => hata);
    })
  );
};
