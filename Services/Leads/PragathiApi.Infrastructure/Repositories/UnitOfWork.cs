using PragathiApi.Application.Interfaces;
using PragathiApi.Infrastructure.Persistence;

namespace PragathiApi.Infrastructure.Repositories;

public class UnitOfWork(AppDb db) : IUnitOfWork
{
    public Task<int> SaveChangesAsync(CancellationToken ct) => db.SaveChangesAsync(ct);
}