namespace HunianKu.Models;

public class Tower
{
    public int TowerId { get; set; }
    public string KodeTower { get; set; } = "";
    public string NamaTower { get; set; } = "";
    public string? Alamat { get; set; }
    public int JumlahLantai { get; set; }
    public int JumlahUnit { get; set; }


    public string Tampilan => TowerId == 0 ? NamaTower : $"{KodeTower} - {NamaTower}";
}

public class TipeUnit
{
    public int TipeUnitId { get; set; }
    public string NamaTipe { get; set; } = "";
    public decimal LuasM2 { get; set; }
    public int JumlahKamar { get; set; }
    public decimal HargaSewaBulanan { get; set; }
    public decimal IplPerBulan { get; set; }
}

public static class StatusUnit
{
    public const string Kosong = "Kosong";
    public const string Terisi = "Terisi";
    public const string Maintenance = "Maintenance";
    public static readonly string[] Semua = [Kosong, Terisi, Maintenance];
}

public class Unit
{
    public int UnitId { get; set; }
    public int TowerId { get; set; }
    public int TipeUnitId { get; set; }
    public string NomorUnit { get; set; } = "";
    public int Lantai { get; set; }
    public string Status { get; set; } = StatusUnit.Kosong;

    public string KodeTower { get; set; } = "";
    public string NamaTipe { get; set; } = "";
    public decimal HargaSewaBulanan { get; set; }

    public string KodeUnit => $"{KodeTower}-{NomorUnit}";
    public string Tampilan => $"{KodeUnit} ({NamaTipe})";
}

public class Teknisi
{
    public int TeknisiId { get; set; }
    public string NamaTeknisi { get; set; } = "";
    public string Keahlian { get; set; } = "";
    public string NoHp { get; set; } = "";
    public string Tampilan => $"{NamaTeknisi} - {Keahlian}";
}

public class Fasilitas
{
    public int FasilitasId { get; set; }
    public string NamaFasilitas { get; set; } = "";
    public string Lokasi { get; set; } = "";
    public int Kapasitas { get; set; }
    public TimeSpan JamBuka { get; set; }
    public TimeSpan JamTutup { get; set; }
    public int MaksDurasiJam { get; set; }

    public string Keterangan =>
        $"{Lokasi} · buka {JamBuka:hh\\:mm}-{JamTutup:hh\\:mm} · kapasitas {Kapasitas} orang · maks {MaksDurasiJam} jam";
}

public class Tarif
{
    public string KodeTarif { get; set; } = "";
    public string NamaTarif { get; set; } = "";
    public decimal Nilai { get; set; }
    public string Satuan { get; set; } = "";
    public DateTime DiubahPada { get; set; }
}

public static class KodeTarif
{
    public const string ListrikPerKwh = "LISTRIK_PER_KWH";
    public const string AirPerM3 = "AIR_PER_M3";
    public const string DendaPerHari = "DENDA_PER_HARI";
    public const string JatuhTempoTanggal = "JATUH_TEMPO_TANGGAL";
    public const string PengingatKontrakHari = "PENGINGAT_KONTRAK_HARI";
}
