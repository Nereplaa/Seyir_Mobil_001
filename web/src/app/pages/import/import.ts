import { Component, OnInit, computed, inject, signal } from '@angular/core';
import { DxDataGridModule, DxButtonModule } from 'devextreme-angular';
import { AracHareketApi } from '../../services/arac-hareket-api';
import { Bildirim } from '../../services/bildirim';
import { ImportDosyaKoprusu } from '../../services/import-dosya-koprusu';
import { ImportSatiriSonucDto, ImportOnaylaSatiriDto } from '../../models/arac-hareket.models';
import { oturumHatasiMi } from '../../utils/hata-yardimcisi';
import { dosyaIndir } from '../../utils/dosya-indir';

// Grid'in KENDI icinde tuttugu ve duzenledigi satir tipi - backend'den gelen
// ImportSatiriSonucDto'ya, sadece client'ta anlam ifade eden `cakismaAksiyonu` alanini ekler.
type GridSatiri = ImportSatiriSonucDto & { cakismaAksiyonu: string };

const CAKISMA_AKSIYON_SECENEKLERI = [
  { deger: 'UzerineYaz', metin: 'Üzerine Yaz' },
  { deger: 'Atla', metin: 'Atla' },
];

@Component({
  selector: 'app-import',
  imports: [DxDataGridModule, DxButtonModule],
  templateUrl: './import.html',
  styleUrl: './import.css',
})
export class Import implements OnInit {
  private readonly api = inject(AracHareketApi);
  private readonly bildirim = inject(Bildirim);
  private readonly dosyaKoprusu = inject(ImportDosyaKoprusu);

  satirlar = signal<GridSatiri[]>([]);
  yukleniyor = signal(false);
  onaylaniyor = signal(false);
  dosyaHatasi = signal<string | null>(null);

  // Hucre duzenlemesi bittiginde otomatik yeniden dogrulamayi tetiklemek icin: art arda
  // birkac hucre duzenlenirken her birinde ayri istek gitmesin diye debounce edilir (liste.ts'teki
  // sinirSorgusuSurumu deseniyle AYNI mantik - HTTP yaniti gelene kadar baska bir duzenleme daha
  // olduysa, eski/bayat yanit sessizce yok sayilir).
  private debounceTimer?: ReturnType<typeof setTimeout>;
  private dogrulamaSurumu = 0;

  readonly columns = [
    { dataField: 'satirNo', caption: '#', width: 45, allowEditing: false, cssClass: 'col-numeric' },
    {
      dataField: 'aracPlaka',
      caption: 'Plaka',
      width: 130,
      cellTemplate: (cellElement: HTMLElement, cellInfo: { value?: string }) => {
        const span = document.createElement('span');
        span.className = 'plaka-chip';
        span.textContent = cellInfo.value ?? '';
        cellElement.appendChild(span);
      },
    },
    {
      dataField: 'veriTarihi',
      caption: 'Tarih',
      width: 130,
      dataType: 'date' as const,
      format: 'dd.MM.yyyy',
      cssClass: 'col-numeric',
      // Elle yazim yerine takvimden secim zorunlu kilinir - kullanicilar tarihi farkli
      // siralarda (gun/ay/yil) ya da farkli ayraclarla (./ -) yazabiliyordu, bu da import'a
      // gecersiz/yanlis-yorumlanan tarihler olarak dusuyordu (gercek kullanici geri bildirimi).
      editorOptions: { calendarOptions: { showTodayButton: true } },
      // dataType 'date' oldugu icin DevExtreme buraya artik ISO STRING degil bir Date nesnesi
      // (ya da gecersiz/bos deger icin null/undefined) veriyor - eskiden burada satirin ham
      // string'i (formatTarih'in bekledigi) geliyordu, tip uyusmazligi TUM grid'in coken bir
      // exception'a (`iso.split is not a function`) yol acip hicbir satirin gorunmemesine
      // sebep oluyordu (gercek kullanici raporuyla bulundu).
      customizeText: (c: { value?: Date | null }) => (c.value ? this.formatTarihDate(c.value) : 'Geçersiz'),
    },
    { dataField: 'hiz', caption: 'Hız', width: 80, dataType: 'number' as const, cssClass: 'col-numeric' },
    {
      dataField: 'kmSayaci',
      caption: 'Km Sayacı',
      width: 120,
      dataType: 'number' as const,
      format: '#,##0.00',
      cssClass: 'col-numeric',
    },
    {
      caption: 'Durum',
      // 190px'te "Çakışma — karar bekliyor" gibi en uzun durum metni çipin kenarından taşıp
      // yan sütuna değiyordu (gerçek kullanıcı geri bildirimi, 2026-08-07) - 230'a çıkarıldı.
      width: 230,
      allowEditing: false,
      calculateCellValue: (r: GridSatiri) => this.durumMetni(r),
      // dx-data-grid cell'leri bu grid'de "cell" editing modunda olsa da (allowEditing: false
      // burada) cellTemplate hala salt-okunur bir gorunum olarak destekleniyor - Durum artik
      // duz metin degil, renkli bir "cip" (nokta + etiket) olarak gosteriliyor (erisilebilirlik
      // icin: renk TEK BASINA anlam tasimiyor, etiket metni de hep goruluyor).
      cellTemplate: (cellElement: HTMLElement, cellInfo: { value?: string; data?: GridSatiri }) => {
        const sinif = cellInfo.data ? this.durumSinifi(cellInfo.data) : 'ok';
        const span = document.createElement('span');
        span.className = `chip chip-${sinif}`;
        span.textContent = cellInfo.value ?? '';
        cellElement.appendChild(span);
      },
    },
    {
      dataField: 'cakismaAksiyonu',
      caption: 'Çakışma Aksiyonu',
      width: 150,
      // Cakisma OLMAYAN bir satirda "Uzerine Yaz" anlamsiz (uzerine yazilacak bir kayit yok) -
      // ama kullanici yine de gozden gecirip GONULLU olarak "Atla" diyebilmeli (kullanici
      // istegi, 2026-08-07: "cakisma olmasa bile insan bakip karar verebilmeli"). Bos deger her
      // iki durumda da "normal ice aktar, aksiyon yok" anlamina geliyor - allowClearing ile
      // kullanici fikrini degistirip bosa donebiliyor.
      // NOT: lookup.dataSource'u SATIRA GORE degisen bir fonksiyon yapmak (DevExtreme'in
      // "bagimli/cascading lookup" deseni gibi gorunuyor) burada DENENDI ve REDDEDILDI - grid,
      // hucre GORUNTUSUNU (value->text) cozerken bu fonksiyonu satir baglami OLMADAN, TEK SEFER
      // bos {} ile cagirip TUM sutun icin ortak bir valueMap olusturuyor (bkz.
      // node_modules/devextreme/cjs/__internal/grids/grid_core/columns_controller/
      // m_columns_controller.js, lookup.update()) - sonuc: 'UzerineYaz' degeri o global map'te hic
      // yer almiyor, secilse bile hucrede BOS gorunuyordu (gercekten test edilip goruldu). Bu
      // yuzden lookup SABIT/TAM listeyi tutuyor (iki degerin de GORUNTUSU her zaman dogru cozulsun
      // diye), satira gore KISITLAMA ise asagidaki onEditorPreparing ile SADECE DUZENLEME
      // ANINDAKI editorun secenek listesinde yapiliyor.
      lookup: {
        dataSource: CAKISMA_AKSIYON_SECENEKLERI,
        valueExpr: 'deger',
        displayExpr: 'metin',
        allowClearing: true,
      },
      // Bos hucre "burada bir seyler yapilmali mi belli degil" hissi veriyordu (gercek kullanici
      // geri bildirimi, 2026-08-07: "kullanicinin oraya birsey secmesini anlamasi gerek") -
      // deger secilmemisse, cakisma GERCEKTEN varsa (karar sart) tikla-secilebilir gorunumlu
      // kesikli bir "Seciniz" etiketi, cakisma yoksa (secim opsiyonel) sadece soluk bir "—"
      // gosteriliyor - boylece zorunlu/opsiyonel ayrimi da gorsel olarak belli oluyor.
      cellTemplate: (cellElement: HTMLElement, cellInfo: { value?: string; data?: GridSatiri }) => {
        const secili = CAKISMA_AKSIYON_SECENEKLERI.find((o) => o.deger === cellInfo.value);
        const span = document.createElement('span');
        if (secili) {
          span.className = 'plaka-chip';
          span.textContent = secili.metin;
        } else if (cellInfo.data?.cakismaVarMi) {
          span.className = 'aksiyon-placeholder aksiyon-placeholder-required';
          span.textContent = 'Seçiniz ▾';
        } else {
          span.className = 'aksiyon-placeholder';
          span.textContent = '—';
        }
        cellElement.appendChild(span);
      },
    },
    {
      caption: 'Hata',
      minWidth: 220,
      allowEditing: false,
      calculateCellValue: (r: GridSatiri) => r.hatalar.join(' · '),
    },
  ];

  toplamSatir = computed(() => this.satirlar().length);
  hepsiGecerliMi = computed(
    () => this.satirlar().length > 0 && this.satirlar().every((s) => this.satirGecerliMi(s))
  );
  cakismaSatiriVarMi = computed(() => this.satirlar().some((s) => s.cakismaVarMi));

  ngOnInit(): void {
    // Liste sayfasindaki "Excel'den Veri Aktar" kisayolundan gelindiyse, dosya zaten
    // secilmis oluyor - kullanicinin burada AYRICA "Excel Dosyası Seç" demesine gerek yok.
    const bekleyen = this.dosyaKoprusu.bekleyenDosya;
    if (bekleyen) {
      this.dosyaKoprusu.bekleyenDosya = null;
      this.dosyaYukle(bekleyen);
    }
  }

  // "Atla" secilen bir satir zaten veritabanina hic yazilmayacak - hatali olsun ya da olmasin
  // onemsiz, digerlerinin ice aktarilmasini ENGELLEMEMELI (kullanici karari, 2026-08-07: tek
  // bozuk satir yuzunden butun dosyayi reddetmek yerine o satiri "atla" diyerek gecebilmeli).
  // Backend'deki `import-onayla` de AYNI onceligi uyguluyor (Atla kontrolu hata kontrolunden once).
  private satirGecerliMi(s: GridSatiri): boolean {
    if (s.cakismaAksiyonu === 'Atla') {
      return true;
    }
    if (s.hatalar.length > 0) {
      return false;
    }
    if (s.cakismaVarMi && !s.cakismaAksiyonu) {
      return false;
    }
    return true;
  }

  durumMetni(s: GridSatiri): string {
    if (s.cakismaAksiyonu === 'Atla') {
      return 'Atlanacak';
    }
    if (s.hatalar.length > 0) {
      return 'Hata';
    }
    if (s.cakismaVarMi && !s.cakismaAksiyonu) {
      return 'Çakışma — karar bekliyor';
    }
    if (s.cakismaVarMi) {
      return 'Üzerine yazılacak';
    }
    if (s.yeniAracMi) {
      return 'Yeni araç';
    }
    return 'Hazır';
  }

  // durumMetni ile AYNI durum ayrımını (hata/çakışma-bekliyor/hazır) bir renk sınıfına
  // eşliyor - hem "Durum" çipi (cellTemplate) hem satır kenarlığı (onRowPrepared) BU TEK
  // fonksiyonu kullanıyor, ikisi arasında tutarsızlık çıkmasın diye (bkz. durumMetni'nin
  // yorumundaki aynı desen: "Atla" dahil hata olmayan/karar bekletmeyen her şey "ok").
  durumSinifi(s: GridSatiri): 'ok' | 'warn' | 'danger' {
    if (s.cakismaAksiyonu === 'Atla') {
      return 'ok';
    }
    if (s.hatalar.length > 0) {
      return 'danger';
    }
    if (s.cakismaVarMi && !s.cakismaAksiyonu) {
      return 'warn';
    }
    return 'ok';
  }

  // Grid satırlarını duruma göre renklendirir - kırmızımsı: hata, sarımsı: çakışma karar
  // bekliyor, yeşilimsi: içe aktarılmaya hazır (Atla dahil - o satır zaten yazılmayacak).
  // Önceden inline style.backgroundColor ile yapılıyordu, artık styles.css'teki
  // .row-ok/.row-warn/.row-danger sınıflarıyla (sol kenarlık vurgusu) - metin çipi (Durum
  // sütunu) asıl anlamı taşıyor, bu satır vurgusu sadece göz taraması için ek bir ipucu.
  onRowPrepared(e: { rowType: string; data?: GridSatiri; rowElement: HTMLElement }): void {
    if (e.rowType !== 'data' || !e.data) {
      return;
    }
    e.rowElement.classList.remove('row-ok', 'row-warn', 'row-danger');
    e.rowElement.classList.add(`row-${this.durumSinifi(e.data)}`);
  }

  // "Çakışma Aksiyonu" hücresi düzenlemeye girdiğinde, gerçek bir çakışma OLMAYAN satırlarda
  // editörün (dxSelectBox) seçenek listesini sadece 'Atla' ile sınırlıyor - "Üzerine Yaz" orada
  // anlamsız (üzerine yazılacak bir kayıt yok). column.lookup'ın kendisi (yukarıda) SABİT/tam
  // liste olarak kalıyor - o sadece görüntü metnini (value->text) doğru çözmek için var, editördeki
  // gerçek seçenek listesi burada, satır bazında ayrıca kısıtlanıyor.
  onEditorPreparing(e: { dataField?: string; row?: { data?: GridSatiri }; editorOptions: any }): void {
    if (e.dataField !== 'cakismaAksiyonu') {
      return;
    }
    e.editorOptions.dataSource = e.row?.data?.cakismaVarMi
      ? CAKISMA_AKSIYON_SECENEKLERI
      : CAKISMA_AKSIYON_SECENEKLERI.filter((o) => o.deger === 'Atla');
  }

  // Dosya secimi icin DevExtreme'in dx-file-uploader'i YERINE gizli bir native <input type=file>
  // + dx-button kullaniyoruz (Liste sayfasindaki "Excel'den Veri Aktar" kisayoluyla AYNI desen) -
  // dx-file-uploader varsayilan olarak buton yaninda genis, sürükle-bırak alanlı bir kutu
  // ciziyor, bu da yanindaki "Şablon İndir" butonuyla hizasiz gorunmesine sebep oluyordu
  // (gerçek kullanıcı geri bildirimi).
  dosyaSecildi(event: Event): void {
    const input = event.target as HTMLInputElement;
    const dosya = input.files?.[0];
    input.value = '';
    if (!dosya) {
      return;
    }
    this.dosyaYukle(dosya);
  }

  private dosyaYukle(dosya: File): void {
    this.dosyaHatasi.set(null);
    this.satirlar.set([]);
    this.yukleniyor.set(true);
    this.api.importOnizle(dosya).subscribe({
      next: (yanit) => {
        this.yukleniyor.set(false);
        if (yanit.dosyaHatasi) {
          this.dosyaHatasi.set(yanit.dosyaHatasi);
          return;
        }
        this.satirlar.set(yanit.satirlar.map((s) => ({ ...s, cakismaAksiyonu: '' })));
      },
      error: (err) => {
        this.yukleniyor.set(false);
        if (oturumHatasiMi(err)) {
          return;
        }
        this.bildirim.hata(`Dosya okunamadı.\n\nHata: ${err.message}`);
      },
    });
  }

  // Cakisan satir sayisi onlarca olabiliyor - hepsini tek tek secmek yerine tek tusla hepsine
  // ayni aksiyonu atamak icin (kullanici istegi). Sadece GERCEKTEN cakisan satirlari etkiler.
  tumCakismalariAyarla(aksiyon: 'UzerineYaz' | 'Atla'): void {
    this.satirlar.update((arr) =>
      arr.map((s) => (s.cakismaVarMi ? { ...s, cakismaAksiyonu: aksiyon } : s))
    );
  }

  sablonIndir(): void {
    this.api.importSablonIndir().subscribe({
      next: (blob) => dosyaIndir(blob, 'import-sablon.xlsx'),
      error: (err) => {
        if (oturumHatasiMi(err)) {
          return;
        }
        this.bildirim.hata(`Şablon indirilemedi.\n\nHata: ${err.message}`);
      },
    });
  }

  // Grid'deki hucre duzenlemeleri DevExtreme tarafindan dataSource dizisi UZERINDE dogrudan
  // yapiliyor (referans degismiyor) - Angular signal'in bunu fark edip bagli computed'lari
  // (hepsiGecerliMi vb.) yeniden hesaplamasi icin referansi ELLE tazeliyoruz.
  gridKaydedildi(): void {
    this.satirlar.update((arr) => [...arr]);
  }

  // Grid'de bir hucre duzenlendiginde tetiklenir - her tus vurusunda degil, hucre duzenlemesi
  // "commit" edildiginde (onCellValueChanged) cagriliyor. Art arda birkac hucre duzenlenirken
  // her birinde ayri bir istek gitmesin diye ~700ms debounce uygulanir: her yeni duzenleme
  // onceki zamanlayiciyi iptal edip yeniden baslatir, sadece SON duzenlemeden bir sure sonra
  // tek bir yenidenDogrula() cagrisi yapilir.
  onCellValueChanged(): void {
    if (this.debounceTimer) {
      clearTimeout(this.debounceTimer);
    }
    this.debounceTimer = setTimeout(() => this.yenidenDogrula(), 700);
  }

  yenidenDogrula(): void {
    const hamSatirlar = this.satirlar().map((s) => ({
      satirNo: s.satirNo,
      aracPlaka: s.aracPlaka,
      veriTarihi: s.veriTarihi ?? '',
      hiz: s.hiz,
      kmSayaci: s.kmSayaci,
    }));

    // liste.ts'teki sinirSorgusuSurumu deseniyle AYNI koruma: her cagri kendi artan surum
    // numarasini alir, yanit gelince hala "guncel" cagri mi diye kontrol edilir - degilse
    // (arada otomatik tetiklemeyle veya manuel butonla daha yeni bir istek atildiysa) bu bayat
    // yanit sessizce yok sayilir, UI'i eski veriyle guncellemez.
    const buSurum = ++this.dogrulamaSurumu;
    this.yukleniyor.set(true);
    this.api.importYenidenDogrula(hamSatirlar).subscribe({
      next: (yanit) => {
        if (buSurum !== this.dogrulamaSurumu) {
          return;
        }
        // "Atla" artik cakisma sart olmaksizin herhangi bir satirda gonullu secilebilen gecerli
        // bir durum (bkz. yukaridaki lookup yorumu) - o yuzden onceki aksiyon kosulsuz tasiniyor,
        // cakisma durumuna gore ayrim yapilmiyor.
        const eskiAksiyonlar = new Map(this.satirlar().map((s) => [s.satirNo, s.cakismaAksiyonu]));
        this.satirlar.set(
          yanit.satirlar.map((s) => ({ ...s, cakismaAksiyonu: eskiAksiyonlar.get(s.satirNo) ?? '' }))
        );
        this.yukleniyor.set(false);
        this.bildirim.bilgi('Yeniden doğrulandı.');
      },
      error: (err) => {
        if (buSurum !== this.dogrulamaSurumu || oturumHatasiMi(err)) {
          return;
        }
        this.yukleniyor.set(false);
        this.bildirim.hata(`Yeniden doğrulanamadı.\n\nHata: ${err.message}`);
      },
    });
  }

  async iceAktar(): Promise<void> {
    if (!this.hepsiGecerliMi()) {
      this.bildirim.bilgi(
        'Tüm satırlar geçerli olmadan içe aktarılamaz. Hatalı veya karar bekleyen satırları düzeltip "Yeniden Doğrula"ya basın.'
      );
      return;
    }
    const onay = await this.bildirim.onayla(`${this.toplamSatir()} satır içe aktarılacak. Devam edilsin mi?`);
    if (!onay) {
      return;
    }

    // "Atla" secilen bir satir hatali/eksik veri icerebilir (zaten yazilmayacak) - o yuzden
    // burada artik "!" ile zorlamiyoruz, oldugu gibi (null olabilir) gonderiyoruz.
    const gonderilecek: ImportOnaylaSatiriDto[] = this.satirlar().map((s) => ({
      satirNo: s.satirNo,
      aracPlaka: s.aracPlaka,
      veriTarihi: s.veriTarihi,
      hiz: s.hiz,
      kmSayaci: s.kmSayaci,
      cakismaAksiyonu: s.cakismaAksiyonu,
    }));

    this.onaylaniyor.set(true);
    this.api.importOnayla(gonderilecek).subscribe({
      next: (sonuc) => {
        this.onaylaniyor.set(false);
        this.bildirim.bilgi(
          `İçe aktarma tamamlandı: ${sonuc.eklenenSayisi} eklendi, ${sonuc.guncellenenSayisi} güncellendi, ${sonuc.atlananSayisi} atlandı.`
        );
        this.satirlar.set([]);
      },
      error: (err) => {
        this.onaylaniyor.set(false);
        if (oturumHatasiMi(err)) {
          return;
        }
        this.bildirim.hata(`İçe aktarılamadı.\n\nHata: ${err.error?.message ?? err.message}`);
      },
    });
  }

  formatTarihDate(d: Date): string {
    const gun = String(d.getDate()).padStart(2, '0');
    const ay = String(d.getMonth() + 1).padStart(2, '0');
    return `${gun}.${ay}.${d.getFullYear()}`;
  }
}
