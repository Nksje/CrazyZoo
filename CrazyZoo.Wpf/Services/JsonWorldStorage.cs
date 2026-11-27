using System.IO;
using System.Text.Json;
using CrazyZoo.Core.Persistence;

namespace CrazyZoo.Wpf.Services;

// Maailma salvestamine JSON-faili (System.Text.Json)
public class JsonWorldStorage : IWorldStorage
{
    private static readonly JsonSerializerOptions Options = new() { WriteIndented = true };

    public async Task SaveAsync(WorldDto world, string path, CancellationToken token = default)
    {
        string json = JsonSerializer.Serialize(world, Options);
        await File.WriteAllTextAsync(path, json, token);
    }

    public async Task<WorldDto> LoadAsync(string path, CancellationToken token = default)
    {
        string text = await File.ReadAllTextAsync(path, token);
        try
        {
            WorldDto? world = JsonSerializer.Deserialize<WorldDto>(text, Options);
            if (world == null)
                throw new InvalidDataException("JSON-fail on tühi.");
            return world;
        }
        catch (JsonException ex)
        {
            // Katkine süntaks või vale väärtuse tüüp
            throw new InvalidDataException("Fail ei ole korrektne JSON: " + ex.Message);
        }
    }
}
