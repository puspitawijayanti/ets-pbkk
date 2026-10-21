using System.Windows;
using System.Windows.Controls;
using HunianKu.Data;
using HunianKu.Helpers;
using HunianKu.Models;
using HunianKu.Services;

namespace HunianKu.Views.Pages;

public partial class KomplainSayaPage : UserControl
{
    private readonly int _penghuniId = Session.PenghuniId!.Value;

    public KomplainSayaPage()
    {
        InitializeComponent();
        CmbKategori.ItemsSource = KategoriKomplain.Semua;
        CmbPrioritas.ItemsSource = PrioritasKomplain.Semua;
        KosongkanForm();

        try
        {
            var kontrak = KontrakRepository.GetAktifByPenghuni(_penghuniId);
            if (kontrak == null)
            {
                TxtUnit.Text = "Anda belum memiliki kontrak aktif, sehingga belum dapat mengajukan komplain.";
                BtnKirim.IsEnabled = false;
            }
            else
            {
                TxtUnit.Text = $"Untuk unit {kontrak.KodeUnit} ({kontrak.NamaTipe})";
            }
        }
        catch (Exception ex)
        {
            Pesan.Error(ex);
        }
        MuatData();
    }

    private void MuatData()
    {
        try
        {
            DgKomplain.ItemsSource = KomplainRepository.GetByPenghuni(_penghuniId);
        }
        catch (Exception ex)
        {
            Pesan.Error(ex);
        }
    }

    private void KosongkanForm()
    {
        CmbKategori.SelectedIndex = -1;
        CmbPrioritas.SelectedItem = "Sedang";
        TxtJudul.Clear();
        TxtDeskripsi.Clear();
    }

    private void Kirim_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            var komplain = new Komplain
            {
                PenghuniId = _penghuniId,
                Kategori = CmbKategori.SelectedItem as string ?? "",
                Prioritas = CmbPrioritas.SelectedItem as string ?? "",
                Judul = TxtJudul.Text,
                Deskripsi = TxtDeskripsi.Text
            };
            LayananService.AjukanKomplain(komplain);
            Pesan.Info("Komplain terkirim. Staf pengelola akan segera menindaklanjuti.");
            KosongkanForm();
            MuatData();
        }
        catch (Exception ex)
        {
            Pesan.Error(ex);
        }
    }
}
