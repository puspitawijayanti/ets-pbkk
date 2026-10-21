using System.Windows;
using System.Windows.Controls;
using HunianKu.Data;
using HunianKu.Helpers;
using HunianKu.Models;
using HunianKu.Services;

namespace HunianKu.Views.Pages;

public partial class MeteranPage : UserControl
{
    private Meteran? _terpilih;
    private DateTime _periode;

    public MeteranPage()
    {
        InitializeComponent();
        Format.IsiComboPeriode(CmbBulan, CmbTahun, TagihanService.AwalBulanIni.AddMonths(-1));
        MuatData();
    }

    private void Tampilkan_Click(object sender, RoutedEventArgs e) => MuatData();

    private void MuatData()
    {
        try
        {
            _periode = Format.AmbilPeriode(CmbBulan, CmbTahun);
            var data = MeteranRepository.GetDaftarPeriode(_periode);
            DgMeteran.ItemsSource = data;
            TxtProgres.Text = $"{data.Count(m => m.SudahDiinput)} / {data.Count} unit sudah diinput";
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
        TxtJudulForm.Text = "Input Meteran";
        TxtInfoForm.Text = "Pilih unit di tabel.";
        foreach (var tb in new[] { TxtListrikAwal, TxtListrikAkhir, TxtAirAwal, TxtAirAkhir }) tb.Clear();
        BtnSimpan.IsEnabled = false;
    }

    private void DgMeteran_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (DgMeteran.SelectedItem is not Meteran m) return;
        _terpilih = m;
        TxtJudulForm.Text = $"Meteran {m.KodeUnit}";
        TxtInfoForm.Text = $"{m.NamaPenghuni} · {Format.Periode(_periode)}";

        try
        {
            if (m.SudahDiinput)
            {
                TxtListrikAwal.Text = m.ListrikAwal.ToString("0.##");
                TxtListrikAkhir.Text = m.ListrikAkhir.ToString("0.##");
                TxtAirAwal.Text = m.AirAwal.ToString("0.##");
                TxtAirAkhir.Text = m.AirAkhir.ToString("0.##");
            }
            else
            {
                var sebelumnya = MeteranRepository.GetAkhirSebelumnya(m.UnitId, _periode);
                TxtListrikAwal.Text = sebelumnya?.Listrik.ToString("0.##") ?? "0";
                TxtAirAwal.Text = sebelumnya?.Air.ToString("0.##") ?? "0";
                TxtListrikAkhir.Clear();
                TxtAirAkhir.Clear();
                TxtListrikAkhir.Focus();
            }
        }
        catch (Exception ex)
        {
            Pesan.Error(ex);
        }

        BtnSimpan.IsEnabled = !m.SudahDitagih;
        if (m.SudahDitagih)
            TxtInfoForm.Text += "\nSudah dipakai untuk tagihan, tidak dapat diubah.";
    }

    private void Simpan_Click(object sender, RoutedEventArgs e)
    {
        if (_terpilih == null) return;
        try
        {
            var meteran = new Meteran
            {
                MeteranId = _terpilih.MeteranId,
                UnitId = _terpilih.UnitId,
                Periode = _periode,
                ListrikAwal = Input.Desimal(TxtListrikAwal.Text, "Listrik awal"),
                ListrikAkhir = Input.Desimal(TxtListrikAkhir.Text, "Listrik akhir"),
                AirAwal = Input.Desimal(TxtAirAwal.Text, "Air awal"),
                AirAkhir = Input.Desimal(TxtAirAkhir.Text, "Air akhir")
            };
            TagihanService.SimpanMeteran(meteran);
            Pesan.Info($"Meteran {_terpilih.KodeUnit} tersimpan.\n" +
                       $"Pemakaian: {meteran.PemakaianListrik:N2} kWh listrik, {meteran.PemakaianAir:N2} m³ air.");
            MuatData();
        }
        catch (Exception ex)
        {
            Pesan.Error(ex);
        }
    }
}