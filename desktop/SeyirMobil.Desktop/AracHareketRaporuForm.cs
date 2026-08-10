using SeyirMobil.Desktop.Models;
using SeyirMobil.Desktop.Services;

namespace SeyirMobil.Desktop;

public partial class AracHareketRaporuForm : Form
{
    private readonly AracHareketApiClient _apiClient = new();

    public AracHareketRaporuForm()
    {
        InitializeComponent();
        ConfigureGridHeaderStyle();
        SetupOzetGridColumns();
    }

    private void ConfigureGridHeaderStyle()
    {
        dgvRapor.ColumnHeadersVisible = true;
        dgvRapor.EnableHeadersVisualStyles = false;
        dgvRapor.ColumnHeadersDefaultCellStyle.BackColor = Color.WhiteSmoke;
        dgvRapor.ColumnHeadersDefaultCellStyle.ForeColor = Color.Black;
        dgvRapor.ColumnHeadersDefaultCellStyle.Font = new Font(dgvRapor.Font, FontStyle.Bold);
        dgvRapor.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
        dgvRapor.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
        dgvRapor.ColumnHeadersHeight = 32;
    }

    // Ozet mod: her plaka icin tek satir (baslangic/bitis/yapilan km).
    private void SetupOzetGridColumns()
    {
        dgvRapor.DataSource = null;
        dgvRapor.Columns.Clear();
        dgvRapor.Columns.Add(new DataGridViewTextBoxColumn
        {
            Name = "AracPlaka",
            DataPropertyName = "AracPlaka",
            HeaderText = "Araç Plakası",
            Width = 180
        });
        dgvRapor.Columns.Add(new DataGridViewTextBoxColumn
        {
            Name = "BaslangicKm",
            DataPropertyName = "BaslangicKm",
            HeaderText = "Başlangıç Km",
            Width = 180
        });
        dgvRapor.Columns.Add(new DataGridViewTextBoxColumn
        {
            Name = "BitisKm",
            DataPropertyName = "BitisKm",
            HeaderText = "Bitiş Km",
            Width = 180
        });
        dgvRapor.Columns.Add(new DataGridViewTextBoxColumn
        {
            Name = "YapilanKm",
            DataPropertyName = "YapilanKm",
            HeaderText = "Yapılan Km",
            Width = 180
        });
    }

    // Detay mod: secilen araliktaki HER gercek okuma, bir onceki okumaya gore artisiyla.
    private void SetupDetayGridColumns()
    {
        dgvRapor.DataSource = null;
        dgvRapor.Columns.Clear();
        dgvRapor.Columns.Add(new DataGridViewTextBoxColumn
        {
            Name = "AracPlaka",
            DataPropertyName = "AracPlaka",
            HeaderText = "Araç Plakası",
            Width = 150
        });
        dgvRapor.Columns.Add(new DataGridViewTextBoxColumn
        {
            Name = "VeriTarihi",
            DataPropertyName = "VeriTarihi",
            HeaderText = "Veri Tarihi",
            Width = 150
        });
        dgvRapor.Columns.Add(new DataGridViewTextBoxColumn
        {
            Name = "KmSayaci",
            DataPropertyName = "KmSayaci",
            HeaderText = "Km Sayacı",
            Width = 150
        });
        dgvRapor.Columns.Add(new DataGridViewTextBoxColumn
        {
            Name = "Artis",
            DataPropertyName = "Artis",
            HeaderText = "Bir Önceki Okumaya Göre Artış",
            Width = 240
        });
    }

    private async void AracHareketRaporuForm_Load(object? sender, EventArgs e)
    {
        lblStatus.Text = "Araç listesi yükleniyor...";
        try
        {
            var plakalar = await _apiClient.GetPlakalarAsync();
            cblPlakalar.Items.Clear();
            foreach (var p in plakalar)
            {
                cblPlakalar.Items.Add(p.AracPlaka);
            }
            lblStatus.Text = $"{plakalar.Count} araç listelendi.";
        }
        catch (Exception ex)
        {
            if (HataYardimcisi.OturumSuresiDolduMu(ex))
            {
                return;
            }
            lblStatus.Text = "Araç listesi yüklenemedi.";
            MessageBox.Show(
                $"Araç listesi alınamadı. Backend API çalışıyor mu?\n\nHata: {ex.Message}",
                "Bağlantı Hatası",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }
    }

    private void cblPlakalar_ItemCheck(object? sender, ItemCheckEventArgs e)
    {
        // ItemCheck, kontrolun kendi Checked durumu guncellenmeden ONCE tetikleniyor -
        // butonun dogru enable/disable durumuna gecmesi icin guncellemeyi bu event
        // tamamen bittikten SONRA (BeginInvoke ile) yapiyoruz.
        BeginInvoke(new MethodInvoker(UpdateRaporButonDurumu));
    }

    private void dtpBaslangic_ValueChanged(object? sender, EventArgs e)
    {
        // Ikinci tarih secici grubu, ilk tarih secilene kadar gizli - ilk kez deger degisince
        // (ya da form yuklenirken varsayilan deger atanirken) gorunur hale getiriliyor.
        groupBitis.Visible = true;

        // Bitis tarihi, baslangictan KESINLIKLE sonraki bir gun olmak zorunda.
        var minBitis = dtpBaslangic.Value.Date.AddDays(1);
        dtpBitis.MinDate = minBitis;
        if (dtpBitis.Value < minBitis)
        {
            dtpBitis.Value = minBitis;
        }

        UpdateRaporButonDurumu();
    }

    private void dtpBitis_ValueChanged(object? sender, EventArgs e)
    {
        UpdateRaporButonDurumu();
    }

    private void UpdateRaporButonDurumu()
    {
        btnRaporOlustur.Enabled = cblPlakalar.CheckedItems.Count > 0 && groupBitis.Visible;
    }

    private void btnGeri_Click(object? sender, EventArgs e)
    {
        Close();
    }

    private async void btnRaporOlustur_Click(object? sender, EventArgs e)
    {
        var secilenPlakalar = cblPlakalar.CheckedItems.Cast<string>().ToList();
        var baslangic = DateOnly.FromDateTime(dtpBaslangic.Value);
        var bitis = DateOnly.FromDateTime(dtpBitis.Value);

        btnRaporOlustur.Enabled = false;
        lblStatus.Text = "Rapor oluşturuluyor...";
        try
        {
            if (chkDetayliRapor.Checked)
            {
                SetupDetayGridColumns();
                var satirlar = await _apiClient.GetDetayRaporuAsync(secilenPlakalar, baslangic, bitis);
                dgvRapor.DataSource = satirlar.Select(s => new
                {
                    s.AracPlaka,
                    VeriTarihi = s.VeriTarihi.ToString("dd.MM.yyyy"),
                    KmSayaci = s.KmSayaci.ToString("N2"),
                    Artis = s.Artis.HasValue ? s.Artis.Value.ToString("N2") : "-"
                }).ToList();
                lblStatus.Text = $"{satirlar.Count} okuma için detaylı rapor oluşturuldu.";
            }
            else
            {
                SetupOzetGridColumns();
                var sonuclar = await _apiClient.GetRaporTopluAsync(secilenPlakalar, baslangic, bitis);
                dgvRapor.DataSource = sonuclar.Select(s => new
                {
                    s.AracPlaka,
                    BaslangicKm = s.BulunduMu ? s.BaslangicKm!.Value.ToString("N2") : "Veri yok",
                    BitisKm = s.BulunduMu ? s.BitisKm!.Value.ToString("N2") : "Veri yok",
                    YapilanKm = s.BulunduMu ? s.YapilanKm!.Value.ToString("N2") : "Veri yok"
                }).ToList();
                lblStatus.Text = $"{sonuclar.Count} araç için rapor oluşturuldu.";
            }
        }
        catch (Exception ex)
        {
            if (HataYardimcisi.OturumSuresiDolduMu(ex))
            {
                return;
            }
            lblStatus.Text = "Rapor oluşturulamadı.";
            MessageBox.Show(
                $"Rapor oluşturulamadı.\n\nHata: {ex.Message}",
                "Hata",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }
        finally
        {
            btnRaporOlustur.Enabled = cblPlakalar.CheckedItems.Count > 0 && groupBitis.Visible;
        }
    }

    private async void btnExcelAktar_Click(object? sender, EventArgs e)
    {
        var secilenPlakalar = cblPlakalar.CheckedItems.Cast<string>().ToList();
        if (secilenPlakalar.Count == 0 || !groupBitis.Visible)
        {
            MessageBox.Show("Önce en az bir plaka ve geçerli bir tarih aralığı seçmelisin.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        var modAdi = chkDetayliRapor.Checked ? "detayli" : "ozet";
        using var dialog = new SaveFileDialog
        {
            Filter = "Excel Dosyası (*.xlsx)|*.xlsx",
            FileName = $"rapor-{modAdi}.xlsx"
        };
        if (dialog.ShowDialog(this) != DialogResult.OK)
        {
            return;
        }

        var request = new RaporExportRequestDto(
            secilenPlakalar,
            DateOnly.FromDateTime(dtpBaslangic.Value),
            DateOnly.FromDateTime(dtpBitis.Value),
            chkDetayliRapor.Checked,
            cmbExportModu.SelectedIndex == 0);

        btnExcelAktar.Enabled = false;
        lblStatus.Text = "Excel oluşturuluyor...";
        try
        {
            var veri = await _apiClient.ExportRaporAsync(request);
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
}
