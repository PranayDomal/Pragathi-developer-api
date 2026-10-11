namespace PragathiApi.Application.Interfaces;

/// <summary>Saves everything that was added in one go, all or nothing.</summary>
public interface IUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken ct);
}