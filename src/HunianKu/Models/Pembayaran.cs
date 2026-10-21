namespace HunianKu.Models;

public static class StatusPembayaran
{
    public const string Menunggu = "Menunggu";
    public const string Diterima = "Diterima";
    public const string Ditolak = "Ditolak";
}

public static class MetodeBayar
{
    public static readonly string[] Semua = ["Transfer", "E-Wallet", "Tunai"];
}

public class Pembayaran
{
    public int PembayaranId { get; set; }
    public int TagihanId { get; set; }
    public DateTime TanggalBayar { get; set; }
    public decimal JumlahBayar { get; set; }
    public string MetodeBayar { get; set; } = "";
    public string? NomorReferensi { get; set; }
    public string Status { get; set; } = StatusPembayaran.Menunggu;
    public string? NamaVerifikator { get; set; }
    public DateTime? TanggalVerifikasi { get; set; }
    public string? Catatan { get; set; }

    public string NomorTagihan { get; set; } = "";
    public string NamaPenghuni { get; set; } = "";
    public string KodeUnit { get; set; } = "";
    public DateTime Periode { get; set; }
    public decimal GrandTotalTagihan { get; set; }

    public string PeriodeTeks => Periode.ToString("MMMM yyyy");
}
