using PragathiApi.Application.Dtos;

namespace PragathiApi.Application.Interfaces;

public interface IAdminRepository
{
    Task<List<LeadRow>> GetLeadsAsync(CancellationToken ct);
    Task<List<ActivityRow>> GetActivitiesAsync(CancellationToken ct);
    Task<List<VisitRow>> GetVisitsAsync(CancellationToken ct);
    Task<List<ConfigurationRow>> GetConfigurationsAsync(CancellationToken ct);

    Task<bool> UpdateLeadAsync(int id, UpdateLeadRequest request, CancellationToken ct);
    Task<bool> UpdateActivityAsync(int id, UpdateActivityRequest request, CancellationToken ct);
    Task<bool> UpdateVisitAsync(int id, UpdateVisitRequest request, CancellationToken ct);

    Task<bool> DeleteLeadAsync(int id, CancellationToken ct);
    Task<bool> DeleteActivityAsync(int id, CancellationToken ct);
    Task<bool> DeleteVisitAsync(int id, CancellationToken ct);
}