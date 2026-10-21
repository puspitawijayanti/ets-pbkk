using System.Globalization;
using System.Windows;
using System.Windows.Markup;
using HunianKu.Helpers;
using HunianKu.Views;

namespace HunianKu;

public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        CultureInfo.DefaultThreadCurrentCulture = Format.Indonesia;
        CultureInfo.DefaultThreadCurrentUICulture = Format.Indonesia;
        Thread.CurrentThread.CurrentCulture = Format.Indonesia;
        Thread.CurrentThread.CurrentUICulture = Format.Indonesia;
        FrameworkElement.LanguageProperty.OverrideMetadata(typeof(FrameworkElement),
            new FrameworkPropertyMetadata(XmlLanguage.GetLanguage(Format.Indonesia.IetfLanguageTag)));

        DispatcherUnhandledException += (_, args) =>
        {
            Pesan.Error(args.Exception);
            args.Handled = true;
        };

        try
        {
            AppConfig.Load();
            using var koneksi = Db.OpenConnection();
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                Pesan.Terjemahkan(ex) + $"\n\nLokasi file konfigurasi:\n{AppConfig.FilePath}",
                "HunianKu - Gagal Terhubung ke Database", MessageBoxButton.OK, MessageBoxImage.Error);
            Shutdown();
            return;
        }

        new LoginWindow().Show();
    }
}
