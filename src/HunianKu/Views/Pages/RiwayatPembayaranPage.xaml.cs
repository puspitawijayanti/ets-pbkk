using System.Windows.Controls;
using HunianKu.Data;
using HunianKu.Helpers;
using HunianKu.Models;

namespace HunianKu.Views.Pages;

public partial class RiwayatPembayaranPage : UserControl
{
    public RiwayatPembayaranPage()
    {
        InitializeComponent();
        try
        {
            var data = PembayaranRepository.GetByPenghuni(Session.PenghuniId!.Value);
            DgRiwayat.ItemsSource = data;
            var diterima = data.Where(p => p.Status == StatusPembayaran.Diterima).ToList();
            TxtRingkasan.Text = $"{data.Count} pembayaran · {diterima.Count} diterima dengan total {Format.Rupiah(diterima.Sum(p => p.JumlahBayar))}";
        }
        catch (Exception ex)
        {
            Pesan.Error(ex);
        }
    }
}