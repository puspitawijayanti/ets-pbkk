using System.Windows;
using System.Windows.Controls;
using HunianKu.Data;
using HunianKu.Helpers;
using HunianKu.Models;
using HunianKu.Services;

namespace HunianKu.Views.Pages;

public partial class TagihanPage : UserControl
{
    private const string SemuaStatus = "Semua Status";

    public TagihanPage()
    {
        InitializeComponent();
        Format.IsiComboPeriode(CmbBulan, CmbTahun, TagihanService.AwalBulanIni.AddMonths(-1));
        CmbStatus.ItemsSource = new[] { SemuaStatus, StatusTagihan.BelumBayar, StatusTagihan.MenungguVerifikasi, StatusTagihan.Lunas };
        CmbStatus.SelectedIndex = 0;
        MuatData();
    }

    private void Tampilkan_Click(object sender, RoutedEventArgs e) => MuatData();

    private void MuatData()
    {
        try
        {
            TagihanRepository.HitungDenda();

            DateTime? periode = ChkSemuaPeriode.IsChecked == true ? null : Format.AmbilPeriode(CmbBulan, CmbTahun);
            string? status = CmbStatus.SelectedItem as string is { } s && s != SemuaStatus ? s : null;
            var data = TagihanRepository.GetAll(periode, status, TxtCari.Text);
            DgTagihan.ItemsSource = data;

            var belumLunas = data.Where(t => t.Status != StatusTagihan.Lunas).ToList();
            TxtRingkasan.Text =
                $"{data.Count} tagihan  ·  Total {Format.Rupiah(data.Sum(t => t.GrandTotal))}  ·  " +
                $"Lunas {data.Count(t => t.Status == StatusTagihan.Lunas)}  ·  " +
                $"Belum lunas {belumLunas.Count} ({Format.Rupiah(belumLunas.Sum(t => t.GrandTotal))})";
        }
        catch (Exception ex)
        {
            Pesan.Error(ex);
        }
    }

    private void Generate_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            var periode = Format.AmbilPeriode(CmbBulan, CmbTahun);
            if (!Pesan.Konfirmasi($"Buat tagihan untuk periode {Format.Periode(periode)}?\n\n" +
                                  "Tagihan dibuat untuk setiap kontrak yang meterannya sudah diinput.")) return;

            var hasil = TagihanService.Generate(periode);
            int dibuat = hasil.Count(h => h.Keterangan == "Dibuat");
            int sudahAda = hasil.Count(h => h.Keterangan == "Sudah ada");
            var tanpaMeteran = hasil.Where(h => h.Keterangan == "Meteran belum diinput").ToList();

            var pesan = $"Periode {Format.Periode(periode)}:\n" +
                        $"• Tagihan baru dibuat: {dibuat}\n" +
                        $"• Sudah ada sebelumnya: {sudahAda}\n" +
                        $"• Dilewati (meteran belum diinput): {tanpaMeteran.Count}";
            if (tanpaMeteran.Count > 0)
                pesan += "\n\nUnit yang meterannya belum diinput:\n" +
                         string.Join("\n", tanpaMeteran.Select(h => $"   - {h.KodeUnit} ({h.NamaPenghuni})"));
            if (hasil.Count == 0)
                pesan = $"Tidak ada kontrak yang berlaku pada periode {Format.Periode(periode)}.";

            Pesan.Info(pesan);
            ChkSemuaPeriode.IsChecked = false;
            MuatData();
        }
        catch (Exception ex)
        {
            Pesan.Error(ex);
        }
    }
}