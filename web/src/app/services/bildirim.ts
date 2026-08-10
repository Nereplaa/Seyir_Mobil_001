import { Service } from '@angular/core';
import Swal from 'sweetalert2';

// Tum native alert()/confirm() cagrilarinin yerini alan tek merkez (Eren bey geri bildirimi,
// 2026-08-05 toplanti) - SweetAlert2 ile tutarli, DevExtreme temasiyla uyumlu bir gorunum.
@Service()
export class Bildirim {
  hata(mesaj: string): void {
    Swal.fire({
      icon: 'error',
      title: 'Hata',
      text: mesaj,
      confirmButtonText: 'Tamam',
      confirmButtonColor: '#dc2626',
    });
  }

  bilgi(mesaj: string): void {
    Swal.fire({
      icon: 'info',
      title: 'Bilgi',
      text: mesaj,
      confirmButtonText: 'Tamam',
      confirmButtonColor: '#2563eb',
    });
  }

  async onaylaSil(mesaj: string): Promise<boolean> {
    const sonuc = await Swal.fire({
      icon: 'warning',
      title: 'Emin misiniz?',
      text: mesaj,
      showCancelButton: true,
      confirmButtonText: 'Evet, sil',
      cancelButtonText: 'Vazgeç',
      confirmButtonColor: '#dc2626',
      cancelButtonColor: '#6b7280',
    });
    return sonuc.isConfirmed;
  }

  // onaylaSil'in genel (silme'ye ozel olmayan) hali - "Evet" / "Vazgec" butonlariyla.
  async onayla(mesaj: string): Promise<boolean> {
    const sonuc = await Swal.fire({
      icon: 'question',
      title: 'Onaylıyor musunuz?',
      text: mesaj,
      showCancelButton: true,
      confirmButtonText: 'Evet',
      cancelButtonText: 'Vazgeç',
      confirmButtonColor: '#2563eb',
      cancelButtonColor: '#6b7280',
    });
    return sonuc.isConfirmed;
  }
}
