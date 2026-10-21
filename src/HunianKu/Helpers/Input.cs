using System.Globalization;

namespace HunianKu.Helpers;

public static class Input
{
    public static string Wajib(string? teks, string namaField, int maksPanjang = 200)
    {
        var t = teks?.Trim() ?? "";
        if (t.Length == 0)
            throw new ValidasiException($"{namaField} wajib diisi.");
        if (t.Length > maksPanjang)
            throw new ValidasiException($"{namaField} maksimal {maksPanjang} karakter.");
        return t;
    }

    public static string? Opsional(string? teks, string namaField = "Isian", int maksPanjang = 200)
    {
        if (string.IsNullOrWhiteSpace(teks)) return null;
        var t = teks.Trim();
        if (t.Length > maksPanjang)
            throw new ValidasiException($"{namaField} maksimal {maksPanjang} karakter.");
        return t;
    }

    public static decimal Desimal(string? teks, string namaField, decimal min = 0)
    {
        var t = (teks ?? "").Trim().Replace(',', '.');
        if (!decimal.TryParse(t, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out var nilai))
            throw new ValidasiException($"{namaField} harus berupa angka (tanpa titik ribuan), contoh: 3500000 atau 1250,5.");
        if (nilai < min)
            throw new ValidasiException($"{namaField} minimal {min:N0}.");
        return nilai;
    }

    public static int Bulat(string? teks, string namaField, int min = 0, int maks = int.MaxValue)
    {
        if (!int.TryParse((teks ?? "").Trim(), NumberStyles.None, CultureInfo.InvariantCulture, out var nilai))
            throw new ValidasiException($"{namaField} harus berupa bilangan bulat.");
        if (nilai < min || nilai > maks)
            throw new ValidasiException($"{namaField} harus di antara {min} dan {maks}.");
        return nilai;
    }

    public static T Pilih<T>(object? terpilih, string namaField) where T : class
        => terpilih as T ?? throw new ValidasiException($"Silakan pilih {namaField} terlebih dahulu.");

    public static DateTime Tanggal(DateTime? tanggal, string namaField)
        => tanggal?.Date ?? throw new ValidasiException($"{namaField} wajib diisi.");
}
