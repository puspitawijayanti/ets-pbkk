using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using HunianKu.Data;
using HunianKu.Helpers;
using HunianKu.Models;
using HunianKu.Services;

namespace HunianKu.Views.Pages;

public partial class KontrakPage : UserControl
{
    private const string SemuaStatus = "Semua Status";
    private Kontrak? _terpilih;
    private int _hariPengingat = 30;
    private bool _siap;

    public KontrakPage()
    {
        InitializeComponent();
        CmbFilterStatus.ItemsSource = new[] { SemuaStatus, StatusKontrak.Aktif, StatusKontrak.Selesai, StatusKontrak.Dibatalkan };
        CmbFilterStatus.SelectedIndex = 0;
        CmbLama.ItemsSource = new[] { "6 bulan", "12 bulan", "24 bulan" };
        try
        {
            _hariPengingat = (int)LaporanRepository.GetNilaiTarif(KodeTarif.PengingatKontrakHari);
        }
        catch (Exception ex)
        {
            Pesan.Error(ex);
        }
        _siap = true;
        TampilkanModeBaru();
        MuatData();
    }

    private void MuatData()
    {
        try
        {
            string? status = CmbFilterStatus.SelectedItem as string is { } s && s != SemuaStatus ? s : null;
            DgKontrak.ItemsSource = KontrakRepository.GetAll(status, TxtCari.Text);
        }
        catch (Exception ex)
        {
            Pesan.Error(ex);
        }
    }

    private void Filter_Changed(object sender, SelectionChangedEventArgs e)
    {
        if (_siap) MuatData();
    }

    private void TxtCari_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter) MuatData();
    }

    private void DgKontrak_LoadingRow(object? sender, DataGridRowEventArgs e)
    {
        if (e.Row.Item is Kontrak k && k.Status == StatusKontrak.Aktif && k.SisaHari <= _hariPengingat)
            e.Row.Background = (Brush)FindResource("WarningSoft");
        else
            e.Row.ClearValue(DataGridRow.BackgroundProperty);
    }

    private void Baru_Click(object sender, RoutedEventArgs e) => TampilkanModeBaru();

    private void TampilkanModeBaru()
    {
        _terpilih = null;
        DgKontrak.SelectedItem = null;
        PanelBaru.Visibility = Visibility.Visible;
        PanelDetail.Visibility = Visibility.Collapsed;
        try
        {
            CmbPenghuni.ItemsSource = PenghuniRepository.GetTanpaKontrakAktif();
            CmbUnit.ItemsSource = UnitRepository.GetAll(status: StatusUnit.Kosong);
        }
        catch (Exception ex)
        {
            Pesan.Error(ex);
        }
        TxtHarga.Clear();
        TxtDeposit.Clear();
        DpMulai.SelectedDate = DateTime.Today;
        CmbLama.SelectedIndex = 1;   // default 12 bulan
    }

    private void CmbUnit_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (CmbUnit.SelectedItem is not Unit u) return;
        TxtHarga.Text = u.HargaSewaBulanan.ToString("0");
        TxtDeposit.Text = (u.HargaSewaBulanan * 2).ToString("0");
    }

    private void DpMulai_Changed(object? sender, SelectionChangedEventArgs e) => HitungTanggalSelesai();
    private void CmbLama_SelectionChanged(object sender, SelectionChangedEventArgs e) => HitungTanggalSelesai();

    private void HitungTanggalSelesai()
    {
        if (DpMulai.SelectedDate is not DateTime mulai || CmbLama.SelectedItem is not string lama) return;
        int bulan = int.Parse(lama.Split(' ')[0]);
        DpSelesai.SelectedDate = mulai.AddMonths(bulan).AddDays(-1);
    }

    private void Buat_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            var penghuni = Input.Pilih<Penghuni>(CmbPenghuni.SelectedItem, "penghuni");
            var unit = Input.Pilih<Unit>(CmbUnit.SelectedItem, "unit");
            var kontrak = new Kontrak
            {
                PenghuniId = penghuni.PenghuniId,
                UnitId = unit.UnitId,
                TanggalMulai = Input.Tanggal(DpMulai.SelectedDate, "Tanggal mulai"),
                TanggalSelesai = Input.Tanggal(DpSelesai.SelectedDate, "Tanggal selesai"),
                HargaSewaBulanan = Input.Desimal(TxtHarga.Text, "Harga sewa"),
                Deposit = Input.Desimal(TxtDeposit.Text, "Deposit")
            };
            KontrakService.Buat(kontrak);
            Pesan.Info($"Kontrak untuk {penghuni.NamaLengkap} di unit {unit.KodeUnit} berhasil dibuat.\nStatus unit otomatis menjadi Terisi.");
            TampilkanModeBaru();
            MuatData();
        }
        catch (Exception ex)
        {
            Pesan.Error(ex);
        }
    }

    private void DgKontrak_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (DgKontrak.SelectedItem is not Kontrak k) return;
        _terpilih = k;
        PanelBaru.Visibility = Visibility.Collapsed;
        PanelDetail.Visibility = Visibility.Visible;

        TxtJudulDetail.Text = $"Kontrak {k.NomorKontrak}";
        TxtInfoDetail.Text =
            $"Penghuni: {k.NamaPenghuni} ({k.NoHp})\n" +
            $"Unit: {k.KodeUnit} - {k.NamaTipe}\n" +
            $"Periode: {Format.Tanggal(k.TanggalMulai)} s/d {Format.Tanggal(k.TanggalSelesai)}\n" +
            $"Sewa: {Format.Rupiah(k.HargaSewaBulanan)} / bulan\n" +
            $"Status: {k.Status}";
        DpSelesaiEdit.SelectedDate = k.TanggalSelesai;
        TxtDepositEdit.Text = k.Deposit.ToString("0");

        bool aktif = k.Status == StatusKontrak.Aktif;
        DpSelesaiEdit.IsEnabled = TxtDepositEdit.IsEnabled = aktif;
        BtnSimpanEdit.IsEnabled = BtnAkhiri.IsEnabled = BtnBatalkan.IsEnabled = aktif;
    }

    private void SimpanEdit_Click(object sender, RoutedEventArgs e)
    {
        if (_terpilih == null) return;
        try
        {
            KontrakService.Ubah(_terpilih,
                Input.Tanggal(DpSelesaiEdit.SelectedDate, "Tanggal selesai"),
                Input.Desimal(TxtDepositEdit.Text, "Deposit"));
            Pesan.Info("Perubahan kontrak berhasil disimpan.");
            MuatData();
            TampilkanModeBaru();
        }
        catch (Exception ex)
        {
            Pesan.Error(ex);
        }
    }

    private void Akhiri_Click(object sender, RoutedEventArgs e) => AkhiriKontrak(dibatalkan: false);
    private void Batalkan_Click(object sender, RoutedEventArgs e) => AkhiriKontrak(dibatalkan: true);

    private void AkhiriKontrak(bool dibatalkan)
    {
        if (_terpilih == null) return;
        string aksi = dibatalkan ? "MEMBATALKAN" : "MENGAKHIRI";
        if (!Pesan.Konfirmasi($"Yakin {aksi} kontrak {_terpilih.NomorKontrak} ({_terpilih.NamaPenghuni})?\n" +
                              $"Unit {_terpilih.KodeUnit} akan kembali berstatus Kosong.")) return;
        try
        {
            KontrakService.Akhiri(_terpilih, dibatalkan);
            Pesan.Info("Status kontrak berhasil diperbarui.");
            MuatData();
            TampilkanModeBaru();
        }
        catch (Exception ex)
        {
            Pesan.Error(ex);
        }
    }
}
