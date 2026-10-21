using HunianKu.Helpers;
using HunianKu.Models;
using Microsoft.Data.SqlClient;

namespace HunianKu.Data;

public static class TipeUnitRepository
{
    private static TipeUnit Map(SqlDataReader r) => new()
    {
        TipeUnitId = r.Int("TipeUnitId"),
        NamaTipe = r.Str("NamaTipe"),
        LuasM2 = r.Dec("LuasM2"),
        JumlahKamar = r.Int("JumlahKamar"),
        HargaSewaBulanan = r.Dec("HargaSewaBulanan"),
        IplPerBulan = r.Dec("IplPerBulan")
    };

    public static List<TipeUnit> GetAll()
        => Db.Query(@"SELECT TipeUnitId, NamaTipe, LuasM2, JumlahKamar, HargaSewaBulanan, IplPerBulan
                      FROM TipeUnit ORDER BY HargaSewaBulanan", Map);

    public static void Insert(TipeUnit t)
        => Db.Execute(@"INSERT INTO TipeUnit (NamaTipe, LuasM2, JumlahKamar, HargaSewaBulanan, IplPerBulan)
                        VALUES (@Nama, @Luas, @Kamar, @Harga, @Ipl)",
            Db.P("@Nama", t.NamaTipe), Db.P("@Luas", t.LuasM2), Db.P("@Kamar", t.JumlahKamar),
            Db.P("@Harga", t.HargaSewaBulanan), Db.P("@Ipl", t.IplPerBulan));

    public static void Update(TipeUnit t)
        => Db.Execute(@"UPDATE TipeUnit SET NamaTipe = @Nama, LuasM2 = @Luas, JumlahKamar = @Kamar,
                               HargaSewaBulanan = @Harga, IplPerBulan = @Ipl
                        WHERE TipeUnitId = @Id",
            Db.P("@Nama", t.NamaTipe), Db.P("@Luas", t.LuasM2), Db.P("@Kamar", t.JumlahKamar),
            Db.P("@Harga", t.HargaSewaBulanan), Db.P("@Ipl", t.IplPerBulan), Db.P("@Id", t.TipeUnitId));

    public static void Delete(int tipeUnitId)
        => Db.Execute("DELETE FROM TipeUnit WHERE TipeUnitId = @Id", Db.P("@Id", tipeUnitId));

    public static bool NamaSudahDipakai(string nama, int kecualiId)
        => Convert.ToInt32(Db.Scalar("SELECT COUNT(*) FROM TipeUnit WHERE NamaTipe = @Nama AND TipeUnitId <> @Id",
            Db.P("@Nama", nama), Db.P("@Id", kecualiId))) > 0;
}
