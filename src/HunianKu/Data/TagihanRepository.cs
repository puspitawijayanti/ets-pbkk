using HunianKu.Helpers;
using HunianKu.Models;
using Microsoft.Data.SqlClient;

namespace HunianKu.Data;

public static class TagihanRepository
{
    private const string SelectSql = @"
        SELECT TagihanId, NomorTagihan, KontrakId, PenghuniId, NamaPenghuni, KodeUnit, Periode,
               TanggalJatuhTempo, PemakaianListrik, PemakaianAir, BiayaSewa, BiayaListrik, BiayaAir,
               BiayaIpl, TotalTagihan, Denda, GrandTotal, Status, HariTerlambat
        FROM vw_TagihanDetail";

    private static Tagihan Map(SqlDataReader r) => new()
    {
        TagihanId = r.Int("TagihanId"),
        NomorTagihan = r.Str("NomorTagihan"),
        KontrakId = r.Int("KontrakId"),
        PenghuniId = r.Int("PenghuniId"),
        NamaPenghuni = r.Str("NamaPenghuni"),
        KodeUnit = r.Str("KodeUnit"),
        Periode = r.Date("Periode"),
        TanggalJatuhTempo = r.Date("TanggalJatuhTempo"),
        PemakaianListrik = r.Dec("PemakaianListrik"),
        PemakaianAir = r.Dec("PemakaianAir"),
        BiayaSewa = r.Dec("BiayaSewa"),
        BiayaListrik = r.Dec("BiayaListrik"),
        BiayaAir = r.Dec("BiayaAir"),
        BiayaIpl = r.Dec("BiayaIpl"),
        TotalTagihan = r.Dec("TotalTagihan"),
        Denda = r.Dec("Denda"),
        GrandTotal = r.Dec("GrandTotal"),
        Status = r.Str("Status"),
        HariTerlambat = r.Int("HariTerlambat")
    };

    public static void HitungDenda() => Db.ExecuteProc("dbo.sp_HitungDenda");

    public static List<HasilGenerateTagihan> Generate(DateTime periode)
        => Db.QueryProc("dbo.sp_GenerateTagihanBulanan",
            r => new HasilGenerateTagihan
            {
                NomorKontrak = r.Str("NomorKontrak"),
                KodeUnit = r.Str("KodeUnit"),
                NamaPenghuni = r.Str("NamaPenghuni"),
                Keterangan = r.Str("Keterangan")
            },
            Db.P("@Periode", periode));

    public static List<Tagihan> GetAll(DateTime? periode = null, string? status = null, string cari = "")
        => Db.Query($@"{SelectSql}
                       WHERE (@Periode IS NULL OR Periode = @Periode)
                         AND (@Status IS NULL OR Status = @Status)
                         AND (@Cari = '' OR NamaPenghuni LIKE '%' + @Cari + '%'
                              OR KodeUnit LIKE '%' + @Cari + '%' OR NomorTagihan LIKE '%' + @Cari + '%')
                       ORDER BY Periode DESC, KodeUnit",
            Map, Db.P("@Periode", periode), Db.P("@Status", status), Db.P("@Cari", cari.Trim()));

    public static List<Tagihan> GetByPenghuni(int penghuniId)
        => Db.Query($"{SelectSql} WHERE PenghuniId = @Id ORDER BY Periode DESC", Map, Db.P("@Id", penghuniId));
}