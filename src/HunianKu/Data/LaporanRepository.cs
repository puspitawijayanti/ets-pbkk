using HunianKu.Helpers;
using HunianKu.Models;

namespace HunianKu.Data;

public static class LaporanRepository
{
    public static List<OkupansiTower> GetOkupansi()
        => Db.Query(@"SELECT KodeTower, NamaTower, TotalUnit, UnitTerisi, UnitKosong, UnitMaintenance, PersenOkupansi
                      FROM vw_OkupansiTower ORDER BY KodeTower",
            r => new OkupansiTower
            {
                KodeTower = r.Str("KodeTower"),
                NamaTower = r.Str("NamaTower"),
                TotalUnit = r.Int("TotalUnit"),
                UnitTerisi = r.Int("UnitTerisi"),
                UnitKosong = r.Int("UnitKosong"),
                UnitMaintenance = r.Int("UnitMaintenance"),
                PersenOkupansi = r.Dec("PersenOkupansi")
            });

    public static List<PendapatanBulanan> GetPendapatan(int tahun)
        => Db.QueryProc("dbo.sp_LaporanPendapatan",
            r => new PendapatanBulanan
            {
                Bulan = r.Int("Bulan"),
                JumlahTransaksi = r.Int("JumlahTransaksi"),
                Sewa = r.Dec("Sewa"),
                Listrik = r.Dec("Listrik"),
                Air = r.Dec("Air"),
                Ipl = r.Dec("Ipl"),
                Denda = r.Dec("Denda"),
                TotalPendapatan = r.Dec("TotalPendapatan")
            },
            Db.P("@Tahun", tahun));

    public static List<Tunggakan> GetTunggakan()
        => Db.QueryProc("dbo.sp_LaporanTunggakan",
            r => new Tunggakan
            {
                NomorTagihan = r.Str("NomorTagihan"),
                NamaPenghuni = r.Str("NamaPenghuni"),
                KodeUnit = r.Str("KodeUnit"),
                Periode = r.Date("Periode"),
                TanggalJatuhTempo = r.Date("TanggalJatuhTempo"),
                HariTerlambat = r.Int("HariTerlambat"),
                TotalTagihan = r.Dec("TotalTagihan"),
                Denda = r.Dec("Denda"),
                GrandTotal = r.Dec("GrandTotal")
            });

    public static RingkasanDashboard GetRingkasan()
    {
        var hariIni = DateTime.Today;
        var bulanLalu = new DateTime(hariIni.Year, hariIni.Month, 1).AddMonths(-1);

        return Db.Query(@"
            SELECT
              (SELECT COUNT(*) FROM Unit) AS TotalUnit,
              (SELECT COUNT(*) FROM Unit WHERE Status = 'Terisi') AS UnitTerisi,
              (SELECT ISNULL(SUM(TotalTagihan + Denda), 0) FROM Tagihan
                 WHERE Status = 'Belum Bayar' AND TanggalJatuhTempo < CAST(GETDATE() AS DATE)) AS TotalTunggakan,
              (SELECT ISNULL(SUM(JumlahBayar), 0) FROM Pembayaran
                 WHERE Status = 'Diterima' AND YEAR(TanggalBayar) = YEAR(GETDATE())
                   AND MONTH(TanggalBayar) = MONTH(GETDATE())) AS PendapatanBulanIni,
              (SELECT COUNT(*) FROM Kontrak k
                 WHERE k.Status <> 'Dibatalkan' AND k.TanggalMulai <= EOMONTH(@BulanLalu)
                   AND k.TanggalSelesai >= @BulanLalu
                   AND NOT EXISTS (SELECT 1 FROM Meteran m WHERE m.UnitId = k.UnitId AND m.Periode = @BulanLalu)
              ) AS MeteranBelumDiinput,
              (SELECT COUNT(*) FROM Pembayaran WHERE Status = 'Menunggu') AS PembayaranMenunggu,
              (SELECT COUNT(*) FROM Komplain WHERE Status = 'Diajukan') AS KomplainDiajukan,
              (SELECT COUNT(*) FROM Komplain WHERE Status = 'Diproses') AS KomplainDiproses",
            r => new RingkasanDashboard
            {
                TotalUnit = r.Int("TotalUnit"),
                UnitTerisi = r.Int("UnitTerisi"),
                TotalTunggakan = r.Dec("TotalTunggakan"),
                PendapatanBulanIni = r.Dec("PendapatanBulanIni"),
                MeteranBelumDiinput = r.Int("MeteranBelumDiinput"),
                PembayaranMenunggu = r.Int("PembayaranMenunggu"),
                KomplainDiajukan = r.Int("KomplainDiajukan"),
                KomplainDiproses = r.Int("KomplainDiproses")
            },
            Db.P("@BulanLalu", bulanLalu)).Single();
    }


    public static List<Tarif> GetTarif()
        => Db.Query("SELECT KodeTarif, NamaTarif, Nilai, Satuan, DiubahPada FROM Tarif ORDER BY KodeTarif",
            r => new Tarif
            {
                KodeTarif = r.Str("KodeTarif"),
                NamaTarif = r.Str("NamaTarif"),
                Nilai = r.Dec("Nilai"),
                Satuan = r.Str("Satuan"),
                DiubahPada = r.Date("DiubahPada")
            });

    public static decimal GetNilaiTarif(string kodeTarif)
        => Convert.ToDecimal(Db.Scalar("SELECT Nilai FROM Tarif WHERE KodeTarif = @Kode", Db.P("@Kode", kodeTarif)) ?? 0m);

    public static void UpdateTarif(string kodeTarif, decimal nilai)
        => Db.Execute("UPDATE Tarif SET Nilai = @Nilai, DiubahPada = SYSDATETIME() WHERE KodeTarif = @Kode",
            Db.P("@Nilai", nilai), Db.P("@Kode", kodeTarif));
}
