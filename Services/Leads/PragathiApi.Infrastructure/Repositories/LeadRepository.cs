using Microsoft.EntityFrameworkCore;
using PragathiApi.Application.Interfaces;
using PragathiApi.Domain.Entities;
using PragathiApi.Domain.Enums;
using PragathiApi.Infrastructure.Persistence;

namespace PragathiApi.Infrastructure.Repositories;

public class LeadRepository(AppDb db) : ILeadRepository
{
    public Task<Lead?> GetByPhoneAsync(string phone, CancellationToken ct) =>
        db.Leads.FirstOrDefaultAsync(l => l.Phone == phone, ct);

    public async Task AddAsync(Lead lead, CancellationToken ct) =>
        await db.Leads.AddAsync(lead, ct);

    public async Task AddActivityAsync(LeadActivity activity, CancellationToken ct) =>
        await db.Activities.AddAsync(activity, ct);

    public Task<bool> HasRecentActivityAsync(
        int leadId, ActivityType type, string? detail, DateTime sinceUtc, CancellationToken ct) =>
        db.Activities.AnyAsync(a =>
            a.LeadId == leadId &&
            a.Type == type &&
            a.CreatedAt >= sinceUtc &&
            (detail == null || a.Detail == detail), ct);
}