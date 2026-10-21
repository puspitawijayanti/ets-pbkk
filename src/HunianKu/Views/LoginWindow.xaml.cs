using System.Windows;
using System.Windows.Input;
using HunianKu.Helpers;
using HunianKu.Services;

namespace HunianKu.Views;

public partial class LoginWindow : Window
{
    public LoginWindow()
    {
        InitializeComponent();
        Loaded += (_, _) => TxtUsername.Focus();
    }

    private void Login_Click(object sender, RoutedEventArgs e)
    {
        TxtError.Visibility = Visibility.Collapsed;
        BtnLogin.IsEnabled = false;
        Mouse.OverrideCursor = Cursors.Wait;
        try
        {
            AuthService.Login(TxtUsername.Text, TxtPassword.Password);

            // Login berhasil: buka jendela utama lalu tutup jendela login.
            new MainWindow().Show();
            Close();
        }
        catch (ValidasiException ex)
        {
            // Salah username/password ditampilkan di bawah form, bukan popup.
            TxtError.Text = ex.Message;
            TxtError.Visibility = Visibility.Visible;
            TxtPassword.Clear();
            TxtPassword.Focus();
        }
        catch (Exception ex)
        {
            Pesan.Error(ex);
        }
        finally
        {
            Mouse.OverrideCursor = null;
            BtnLogin.IsEnabled = true;
        }
    }
}
