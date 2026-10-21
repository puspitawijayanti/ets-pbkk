using System.Security.Cryptography;

namespace HunianKu.Helpers;

/// <summary>
/// Hash password dengan PBKDF2-SHA256 (bawaan .NET, tanpa library tambahan).
///
/// Kenapa tidak disimpan apa adanya / cukup SHA256 biasa?
///  - Salt acak 16 byte: dua user dengan password sama tetap punya hash berbeda.
///  - 100.000 iterasi: membuat tebak-tebakan password (brute force) sangat lambat.
///
/// Format yang disimpan di kolom Users.PasswordHash:
///   PBKDF2$100000$&lt;salt base64&gt;$&lt;hash base64&gt;
/// </summary>
public static class PasswordHasher
{
    private const int Iterasi = 100_000;
    private const int PanjangSalt = 16;   // byte
    private const int PanjangHash = 32;   // byte (256 bit)

    public static string Hash(string password)
    {
        byte[] salt = RandomNumberGenerator.GetBytes(PanjangSalt);
        byte[] hash = Rfc2898DeriveBytes.Pbkdf2(password, salt, Iterasi, HashAlgorithmName.SHA256, PanjangHash);
        return $"PBKDF2${Iterasi}${Convert.ToBase64String(salt)}${Convert.ToBase64String(hash)}";
    }

    public static bool Verify(string password, string hashTersimpan)
    {
        var bagian = hashTersimpan.Split('$');
        if (bagian.Length != 4 || bagian[0] != "PBKDF2")
            return false;

        int iterasi = int.Parse(bagian[1]);
        byte[] salt = Convert.FromBase64String(bagian[2]);
        byte[] hashAsli = Convert.FromBase64String(bagian[3]);

        // Hitung ulang hash dari password yang diketik dengan salt yang sama.
        byte[] hashInput = Rfc2898DeriveBytes.Pbkdf2(password, salt, iterasi, HashAlgorithmName.SHA256, hashAsli.Length);

        // Perbandingan waktu-konstan agar tidak bisa ditebak lewat lama proses.
        return CryptographicOperations.FixedTimeEquals(hashInput, hashAsli);
    }
}
