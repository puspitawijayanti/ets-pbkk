using HunianKu.Helpers;
using HunianKu.Models;
using Microsoft.Data.SqlClient;

namespace HunianKu.Data;

public static class UnitRepository
{
    private const string SelectSql = @"
        SELECT u.UnitId, u.TowerId, u.TipeUnitId, u.NomorUnit, u.Lantai, u.Status,
               tw.KodeTower, tu.NamaTipe, tu.HargaSewaBulanan
        FROM Unit u
        JOIN Tower tw    ON tw.TowerId = u.TowerId
        JOIN TipeUnit tu ON tu.TipeUnitId = u.TipeUnitId";

    private static Unit Map(SqlDataReader r) => new()
    {
        UnitId = r.Int("UnitId"),
        TowerId = r.Int("TowerId"),
        TipeUnitId = r.Int("TipeUnitId"),
        NomorUnit = r.Str("NomorUnit"),
        Lantai = r.Int("Lantai"),
        Status = r.Str("Status"),
        KodeTower = r.Str("KodeTower"),
        NamaTipe = r.Str("NamaTipe"),
        HargaSewaBulanan = r.Dec("HargaSewaBulanan")
    };

    public static List<Unit> GetAll(int? towerId = null, string? status = null)
        => Db.Query($@"{SelectSql}
                       WHERE (@TowerId IS NULL OR u.TowerId = @TowerId)
                         AND (@Status IS NULL OR u.Status = @Status)
                       ORDER BY tw.KodeTower, u.NomorUnit", Map,
            Db.P("@TowerId", towerId), Db.P("@Status", status));

    public static Unit? GetById(int unitId)
        => Db.Query($"{SelectSql} WHERE u.UnitId = @Id", Map, Db.P("@Id", unitId)).FirstOrDefault();

    public static void Insert(Unit u)
        => Db.Execute(@"INSERT INTO Unit (TowerId, TipeUnitId, NomorUnit, Lantai, Status)
                        VALUES (@Tower, @Tipe, @Nomor, @Lantai, @Status)",
            Db.P("@Tower", u.TowerId), Db.P("@Tipe", u.TipeUnitId), Db.P("@Nomor", u.NomorUnit),
            Db.P("@Lantai", u.Lantai), Db.P("@Status", u.Status));

    public static void Update(Unit u)
        => Db.Execute(@"UPDATE Unit SET TowerId = @Tower, TipeUnitId = @Tipe, NomorUnit = @Nomor,
                               Lantai = @Lantai, Status = @Status
                        WHERE UnitId = @Id",
            Db.P("@Tower", u.TowerId), Db.P("@Tipe", u.TipeUnitId), Db.P("@Nomor", u.NomorUnit),
            Db.P("@Lantai", u.Lantai), Db.P("@Status", u.Status), Db.P("@Id", u.UnitId));

    public static void Delete(int unitId)
        => Db.Execute("DELETE FROM Unit WHERE UnitId = @Id", Db.P("@Id", unitId));

    public static bool NomorSudahDipakai(int towerId, string nomor, int kecualiId)
        => Convert.ToInt32(Db.Scalar(
            "SELECT COUNT(*) FROM Unit WHERE TowerId = @Tower AND NomorUnit = @Nomor AND UnitId <> @Id",
            Db.P("@Tower", towerId), Db.P("@Nomor", nomor), Db.P("@Id", kecualiId))) > 0;

    public static bool PunyaKontrakAktif(int unitId)
        => Convert.ToInt32(Db.Scalar("SELECT COUNT(*) FROM Kontrak WHERE UnitId = @Id AND Status = 'Aktif'",
            Db.P("@Id", unitId))) > 0;
}
