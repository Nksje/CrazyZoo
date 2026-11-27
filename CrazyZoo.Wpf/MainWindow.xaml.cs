using System.Windows;
using CrazyZoo.Wpf.ViewModels;

namespace CrazyZoo.Wpf;

// Kooditaguses pole äriloogikat: kogu loogika on Core-projektis ja MainViewModel'is.
// ViewModeli annab DI-konteiner (App.xaml.cs).
public partial class MainWindow : Window
{
    public MainWindow(MainViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}
