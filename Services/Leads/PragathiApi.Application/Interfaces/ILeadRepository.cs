using PragathiApi.Domain.Entities;
using PragathiApi.Domain.Enums;

namespace PragathiApi.Application.Interfaces;

public interface ILeadRepository
{
    /// <summary>Finds a lead by international phone number, or null.</summary>
    Task<Lead?> GetByPhoneAsync(string phone, CancellationToken ct);

    /// <summary>Adds a brand-new lead (together with its first activity).</summary>
    Task AddAsync(Lead lead, CancellationToken ct);

    /// <summary>Adds an activity to a lead that already exists.</summary>
    Task AddActivityAsync(LeadActivity activity, CancellationToken ct);

    /// <summary>
    /// True if this lead already did the same thing since the given time
    /// (used to ignore double clicks and repeated spam).
    /// </summary>
    Task<bool> HasRecentActivityAsync(int leadId, ActivityType type, string? detail, DateTime sinceUtc, CancellationToken ct);
}