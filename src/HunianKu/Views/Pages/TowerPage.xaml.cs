using System.Windows;
using System.Windows.Controls;
using HunianKu.Data;
using HunianKu.Helpers;
using HunianKu.Models;
using HunianKu.Services;

namespace HunianKu.Views.Pages;

public partial class TowerPage : UserControl
{
    private Tower? _terpilih;   

    public TowerPage()
    {
        InitializeComponent();
        MuatData();
    }

    private void MuatData()
    {
        try
        {
            DgTower.ItemsSource = TowerRepository.GetAll();
        }
        catch (Exception ex)
        {
            Pesan.Error(ex);
        }
    }

    private void DgTower_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (DgTower.SelectedItem is not Tower t) return;
        _terpilih = t;
        TxtKode.Text = t.KodeTower;
        TxtNama.Text = t.NamaTower;
        TxtAlamat.Text = t.Alamat;
        TxtLantai.Text = t.JumlahLantai.ToString();
        TxtJudulForm.Text = $"Ubah Tower {t.KodeTower}";
        BtnHapus.IsEnabled = true;
    }

    private void Baru_Click(object sender, RoutedEventArgs e) => KosongkanForm();

    private void KosongkanForm()
    {
        _terpilih = null;
        DgTower.SelectedItem = null;
        TxtKode.Clear();
        TxtNama.Clear();
        TxtAlamat.Clear();
        TxtLantai.Clear();
        TxtJudulForm.Text = "Tambah Tower";
        BtnHapus.IsEnabled = false;
        TxtKode.Focus();
    }

    private void Simpan_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            var tower = new Tower
            {
                TowerId = _terpilih?.TowerId ?? 0,
                KodeTower = TxtKode.Text,
                NamaTower = TxtNama.Text,
                Alamat = TxtAlamat.Text,
                JumlahLantai = Input.Bulat(TxtLantai.Text, "Jumlah lantai", 1, 100)
            };
            MasterDataService.SimpanTower(tower);
            Pesan.Info("Data tower berhasil disimpan.");
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
        if (!Pesan.Konfirmasi($"Hapus tower {_terpilih.Tampilan}?")) return;
        try
        {
            MasterDataService.HapusTower(_terpilih);
            KosongkanForm();
            MuatData();
        }
        catch (Exception ex)
        {
            Pesan.Error(ex);
        }
    }
}
