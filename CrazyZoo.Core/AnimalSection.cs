namespace CrazyZoo.Core;

// Mittegeneeriline vaade rühmale (hoidlale), et UI saaks kõiki rühmi ühtemoodi kasutada
public interface IAnimalSection
{
    int Id { get; }
    string Name { get; }        // rühma nimi, nt "Ahvid A"
    string TypeName { get; }    // loomatüüp UI jaoks, nt "Ahv"
    string TypeKey { get; }     // loomatüüp faili ja andmebaasi jaoks, nt "Monkey"
    string Title { get; }       // "Ahvid A (Ahv)"
    IReadOnlyList<Animal> Items { get; }

    Animal Create(string name);
    bool Remove(Animal animal);
    string CrazyAction(Animal animal);

    bool Accepts(Animal animal);                // kas loom sobib sellesse rühma
    void AddExisting(Animal animal);            // rühma vahetus: uued naabrid tervitavad
    Animal RestoreAnimal(int id, string name);  // laadimine: keegi ei tervita
}

// Üks rühm = üks tüübipõhine hoidla Repository<T>
public class AnimalSection<T> : IAnimalSection where T : Animal
{
    private readonly Repository<T> _repository = new();
    private readonly Func<int, string, T> _creator;
    private readonly Func<int> _nextId;

    public int Id { get; }
    public string Name { get; }
    public string TypeName { get; }
    public string TypeKey => typeof(T).Name;
    public string Title => $"{Name} ({TypeName})";

    public AnimalSection(int id, string name, string typeName,
        Func<int, string, T> creator, Func<int> nextId, Action<string> log)
    {
        Id = id;
        Name = name;
        TypeName = typeName;
        _creator = creator;
        _nextId = nextId;
        _repository.Logged += log;
    }

    public IReadOnlyList<Animal> Items => _repository.GetAll().Cast<Animal>().ToList();

    // Uus loom: ID annab maailm
    public Animal Create(string name)
    {
        T animal = _creator(_nextId(), name); // konstruktor valideerib nime
        _repository.Add(animal);
        return animal;
    }

    public bool Remove(Animal animal) => animal is T typed && _repository.Remove(typed);

    public string CrazyAction(Animal animal)
    {
        if (animal is not T typed)
            throw new ArgumentException("Loom ei kuulu sellesse hoidlasse.");
        return _repository.PerformCrazyAction(typed);
    }

    public bool Accepts(Animal animal) => animal is T;

    // Rühma vahetus: tavaline Add, seega uue rühma loomad tervitavad
    public void AddExisting(Animal animal)
    {
        if (animal is not T typed)
            throw new ArgumentException("Vale loomatüüp selle rühma jaoks.");
        _repository.Add(typed);
    }

    // Laadimine: loom saab salvestatud ID ja lisatakse ilma sündmusteta
    public Animal RestoreAnimal(int id, string name)
    {
        T animal = _creator(id, name);
        _repository.Restore(animal);
        return animal;
    }
}
