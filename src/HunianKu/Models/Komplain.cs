namespace HunianKu.Models;

public static class StatusKomplain
{
    public const string Diajukan = "Diajukan";
    public const string Diproses = "Diproses";
    public const string Selesai = "Selesai";
    public static readonly string[] Semua = [Diajukan, Diproses, Selesai];
}

public static class KategoriKomplain
{
    public static readonly string[] Semua = ["Listrik", "Air", "AC", "Bangunan", "Kebersihan", "Lainnya"];
}

public static class PrioritasKomplain
{
    public static readonly string[] Semua = ["Rendah", "Sedang", "Tinggi"];
}

public class Komplain
{
    public int KomplainId { get; set; }
    public string NomorTiket { get; set; } = "";
    public int PenghuniId { get; set; }
    public int UnitId { get; set; }
    public string Kategori { get; set; } = "";
    public string Judul { get; set; } = "";
    public string Deskripsi { get; set; } = "";
    public string Prioritas { get; set; } = "Sedang";
    public string Status { get; set; } = StatusKomplain.Diajukan;
    public int? TeknisiId { get; set; }
    public DateTime TanggalAjukan { get; set; }
    public DateTime? TanggalSelesai { get; set; }
    public string? CatatanPenanganan { get; set; }

    // dari JOIN
    public string NamaPenghuni { get; set; } = "";
    public string KodeUnit { get; set; } = "";
    public string? NamaTeknisi { get; set; }
}
