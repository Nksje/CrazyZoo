namespace CrazyZoo.Core;

public class Lion : Animal
{
    public Lion(string name) : base(name, "Lõvi") { }

    public override string MakeSound() => $"{Name}: ROOOAAAR!";

    public override string CrazyAction()
    {
        if (Energy < 30)
            return $"{Name} on liiga väsinud, et hullata. Lase tal puhata.";

        ChangeEnergy(-30);
        ChangeHunger(20);
        return $"{Name} ajas taga omaenda saba! Energia: {Energy}, nälg: {Hunger}.";
    }

    protected override string ReactToCrazyAction(Animal actor)
    {
        ChangeMood(-2);
        ChangeEnergy(-2);
        return $"{Name} urisab vaikselt, kui {actor.Name} hullab. Energia: {Energy}.";
    }
}
