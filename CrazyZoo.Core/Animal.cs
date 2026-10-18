namespace CrazyZoo.Core;

public abstract class Animal
{
    public string Name { get; }
    public string Species { get; }

    public int Energy { get; private set; } = 50;
    public int Hunger { get; private set; } = 50;

    protected Animal(string name, string species)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Nimi ei tohi olla tühi.");
        if (name.Trim().Length > 20)
            throw new ArgumentException("Nimi on liiga pikk (max 20 märki).");

        Name = name.Trim();
        Species = species;
    }

    protected void ChangeEnergy(int delta) => Energy = Math.Clamp(Energy + delta, 0, 100);
    protected void ChangeHunger(int delta) => Hunger = Math.Clamp(Hunger + delta, 0, 100);

    public string Eat(int amount)
    {
        if (amount <= 0 || amount > 100)
            return $"{Name}: vigane toidukogus ({amount}). Sisesta number 1..100.";

        ChangeHunger(-amount);
        ChangeEnergy(amount / 2);
        return $"{Name} sõi toitu ({amount}). Nälg: {Hunger}, energia: {Energy}.";
    }

    public string Rest()
    {
        ChangeEnergy(20);
        ChangeHunger(5);
        return $"{Name} puhkas. Energia: {Energy}, nälg: {Hunger}.";
    }

    public virtual string MakeSound() => $"{Name} teeb mingit häält.";

    public abstract string CrazyAction();

    public override string ToString() => $"{Name} ({Species})";

    // Reaktsioonide olek (0..100), muutub ainult naabrite tegevustele reageerides
    public int Mood { get; private set; } = 50;
    public int ReactionCount { get; private set; }

    public event Action<string>? ReactionLogged;

    protected void ChangeMood(int delta) => Mood = Math.Clamp(Mood + delta, 0, 100);

    // Hoidla tellib need käsitlejad (Subscribe). Tegevuse põhjustaja ei reageeri iseendale.
    public void HandleNeighborAdded(object? sender, AnimalEventArgs e) => React(e.Source, ReactToNewcomer);
    public void HandleNeighborRemoved(object? sender, AnimalEventArgs e) => React(e.Source, ReactToLeaving);
    public void HandleNeighborCrazyAction(object? sender, AnimalEventArgs e) => React(e.Source, ReactToCrazyAction);

    private void React(Animal source, Func<Animal, string> reaction)
    {
        if (ReferenceEquals(source, this))
            return;

        string message = reaction(source);
        ReactionCount++;
        ReactionLogged?.Invoke(message);
    }

    // Alamklassid võivad reaktsiooni override abil muuta
    protected virtual string ReactToNewcomer(Animal newcomer)
    {
        ChangeMood(5);
        return $"{Name} tervitab uut looma {newcomer.Name}.";
    }

    protected virtual string ReactToLeaving(Animal leaver)
    {
        ChangeMood(-5);
        return $"{Name} jätab hüvasti loomaga {leaver.Name}.";
    }

    protected virtual string ReactToCrazyAction(Animal actor)
    {
        ChangeMood(3);
        return $"{Name} vaatab, kuidas {actor.Name} hullab.";
    }
}
