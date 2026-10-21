using System.Text.RegularExpressions;
using HunianKu.Data;
using HunianKu.Helpers;
using HunianKu.Models;

namespace HunianKu.Services;

public static class PenghuniService
{
    private const string PolaUsername = "^[a-z0-9._]{3,50}$";
    private const string PolaNoHp = @"^\+?[0-9]{10,15}$";
    private const string PolaEmail = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";

    public static void Simpan(Penghuni p, string? passwordAwal)
    {
        p.NamaLengkap = Input.Wajib(p.NamaLengkap, "Nama lengkap", 100);
        p.Username = Input.Wajib(p.Username, "Username", 50).ToLower();
        p.NIK = Input.Wajib(p.NIK, "NIK", 16);
        p.NoHp = Input.Wajib(p.NoHp, "No. HP", 20).Replace(" ", "").Replace("-", "");
        p.Email = Input.Opsional(p.Email, "Email", 100);
        p.Pekerjaan = Input.Opsional(p.Pekerjaan, "Pekerjaan", 100);
        p.KontakDarurat = Input.Opsional(p.KontakDarurat, "Kontak darurat", 150);

        if (!Regex.IsMatch(p.Username, PolaUsername))
            throw new ValidasiException("Username 3-50 karakter, hanya huruf kecil, angka, titik, atau garis bawah (tanpa spasi).");
        if (p.NIK.Length != 16 || !p.NIK.All(char.IsDigit))
            throw new ValidasiException("NIK harus 16 digit angka.");
        if (!Regex.IsMatch(p.NoHp, PolaNoHp))
            throw new ValidasiException("No. HP harus 10-15 digit angka (boleh diawali +).");
        if (p.Email != null && !Regex.IsMatch(p.Email, PolaEmail))
            throw new ValidasiException("Format email tidak valid.");
        if (p.JenisKelamin is not ("L" or "P"))
            throw new ValidasiException("Pilih jenis kelamin.");

        if (UserRepository.UsernameSudahDipakai(p.Username, p.UserId == 0 ? null : p.UserId))
            throw new ValidasiException($"Username \"{p.Username}\" sudah dipakai.");
        if (PenghuniRepository.NikSudahDipakai(p.NIK, p.PenghuniId))
            throw new ValidasiException("NIK tersebut sudah terdaftar.");

        if (p.PenghuniId == 0)
        {
            AuthService.ValidasiPassword(passwordAwal ?? "");
            PenghuniRepository.Insert(p, PasswordHasher.Hash(passwordAwal!));
        }
        else
        {
            if (!p.IsAktif && p.UnitAktif != null)
                throw new ValidasiException("Penghuni yang masih punya kontrak aktif tidak dapat dinonaktifkan.");
            PenghuniRepository.Update(p);
        }
    }

    public static void ResetPassword(Penghuni p, string passwordBaru)
    {
        AuthService.ValidasiPassword(passwordBaru);
        UserRepository.UpdatePassword(p.UserId, PasswordHasher.Hash(passwordBaru));
    }

    public static void Hapus(Penghuni p)
    {
        if (p.UnitAktif != null)
            throw new ValidasiException("Penghuni masih memiliki kontrak aktif dan tidak dapat dihapus.");
        PenghuniRepository.Delete(p);
    }
}