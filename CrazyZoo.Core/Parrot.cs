using System;
using System.Collections.Generic;
using System.Text;

namespace CrazyZoo.Core;

public class Parrot : Animal, IFly
{
    public Parrot(string name) : base(name, "Papagoi") { }

    public override string MakeSound() => $"{Name}: Kurr-kurr! Tere, tere!";

    public string Fly()
    {
        if (Energy < 10) return $"{Name} on liiga väsinud, et lennata.";
        ChangeEnergy(-10);
        return $"{Name} lendas ringi üle loomaaia. Energia: {Energy}.";
    }

    public override string CrazyAction()
    {
        if (Energy < 15)
            return $"{Name} tahaks külastajaid imiteerida, aga on liiga väsinud.";

        ChangeEnergy(-15);
        ChangeHunger(10);
        return $"{Name} imiteeris valvuri häält ja käskis kõigil koju minna! Energia: {Energy}, nälg: {Hunger}.";
    }
}