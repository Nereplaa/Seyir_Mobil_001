import { Component, OnInit, computed, inject, signal } from '@angular/core';
import {
  DxDateRangeBoxModule,
  DxTagBoxModule,
  DxCheckBoxModule,
  DxButtonModule,
  DxDataGridModule,
} from 'devextreme-angular';
import { exportDataGrid } from 'devextreme/excel_exporter';
import { Workbook } from 'exceljs';
import { AracHareketApi } from '../../services/arac-hareket-api';
import { Bildirim } from '../../services/bildirim';
import {
  AracPlakaLookupDto,
  AracRaporSonucuDto,
  AracHareketDetayRaporSatiriDto,
} from '../../models/arac-hareket.models';
import { dosyaIndir } from '../../utils/dosya-indir';
import { oturumHatasiMi } from '../../utils/hata-yardimcisi';

function bugunIso(): string {
  const d = new Date();
  const yerelGun = new Date(d.getTime() - d.getTimezoneOffset() * 60000);
  return yerelGun.toISOString().slice(0, 10);
}

function yarinIso(iso: string): string {
  const d = new Date(iso + 'T00:00:00');
  d.setDate(d.getDate() + 1);
  return d.toISOString().slice(0, 10);
}

// DevExtreme DateRangeBox Date nesneleriyle calisiyor, backend/rapor mantigi ise ISO tarih
// string'i ("yyyy-MM-dd") bekliyor - bu iki yardimci fonksiyon aradaki donusumu yapiyor.
// getFullYear/getMonth/getDate KULLANILIYOR (toISOString DEGIL) - toISOString once UTC'ye
// cevirir, yerel saat diliminde gece yarisina yakin secimlerde bir gun kayabilirdi.
function tarihToIso(d: Date): string {
  const y = d.getFullYear();
  const ay = String(d.getMonth() + 1).padStart(2, '0');
  const gun = String(d.getDate()).padStart(2, '0');
  return `${y}-${ay}-${gun}`;
}

function isoToTarih(iso: string): Date {
  return new Date(iso + 'T00:00:00');
}

@Component({
  selector: 'app-rapor',
  imports: [
    DxDateRangeBoxModule,
    DxTagBoxModule,
    DxCheckBoxModule,
    DxButtonModule,
    DxDataGridModule,
  ],
  templateUrl: './rapor.html',
  styleUrl: './rapor.css',
})
export class Rapor implements OnInit {
  private readonly api = inject(AracHareketApi);
  private readonly bildirim = inject(Bildirim);

  // ---------- DevExtreme DataGrid kolonlari (ozet + detayli rapor) ----------
  readonly ozetColumns = [
    {
      dataField: 'aracPlaka',
      caption: 'Araç Plakası',
      cellTemplate: (cellElement: HTMLElement, cellInfo: { value?: string }) => {
        const span = document.createElement('span');
        span.className = 'plaka-chip';
        span.textContent = cellInfo.value ?? '';
        cellElement.appendChild(span);
      },
    },
    {
      dataField: 'baslangicKm',
      caption: 'Başlangıç Km',
      cssClass: 'col-numeric',
      customizeText: (cellInfo: { value?: number | null }) =>
        cellInfo.value == null ? 'Veri yok' : this.formatKm(cellInfo.value),
    },
    {
      dataField: 'bitisKm',
      caption: 'Bitiş Km',
      cssClass: 'col-numeric',
      customizeText: (cellInfo: { value?: number | null }) =>
        cellInfo.value == null ? 'Veri yok' : this.formatKm(cellInfo.value),
    },
    {
      dataField: 'yapilanKm',
      caption: 'Yapılan Km',
      cssClass: 'col-numeric',
      customizeText: (cellInfo: { value?: number | null }) =>
        cellInfo.value == null ? 'Veri yok' : this.formatKm(cellInfo.value),
    },
  ];

  readonly detayColumns = [
    {
      dataField: 'aracPlaka',
      caption: 'Araç Plaka',
      cellTemplate: (cellElement: HTMLElement, cellInfo: { value?: string }) => {
        const span = document.createElement('span');
        span.className = 'plaka-chip';
        span.textContent = cellInfo.value ?? '';
        cellElement.appendChild(span);
      },
    },
    {
      dataField: 'veriTarihi',
      caption: 'Veri Tarihi',
      cssClass: 'col-numeric',
      customizeText: (cellInfo: { value?: string }) => (cellInfo.value ? this.formatTarih(cellInfo.value) : ''),
    },
    {
      dataField: 'kmSayaci',
      caption: 'Km Sayacı',
      cssClass: 'col-numeric',
      customizeText: (cellInfo: { value?: number }) => (cellInfo.value == null ? '' : this.formatKm(cellInfo.value)),
    },
    {
      dataField: 'artis',
      caption: 'Bir Önceki Okumaya Göre Artış',
      cssClass: 'col-numeric',
      customizeText: (cellInfo: { value?: number | null }) =>
        cellInfo.value == null ? '-' : this.formatKm(cellInfo.value),
    },
  ];

  plakalar = signal<AracPlakaLookupDto[]>([]);
  // Plaka secimi artik dx-tag-box'in KENDI arama+cip mekanizmasi ile yapiliyor - onceki elle
  // yazilmis arama kutusu/acilir liste/cip listesi (plakaArama, lookupAcik, lookupSonuclari,
  // lookupAc/Kapat, plakaSec/Kaldir) tamamen kaldirildi, DevExtreme zaten ayni ozelligi
  // (aranabilir coklu-secim + kaldirilabilir cipler) hazir sunuyor.
  seciliPlakalar = signal<string[]>([]);

  seciliPlakalarDegisti(event: { value?: string[] | null }): void {
    this.seciliPlakalar.set(event.value ?? []);
  }

  baslangic = bugunIso();
  bitis = yarinIso(bugunIso());
  detayliRapor = false;

  // dx-date-range-box'a baglanan Date alanlari - asil "kaynak" hala baslangic/bitis (ISO
  // string) alanlari, rapor mantigi/export/backend cagrilari hepsi bu string'leri kullaniyor.
  // BILINCLI OLARAK getter/setter DEGIL, sabit alan: bir getter her change-detection turunda
  // "new Date(...)" ile YENI bir nesne dondurseydi, DevExtreme bunu "deger degisti" sanip
  // widget'i surekli yeniden baslatiyordu (gercek bug, ekranda ust uste yigilan onlarca takvim
  // olarak ortaya cikti) - sabit alan + degisiklikleri SADECE kullanici etkilesiminde (event
  // handler'larda) guncelleme, referans kararliligini koruyor.
  baslangicDate: Date = isoToTarih(this.baslangic);
  bitisDate: Date = isoToTarih(this.bitis);

  baslangicDateDegisti(deger: string | number | Date | null): void {
    if (deger == null) {
      return;
    }
    const tarih = new Date(deger);
    this.baslangicDate = tarih;
    this.baslangic = tarihToIso(tarih);
    this.onBaslangicDegisti();
  }

  bitisDateDegisti(deger: string | number | Date | null): void {
    if (deger == null) {
      return;
    }
    const tarih = new Date(deger);
    this.bitisDate = tarih;
    this.bitis = tarihToIso(tarih);
  }

  ozetSonuclar = signal<AracRaporSonucuDto[]>([]);
  detaySonuclar = signal<AracHareketDetayRaporSatiriDto[]>([]);
  raporUretildi = signal(false);
  yukleniyor = signal(false);
  statusText = signal('');

  raporOlusturEtkin = computed(() => this.seciliPlakalar().length > 0 && this.bitis > this.baslangic);

  ngOnInit(): void {
    this.api.getPlakalar().subscribe({
      next: (plakalar) => this.plakalar.set(plakalar),
      error: (err) => {
        if (!oturumHatasiMi(err)) {
          this.bildirim.hata(`Araç listesi alınamadı.\n\nHata: ${err.message}`);
        }
      },
    });
  }

  onBaslangicDegisti(): void {
    const minBitis = yarinIso(this.baslangic);
    if (this.bitis < minBitis) {
      this.bitis = minBitis;
      this.bitisDate = isoToTarih(this.bitis);
    }
  }

  raporOlustur(): void {
    const plakalar = this.seciliPlakalar();
    this.yukleniyor.set(true);
    this.statusText.set('Rapor oluşturuluyor...');
    this.raporUretildi.set(false);

    if (this.detayliRapor) {
      this.api
        .getDetayRaporu({ plakalar, baslangic: this.baslangic, bitis: this.bitis })
        .subscribe({
          next: (satirlar) => {
            this.detaySonuclar.set(satirlar);
            this.raporUretildi.set(true);
            this.yukleniyor.set(false);
            this.statusText.set(`${satirlar.length} okuma için detaylı rapor oluşturuldu.`);
          },
          error: (err) => this.raporHatasi(err),
        });
    } else {
      this.api
        .getRaporToplu({ plakalar, baslangic: this.baslangic, bitis: this.bitis })
        .subscribe({
          next: (sonuclar) => {
            this.ozetSonuclar.set(sonuclar);
            this.raporUretildi.set(true);
            this.yukleniyor.set(false);
            this.statusText.set(`${sonuclar.length} araç için rapor oluşturuldu.`);
          },
          error: (err) => this.raporHatasi(err),
        });
    }
  }

  private raporHatasi(err: any): void {
    this.yukleniyor.set(false);
    if (oturumHatasiMi(err)) {
      return;
    }
    this.statusText.set('Rapor oluşturulamadı.');
    this.bildirim.hata(`Rapor oluşturulamadı.\n\nHata: ${err.message}`);
  }

  // ---------- Excel'e Aktar (DevExtreme dx-data-grid'in kendi export'u, tarayicida ureterek) ----------
  // Backend'e hic gitmiyor - eski ClosedXML "her plaka icin ayri bolum" ozel bicimlendirmesi
  // (exportModu secimi) bu yuzden kayboldu, DevExtreme grid export'u TEK duz tablo uretiyor
  // (kullanici onayli sadelesme, bkz. AI_NOTES/decisions.md).

  onExportingOzet(e: { component: unknown }): void {
    this.gridDenIndir(e, 'Özet Rapor', 'rapor-ozet.xlsx');
  }

  onExportingDetay(e: { component: unknown }): void {
    this.gridDenIndir(e, 'Detaylı Rapor', 'rapor-detay.xlsx');
  }

  private gridDenIndir(e: { component: unknown }, sayfaAdi: string, dosyaAdi: string): void {
    const workbook = new Workbook();
    const worksheet = workbook.addWorksheet(sayfaAdi);
    exportDataGrid({ component: e.component as never, worksheet }).then(() => {
      workbook.xlsx.writeBuffer().then((buffer) => {
        dosyaIndir(new Blob([buffer], { type: 'application/octet-stream' }), dosyaAdi);
      });
    });
  }

  formatTarih(iso: string | null): string {
    if (!iso) {
      return '-';
    }
    const [y, m, d] = iso.split('-');
    return `${d}.${m}.${y}`;
  }

  formatKm(km: number | null): string {
    return km == null ? '-' : km.toFixed(2);
  }
}
