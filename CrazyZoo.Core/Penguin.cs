namespace CrazyZoo.Core;

public class Penguin : Animal, ISwimmable, IFly
{
    public Penguin(string name) : base(name, "Pingviin") { }

    public override string MakeSound() => $"{Name}: Kraaa!";

    public string Swim()
    {
        if (Energy < 5) return $"{Name} on liiga väsinud, et ujuda.";
        ChangeEnergy(-5);
        return $"{Name} ujub nagu torpeedo. Energia: {Energy}.";
    }

    public string Fly()
    {
        ChangeEnergy(-10);
        return $"{Name} proovis lennata ja kukkus kõhuli. Energia: {Energy}.";
    }

    public override string CrazyAction()
    {
        ChangeEnergy(-5);
        ChangeHunger(5);
        return $"{Name} libistas end jääl kõhuli ja karjus rõõmust. Energia: {Energy}.";
    }
}