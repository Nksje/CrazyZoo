using System.IO;
using System.Text.Json;

namespace CrazyZoo.Wpf.Services;

// Seadistus failist appsettings.json (kopeeritakse exe kõrvale)
public class AppSettings
{
    // "File" = JSON/XML failid, "Sql" = LocalDB
    public string Storage { get; set; } = "File";

    public string ConnectionString { get; set; } = "";

    public static AppSettings Load()
    {
        string path = Path.Combine(AppContext.BaseDirectory, "appsettings.json");
        if (!File.Exists(path))
            return new AppSettings();

        string json = File.ReadAllText(path);
        return JsonSerializer.Deserialize<AppSettings>(json) ?? new AppSettings();
    }
}
