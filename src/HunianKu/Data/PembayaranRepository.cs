using HunianKu.Helpers;
using HunianKu.Models;
using Microsoft.Data.SqlClient;

namespace HunianKu.Data;

public static class PembayaranRepository
{
    private const string SelectSql = @"
        SELECT pb.PembayaranId, pb.TagihanId, pb.TanggalBayar, pb.JumlahBayar, pb.MetodeBayar,
               pb.NomorReferensi, pb.Status, pb.TanggalVerifikasi, pb.Catatan,
               v.NomorTagihan, v.NamaPenghuni, v.KodeUnit, v.Periode, v.GrandTotal,
               us.NamaLengkap AS NamaVerifikator
        FROM Pembayaran pb
        JOIN vw_TagihanDetail v ON v.TagihanId = pb.TagihanId
        LEFT JOIN Users us      ON us.UserId = pb.DiverifikasiOleh";

    private static Pembayaran Map(SqlDataReader r) => new()
    {
        PembayaranId = r.Int("PembayaranId"),
        TagihanId = r.Int("TagihanId"),
        TanggalBayar = r.Date("TanggalBayar"),
        JumlahBayar = r.Dec("JumlahBayar"),
        MetodeBayar = r.Str("MetodeBayar"),
        NomorReferensi = r.StrNull("NomorReferensi"),
        Status = r.Str("Status"),
        TanggalVerifikasi = r.DateNull("TanggalVerifikasi"),
        Catatan = r.StrNull("Catatan"),
        NomorTagihan = r.Str("NomorTagihan"),
        NamaPenghuni = r.Str("NamaPenghuni"),
        KodeUnit = r.Str("KodeUnit"),
        Periode = r.Date("Periode"),
        GrandTotalTagihan = r.Dec("GrandTotal"),
        NamaVerifikator = r.StrNull("NamaVerifikator")
    };

    public static List<Pembayaran> GetAll(string? status = null)
        => Db.Query($@"{SelectSql} WHERE (@Status IS NULL OR pb.Status = @Status)
                       ORDER BY pb.TanggalBayar DESC, pb.PembayaranId DESC",
            Map, Db.P("@Status", status));

    public static List<Pembayaran> GetByPenghuni(int penghuniId)
        => Db.Query($"{SelectSql} WHERE v.PenghuniId = @Id ORDER BY pb.TanggalBayar DESC, pb.PembayaranId DESC",
            Map, Db.P("@Id", penghuniId));

    public static void Ajukan(int tagihanId, string metode, string? nomorReferensi)
    {
        using var conn = Db.OpenConnection();
        using var tx = conn.BeginTransaction();
        try
        {
            var cmdTagihan = new SqlCommand(@"
                UPDATE Tagihan
                SET Denda = dbo.fn_HitungDenda(TanggalJatuhTempo, CAST(GETDATE() AS DATE)),
                    Status = 'Menunggu Verifikasi'
                WHERE TagihanId = @Id AND Status = 'Belum Bayar'", conn, tx);
            cmdTagihan.Parameters.Add(Db.P("@Id", tagihanId));
            if (cmdTagihan.ExecuteNonQuery() == 0)
                throw new ValidasiException("Tagihan ini sudah dibayar atau sedang menunggu verifikasi.");

            var cmdBayar = new SqlCommand(@"
                INSERT INTO Pembayaran (TagihanId, TanggalBayar, JumlahBayar, MetodeBayar, NomorReferensi, Status)
                SELECT TagihanId, CAST(GETDATE() AS DATE), TotalTagihan + Denda, @Metode, @Ref, 'Menunggu'
                FROM Tagihan WHERE TagihanId = @Id", conn, tx);
            cmdBayar.Parameters.AddRange([
                Db.P("@Id", tagihanId), Db.P("@Metode", metode), Db.P("@Ref", nomorReferensi)]);
            cmdBayar.ExecuteNonQuery();

            tx.Commit();
        }
        catch
        {
            tx.Rollback();
            throw;
        }
    }

    public static void Verifikasi(int pembayaranId, bool diterima, int verifikatorId, string? catatan)
    {
        using var conn = Db.OpenConnection();
        using var tx = conn.BeginTransaction();
        try
        {
            var cmdBayar = new SqlCommand(@"
                UPDATE Pembayaran
                SET Status = @Status, DiverifikasiOleh = @Oleh, TanggalVerifikasi = SYSDATETIME(), Catatan = @Catatan
                OUTPUT INSERTED.TagihanId
                WHERE PembayaranId = @Id AND Status = 'Menunggu'", conn, tx);
            cmdBayar.Parameters.AddRange([
                Db.P("@Status", diterima ? StatusPembayaran.Diterima : StatusPembayaran.Ditolak),
                Db.P("@Oleh", verifikatorId), Db.P("@Catatan", catatan), Db.P("@Id", pembayaranId)]);
            var tagihanId = cmdBayar.ExecuteScalar();
            if (tagihanId == null)
                throw new ValidasiException("Pembayaran ini sudah diverifikasi sebelumnya.");

            var cmdTagihan = new SqlCommand("UPDATE Tagihan SET Status = @Status WHERE TagihanId = @Id", conn, tx);
            cmdTagihan.Parameters.AddRange([
                Db.P("@Status", diterima ? StatusTagihan.Lunas : StatusTagihan.BelumBayar),
                Db.P("@Id", tagihanId)]);
            cmdTagihan.ExecuteNonQuery();

            tx.Commit();
        }
        catch
        {
            tx.Rollback();
            throw;
        }
    }
}