using HunianKu.Helpers;
using HunianKu.Models;
using Microsoft.Data.SqlClient;

namespace HunianKu.Data;

public static class TowerRepository
{
    private const string SelectSql = @"
        SELECT t.TowerId, t.KodeTower, t.NamaTower, t.Alamat, t.JumlahLantai,
               (SELECT COUNT(*) FROM Unit u WHERE u.TowerId = t.TowerId) AS JumlahUnit
        FROM Tower t";

    private static Tower Map(SqlDataReader r) => new()
    {
        TowerId = r.Int("TowerId"),
        KodeTower = r.Str("KodeTower"),
        NamaTower = r.Str("NamaTower"),
        Alamat = r.StrNull("Alamat"),
        JumlahLantai = r.Int("JumlahLantai"),
        JumlahUnit = r.Int("JumlahUnit")
    };

    public static List<Tower> GetAll()
        => Db.Query($"{SelectSql} ORDER BY t.KodeTower", Map);

    public static Tower? GetById(int towerId)
        => Db.Query($"{SelectSql} WHERE t.TowerId = @Id", Map, Db.P("@Id", towerId)).FirstOrDefault();

    public static void Insert(Tower t)
        => Db.Execute(@"INSERT INTO Tower (KodeTower, NamaTower, Alamat, JumlahLantai)
                        VALUES (@Kode, @Nama, @Alamat, @Lantai)",
            Db.P("@Kode", t.KodeTower), Db.P("@Nama", t.NamaTower),
            Db.P("@Alamat", t.Alamat), Db.P("@Lantai", t.JumlahLantai));

    public static void Update(Tower t)
        => Db.Execute(@"UPDATE Tower SET KodeTower = @Kode, NamaTower = @Nama, Alamat = @Alamat,
                               JumlahLantai = @Lantai
                        WHERE TowerId = @Id",
            Db.P("@Kode", t.KodeTower), Db.P("@Nama", t.NamaTower), Db.P("@Alamat", t.Alamat),
            Db.P("@Lantai", t.JumlahLantai), Db.P("@Id", t.TowerId));

    public static void Delete(int towerId)
        => Db.Execute("DELETE FROM Tower WHERE TowerId = @Id", Db.P("@Id", towerId));

    public static bool KodeSudahDipakai(string kode, int kecualiId)
        => Convert.ToInt32(Db.Scalar("SELECT COUNT(*) FROM Tower WHERE KodeTower = @Kode AND TowerId <> @Id",
            Db.P("@Kode", kode), Db.P("@Id", kecualiId))) > 0;

    public static int LantaiTertinggiTerpakai(int towerId)
        => Convert.ToInt32(Db.Scalar("SELECT ISNULL(MAX(Lantai), 0) FROM Unit WHERE TowerId = @Id",
            Db.P("@Id", towerId)));
}
