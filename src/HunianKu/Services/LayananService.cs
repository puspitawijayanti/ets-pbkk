using HunianKu.Data;
using HunianKu.Helpers;
using HunianKu.Models;

namespace HunianKu.Services;

public static class LayananService
{

    public static void AjukanKomplain(Komplain k)
    {
        var kontrak = KontrakRepository.GetAktifByPenghuni(k.PenghuniId)
            ?? throw new ValidasiException("Komplain hanya bisa diajukan oleh penghuni dengan kontrak aktif.");
        k.UnitId = kontrak.UnitId;

        if (!KategoriKomplain.Semua.Contains(k.Kategori)) throw new ValidasiException("Pilih kategori komplain.");
        if (!PrioritasKomplain.Semua.Contains(k.Prioritas)) throw new ValidasiException("Pilih prioritas komplain.");
        k.Judul = Input.Wajib(k.Judul, "Judul", 100);
        k.Deskripsi = Input.Wajib(k.Deskripsi, "Deskripsi", 1000);
        if (k.Deskripsi.Length < 10)
            throw new ValidasiException("Deskripsi minimal 10 karakter agar teknisi paham masalahnya.");

        KomplainRepository.Insert(k);
    }

    public static void TanganiKomplain(Komplain k, int? teknisiId, string statusBaru, string? catatan)
    {
        if (k.Status == StatusKomplain.Selesai)
            throw new ValidasiException("Komplain yang sudah selesai tidak dapat diubah.");
        if (!StatusKomplain.Semua.Contains(statusBaru))
            throw new ValidasiException("Pilih status komplain.");
        if (Array.IndexOf(StatusKomplain.Semua, statusBaru) < Array.IndexOf(StatusKomplain.Semua, k.Status))
            throw new ValidasiException($"Status tidak bisa dikembalikan dari {k.Status} ke {statusBaru}.");
        if (statusBaru != StatusKomplain.Diajukan && teknisiId == null)
            throw new ValidasiException("Pilih teknisi yang ditugaskan sebelum memproses komplain.");

        catatan = Input.Opsional(catatan, "Catatan penanganan", 500);
        if (statusBaru == StatusKomplain.Selesai && catatan == null)
            throw new ValidasiException("Isi catatan penanganan sebelum menandai komplain selesai.");

        if (KomplainRepository.UpdatePenanganan(k.KomplainId, teknisiId, statusBaru, catatan) == 0)
            throw new ValidasiException("Komplain sudah diselesaikan oleh staf lain. Muat ulang data.");
    }


    public static int BuatBooking(BookingFasilitas b)
    {
        if (b.FasilitasId == 0) throw new ValidasiException("Pilih fasilitas.");
        if (b.Tanggal.Date < DateTime.Today) throw new ValidasiException("Tanggal booking tidak boleh di masa lalu.");
        if (b.JamSelesai <= b.JamMulai) throw new ValidasiException("Jam selesai harus setelah jam mulai.");
        if (b.JumlahOrang < 1) throw new ValidasiException("Jumlah orang minimal 1.");
        b.Keperluan = Input.Opsional(b.Keperluan, "Keperluan", 200);

        return BookingRepository.Buat(b);
    }

    public static void BatalkanBooking(BookingFasilitas b)
    {
        if (!b.BisaDibatalkan)
            throw new ValidasiException("Booking yang sudah lewat atau sudah dibatalkan tidak dapat dibatalkan.");
        if (BookingRepository.Batalkan(b.BookingId, b.PenghuniId) == 0)
            throw new ValidasiException("Booking tidak ditemukan atau sudah dibatalkan.");
    }
}
