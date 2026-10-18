using System;
using System.Collections.Generic;
using System.Text;

namespace CrazyZoo.Core;

public class Elephant : Animal
{
    public Elephant(string name) : base(name, "Elevant") { }

    public override string MakeSound() => $"{Name}: Tuuuut!";

    public override string CrazyAction()
    {
        if (Energy < 20)
            return $"{Name} tahaks vett pritsida, aga on liiga väsinud.";

        ChangeEnergy(-20);
        ChangeHunger(15);
        return $"{Name} pritsis lonti abil kõigi peale vett! Energia: {Energy}, nälg: {Hunger}.";
    }

    protected override string ReactToCrazyAction(Animal actor)
    {
        ChangeMood(-3);
        return $"{Name} tõstab lonti ja trompetib, kui {actor.Name} hullab.";
    }
}