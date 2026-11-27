namespace CrazyZoo.Core.Persistence;

// Ühine leping: JSON, XML ja LocalDB teostavad sama liidest
public interface IWorldStorage
{
    Task SaveAsync(WorldDto world, string path, CancellationToken token = default);
    Task<WorldDto> LoadAsync(string path, CancellationToken token = default);
}
