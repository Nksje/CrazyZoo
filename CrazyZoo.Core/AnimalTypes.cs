namespace CrazyZoo.Core;

// Ainus koht, kus tüübi võti ("Monkey") seotakse klassiga ja tema hoidlaga Repository<T>
public static class AnimalTypes
{
    public static IReadOnlyList<string> Keys { get; } =
        new[] { nameof(Monkey), nameof(Penguin), nameof(Lion), nameof(Parrot),
                nameof(Dolphin), nameof(Elephant), nameof(Squirrel) };

    internal static IAnimalSection CreateSection(string typeKey, int id, string name,
        Func<int> nextId, Action<string> log) => typeKey switch
        {
            nameof(Monkey) => new AnimalSection<Monkey>(id, name, "Ahv", (i, n) => new Monkey(i, n), nextId, log),
            nameof(Penguin) => new AnimalSection<Penguin>(id, name, "Pingviin", (i, n) => new Penguin(i, n), nextId, log),
            nameof(Lion) => new AnimalSection<Lion>(id, name, "Lõvi", (i, n) => new Lion(i, n), nextId, log),
            nameof(Parrot) => new AnimalSection<Parrot>(id, name, "Papagoi", (i, n) => new Parrot(i, n), nextId, log),
            nameof(Dolphin) => new AnimalSection<Dolphin>(id, name, "Delfiin", (i, n) => new Dolphin(i, n), nextId, log),
            nameof(Elephant) => new AnimalSection<Elephant>(id, name, "Elevant", (i, n) => new Elephant(i, n), nextId, log),
            nameof(Squirrel) => new AnimalSection<Squirrel>(id, name, "Orav", (i, n) => new Squirrel(i, n), nextId, log),
            _ => throw new InvalidDataException($"Tundmatu loomatüüp: '{typeKey}'.")
        };
}