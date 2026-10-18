namespace CrazyZoo.Core;

public interface IRepository<T> where T : Animal
{
    void Add(T item);
    bool Remove(T item);
    IEnumerable<T> GetAll();
    T? Find(Func<T, bool> predicate);
}
