using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using HunianKu.Data;
using HunianKu.Helpers;
using HunianKu.Models;

namespace HunianKu.Views.Pages;


public partial class DashboardPage : UserControl
{
    public DashboardPage()
    {
        InitializeComponent();
        try
        {
            TagihanRepository.HitungDenda();  

            switch (Session.User.Role)
            {
                case Roles.Admin: TampilkanAdmin(); break;
                case Roles.Staf: TampilkanStaf(); break;
                default: TampilkanPenghuni(); break;
            }
        }
        catch (Exception ex)
        {
            Pesan.Error(ex);
        }
    }

    private void TampilkanAdmin()
    {
        var r = LaporanRepository.GetRingkasan();
        TambahKartu("Okupansi", $"{r.PersenOkupansi:0.#}%", $"{r.UnitTerisi} dari {r.TotalUnit} unit terisi");
        TambahKartu("Pendapatan Bulan Ini", Format.Rupiah(r.PendapatanBulanIni), "Pembayaran yang sudah diverifikasi");
        TambahKartu("Total Tunggakan", Format.Rupiah(r.TotalTunggakan), "Tagihan lewat jatuh tempo + denda",
            r.TotalTunggakan > 0 ? (Brush)FindResource("Danger") : null);
        TambahKartu("Komplain Aktif", $"{r.KomplainDiajukan + r.KomplainDiproses}",
            $"{r.KomplainDiajukan} diajukan, {r.KomplainDiproses} diproses");

        var kontrak = KontrakRepository.GetHampirHabis();
        TampilkanTabel(DgKontrak, kontrak, "Pengingat: Kontrak yang Akan Berakhir",
            "Tidak ada kontrak yang akan berakhir dalam waktu dekat.");
    }

    private void TampilkanStaf()
    {
        var r = LaporanRepository.GetRingkasan();
        var bulanLalu = DateTime.Today.AddMonths(-1);
        TambahKartu("Meteran Belum Diinput", $"{r.MeteranBelumDiinput}", $"Unit untuk periode {Format.Periode(bulanLalu)}",
            r.MeteranBelumDiinput > 0 ? (Brush)FindResource("Warning") : null);
        TambahKartu("Menunggu Verifikasi", $"{r.PembayaranMenunggu}", "Pembayaran dari penghuni",
            r.PembayaranMenunggu > 0 ? (Brush)FindResource("Warning") : null);
        TambahKartu("Komplain Baru", $"{r.KomplainDiajukan}", "Belum ditugaskan ke teknisi");
        TambahKartu("Komplain Diproses", $"{r.KomplainDiproses}", "Sedang dikerjakan teknisi");

        TampilkanTabel(DgKomplain, KomplainRepository.GetAll(StatusKomplain.Diajukan),
            "Komplain Baru yang Perlu Ditangani", "Tidak ada komplain baru.");
    }

    private void TampilkanPenghuni()
    {
        int penghuniId = Session.PenghuniId!.Value;
        var kontrak = KontrakRepository.GetAktifByPenghuni(penghuniId);
        var tagihan = TagihanRepository.GetByPenghuni(penghuniId);
        var belumBayar = tagihan.Where(t => t.Status == StatusTagihan.BelumBayar).ToList();
        int komplainAktif = KomplainRepository.GetByPenghuni(penghuniId).Count(k => k.Status != StatusKomplain.Selesai);

        TambahKartu("Unit Saya", kontrak?.KodeUnit ?? "-", kontrak?.NamaTipe ?? "Belum ada kontrak aktif");
        TambahKartu("Kontrak Berakhir", kontrak == null ? "-" : Format.Tanggal(kontrak.TanggalSelesai),
            kontrak == null ? "" : $"{kontrak.SisaHari} hari lagi");
        TambahKartu("Tagihan Belum Dibayar", Format.Rupiah(belumBayar.Sum(t => t.GrandTotal)), $"{belumBayar.Count} tagihan",
            belumBayar.Count > 0 ? (Brush)FindResource("Danger") : null);
        TambahKartu("Komplain Aktif", $"{komplainAktif}", "Diajukan atau sedang diproses");

   
        var pesan = new List<string>();
        int hariPengingat = (int)LaporanRepository.GetNilaiTarif(KodeTarif.PengingatKontrakHari);
        if (kontrak != null && kontrak.SisaHari <= hariPengingat)
            pesan.Add($"Kontrak sewa Anda akan berakhir pada {Format.Tanggal(kontrak.TanggalSelesai)} ({kontrak.SisaHari} hari lagi). " +
                      "Silakan hubungi pengelola untuk perpanjangan.");
        int terlambat = belumBayar.Count(t => t.HariTerlambat > 0);
        if (terlambat > 0)
            pesan.Add($"Ada {terlambat} tagihan yang sudah lewat jatuh tempo dan dikenakan denda harian. Segera lakukan pembayaran.");
        if (pesan.Count > 0)
        {
            TxtBanner.Text = string.Join("\n", pesan);
            BannerInfo.Visibility = Visibility.Visible;
        }

        TampilkanTabel(DgTagihan, tagihan.Where(t => t.Status != StatusTagihan.Lunas).ToList(),
            "Tagihan yang Belum Lunas", "Semua tagihan sudah lunas. Terima kasih!");
    }

  

    private void TambahKartu(string judul, string nilai, string keterangan, Brush? warnaNilai = null)
    {
        var isi = new StackPanel();
        isi.Children.Add(new TextBlock { Text = judul, Style = (Style)FindResource("MutedText") });
        isi.Children.Add(new TextBlock
        {
            Text = nilai,
            FontSize = 24,
            FontWeight = FontWeights.SemiBold,
            Margin = new Thickness(0, 6, 0, 4),
            Foreground = warnaNilai ?? (Brush)FindResource("Text")
        });
        isi.Children.Add(new TextBlock { Text = keterangan, Style = (Style)FindResource("MutedText") });

        PanelKartu.Children.Add(new Border { Style = (Style)FindResource("StatCard"), Width = 250, Child = isi });
    }

    private void TampilkanTabel<T>(DataGrid grid, List<T> data, string judul, string pesanKosong)
    {
        TxtJudulTabel.Text = judul;
        grid.ItemsSource = data;
        grid.Visibility = Visibility.Visible;
        if (data.Count == 0)
        {
            TxtTabelKosong.Text = pesanKosong;
            TxtTabelKosong.Visibility = Visibility.Visible;
        }
    }
}
