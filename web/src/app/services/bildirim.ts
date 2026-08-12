import { Service, inject, Injector } from '@angular/core';
import { TranslateService } from '@ngx-translate/core';
import Swal from 'sweetalert2';

// Tum native alert()/confirm() cagrilarinin yerini alan tek merkez (Eren bey geri bildirimi,
// 2026-08-05 toplanti) - SweetAlert2 ile tutarli, DevExtreme temasiyla uyumlu bir gorunum.
// Baslik/buton metinleri i18n'li (2026-08-12'de tasindi) - `mesaj` parametresi caginin kendi
// sorumlulugu, cagiran taraf translate.instant() ile cevrilmis bir metin gecirmeli.
//
// TranslateService constructor'da DEGIL, Injector uzerinden TEMBEL (lazy) aliniyor - gercek bir
// dongusel bagimlilik (NG0200) yasandi (2026-08-12): authInterceptor HER HTTP istegi icin
// Bildirim'i inject ediyor, TranslateService de kendi ceviri dosyasini yuklerken HttpClient
// kullaniyor (bu istek de interceptor'dan geciyor) - Bildirim constructor'da dogrudan
// TranslateService isteseydi, TranslateService HENUZ KENDI OLUSTURULMASINI BITIRMEDEN tekrar
// istenmis oluyordu (Dil -> TranslateService -> [http loader] -> interceptor -> Bildirim ->
// TranslateService dongusu). Injector.get() cagrisi ilk gercek kullanima (hata()/bilgi() vb.
// cagrildigi ana) ertelendigi icin o noktada TranslateService zaten tam kurulu oluyor, dongu
// kirilmis oluyor.
@Service()
export class Bildirim {
  private readonly injector = inject(Injector);
  private get translate(): TranslateService {
    return this.injector.get(TranslateService);
  }

  hata(mesaj: string): void {
    Swal.fire({
      icon: 'error',
      title: this.translate.instant('common.hata'),
      text: mesaj,
      confirmButtonText: this.translate.instant('common.tamam'),
      confirmButtonColor: '#dc2626',
    });
  }

  bilgi(mesaj: string): void {
    Swal.fire({
      icon: 'info',
      title: this.translate.instant('common.bilgi'),
      text: mesaj,
      confirmButtonText: this.translate.instant('common.tamam'),
      confirmButtonColor: '#2563eb',
    });
  }

  async onaylaSil(mesaj: string): Promise<boolean> {
    const sonuc = await Swal.fire({
      icon: 'warning',
      title: this.translate.instant('common.eminMisiniz'),
      text: mesaj,
      showCancelButton: true,
      confirmButtonText: this.translate.instant('common.evetSil'),
      cancelButtonText: this.translate.instant('common.vazgec'),
      confirmButtonColor: '#dc2626',
      cancelButtonColor: '#6b7280',
    });
    return sonuc.isConfirmed;
  }

  // onaylaSil'in genel (silme'ye ozel olmayan) hali - "Evet" / "Vazgec" butonlariyla.
  async onayla(mesaj: string): Promise<boolean> {
    const sonuc = await Swal.fire({
      icon: 'question',
      title: this.translate.instant('common.onayliyorMusunuz'),
      text: mesaj,
      showCancelButton: true,
      confirmButtonText: this.translate.instant('common.evet'),
      cancelButtonText: this.translate.instant('common.vazgec'),
      confirmButtonColor: '#2563eb',
      cancelButtonColor: '#6b7280',
    });
    return sonuc.isConfirmed;
  }
}
