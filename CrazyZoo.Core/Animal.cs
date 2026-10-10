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

}