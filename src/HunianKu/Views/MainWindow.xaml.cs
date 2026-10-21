using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using HunianKu.Data;
using HunianKu.Helpers;
using HunianKu.Models;
using HunianKu.Services;
using HunianKu.Views.Pages;

namespace HunianKu.Views;

public partial class MainWindow : Window
{
    private record MenuItem(string Ikon, string Judul, Func<UserControl> BuatHalaman);

    public MainWindow()
    {
        InitializeComponent();

        var user = Session.User;
        TxtNamaUser.Text = user.NamaLengkap;
        TxtRoleUser.Text = user.Role == Roles.Admin ? "Admin / Pemilik" : user.Role == Roles.Staf ? "Staf Pengelola" : "Penghuni";
        TxtTanggal.Text = DateTime.Today.ToString("dddd, dd MMMM yyyy");

        var menu = DaftarMenu(user.Role);
        foreach (var item in menu)
            PanelMenu.Children.Add(BuatTombolMenu(item));

        BukaHalaman(menu[0], (Button)PanelMenu.Children[0]);
        Loaded += (_, _) => TampilkanPengingatKontrak();
    }

    private static List<MenuItem> DaftarMenu(string role)
    {
        var dashboard = new MenuItem("", "Dashboard", () => new DashboardPage());
        var gantiPassword = new MenuItem("", "Ganti Password", () => new GantiPasswordPage());

        return role switch
        {
            Roles.Admin =>
            [
                dashboard,
                new("", "Tower", () => new TowerPage()),
                new("", "Tipe Unit", () => new TipeUnitPage()),
                new("", "Unit", () => new UnitPage()),
                new("", "Penghuni", () => new PenghuniPage()),
                new("", "Kontrak Sewa", () => new KontrakPage()),
                new("", "Tarif", () => new TarifPage()),
                new("", "Tagihan", () => new TagihanPage()),
                new("", "Laporan", () => new LaporanPage()),
                gantiPassword
            ],
            Roles.Staf =>
            [
                dashboard,
                new("", "Input Meteran", () => new MeteranPage()),
                new("", "Tagihan", () => new TagihanPage()),
                new("", "Verifikasi Pembayaran", () => new VerifikasiPembayaranPage()),
                new("", "Komplain & Perbaikan", () => new KomplainKelolaPage()),
                gantiPassword
            ],
            _ =>
            [
                dashboard,
                new("", "Tagihan Saya", () => new TagihanSayaPage()),
                new("", "Riwayat Pembayaran", () => new RiwayatPembayaranPage()),
                new("", "Komplain Saya", () => new KomplainSayaPage()),
                new("", "Booking Fasilitas", () => new BookingFasilitasPage()),
                gantiPassword
            ]
        };
    }

    private Button BuatTombolMenu(MenuItem item)
    {
        var isi = new StackPanel { Orientation = Orientation.Horizontal };
        isi.Children.Add(new TextBlock
        {
            Text = item.Ikon,
            FontFamily = new FontFamily("Segoe MDL2 Assets"),
            Width = 26,
            VerticalAlignment = VerticalAlignment.Center
        });
        isi.Children.Add(new TextBlock { Text = item.Judul });

        var tombol = new Button { Content = isi, Style = (Style)FindResource("MenuButton") };
        tombol.Click += (_, _) => BukaHalaman(item, tombol);
        return tombol;
    }

    private void BukaHalaman(MenuItem item, Button tombol)
    {
        try
        {
            Konten.Content = item.BuatHalaman();
            TxtJudulHalaman.Text = item.Judul;

            // Tandai tombol yang aktif (lihat trigger Tag="aktif" di Theme.xaml)
            foreach (Button b in PanelMenu.Children) b.Tag = null;
            tombol.Tag = "aktif";
        }
        catch (Exception ex)
        {
            Pesan.Error(ex);
        }
    }

    private void TampilkanPengingatKontrak()
    {
        if (Session.User.Role != Roles.Admin) return;
        try
        {
            var hampirHabis = KontrakRepository.GetHampirHabis();
            if (hampirHabis.Count == 0) return;

            var daftar = string.Join("\n", hampirHabis.Select(k =>
                $"• {k.NomorKontrak} - {k.NamaPenghuni} ({k.KodeUnit}), berakhir {Format.Tanggal(k.TanggalSelesai)} ({k.SisaHari} hari lagi)"));
            Pesan.Peringatan($"Ada {hampirHabis.Count} kontrak yang akan segera berakhir:\n\n{daftar}\n\n" +
                             "Hubungi penghuni untuk perpanjangan di menu Kontrak Sewa.");
        }
        catch (Exception ex)
        {
            Pesan.Error(ex);
        }
    }

    private void Logout_Click(object sender, RoutedEventArgs e)
    {
        if (!Pesan.Konfirmasi("Keluar dari aplikasi dan kembali ke halaman login?")) return;
        AuthService.Logout();
        new LoginWindow().Show();
        Close();
    }
}
