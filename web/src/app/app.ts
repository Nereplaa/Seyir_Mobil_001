import { Component, HostListener, inject, signal } from '@angular/core';
import { Router, RouterOutlet, NavigationEnd } from '@angular/router';
import { filter } from 'rxjs';
import { DxButtonModule, DxSelectBoxModule, DxTabsModule } from 'devextreme-angular';
import { TranslatePipe, TranslateService } from '@ngx-translate/core';
import { Auth } from './services/auth';
import { EtkilesimLoglayici } from './services/etkilesim-loglayici';
import { Dil, DESTEKLENEN_DILLER } from './services/dil';

interface Sekme {
  text: string;
  yol: string;
}

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
  imports: [RouterOutlet, DxButtonModule, DxSelectBoxModule, DxTabsModule, TranslatePipe],
  templateUrl: './app.html',
  styleUrl: './app.css'
})
export class App {
  protected readonly auth = inject(Auth);
  protected readonly dil = inject(Dil);
  private readonly router = inject(Router);
  private readonly translate = inject(TranslateService);
  private readonly etkilesimLoglayici = inject(EtkilesimLoglayici);

  protected readonly dilSecenekleri = DESTEKLENEN_DILLER;

  // Ust menu (2026-08-18, roadmap: "ust menu/sekme yapisinin DevExtreme'e tasinmasi") - duz
  // <nav><a routerLink> yerine dx-tabs. Metinler ceviri icerdigi icin (admin-paneli.ts/liste.ts
  // grid kolonlarinda oldugu gibi) sabit dizi degil, dil degisince yeniden kurulan bir signal.
  protected readonly sekmeler = signal<Sekme[]>(this.sekmeleriKur());
  protected readonly secilenSekmeIndex = signal(this.sekmeIndexBul(this.router.url));

  private sekmeleriKur(): Sekme[] {
    const sekmeler: Sekme[] = [
      { text: this.translate.instant('nav.hareketler'), yol: '/' },
      { text: this.translate.instant('nav.rapor'), yol: '/rapor' },
      { text: this.translate.instant('nav.import'), yol: '/import' },
    ];
    if (this.auth.role() === 'Admin') {
      sekmeler.push({ text: this.translate.instant('nav.admin'), yol: '/admin' });
    }
    return sekmeler;
  }

  private sekmeIndexBul(url: string): number {
    const yol = url.split('?')[0];
    const index = this.sekmeler().findIndex((s) => s.yol === yol);
    return index === -1 ? 0 : index;
  }

  sekmeSecildi(event: { itemData?: Sekme }): void {
    if (event.itemData) {
      this.router.navigateByUrl(event.itemData.yol);
    }
  }

  // Login (ve aynı tam-sayfa split-screen kabuğunu paylaşan Şifremi Unuttum / Şifre Sıfırla)
  // ekranları KENDİ marka/rota panelini gösteriyor - üst header'ın orada da görünmesi çift/
  // gereksiz duruyordu (2026-08-07 geri bildirimi). auth durumuna göre gizlemek YETERSİZDİ:
  // kullanıcı token'ı hâlâ geçerliyken bu rotalara gelirse (guard bunu engellemiyor, zaten
  // hepsi auth gerektirmeyen public rotalar) hem header hem form aynı anda görünüyordu - bu
  // yüzden doğrudan ROTAYA göre (auth durumundan bağımsız) karar veriliyor.
  private readonly AUTH_ROTALARI = ['/login', '/sifremi-unuttum', '/sifre-sifirla'];
  protected readonly loginEkraninda = signal(this.authRotasindaMi(this.router.url));

  constructor() {
    this.dil.baslat();
    this.router.events.pipe(filter((e) => e instanceof NavigationEnd)).subscribe((e) => {
      const url = (e as NavigationEnd).urlAfterRedirects;
      this.loginEkraninda.set(this.authRotasindaMi(url));
      // Rol, login sonrasi ilk NavigationEnd'e kadar henuz belli olmayabilir (bkz. Admin
      // sekmesinin auth.role()'e bagli olmasi) - bu yuzden sekmeler her rota degisiminde de
      // yeniden kuruluyor, sadece dil degisiminde degil.
      this.sekmeler.set(this.sekmeleriKur());
      this.secilenSekmeIndex.set(this.sekmeIndexBul(url));
    });
    this.translate.onLangChange.subscribe(() => this.sekmeler.set(this.sekmeleriKur()));
  }

  private authRotasindaMi(url: string): boolean {
    return this.AUTH_ROTALARI.some((rota) => url.startsWith(rota));
  }

  dilDegistir(kod: string): void {
    this.dil.degistir(kod);
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
