using HunianKu.Helpers;
using HunianKu.Models;
using Microsoft.Data.SqlClient;

namespace HunianKu.Data;

public static class BookingRepository
{
    private const string SelectSql = @"
        SELECT b.BookingId, b.FasilitasId, b.PenghuniId, b.Tanggal, b.JamMulai, b.JamSelesai,
               b.JumlahOrang, b.Keperluan, b.Status, f.NamaFasilitas, us.NamaLengkap AS NamaPenghuni,
               ISNULL((SELECT tw.KodeTower + '-' + u.NomorUnit FROM Kontrak k
                       JOIN Unit u ON u.UnitId = k.UnitId JOIN Tower tw ON tw.TowerId = u.TowerId
                       WHERE k.PenghuniId = b.PenghuniId AND k.Status = 'Aktif'), '-') AS KodeUnit
        FROM BookingFasilitas b
        JOIN Fasilitas f ON f.FasilitasId = b.FasilitasId
        JOIN Penghuni p  ON p.PenghuniId = b.PenghuniId
        JOIN Users us    ON us.UserId = p.UserId";

    private static BookingFasilitas Map(SqlDataReader r) => new()
    {
        BookingId = r.Int("BookingId"),
        FasilitasId = r.Int("FasilitasId"),
        PenghuniId = r.Int("PenghuniId"),
        Tanggal = r.Date("Tanggal"),
        JamMulai = r.Time("JamMulai"),
        JamSelesai = r.Time("JamSelesai"),
        JumlahOrang = r.Int("JumlahOrang"),
        Keperluan = r.StrNull("Keperluan"),
        Status = r.Str("Status"),
        NamaFasilitas = r.Str("NamaFasilitas"),
        NamaPenghuni = r.Str("NamaPenghuni"),
        KodeUnit = r.Str("KodeUnit")
    };

    public static List<Fasilitas> GetFasilitasAktif()
        => Db.Query(@"SELECT FasilitasId, NamaFasilitas, Lokasi, Kapasitas, JamBuka, JamTutup, MaksDurasiJam
                      FROM Fasilitas WHERE IsAktif = 1 ORDER BY NamaFasilitas",
            r => new Fasilitas
            {
                FasilitasId = r.Int("FasilitasId"),
                NamaFasilitas = r.Str("NamaFasilitas"),
                Lokasi = r.Str("Lokasi"),
                Kapasitas = r.Int("Kapasitas"),
                JamBuka = r.Time("JamBuka"),
                JamTutup = r.Time("JamTutup"),
                MaksDurasiJam = r.Int("MaksDurasiJam")
            });

    public static List<BookingFasilitas> GetJadwal(int fasilitasId, DateTime tanggal)
        => Db.Query($@"{SelectSql}
                       WHERE b.FasilitasId = @Fasilitas AND b.Tanggal = @Tanggal AND b.Status = 'Aktif'
                       ORDER BY b.JamMulai",
            Map, Db.P("@Fasilitas", fasilitasId), Db.P("@Tanggal", tanggal.Date));

    public static List<BookingFasilitas> GetByPenghuni(int penghuniId)
        => Db.Query($"{SelectSql} WHERE b.PenghuniId = @Id ORDER BY b.Tanggal DESC, b.JamMulai",
            Map, Db.P("@Id", penghuniId));

    public static int Buat(BookingFasilitas b)
        => Db.QueryProc("dbo.sp_BuatBooking", r => r.Int("BookingId"),
            Db.P("@FasilitasId", b.FasilitasId), Db.P("@PenghuniId", b.PenghuniId),
            Db.P("@Tanggal", b.Tanggal.Date), Db.P("@JamMulai", b.JamMulai),
            Db.P("@JamSelesai", b.JamSelesai), Db.P("@JumlahOrang", b.JumlahOrang),
            Db.P("@Keperluan", b.Keperluan)).Single();

    public static int Batalkan(int bookingId, int penghuniId)
        => Db.Execute(@"UPDATE BookingFasilitas SET Status = 'Dibatalkan'
                        WHERE BookingId = @Id AND PenghuniId = @Penghuni AND Status = 'Aktif'",
            Db.P("@Id", bookingId), Db.P("@Penghuni", penghuniId));
}
