using System.Windows;
using System.Windows.Controls;
using HunianKu.Data;
using HunianKu.Helpers;
using HunianKu.Models;
using HunianKu.Services;

namespace HunianKu.Views.Pages;


public partial class KomplainKelolaPage : UserControl
{
    private const string BelumSelesai = "Belum Selesai";
    private const string Semua = "Semua";
    private Komplain? _terpilih;

    public KomplainKelolaPage()
    {
        InitializeComponent();
        try
        {
            CmbTeknisi.ItemsSource = KomplainRepository.GetTeknisiAktif();
        }
        catch (Exception ex)
        {
            Pesan.Error(ex);
        }
        CmbStatus.ItemsSource = StatusKomplain.Semua;
        CmbFilter.ItemsSource = new[] { BelumSelesai, StatusKomplain.Diajukan, StatusKomplain.Diproses, StatusKomplain.Selesai, Semua };
        CmbFilter.SelectedIndex = 0;   // memicu MuatData
    }

    private void CmbFilter_SelectionChanged(object sender, SelectionChangedEventArgs e) => MuatData();

    private void MuatData()
    {
        try
        {
            var filter = CmbFilter.SelectedItem as string;
            var data = filter is BelumSelesai or Semua ? KomplainRepository.GetAll() : KomplainRepository.GetAll(filter);
            if (filter == BelumSelesai)
                data = data.Where(k => k.Status != StatusKomplain.Selesai).ToList();
            DgKomplain.ItemsSource = data;
            KosongkanForm();
        }
        catch (Exception ex)
        {
            Pesan.Error(ex);
        }
    }

    private void KosongkanForm()
    {
        _terpilih = null;
        TxtJudulTiket.Text = "Penanganan Komplain";
        TxtDetail.Text = "Pilih tiket di tabel.";
        CmbTeknisi.SelectedIndex = -1;
        CmbStatus.SelectedIndex = -1;
        TxtCatatan.Clear();
        BtnSimpan.IsEnabled = false;
    }

    private void DgKomplain_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (DgKomplain.SelectedItem is not Komplain k) return;
        _terpilih = k;
        TxtJudulTiket.Text = $"{k.NomorTiket} · {k.Judul}";
        TxtDetail.Text =
            $"Unit {k.KodeUnit} · {k.NamaPenghuni}\n" +
            $"Kategori {k.Kategori} · Prioritas {k.Prioritas}\n" +
            $"Diajukan {k.TanggalAjukan:dd MMM yyyy HH:mm}" +
            (k.TanggalSelesai != null ? $"\nSelesai {k.TanggalSelesai:dd MMM yyyy HH:mm}" : "") +
            $"\n\n\"{k.Deskripsi}\"";

        CmbTeknisi.SelectedItem = ((List<Teknisi>?)CmbTeknisi.ItemsSource)?.FirstOrDefault(t => t.TeknisiId == k.TeknisiId);
        CmbStatus.SelectedItem = k.Status;
        TxtCatatan.Text = k.CatatanPenanganan;

        bool bisaDiubah = k.Status != StatusKomplain.Selesai;
        CmbTeknisi.IsEnabled = CmbStatus.IsEnabled = TxtCatatan.IsEnabled = BtnSimpan.IsEnabled = bisaDiubah;
    }

    private void Simpan_Click(object sender, RoutedEventArgs e)
    {
        if (_terpilih == null) return;
        try
        {
            var teknisi = CmbTeknisi.SelectedItem as Teknisi;
            var status = CmbStatus.SelectedItem as string ?? "";
            LayananService.TanganiKomplain(_terpilih, teknisi?.TeknisiId, status, TxtCatatan.Text);
            Pesan.Info($"Tiket {_terpilih.NomorTiket} diperbarui menjadi \"{status}\".");
            MuatData();
        }
        catch (Exception ex)
        {
            Pesan.Error(ex);
        }
    }
}
