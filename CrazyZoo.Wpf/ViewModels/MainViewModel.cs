using System.Collections.ObjectModel;
using System.IO;
using System.Windows.Input;
using CrazyZoo.Core;
using CrazyZoo.Core.Persistence;
using CrazyZoo.Wpf.Services;
using Microsoft.Data.SqlClient;

namespace CrazyZoo.Wpf.ViewModels;

public class MainViewModel : ViewModelBase
{
    // Vol.4: salvestusteenused tulevad DI kaudu konstruktorist
    private readonly IWorldStorage _storage;
    private readonly IStorageLocationPicker _locationPicker;

    // Maailm pole enam readonly: laadimisel asendatakse see uuega
    private ZooWorld _world = ZooWorld.CreateDefault();

    private IAnimalSection? _selectedSection;
    private Animal? _selectedAnimal;
    private IAnimalSection? _selectedMoveTarget;
    private string _newName = string.Empty;
    private string _foodAmountText = "20";
    private string _searchText = string.Empty;
    private bool _isBusy;

    // Rühmad (hoidlad) ja valitud rühma loomad
    public IReadOnlyList<IAnimalSection> Sections => _world.Sections;
    public ObservableCollection<Animal> Animals { get; } = new();

    // Rühmad, kuhu valitud looma saab viia (sama tüüp, teine rühm)
    public ObservableCollection<IAnimalSection> MoveTargets { get; } = new();

    // Logi ja LINQ-päringute tulemused
    public ObservableCollection<string> LogEntries { get; } = new();
    public ObservableCollection<Animal> SearchResults { get; } = new();
    public ObservableCollection<Animal> SortedAnimals { get; } = new();
    public ObservableCollection<SpeciesCount> SpeciesSummary { get; } = new();

    // Käsud
    public RelayCommand AddCommand { get; }
    public RelayCommand RemoveCommand { get; }
    public RelayCommand EatCommand { get; }
    public RelayCommand RestCommand { get; }
    public RelayCommand SoundCommand { get; }
    public RelayCommand CrazyActionCommand { get; }
    public RelayCommand SwimCommand { get; }
    public RelayCommand ClimbCommand { get; }
    public RelayCommand FlyCommand { get; }

    // Vol.4: rühma vahetus, salvestamine ja laadimine
    public RelayCommand MoveCommand { get; }
    public RelayCommand SaveCommand { get; }
    public RelayCommand LoadCommand { get; }

    // Vol.3: prognooside vahekaart
    public ForecastViewModel Forecast { get; }

    public MainViewModel(IWorldStorage storage, IStorageLocationPicker locationPicker)
    {
        _storage = storage;
        _locationPicker = locationPicker;

        _world.Logged += AddLog;
        Forecast = new ForecastViewModel(_world);

        AddCommand = new RelayCommand(AddAnimal, () => SelectedSection != null && !string.IsNullOrWhiteSpace(NewName));
        RemoveCommand = new RelayCommand(RemoveAnimal, HasAnimal);
        EatCommand = new RelayCommand(Eat, () => HasAnimal() && int.TryParse(FoodAmountText, out _));
        RestCommand = new RelayCommand(Rest, HasAnimal);
        SoundCommand = new RelayCommand(MakeSound, HasAnimal);
        CrazyActionCommand = new RelayCommand(CrazyAction, HasAnimal);
        SwimCommand = new RelayCommand(Swim, HasAnimal);
        ClimbCommand = new RelayCommand(Climb, HasAnimal);
        FlyCommand = new RelayCommand(Fly, HasAnimal);

        MoveCommand = new RelayCommand(MoveAnimal, () => HasAnimal() && SelectedMoveTarget != null);
        SaveCommand = new RelayCommand(async () => await SaveWorldAsync(), () => !IsBusy);
        LoadCommand = new RelayCommand(async () => await LoadWorldAsync(), () => !IsBusy && !Forecast.IsRunning);

        SelectedSection = Sections.FirstOrDefault();
        RefreshQueries();
    }

    public IAnimalSection? SelectedSection
    {
        get => _selectedSection;
        set
        {
            if (SetField(ref _selectedSection, value))
            {
                RefreshAnimals();
                SelectedAnimal = null;
            }
        }
    }

    public Animal? SelectedAnimal
    {
        get => _selectedAnimal;
        set
        {
            if (SetField(ref _selectedAnimal, value))
            {
                RefreshDetails();
                RefreshMoveTargets();
            }
        }
    }

    public IAnimalSection? SelectedMoveTarget
    {
        get => _selectedMoveTarget;
        set => SetField(ref _selectedMoveTarget, value);
    }

    public string NewName
    {
        get => _newName;
        set => SetField(ref _newName, value);
    }

    public string FoodAmountText
    {
        get => _foodAmountText;
        set => SetField(ref _foodAmountText, value);
    }

    public string SearchText
    {
        get => _searchText;
        set
        {
            if (SetField(ref _searchText, value))
                RefreshQueries();
        }
    }

    // Salvestamise/laadimise ajal on nupud keelatud
    public bool IsBusy
    {
        get => _isBusy;
        private set
        {
            if (SetField(ref _isBusy, value))
                CommandManager.InvalidateRequerySuggested();
        }
    }

    // Näitab, millise salvestusteostuse DI valis
    public string StorageInfo => $"Teostus (DI): {_storage.GetType().Name} – {_locationPicker.Description}";

    // Valitud looma andmed (UI uuendub PropertyChanged kaudu)
    public string DetailsTitle => SelectedAnimal is null ? "Vali loom listist" : $"#{SelectedAnimal.Id} {SelectedAnimal}";
    public int SelectedEnergy => SelectedAnimal?.Energy ?? 0;
    public int SelectedHunger => SelectedAnimal?.Hunger ?? 0;
    public int SelectedMood => SelectedAnimal?.Mood ?? 0;
    public int SelectedReactionCount => SelectedAnimal?.ReactionCount ?? 0;

    private bool HasAnimal() => SelectedAnimal != null;

    private void AddLog(string message) => LogEntries.Insert(0, message);

    private void RefreshAnimals()
    {
        Animals.Clear();
        if (SelectedSection == null)
            return;

        foreach (Animal animal in SelectedSection.Items)
            Animals.Add(animal);
    }

    private void RefreshDetails()
    {
        OnPropertyChanged(nameof(DetailsTitle));
        OnPropertyChanged(nameof(SelectedEnergy));
        OnPropertyChanged(nameof(SelectedHunger));
        OnPropertyChanged(nameof(SelectedMood));
        OnPropertyChanged(nameof(SelectedReactionCount));
    }

    // Sama tüüpi teised rühmad, kuhu valitud looma saab viia
    private void RefreshMoveTargets()
    {
        MoveTargets.Clear();
        SelectedMoveTarget = null;
        if (SelectedAnimal == null)
            return;

        foreach (IAnimalSection section in Sections)
        {
            if (section != SelectedSection && section.Accepts(SelectedAnimal))
                MoveTargets.Add(section);
        }
        SelectedMoveTarget = MoveTargets.FirstOrDefault();
    }

    // LINQ-päringud kõigi hoidlate loomade peal
    private void RefreshQueries()
    {
        Replace(SearchResults, ZooQueries.Search(_world.AllAnimals, SearchText));
        Replace(SortedAnimals, ZooQueries.SortByEnergy(_world.AllAnimals));
        Replace(SpeciesSummary, ZooQueries.CountBySpecies(_world.AllAnimals));
    }

    private void RefreshAll()
    {
        RefreshDetails();
        RefreshQueries();
    }

    private static void Replace<T>(ObservableCollection<T> target, IEnumerable<T> items)
    {
        target.Clear();
        foreach (T item in items)
            target.Add(item);
    }

    private void AddAnimal()
    {
        if (SelectedSection == null)
            return;

        try
        {
            Animal animal = SelectedSection.Create(NewName);
            Animals.Add(animal);
            AddLog($"Lisati: #{animal.Id} {animal}");
            NewName = string.Empty;
            SelectedAnimal = animal;
            RefreshAll();
        }
        catch (ArgumentException ex)
        {
            AddLog("Viga: " + ex.Message); // vigane sisend ei muuda midagi
        }
    }

    private void RemoveAnimal()
    {
        if (SelectedSection == null || SelectedAnimal == null)
            return;

        Animal animal = SelectedAnimal;
        if (SelectedSection.Remove(animal))
        {
            Animals.Remove(animal);
            AddLog($"Eemaldati: #{animal.Id} {animal}");
            SelectedAnimal = null;
            RefreshAll();
        }
    }

    // Vol.4: rühma vahetus. Vana rühm jätab hüvasti, uus rühm tervitab.
    private void MoveAnimal()
    {
        if (SelectedSection == null || SelectedAnimal == null || SelectedMoveTarget == null)
            return;

        Animal animal = SelectedAnimal;
        IAnimalSection target = SelectedMoveTarget;
        try
        {
            _world.Move(animal, SelectedSection, target);
            AddLog($"{animal.Name} viidi rühma {target.Title}.");
            Animals.Remove(animal);
            SelectedAnimal = null;
            RefreshAll();
        }
        catch (ArgumentException ex)
        {
            AddLog("Viga: " + ex.Message);
        }
    }

    private void Eat()
    {
        if (SelectedAnimal == null)
            return;

        if (!int.TryParse(FoodAmountText, out int amount))
        {
            AddLog("Viga: toidukogus peab olema täisarv.");
            return;
        }

        AddLog(SelectedAnimal.Eat(amount));
        RefreshAll();
    }

    private void Rest()
    {
        if (SelectedAnimal == null)
            return;

        AddLog(SelectedAnimal.Rest());
        RefreshAll();
    }

    private void MakeSound()
    {
        if (SelectedAnimal == null)
            return;

        AddLog(SelectedAnimal.MakeSound()); // polümorfism
    }

    private void CrazyAction()
    {
        if (SelectedSection == null || SelectedAnimal == null)
            return;

        // Hoidla käivitab tegevuse ja teised sama hoidla loomad reageerivad
        AddLog(SelectedSection.CrazyAction(SelectedAnimal));
        RefreshAll();
    }

    // Võimete kontroll operaatoritega is / as
    private void Swim()
    {
        if (SelectedAnimal == null)
            return;

        if (SelectedAnimal is ISwimmable swimmer) AddLog(swimmer.Swim());
        else AddLog($"{SelectedAnimal.Name} ei oska ujuda.");
        RefreshAll();
    }

    private void Climb()
    {
        if (SelectedAnimal == null)
            return;

        if (SelectedAnimal is IClimb climber) AddLog(climber.Climb());
        else AddLog($"{SelectedAnimal.Name} ei oska ronida.");
        RefreshAll();
    }

    private void Fly()
    {
        if (SelectedAnimal == null)
            return;

        IFly? flyer = SelectedAnimal as IFly;
        if (flyer != null) AddLog(flyer.Fly());
        else AddLog($"{SelectedAnimal.Name} ei oska lennata.");
        RefreshAll();
    }

    // ===== Vol.4: salvestamine ja laadimine =====

    private async Task SaveWorldAsync()
    {
        string? location = _locationPicker.PickSaveLocation();
        if (location == null)
            return; // Cancel → midagi ei juhtu

        IsBusy = true;
        try
        {
            WorldDto dto = WorldMapper.ToDto(_world); // andmete koopia, mitte aknad ega käsud
            await _storage.SaveAsync(dto, location);
            AddLog($"Maailm salvestati: {location} ({dto.Animals.Count} looma, {dto.Groups.Count} rühma)");
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException
                                      or InvalidDataException or SqlException or InvalidOperationException)
        {
            AddLog("Salvestamine ebaõnnestus: " + ex.Message);
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task LoadWorldAsync()
    {
        string? location = _locationPicker.PickLoadLocation();
        if (location == null)
            return;

        IsBusy = true;
        try
        {
            WorldDto dto = await _storage.LoadAsync(location);
            ZooWorld loaded = WorldMapper.FromDto(dto); // kontroll; vea korral erind
            ReplaceWorld(loaded);                       // alles nüüd asendame nähtava maailma
            AddLog($"Maailm laaditi: {location} ({loaded.AllAnimals.Count()} looma)");
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException
                                      or InvalidDataException or SqlException or InvalidOperationException)
        {
            AddLog("Laadimine ebaõnnestus, senine maailm jäi alles: " + ex.Message);
        }
        finally
        {
            IsBusy = false;
        }
    }

    // Asendab maailma ja teavitab UI-d kõigist muutunud andmetest
    private void ReplaceWorld(ZooWorld newWorld)
    {
        _world.Logged -= AddLog;
        _world = newWorld;
        _world.Logged += AddLog;

        OnPropertyChanged(nameof(Sections));
        SelectedSection = Sections.FirstOrDefault();
        SelectedAnimal = null;
        RefreshAnimals();
        RefreshMoveTargets();
        RefreshAll();

        Forecast.SetWorld(_world);
    }
}
