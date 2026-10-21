using System.Windows;
using System.Windows.Controls;
using HunianKu.Data;
using HunianKu.Helpers;
using HunianKu.Models;
using HunianKu.Services;

namespace HunianKu.Views.Pages;


public partial class UnitPage : UserControl
{
    private const string SemuaStatus = "Semua Status";
    private Unit? _terpilih;
    private bool _siap;   

    public UnitPage()
    {
        InitializeComponent();
        try
        {
            var tower = TowerRepository.GetAll();
            CmbTower.ItemsSource = tower;
            CmbTipe.ItemsSource = TipeUnitRepository.GetAll();
            CmbStatus.ItemsSource = StatusUnit.Semua;

            
            var filterTower = new List<Tower> { new() { TowerId = 0, NamaTower = "Semua Tower" } };
            filterTower.AddRange(tower);
            CmbFilterTower.ItemsSource = filterTower;
            CmbFilterTower.SelectedIndex = 0;
            CmbFilterStatus.ItemsSource = new[] { SemuaStatus }.Concat(StatusUnit.Semua).ToList();
            CmbFilterStatus.SelectedIndex = 0;
        }
        catch (Exception ex)
        {
            Pesan.Error(ex);
        }
        _siap = true;
        KosongkanForm();
        MuatData();
    }

    private void MuatData()
    {
        try
        {
            int? towerId = CmbFilterTower.SelectedItem is Tower { TowerId: > 0 } t ? t.TowerId : null;
            string? status = CmbFilterStatus.SelectedItem as string is { } s && s != SemuaStatus ? s : null;

            var data = UnitRepository.GetAll(towerId, status);
            DgUnit.ItemsSource = data;
            TxtJumlah.Text = $"{data.Count} unit ditampilkan";
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

    private void DgUnit_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (DgUnit.SelectedItem is not Unit u) return;
        _terpilih = u;
        CmbTower.SelectedItem = ((List<Tower>)CmbTower.ItemsSource).FirstOrDefault(t => t.TowerId == u.TowerId);
        CmbTipe.SelectedItem = ((List<TipeUnit>)CmbTipe.ItemsSource).FirstOrDefault(t => t.TipeUnitId == u.TipeUnitId);
        TxtNomor.Text = u.NomorUnit;
        TxtLantai.Text = u.Lantai.ToString();
        CmbStatus.SelectedItem = u.Status;
        TxtJudulForm.Text = $"Ubah Unit {u.KodeUnit}";
        BtnHapus.IsEnabled = true;
    }

    private void Baru_Click(object sender, RoutedEventArgs e) => KosongkanForm();

    private void KosongkanForm()
    {
        _terpilih = null;
        DgUnit.SelectedItem = null;
        CmbTower.SelectedIndex = -1;
        CmbTipe.SelectedIndex = -1;
        TxtNomor.Clear();
        TxtLantai.Clear();
        CmbStatus.SelectedItem = StatusUnit.Kosong;
        TxtJudulForm.Text = "Tambah Unit";
        BtnHapus.IsEnabled = false;
    }

    private void Simpan_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            var unit = new Unit
            {
                UnitId = _terpilih?.UnitId ?? 0,
                TowerId = Input.Pilih<Tower>(CmbTower.SelectedItem, "tower").TowerId,
                TipeUnitId = Input.Pilih<TipeUnit>(CmbTipe.SelectedItem, "tipe unit").TipeUnitId,
                NomorUnit = TxtNomor.Text,
                Lantai = Input.Bulat(TxtLantai.Text, "Lantai", 0, 100),
                Status = CmbStatus.SelectedItem as string ?? StatusUnit.Kosong
            };
            MasterDataService.SimpanUnit(unit);
            Pesan.Info("Data unit berhasil disimpan.");
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
        if (!Pesan.Konfirmasi($"Hapus unit {_terpilih.KodeUnit}?")) return;
        try
        {
            MasterDataService.HapusUnit(_terpilih);
            KosongkanForm();
            MuatData();
        }
        catch (Exception ex)
        {
            Pesan.Error(ex);
        }
    }
}
