using HunianKu.Data;
using HunianKu.Helpers;
using HunianKu.Models;

namespace HunianKu.Services;

public static class AuthService
{
    public const int PanjangPasswordMinimal = 6;

    public static User Login(string username, string password)
    {
        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrEmpty(password))
            throw new ValidasiException("Username dan password wajib diisi.");

        var user = UserRepository.GetByUsername(username.Trim());

        if (user == null || !PasswordHasher.Verify(password, user.PasswordHash))
            throw new ValidasiException("Username atau password salah.");

        if (!user.IsAktif)
            throw new ValidasiException("Akun Anda sedang dinonaktifkan. Silakan hubungi pengelola apartemen.");

        int? penghuniId = null;
        if (user.Role == Roles.Penghuni)
        {
            var penghuni = PenghuniRepository.GetByUserId(user.UserId)
                ?? throw new ValidasiException("Data penghuni untuk akun ini tidak ditemukan.");
            penghuniId = penghuni.PenghuniId;
        }

        Session.Login(user, penghuniId);
        return user;
    }

    public static void Logout() => Session.Logout();

    public static void GantiPassword(string passwordLama, string passwordBaru, string konfirmasi)
    {
        var user = UserRepository.GetById(Session.User.UserId)
            ?? throw new ValidasiException("Akun tidak ditemukan.");

        if (!PasswordHasher.Verify(passwordLama, user.PasswordHash))
            throw new ValidasiException("Password lama salah.");

        ValidasiPassword(passwordBaru);

        if (passwordBaru != konfirmasi)
            throw new ValidasiException("Konfirmasi password tidak sama dengan password baru.");

        if (passwordBaru == passwordLama)
            throw new ValidasiException("Password baru harus berbeda dari password lama.");

        UserRepository.UpdatePassword(user.UserId, PasswordHasher.Hash(passwordBaru));
    }

    public static void ValidasiPassword(string password)
    {
        if (string.IsNullOrEmpty(password) || password.Length < PanjangPasswordMinimal)
            throw new ValidasiException($"Password minimal {PanjangPasswordMinimal} karakter.");
    }
}
