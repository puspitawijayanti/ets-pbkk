using HunianKu.Helpers;
using HunianKu.Models;

namespace HunianKu.Data;

public static class MeteranRepository
{
    public static List<Meteran> GetDaftarPeriode(DateTime periode)
        => Db.Query(@"
            SELECT u.UnitId, tw.KodeTower + '-' + u.NomorUnit AS KodeUnit, us.NamaLengkap AS NamaPenghuni,
                   m.MeteranId, m.ListrikAwal, m.ListrikAkhir, m.AirAwal, m.AirAkhir,
                   CASE WHEN EXISTS (SELECT 1 FROM Tagihan t WHERE t.MeteranId = m.MeteranId)
                        THEN 1 ELSE 0 END AS SudahDitagih
            FROM Kontrak k
            JOIN Unit u      ON u.UnitId = k.UnitId
            JOIN Tower tw    ON tw.TowerId = u.TowerId
            JOIN Penghuni p  ON p.PenghuniId = k.PenghuniId
            JOIN Users us    ON us.UserId = p.UserId
            LEFT JOIN Meteran m ON m.UnitId = k.UnitId AND m.Periode = @Periode
            WHERE k.Status <> 'Dibatalkan'
              AND k.TanggalMulai <= EOMONTH(@Periode)
              AND k.TanggalSelesai >= @Periode
            ORDER BY KodeUnit",
            r => new Meteran
            {
                UnitId = r.Int("UnitId"),
                KodeUnit = r.Str("KodeUnit"),
                NamaPenghuni = r.Str("NamaPenghuni"),
                MeteranId = r.IntNull("MeteranId"),
                Periode = periode,
                ListrikAwal = r.Dec("ListrikAwal"),
                ListrikAkhir = r.Dec("ListrikAkhir"),
                AirAwal = r.Dec("AirAwal"),
                AirAkhir = r.Dec("AirAkhir"),
                SudahDitagih = r.Int("SudahDitagih") == 1
            },
            Db.P("@Periode", periode));


    public static (decimal Listrik, decimal Air)? GetAkhirSebelumnya(int unitId, DateTime periode)
    {
        var hasil = Db.Query(@"
            SELECT TOP 1 ListrikAkhir, AirAkhir FROM Meteran
            WHERE UnitId = @UnitId AND Periode < @Periode
            ORDER BY Periode DESC",
            r => (r.Dec("ListrikAkhir"), r.Dec("AirAkhir")),
            Db.P("@UnitId", unitId), Db.P("@Periode", periode));
        return hasil.Count == 0 ? null : hasil[0];
    }

    public static void Insert(Meteran m, int dicatatOleh)
        => Db.Execute(@"
            INSERT INTO Meteran (UnitId, Periode, ListrikAwal, ListrikAkhir, AirAwal, AirAkhir, DicatatOleh)
            VALUES (@UnitId, @Periode, @LAwal, @LAkhir, @AAwal, @AAkhir, @Oleh)",
            Db.P("@UnitId", m.UnitId), Db.P("@Periode", m.Periode),
            Db.P("@LAwal", m.ListrikAwal), Db.P("@LAkhir", m.ListrikAkhir),
            Db.P("@AAwal", m.AirAwal), Db.P("@AAkhir", m.AirAkhir), Db.P("@Oleh", dicatatOleh));

    public static int Update(Meteran m, int dicatatOleh)
        => Db.Execute(@"
            UPDATE Meteran SET ListrikAwal = @LAwal, ListrikAkhir = @LAkhir, AirAwal = @AAwal, AirAkhir = @AAkhir,
                   DicatatOleh = @Oleh, TanggalCatat = SYSDATETIME()
            WHERE MeteranId = @Id
              AND NOT EXISTS (SELECT 1 FROM Tagihan WHERE MeteranId = @Id)",
            Db.P("@LAwal", m.ListrikAwal), Db.P("@LAkhir", m.ListrikAkhir),
            Db.P("@AAwal", m.AirAwal), Db.P("@AAkhir", m.AirAkhir),
            Db.P("@Oleh", dicatatOleh), Db.P("@Id", m.MeteranId));
}