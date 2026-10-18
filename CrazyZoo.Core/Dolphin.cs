using System;
using System.Collections.Generic;
using System.Text;

namespace CrazyZoo.Core;

public class Dolphin : Animal, ISwimmable
{
    public Dolphin(string name) : base(name, "Delfiin") { }

    public override string MakeSound() => $"{Name}: Klik-klik-kliiks!";

    public string Swim()
    {
        if (Energy < 5) return $"{Name} on liiga väsinud, et ujuda.";
        ChangeEnergy(-5);
        return $"{Name} ujub kiiresti ringi. Energia: {Energy}.";
    }

    public override string CrazyAction()
    {
        if (Energy < 15)
            return $"{Name} tahaks rõngast läbi hüpata, aga on liiga väsinud.";

        ChangeEnergy(-15);
        ChangeHunger(10);
        return $"{Name} hüppas läbi rõnga ja pritsis külastajaid! Energia: {Energy}, nälg: {Hunger}.";
    }

    protected override string ReactToNewcomer(Animal newcomer)
    {
        ChangeMood(7);
        return $"{Name} tervitab uut looma {newcomer.Name} vilistades.";
    }
}