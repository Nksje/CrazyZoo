using System.IO;
using CrazyZoo.Core.Persistence;

namespace CrazyZoo.Wpf.Services;

// Failirežiimi teostus: valib JSON-i või XML-i faililaiendi järgi.
// Nii saab ViewModel ühe IWorldStorage'i ja kasutaja saab salvestada mõlemasse vormingusse.
public class FileWorldStorage : IWorldStorage
{
    private readonly JsonWorldStorage _json;
    private readonly XmlWorldStorage _xml;

    public FileWorldStorage(JsonWorldStorage json, XmlWorldStorage xml)
    {
        _json = json;
        _xml = xml;
    }

    public Task SaveAsync(WorldDto world, string path, CancellationToken token = default)
        => Choose(path).SaveAsync(world, path, token);

    public Task<WorldDto> LoadAsync(string path, CancellationToken token = default)
        => Choose(path).LoadAsync(path, token);

    private IWorldStorage Choose(string path)
    {
        string extension = Path.GetExtension(path).ToLowerInvariant();
        if (extension == ".json")
            return _json;
        if (extension == ".xml")
            return _xml;
        throw new InvalidDataException("Toetatud on ainult .json ja .xml failid.");
    }
}
