namespace HunianKu.Models;


public class Penghuni
{
    public int PenghuniId { get; set; }
    public int UserId { get; set; }


    public string NamaLengkap { get; set; } = "";
    public string Username { get; set; } = "";
    public bool IsAktif { get; set; } = true;


    public string NIK { get; set; } = "";
    public string JenisKelamin { get; set; } = "L";
    public string NoHp { get; set; } = "";
    public string? Email { get; set; }
    public string? Pekerjaan { get; set; }
    public string? KontakDarurat { get; set; }

    public string? UnitAktif { get; set; }

    public string Tampilan => $"{NamaLengkap} ({NIK})";
}
