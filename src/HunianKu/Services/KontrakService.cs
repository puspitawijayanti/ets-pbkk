using HunianKu.Data;
using HunianKu.Helpers;
using HunianKu.Models;

namespace HunianKu.Services;

public static class KontrakService
{
    public static void Buat(Kontrak k)
    {
        if (k.PenghuniId == 0) throw new ValidasiException("Silakan pilih penghuni.");
        if (k.UnitId == 0) throw new ValidasiException("Silakan pilih unit.");
        ValidasiPeriode(k.TanggalMulai, k.TanggalSelesai);
        if (k.HargaSewaBulanan <= 0) throw new ValidasiException("Harga sewa harus lebih dari 0.");
        if (k.Deposit < 0) throw new ValidasiException("Deposit tidak boleh negatif.");

        if (KontrakRepository.GetAktifByPenghuni(k.PenghuniId) != null)
            throw new ValidasiException("Penghuni ini sudah memiliki kontrak aktif.");

        KontrakRepository.Insert(k);   // juga mengubah unit menjadi "Terisi"
    }

    public static void Ubah(Kontrak k, DateTime tanggalSelesaiBaru, decimal depositBaru)
    {
        if (k.Status != StatusKontrak.Aktif)
            throw new ValidasiException("Hanya kontrak aktif yang dapat diubah.");
        ValidasiPeriode(k.TanggalMulai, tanggalSelesaiBaru);
        if (tanggalSelesaiBaru < DateTime.Today)
            throw new ValidasiException("Tanggal selesai tidak boleh sebelum hari ini. Gunakan tombol Akhiri Kontrak.");
        if (depositBaru < 0) throw new ValidasiException("Deposit tidak boleh negatif.");

        KontrakRepository.Update(k.KontrakId, tanggalSelesaiBaru, depositBaru);
    }

    public static void Akhiri(Kontrak k, bool dibatalkan)
    {
        if (k.Status != StatusKontrak.Aktif)
            throw new ValidasiException("Kontrak ini sudah tidak aktif.");
        KontrakRepository.Akhiri(k.KontrakId, k.UnitId, dibatalkan ? StatusKontrak.Dibatalkan : StatusKontrak.Selesai);
    }

    private static void ValidasiPeriode(DateTime mulai, DateTime selesai)
    {
        if (selesai <= mulai)
            throw new ValidasiException("Tanggal selesai harus setelah tanggal mulai.");
        if (selesai < mulai.AddMonths(1).AddDays(-1))
            throw new ValidasiException("Masa kontrak minimal 1 bulan.");
    }
}
