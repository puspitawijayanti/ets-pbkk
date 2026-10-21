namespace HunianKu.Models;

public class BookingFasilitas
{
    public int BookingId { get; set; }
    public int FasilitasId { get; set; }
    public int PenghuniId { get; set; }
    public DateTime Tanggal { get; set; }
    public TimeSpan JamMulai { get; set; }
    public TimeSpan JamSelesai { get; set; }
    public int JumlahOrang { get; set; }
    public string? Keperluan { get; set; }
    public string Status { get; set; } = "Aktif";


    public string NamaFasilitas { get; set; } = "";
    public string NamaPenghuni { get; set; } = "";
    public string KodeUnit { get; set; } = "";

    public string JamTeks => $"{JamMulai:hh\\:mm} - {JamSelesai:hh\\:mm}";


    public bool BisaDibatalkan => Status == "Aktif" && Tanggal.Date + JamMulai > DateTime.Now;
}
