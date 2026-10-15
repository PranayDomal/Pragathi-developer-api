using Microsoft.EntityFrameworkCore;
using PragathiApi.Application.Dtos;
using PragathiApi.Application.Interfaces;
using PragathiApi.Infrastructure.Persistence;
using PragathiApi.Application.Common;

namespace PragathiApi.Infrastructure.Repositories;

public class AdminRepository(AppDb db) : IAdminRepository
{

    // ---------- Get ----------
    public Task<List<LeadRow>> GetLeadsAsync(CancellationToken ct) =>
        db.Leads.AsNoTracking()
            .OrderByDescending(l => l.Id)
            .Select(l => new LeadRow(l.Id, l.Name, l.Phone, l.Email, l.CreatedAt, l.UpdatedAt))
            .ToListAsync(ct);

    public Task<List<ActivityRow>> GetActivitiesAsync(CancellationToken ct) =>
        db.Activities.AsNoTracking()
            .OrderByDescending(a => a.Id)
            .Select(a => new ActivityRow(
                a.Id, a.LeadId, a.Lead.Name, a.Type,
                a.Configuration != null ? a.Configuration.Name : null,
                a.HomeLoan, a.Message, a.Detail, a.CreatedAt))
            .ToListAsync(ct);

    public Task<List<VisitRow>> GetVisitsAsync(CancellationToken ct) =>
        db.Visits.AsNoTracking()
            .OrderByDescending(v => v.Id)
            .Select(v => new VisitRow(
                v.Id, v.ActivityId, v.Activity.Lead.Name, v.PreferredDate,
                v.TimeSlot, v.Visitors, v.Status, v.CreatedAt))
            .ToListAsync(ct);

    public Task<List<ConfigurationRow>> GetConfigurationsAsync(CancellationToken ct) =>
        db.Configurations.AsNoTracking()
            .OrderBy(c => c.SortOrder)
            .Select(c => new ConfigurationRow(c.Id, c.Name, c.SortOrder))
            .ToListAsync(ct);


    // ---------- Updates ----------
    public async Task<bool> UpdateLeadAsync(int id, UpdateLeadRequest r, CancellationToken ct)
    {
        var lead = await db.Leads.FindAsync([id], ct);
        if (lead is null) return false;

        var phone = Phones.Normalize(r.Phone)
            ?? throw new ValidationFailedException("phone", "Enter a valid phone number.");
        if (await db.Leads.AnyAsync(l => l.Phone == phone && l.Id != id, ct))
            throw new ValidationFailedException("phone", "Another lead already has this phone number.");

        lead.Name = r.Name.Trim();
        lead.Phone = phone;
        lead.Email = Clean(r.Email);
        lead.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync(ct);
        return true;
    }

    public async Task<bool> UpdateActivityAsync(int id, UpdateActivityRequest r, CancellationToken ct)
    {
        var activity = await db.Activities.FindAsync([id], ct);
        if (activity is null) return false;

        int? configurationId = null;
        var name = Clean(r.Configuration);
        if (name is not null && !name.Equals("Any", StringComparison.OrdinalIgnoreCase))
        {
            var lowered = name.ToLower();
            var configuration = await db.Configurations.FirstOrDefaultAsync(c => c.Name.ToLower() == lowered, ct)
                ?? throw new ValidationFailedException("configuration", "Unknown room configuration.");
            configurationId = configuration.Id;
        }

        activity.Type = r.Type!.Value;
        activity.ConfigurationId = configurationId;
        activity.HomeLoan = r.HomeLoan;
        activity.Message = Clean(r.Message);
        activity.Detail = Clean(r.Detail);
        await db.SaveChangesAsync(ct);
        return true;
    }

    public async Task<bool> UpdateVisitAsync(int id, UpdateVisitRequest r, CancellationToken ct)
    {
        var visit = await db.Visits.FindAsync([id], ct);
        if (visit is null) return false;

        visit.PreferredDate = r.PreferredDate!.Value;
        visit.TimeSlot = r.TimeSlot.Trim();
        visit.Visitors = r.Visitors;
        visit.Status = r.Status!.Value;
        await db.SaveChangesAsync(ct);
        return true;
    }

    // ---------- Deletes (the database cascades: lead -> activities -> visit) ----------
    public async Task<bool> DeleteLeadAsync(int id, CancellationToken ct) =>
        await db.Leads.Where(x => x.Id == id).ExecuteDeleteAsync(ct) > 0;

    public async Task<bool> DeleteActivityAsync(int id, CancellationToken ct) =>
        await db.Activities.Where(x => x.Id == id).ExecuteDeleteAsync(ct) > 0;

    public async Task<bool> DeleteVisitAsync(int id, CancellationToken ct) =>
        await db.Visits.Where(x => x.Id == id).ExecuteDeleteAsync(ct) > 0;

    private static string? Clean(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}