using System.Windows;
using System.Windows.Controls;
using HunianKu.Data;
using HunianKu.Helpers;
using HunianKu.Models;
using HunianKu.Services;

namespace HunianKu.Views.Pages;

public partial class TagihanSayaPage : UserControl
{
    private Tagihan? _terpilih;

    public TagihanSayaPage()
    {
        InitializeComponent();
        CmbMetode.ItemsSource = MetodeBayar.Semua;
        CmbMetode.SelectedIndex = 0;
        MuatData();
    }

    private void MuatData()
    {
        try
        {
            TagihanRepository.HitungDenda();
            DgTagihan.ItemsSource = TagihanRepository.GetByPenghuni(Session.PenghuniId!.Value);
            _terpilih = null;
            GridRincian.Visibility = PanelBayar.Visibility = Visibility.Collapsed;
            TxtKosong.Visibility = Visibility.Visible;
            TxtJudulRincian.Text = "Rincian Tagihan";
        }
        catch (Exception ex)
        {
            Pesan.Error(ex);
        }
    }

    private void DgTagihan_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (DgTagihan.SelectedItem is not Tagihan t) return;
        _terpilih = t;

        TxtJudulRincian.Text = $"{t.NomorTagihan} · {t.PeriodeTeks}";
        TxtSewa.Text = Format.Rupiah(t.BiayaSewa);
        LblListrik.Text = $"Listrik ({t.PemakaianListrik:N0} kWh)";
        TxtListrik.Text = Format.Rupiah(t.BiayaListrik);
        LblAir.Text = $"Air ({t.PemakaianAir:N0} m³)";
        TxtAir.Text = Format.Rupiah(t.BiayaAir);
        TxtIpl.Text = Format.Rupiah(t.BiayaIpl);
        TxtSubtotal.Text = Format.Rupiah(t.TotalTagihan);
        LblDenda.Text = t.HariTerlambat > 0 ? $"Denda ({t.HariTerlambat} hari terlambat)" : "Denda";
        TxtDenda.Text = Format.Rupiah(t.Denda);
        TxtGrandTotal.Text = Format.Rupiah(t.GrandTotal);
        TxtJatuhTempo.Text = $"Jatuh tempo {Format.Tanggal(t.TanggalJatuhTempo)} · Status: {t.Status}";

        TxtKosong.Visibility = Visibility.Collapsed;
        GridRincian.Visibility = Visibility.Visible;
        PanelBayar.Visibility = t.Status == StatusTagihan.BelumBayar ? Visibility.Visible : Visibility.Collapsed;
        TxtReferensi.Clear();
    }

    private void Bayar_Click(object sender, RoutedEventArgs e)
    {
        if (_terpilih == null) return;
        if (!Pesan.Konfirmasi($"Kirim pembayaran tagihan {_terpilih.PeriodeTeks} sebesar {Format.Rupiah(_terpilih.GrandTotal)}?\n\n" +
                              "Denda dihitung sampai hari ini dan tidak bertambah selama menunggu verifikasi.")) return;
        try
        {
            TagihanService.AjukanPembayaran(_terpilih, CmbMetode.SelectedItem as string, TxtReferensi.Text);
            Pesan.Info("Pembayaran terkirim dan sedang menunggu verifikasi staf.");
            MuatData();
        }
        catch (Exception ex)
        {
            Pesan.Error(ex);
        }
    }
}