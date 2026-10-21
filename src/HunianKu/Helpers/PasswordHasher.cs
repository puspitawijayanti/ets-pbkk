using System.Security.Cryptography;

namespace HunianKu.Helpers;

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

        byte[] hashInput = Rfc2898DeriveBytes.Pbkdf2(password, salt, iterasi, HashAlgorithmName.SHA256, hashAsli.Length);
        
        return CryptographicOperations.FixedTimeEquals(hashInput, hashAsli);
    }
}
