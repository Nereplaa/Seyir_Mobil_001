import {
  Component,
  ElementRef,
  OnDestroy,
  OnInit,
  ViewChild,
  computed,
  inject,
  signal,
} from '@angular/core';
import { Router } from '@angular/router';
import {
  DxDataGridModule,
  DxSelectBoxModule,
  DxDateBoxModule,
  DxDateRangeBoxModule,
  DxTagBoxModule,
  DxNumberBoxModule,
  DxCheckBoxModule,
  DxButtonModule,
} from 'devextreme-angular';
import { TranslatePipe, TranslateService } from '@ngx-translate/core';
import { exportDataGrid } from 'devextreme/excel_exporter';
import { Workbook } from 'exceljs';
import { AracHareketApi } from '../../services/arac-hareket-api';
import { Bildirim } from '../../services/bildirim';
import { ImportDosyaKoprusu } from '../../services/import-dosya-koprusu';
import {
  AracHareketDto,
  AracPlakaLookupDto,
  AracHareketSinirlarDto,
} from '../../models/arac-hareket.models';
import { dosyaIndir } from '../../utils/dosya-indir';
import { oturumHatasiMi } from '../../utils/hata-yardimcisi';

function bugunIso(): string {
  const d = new Date();
  const yerelGun = new Date(d.getTime() - d.getTimezoneOffset() * 60000);
  return yerelGun.toISOString().slice(0, 10);
}

// DevExtreme tarih bilesenleri (dx-date-box) Date nesneleriyle calisiyor, backend/filtre
// mantigi ise ISO tarih string'i ("yyyy-MM-dd") bekliyor - donusum burada yapiliyor.
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
  selector: 'app-liste',
  imports: [
    DxDataGridModule,
    DxSelectBoxModule,
    DxDateBoxModule,
    DxDateRangeBoxModule,
    DxTagBoxModule,
    DxNumberBoxModule,
    DxCheckBoxModule,
    DxButtonModule,
    TranslatePipe,
  ],
  templateUrl: './liste.html',
  styleUrl: './liste.css',
})
export class Liste implements OnInit, OnDestroy {
  private readonly api = inject(AracHareketApi);
  private readonly router = inject(Router);
  private readonly importDosyaKoprusu = inject(ImportDosyaKoprusu);
  private readonly bildirim = inject(Bildirim);
  private readonly translate = inject(TranslateService);

  // Plaka çip taşma alanının sağ kenarını Km Sayacı'nın sağ kenarıyla hizalamak için Plaka'nın
  // sol kenarı - Km Sayacı'nın sağ kenarı arasındaki GERÇEK mesafeyi ölçüyoruz (Rapor sayfasındaki
  // AYNI teknik, bkz. rapor.ts yorumu - sabit piksel toplamı ("770") DENENDİ ama gerçek render
  // genişliğiyle tam örtüşmüyordu, artık DOM'dan doğrudan ölçülüyor).
  @ViewChild('filtrePlakaAlan') private filtrePlakaAlanRef?: ElementRef<HTMLElement>;
  @ViewChild('filtreKmAlan') private filtreKmAlanRef?: ElementRef<HTMLElement>;
  private resizeObserver?: ResizeObserver;
  plakaTasmaGenislik = signal<number | null>(null);

  ngAfterViewInit(): void {
    const plakaEl = this.filtrePlakaAlanRef?.nativeElement;
    const kmEl = this.filtreKmAlanRef?.nativeElement;
    if (!plakaEl || !kmEl) {
      return;
    }
    this.resizeObserver = new ResizeObserver(() => this.plakaTasmaGenisliginiOlc());
    this.resizeObserver.observe(plakaEl);
    this.resizeObserver.observe(kmEl);
    this.plakaTasmaGenisliginiOlc();
  }

  ngOnDestroy(): void {
    this.resizeObserver?.disconnect();
  }

  private plakaTasmaGenisliginiOlc(): void {
    const plakaEl = this.filtrePlakaAlanRef?.nativeElement;
    const kmEl = this.filtreKmAlanRef?.nativeElement;
    if (!plakaEl || !kmEl) {
      return;
    }
    const solKenar = plakaEl.getBoundingClientRect().left;
    const sagKenar = kmEl.getBoundingClientRect().right;
    this.plakaTasmaGenislik.set(Math.round(sagKenar - solKenar));
  }

  // ---------- Ana liste ----------
  tumHareketler = signal<AracHareketDto[]>([]);
  gosterilenHareketler = signal<AracHareketDto[]>([]);
  seciliHareket = signal<AracHareketDto | null>(null);
  statusText = signal('');

  // ---------- DevExtreme DataGrid ----------
  // Sayfalama/siralama/arama artik dx-data-grid'in KENDI ic mekanizmasi tarafindan
  // yonetiliyor - elle yazilmis sayfa/slice mantigina gerek kalmadi. gosterilenHareketler()
  // (filtreli TAM liste) dogrudan dataSource olarak veriliyor, grid sadece GORUNUMU sayfaliyor.
  readonly sayfaBoyutuSecenekleri = [10, 25, 50, 100];
  readonly ilkSayfaBoyutu = 25;

  // Kolon basliklari ceviri iceriyor - dil degisince yeniden kurulmasi gerekiyor (bkz.
  // constructor'daki onLangChange abonesi), bu yuzden sabit dizi degil signal.
  readonly dataGridColumns = signal(this.kolonlariKur());

  private kolonlariKur() {
    return [
      { dataField: 'aracId', caption: this.translate.instant('liste.kolonAracId'), width: 115, alignment: 'left' as const, cssClass: 'col-numeric' },
      {
        dataField: 'aracPlaka',
        caption: this.translate.instant('liste.kolonAracPlaka'),
        width: 150,
        cellTemplate: (cellElement: HTMLElement, cellInfo: { value?: string }) => {
          const span = document.createElement('span');
          span.className = 'plaka-chip';
          span.textContent = cellInfo.value ?? '';
          cellElement.appendChild(span);
        },
      },
      {
        dataField: 'veriTarihi',
        caption: this.translate.instant('liste.kolonVeriTarihi'),
        width: 130,
        cssClass: 'col-numeric',
        calculateSortValue: 'veriTarihi',
        customizeText: (cellInfo: { value?: string }) => (cellInfo.value ? this.formatTarih(cellInfo.value) : ''),
      },
      { dataField: 'hiz', caption: this.translate.instant('liste.kolonHiz'), width: 90, alignment: 'left' as const, cssClass: 'col-numeric' },
      { dataField: 'kmSayaci', caption: this.translate.instant('liste.kolonKmSayaci'), format: '#,##0.00', cssClass: 'col-numeric' },
    ];
  }

  constructor() {
    this.translate.onLangChange.subscribe(() => this.dataGridColumns.set(this.kolonlariKur()));
  }

  onSelectionChanged(event: { selectedRowsData: AracHareketDto[] }): void {
    this.seciliHareket.set(event.selectedRowsData[0] ?? null);
  }

  // ---------- Filtre şeridi ----------
  // Plaka: dx-tag-box ile birden fazla plaka birden secilebiliyor (bos secim = "Tumu",
  // Rapor sayfasindaki AYNI desen). Tarih: tek gun yerine dx-date-range-box ile araliktir.
  filtrePlakaListesi = signal<string[]>([]);
  filtreSeciliPlakalar = signal<string[]>([]);
  filtreSeciliPlakalarDegisti(event: { value?: string[] | null }): void {
    this.filtreSeciliPlakalar.set(event.value ?? []);
  }

  // dx-tag-box'ın kendi çipleri gizlendiği için (bkz. liste.html/css), seçili plakalar ayrı bir
  // taşan alanda gösteriliyor - 3 satıra sığacak şekilde EN FAZLA 20 "bubble" (2026-08-10 geri
  // bildirimi). 20'ye kadar HEPSİ plaka olarak gösteriliyor; 20'den fazlaysa son bubble'ı
  // "+N daha" özetine ayırmak için 19 plaka + 1 özet bubble'ı = yine toplam 20'yi geçmiyor.
  private readonly plakaCipTavani = 20;
  readonly plakaCipGorunumu = computed(() => {
    const secili = this.filtreSeciliPlakalar();
    if (secili.length <= this.plakaCipTavani) {
      return { gosterilenler: secili, fazlaSayisi: 0 };
    }
    const gosterilecekSayi = this.plakaCipTavani - 1;
    return {
      gosterilenler: secili.slice(0, gosterilecekSayi),
      fazlaSayisi: secili.length - gosterilecekSayi,
    };
  });

  plakaCipiKaldir(plaka: string): void {
    this.filtreSeciliPlakalar.update((arr) => arr.filter((p) => p !== plaka));
  }
  filtreTarihAktif = false;
  filtreBaslangicDate: Date = isoToTarih(bugunIso());
  filtreBitisDate: Date = isoToTarih(bugunIso());

  // dx-date-range-box'in (start/end)DateChange event'i string|number|Date donduruyor (Rapor
  // sayfasindaki AYNI durum) - alan tipi Date oldugu icin dogrudan atanamiyor, donusum burada.
  filtreBaslangicDateDegisti(deger: string | number | Date | null): void {
    if (deger == null) {
      return;
    }
    this.filtreBaslangicDate = new Date(deger);
  }

  filtreBitisDateDegisti(deger: string | number | Date | null): void {
    if (deger == null) {
      return;
    }
    this.filtreBitisDate = new Date(deger);
  }
  filtreHiz: number | null = null;
  filtreKm: number | null = null;
  filtreHatasi = signal('');

  // ---------- Ekleme sihirbazı ----------
  plakalar = signal<AracPlakaLookupDto[]>([]);
  wizardAracId: number | null = null;
  wizardTarihDate: Date = isoToTarih(bugunIso());
  tarihOnaylandi = signal(false);
  sinirlarHesaplaniyor = signal(false);
  sinirBilgisi = signal('');
  wizardHiz = 0;
  wizardKm = 0;
  kmMin = signal(0);
  kmMax = signal(99999999.99);
  ekleEtkin = signal(false);
  ekleniyor = signal(false);

  private sinirSorgusuSurumu = 0;

  ngOnInit(): void {
    this.refreshGrid();
    this.plakalarYukle();
  }

  // ---------- Ana liste ----------

  refreshGrid(): void {
    this.statusText.set(this.translate.instant('liste.yukleniyor'));
    this.api.getTumHareketler().subscribe({
      next: (hareketler) => {
        this.tumHareketler.set(hareketler);
        this.filtrePlakaListesiniDoldur(hareketler);

        // Aktif bir filtre varsa (filtre kriterleri varsayilan degilse) yeni veriye de AYNI
        // filtreyi tekrar uygula - "Yenile" filtreyi sifirlamamali, sadece veriyi tazelemeli.
        if (this.filtreAktifMi()) {
          const sonuc = this.filtreleUygulaKriterleri(hareketler);
          this.gosterilenHareketler.set(sonuc);
          this.statusText.set(
            this.translate.instant('liste.kayitGosteriliyorFiltreli', { gosterilen: sonuc.length, toplam: hareketler.length })
          );
        } else {
          this.gosterilenHareketler.set(hareketler);
          this.statusText.set(this.translate.instant('liste.hareketKaydiYuklendi', { sayi: hareketler.length }));
        }
      },
      error: (err) => {
        if (oturumHatasiMi(err)) {
          return;
        }
        this.statusText.set(this.translate.instant('liste.veriYuklenemedi'));
        this.bildirim.hata(`${this.translate.instant('liste.hataHareketlerAlinamadi')}\n\nHata: ${err.message}`);
      },
    });
  }

  async sil(): Promise<void> {
    const secili = this.seciliHareket();
    if (!secili) {
      return;
    }
    const onay = await this.bildirim.onaylaSil(
      this.translate.instant('liste.silOnaySorusu', {
        plaka: secili.aracPlaka,
        tarih: this.formatTarih(secili.veriTarihi),
      })
    );
    if (!onay) {
      return;
    }

    this.statusText.set(this.translate.instant('liste.siliniyor'));
    this.api.deleteHareket(secili.id).subscribe({
      next: () => {
        this.seciliHareket.set(null);
        this.refreshGrid();
      },
      error: (err) => {
        if (oturumHatasiMi(err)) {
          return;
        }
        this.statusText.set(this.translate.instant('liste.silmeBasarisiz'));
        this.bildirim.hata(`${this.translate.instant('liste.hataKayitSilinemedi')}\n\nHata: ${err.message}`);
      },
    });
  }

  // ---------- Ekleme sihirbazı ----------

  private plakalarYukle(): void {
    this.api.getPlakalar().subscribe({
      next: (plakalar) => this.plakalar.set(plakalar),
      error: (err) => {
        if (!oturumHatasiMi(err)) {
          this.bildirim.hata(`${this.translate.instant('liste.hataPlakaListesiAlinamadi')}\n\nHata: ${err.message}`);
        }
      },
    });
  }

  wizardPlakaDegisti(event: { value?: number | null }): void {
    this.wizardAracId = event.value ?? null;
    this.onPlakaSecildi();
  }

  onPlakaSecildi(): void {
    this.sonrakiAdimlariSifirla();
    if (this.wizardAracId != null) {
      this.wizardTarihDate = isoToTarih(bugunIso());
    }
  }

  wizardTarihDegisti(event: { value?: Date | null }): void {
    if (!event.value) {
      return;
    }
    this.wizardTarihDate = event.value;
    this.onTarihDegisti();
  }

  onTarihDegisti(): void {
    this.sonrakiAdimlariSifirla();
  }

  private sonrakiAdimlariSifirla(): void {
    this.tarihOnaylandi.set(false);
    this.ekleEtkin.set(false);
    this.sinirBilgisi.set('');
  }

  get seciliAracPlaka(): string | null {
    const arac = this.plakalar().find((p) => p.aracId === this.wizardAracId);
    return arac ? arac.aracPlaka : null;
  }

  tarihiOnayla(): void {
    this.guncelleSinirlarVeAdimlari();
  }

  private guncelleSinirlarVeAdimlari(): void {
    const plaka = this.seciliAracPlaka;
    if (!plaka) {
      return;
    }

    const buSurum = ++this.sinirSorgusuSurumu;
    this.sinirlarHesaplaniyor.set(true);
    this.statusText.set(this.translate.instant('liste.sinirlarHesaplaniyor'));

    this.api.getSinirlar(plaka, tarihToIso(this.wizardTarihDate)).subscribe({
      next: (sinirlar: AracHareketSinirlarDto) => {
        if (buSurum !== this.sinirSorgusuSurumu) {
          return;
        }
        this.sinirlarHesaplaniyor.set(false);

        if (sinirlar.ayniTarihVarMi) {
          this.statusText.set(this.translate.instant('liste.ayniTarihKaydiVar'));
          return;
        }

        const min = sinirlar.oncekiKm != null ? sinirlar.oncekiKm + 0.01 : 0;
        const max = sinirlar.sonrakiKm != null ? sinirlar.sonrakiKm - 0.01 : 99999999.99;

        if (min > max) {
          this.statusText.set(this.translate.instant('liste.gecerliKmAraligiYok'));
          return;
        }

        this.kmMin.set(min);
        this.kmMax.set(max);
        this.wizardKm = Math.round(min * 100) / 100;
        this.wizardHiz = 0;

        const oncekiMetin = sinirlar.oncekiTarih
          ? this.translate.instant('liste.oncekiMetin', {
              tarih: this.formatTarih(sinirlar.oncekiTarih),
              km: sinirlar.oncekiKm?.toFixed(2),
            })
          : this.translate.instant('liste.oncekiYok');
        const sonrakiMetin = sinirlar.sonrakiTarih
          ? this.translate.instant('liste.sonrakiMetin', {
              tarih: this.formatTarih(sinirlar.sonrakiTarih),
              km: sinirlar.sonrakiKm?.toFixed(2),
            })
          : this.translate.instant('liste.sonrakiYok');
        this.sinirBilgisi.set(
          `${oncekiMetin}\n${sonrakiMetin}\n${this.translate.instant('liste.kmAraligiBilgisi')}`
        );

        this.tarihOnaylandi.set(true);
        this.ekleEtkin.set(true);
        this.statusText.set(this.translate.instant('liste.hizVeKmGirin'));
      },
      error: (err) => {
        if (buSurum !== this.sinirSorgusuSurumu || oturumHatasiMi(err)) {
          return;
        }
        this.sinirlarHesaplaniyor.set(false);
        this.statusText.set(this.translate.instant('liste.sinirlarHesaplanamadi'));
        this.bildirim.hata(`${this.translate.instant('liste.sinirlarHesaplanamadi')}\n\nHata: ${err.message}`);
      },
    });
  }

  ekle(): void {
    const arac = this.plakalar().find((p) => p.aracId === this.wizardAracId);
    if (!arac) {
      return;
    }
    if (this.wizardKm < this.kmMin() || this.wizardKm > this.kmMax()) {
      this.bildirim.hata(
        this.translate.instant('liste.hataKmAraligi', { min: this.kmMin().toFixed(2), max: this.kmMax().toFixed(2) })
      );
      return;
    }

    this.ekleniyor.set(true);
    this.statusText.set(this.translate.instant('liste.ekleniyorDurum'));
    this.api
      .createHareket({
        aracId: arac.aracId,
        aracPlaka: arac.aracPlaka,
        veriTarihi: tarihToIso(this.wizardTarihDate),
        hiz: this.wizardHiz,
        kmSayaci: this.wizardKm,
      })
      .subscribe({
        next: () => {
          this.statusText.set(this.translate.instant('liste.kayitEklendi'));
          this.wizardAracId = null;
          this.sonrakiAdimlariSifirla();
          this.ekleniyor.set(false);
          this.refreshGrid();
        },
        error: (err) => {
          this.ekleniyor.set(false);
          if (oturumHatasiMi(err)) {
            return;
          }
          this.statusText.set(this.translate.instant('liste.eklemeBasarisiz'));
          this.bildirim.hata(`${this.translate.instant('liste.hataKayitEklenemedi')}\n\nHata: ${err.message}`);
        },
      });
  }

  // ---------- Filtre şeridi (yerelde, bellekte - API'ye tekrar gitmez) ----------

  private filtrePlakaListesiniDoldur(hareketler: AracHareketDto[]): void {
    const plakalar = [...new Set(hareketler.map((h) => h.aracPlaka))].sort();
    this.filtrePlakaListesi.set(plakalar);
  }

  // Herhangi bir filtre kriteri varsayilan (bos) degilse true - hem "Filtrele" hem "Yenile"
  // bu soruyu ayni sekilde cevaplamali, o yuzden mantik burada TEK yerde toplandi.
  private filtreAktifMi(): boolean {
    return (
      this.filtreSeciliPlakalar().length > 0 ||
      this.filtreTarihAktif ||
      this.filtreHiz != null ||
      this.filtreKm != null
    );
  }

  // Su anki filtre kriterlerini (component alanlari) verilen kaynak diziye uygular. "Filtrele"
  // butonu VE "Yenile" (refreshGrid) AYNI bu fonksiyonu kullanir - mantik iki yerde ayri ayri
  // yazilirsa, biri guncellenip digeri unutulunca (tam da bu bug'da oldugu gibi) tekrar bozulur.
  private filtreleUygulaKriterleri(kaynak: AracHareketDto[]): AracHareketDto[] {
    let sonuc = kaynak;

    if (this.filtreSeciliPlakalar().length > 0) {
      const secili = this.filtreSeciliPlakalar();
      sonuc = sonuc.filter((h) => secili.includes(h.aracPlaka));
    }

    if (this.filtreTarihAktif) {
      const baslangicIso = tarihToIso(this.filtreBaslangicDate);
      const bitisIso = tarihToIso(this.filtreBitisDate);
      sonuc = sonuc.filter((h) => h.veriTarihi >= baslangicIso && h.veriTarihi <= bitisIso);
    }

    if (this.filtreHiz != null) {
      sonuc = sonuc.filter((h) => h.hiz === this.filtreHiz);
    }

    if (this.filtreKm != null) {
      sonuc = sonuc.filter((h) => h.kmSayaci === this.filtreKm);
    }

    return sonuc;
  }

  filtreleUygula(): void {
    this.filtreHatasi.set('');
    const sonuc = this.filtreleUygulaKriterleri(this.tumHareketler());
    this.gosterilenHareketler.set(sonuc);
    this.statusText.set(
      this.translate.instant('liste.kayitGosteriliyorFiltreli', {
        gosterilen: sonuc.length,
        toplam: this.tumHareketler().length,
      })
    );
  }

  filtreleTemizle(): void {
    this.filtreSeciliPlakalar.set([]);
    this.filtreTarihAktif = false;
    this.filtreBaslangicDate = isoToTarih(bugunIso());
    this.filtreBitisDate = isoToTarih(bugunIso());
    this.filtreHiz = null;
    this.filtreKm = null;
    this.filtreHatasi.set('');
    this.gosterilenHareketler.set(this.tumHareketler());
    this.statusText.set(this.translate.instant('liste.hareketKaydiYuklendi', { sayi: this.tumHareketler().length }));
  }

  // ---------- Excel'e Aktar (DevExtreme dx-data-grid'in kendi export'u, tarayicida ureterek) ----------

  // Grid'in kendi "export" toolbar ikonuna basilinca tetiklenir. Backend'e hic gitmiyor -
  // exportDataGrid grid'in O AN GORUNEN (filtreli/siralanmis) verisini dogrudan ExcelJS
  // Workbook'una yaziyor, sonuc buffer'i mevcut dosyaIndir() yardimcisiyla indiriliyor.
  onExporting(e: { component: unknown }): void {
    const workbook = new Workbook();
    const worksheet = workbook.addWorksheet('Araç Hareketleri');
    exportDataGrid({ component: e.component as never, worksheet }).then(() => {
      workbook.xlsx.writeBuffer().then((buffer) => {
        dosyaIndir(
          new Blob([buffer], { type: 'application/octet-stream' }),
          'arac-hareketleri.xlsx'
        );
      });
    });
  }

  formatTarih(iso: string): string {
    const [y, m, d] = iso.split('-');
    return `${d}.${m}.${y}`;
  }

  // "Excel'den Veri Aktar" kısayolu: dosyayı burada seçip doğrudan /import ekranına
  // yönlendiriyor - kullanıcının önce Import sayfasına gidip ORADA tekrar dosya seçmesine
  // gerek kalmıyor (gerçek kullanıcı geri bildirimi: "iki ayrı adım kafa karıştırıcı").
  excelDenAktarSecildi(event: Event): void {
    const input = event.target as HTMLInputElement;
    const dosya = input.files?.[0];
    input.value = ''; // ayni dosya tekrar secilse bile (change) yine tetiklensin diye
    if (!dosya) {
      return;
    }
    this.importDosyaKoprusu.bekleyenDosya = dosya;
    this.router.navigateByUrl('/import');
  }
}
