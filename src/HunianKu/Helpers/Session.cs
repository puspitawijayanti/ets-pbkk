using HunianKu.Models;

namespace HunianKu.Helpers;

public static class Session
{
    public static User? CurrentUser { get; private set; }

    public static int? PenghuniId { get; private set; }

    public static User User => CurrentUser ?? throw new InvalidOperationException("Belum ada user yang login.");

    public static void Login(User user, int? penghuniId)
    {
        CurrentUser = user;
        PenghuniId = penghuniId;
    }

    public static void Logout()
    {
        CurrentUser = null;
        PenghuniId = null;
    }
}
