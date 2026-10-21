using HunianKu.Data;
using HunianKu.Helpers;
using HunianKu.Models;

namespace HunianKu.Services;

public static class TagihanService
{
    public static DateTime AwalBulanIni => new(DateTime.Today.Year, DateTime.Today.Month, 1);

    private static void ValidasiPeriode(DateTime periode)
    {
        if (periode.Day != 1)
            throw new ValidasiException("Periode harus tanggal 1.");
        if (periode >= AwalBulanIni)
            throw new ValidasiException("Periode harus bulan yang sudah berakhir (paling baru bulan lalu).");
    }


    public static void SimpanMeteran(Meteran m)
    {
        ValidasiPeriode(m.Periode);
        if (m.ListrikAwal < 0 || m.AirAwal < 0)
            throw new ValidasiException("Angka awal meteran tidak boleh negatif.");
        if (m.ListrikAkhir < m.ListrikAwal)
            throw new ValidasiException("Angka akhir listrik tidak boleh lebih kecil dari angka awal.");
        if (m.AirAkhir < m.AirAwal)
            throw new ValidasiException("Angka akhir air tidak boleh lebih kecil dari angka awal.");

        var sebelumnya = MeteranRepository.GetAkhirSebelumnya(m.UnitId, m.Periode);
        if (sebelumnya != null && (m.ListrikAwal < sebelumnya.Value.Listrik || m.AirAwal < sebelumnya.Value.Air))
            throw new ValidasiException(
                $"Angka awal tidak boleh lebih kecil dari angka akhir bulan sebelumnya " +
                $"(listrik {sebelumnya.Value.Listrik:N2}, air {sebelumnya.Value.Air:N2}).");

        if (m.MeteranId == null)
        {
            MeteranRepository.Insert(m, Session.User.UserId);
        }
        else if (MeteranRepository.Update(m, Session.User.UserId) == 0)
        {
            throw new ValidasiException("Meteran ini sudah dipakai untuk tagihan sehingga tidak dapat diubah.");
        }
    }


    public static List<HasilGenerateTagihan> Generate(DateTime periode)
    {
        ValidasiPeriode(periode);
        return TagihanRepository.Generate(periode);
    }


    public static void UbahTarif(Tarif t, decimal nilaiBaru)
    {
        switch (t.KodeTarif)
        {
            case KodeTarif.JatuhTempoTanggal:
                if (nilaiBaru < 1 || nilaiBaru > 28 || nilaiBaru != Math.Floor(nilaiBaru))
                    throw new ValidasiException("Tanggal jatuh tempo harus bilangan bulat 1-28 (agar ada di setiap bulan).");
                break;
            case KodeTarif.PengingatKontrakHari:
                if (nilaiBaru < 1 || nilaiBaru > 365 || nilaiBaru != Math.Floor(nilaiBaru))
                    throw new ValidasiException("Hari pengingat harus bilangan bulat 1-365.");
                break;
            default:
                if (nilaiBaru < 0) throw new ValidasiException("Tarif tidak boleh negatif.");
                break;
        }
        LaporanRepository.UpdateTarif(t.KodeTarif, nilaiBaru);
    }


    public static void AjukanPembayaran(Tagihan t, string? metode, string? nomorReferensi)
    {
        if (t.Status != StatusTagihan.BelumBayar)
            throw new ValidasiException("Tagihan ini sudah dibayar atau sedang menunggu verifikasi.");
        if (metode == null || !MetodeBayar.Semua.Contains(metode))
            throw new ValidasiException("Pilih metode pembayaran.");

        nomorReferensi = Input.Opsional(nomorReferensi, "Nomor referensi", 50);
        if (metode != "Tunai" && nomorReferensi == null)
            throw new ValidasiException("Nomor referensi/bukti transfer wajib diisi untuk pembayaran non-tunai.");

        PembayaranRepository.Ajukan(t.TagihanId, metode, nomorReferensi);
    }

    public static void Verifikasi(Pembayaran p, bool diterima, string? catatan)
    {
        if (p.Status != StatusPembayaran.Menunggu)
            throw new ValidasiException("Pembayaran ini sudah diverifikasi.");
        catatan = Input.Opsional(catatan, "Catatan", 250);
        if (!diterima && catatan == null)
            throw new ValidasiException("Tuliskan alasan penolakan pada kolom catatan.");

        PembayaranRepository.Verifikasi(p.PembayaranId, diterima, Session.User.UserId, catatan);
    }
}