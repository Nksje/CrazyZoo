namespace CrazyZoo.Core;

public record SpeciesCount(string Species, int Count);

public static class ZooQueries
{
    // Otsimine (Where): nime või liigi järgi
    public static List<Animal> Search(IEnumerable<Animal> animals, string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return animals.ToList();

        string t = text.Trim();
        return animals
            .Where(a => a.Name.Contains(t, StringComparison.OrdinalIgnoreCase)
                     || a.Species.Contains(t, StringComparison.OrdinalIgnoreCase))
            .ToList();
    }

    // Sorteerimine (OrderByDescending): kõige energilisemad esimesena
    public static List<Animal> SortByEnergy(IEnumerable<Animal> animals)
    {
        return animals
            .OrderByDescending(a => a.Energy)
            .ThenBy(a => a.Name)
            .ToList();
    }

    // Grupeerimine (GroupBy): loomade arv igas liigis
    public static List<SpeciesCount> CountBySpecies(IEnumerable<Animal> animals)
    {
        return animals
            .GroupBy(a => a.Species)
            .Select(g => new SpeciesCount(g.Key, g.Count()))
            .OrderBy(x => x.Species)
            .ToList();
    }
}
