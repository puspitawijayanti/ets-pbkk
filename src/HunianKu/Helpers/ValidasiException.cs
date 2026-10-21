namespace HunianKu.Helpers;

/// <summary>
/// Exception khusus untuk kesalahan input/aturan bisnis yang pesannya
/// aman dan ramah untuk langsung ditampilkan ke user.
/// Contoh: "Nomor unit wajib diisi."
/// </summary>
public class ValidasiException(string message) : Exception(message);
