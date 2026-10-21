using System.Windows;
using System.Windows.Controls;
using HunianKu.Data;
using HunianKu.Helpers;
using HunianKu.Models;
using HunianKu.Services;

namespace HunianKu.Views.Pages;

public partial class VerifikasiPembayaranPage : UserControl
{
    private const string SemuaStatus = "Semua Status";
    private Pembayaran? _terpilih;

    public VerifikasiPembayaranPage()
    {
        InitializeComponent();
        CmbStatus.ItemsSource = new[] { StatusPembayaran.Menunggu, StatusPembayaran.Diterima, StatusPembayaran.Ditolak, SemuaStatus };
        CmbStatus.SelectedIndex = 0;
    }

    private void CmbStatus_SelectionChanged(object sender, SelectionChangedEventArgs e) => MuatData();

    private void MuatData()
    {
        try
        {
            string? status = CmbStatus.SelectedItem as string is { } s && s != SemuaStatus ? s : null;
            DgPembayaran.ItemsSource = PembayaranRepository.GetAll(status);
            _terpilih = null;
            TxtDetail.Text = "Pilih pembayaran di tabel.";
            TxtCatatan.Clear();
            BtnTerima.IsEnabled = BtnTolak.IsEnabled = false;
        }
        catch (Exception ex)
        {
            Pesan.Error(ex);
        }
    }

    private void DgPembayaran_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (DgPembayaran.SelectedItem is not Pembayaran p) return;
        _terpilih = p;
        TxtDetail.Text =
            $"Tagihan: {p.NomorTagihan} ({p.PeriodeTeks})\n" +
            $"Penghuni: {p.NamaPenghuni} - {p.KodeUnit}\n" +
            $"Tanggal bayar: {Format.Tanggal(p.TanggalBayar)}\n" +
            $"Jumlah dibayar: {Format.Rupiah(p.JumlahBayar)}\n" +
            $"Metode: {p.MetodeBayar} {(p.NomorReferensi != null ? "(" + p.NomorReferensi + ")" : "")}\n" +
            $"Status: {p.Status}" +
            (p.TanggalVerifikasi != null ? $"\nDiverifikasi: {p.NamaVerifikator}, {p.TanggalVerifikasi:dd MMM yyyy HH:mm}" : "");
        TxtCatatan.Text = p.Catatan;

        bool menunggu = p.Status == StatusPembayaran.Menunggu;
        BtnTerima.IsEnabled = BtnTolak.IsEnabled = TxtCatatan.IsEnabled = menunggu;
    }

    private void Terima_Click(object sender, RoutedEventArgs e) => Verifikasi(true);
    private void Tolak_Click(object sender, RoutedEventArgs e) => Verifikasi(false);

    private void Verifikasi(bool diterima)
    {
        if (_terpilih == null) return;
        string aksi = diterima ? "MENERIMA" : "MENOLAK";
        if (!Pesan.Konfirmasi($"{aksi} pembayaran {Format.Rupiah(_terpilih.JumlahBayar)} dari {_terpilih.NamaPenghuni}?")) return;
        try
        {
            TagihanService.Verifikasi(_terpilih, diterima, TxtCatatan.Text);
            Pesan.Info(diterima ? "Pembayaran diterima. Tagihan sudah Lunas." : "Pembayaran ditolak. Tagihan kembali Belum Bayar.");
            MuatData();
        }
        catch (Exception ex)
        {
            Pesan.Error(ex);
        }
    }
}