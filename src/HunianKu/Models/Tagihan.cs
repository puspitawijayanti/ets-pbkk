namespace HunianKu.Models;

public static class StatusTagihan
{
    public const string BelumBayar = "Belum Bayar";
    public const string MenungguVerifikasi = "Menunggu Verifikasi";
    public const string Lunas = "Lunas";
}

public class Tagihan
{
    public int TagihanId { get; set; }
    public string NomorTagihan { get; set; } = "";
    public int KontrakId { get; set; }
    public int PenghuniId { get; set; }
    public string NamaPenghuni { get; set; } = "";
    public string KodeUnit { get; set; } = "";
    public DateTime Periode { get; set; }
    public DateTime TanggalJatuhTempo { get; set; }
    public decimal PemakaianListrik { get; set; }
    public decimal PemakaianAir { get; set; }
    public decimal BiayaSewa { get; set; }
    public decimal BiayaListrik { get; set; }
    public decimal BiayaAir { get; set; }
    public decimal BiayaIpl { get; set; }
    public decimal TotalTagihan { get; set; }  
    public decimal Denda { get; set; }
    public decimal GrandTotal { get; set; }    
    public string Status { get; set; } = "";
    public int HariTerlambat { get; set; }

    public string PeriodeTeks => Periode.ToString("MMMM yyyy");
}


public class HasilGenerateTagihan
{
    public string NomorKontrak { get; set; } = "";
    public string KodeUnit { get; set; } = "";
    public string NamaPenghuni { get; set; } = "";
    public string Keterangan { get; set; } = "";   
}
