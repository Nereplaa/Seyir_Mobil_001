import { Component, OnInit, inject, signal } from '@angular/core';
import { DxDataGridModule, DxTextBoxModule, DxSelectBoxModule, DxButtonModule } from 'devextreme-angular';
import { TranslatePipe, TranslateService } from '@ngx-translate/core';
import { UserApi } from '../../services/user-api';
import { Auth } from '../../services/auth';
import { Bildirim } from '../../services/bildirim';
import { UserSummaryDto } from '../../models/user.models';
import { oturumHatasiMi } from '../../utils/hata-yardimcisi';

@Component({
  selector: 'app-admin-paneli',
  imports: [DxDataGridModule, DxTextBoxModule, DxSelectBoxModule, DxButtonModule, TranslatePipe],
  templateUrl: './admin-paneli.html',
  styleUrl: './admin-paneli.css',
})
export class AdminPaneli implements OnInit {
  private readonly api = inject(UserApi);
  private readonly auth = inject(Auth);
  private readonly bildirim = inject(Bildirim);
  private readonly translate = inject(TranslateService);

  readonly rolSecenekleri = ['Admin', 'Viewer'];

  // Kolon basliklari ceviri iceriyor - dil degisince yeniden kurulmasi gerekiyor (bkz.
  // constructor'daki onLangChange abonesi), bu yuzden sabit dizi degil signal.
  readonly dataGridColumns = signal(this.kolonlariKur());

  private kolonlariKur() {
    return [
      { dataField: 'username', caption: this.translate.instant('admin.kolonKullaniciAdi') },
      { dataField: 'email', caption: this.translate.instant('admin.kolonEmail') },
      { dataField: 'role', caption: this.translate.instant('admin.kolonRol'), width: 120 },
      {
        dataField: 'olusturmaTarihi',
        caption: this.translate.instant('admin.kolonOlusturmaTarihi'),
        width: 170,
        cssClass: 'col-numeric',
        customizeText: (cellInfo: { value?: string }) => (cellInfo.value ? this.formatTarih(cellInfo.value) : ''),
      },
    ];
  }

  kullanicilar = signal<UserSummaryDto[]>([]);
  seciliKullanici = signal<UserSummaryDto | null>(null);
  statusText = signal('');

  yeniKullaniciAdi = '';
  yeniSifre = '';
  yeniRol = 'Viewer';
  yeniEmail = '';
  ekleniyor = signal(false);

  constructor() {
    this.translate.onLangChange.subscribe(() => this.dataGridColumns.set(this.kolonlariKur()));
  }

  ngOnInit(): void {
    this.yukle();
  }

  private yukle(): void {
    this.statusText.set(this.translate.instant('admin.yukleniyor'));
    this.api.getKullanicilar().subscribe({
      next: (kullanicilar) => {
        this.kullanicilar.set(kullanicilar);
        this.statusText.set(this.translate.instant('admin.kullaniciSayisi', { sayi: kullanicilar.length }));
      },
      error: (err) => {
        if (oturumHatasiMi(err)) {
          return;
        }
        this.statusText.set(this.translate.instant('admin.kullanicilarAlinamadi'));
        this.bildirim.hata(`${this.translate.instant('admin.kullanicilarAlinamadi')}\n\nHata: ${err.message}`);
      },
    });
  }

  onSelectionChanged(event: { selectedRowsData: UserSummaryDto[] }): void {
    this.seciliKullanici.set(event.selectedRowsData[0] ?? null);
  }

  // Kendi hesabini silme (backend'de bu kontrol yok, self-lockout riskini istemci
  // tarafinda onlemek icin ucuz/basit bir guvenlik onlemi).
  get kendisiSecilmiMi(): boolean {
    const secili = this.seciliKullanici();
    return !!secili && secili.username === this.auth.username();
  }

  ekle(): void {
    if (!this.yeniKullaniciAdi.trim()) {
      this.bildirim.hata(this.translate.instant('admin.hataKullaniciAdiBos'));
      return;
    }
    if (this.yeniSifre.length < 6) {
      this.bildirim.hata(this.translate.instant('admin.hataKisaSifre'));
      return;
    }
    if (!this.yeniEmail.trim() || !this.yeniEmail.includes('@')) {
      this.bildirim.hata(this.translate.instant('admin.hataGecersizEmail'));
      return;
    }

    this.ekleniyor.set(true);
    this.api
      .createKullanici({
        username: this.yeniKullaniciAdi.trim(),
        password: this.yeniSifre,
        role: this.yeniRol,
        email: this.yeniEmail.trim(),
      })
      .subscribe({
        next: () => {
          this.ekleniyor.set(false);
          this.yeniKullaniciAdi = '';
          this.yeniSifre = '';
          this.yeniRol = 'Viewer';
          this.yeniEmail = '';
          this.yukle();
        },
        error: (err) => {
          this.ekleniyor.set(false);
          this.bildirim.hata(
            err.error?.message ?? `${this.translate.instant('admin.hataKullaniciEklenemedi')}\n\nHata: ${err.message}`
          );
        },
      });
  }

  async sil(): Promise<void> {
    const secili = this.seciliKullanici();
    if (!secili || this.kendisiSecilmiMi) {
      return;
    }

    const onay = await this.bildirim.onaylaSil(
      this.translate.instant('admin.silOnaySorusu', { kullaniciAdi: secili.username })
    );
    if (!onay) {
      return;
    }

    this.api.deleteKullanici(secili.id).subscribe({
      next: () => {
        this.seciliKullanici.set(null);
        this.yukle();
      },
      error: (err) => {
        this.bildirim.hata(
          err.error?.message ?? `${this.translate.instant('admin.hataKullaniciSilinemedi')}\n\nHata: ${err.message}`
        );
      },
    });
  }

  formatTarih(iso: string): string {
    const yerelAyar = this.translate.currentLang() === 'en' ? 'en-US' : 'tr-TR';
    const d = new Date(iso);
    const tarih = d.toLocaleDateString(yerelAyar);
    const saat = d.toLocaleTimeString(yerelAyar, { hour: '2-digit', minute: '2-digit' });
    return `${tarih} ${saat}`;
  }
}
