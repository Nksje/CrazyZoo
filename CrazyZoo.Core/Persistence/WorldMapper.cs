namespace CrazyZoo.Core.Persistence;

// Teisendab maailma DTO-ks ja tagasi. Kõik salvestusviisid kasutavad sama kontrolli.
public static class WorldMapper
{
    public const int CurrentVersion = 1;

    // Salvestamine: teeb maailmast andmete koopia
    public static WorldDto ToDto(ZooWorld world)
    {
        var dto = new WorldDto { Version = CurrentVersion };

        foreach (IAnimalSection section in world.Sections)
        {
            dto.Groups.Add(new GroupDto
            {
                Id = section.Id,
                Name = section.Name,
                AnimalType = section.TypeKey
            });

            foreach (Animal animal in section.Items)
            {
                dto.Animals.Add(new AnimalDto
                {
                    Id = animal.Id,
                    Type = section.TypeKey,
                    Name = animal.Name,
                    Energy = animal.Energy,
                    Hunger = animal.Hunger,
                    Mood = animal.Mood,
                    ReactionCount = animal.ReactionCount,
                    GroupId = section.Id
                });
            }
        }

        return dto;
    }

    // Laadimine: kontrollib andmeid ja loob UUE maailma.
    // Vea korral visatakse InvalidDataException ja senine maailm jääb puutumata.
    public static ZooWorld FromDto(WorldDto? dto)
    {
        if (dto == null)
            throw new InvalidDataException("Fail on tühi.");
        if (dto.Version != CurrentVersion)
            throw new InvalidDataException($"Toetamata failiversioon: {dto.Version}.");
        if (dto.Groups == null || dto.Animals == null)
            throw new InvalidDataException("Failist puuduvad rühmad või loomad.");

        var world = new ZooWorld();

        // 1) Rühmad: iga rühm saab õige tüübiga hoidla Repository<T>
        var groupsById = new Dictionary<int, IAnimalSection>();
        foreach (GroupDto group in dto.Groups)
        {
            if (group == null)
                throw new InvalidDataException("Failis on tühi rühm.");
            if (group.Id <= 0)
                throw new InvalidDataException($"Vigane rühma ID: {group.Id}.");
            if (string.IsNullOrWhiteSpace(group.Name))
                throw new InvalidDataException($"Rühmal {group.Id} puudub nimi.");
            if (groupsById.ContainsKey(group.Id))
                throw new InvalidDataException($"Rühma ID {group.Id} kordub.");

            // Tundmatu tüüp → InvalidDataException (AnimalTypes)
            groupsById[group.Id] = world.RestoreSection(group.Id, group.AnimalType, group.Name);
        }

        // 2) Loomad: õige tüüp, õige rühm, sama ID ja olek
        var animalIds = new HashSet<int>();
        foreach (AnimalDto data in dto.Animals)
        {
            if (data == null)
                throw new InvalidDataException("Failis on tühi loom.");
            if (!animalIds.Add(data.Id))
                throw new InvalidDataException($"Looma ID {data.Id} kordub.");
            if (!groupsById.TryGetValue(data.GroupId, out IAnimalSection? group))
                throw new InvalidDataException($"Loomal {data.Id} on tundmatu rühm {data.GroupId}.");
            if (data.Type != group.TypeKey)
                throw new InvalidDataException($"Loom {data.Id} ({data.Type}) ei sobi rühma {group.Name}.");

            CheckRange(data.Energy, "energia", data.Id);
            CheckRange(data.Hunger, "nälg", data.Id);
            CheckRange(data.Mood, "meeleolu", data.Id);
            if (data.ReactionCount < 0)
                throw new InvalidDataException($"Loom {data.Id}: reaktsioonide arv ei tohi olla negatiivne.");

            Animal animal;
            try
            {
                // Konstruktor kontrollib ID-d ja nime. Hoidla loob reaktsioonide tellimused.
                animal = group.RestoreAnimal(data.Id, data.Name);
            }
            catch (ArgumentException ex)
            {
                throw new InvalidDataException($"Loom {data.Id}: {ex.Message}");
            }

            animal.RestoreState(data.Energy, data.Hunger, data.Mood, data.ReactionCount);
            world.NoteAnimalId(data.Id);
        }

        return world;
    }

    private static void CheckRange(int value, string field, int animalId)
    {
        if (value < 0 || value > 100)
            throw new InvalidDataException($"Loom {animalId}: {field} peab olema 0..100 (oli {value}).");
    }
}
