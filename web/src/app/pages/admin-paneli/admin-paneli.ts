import { Component, OnInit, inject, signal } from '@angular/core';
import { DxDataGridModule, DxTextBoxModule, DxSelectBoxModule, DxButtonModule } from 'devextreme-angular';
import { UserApi } from '../../services/user-api';
import { Auth } from '../../services/auth';
import { Bildirim } from '../../services/bildirim';
import { UserSummaryDto } from '../../models/user.models';
import { oturumHatasiMi } from '../../utils/hata-yardimcisi';

@Component({
  selector: 'app-admin-paneli',
  imports: [DxDataGridModule, DxTextBoxModule, DxSelectBoxModule, DxButtonModule],
  templateUrl: './admin-paneli.html',
  styleUrl: './admin-paneli.css',
})
export class AdminPaneli implements OnInit {
  private readonly api = inject(UserApi);
  private readonly auth = inject(Auth);
  private readonly bildirim = inject(Bildirim);

  readonly rolSecenekleri = ['Admin', 'Viewer'];

  readonly dataGridColumns = [
    { dataField: 'username', caption: 'Kullanıcı Adı' },
    { dataField: 'role', caption: 'Rol', width: 120 },
    {
      dataField: 'olusturmaTarihi',
      caption: 'Oluşturma Tarihi',
      width: 170,
      cssClass: 'col-numeric',
      customizeText: (cellInfo: { value?: string }) => (cellInfo.value ? this.formatTarih(cellInfo.value) : ''),
    },
  ];

  kullanicilar = signal<UserSummaryDto[]>([]);
  seciliKullanici = signal<UserSummaryDto | null>(null);
  statusText = signal('');

  yeniKullaniciAdi = '';
  yeniSifre = '';
  yeniRol = 'Viewer';
  ekleniyor = signal(false);

  ngOnInit(): void {
    this.yukle();
  }

  private yukle(): void {
    this.statusText.set('Yükleniyor...');
    this.api.getKullanicilar().subscribe({
      next: (kullanicilar) => {
        this.kullanicilar.set(kullanicilar);
        this.statusText.set(`${kullanicilar.length} kullanıcı.`);
      },
      error: (err) => {
        if (oturumHatasiMi(err)) {
          return;
        }
        this.statusText.set('Kullanıcılar alınamadı.');
        this.bildirim.hata(`Kullanıcılar alınamadı.\n\nHata: ${err.message}`);
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
      this.bildirim.hata('Kullanıcı adı boş olamaz.');
      return;
    }
    if (this.yeniSifre.length < 6) {
      this.bildirim.hata('Şifre en az 6 karakter olmalı.');
      return;
    }

    this.ekleniyor.set(true);
    this.api
      .createKullanici({ username: this.yeniKullaniciAdi.trim(), password: this.yeniSifre, role: this.yeniRol })
      .subscribe({
        next: () => {
          this.ekleniyor.set(false);
          this.yeniKullaniciAdi = '';
          this.yeniSifre = '';
          this.yeniRol = 'Viewer';
          this.yukle();
        },
        error: (err) => {
          this.ekleniyor.set(false);
          this.bildirim.hata(err.error?.message ?? `Kullanıcı eklenemedi.\n\nHata: ${err.message}`);
        },
      });
  }

  async sil(): Promise<void> {
    const secili = this.seciliKullanici();
    if (!secili || this.kendisiSecilmiMi) {
      return;
    }

    const onay = await this.bildirim.onaylaSil(`"${secili.username}" kullanıcısını silmek istediğine emin misin?`);
    if (!onay) {
      return;
    }

    this.api.deleteKullanici(secili.id).subscribe({
      next: () => {
        this.seciliKullanici.set(null);
        this.yukle();
      },
      error: (err) => {
        this.bildirim.hata(err.error?.message ?? `Kullanıcı silinemedi.\n\nHata: ${err.message}`);
      },
    });
  }

  formatTarih(iso: string): string {
    const d = new Date(iso);
    const tarih = d.toLocaleDateString('tr-TR');
    const saat = d.toLocaleTimeString('tr-TR', { hour: '2-digit', minute: '2-digit' });
    return `${tarih} ${saat}`;
  }
}
