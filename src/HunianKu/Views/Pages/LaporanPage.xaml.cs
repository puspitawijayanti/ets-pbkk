using System.Windows.Controls;
using HunianKu.Data;
using HunianKu.Helpers;

namespace HunianKu.Views.Pages;


public partial class LaporanPage : UserControl
{
    public LaporanPage()
    {
        InitializeComponent();
        MuatOkupansi();
        MuatTunggakan();

        CmbTahun.ItemsSource = Enumerable.Range(DateTime.Today.Year - 3, 4).Reverse().ToList();
        CmbTahun.SelectedItem = DateTime.Today.Year;  
    }

    private void MuatOkupansi()
    {
        try
        {
            var data = LaporanRepository.GetOkupansi();
            DgOkupansi.ItemsSource = data;

            int total = data.Sum(d => d.TotalUnit);
            int terisi = data.Sum(d => d.UnitTerisi);
            decimal persen = total == 0 ? 0 : terisi * 100m / total;
            TxtTotalOkupansi.Text = $"Okupansi keseluruhan: {persen:0.#}%  ({terisi} dari {total} unit terisi)";
        }
        catch (Exception ex)
        {
            Pesan.Error(ex);
        }
    }

    private void CmbTahun_SelectionChanged(object sender, SelectionChangedEventArgs e) => MuatPendapatan();

    private void MuatPendapatan()
    {
        if (CmbTahun.SelectedItem is not int tahun) return;
        try
        {
            var data = LaporanRepository.GetPendapatan(tahun);


            decimal maks = data.Max(d => d.TotalPendapatan);
            foreach (var d in data)
                d.PersenBatang = maks == 0 ? 0 : (double)(d.TotalPendapatan / maks * 100);

            DgPendapatan.ItemsSource = data;
            TxtTotalPendapatan.Text = $"Total {tahun}: {Format.Rupiah(data.Sum(d => d.TotalPendapatan))}";
        }
        catch (Exception ex)
        {
            Pesan.Error(ex);
        }
    }

    private void MuatTunggakan()
    {
        try
        {
            var data = LaporanRepository.GetTunggakan();
            DgTunggakan.ItemsSource = data;
            int jumlahPenghuni = data.Select(d => d.NamaPenghuni).Distinct().Count();
            TxtTotalTunggakan.Text = data.Count == 0
                ? "Tidak ada tunggakan."
                : $"Total tunggakan: {Format.Rupiah(data.Sum(d => d.GrandTotal))}  ·  {data.Count} tagihan dari {jumlahPenghuni} penghuni";
        }
        catch (Exception ex)
        {
            Pesan.Error(ex);
        }
    }
}
