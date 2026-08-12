import { Component, inject, signal } from '@angular/core';
import { Router, RouterLink } from '@angular/router';
import { DxTextBoxModule, DxButtonModule } from 'devextreme-angular';
import { TranslatePipe, TranslateService } from '@ngx-translate/core';
import { Auth } from '../../services/auth';
import { rolBaslangicRotasi } from '../../utils/rol-yonlendirme';

@Component({
  selector: 'app-login',
  imports: [RouterLink, DxTextBoxModule, DxButtonModule, TranslatePipe],
  templateUrl: './login.html',
  styleUrl: './login.css',
})
export class Login {
  private readonly auth = inject(Auth);
  private readonly router = inject(Router);
  private readonly translate = inject(TranslateService);

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
