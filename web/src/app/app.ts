import { Component, HostListener, inject, signal } from '@angular/core';
import { Router, RouterOutlet, RouterLink, RouterLinkActive, NavigationEnd } from '@angular/router';
import { filter } from 'rxjs';
import { DxButtonModule } from 'devextreme-angular';
import { Auth } from './services/auth';
import { EtkilesimLoglayici } from './services/etkilesim-loglayici';

// Interaktif kabul edilen elemanlar - DevExtreme kendi bilesenlerini bu class'larla
// render ediyor (dx-button, checkbox, calendar hucresi, liste/dropdown ogesi vb.), duz
// HTML button/a/role="button" da dahil - boylece "her tiklama" hem native hem DevExtreme
// elemanlari icin TEK bir global dinleyiciyle yakalaniyor, her component'e ayri ayri
// tiklama loglama kodu eklemeye gerek kalmiyor.
const INTERAKTIF_SECICI =
  'button, a, [role="button"], .dx-button, .dx-checkbox, .dx-calendar-cell, ' +
  '.dx-list-item, .dx-item, input, .dx-selectbox, .dx-datebox';

@Component({
  selector: 'app-root',
  imports: [RouterOutlet, RouterLink, RouterLinkActive, DxButtonModule],
  templateUrl: './app.html',
  styleUrl: './app.css'
})
export class App {
  protected readonly auth = inject(Auth);
  private readonly router = inject(Router);
  private readonly etkilesimLoglayici = inject(EtkilesimLoglayici);

  // Login ekranı KENDİ tam-sayfa marka/rota panelini gösteriyor - üst header'ın (marka şeridi +
  // nav) orada da görünmesi çift/gereksiz duruyordu (2026-08-07 geri bildirimi). auth durumuna
  // göre gizlemek YETERSİZDİ: kullanıcı token'ı hâlâ geçerliyken /login'e gelirse (guard bunu
  // engellemiyor) hem header hem login formu aynı anda görünüyordu - bu yüzden doğrudan ROTAYA
  // göre (yalnızca /login'de gizli) karar veriliyor, auth durumundan bağımsız.
  protected readonly loginEkraninda = signal(this.router.url.startsWith('/login'));

  constructor() {
    this.router.events.pipe(filter((e) => e instanceof NavigationEnd)).subscribe((e) => {
      this.loginEkraninda.set((e as NavigationEnd).urlAfterRedirects.startsWith('/login'));
    });
  }

  cikisYap(): void {
    this.auth.logout();
    this.router.navigateByUrl('/login');
  }

  // Madde 5 (Eren bey 2. geri bildirimi): "frontend'deki her tiklama loglanacak" karari
  // buradan uygulaniyor - tek bir document-level dinleyici, tum sayfalardaki/bilesenlerdeki
  // butonlari/linkleri/DevExtreme widget'larini tek tek isaretlemeye gerek kalmadan yakalar.
  @HostListener('document:click', ['$event'])
  onDocumentClick(event: MouseEvent): void {
    const hedef = event.target as HTMLElement | null;
    const interaktifEleman = hedef?.closest(INTERAKTIF_SECICI) as HTMLElement | null;
    if (!interaktifEleman) {
      return;
    }
    this.etkilesimLoglayici.logla('tiklama', this.etiketCikar(interaktifEleman));
  }

  private etiketCikar(el: HTMLElement): string {
    const metin = el.textContent?.trim().replace(/\s+/g, ' ').slice(0, 80);
    if (metin) {
      return `${el.tagName.toLowerCase()}: ${metin}`;
    }
    const aria = el.getAttribute('aria-label') ?? el.getAttribute('title');
    return aria ? `${el.tagName.toLowerCase()}: ${aria}` : el.tagName.toLowerCase();
  }
}
