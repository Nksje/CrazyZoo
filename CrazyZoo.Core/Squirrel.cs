using System;
using System.Collections.Generic;
using System.Text;

namespace CrazyZoo.Core;

public class Squirrel : Animal, IClimb
{
    public Squirrel(string name) : base(name, "Orav") { }

    public override string MakeSound() => $"{Name}: Tsirr-tsirr!";

    public string Climb()
    {
        if (Energy < 5) return $"{Name} on liiga väsinud, et ronida.";
        ChangeEnergy(-5);
        return $"{Name} ronis kiiresti puu otsa. Energia: {Energy}.";
    }

    public override string CrazyAction()
    {
        if (Energy < 10)
            return $"{Name} tahaks pähkleid peita, aga on liiga väsinud.";

        ChangeEnergy(-10);
        ChangeHunger(-10);
        return $"{Name} peitis pähkleid kõikjale ja sõi mõned ära! Energia: {Energy}, nälg: {Hunger}.";
    }

    protected override string ReactToLeaving(Animal leaver)
    {
        ChangeMood(-8);
        return $"{Name} kaotab sõbra {leaver.Name} ja nutab vaikselt."; 
    }
}