using System.Globalization;

namespace HunianKu.Models;

public class OkupansiTower
{
    public string KodeTower { get; set; } = "";
    public string NamaTower { get; set; } = "";
    public int TotalUnit { get; set; }
    public int UnitTerisi { get; set; }
    public int UnitKosong { get; set; }
    public int UnitMaintenance { get; set; }
    public decimal PersenOkupansi { get; set; }
}

public class PendapatanBulanan
{
    public int Bulan { get; set; }
    public int JumlahTransaksi { get; set; }
    public decimal Sewa { get; set; }
    public decimal Listrik { get; set; }
    public decimal Air { get; set; }
    public decimal Ipl { get; set; }
    public decimal Denda { get; set; }
    public decimal TotalPendapatan { get; set; }


    public double PersenBatang { get; set; }

    public string NamaBulan => CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(Bulan);
}

public class Tunggakan
{
    public string NomorTagihan { get; set; } = "";
    public string NamaPenghuni { get; set; } = "";
    public string KodeUnit { get; set; } = "";
    public DateTime Periode { get; set; }
    public DateTime TanggalJatuhTempo { get; set; }
    public int HariTerlambat { get; set; }
    public decimal TotalTagihan { get; set; }
    public decimal Denda { get; set; }
    public decimal GrandTotal { get; set; }

    public string PeriodeTeks => Periode.ToString("MMMM yyyy");
}


public class RingkasanDashboard
{
    // Admin
    public int TotalUnit { get; set; }
    public int UnitTerisi { get; set; }
    public decimal TotalTunggakan { get; set; }
    public decimal PendapatanBulanIni { get; set; }

    // Staf
    public int MeteranBelumDiinput { get; set; }
    public int PembayaranMenunggu { get; set; }

    // Bersama
    public int KomplainDiajukan { get; set; }
    public int KomplainDiproses { get; set; }

    public decimal PersenOkupansi => TotalUnit == 0 ? 0 : Math.Round(UnitTerisi * 100m / TotalUnit, 1);
}
