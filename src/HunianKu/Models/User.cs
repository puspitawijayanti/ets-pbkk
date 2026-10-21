namespace HunianKu.Models;

public class User
{
    public int UserId { get; set; }
    public string Username { get; set; } = "";
    public string PasswordHash { get; set; } = "";
    public string NamaLengkap { get; set; } = "";
    public string Role { get; set; } = "";   
    public bool IsAktif { get; set; } = true;
}

public static class Roles
{
    public const string Admin = "Admin";
    public const string Staf = "Staf";
    public const string Penghuni = "Penghuni";
}
