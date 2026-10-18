namespace CrazyZoo.Core;

// Mittegeneeriline vaade hoidlale, et UI saaks kõiki hoidlaid ühtemoodi kasutada
public interface IAnimalSection
{
    string TypeName { get; }
    IReadOnlyList<Animal> Items { get; }
    Animal Create(string name);
    bool Remove(Animal animal);
    string CrazyAction(Animal animal);
}

// Seob ühe tüübipõhise hoidla ja selle tüübi loomise reegli
public class AnimalSection<T> : IAnimalSection where T : Animal
{
    private readonly Repository<T> _repository = new();
    private readonly Func<string, T> _creator;

    public string TypeName { get; }

    public AnimalSection(string typeName, Func<string, T> creator, Action<string> log)
    {
        TypeName = typeName;
        _creator = creator;
        _repository.Logged += log;
    }

    public IReadOnlyList<Animal> Items => _repository.GetAll().Cast<Animal>().ToList();

    public Animal Create(string name)
    {
        T animal = _creator(name); // konstruktor valideerib nime
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
}
