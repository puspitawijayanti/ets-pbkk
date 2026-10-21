using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace HunianKu.Helpers;

public class StatusWarnaConverter : IValueConverter
{
    private static readonly Brush Hijau = new SolidColorBrush(Color.FromRgb(0x15, 0x80, 0x3D));
    private static readonly Brush Merah = new SolidColorBrush(Color.FromRgb(0xB9, 0x1C, 0x1C));
    private static readonly Brush Kuning = new SolidColorBrush(Color.FromRgb(0xB4, 0x53, 0x09));
    private static readonly Brush Biru = new SolidColorBrush(Color.FromRgb(0x1D, 0x4E, 0xD8));
    private static readonly Brush Abu = new SolidColorBrush(Color.FromRgb(0x6B, 0x72, 0x80));

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        => (value as string) switch
        {
            "Lunas" or "Selesai" or "Aktif" or "Diterima" or "Kosong" or "Sudah diinput" or "Dibuat" => Hijau,
            "Belum Bayar" or "Ditolak" or "Belum diinput" or "Tinggi" or "Meteran belum diinput" => Merah,
            "Menunggu Verifikasi" or "Menunggu" or "Diproses" or "Maintenance" or "Sedang" => Kuning,
            "Diajukan" or "Terisi" or "Sudah ditagih" => Biru,
            _ => Abu
        };

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => throw new NotSupportedException();
}
