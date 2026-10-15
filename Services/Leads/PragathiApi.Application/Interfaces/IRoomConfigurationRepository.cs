using PragathiApi.Domain.Entities;

namespace PragathiApi.Application.Interfaces;

public interface IRoomConfigurationRepository
{
    /// <summary>Finds a configuration by its name (ignoring upper/lower case), or null.</summary>
    Task<RoomConfiguration?> FindByNameAsync(string name, CancellationToken ct);
}