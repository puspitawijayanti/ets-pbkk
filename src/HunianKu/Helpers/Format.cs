using System.Globalization;
using System.Windows.Controls;

namespace HunianKu.Helpers;

public static class Format
{
    public static readonly CultureInfo Indonesia = new("id-ID");

    public static string Rupiah(decimal nilai) => "Rp " + nilai.ToString("N0", Indonesia);

    public static string Tanggal(DateTime t) => t.ToString("dd MMM yyyy", Indonesia);

    public static string Periode(DateTime t) => t.ToString("MMMM yyyy", Indonesia);

    public static void IsiComboPeriode(ComboBox cmbBulan, ComboBox cmbTahun, DateTime pilihan)
    {
        cmbBulan.ItemsSource = Enumerable.Range(1, 12)
            .Select(b => Indonesia.DateTimeFormat.GetMonthName(b)).ToList();
        cmbTahun.ItemsSource = Enumerable.Range(DateTime.Today.Year - 3, 4).ToList();

        cmbBulan.SelectedIndex = pilihan.Month - 1;
        cmbTahun.SelectedItem = pilihan.Year;
    }

    public static DateTime AmbilPeriode(ComboBox cmbBulan, ComboBox cmbTahun)
    {
        if (cmbBulan.SelectedIndex < 0 || cmbTahun.SelectedItem is not int tahun)
            throw new ValidasiException("Pilih bulan dan tahun periode.");
        return new DateTime(tahun, cmbBulan.SelectedIndex + 1, 1);
    }
}
