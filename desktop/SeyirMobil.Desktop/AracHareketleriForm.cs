using SeyirMobil.Desktop.Models;
using SeyirMobil.Desktop.Services;

namespace SeyirMobil.Desktop;

public partial class AracHareketleriForm : Form
{
    private readonly AracHareketApiClient _apiClient = new();

    // Program.cs'teki login<->ana-ekran dongusune, formun NEDEN kapandigini bildirir: true ise
    // (Cikis Yap butonu veya oturumun gecersiz olmasi/401) login ekranina geri donulur, false
    // (varsayilan - kullanici pencereyi X ile kapatti) ise program tamamen sonlanir.
    public static bool OturumSonlandirildiMi;
    private bool _oturumSonlandiIsleniyor;

    // Ayni anda birden fazla sinir hesaplama istegi cakisirsa (ör. plaka hizli degistirilirse),
    // sadece EN SON baslatilan istegin sonucu UI'a uygulanir - eskisi "surum" uyusmadigi icin
    // sessizce yok sayilir.
    private int _sinirSorgusuSurumu;

    // Filtre seridi, API'ye tekrar gitmeden bu listenin uzerinde bellekte calisiyor.
    private List<AracHareketDto> _tumHareketler = [];

    // Filtrenin o an ORTAYA COKARDIGI (filtresizken _tumHareketler ile ayni) liste - sayfalama
    // bunun uzerinde calisir, Excel'e Aktar da BUNU (sadece o anki sayfayi degil) disa aktarir.
    private List<AracHareketDto> _gosterilenHareketler = [];
    private int _suankiSayfa = 1;
    private int _sayfaBoyutu = 25;

    public AracHareketleriForm()
    {
        InitializeComponent();
        // Sayfalama eklendikten sonra dgvHareketler'in Yuksekligi her DataSource degisiminde
        // (sayfa/sayfa boyutu degisince) yeniden hesaplaniyor - bu, form cift-tamponlanmadan
        // (double buffered) yapilinca eskiden kapladigi alanin "hayalet" gibi ekranda kalmasina
        // (arkadaki baska bir pencerenin gorunmesine kadar varan bir cizim artifaktina) yol
        // aciyordu. DoubleBuffered = true, WinForms'ta bu sinif sorunlar icin standart cozum.
        DoubleBuffered = true;
        SetupGridColumns();
        TokenStore.OturumGecersizOldu += OturumGecersizOldu_Handler;
    }

    protected override void OnFormClosed(FormClosedEventArgs e)
    {
        // Statik bir olaya abone olundugu icin ACIKCA ayrilmazsa, bu form kapandiktan sonra
        // (ornegin login dongusu yeni bir AracHareketleriForm olusturdugunda) eski, artik
        // Dispose edilmis forma hala olay gonderilmeye calisilip cokmeye yol acabilirdi.
        TokenStore.OturumGecersizOldu -= OturumGecersizOldu_Handler;
        base.OnFormClosed(e);
    }

    // OturumHandler (baska bir HTTP istegi sirasinda, arka planda) 401 yakaladiginda tetiklenir -
    // farkli bir thread'den gelebilecegi icin UI thread'ine Invoke ile gecilir.
    private void OturumGecersizOldu_Handler()
    {
        if (IsDisposed || !IsHandleCreated)
        {
            return;
        }
        Invoke(() =>
        {
            if (_oturumSonlandiIsleniyor)
            {
                return;
            }
            _oturumSonlandiIsleniyor = true;
            MessageBox.Show(
                this,
                "Oturum süresi doldu veya geçersiz. Lütfen tekrar giriş yapın.",
                "Oturum Sona Erdi",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
            OturumSonlandirildiMi = true;
            Close();
        });
    }

    private async void btnCikisYap_Click(object? sender, EventArgs e)
    {
        await _apiClient.LogoutAsync();
        TokenStore.OturumBitir();
        OturumSonlandirildiMi = true;
        Close();
    }

    private void SetupGridColumns()
    {
        // Basliklarin her zaman gorunur ve okunakli olmasi icin acikca ayarlaniyor.
        dgvHareketler.ColumnHeadersVisible = true;
        dgvHareketler.EnableHeadersVisualStyles = false;
        dgvHareketler.ColumnHeadersDefaultCellStyle.BackColor = Color.WhiteSmoke;
        dgvHareketler.ColumnHeadersDefaultCellStyle.ForeColor = Color.Black;
        dgvHareketler.ColumnHeadersDefaultCellStyle.Font = new Font(dgvHareketler.Font, FontStyle.Bold);
        dgvHareketler.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;

        dgvHareketler.Columns.Add(new DataGridViewTextBoxColumn
        {
            Name = "AracId",
            DataPropertyName = "AracId",
            HeaderText = "Araç ID",
            Width = 80
        });
        dgvHareketler.Columns.Add(new DataGridViewTextBoxColumn
        {
            Name = "AracPlaka",
            DataPropertyName = "AracPlaka",
            HeaderText = "Araç Plaka",
            Width = 140
        });
        dgvHareketler.Columns.Add(new DataGridViewTextBoxColumn
        {
            Name = "VeriTarihi",
            DataPropertyName = "VeriTarihi",
            HeaderText = "Veri Tarihi",
            Width = 130,
            DefaultCellStyle = new DataGridViewCellStyle { Format = "dd.MM.yyyy" }
        });
        dgvHareketler.Columns.Add(new DataGridViewTextBoxColumn
        {
            Name = "Hiz",
            DataPropertyName = "Hiz",
            HeaderText = "Hız",
            Width = 90
        });
        dgvHareketler.Columns.Add(new DataGridViewTextBoxColumn
        {
            Name = "KmSayaci",
            DataPropertyName = "KmSayaci",
            HeaderText = "Km Sayacı",
            Width = 150,
            DefaultCellStyle = new DataGridViewCellStyle { Format = "N2" }
        });
    }

    private async void AracHareketleriForm_Load(object? sender, EventArgs e)
    {
        cmbSayfaBoyutu.Items.AddRange([10, 25, 50, 100]);
        cmbSayfaBoyutu.SelectedItem = _sayfaBoyutu;

        await RefreshGridAsync();
        await PlakalarYukleAsync();
        FiltrePlakaListesiniDoldur();
    }

    private async void btnYenile_Click(object? sender, EventArgs e)
    {
        await RefreshGridAsync();
    }

    private void btnHareketRaporu_Click(object? sender, EventArgs e)
    {
        using var raporForm = new AracHareketRaporuForm();
        raporForm.ShowDialog(this);
    }

    private async void btnExcelIceAktar_Click(object? sender, EventArgs e)
    {
        using var importForm = new AracHareketImportForm();
        importForm.ShowDialog(this);
        // Import ekranindan yeni kayitlar eklenmis olabilir - ana listeyi tazele.
        await RefreshGridAsync();
    }

    private async Task RefreshGridAsync()
    {
        lblStatus.Text = "Yükleniyor...";
        try
        {
            _tumHareketler = await _apiClient.GetTumHareketlerAsync();
            _gosterilenHareketler = _tumHareketler;
            _suankiSayfa = 1;
            GuncelleSayfalamaVeGrid();
            lblStatus.Text = $"{_tumHareketler.Count} hareket kaydı yüklendi.";
        }
        catch (Exception ex)
        {
            if (HataYardimcisi.OturumSuresiDolduMu(ex))
            {
                return;
            }
            lblStatus.Text = "Veri yüklenemedi.";
            MessageBox.Show(
                $"Araç hareketleri alınamadı. Backend API çalışıyor mu?\n\nHata: {ex.Message}",
                "Bağlantı Hatası",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }
        finally
        {
            // flowUst'un yuksekligi adim adim degistigi icin (gruplar acilip kapaniyor),
            // dgvHareketler'in (Dock=Fill) hemen dogru sekilde yeniden hizalanmasini garantiler -
            // aksi halde basliklar/ust satirlar bir onceki yerlesimin altinda kalabiliyordu.
            flowUst.PerformLayout();
            PerformLayout();
        }
    }

    // ---------- Sayfalama (filtrelenmiş/gösterilen liste üzerinde, yerelde) ----------

    private void GuncelleSayfalamaVeGrid()
    {
        var toplamSayfa = Math.Max(1, (int)Math.Ceiling(_gosterilenHareketler.Count / (double)_sayfaBoyutu));
        _suankiSayfa = Math.Clamp(_suankiSayfa, 1, toplamSayfa);

        var sayfaVerisi = _gosterilenHareketler
            .Skip((_suankiSayfa - 1) * _sayfaBoyutu)
            .Take(_sayfaBoyutu)
            .ToList();

        dgvHareketler.DataSource = sayfaVerisi;
        lblSayfaGostergesi.Text = $"Sayfa {_suankiSayfa} / {toplamSayfa}";
        btnOncekiSayfa.Enabled = _suankiSayfa > 1;
        btnSonrakiSayfa.Enabled = _suankiSayfa < toplamSayfa;

        // tableRoot 4 satira (flowUst/flowFiltre/dgvHareketler/flowSayfalama) cikinca,
        // DataSource her degistiginde grid + sayfalama seridinin dogru yeniden hizalanmasini
        // garantiliyoruz. Invalidate(true) BUTUN formu (sadece degisen kontrolu degil) yeniden
        // ciziyor - aksi halde dgvHareketler kucculunce eskiden kapladigi alan hic yeniden
        // boyanmayip "hayalet" gibi kalabiliyordu (bkz. api_patterns.md - TableLayoutPanel +
        // PerformLayout deseni, ve DoubleBuffered notu yukarida).
        tableRoot.PerformLayout();
        PerformLayout();
        Invalidate(true);
        Update();
    }

    private void cmbSayfaBoyutu_SelectedIndexChanged(object? sender, EventArgs e)
    {
        if (cmbSayfaBoyutu.SelectedItem is not int boyut)
        {
            return;
        }
        _sayfaBoyutu = boyut;
        _suankiSayfa = 1;
        GuncelleSayfalamaVeGrid();
    }

    private void btnOncekiSayfa_Click(object? sender, EventArgs e)
    {
        _suankiSayfa--;
        GuncelleSayfalamaVeGrid();
    }

    private void btnSonrakiSayfa_Click(object? sender, EventArgs e)
    {
        _suankiSayfa++;
        GuncelleSayfalamaVeGrid();
    }

    // ---------- Excel'e Aktar (o an GORUNEN - filtreli olabilir - TUM satirlar, sadece o anki sayfa degil) ----------

    private async void btnExcelAktar_Click(object? sender, EventArgs e)
    {
        if (_gosterilenHareketler.Count == 0)
        {
            MessageBox.Show("Aktarılacak kayıt yok.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        using var dialog = new SaveFileDialog
        {
            Filter = "Excel Dosyası (*.xlsx)|*.xlsx",
            FileName = "arac-hareketleri.xlsx"
        };
        if (dialog.ShowDialog(this) != DialogResult.OK)
        {
            return;
        }

        btnExcelAktar.Enabled = false;
        lblStatus.Text = "Excel oluşturuluyor...";
        try
        {
            var veri = await _apiClient.ExportHareketlerAsync(_gosterilenHareketler);
            await File.WriteAllBytesAsync(dialog.FileName, veri);
            lblStatus.Text = "Excel dosyası kaydedildi.";
        }
        catch (Exception ex)
        {
            if (HataYardimcisi.OturumSuresiDolduMu(ex))
            {
                return;
            }
            lblStatus.Text = "Excel'e aktarılamadı.";
            MessageBox.Show($"Excel'e aktarılamadı.\n\nHata: {ex.Message}", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        finally
        {
            btnExcelAktar.Enabled = true;
        }
    }

    // ---------- Silme ----------

    private void dgvHareketler_SelectionChanged(object? sender, EventArgs e)
    {
        btnSil.Enabled = dgvHareketler.CurrentRow is not null;
    }

    private async void btnSil_Click(object? sender, EventArgs e)
    {
        if (dgvHareketler.CurrentRow?.DataBoundItem is not AracHareketDto secili)
        {
            return;
        }

        var onay = MessageBox.Show(
            $"\"{secili.AracPlaka}\" - {secili.VeriTarihi:dd.MM.yyyy} tarihli kaydı silmek istediğine emin misin?",
            "Silme Onayı",
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Question);
        if (onay != DialogResult.Yes)
        {
            return;
        }

        btnSil.Enabled = false;
        lblStatus.Text = "Siliniyor...";
        try
        {
            await _apiClient.DeleteHareketAsync(secili.Id);
            await RefreshGridAsync();
        }
        catch (Exception ex)
        {
            if (HataYardimcisi.OturumSuresiDolduMu(ex))
            {
                return;
            }
            lblStatus.Text = "Silme başarısız.";
            MessageBox.Show($"Kayıt silinemedi.\n\nHata: {ex.Message}", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        finally
        {
            btnSil.Enabled = dgvHareketler.CurrentRow is not null;
        }
    }

    // ---------- Ekleme sihirbazı (4 adım: Plaka -> Tarih (Onayla) -> Hız -> Km Sayacı) ----------

    private async Task PlakalarYukleAsync()
    {
        try
        {
            var plakalar = await _apiClient.GetPlakalarAsync();

            // DataSource atarken WinForms ilk ogeyi otomatik secip SelectedIndexChanged'i hemen
            // tetikliyor - bu istenmeyen bir sihirbaz baslangicina yol aciyordu (bkz. 2026-08-03
            // kullanici bulgusu). Baglama sirasinda event'i gecici olarak ayirip, listeyi
            // SECIMSIZ (SelectedIndex=-1) birakip SONRA event'i tekrar bagliyoruz.
            cmbPlaka.SelectedIndexChanged -= cmbPlaka_SelectedIndexChanged;
            cmbPlaka.DataSource = plakalar;
            cmbPlaka.DisplayMember = nameof(AracPlakaLookupDto.AracPlaka);
            cmbPlaka.ValueMember = nameof(AracPlakaLookupDto.AracId);
            cmbPlaka.SelectedIndex = -1;
            cmbPlaka.SelectedIndexChanged += cmbPlaka_SelectedIndexChanged;
        }
        catch (Exception ex)
        {
            if (HataYardimcisi.OturumSuresiDolduMu(ex))
            {
                return;
            }
            MessageBox.Show($"Plaka listesi alınamadı.\n\nHata: {ex.Message}", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void cmbPlaka_SelectedIndexChanged(object? sender, EventArgs e)
    {
        // Adim 1 degisince, henuz tamamlanmamis TUM sonraki adimlar sifirlanir - onceki plaka
        // icin hesaplanmis sinirlar/degerler yanlislikla yeni plakaya tasinmasin diye.
        SonrakiAdimlariSifirla();

        if (cmbPlaka.SelectedIndex < 0)
        {
            groupTarih.Visible = false;
            return;
        }

        groupTarih.Visible = true;
        dtpTarih.Value = DateTime.Today;
        flowUst.PerformLayout();
    }

    private void dtpTarih_ValueChanged(object? sender, EventArgs e)
    {
        // Tarih degistigi (veya ilk kez ayarlandigi) an, henuz "onaylanmadigi" icin sonraki
        // adimlar (hiz/km) gizli kalir - kullanici "Tarihi Onayla"ya basmadan gorunmezler.
        SonrakiAdimlariSifirla();
    }

    private void SonrakiAdimlariSifirla()
    {
        groupHiz.Visible = false;
        groupKm.Visible = false;
        btnEkle.Enabled = false;
        lblSinirBilgisi.Text = "";
        flowUst.PerformLayout();
        PerformLayout();
    }

    private void nudHiz_ValueChanged(object? sender, EventArgs e)
    {
        // Hiz, NumericUpDown'un kendi Minimum/Maximum'u ile zaten "dogrulanmis" sayilir.
    }

    private async void btnTarihOnayla_Click(object? sender, EventArgs e)
    {
        await GuncelleSinirlarVeAdimlariAsync();
    }

    // Secilen plaka + "onaylanmis" tarihe gore en yakin onceki/sonraki okumayi backend'den
    // ceker, km sayaci icin gecerli araligi hesaplayip Adim 3 (hiz) + Adim 4'u (km) gosterir.
    private async Task GuncelleSinirlarVeAdimlariAsync()
    {
        if (cmbPlaka.SelectedItem is not AracPlakaLookupDto secilenArac)
        {
            return;
        }

        var buSurum = ++_sinirSorgusuSurumu;
        var tarih = DateOnly.FromDateTime(dtpTarih.Value);
        lblStatus.Text = "Sınırlar hesaplanıyor...";

        try
        {
            var sinirlar = await _apiClient.GetSinirlarAsync(secilenArac.AracPlaka, tarih);

            // Bu bekleme surerken kullanici plakayi/tarihi degistirmis olabilir - o zaman bu
            // sonuc artik ESKI (surum uyusmuyor), UI'a hic dokunmadan sessizce cikilir.
            if (buSurum != _sinirSorgusuSurumu)
            {
                return;
            }

            if (sinirlar.AyniTarihVarMi)
            {
                lblStatus.Text = "Bu plaka için bu tarihte zaten bir kayıt var. Farklı bir tarih seçin.";
                return;
            }

            nudKm.Minimum = 0m;
            nudKm.Maximum = 99999999.99m;

            var min = sinirlar.OncekiKm.HasValue ? sinirlar.OncekiKm.Value + 0.01m : 0m;
            var max = sinirlar.SonrakiKm.HasValue ? sinirlar.SonrakiKm.Value - 0.01m : 99999999.99m;

            if (min > max)
            {
                lblStatus.Text = "Bu tarih için geçerli bir km aralığı yok (önceki/sonraki okumalar birbirine çok yakın).";
                return;
            }

            nudKm.Minimum = min;
            nudKm.Maximum = max;
            nudKm.Value = min;

            var oncekiMetin = sinirlar.OncekiTarih.HasValue
                ? $"Önceki: {sinirlar.OncekiTarih:dd.MM.yyyy} → {sinirlar.OncekiKm:N2} km"
                : "Önceki: yok (bu, ilk kayıt olacak)";
            var sonrakiMetin = sinirlar.SonrakiTarih.HasValue
                ? $"Sonraki: {sinirlar.SonrakiTarih:dd.MM.yyyy} → {sinirlar.SonrakiKm:N2} km"
                : "Sonraki: yok (bu, son kayıt olacak)";
            lblSinirBilgisi.Text = $"{oncekiMetin}\n{sonrakiMetin}\nGirilecek km bu ikisinin arasında olmalı.";

            groupHiz.Visible = true;
            groupKm.Visible = true;
            btnEkle.Enabled = true;
            flowUst.PerformLayout();
            PerformLayout();
            lblStatus.Text = "Hız ve km sayacını girip Ekle'ye basabilirsin.";
        }
        catch (Exception ex)
        {
            if (buSurum != _sinirSorgusuSurumu || HataYardimcisi.OturumSuresiDolduMu(ex))
            {
                return;
            }
            lblStatus.Text = "Sınırlar hesaplanamadı.";
            MessageBox.Show($"Sınırlar hesaplanamadı.\n\nHata: {ex.Message}", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private async void btnEkle_Click(object? sender, EventArgs e)
    {
        if (cmbPlaka.SelectedItem is not AracPlakaLookupDto secilenArac)
        {
            return;
        }

        var request = new CreateAracHareketRequestDto(
            secilenArac.AracId,
            secilenArac.AracPlaka,
            DateOnly.FromDateTime(dtpTarih.Value),
            (int)nudHiz.Value,
            nudKm.Value);

        btnEkle.Enabled = false;
        lblStatus.Text = "Ekleniyor...";
        try
        {
            await _apiClient.CreateHareketAsync(request);
            lblStatus.Text = "Kayıt eklendi.";

            // Sihirbazi basa sar.
            cmbPlaka.SelectedIndex = -1;

            await RefreshGridAsync();
        }
        catch (Exception ex)
        {
            if (HataYardimcisi.OturumSuresiDolduMu(ex))
            {
                return;
            }
            lblStatus.Text = "Ekleme başarısız.";
            MessageBox.Show($"Kayıt eklenemedi.\n\nHata: {ex.Message}", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            btnEkle.Enabled = true;
        }
    }

    // ---------- Filtre şeridi (yerelde, bellekte - API'ye tekrar gitmez) ----------

    private const string FiltreTumu = "Tümü";

    private void FiltrePlakaListesiniDoldur()
    {
        var plakalar = _tumHareketler
            .Select(h => h.AracPlaka)
            .Distinct()
            .OrderBy(p => p)
            .ToList();
        plakalar.Insert(0, FiltreTumu);

        cmbFiltrePlaka.DataSource = plakalar;
        cmbFiltrePlaka.SelectedIndex = 0;
    }

    private void chkFiltreTarih_CheckedChanged(object? sender, EventArgs e)
    {
        dtpFiltreTarih.Enabled = chkFiltreTarih.Checked;
    }

    private void btnFiltreUygula_Click(object? sender, EventArgs e)
    {
        IEnumerable<AracHareketDto> sonuc = _tumHareketler;

        if (cmbFiltrePlaka.SelectedItem is string plaka && plaka != FiltreTumu)
        {
            sonuc = sonuc.Where(h => h.AracPlaka == plaka);
        }

        if (chkFiltreTarih.Checked)
        {
            var tarih = DateOnly.FromDateTime(dtpFiltreTarih.Value);
            sonuc = sonuc.Where(h => h.VeriTarihi == tarih);
        }

        if (!string.IsNullOrWhiteSpace(txtFiltreHiz.Text))
        {
            if (!int.TryParse(txtFiltreHiz.Text.Trim(), out var hiz))
            {
                MessageBox.Show("Hız filtresi geçerli bir tam sayı olmalı.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            sonuc = sonuc.Where(h => h.Hiz == hiz);
        }

        if (!string.IsNullOrWhiteSpace(txtFiltreKm.Text))
        {
            if (!decimal.TryParse(txtFiltreKm.Text.Trim(), out var km))
            {
                MessageBox.Show("Km sayacı filtresi geçerli bir sayı olmalı.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            sonuc = sonuc.Where(h => h.KmSayaci == km);
        }

        _gosterilenHareketler = sonuc.ToList();
        _suankiSayfa = 1;
        GuncelleSayfalamaVeGrid();
        lblStatus.Text = $"{_gosterilenHareketler.Count} / {_tumHareketler.Count} kayıt gösteriliyor (filtreli).";
    }

    private void btnFiltreTemizle_Click(object? sender, EventArgs e)
    {
        cmbFiltrePlaka.SelectedIndex = 0;
        chkFiltreTarih.Checked = false;
        txtFiltreHiz.Clear();
        txtFiltreKm.Clear();

        _gosterilenHareketler = _tumHareketler;
        _suankiSayfa = 1;
        GuncelleSayfalamaVeGrid();
        lblStatus.Text = $"{_tumHareketler.Count} hareket kaydı yüklendi.";
    }
}
