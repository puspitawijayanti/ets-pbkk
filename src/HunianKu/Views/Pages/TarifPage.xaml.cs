using System.Windows;
using System.Windows.Controls;
using HunianKu.Data;
using HunianKu.Helpers;
using HunianKu.Models;
using HunianKu.Services;

namespace HunianKu.Views.Pages;

public partial class TarifPage : UserControl
{
    private Tarif? _terpilih;

    public TarifPage()
    {
        InitializeComponent();
        MuatData();
    }

    private void MuatData()
    {
        try
        {
            DgTarif.ItemsSource = LaporanRepository.GetTarif();
        }
        catch (Exception ex)
        {
            Pesan.Error(ex);
        }
    }

    private void DgTarif_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (DgTarif.SelectedItem is not Tarif t) return;
        _terpilih = t;
        TxtNamaTarif.Text = $"{t.NamaTarif} ({t.Satuan})";
        TxtNilai.Text = t.Nilai.ToString("0.##");
        BtnSimpan.IsEnabled = true;
    }

    private void Simpan_Click(object sender, RoutedEventArgs e)
    {
        if (_terpilih == null) return;
        try
        {
            TagihanService.UbahTarif(_terpilih, Input.Desimal(TxtNilai.Text, "Nilai tarif"));
            Pesan.Info("Tarif berhasil diperbarui.");
            MuatData();
        }
        catch (Exception ex)
        {
            Pesan.Error(ex);
        }
    }
}