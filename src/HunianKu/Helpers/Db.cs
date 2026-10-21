using System.Data;
using Microsoft.Data.SqlClient;

namespace HunianKu.Helpers;

public static class Db
{

    public static SqlConnection OpenConnection()
    {
        var conn = new SqlConnection(AppConfig.ConnectionString);
        conn.Open();
        return conn;
    }


    public static SqlParameter P(string name, object? value) => new(name, value ?? DBNull.Value);

    public static List<T> Query<T>(string sql, Func<SqlDataReader, T> map, params SqlParameter[] parameters)
        => QueryCore(sql, CommandType.Text, map, parameters);


    public static List<T> QueryProc<T>(string procName, Func<SqlDataReader, T> map, params SqlParameter[] parameters)
        => QueryCore(procName, CommandType.StoredProcedure, map, parameters);

    private static List<T> QueryCore<T>(string sql, CommandType type, Func<SqlDataReader, T> map, SqlParameter[] parameters)
    {
        using var conn = OpenConnection();
        using var cmd = new SqlCommand(sql, conn) { CommandType = type };
        cmd.Parameters.AddRange(parameters);

        using var reader = cmd.ExecuteReader();
        var hasil = new List<T>();
        while (reader.Read())
            hasil.Add(map(reader));
        return hasil;
    }

    public static int Execute(string sql, params SqlParameter[] parameters)
    {
        using var conn = OpenConnection();
        using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.AddRange(parameters);
        return cmd.ExecuteNonQuery();
    }


    public static void ExecuteProc(string procName, params SqlParameter[] parameters)
    {
        using var conn = OpenConnection();
        using var cmd = new SqlCommand(procName, conn) { CommandType = CommandType.StoredProcedure };
        cmd.Parameters.AddRange(parameters);
        cmd.ExecuteNonQuery();
    }

    public static object? Scalar(string sql, params SqlParameter[] parameters)
    {
        using var conn = OpenConnection();
        using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.AddRange(parameters);
        var hasil = cmd.ExecuteScalar();
        return hasil == DBNull.Value ? null : hasil;
    }



    public static int Int(this SqlDataReader r, string kolom) => Convert.ToInt32(r[kolom]);

    public static int? IntNull(this SqlDataReader r, string kolom)
        => r[kolom] == DBNull.Value ? null : Convert.ToInt32(r[kolom]);

    public static string Str(this SqlDataReader r, string kolom)
        => r[kolom] == DBNull.Value ? "" : Convert.ToString(r[kolom])!;

    public static string? StrNull(this SqlDataReader r, string kolom)
        => r[kolom] == DBNull.Value ? null : Convert.ToString(r[kolom]);

    public static decimal Dec(this SqlDataReader r, string kolom)
        => r[kolom] == DBNull.Value ? 0 : Convert.ToDecimal(r[kolom]);

    public static DateTime Date(this SqlDataReader r, string kolom) => Convert.ToDateTime(r[kolom]);

    public static DateTime? DateNull(this SqlDataReader r, string kolom)
        => r[kolom] == DBNull.Value ? null : Convert.ToDateTime(r[kolom]);

    public static TimeSpan Time(this SqlDataReader r, string kolom) => (TimeSpan)r[kolom];

    public static bool Bool(this SqlDataReader r, string kolom) => Convert.ToBoolean(r[kolom]);
}
