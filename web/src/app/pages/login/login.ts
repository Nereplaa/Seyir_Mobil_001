import { Component, inject, signal } from '@angular/core';
import { Router, RouterLink } from '@angular/router';
import { DxTextBoxModule, DxButtonModule, DxSelectBoxModule } from 'devextreme-angular';
import { TranslatePipe, TranslateService } from '@ngx-translate/core';
import { Auth } from '../../services/auth';
import { Dil, DESTEKLENEN_DILLER } from '../../services/dil';
import { rolBaslangicRotasi } from '../../utils/rol-yonlendirme';

@Component({
  selector: 'app-login',
  imports: [RouterLink, DxTextBoxModule, DxButtonModule, DxSelectBoxModule, TranslatePipe],
  templateUrl: './login.html',
  styleUrl: './login.css',
})
export class Login {
  private readonly auth = inject(Auth);
  private readonly router = inject(Router);
  private readonly translate = inject(TranslateService);
  protected readonly dil = inject(Dil);
  protected readonly dilSecenekleri = DESTEKLENEN_DILLER;

  dilDegistir(kod: string): void {
    this.dil.degistir(kod);
  }

  username = signal('');
  password = signal('');
  hataMesaji = signal('');
  girisYapiliyor = signal(false);

  girisYap(): void {
    if (!this.username().trim() || !this.password()) {
      this.hataMesaji.set(this.translate.instant('login.hataEksikAlan'));
      return;
    }

    this.hataMesaji.set('');
    this.girisYapiliyor.set(true);
    this.auth.login(this.username().trim(), this.password()).subscribe({
      next: () => {
        this.girisYapiliyor.set(false);
        this.router.navigateByUrl(rolBaslangicRotasi(this.auth.role()));
      },
      error: (hata) => {
        this.girisYapiliyor.set(false);
        this.hataMesaji.set(
          this.translate.instant(hata.status === 401 ? 'login.hataYanlisBilgi' : 'login.hataBaglanti')
        );
      },
    });
  }
}
