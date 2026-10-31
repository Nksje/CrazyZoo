namespace CrazyZoo.Core.Forecast;

// Ressursimahukas CPU-arvutus: hooldusplaanide võrdlus.
// Iga plaani jaoks simuleeritakse looma päeva väga palju kordi ja arvutatakse keskmine heaolu.
public static class ForecastCalculator
{
    // Kolm plaani: kui palju toitu loom saab ühe söögikorra ajal
    private static readonly string[] PlanNames = { "Vähe toitu", "Tavaline", "Palju toitu" };
    private static readonly int[] FoodAmounts = { 10, 25, 45 };

    public static string Calculate(ForecastJob job, int days, CancellationToken token)
    {
        AnimalSnapshot animal = job.Animal;

        // Sama sisend annab alati sama seemne, seega on tulemus mõlemas režiimis sama
        Random random = new Random(MakeSeed(animal, days));

        double[] averages = new double[PlanNames.Length];

        for (int plan = 0; plan < PlanNames.Length; plan++)
        {
            double scoreSum = 0;

            for (int day = 0; day < days; day++)
            {
                // Iga tsükkel kontrollib, kas kasutaja vajutas "Tühista"
                token.ThrowIfCancellationRequested();

                // Demo: meelega tekitatud viga keset arvutust
                if (job.MakeError && plan == 1 && day == days / 2)
                    throw new InvalidOperationException($"{animal.Name} põgenes simulatsiooni ajal aedikust!");

                scoreSum += SimulateOneDay(animal, FoodAmounts[plan], random);
            }

            averages[plan] = scoreSum / days;
        }

        // Leiame parima plaani
        int best = 0;
        for (int plan = 1; plan < averages.Length; plan++)
        {
            if (averages[plan] > averages[best])
                best = plan;
        }

        return $"Parim: {PlanNames[best]} ({averages[best]:F2}) | " +
               $"Vähe {averages[0]:F2}, Tavaline {averages[1]:F2}, Palju {averages[2]:F2}";
    }

    // Simuleerib ühe päeva (24 tundi) ja tagastab heaolu skoori 0..100
    private static double SimulateOneDay(AnimalSnapshot animal, int food, Random random)
    {
        int energy = animal.Energy;
        int hunger = animal.Hunger;
        int mood = animal.Mood;

        // Kui tihti loom hullab (% tunnis) ja mida see maksab – sama loogika nagu loomade CrazyAction'is
        int crazyChance = 20, energyCost = 10, hungerChange = 10;
        switch (animal.Species)
        {
            case "Ahv": crazyChance = 30; energyCost = 10; hungerChange = -20; break;
            case "Pingviin": crazyChance = 40; energyCost = 5; hungerChange = 5; break;
            case "Lõvi": crazyChance = 10; energyCost = 30; hungerChange = 20; break;
            case "Papagoi": crazyChance = 25; energyCost = 15; hungerChange = 10; break;
        }

        for (int hour = 0; hour < 24; hour++)
        {
            if (hour < 7)
            {
                // Öö: loom magab
                energy += 5;
                hunger += 1;
            }
            else
            {
                energy -= 1;
                hunger += 3;

                // Söögikorrad
                if (hour == 8 || hour == 13 || hour == 19)
                {
                    hunger -= food;
                    energy += food / 2;
                    mood += 2;
                    if (hunger < 10)
                    {
                        mood -= 8;   // söödi liiga palju
                        energy -= 5; // loom on loid
                    }
                }

                // Juhuslik CrazyAction (kui jõudu on)
                if (energy >= energyCost && random.Next(100) < crazyChance)
                {
                    energy -= energyCost;
                    hunger += hungerChange;
                    mood += 4;
                }

                if (hunger > 80) mood -= 3;
                if (energy < 15) mood -= 2;
            }

            energy = Math.Clamp(energy, 0, 100);
            hunger = Math.Clamp(hunger, 0, 100);
            mood = Math.Clamp(mood, 0, 100);
        }

        return (energy + mood + (100 - hunger)) / 3.0;
    }

    // Lihtne seeme looma andmetest: sama sisend → sama seeme
    private static int MakeSeed(AnimalSnapshot animal, int days)
    {
        int seed = days + animal.Energy * 7 + animal.Hunger * 13 + animal.Mood * 17;
        foreach (char c in animal.Name + animal.Species)
            seed = seed * 31 + c;
        return seed;
    }
}
