namespace CrazyZoo.Core;

// Kogu loomaaed: iga loomatüüp saab oma hoidla
public class ZooWorld
{
    private readonly List<IAnimalSection> _sections = new();
    private int _nextAnimalId = 1;
    private int _nextSectionId = 1;

    public IReadOnlyList<IAnimalSection> Sections => _sections;
    public IEnumerable<Animal> AllAnimals => _sections.SelectMany(s => s.Items);
    public event Action<string>? Logged;

    // Tühi maailm (laadimine). Vaikemaailm tuleb CreateDefault'ist.
    public ZooWorld() { }

    public static ZooWorld CreateDefault()
    {
        var w = new ZooWorld();
        w.AddSection(nameof(Monkey), "Ahvid A").Create("Bobo");
        w.AddSection(nameof(Monkey), "Ahvid B");
        w.AddSection(nameof(Penguin), "Pingviinid").Create("Pingu");
        w.AddSection(nameof(Lion), "Lõvid").Create("Simba");
        w.AddSection(nameof(Parrot), "Papagoid").Create("Kesha");
        return w;
    }

    // Uus rühm: ID annab maailm ise
    public IAnimalSection AddSection(string typeKey, string name) => AddSectionCore(_nextSectionId++, typeKey, name);

    // Laadimine: rühm saab salvestatud ID
    internal IAnimalSection RestoreSection(int id, string typeKey, string name)
    {
        var s = AddSectionCore(id, typeKey, name);
        _nextSectionId = Math.Max(_nextSectionId, id + 1);
        return s;
    }

    // Laadimise järel peab uus loom saama vaba ID (suurim + 1)
    internal void NoteAnimalId(int id) => _nextAnimalId = Math.Max(_nextAnimalId, id + 1);

    private IAnimalSection AddSectionCore(int id, string typeKey, string name)
    {
        var s = AnimalTypes.CreateSection(typeKey, id, name, () => _nextAnimalId++, m => Logged?.Invoke(m));
        _sections.Add(s);
        return s;
    }

    // Rühma vahetus: vanad naabrid jätavad hüvasti, uued tervitavad
    public void Move(Animal animal, IAnimalSection from, IAnimalSection to)
    {
        if (ReferenceEquals(from, to)) return;
        if (!to.Accepts(animal)) throw new ArgumentException($"{animal.Name} ei sobi rühma {to.Name}.");
        if (!from.Remove(animal)) throw new ArgumentException("Loom pole lähterühmas.");
        to.AddExisting(animal);
    }
}