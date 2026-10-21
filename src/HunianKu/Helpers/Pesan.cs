using System.Windows;
using Microsoft.Data.SqlClient;

namespace HunianKu.Helpers;

/// <summary>
/// Menampilkan pesan ke user dengan bahasa yang ramah,
/// termasuk menerjemahkan error teknis SQL Server.
/// </summary>
public static class Pesan
{
    public static void Info(string pesan)
        => MessageBox.Show(pesan, "Informasi", MessageBoxButton.OK, MessageBoxImage.Information);

    public static void Peringatan(string pesan)
        => MessageBox.Show(pesan, "Perhatian", MessageBoxButton.OK, MessageBoxImage.Warning);

    public static bool Konfirmasi(string pesan)
        => MessageBox.Show(pesan, "Konfirmasi", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes;

    /// <summary>Tampilkan error. ValidasiException = peringatan biasa, lainnya = error.</summary>
    public static void Error(Exception ex)
    {
        if (ex is ValidasiException)
        {
            Peringatan(ex.Message);
            return;
        }
        MessageBox.Show(Terjemahkan(ex), "Terjadi Kesalahan", MessageBoxButton.OK, MessageBoxImage.Error);
    }

    /// <summary>Mengubah exception teknis menjadi kalimat yang bisa dipahami user.</summary>
    public static string Terjemahkan(Exception ex)
    {
        if (ex is not SqlException sql)
            return "Terjadi kesalahan: " + ex.Message;

        // Error buatan sendiri dari stored procedure (THROW 50001 dst.)
        if (sql.Number >= 50000)
            return sql.Message;

        return sql.Number switch
        {
            2627 or 2601 => "Data yang sama sudah ada. Periksa kembali isian yang harus unik (kode, nomor, username, NIK, dll).",
            547 when sql.Message.Contains("REFERENCE") =>
                "Data tidak dapat dihapus/diubah karena masih dipakai oleh data lain.",
            547 => "Isian tidak memenuhi aturan data (constraint). Periksa kembali nilai yang dimasukkan.",
            2 or 53 or -1 or 258 =>
                "Tidak dapat terhubung ke SQL Server.\nPastikan SQL Server berjalan dan \"Server=\" di appsettings.json sudah benar.",
            4060 => "Database HunianKu tidak ditemukan.\nJalankan dulu script database/HunianKu.sql.",
            18456 => "Login ke SQL Server ditolak. Periksa pengaturan autentikasi di appsettings.json.",
            _ => $"Kesalahan database ({sql.Number}): {sql.Message}"
        };
    }
}
