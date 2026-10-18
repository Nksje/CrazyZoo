using System.Collections.ObjectModel;
using CrazyZoo.Core;

namespace CrazyZoo.Wpf.ViewModels;

public class MainViewModel : ViewModelBase
{
    private readonly ZooWorld _world = new();

    private IAnimalSection? _selectedSection;
    private Animal? _selectedAnimal;
    private string _newName = string.Empty;
    private string _foodAmountText = "20";
    private string _searchText = string.Empty;

    // Hoidlad ja valitud hoidla loomad
    public IReadOnlyList<IAnimalSection> Sections => _world.Sections;
    public ObservableCollection<Animal> Animals { get; } = new();

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

    public MainViewModel()
    {
        _world.Logged += AddLog;

        AddCommand = new RelayCommand(AddAnimal, () => SelectedSection != null && !string.IsNullOrWhiteSpace(NewName));
        RemoveCommand = new RelayCommand(RemoveAnimal, HasAnimal);
        EatCommand = new RelayCommand(Eat, () => HasAnimal() && int.TryParse(FoodAmountText, out _));
        RestCommand = new RelayCommand(Rest, HasAnimal);
        SoundCommand = new RelayCommand(MakeSound, HasAnimal);
        CrazyActionCommand = new RelayCommand(CrazyAction, HasAnimal);
        SwimCommand = new RelayCommand(Swim, HasAnimal);
        ClimbCommand = new RelayCommand(Climb, HasAnimal);
        FlyCommand = new RelayCommand(Fly, HasAnimal);

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
                RefreshDetails();
        }
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

    // Valitud looma andmed (UI uuendub PropertyChanged kaudu)
    public string DetailsTitle => SelectedAnimal is null ? "Vali loom listist" : SelectedAnimal.ToString();
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
            AddLog($"Lisati: {animal}");
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
            AddLog($"Eemaldati: {animal}");
            SelectedAnimal = null;
            RefreshAll();
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
}
