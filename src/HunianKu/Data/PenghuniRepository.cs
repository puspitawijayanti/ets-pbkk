using HunianKu.Helpers;
using HunianKu.Models;
using Microsoft.Data.SqlClient;

namespace HunianKu.Data;

public static class PenghuniRepository
{
    private const string SelectSql = @"
        SELECT p.PenghuniId, p.UserId, u.NamaLengkap, u.Username, u.IsAktif,
               p.NIK, p.JenisKelamin, p.NoHp, p.Email, p.Pekerjaan, p.KontakDarurat,
               (SELECT tw.KodeTower + '-' + un.NomorUnit
                FROM Kontrak k
                JOIN Unit un  ON un.UnitId = k.UnitId
                JOIN Tower tw ON tw.TowerId = un.TowerId
                WHERE k.PenghuniId = p.PenghuniId AND k.Status = 'Aktif') AS UnitAktif
        FROM Penghuni p
        JOIN Users u ON u.UserId = p.UserId";

    private static Penghuni Map(SqlDataReader r) => new()
    {
        PenghuniId = r.Int("PenghuniId"),
        UserId = r.Int("UserId"),
        NamaLengkap = r.Str("NamaLengkap"),
        Username = r.Str("Username"),
        IsAktif = r.Bool("IsAktif"),
        NIK = r.Str("NIK"),
        JenisKelamin = r.Str("JenisKelamin"),
        NoHp = r.Str("NoHp"),
        Email = r.StrNull("Email"),
        Pekerjaan = r.StrNull("Pekerjaan"),
        KontakDarurat = r.StrNull("KontakDarurat"),
        UnitAktif = r.StrNull("UnitAktif")
    };

    public static List<Penghuni> GetAll(string cari = "")
        => Db.Query($@"{SelectSql}
                       WHERE @Cari = '' OR u.NamaLengkap LIKE '%' + @Cari + '%'
                          OR p.NIK LIKE '%' + @Cari + '%' OR u.Username LIKE '%' + @Cari + '%'
                       ORDER BY u.NamaLengkap", Map, Db.P("@Cari", cari.Trim()));

    public static Penghuni? GetByUserId(int userId)
        => Db.Query($"{SelectSql} WHERE p.UserId = @Id", Map, Db.P("@Id", userId)).FirstOrDefault();

    public static List<Penghuni> GetTanpaKontrakAktif()
        => Db.Query($@"{SelectSql}
                       WHERE u.IsAktif = 1
                         AND NOT EXISTS (SELECT 1 FROM Kontrak k WHERE k.PenghuniId = p.PenghuniId AND k.Status = 'Aktif')
                       ORDER BY u.NamaLengkap", Map);

    public static bool NikSudahDipakai(string nik, int kecualiPenghuniId)
        => Convert.ToInt32(Db.Scalar("SELECT COUNT(*) FROM Penghuni WHERE NIK = @Nik AND PenghuniId <> @Id",
            Db.P("@Nik", nik), Db.P("@Id", kecualiPenghuniId))) > 0;

    public static void Insert(Penghuni p, string passwordHash)
    {
        using var conn = Db.OpenConnection();
        using var tx = conn.BeginTransaction();
        try
        {
            var cmdUser = new SqlCommand(@"
                INSERT INTO Users (Username, PasswordHash, NamaLengkap, Role, IsAktif)
                OUTPUT INSERTED.UserId
                VALUES (@Username, @Hash, @Nama, 'Penghuni', @Aktif)", conn, tx);
            cmdUser.Parameters.AddRange([
                Db.P("@Username", p.Username), Db.P("@Hash", passwordHash),
                Db.P("@Nama", p.NamaLengkap), Db.P("@Aktif", p.IsAktif)]);
            int userIdBaru = (int)cmdUser.ExecuteScalar()!;

            var cmdPenghuni = new SqlCommand(@"
                INSERT INTO Penghuni (UserId, NIK, JenisKelamin, NoHp, Email, Pekerjaan, KontakDarurat)
                VALUES (@UserId, @Nik, @JK, @NoHp, @Email, @Pekerjaan, @Darurat)", conn, tx);
            cmdPenghuni.Parameters.Add(Db.P("@UserId", userIdBaru));
            cmdPenghuni.Parameters.AddRange(ParameterPenghuni(p));
            cmdPenghuni.ExecuteNonQuery();

            tx.Commit();
        }
        catch
        {
            tx.Rollback();
            throw;
        }
    }

    public static void Update(Penghuni p)
    {
        using var conn = Db.OpenConnection();
        using var tx = conn.BeginTransaction();
        try
        {
            var cmdUser = new SqlCommand(@"
                UPDATE Users SET Username = @Username, NamaLengkap = @Nama, IsAktif = @Aktif
                WHERE UserId = @UserId", conn, tx);
            cmdUser.Parameters.AddRange([
                Db.P("@Username", p.Username), Db.P("@Nama", p.NamaLengkap),
                Db.P("@Aktif", p.IsAktif), Db.P("@UserId", p.UserId)]);
            cmdUser.ExecuteNonQuery();

            var cmdPenghuni = new SqlCommand(@"
                UPDATE Penghuni SET NIK = @Nik, JenisKelamin = @JK, NoHp = @NoHp, Email = @Email,
                       Pekerjaan = @Pekerjaan, KontakDarurat = @Darurat
                WHERE PenghuniId = @Id", conn, tx);
            cmdPenghuni.Parameters.Add(Db.P("@Id", p.PenghuniId));
            cmdPenghuni.Parameters.AddRange(ParameterPenghuni(p));
            cmdPenghuni.ExecuteNonQuery();

            tx.Commit();
        }
        catch
        {
            tx.Rollback();
            throw;
        }
    }

    public static void Delete(Penghuni p)
    {
        using var conn = Db.OpenConnection();
        using var tx = conn.BeginTransaction();
        try
        {
            var cmd1 = new SqlCommand("DELETE FROM Penghuni WHERE PenghuniId = @Id", conn, tx);
            cmd1.Parameters.Add(Db.P("@Id", p.PenghuniId));
            cmd1.ExecuteNonQuery();

            var cmd2 = new SqlCommand("DELETE FROM Users WHERE UserId = @Id", conn, tx);
            cmd2.Parameters.Add(Db.P("@Id", p.UserId));
            cmd2.ExecuteNonQuery();

            tx.Commit();
        }
        catch
        {
            tx.Rollback();
            throw;
        }
    }

    private static SqlParameter[] ParameterPenghuni(Penghuni p) =>
    [
        Db.P("@Nik", p.NIK), Db.P("@JK", p.JenisKelamin), Db.P("@NoHp", p.NoHp),
        Db.P("@Email", p.Email), Db.P("@Pekerjaan", p.Pekerjaan), Db.P("@Darurat", p.KontakDarurat)
    ];
}