import { CanActivateFn, Router } from '@angular/router';
import { inject } from '@angular/core';
import { Auth } from '../services/auth';
import { rolBaslangicRotasi } from '../utils/rol-yonlendirme';

// Bir route'u belirli rollerle sinirlamak icin kullanilan bir GUARD FABRIKASI - "admin ise X
// degilse Y" gibi tek bir sabit kontrol yerine, her route kendi izinli rol listesini verir
// (ör. canActivate: [roleGuard(['Admin'])]). Yeni bir rol/route kombinasyonu eklendiginde bu
// fonksiyonun icine DOKUNULMAZ, sadece route tanimindaki listeye rol eklenir.
export function roleGuard(izinliRoller: string[]): CanActivateFn {
  return () => {
    const auth = inject(Auth);
    const router = inject(Router);

    if (!auth.girisYapilmisMi()) {
      router.navigate(['/login']);
      return false;
    }

    if (izinliRoller.includes(auth.role() ?? '')) {
      return true;
    }

    // Giris yapmis ama bu route'a izinli degil - hata sayfasi yerine KENDI rolunun
    // baslangic ekranina yonlendir, kullanici sessizce "yetkisiz" bir bosluga dusmesin.
    router.navigateByUrl(rolBaslangicRotasi(auth.role()));
    return false;
  };
}
