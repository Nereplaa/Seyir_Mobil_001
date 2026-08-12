import { Component, inject, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { DxTextBoxModule, DxButtonModule } from 'devextreme-angular';
import { Auth } from '../../services/auth';

@Component({
  selector: 'app-sifremi-unuttum',
  imports: [RouterLink, DxTextBoxModule, DxButtonModule],
  templateUrl: './sifremi-unuttum.html',
  styleUrl: './sifremi-unuttum.css',
})
export class SifremiUnuttum {
  private readonly auth = inject(Auth);

  email = signal('');
  hataMesaji = signal('');
  sonucMesaji = signal('');
  gonderiliyor = signal(false);
  gonderildi = signal(false);

  gonder(): void {
    if (!this.email().trim() || !this.email().includes('@')) {
      this.hataMesaji.set('Geçerli bir e-posta adresi girin.');
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
        this.hataMesaji.set('İstek gönderilemedi. Backend API çalışıyor mu?');
      },
    });
  }
}
