namespace CrazyZoo.Core;

// Kogu loomaaed: iga loomatüüp saab oma hoidla
public class ZooWorld
{
    private readonly List<IAnimalSection> _sections = new();

    public IReadOnlyList<IAnimalSection> Sections => _sections;

    // Kõigi hoidlate loomade reaktsioonide logi
    public event Action<string>? Logged;

    public ZooWorld()
    {
        // Uue tüübi lisamiseks piisab ühest Register-reast
        Register("Ahv", name => new Monkey(name)).Create("Bobo");
        Register("Pingviin", name => new Penguin(name)).Create("Pingu");
        Register("Lõvi", name => new Lion(name)).Create("Simba");
        Register("Papagoi", name => new Parrot(name)).Create("Kesha");
        Register("Delfiin", name => new Dolphin(name)).Create("Flipper");
        Register("Elevant", name => new Elephant(name)).Create("Dumbo");
        Register("Orav", name => new Squirrel(name)).Create("Chip");
    }

    public AnimalSection<T> Register<T>(string typeName, Func<string, T> creator) where T : Animal
    {
        var section = new AnimalSection<T>(typeName, creator, message => Logged?.Invoke(message));
        _sections.Add(section);
        return section;
    }

    // Kõigi hoidlate lugemisvaade LINQ-päringute jaoks
    public IEnumerable<Animal> AllAnimals => _sections.SelectMany(s => s.Items);
}
