using System.Diagnostics;

namespace CrazyZoo.Core.Forecast;

public enum JobStatus { Õnnestus, Viga, Tühistatud }

// Raporti üks rida: ühe looma arvutuse tulemus
public record JobReport(string Name, JobStatus Status, string Result, TimeSpan Duration)
{
    public string DurationText => $"{Duration.TotalSeconds:F2} s";
}

// Käivitab arvutused kahes režiimis: järjestikku ja paralleelselt
public static class ForecastRunner
{
    // Järjestikune: ootame ühe looma lõppu, alles siis alustame järgmist
    public static async Task<List<JobReport>> RunSequentialAsync(
        List<ForecastJob> jobs, int days, CancellationToken token, IProgress<JobReport>? progress = null)
    {
        List<JobReport> reports = new List<JobReport>();

        foreach (ForecastJob job in jobs)
        {
            JobReport report = await RunOneAsync(job, days, token);
            reports.Add(report);
            progress?.Report(report);
        }

        return reports;
    }

    // Paralleelne: käivitame kõik arvutused korraga ja ootame Task.WhenAll-iga
    public static async Task<List<JobReport>> RunParallelAsync(
        List<ForecastJob> jobs, int days, CancellationToken token, IProgress<JobReport>? progress = null)
    {
        List<Task<JobReport>> tasks = new List<Task<JobReport>>();

        foreach (ForecastJob job in jobs)
            tasks.Add(RunOneAndReportAsync(job, days, token, progress));

        JobReport[] reports = await Task.WhenAll(tasks);
        return reports.ToList();
    }

    private static async Task<JobReport> RunOneAndReportAsync(
        ForecastJob job, int days, CancellationToken token, IProgress<JobReport>? progress)
    {
        JobReport report = await RunOneAsync(job, days, token);
        progress?.Report(report);
        return report;
    }

    // Üks loom: arvutus käib Task.Run abil taustalõimes.
    // Viga ja tühistus püütakse siin kinni, seega ühe looma viga ei peata teisi.
    private static async Task<JobReport> RunOneAsync(ForecastJob job, int days, CancellationToken token)
    {
        string name = job.Animal.Name;
        Stopwatch stopwatch = Stopwatch.StartNew();

        try
        {
            string result = await Task.Run(() => ForecastCalculator.Calculate(job, days, token));
            return new JobReport(name, JobStatus.Õnnestus, result, stopwatch.Elapsed);
        }
        catch (OperationCanceledException)
        {
            return new JobReport(name, JobStatus.Tühistatud, "Arvutus tühistati.", stopwatch.Elapsed);
        }
        catch (Exception ex)
        {
            return new JobReport(name, JobStatus.Viga, ex.Message, stopwatch.Elapsed);
        }
    }
}
