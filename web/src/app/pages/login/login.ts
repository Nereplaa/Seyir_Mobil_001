import { Component, inject, signal } from '@angular/core';
import { Router } from '@angular/router';
import { DxTextBoxModule, DxButtonModule } from 'devextreme-angular';
import { Auth } from '../../services/auth';
import { rolBaslangicRotasi } from '../../utils/rol-yonlendirme';

@Component({
  selector: 'app-login',
  imports: [DxTextBoxModule, DxButtonModule],
  templateUrl: './login.html',
  styleUrl: './login.css',
})
export class Login {
  private readonly auth = inject(Auth);
  private readonly router = inject(Router);

  username = signal('');
  password = signal('');
  hataMesaji = signal('');
  girisYapiliyor = signal(false);

  girisYap(): void {
    if (!this.username().trim() || !this.password()) {
      this.hataMesaji.set('Kullanıcı adı ve şifre gerekli.');
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
          hata.status === 401
            ? 'Kullanıcı adı veya şifre hatalı.'
            : 'Giriş yapılamadı. Backend API çalışıyor mu?'
        );
      },
    });
  }
}
