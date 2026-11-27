using System.IO;
using System.Xml.Serialization;
using CrazyZoo.Core.Persistence;

namespace CrazyZoo.Wpf.Services;

// Maailma salvestamine XML-faili (XmlSerializer)
public class XmlWorldStorage : IWorldStorage
{
    private readonly XmlSerializer _serializer = new(typeof(WorldDto));

    public async Task SaveAsync(WorldDto world, string path, CancellationToken token = default)
    {
        await using FileStream stream = File.Create(path);
        _serializer.Serialize(stream, world);
    }

    public async Task<WorldDto> LoadAsync(string path, CancellationToken token = default)
    {
        await using FileStream stream = File.OpenRead(path);
        try
        {
            WorldDto? world = _serializer.Deserialize(stream) as WorldDto;
            if (world == null)
                throw new InvalidDataException("XML-fail on tühi.");
            return world;
        }
        catch (InvalidOperationException ex)
        {
            // "Root element is missing", vale juurelement, vale väärtuse tüüp jne
            string reason = ex.InnerException?.Message ?? ex.Message;
            throw new InvalidDataException("Fail ei ole korrektne XML: " + reason);
        }
    }
}
