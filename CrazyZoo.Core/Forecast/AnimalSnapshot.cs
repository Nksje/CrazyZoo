namespace CrazyZoo.Core.Forecast;

// Looma andmete hetktõmmis (koopia). Arvutus kasutab ainult seda koopiat,
// mitte päris looma, seega ei sega taustalõimed UI-s olevaid loomi.
public record AnimalSnapshot(string Name, string Species, int Energy, int Hunger, int Mood)
{
    public override string ToString() => $"{Name} ({Species}) – energia {Energy}, nälg {Hunger}, meeleolu {Mood}";
}

// Üks arvutustöö: millise looma koopiaga arvutada ja kas tekitada meelega viga (demo jaoks)
public record ForecastJob(AnimalSnapshot Animal, bool MakeError);
