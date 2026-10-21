using System.Windows;
using System.Windows.Controls;
using HunianKu.Data;
using HunianKu.Helpers;
using HunianKu.Models;
using HunianKu.Services;

namespace HunianKu.Views.Pages;

public partial class TipeUnitPage : UserControl
{
    private TipeUnit? _terpilih;

    public TipeUnitPage()
    {
        InitializeComponent();
        MuatData();
    }

    private void MuatData()
    {
        try
        {
            DgTipe.ItemsSource = TipeUnitRepository.GetAll();
        }
        catch (Exception ex)
        {
            Pesan.Error(ex);
        }
    }

    private void DgTipe_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (DgTipe.SelectedItem is not TipeUnit t) return;
        _terpilih = t;
        TxtNama.Text = t.NamaTipe;
        TxtLuas.Text = t.LuasM2.ToString("0.##");
        TxtKamar.Text = t.JumlahKamar.ToString();
        TxtHarga.Text = t.HargaSewaBulanan.ToString("0");
        TxtIpl.Text = t.IplPerBulan.ToString("0");
        TxtJudulForm.Text = $"Ubah Tipe {t.NamaTipe}";
        BtnHapus.IsEnabled = true;
    }

    private void Baru_Click(object sender, RoutedEventArgs e) => KosongkanForm();

    private void KosongkanForm()
    {
        _terpilih = null;
        DgTipe.SelectedItem = null;
        TxtNama.Clear();
        TxtLuas.Clear();
        TxtKamar.Clear();
        TxtHarga.Clear();
        TxtIpl.Clear();
        TxtJudulForm.Text = "Tambah Tipe Unit";
        BtnHapus.IsEnabled = false;
        TxtNama.Focus();
    }

    private void Simpan_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            var tipe = new TipeUnit
            {
                TipeUnitId = _terpilih?.TipeUnitId ?? 0,
                NamaTipe = TxtNama.Text,
                LuasM2 = Input.Desimal(TxtLuas.Text, "Luas"),
                JumlahKamar = Input.Bulat(TxtKamar.Text, "Jumlah kamar", 0, 10),
                HargaSewaBulanan = Input.Desimal(TxtHarga.Text, "Harga sewa"),
                IplPerBulan = Input.Desimal(TxtIpl.Text, "IPL")
            };
            MasterDataService.SimpanTipeUnit(tipe);
            Pesan.Info("Data tipe unit berhasil disimpan.");
            KosongkanForm();
            MuatData();
        }
        catch (Exception ex)
        {
            Pesan.Error(ex);
        }
    }

    private void Hapus_Click(object sender, RoutedEventArgs e)
    {
        if (_terpilih == null) return;
        if (!Pesan.Konfirmasi($"Hapus tipe unit {_terpilih.NamaTipe}?")) return;
        try
        {
            MasterDataService.HapusTipeUnit(_terpilih);
            KosongkanForm();
            MuatData();
        }
        catch (Exception ex)
        {
            Pesan.Error(ex);   
        }
    }
}
