using Microsoft.EntityFrameworkCore;
using PragathiApi.Application.Interfaces;
using PragathiApi.Domain.Entities;
using PragathiApi.Infrastructure.Persistence;

namespace PragathiApi.Infrastructure.Repositories;

public class RoomConfigurationRepository(AppDb db) : IRoomConfigurationRepository
{
    public Task<RoomConfiguration?> FindByNameAsync(string name, CancellationToken ct)
    {
        var wanted = name.Trim().ToLower();
        return db.Configurations.FirstOrDefaultAsync(c => c.Name.ToLower() == wanted, ct);
    }
}