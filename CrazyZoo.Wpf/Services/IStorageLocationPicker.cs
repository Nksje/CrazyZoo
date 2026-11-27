using System.Windows;
using Microsoft.Win32;

namespace CrazyZoo.Wpf.Services;

// Kuhu salvestada ja kust laadida.
// Failide puhul küsib dialoog, andmebaasi puhul on maailma nimi fikseeritud.
// null tähendab, et kasutaja vajutas Cancel.
public interface IStorageLocationPicker
{
    string Description { get; }
    string? PickSaveLocation();
    string? PickLoadLocation();
}

// Failirežiim: SaveFileDialog ja OpenFileDialog
public class FileDialogLocationPicker : IStorageLocationPicker
{
    private const string Filter = "JSON-fail (*.json)|*.json|XML-fail (*.xml)|*.xml";

    public string Description => "JSON/XML fail";

    public string? PickSaveLocation()
    {
        var dialog = new SaveFileDialog
        {
            Filter = Filter,
            DefaultExt = ".json",
            AddExtension = true,
            OverwritePrompt = true,
            FileName = "crazyzoo"
        };
        return dialog.ShowDialog(Application.Current.MainWindow) == true ? dialog.FileName : null;
    }

    public string? PickLoadLocation()
    {
        var dialog = new OpenFileDialog
        {
            Filter = Filter,
            CheckFileExists = true
        };
        return dialog.ShowDialog(Application.Current.MainWindow) == true ? dialog.FileName : null;
    }
}

// Andmebaasirežiim: alati sama maailma nimi
public class FixedLocationPicker : IStorageLocationPicker
{
    private readonly string _worldName;

    public FixedLocationPicker(string worldName)
    {
        _worldName = worldName;
    }

    public string Description => $"LocalDB, maailm '{_worldName}'";
    public string? PickSaveLocation() => _worldName;
    public string? PickLoadLocation() => _worldName;
}
