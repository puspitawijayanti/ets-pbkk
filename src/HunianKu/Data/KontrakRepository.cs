using HunianKu.Helpers;
using HunianKu.Models;
using Microsoft.Data.SqlClient;

namespace HunianKu.Data;

public static class KontrakRepository
{
    private const string SelectSql = @"
        SELECT k.KontrakId, k.NomorKontrak, k.UnitId, k.PenghuniId, k.TanggalMulai, k.TanggalSelesai,
               k.HargaSewaBulanan, k.Deposit, k.Status,
               tw.KodeTower + '-' + u.NomorUnit AS KodeUnit, tu.NamaTipe,
               us.NamaLengkap AS NamaPenghuni, p.NoHp
        FROM Kontrak k
        JOIN Unit u      ON u.UnitId = k.UnitId
        JOIN Tower tw    ON tw.TowerId = u.TowerId
        JOIN TipeUnit tu ON tu.TipeUnitId = u.TipeUnitId
        JOIN Penghuni p  ON p.PenghuniId = k.PenghuniId
        JOIN Users us    ON us.UserId = p.UserId";

    private static Kontrak Map(SqlDataReader r) => new()
    {
        KontrakId = r.Int("KontrakId"),
        NomorKontrak = r.Str("NomorKontrak"),
        UnitId = r.Int("UnitId"),
        PenghuniId = r.Int("PenghuniId"),
        TanggalMulai = r.Date("TanggalMulai"),
        TanggalSelesai = r.Date("TanggalSelesai"),
        HargaSewaBulanan = r.Dec("HargaSewaBulanan"),
        Deposit = r.Dec("Deposit"),
        Status = r.Str("Status"),
        KodeUnit = r.Str("KodeUnit"),
        NamaTipe = r.Str("NamaTipe"),
        NamaPenghuni = r.Str("NamaPenghuni"),
        NoHp = r.Str("NoHp")
    };

    public static List<Kontrak> GetAll(string? status = null, string cari = "")
        => Db.Query($@"{SelectSql}
                       WHERE (@Status IS NULL OR k.Status = @Status)
                         AND (@Cari = '' OR us.NamaLengkap LIKE '%' + @Cari + '%'
                              OR k.NomorKontrak LIKE '%' + @Cari + '%'
                              OR tw.KodeTower + '-' + u.NomorUnit LIKE '%' + @Cari + '%')
                       ORDER BY CASE k.Status WHEN 'Aktif' THEN 0 ELSE 1 END, k.TanggalSelesai",
            Map, Db.P("@Status", status), Db.P("@Cari", cari.Trim()));

    public static Kontrak? GetAktifByPenghuni(int penghuniId)
        => Db.Query($"{SelectSql} WHERE k.PenghuniId = @Id AND k.Status = 'Aktif'", Map,
            Db.P("@Id", penghuniId)).FirstOrDefault();

    public static List<Kontrak> GetHampirHabis()
        => Db.Query(@"SELECT KontrakId, NomorKontrak, PenghuniId, NamaPenghuni, NoHp, KodeUnit,
                             TanggalMulai, TanggalSelesai
                      FROM vw_KontrakHampirHabis ORDER BY TanggalSelesai",
            r => new Kontrak
            {
                KontrakId = r.Int("KontrakId"),
                NomorKontrak = r.Str("NomorKontrak"),
                PenghuniId = r.Int("PenghuniId"),
                NamaPenghuni = r.Str("NamaPenghuni"),
                NoHp = r.Str("NoHp"),
                KodeUnit = r.Str("KodeUnit"),
                TanggalMulai = r.Date("TanggalMulai"),
                TanggalSelesai = r.Date("TanggalSelesai")
            });
    public static void Insert(Kontrak k)
    {
        using var conn = Db.OpenConnection();
        using var tx = conn.BeginTransaction();
        try
        {
            var cek = new SqlCommand("SELECT Status FROM Unit WITH (UPDLOCK) WHERE UnitId = @UnitId", conn, tx);
            cek.Parameters.Add(Db.P("@UnitId", k.UnitId));
            if ((string?)cek.ExecuteScalar() != StatusUnit.Kosong)
                throw new ValidasiException("Unit sudah tidak kosong. Silakan pilih unit lain.");

            var insert = new SqlCommand(@"
                INSERT INTO Kontrak (UnitId, PenghuniId, TanggalMulai, TanggalSelesai, HargaSewaBulanan, Deposit, Status)
                VALUES (@UnitId, @PenghuniId, @Mulai, @Selesai, @Harga, @Deposit, 'Aktif')", conn, tx);
            insert.Parameters.AddRange([
                Db.P("@UnitId", k.UnitId), Db.P("@PenghuniId", k.PenghuniId),
                Db.P("@Mulai", k.TanggalMulai), Db.P("@Selesai", k.TanggalSelesai),
                Db.P("@Harga", k.HargaSewaBulanan), Db.P("@Deposit", k.Deposit)]);
            insert.ExecuteNonQuery();

            var unit = new SqlCommand("UPDATE Unit SET Status = 'Terisi' WHERE UnitId = @UnitId", conn, tx);
            unit.Parameters.Add(Db.P("@UnitId", k.UnitId));
            unit.ExecuteNonQuery();

            tx.Commit();
        }
        catch
        {
            tx.Rollback();
            throw;
        }
    }

    public static void Update(int kontrakId, DateTime tanggalSelesai, decimal deposit)
        => Db.Execute(@"UPDATE Kontrak SET TanggalSelesai = @Selesai, Deposit = @Deposit
                        WHERE KontrakId = @Id AND Status = 'Aktif'",
            Db.P("@Selesai", tanggalSelesai), Db.P("@Deposit", deposit), Db.P("@Id", kontrakId));

    public static void Akhiri(int kontrakId, int unitId, string statusBaru)
    {
        using var conn = Db.OpenConnection();
        using var tx = conn.BeginTransaction();
        try
        {
            var cmd = new SqlCommand(
                "UPDATE Kontrak SET Status = @Status WHERE KontrakId = @Id AND Status = 'Aktif'", conn, tx);
            cmd.Parameters.AddRange([Db.P("@Status", statusBaru), Db.P("@Id", kontrakId)]);
            if (cmd.ExecuteNonQuery() == 0)
                throw new ValidasiException("Kontrak ini sudah tidak aktif.");

            var unit = new SqlCommand("UPDATE Unit SET Status = 'Kosong' WHERE UnitId = @UnitId", conn, tx);
            unit.Parameters.Add(Db.P("@UnitId", unitId));
            unit.ExecuteNonQuery();

            tx.Commit();
        }
        catch
        {
            tx.Rollback();
            throw;
        }
    }
}
