import { Component, OnInit, inject, signal } from '@angular/core';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { DxTextBoxModule, DxButtonModule } from 'devextreme-angular';
import { Auth } from '../../services/auth';

@Component({
  selector: 'app-sifre-sifirla',
  imports: [RouterLink, DxTextBoxModule, DxButtonModule],
  templateUrl: './sifre-sifirla.html',
  styleUrl: './sifre-sifirla.css',
})
export class SifreSifirla implements OnInit {
  private readonly route = inject(ActivatedRoute);
  private readonly auth = inject(Auth);

  token = '';
  yeniSifre = signal('');
  yeniSifreTekrar = signal('');
  hataMesaji = signal('');
  gonderiliyor = signal(false);
  basariliMi = signal(false);

  ngOnInit(): void {
    this.token = this.route.snapshot.queryParamMap.get('token') ?? '';
    if (!this.token) {
      this.hataMesaji.set('Geçersiz sıfırlama bağlantısı. Yeniden "Şifremi Unuttum" isteğinde bulun.');
    }
  }

  sifirla(): void {
    if (!this.token) {
      return;
    }
    if (this.yeniSifre().length < 6) {
      this.hataMesaji.set('Şifre en az 6 karakter olmalı.');
      return;
    }
    if (this.yeniSifre() !== this.yeniSifreTekrar()) {
      this.hataMesaji.set('Şifreler eşleşmiyor.');
      return;
    }

    this.hataMesaji.set('');
    this.gonderiliyor.set(true);
    this.auth.sifreSifirla(this.token, this.yeniSifre()).subscribe({
      next: () => {
        this.gonderiliyor.set(false);
        this.basariliMi.set(true);
      },
      error: (err) => {
        this.gonderiliyor.set(false);
        this.hataMesaji.set(err.error?.message ?? 'Şifre sıfırlanamadı. Backend API çalışıyor mu?');
      },
    });
  }
}
