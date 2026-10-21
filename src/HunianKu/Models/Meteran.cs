namespace HunianKu.Models;

public class Meteran
{
    public int? MeteranId { get; set; }     
    public int UnitId { get; set; }
    public DateTime Periode { get; set; }
    public decimal ListrikAwal { get; set; }
    public decimal ListrikAkhir { get; set; }
    public decimal AirAwal { get; set; }
    public decimal AirAkhir { get; set; }

   
    public string KodeUnit { get; set; } = "";
    public string NamaPenghuni { get; set; } = "";
    public bool SudahDitagih { get; set; }

    public bool SudahDiinput => MeteranId.HasValue;
    public decimal PemakaianListrik => ListrikAkhir - ListrikAwal;
    public decimal PemakaianAir => AirAkhir - AirAwal;
    public string StatusInput => !SudahDiinput ? "Belum diinput" : SudahDitagih ? "Sudah ditagih" : "Sudah diinput";
}
