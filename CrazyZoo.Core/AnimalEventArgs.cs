namespace CrazyZoo.Core;

// Sündmuse andmed: milline loom tegevuse põhjustas
public class AnimalEventArgs : EventArgs
{
    public Animal Source { get; }

    public AnimalEventArgs(Animal source)
    {
        Source = source ?? throw new ArgumentNullException(nameof(source));
    }
}
