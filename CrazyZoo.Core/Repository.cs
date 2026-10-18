namespace CrazyZoo.Core;

// Üks hoidla hoiab ainult ühte loomatüüpi T. Vale tüübi lisamist takistab kompilaator.
public class Repository<T> : IRepository<T> where T : Animal
{
    private readonly List<T> _items = new();

    // Kolm sündmust: tellijateks on hoidla enda loomad
    public event EventHandler<AnimalEventArgs>? ItemAdded;
    public event EventHandler<AnimalEventArgs>? ItemRemoved;
    public event EventHandler<AnimalEventArgs>? ItemCrazyAction;

    // Loomade reaktsioonide logikirjed
    public event Action<string>? Logged;

    public void Add(T item)
    {
        ArgumentNullException.ThrowIfNull(item);
        if (_items.Contains(item))
            throw new ArgumentException("See loom on juba hoidlas.");

        // 1) Senised loomad tervitavad. Uus loom ei ole veel tellija, seega ei tervita ennast.
        ItemAdded?.Invoke(this, new AnimalEventArgs(item));

        // 2) Alles nüüd hakkab uus loom sündmusi tellima
        _items.Add(item);
        Subscribe(item);
    }

    public bool Remove(T item)
    {
        if (item is null || !_items.Remove(item))
            return false;

        // Eemaldatud loom lõpetab tellimise enne, kui teised hüvasti jätavad
        Unsubscribe(item);
        ItemRemoved?.Invoke(this, new AnimalEventArgs(item));
        return true;
    }

    public IEnumerable<T> GetAll() => _items.ToList();

    public T? Find(Func<T, bool> predicate) => _items.FirstOrDefault(predicate);

    // Käivitab looma CrazyAction'i ja teatab sellest teistele sama hoidla loomadele.
    // Kui tegevus ei muutnud olekut (loom keeldus), siis keegi ei reageeri.
    public string PerformCrazyAction(T item)
    {
        if (!_items.Contains(item))
            throw new ArgumentException("Looma pole selles hoidlas.");

        int energyBefore = item.Energy;
        int hungerBefore = item.Hunger;

        string message = item.CrazyAction();

        bool actionHappened = item.Energy != energyBefore || item.Hunger != hungerBefore;
        if (actionHappened)
            ItemCrazyAction?.Invoke(this, new AnimalEventArgs(item));

        return message;
    }

    private void Subscribe(T item)
    {
        ItemAdded += item.HandleNeighborAdded;
        ItemRemoved += item.HandleNeighborRemoved;
        ItemCrazyAction += item.HandleNeighborCrazyAction;
        item.ReactionLogged += OnReactionLogged;
    }

    private void Unsubscribe(T item)
    {
        ItemAdded -= item.HandleNeighborAdded;
        ItemRemoved -= item.HandleNeighborRemoved;
        ItemCrazyAction -= item.HandleNeighborCrazyAction;
        item.ReactionLogged -= OnReactionLogged;
    }

    private void OnReactionLogged(string message) => Logged?.Invoke(message);
}
