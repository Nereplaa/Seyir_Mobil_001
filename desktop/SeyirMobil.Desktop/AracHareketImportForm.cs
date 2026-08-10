using SeyirMobil.Desktop.Models;
using SeyirMobil.Desktop.Services;

namespace SeyirMobil.Desktop;

// Gorsel tasarimci (Designer.cs) yerine dogrudan kod ile olusturuldu - bu ekran diger
// formlara gore cok daha dinamik bir grid (satir sayisi/hata durumu her doldurmada
// degisiyor), sabit tasarimci duzeninden cok, kod icinde kurulan bir DataGridView'a
// daha uygun. Diger formlarla AYNI FlowLayoutPanel/DataGridView(Dock=Fill) deseni korunuyor.
public class AracHareketImportForm : Form
{
    private readonly AracHareketApiClient _apiClient = new();
    private readonly FlowLayoutPanel _flowUst = new();
    private readonly Button _btnDosyaSec = new();
    private readonly Button _btnYenidenDogrula = new();
    private readonly Button _btnIceAktar = new();
    private readonly Label _lblStatus = new();
    private readonly DataGridView _grid = new();

    private List<ImportSatiriSonucDto> _satirlar = [];
    // SatirNo -> kullanicinin sectigi cakisma aksiyonu ("UzerineYaz" | "Atla" | "").
    private readonly Dictionary<int, string> _cakismaAksiyonlari = [];
    private string? _seciliDosyaYolu;

    public AracHareketImportForm()
    {
        Text = "Excel'den Toplu Veri Girişi";
        Width = 900;
        Height = 600;
        StartPosition = FormStartPosition.CenterParent;
        DoubleBuffered = true;

        SetupUstPanel();
        SetupGrid();
        SetupStatusLabel();

        Controls.Add(_grid);
        Controls.Add(_lblStatus);
        Controls.Add(_flowUst);
    }

    private void SetupUstPanel()
    {
        _flowUst.Dock = DockStyle.Top;
        _flowUst.AutoSize = true;
        _flowUst.WrapContents = true;
        _flowUst.Padding = new Padding(8);

        _btnDosyaSec.Text = "Dosya Seç ve Doğrula...";
        _btnDosyaSec.Size = new Size(180, 30);
        _btnDosyaSec.Margin = new Padding(3, 3, 12, 3);
        _btnDosyaSec.Click += BtnDosyaSec_Click;

        _btnYenidenDogrula.Text = "Yeniden Doğrula";
        _btnYenidenDogrula.Size = new Size(140, 30);
        _btnYenidenDogrula.Margin = new Padding(3);
        _btnYenidenDogrula.Enabled = false;
        _btnYenidenDogrula.Click += BtnYenidenDogrula_Click;

        _btnIceAktar.Text = "İçe Aktar";
        _btnIceAktar.Size = new Size(120, 30);
        _btnIceAktar.Margin = new Padding(3);
        _btnIceAktar.Enabled = false;
        _btnIceAktar.Click += BtnIceAktar_Click;

        _flowUst.Controls.Add(_btnDosyaSec);
        _flowUst.Controls.Add(_btnYenidenDogrula);
        _flowUst.Controls.Add(_btnIceAktar);
    }

    private void SetupStatusLabel()
    {
        _lblStatus.Dock = DockStyle.Top;
        _lblStatus.AutoSize = false;
        _lblStatus.Height = 24;
        _lblStatus.Padding = new Padding(8, 0, 8, 0);
        _lblStatus.Text = "Sütunlar: AracPlaka, VeriTarihi, Hiz, KmSayaci (ilk satır başlık). Bir dosya seçin.";
    }

    private void SetupGrid()
    {
        _grid.Dock = DockStyle.Fill;
        _grid.AllowUserToAddRows = false;
        _grid.AllowUserToDeleteRows = false;
        _grid.AutoGenerateColumns = false;
        _grid.EnableHeadersVisualStyles = false;
        _grid.ColumnHeadersDefaultCellStyle.BackColor = Color.WhiteSmoke;
        _grid.ColumnHeadersDefaultCellStyle.Font = new Font(_grid.Font, FontStyle.Bold);
        _grid.ColumnHeadersHeight = 32;
        _grid.RowHeadersVisible = false;

        _grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "SatirNo", HeaderText = "#", Width = 40, ReadOnly = true });
        _grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "AracPlaka", HeaderText = "Plaka", Width = 130 });
        _grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "VeriTarihi", HeaderText = "Tarih (yyyy-MM-dd)", Width = 130 });
        _grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Hiz", HeaderText = "Hız", Width = 70 });
        _grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "KmSayaci", HeaderText = "Km Sayacı", Width = 110 });
        _grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Durum", HeaderText = "Durum", Width = 160, ReadOnly = true });

        var cakismaCombo = new DataGridViewComboBoxColumn
        {
            Name = "CakismaAksiyonu",
            HeaderText = "Çakışma Aksiyonu",
            Width = 140,
            DisplayStyle = DataGridViewComboBoxDisplayStyle.ComboBox
        };
        cakismaCombo.Items.AddRange("— Seç —", "Üzerine Yaz", "Atla");
        _grid.Columns.Add(cakismaCombo);

        _grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Hata", HeaderText = "Hata", Width = 260, ReadOnly = true });

        _grid.CellValueChanged += Grid_CellValueChanged;
        _grid.CurrentCellDirtyStateChanged += (_, _) =>
        {
            if (_grid.IsCurrentCellDirty)
            {
                _grid.CommitEdit(DataGridViewDataErrorContexts.Commit);
            }
        };
    }

    private async void BtnDosyaSec_Click(object? sender, EventArgs e)
    {
        using var dialog = new OpenFileDialog
        {
            Filter = "Excel Dosyaları (*.xlsx)|*.xlsx",
            Title = "İçe aktarılacak Excel dosyasını seçin"
        };
        if (dialog.ShowDialog(this) != DialogResult.OK)
        {
            return;
        }
        _seciliDosyaYolu = dialog.FileName;
        await DogrulaVeGridiDoldurAsync(() => _apiClient.ImportOnizleAsync(_seciliDosyaYolu));
    }

    private async void BtnYenidenDogrula_Click(object? sender, EventArgs e)
    {
        var hamSatirlar = GridDenHamSatirlariOku();
        await DogrulaVeGridiDoldurAsync(() => _apiClient.ImportYenidenDogrulaAsync(hamSatirlar));
    }

    private async Task DogrulaVeGridiDoldurAsync(Func<Task<ImportOnizlemeYanitiDto>> dogrulamaCagrisi)
    {
        _btnDosyaSec.Enabled = false;
        _btnYenidenDogrula.Enabled = false;
        _btnIceAktar.Enabled = false;
        _lblStatus.Text = "Doğrulanıyor...";
        try
        {
            var yanit = await dogrulamaCagrisi();
            _satirlar = yanit.Satirlar;
            GridiDoldur();
            _lblStatus.Text = $"{_satirlar.Count} satır doğrulandı. {_satirlar.Count(HataliMi)} satırda sorun var.";
        }
        catch (Exception ex)
        {
            if (!HataYardimcisi.OturumSuresiDolduMu(ex))
            {
                MessageBox.Show(this, ex.Message, "Doğrulama başarısız", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        finally
        {
            _btnDosyaSec.Enabled = true;
            var satirVarMi = _satirlar.Count > 0;
            _btnYenidenDogrula.Enabled = satirVarMi;
            _btnIceAktar.Enabled = satirVarMi && !_satirlar.Any(HataliMi);
        }
    }

    private bool HataliMi(ImportSatiriSonucDto s)
    {
        // "Atla" secilen bir satir zaten veritabanina hic yazilmayacak - hatali olsun ya da
        // olmasin onemsiz, digerlerinin ice aktarilmasini ENGELLEMEMELI (kullanici karari,
        // 2026-08-07; backend'deki import-onayla de AYNI onceligi uyguluyor).
        if (_cakismaAksiyonlari.GetValueOrDefault(s.SatirNo) == "Atla")
        {
            return false;
        }
        if (s.Hatalar.Count > 0)
        {
            return true;
        }
        if (s.CakismaVarMi && string.IsNullOrEmpty(_cakismaAksiyonlari.GetValueOrDefault(s.SatirNo)))
        {
            return true;
        }
        return false;
    }

    private void GridiDoldur()
    {
        _grid.CellValueChanged -= Grid_CellValueChanged;
        _grid.Rows.Clear();
        foreach (var s in _satirlar)
        {
            var aksiyon = _cakismaAksiyonlari.GetValueOrDefault(s.SatirNo, "");
            var aksiyonMetin = aksiyon switch
            {
                "UzerineYaz" => "Üzerine Yaz",
                "Atla" => "Atla",
                _ => "— Seç —"
            };
            var rowIndex = _grid.Rows.Add(
                s.SatirNo, s.AracPlaka, s.VeriTarihi ?? "", (object?)s.Hiz ?? "", (object?)s.KmSayaci ?? "",
                DurumMetni(s, aksiyon), aksiyonMetin, string.Join(" ", s.Hatalar));

            _grid.Rows[rowIndex].Cells["CakismaAksiyonu"].ReadOnly = !s.CakismaVarMi;
            if (aksiyon == "Atla")
            {
                _grid.Rows[rowIndex].DefaultCellStyle.BackColor = Color.Honeydew;
            }
            else if (s.Hatalar.Count > 0)
            {
                _grid.Rows[rowIndex].DefaultCellStyle.BackColor = Color.MistyRose;
            }
            else if (s.CakismaVarMi && string.IsNullOrEmpty(aksiyon))
            {
                _grid.Rows[rowIndex].DefaultCellStyle.BackColor = Color.LightYellow;
            }
            else
            {
                _grid.Rows[rowIndex].DefaultCellStyle.BackColor = Color.Honeydew;
            }
        }
        _grid.CellValueChanged += Grid_CellValueChanged;
    }

    private static string DurumMetni(ImportSatiriSonucDto s, string aksiyon)
    {
        if (aksiyon == "Atla")
        {
            return "Atlanacak";
        }
        if (s.Hatalar.Count > 0)
        {
            return "Hata";
        }
        if (s.CakismaVarMi && string.IsNullOrEmpty(aksiyon))
        {
            return "Çakışma — karar bekliyor";
        }
        if (s.CakismaVarMi)
        {
            return "Üzerine yazılacak";
        }
        return s.YeniAracMi ? "Yeni araç" : "Hazır";
    }

    private void Grid_CellValueChanged(object? sender, DataGridViewCellEventArgs e)
    {
        if (e.RowIndex < 0)
        {
            return;
        }
        var row = _grid.Rows[e.RowIndex];
        var satirNo = Convert.ToInt32(row.Cells["SatirNo"].Value);

        if (_grid.Columns[e.ColumnIndex].Name == "CakismaAksiyonu")
        {
            var secilen = row.Cells["CakismaAksiyonu"].Value?.ToString() ?? "— Seç —";
            _cakismaAksiyonlari[satirNo] = secilen switch
            {
                "Üzerine Yaz" => "UzerineYaz",
                "Atla" => "Atla",
                _ => ""
            };
            // Satirin durumu/rengini (aksiyon secildikten sonra) taze veriden yeniden hesapla.
            var s = _satirlar.FirstOrDefault(x => x.SatirNo == satirNo);
            if (s is not null)
            {
                row.Cells["Durum"].Value = DurumMetni(s, _cakismaAksiyonlari[satirNo]);
                row.DefaultCellStyle.BackColor = string.IsNullOrEmpty(_cakismaAksiyonlari[satirNo]) ? Color.LightYellow : Color.Honeydew;
            }
        }
        _btnIceAktar.Enabled = _satirlar.Count > 0 && !_satirlar.Any(HataliMi);
    }

    private List<ImportHamSatirDto> GridDenHamSatirlariOku()
    {
        var sonuc = new List<ImportHamSatirDto>();
        foreach (DataGridViewRow row in _grid.Rows)
        {
            var satirNo = Convert.ToInt32(row.Cells["SatirNo"].Value);
            var plaka = row.Cells["AracPlaka"].Value?.ToString() ?? "";
            var tarih = row.Cells["VeriTarihi"].Value?.ToString() ?? "";
            var hizStr = row.Cells["Hiz"].Value?.ToString();
            var kmStr = row.Cells["KmSayaci"].Value?.ToString();
            int? hiz = int.TryParse(hizStr, out var h) ? h : null;
            decimal? km = decimal.TryParse(kmStr, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out var k) ? k : null;
            sonuc.Add(new ImportHamSatirDto(satirNo, plaka, tarih, hiz, km));
        }
        return sonuc;
    }

    private async void BtnIceAktar_Click(object? sender, EventArgs e)
    {
        if (_satirlar.Count == 0 || _satirlar.Any(HataliMi))
        {
            MessageBox.Show(this, "Tüm satırlar geçerli olmadan içe aktarılamaz.", "Eksik/hatalı satırlar var", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        var onay = MessageBox.Show(this, $"{_satirlar.Count} satır içe aktarılacak. Devam edilsin mi?",
            "Onaylıyor musunuz?", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
        if (onay != DialogResult.Yes)
        {
            return;
        }

        var hamSatirlar = GridDenHamSatirlariOku();
        var gonderilecek = hamSatirlar.Select(s => new ImportOnaylaSatiriDto(
            s.SatirNo, s.AracPlaka, s.VeriTarihi, s.Hiz ?? 0, s.KmSayaci ?? 0,
            _cakismaAksiyonlari.GetValueOrDefault(s.SatirNo, ""))).ToList();

        _btnIceAktar.Enabled = false;
        _lblStatus.Text = "İçe aktarılıyor...";
        try
        {
            var sonuc = await _apiClient.ImportOnaylaAsync(gonderilecek);
            MessageBox.Show(this,
                $"İçe aktarma tamamlandı:\n{sonuc.EklenenSayisi} eklendi, {sonuc.GuncellenenSayisi} güncellendi, {sonuc.AtlananSayisi} atlandı.",
                "Tamamlandı", MessageBoxButtons.OK, MessageBoxIcon.Information);
            _satirlar = [];
            _cakismaAksiyonlari.Clear();
            _grid.Rows.Clear();
            _lblStatus.Text = "Bir dosya seçin.";
            _btnYenidenDogrula.Enabled = false;
        }
        catch (Exception ex)
        {
            if (!HataYardimcisi.OturumSuresiDolduMu(ex))
            {
                MessageBox.Show(this, ex.Message, "İçe aktarılamadı", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            _btnIceAktar.Enabled = true;
        }
    }
}
