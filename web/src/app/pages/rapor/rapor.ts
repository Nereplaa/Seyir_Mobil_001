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
import {
  DxDateRangeBoxModule,
  DxTagBoxModule,
  DxCheckBoxModule,
  DxButtonModule,
  DxDataGridModule,
} from 'devextreme-angular';
import { TranslatePipe, TranslateService } from '@ngx-translate/core';
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
    TranslatePipe,
  ],
  templateUrl: './rapor.html',
  styleUrl: './rapor.css',
})
export class Rapor implements OnInit, OnDestroy {
  private readonly api = inject(AracHareketApi);
  private readonly bildirim = inject(Bildirim);
  private readonly translate = inject(TranslateService);

  // Alttaki plaka taşma alanının (.plaka-cip-tasma) sağ kenarını "Rapor Oluştur" butonunun sağ
  // kenarıyla hizalamak için field-row'un GERÇEK render genişliğini ölçüyoruz - CSS-only bir
  // shrink-to-fit denemesi başarısız oldu (bkz. rapor.css yorumu, çipli alanın kendi sınırsız
  // max-content'i sarmalayıcıyı sayfa sonuna kadar genişletiyordu). ResizeObserver, checkbox/buton
  // gibi sabit width'i olmayan (metne bağlı) alanlar dahil her koşulda doğru genişliği veriyor.
  @ViewChild('raporFieldRow') private raporFieldRowRef?: ElementRef<HTMLElement>;
  private resizeObserver?: ResizeObserver;
  fieldRowGenislik = signal<number | null>(null);

  constructor() {
    this.translate.onLangChange.subscribe(() => {
      this.ozetColumns.set(this.ozetKolonlariKur());
      this.detayColumns.set(this.detayKolonlariKur());
    });
  }

  ngAfterViewInit(): void {
    const el = this.raporFieldRowRef?.nativeElement;
    if (!el) {
      return;
    }
    // contentRect.width DEĞİL (field-row .panel-form .field-row{max-width:960px} sayesinde her
    // zaman panel'in izin verdiği genişliğe kadar "kutu" olarak geniş - içeriğin GÖRÜNEN/kullanılan
    // genişliği değil, sağdaki boşluğu da sayardı, ilk denemedeki hizasızlığın gerçek sebebi buydu).
    // Bunun yerine SON alanın (buton) sağ kenarı ile satırın sol kenarı arasındaki GERÇEK mesafeyi
    // ölçüyoruz - field-row'un kendi padding/border'ı yok, bu yüzden satırın sol kenarı = ilk
    // alanın sol kenarı, bu da tam olarak "görsel içerik genişliği"ni verir.
    this.resizeObserver = new ResizeObserver(() => this.fieldRowGenisliginiOlc());
    this.resizeObserver.observe(el);
    const sonAlan = el.lastElementChild;
    if (sonAlan) {
      this.resizeObserver.observe(sonAlan);
    }
    this.fieldRowGenisliginiOlc();
  }

  private fieldRowGenisliginiOlc(): void {
    const el = this.raporFieldRowRef?.nativeElement;
    const sonAlan = el?.lastElementChild;
    if (!el || !sonAlan) {
      return;
    }
    const satirSol = el.getBoundingClientRect().left;
    const sonAlanSag = sonAlan.getBoundingClientRect().right;
    this.fieldRowGenislik.set(Math.round(sonAlanSag - satirSol));
  }

  ngOnDestroy(): void {
    this.resizeObserver?.disconnect();
  }

  // ---------- DevExtreme DataGrid kolonlari (ozet + detayli rapor) ----------
  // Kolon basliklari ceviri iceriyor - dil degisince yeniden kurulmasi gerekiyor (bkz.
  // constructor'daki onLangChange abonesi), bu yuzden sabit dizi degil signal.
  readonly ozetColumns = signal(this.ozetKolonlariKur());
  readonly detayColumns = signal(this.detayKolonlariKur());

  private ozetKolonlariKur() {
    return [
      {
        dataField: 'aracPlaka',
        caption: this.translate.instant('rapor.kolonAracPlakasi'),
        cellTemplate: (cellElement: HTMLElement, cellInfo: { value?: string }) => {
          const span = document.createElement('span');
          span.className = 'plaka-chip';
          span.textContent = cellInfo.value ?? '';
          cellElement.appendChild(span);
        },
      },
      {
        dataField: 'baslangicKm',
        caption: this.translate.instant('rapor.kolonBaslangicKm'),
        cssClass: 'col-numeric',
        customizeText: (cellInfo: { value?: number | null }) =>
          cellInfo.value == null ? this.translate.instant('rapor.veriYok') : this.formatKm(cellInfo.value),
      },
      {
        dataField: 'bitisKm',
        caption: this.translate.instant('rapor.kolonBitisKm'),
        cssClass: 'col-numeric',
        customizeText: (cellInfo: { value?: number | null }) =>
          cellInfo.value == null ? this.translate.instant('rapor.veriYok') : this.formatKm(cellInfo.value),
      },
      {
        dataField: 'yapilanKm',
        caption: this.translate.instant('rapor.kolonYapilanKm'),
        cssClass: 'col-numeric',
        customizeText: (cellInfo: { value?: number | null }) =>
          cellInfo.value == null ? this.translate.instant('rapor.veriYok') : this.formatKm(cellInfo.value),
      },
    ];
  }

  private detayKolonlariKur() {
    return [
      {
        dataField: 'aracPlaka',
        caption: this.translate.instant('rapor.kolonAracPlaka'),
        cellTemplate: (cellElement: HTMLElement, cellInfo: { value?: string }) => {
          const span = document.createElement('span');
          span.className = 'plaka-chip';
          span.textContent = cellInfo.value ?? '';
          cellElement.appendChild(span);
        },
      },
      {
        dataField: 'veriTarihi',
        caption: this.translate.instant('rapor.kolonVeriTarihi'),
        cssClass: 'col-numeric',
        customizeText: (cellInfo: { value?: string }) => (cellInfo.value ? this.formatTarih(cellInfo.value) : ''),
      },
      {
        dataField: 'kmSayaci',
        caption: this.translate.instant('rapor.kolonKmSayaci'),
        cssClass: 'col-numeric',
        customizeText: (cellInfo: { value?: number }) => (cellInfo.value == null ? '' : this.formatKm(cellInfo.value)),
      },
      {
        dataField: 'artis',
        caption: this.translate.instant('rapor.kolonArtis'),
        cssClass: 'col-numeric',
        customizeText: (cellInfo: { value?: number | null }) =>
          cellInfo.value == null ? '-' : this.formatKm(cellInfo.value),
      },
    ];
  }

  plakalar = signal<AracPlakaLookupDto[]>([]);
  // Plaka secimi artik dx-tag-box'in KENDI arama+cip mekanizmasi ile yapiliyor - onceki elle
  // yazilmis arama kutusu/acilir liste/cip listesi (plakaArama, lookupAcik, lookupSonuclari,
  // lookupAc/Kapat, plakaSec/Kaldir) tamamen kaldirildi, DevExtreme zaten ayni ozelligi
  // (aranabilir coklu-secim + kaldirilabilir cipler) hazir sunuyor.
  seciliPlakalar = signal<string[]>([]);

  seciliPlakalarDegisti(event: { value?: string[] | null }): void {
    this.seciliPlakalar.set(event.value ?? []);
  }

  // dx-tag-box'ın kendi çipleri gizli (bkz. rapor.html/css) - liste.ts'teki AYNI desen,
  // gerekçe için oradaki yoruma bakın.
  private readonly plakaCipTavani = 20;
  readonly plakaCipGorunumu = computed(() => {
    const secili = this.seciliPlakalar();
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
    this.seciliPlakalar.update((arr) => arr.filter((p) => p !== plaka));
  }

  // 2026-08-13 QA turu: signal'a cevrildi - onceden duz alandi, "raporOlusturEtkin" computed'i
  // SADECE gercek signal okumalarini (seciliPlakalar()) bagimlilik olarak izliyor, duz alan
  // mutasyonlarini (this.baslangic = ...) HIC gormuyordu. Sonuc: kullanici once plakayi
  // secip SONRA tarih araligini degistirirse, computed yeniden calismiyor ve "Rapor Olustur"
  // butonu tarihler artik gecerliyken bile devre disi kalmaya devam ediyordu (gercek bug,
  // Chrome'da manuel QA turunda bulundu - `window.ng.getComponent` ile computed'in "etkin:false"
  // dondurdugu, ama baslangic/bitis'in ASLINDA dogru oldugu dogrudan gozlemlendi).
  baslangic = signal(bugunIso());
  bitis = signal(yarinIso(bugunIso()));
  detayliRapor = false;

  // dx-date-range-box'a baglanan Date alanlari - asil "kaynak" hala baslangic/bitis (ISO
  // string) signal'lari, rapor mantigi/export/backend cagrilari hepsi bu string'leri kullaniyor.
  // BILINCLI OLARAK getter/setter DEGIL, sabit alan: bir getter her change-detection turunda
  // "new Date(...)" ile YENI bir nesne dondurseydi, DevExtreme bunu "deger degisti" sanip
  // widget'i surekli yeniden baslatiyordu (gercek bug, ekranda ust uste yigilan onlarca takvim
  // olarak ortaya cikti) - sabit alan + degisiklikleri SADECE kullanici etkilesiminde (event
  // handler'larda) guncelleme, referans kararliligini koruyor.
  baslangicDate: Date = isoToTarih(this.baslangic());
  bitisDate: Date = isoToTarih(this.bitis());

  baslangicDateDegisti(deger: string | number | Date | null): void {
    if (deger == null) {
      return;
    }
    const tarih = new Date(deger);
    this.baslangicDate = tarih;
    this.baslangic.set(tarihToIso(tarih));
    this.onBaslangicDegisti();
  }

  bitisDateDegisti(deger: string | number | Date | null): void {
    if (deger == null) {
      return;
    }
    const tarih = new Date(deger);
    this.bitisDate = tarih;
    this.bitis.set(tarihToIso(tarih));
  }

  ozetSonuclar = signal<AracRaporSonucuDto[]>([]);
  detaySonuclar = signal<AracHareketDetayRaporSatiriDto[]>([]);
  raporUretildi = signal(false);
  yukleniyor = signal(false);
  statusText = signal('');

  raporOlusturEtkin = computed(() => this.seciliPlakalar().length > 0 && this.bitis() > this.baslangic());

  ngOnInit(): void {
    this.api.getPlakalar().subscribe({
      next: (plakalar) => this.plakalar.set(plakalar),
      error: (err) => {
        if (!oturumHatasiMi(err)) {
          this.bildirim.hata(`${this.translate.instant('rapor.hataAracListesi')}\n\nHata: ${err.message}`);
        }
      },
    });
  }

  onBaslangicDegisti(): void {
    const minBitis = yarinIso(this.baslangic());
    if (this.bitis() < minBitis) {
      this.bitis.set(minBitis);
      this.bitisDate = isoToTarih(this.bitis());
    }
  }

  raporOlustur(): void {
    const plakalar = this.seciliPlakalar();
    this.yukleniyor.set(true);
    this.statusText.set(this.translate.instant('rapor.olusturuluyorDurum'));
    this.raporUretildi.set(false);

    if (this.detayliRapor) {
      this.api
        .getDetayRaporu({ plakalar, baslangic: this.baslangic(), bitis: this.bitis() })
        .subscribe({
          next: (satirlar) => {
            this.detaySonuclar.set(satirlar);
            this.raporUretildi.set(true);
            this.yukleniyor.set(false);
            this.statusText.set(this.translate.instant('rapor.detayliRaporOlusturuldu', { sayi: satirlar.length }));
          },
          error: (err) => this.raporHatasi(err),
        });
    } else {
      this.api
        .getRaporToplu({ plakalar, baslangic: this.baslangic(), bitis: this.bitis() })
        .subscribe({
          next: (sonuclar) => {
            this.ozetSonuclar.set(sonuclar);
            this.raporUretildi.set(true);
            this.yukleniyor.set(false);
            this.statusText.set(this.translate.instant('rapor.ozetRaporOlusturuldu', { sayi: sonuclar.length }));
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
    this.statusText.set(this.translate.instant('rapor.raporOlusturulamadi'));
    this.bildirim.hata(`${this.translate.instant('rapor.raporOlusturulamadi')}\n\nHata: ${err.message}`);
  }

  // ---------- Excel'e Aktar (DevExtreme dx-data-grid'in kendi export'u, tarayicida ureterek) ----------
  // Backend'e hic gitmiyor - eski ClosedXML "her plaka icin ayri bolum" ozel bicimlendirmesi
  // (exportModu secimi) bu yuzden kayboldu, DevExtreme grid export'u TEK duz tablo uretiyor
  // (kullanici onayli sadelesme, bkz. AI_NOTES/decisions.md).

  onExportingOzet(e: { component: unknown }): void {
    this.gridDenIndir(e, this.translate.instant('rapor.sheetOzet'), 'rapor-ozet.xlsx');
  }

  onExportingDetay(e: { component: unknown }): void {
    this.gridDenIndir(e, this.translate.instant('rapor.sheetDetay'), 'rapor-detay.xlsx');
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
