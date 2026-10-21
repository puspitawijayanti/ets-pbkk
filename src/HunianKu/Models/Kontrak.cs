namespace HunianKu.Models;

public static class StatusKontrak
{
    public const string Aktif = "Aktif";
    public const string Selesai = "Selesai";
    public const string Dibatalkan = "Dibatalkan";
}

public class Kontrak
{
    public int KontrakId { get; set; }
    public string NomorKontrak { get; set; } = "";
    public int UnitId { get; set; }
    public int PenghuniId { get; set; }
    public DateTime TanggalMulai { get; set; }
    public DateTime TanggalSelesai { get; set; }
    public decimal HargaSewaBulanan { get; set; }
    public decimal Deposit { get; set; }
    public string Status { get; set; } = StatusKontrak.Aktif;


    public string KodeUnit { get; set; } = "";
    public string NamaTipe { get; set; } = "";
    public string NamaPenghuni { get; set; } = "";
    public string NoHp { get; set; } = "";

    public int SisaHari => (TanggalSelesai.Date - DateTime.Today).Days;


    public string SisaHariTeks => Status == StatusKontrak.Aktif ? SisaHari.ToString() : "-";
}
