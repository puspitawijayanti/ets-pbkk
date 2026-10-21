using System.IO;
using System.Text.Json;

namespace HunianKu.Helpers;


public static class AppConfig
{
    public static string FilePath => Path.Combine(AppContext.BaseDirectory, "appsettings.json");

    public static string ConnectionString { get; private set; } = "";

    public static void Load()
    {
        if (!File.Exists(FilePath))
            throw new FileNotFoundException($"File konfigurasi tidak ditemukan:\n{FilePath}");


        var options = new JsonDocumentOptions
        {
            CommentHandling = JsonCommentHandling.Skip,
            AllowTrailingCommas = true
        };

        using var doc = JsonDocument.Parse(File.ReadAllText(FilePath), options);
        ConnectionString = doc.RootElement
            .GetProperty("ConnectionStrings")
            .GetProperty("HunianKu")
            .GetString() ?? "";

        if (string.IsNullOrWhiteSpace(ConnectionString))
            throw new InvalidOperationException("Connection string \"HunianKu\" di appsettings.json masih kosong.");
    }
}
