using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows.Input;
using System.Windows.Threading;
using CrazyZoo.Core;
using CrazyZoo.Core.Forecast;

namespace CrazyZoo.Wpf.ViewModels;

// Üks rida loomade valikus: hetktõmmis + kasutaja linnukesed
public class AnimalChoice
{
    public AnimalSnapshot Snapshot { get; }
    public bool IsSelected { get; set; } = true;
    public bool MakeError { get; set; }

    public AnimalChoice(AnimalSnapshot snapshot)
    {
        Snapshot = snapshot;
    }
}

// Vol.3: hooldusplaanide prognoos järjestikku ja paralleelselt
public class ForecastViewModel : ViewModelBase
{
    private readonly ZooWorld _world;
    private CancellationTokenSource? _cts;
    private readonly DispatcherTimer _timer = new() { Interval = TimeSpan.FromMilliseconds(100) };
    private readonly Stopwatch _runStopwatch = new();

    private int _selectedDays = 3_000_000;
    private bool _isRunning;
    private string _snapshotText = string.Empty;
    private string _statusText = "Vali loomad ja töömaht ning käivita arvutus.";
    private string _elapsedText = string.Empty;
    private string _totalText = string.Empty;
    private string _comparisonText = "Käivita mõlemad režiimid, et neid võrrelda.";

    // Viimaste käivituste tulemused võrdlemiseks
    private List<JobReport>? _lastSequential;
    private TimeSpan _lastSequentialTime;
    private List<JobReport>? _lastParallel;
    private TimeSpan _lastParallelTime;

    public ObservableCollection<AnimalChoice> Choices { get; } = new();
    public ObservableCollection<JobReport> Report { get; } = new();

    // Töömaht: mitu päeva simuleeritakse iga plaani kohta
    public List<int> WorkloadOptions { get; } = new() { 1_000_000, 3_000_000, 6_000_000, 12_000_000 };

    public RelayCommand SnapshotCommand { get; }
    public RelayCommand RunSequentialCommand { get; }
    public RelayCommand RunParallelCommand { get; }
    public RelayCommand CancelCommand { get; }

    public ForecastViewModel(ZooWorld world)
    {
        _world = world;

        SnapshotCommand = new RelayCommand(TakeSnapshot, () => !IsRunning);
        RunSequentialCommand = new RelayCommand(async () => await RunAsync(false), () => !IsRunning);
        RunParallelCommand = new RelayCommand(async () => await RunAsync(true), () => !IsRunning);
        CancelCommand = new RelayCommand(Cancel, () => IsRunning);

        // Taimer näitab, et UI elab arvutuse ajal edasi
        _timer.Tick += (s, e) => ElapsedText = $"Möödunud aeg: {_runStopwatch.Elapsed.TotalSeconds:F1} s";

        TakeSnapshot();
    }

    public int SelectedDays
    {
        get => _selectedDays;
        set
        {
            if (SetField(ref _selectedDays, value))
                ClearComparison(); // töömaht muutus → vanad tulemused pole enam võrreldavad
        }
    }

    public bool IsRunning
    {
        get => _isRunning;
        private set
        {
            SetField(ref _isRunning, value);
            CommandManager.InvalidateRequerySuggested(); // nuppude lubamine/keelamine uuesti
        }
    }

    public string SnapshotText { get => _snapshotText; private set => SetField(ref _snapshotText, value); }
    public string StatusText { get => _statusText; private set => SetField(ref _statusText, value); }
    public string ElapsedText { get => _elapsedText; private set => SetField(ref _elapsedText, value); }
    public string TotalText { get => _totalText; private set => SetField(ref _totalText, value); }
    public string ComparisonText { get => _comparisonText; private set => SetField(ref _comparisonText, value); }

    // Võtab loomaaia loomadest uue koopia. Kuni uue hetktõmmiseni kasutavad mõlemad režiimid sama sisendit.
    private void TakeSnapshot()
    {
        Choices.Clear();
        foreach (Animal animal in _world.AllAnimals)
            Choices.Add(new AnimalChoice(animal.CreateSnapshot()));

        SnapshotText = $"Hetktõmmis tehtud kell {DateTime.Now:HH:mm:ss} ({Choices.Count} looma)";
        ClearComparison();
    }

    private void ClearComparison()
    {
        _lastSequential = null;
        _lastParallel = null;
        ComparisonText = "Käivita mõlemad režiimid, et neid võrrelda.";
    }

    private async Task RunAsync(bool parallel)
    {
        // Kogume valitud loomad
        List<ForecastJob> jobs = new List<ForecastJob>();
        foreach (AnimalChoice choice in Choices)
        {
            if (choice.IsSelected)
                jobs.Add(new ForecastJob(choice.Snapshot, choice.MakeError));
        }

        if (jobs.Count == 0)
        {
            StatusText = "Vali vähemalt üks loom.";
            return;
        }

        string modeName = parallel ? "Paralleelne" : "Järjestikune";
        Report.Clear();
        TotalText = string.Empty;
        StatusText = $"{modeName} arvutus käib ({jobs.Count} looma, {SelectedDays:N0} päeva)...";

        // Iga lõpetatud loom ilmub kohe raportisse (Progress viib andmed UI-lõime)
        Progress<JobReport> progress = new Progress<JobReport>(report => Report.Add(report));

        _cts = new CancellationTokenSource();
        IsRunning = true;
        _runStopwatch.Restart();
        _timer.Start();

        List<JobReport> results;
        try
        {
            if (parallel)
                results = await ForecastRunner.RunParallelAsync(jobs, SelectedDays, _cts.Token, progress);
            else
                results = await ForecastRunner.RunSequentialAsync(jobs, SelectedDays, _cts.Token, progress);
        }
        finally
        {
            _runStopwatch.Stop();
            _timer.Stop();
            _cts.Dispose();
            _cts = null;
            IsRunning = false;
        }

        TimeSpan total = _runStopwatch.Elapsed;

        // Lõplik raport samas järjekorras, nagu loomad valiti
        Report.Clear();
        foreach (JobReport report in results)
            Report.Add(report);

        int ok = results.Count(r => r.Status == JobStatus.Õnnestus);
        int errors = results.Count(r => r.Status == JobStatus.Viga);
        int canceled = results.Count(r => r.Status == JobStatus.Tühistatud);

        TotalText = $"{modeName}: kogu töö aeg {total.TotalSeconds:F2} s";
        StatusText = $"Valmis. Õnnestus: {ok}, viga: {errors}, tühistatud: {canceled}.";
        ElapsedText = string.Empty;

        if (parallel)
        {
            _lastParallel = results;
            _lastParallelTime = total;
        }
        else
        {
            _lastSequential = results;
            _lastSequentialTime = total;
        }

        UpdateComparison();
    }

    private void Cancel()
    {
        _cts?.Cancel(); // sama token jõuab kõigi arvutusteni
        StatusText = "Tühistan...";
    }

    private void UpdateComparison()
    {
        if (_lastSequential == null || _lastParallel == null)
        {
            ComparisonText = "Käivita ka teine režiim, et neid võrrelda.";
            return;
        }

        if (_lastSequential.Count != _lastParallel.Count)
        {
            ComparisonText = "Režiimides valiti erinev arv loomi – sisend pole sama.";
            return;
        }

        // Võrdleme ainult neid loomi, kes õnnestusid mõlemas režiimis
        int compared = 0;
        int same = 0;
        for (int i = 0; i < _lastSequential.Count; i++)
        {
            JobReport a = _lastSequential[i];
            JobReport b = _lastParallel[i];
            if (a.Status == JobStatus.Õnnestus && b.Status == JobStatus.Õnnestus)
            {
                compared++;
                if (a.Result == b.Result)
                    same++;
            }
        }

        string times = $"Järjestikune: {_lastSequentialTime.TotalSeconds:F2} s | Paralleelne: {_lastParallelTime.TotalSeconds:F2} s";

        // Aegu on aus võrrelda ainult siis, kui mõlemas režiimis tehti kogu töö ära
        if (compared == _lastSequential.Count && _lastParallelTime.TotalSeconds > 0)
            times += $" | paralleelne oli {_lastSequentialTime.TotalSeconds / _lastParallelTime.TotalSeconds:F1}× kiirem.";
        else
            times += " | kiirust ei võrrelda, sest mõni arvutus lõppes vea või tühistusega.";

        string results = compared == 0
            ? "Tulemusi pole võrrelda (pole looma, kes õnnestus mõlemas režiimis)."
            : $"Tulemused kattuvad: {same}/{compared} looma" + (same == compared ? " ✔" : " ✘");

        ComparisonText = times + "\n" + results;
    }
}
