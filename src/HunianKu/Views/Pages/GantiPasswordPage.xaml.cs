using System.Windows;
using System.Windows.Controls;
using HunianKu.Helpers;
using HunianKu.Services;

namespace HunianKu.Views.Pages;

public partial class GantiPasswordPage : UserControl
{
    public GantiPasswordPage()
    {
        InitializeComponent();
    }

    private void Simpan_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            AuthService.GantiPassword(TxtLama.Password, TxtBaru.Password, TxtKonfirmasi.Password);
            TxtLama.Clear();
            TxtBaru.Clear();
            TxtKonfirmasi.Clear();
            Pesan.Info("Password berhasil diganti. Gunakan password baru saat login berikutnya.");
        }
        catch (Exception ex)
        {
            Pesan.Error(ex);
        }
    }
}
