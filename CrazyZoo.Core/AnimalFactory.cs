namespace CrazyZoo.Core;

public static class AnimalFactory
{
    private static readonly Dictionary<string, Func<string, Animal>> _creators = new()
    {
        { "Ahv", name => new Monkey(name) },
        { "Pingviin", name => new Penguin(name) },
        { "Lõvi", name => new Lion(name) },
        {"Papagoi", name => new Parrot(name) }
    };

    public static IEnumerable<string> TypeNames => _creators.Keys;

    public static Animal Create(string typeName, string name)
    {
        if (!_creators.TryGetValue(typeName, out var creator))
            throw new ArgumentException("Tundmatu loomatüüp.");
        return creator(name);
    }
}