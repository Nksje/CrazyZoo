using System.Xml.Serialization;

namespace CrazyZoo.Core.Persistence;

// Kogu maailma salvestatav olek. Siin on ainult andmed: mitte aknaid, käske ega sündmuste tellimusi.
[XmlRoot("World")]
public class WorldDto
{
    [XmlAttribute("version")]
    public int Version { get; set; } = 1;

    [XmlArray("Groups"), XmlArrayItem("Group")]
    public List<GroupDto> Groups { get; set; } = new();

    [XmlArray("Animals"), XmlArrayItem("Animal")]
    public List<AnimalDto> Animals { get; set; } = new();
}

// Üks rühm (hoidla)
public class GroupDto
{
    [XmlAttribute("id")]
    public int Id { get; set; }

    public string Name { get; set; } = "";

    // Millist tüüpi loomad selles rühmas elavad, nt "Monkey"
    public string AnimalType { get; set; } = "";
}

// Üks loom
public class AnimalDto
{
    [XmlAttribute("id")]
    public int Id { get; set; }

    public string Type { get; set; } = "";
    public string Name { get; set; } = "";
    public int Energy { get; set; }
    public int Hunger { get; set; }
    public int Mood { get; set; }
    public int ReactionCount { get; set; }

    // Rühmakuuluvus: viide GroupDto.Id-le
    public int GroupId { get; set; }
}
