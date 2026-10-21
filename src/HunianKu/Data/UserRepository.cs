using HunianKu.Helpers;
using HunianKu.Models;
using Microsoft.Data.SqlClient;

namespace HunianKu.Data;

public static class UserRepository
{
    private const string SelectSql =
        "SELECT UserId, Username, PasswordHash, NamaLengkap, Role, IsAktif FROM Users";

    private static User Map(SqlDataReader r) => new()
    {
        UserId = r.Int("UserId"),
        Username = r.Str("Username"),
        PasswordHash = r.Str("PasswordHash"),
        NamaLengkap = r.Str("NamaLengkap"),
        Role = r.Str("Role"),
        IsAktif = r.Bool("IsAktif")
    };

    public static User? GetByUsername(string username)
        => Db.Query($"{SelectSql} WHERE Username = @Username", Map, Db.P("@Username", username)).FirstOrDefault();

    public static User? GetById(int userId)
        => Db.Query($"{SelectSql} WHERE UserId = @UserId", Map, Db.P("@UserId", userId)).FirstOrDefault();

    public static void UpdatePassword(int userId, string passwordHash)
        => Db.Execute("UPDATE Users SET PasswordHash = @Hash WHERE UserId = @UserId",
            Db.P("@Hash", passwordHash), Db.P("@UserId", userId));

    public static bool UsernameSudahDipakai(string username, int? kecualiUserId)
        => Convert.ToInt32(Db.Scalar(
            "SELECT COUNT(*) FROM Users WHERE Username = @Username AND UserId <> ISNULL(@Kecuali, 0)",
            Db.P("@Username", username), Db.P("@Kecuali", kecualiUserId))) > 0;
}
