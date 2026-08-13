import { Component, inject } from '@angular/core';
import { DxSelectBoxModule } from 'devextreme-angular';
import { TranslatePipe } from '@ngx-translate/core';
import { Dil, DESTEKLENEN_DILLER } from '../../services/dil';

// 2026-08-13: login/şifremi-unuttum/şifre-sıfırla ekranlarının ortak "sahnesi" (koyu tam ekran
// zemin, dil seçici, pusula+marka yazısı, çember siluetli rota ağı, telemetri) tek bir yerde
// topluyor - önceden bu ~150 satırlık SVG bloğu 3 sayfada BİREBİR kopyalanıyordu (özellikle rota
// ağının koordinatları defalarca elden geçtiği için senkron tutmak kırılgandı). Sayfaya özgü
// form içeriği <ng-content> ile projekte ediliyor - .login-form-flat (konum/kutu stili) BU
// bileşende tanımlı DEĞİL, her sayfa kendi CSS'inde tanımlıyor (Angular'ın view encapsulation'ı
// projekte edilen içeriğe üst bileşenden CSS descendant selector'ı ULAŞTIRMIYOR - bu yüzden
// kutunun KENDİSİ her sayfada küçük bir kopya olarak kalıyor, ama devasa SVG tek kaynaklı oldu).
@Component({
  selector: 'app-auth-arka-plan',
  imports: [DxSelectBoxModule, TranslatePipe],
  templateUrl: './auth-arka-plan.html',
  styleUrl: './auth-arka-plan.css',
})
export class AuthArkaPlan {
  protected readonly dil = inject(Dil);
  protected readonly dilSecenekleri = DESTEKLENEN_DILLER;

  dilDegistir(kod: string): void {
    this.dil.degistir(kod);
  }
}
