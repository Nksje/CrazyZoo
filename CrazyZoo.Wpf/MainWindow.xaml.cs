using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using CrazyZoo.Core;

namespace CrazyZoo.Wpf;

public partial class MainWindow : Window
{
    private readonly ObservableCollection<Animal> _animals = new();

    public MainWindow()
    {
        InitializeComponent();
        AnimalList.ItemsSource = _animals;
        TypeBox.ItemsSource = AnimalFactory.TypeNames;
        TypeBox.SelectedIndex = 0;

        
        _animals.Add(new Monkey("Bobo"));
        _animals.Add(new Penguin("Pingu"));
        _animals.Add(new Lion("Simba"));
    }

    private Animal? Selected => AnimalList.SelectedItem as Animal;

    private void Log(string message) => LogList.Items.Insert(0, message);

    private void UpdateDetails()
    {
        if (Selected is Animal a)
        {
            DetailsText.Text = $"{a.Name} ({a.Species})";
            EnergyBar.Value = a.Energy;
            HungerBar.Value = a.Hunger;
        }
        else
        {
            DetailsText.Text = "Vali loom listist";
            EnergyBar.Value = 0;
            HungerBar.Value = 0;
        }
    }

    private bool HasSelection()
    {
        if (Selected == null)
        {
            Log("Vali kõigepealt loom!");
            return false;
        }
        return true;
    }

    private void AnimalList_SelectionChanged(object sender, SelectionChangedEventArgs e) => UpdateDetails();

    private void AddButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            string type = TypeBox.SelectedItem?.ToString() ?? "";
            Animal animal = AnimalFactory.Create(type, NameBox.Text);
            _animals.Add(animal);
            Log($"Lisati: {animal}");
            NameBox.Clear();
        }
        catch (ArgumentException ex)
        {
            Log("Viga: " + ex.Message);
        }
    }

    private void RemoveButton_Click(object sender, RoutedEventArgs e)
    {
        if (!HasSelection()) return;
        Log($"Eemaldati: {Selected}");
        _animals.Remove(Selected!);
        UpdateDetails();
    }

    private void EatButton_Click(object sender, RoutedEventArgs e)
    {
        if (!HasSelection()) return;
        if (!int.TryParse(FoodBox.Text, out int amount))
        {
            Log("Viga: toidukogus peab olema täisarv.");
            return;
        }
        Log(Selected!.Eat(amount));
        UpdateDetails();
    }

    private void RestButton_Click(object sender, RoutedEventArgs e)
    {
        if (!HasSelection()) return;
        Log(Selected!.Rest());
        UpdateDetails();
    }

    private void SoundButton_Click(object sender, RoutedEventArgs e)
    {
        if (!HasSelection()) return;
        Log(Selected!.MakeSound());
    }

    private void CrazyButton_Click(object sender, RoutedEventArgs e)
    {
        if (!HasSelection()) return;
        Log(Selected!.CrazyAction());
        UpdateDetails();
    }

    private void SwimButton_Click(object sender, RoutedEventArgs e)
    {
        if (!HasSelection()) return;
        if (Selected is ISwimmable swimmer) Log(swimmer.Swim());
        else Log($"{Selected!.Name} ei oska ujuda.");
        UpdateDetails();
    }

    private void ClimbButton_Click(object sender, RoutedEventArgs e)
    {
        if (!HasSelection()) return;
        if (Selected is IClimb climber) Log(climber.Climb());
        else Log($"{Selected!.Name} ei oska ronida.");
        UpdateDetails();
    }

    private void FlyButton_Click(object sender, RoutedEventArgs e)
    {
        if (!HasSelection()) return;
        IFly? flyer = Selected as IFly;
        if (flyer != null) Log(flyer.Fly());
        else Log($"{Selected!.Name} ei oska lennata.");
        UpdateDetails();
    }
}