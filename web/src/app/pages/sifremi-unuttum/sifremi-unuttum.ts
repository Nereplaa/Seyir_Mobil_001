import { Component, inject, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { DxTextBoxModule, DxButtonModule } from 'devextreme-angular';
import { TranslatePipe, TranslateService } from '@ngx-translate/core';
import { Auth } from '../../services/auth';
import { AuthArkaPlan } from '../../components/auth-arka-plan/auth-arka-plan';

@Component({
  selector: 'app-sifremi-unuttum',
  imports: [RouterLink, DxTextBoxModule, DxButtonModule, TranslatePipe, AuthArkaPlan],
  templateUrl: './sifremi-unuttum.html',
  styleUrl: './sifremi-unuttum.css',
})
export class SifremiUnuttum {
  private readonly auth = inject(Auth);
  private readonly translate = inject(TranslateService);

  email = signal('');
  hataMesaji = signal('');
  // Backend'den gelen yanit mesaji (yanit.message) artik backend'de de i18n'li (2026-08-12,
  // auth-interceptor Accept-Language header'ini gonderiyor) - kullanicinin secili uygulama
  // diline gore dogru dilde geliyor.
  sonucMesaji = signal('');
  gonderiliyor = signal(false);
  gonderildi = signal(false);

  gonder(): void {
    if (!this.email().trim() || !this.email().includes('@')) {
      this.hataMesaji.set(this.translate.instant('sifremiUnuttum.hataGecersizEmail'));
      return;
    }

    this.hataMesaji.set('');
    this.gonderiliyor.set(true);
    this.auth.sifremiUnuttum(this.email().trim()).subscribe({
      next: (yanit) => {
        this.gonderiliyor.set(false);
        this.gonderildi.set(true);
        this.sonucMesaji.set(yanit.message);
      },
      error: () => {
        this.gonderiliyor.set(false);
        this.hataMesaji.set(this.translate.instant('sifremiUnuttum.hataBaglanti'));
      },
    });
  }
}
