namespace CrazyZoo.Core;

public class Monkey : Animal, IClimb, ISwimmable
{
    public Monkey(string name) : base(name, "Ahv") { }

    public override string MakeSound() => $"{Name}: Uh-uh-ah-ah!";

    public string Climb()
    {
        if (Energy < 10) return $"{Name} on liiga väsinud, et ronida.";
        ChangeEnergy(-10);
        return $"{Name} ronis puu otsa. Energia: {Energy}.";
    }

    public string Swim()
    {
        if (Energy < 15) return $"{Name} on liiga väsinud, et ujuda.";
        ChangeEnergy(-15);
        return $"{Name} ujus tiigis ja vihkab seda. Energia: {Energy}.";
    }

    public override string CrazyAction()
    {
        if (Energy < 10) return $"{Name} tahaks banaani varastada, aga pole jõudu.";
        ChangeHunger(-20);
        ChangeEnergy(-10);
        return $"{Name} varastas külastajalt banaani! Nälg: {Hunger}, energia: {Energy}.";
    }

    protected override string ReactToCrazyAction(Animal actor)
    {
        ChangeMood(10);
        return $"{Name} naerab ja plaksutab, kui {actor.Name} hullab!";
    }
}
