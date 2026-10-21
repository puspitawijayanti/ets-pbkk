using HunianKu.Helpers;
using HunianKu.Models;
using Microsoft.Data.SqlClient;

namespace HunianKu.Data;

public static class KomplainRepository
{
    private const string SelectSql = @"
        SELECT k.KomplainId, k.NomorTiket, k.PenghuniId, k.UnitId, k.Kategori, k.Judul, k.Deskripsi,
               k.Prioritas, k.Status, k.TeknisiId, k.TanggalAjukan, k.TanggalSelesai, k.CatatanPenanganan,
               us.NamaLengkap AS NamaPenghuni, tw.KodeTower + '-' + u.NomorUnit AS KodeUnit, te.NamaTeknisi
        FROM Komplain k
        JOIN Penghuni p   ON p.PenghuniId = k.PenghuniId
        JOIN Users us     ON us.UserId = p.UserId
        JOIN Unit u       ON u.UnitId = k.UnitId
        JOIN Tower tw     ON tw.TowerId = u.TowerId
        LEFT JOIN Teknisi te ON te.TeknisiId = k.TeknisiId";


    private const string OrderSql = @"
        ORDER BY CASE k.Status WHEN 'Diajukan' THEN 0 WHEN 'Diproses' THEN 1 ELSE 2 END,
                 CASE k.Prioritas WHEN 'Tinggi' THEN 0 WHEN 'Sedang' THEN 1 ELSE 2 END,
                 k.TanggalAjukan DESC";

    private static Komplain Map(SqlDataReader r) => new()
    {
        KomplainId = r.Int("KomplainId"),
        NomorTiket = r.Str("NomorTiket"),
        PenghuniId = r.Int("PenghuniId"),
        UnitId = r.Int("UnitId"),
        Kategori = r.Str("Kategori"),
        Judul = r.Str("Judul"),
        Deskripsi = r.Str("Deskripsi"),
        Prioritas = r.Str("Prioritas"),
        Status = r.Str("Status"),
        TeknisiId = r.IntNull("TeknisiId"),
        TanggalAjukan = r.Date("TanggalAjukan"),
        TanggalSelesai = r.DateNull("TanggalSelesai"),
        CatatanPenanganan = r.StrNull("CatatanPenanganan"),
        NamaPenghuni = r.Str("NamaPenghuni"),
        KodeUnit = r.Str("KodeUnit"),
        NamaTeknisi = r.StrNull("NamaTeknisi")
    };

    public static List<Komplain> GetAll(string? status = null)
        => Db.Query($"{SelectSql} WHERE (@Status IS NULL OR k.Status = @Status) {OrderSql}",
            Map, Db.P("@Status", status));

    public static List<Komplain> GetByPenghuni(int penghuniId)
        => Db.Query($"{SelectSql} WHERE k.PenghuniId = @Id {OrderSql}", Map, Db.P("@Id", penghuniId));

    public static void Insert(Komplain k)
        => Db.Execute(@"INSERT INTO Komplain (PenghuniId, UnitId, Kategori, Judul, Deskripsi, Prioritas)
                        VALUES (@Penghuni, @Unit, @Kategori, @Judul, @Deskripsi, @Prioritas)",
            Db.P("@Penghuni", k.PenghuniId), Db.P("@Unit", k.UnitId), Db.P("@Kategori", k.Kategori),
            Db.P("@Judul", k.Judul), Db.P("@Deskripsi", k.Deskripsi), Db.P("@Prioritas", k.Prioritas));

    public static int UpdatePenanganan(int komplainId, int? teknisiId, string status, string? catatan)
        => Db.Execute(@"UPDATE Komplain
                        SET TeknisiId = @Teknisi, Status = @Status, CatatanPenanganan = @Catatan,
                            TanggalSelesai = CASE WHEN @Status = 'Selesai' THEN SYSDATETIME() ELSE NULL END
                        WHERE KomplainId = @Id AND Status <> 'Selesai'",
            Db.P("@Teknisi", teknisiId), Db.P("@Status", status), Db.P("@Catatan", catatan), Db.P("@Id", komplainId));

    public static List<Teknisi> GetTeknisiAktif()
        => Db.Query("SELECT TeknisiId, NamaTeknisi, Keahlian, NoHp FROM Teknisi WHERE IsAktif = 1 ORDER BY NamaTeknisi",
            r => new Teknisi
            {
                TeknisiId = r.Int("TeknisiId"),
                NamaTeknisi = r.Str("NamaTeknisi"),
                Keahlian = r.Str("Keahlian"),
                NoHp = r.Str("NoHp")
            });
}
