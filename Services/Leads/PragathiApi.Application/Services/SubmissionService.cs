using PragathiApi.Application.Common;
using PragathiApi.Application.Dtos;
using PragathiApi.Application.Interfaces;
using PragathiApi.Domain.Entities;
using PragathiApi.Domain.Enums;

namespace PragathiApi.Application.Services;

public class SubmissionService(
    ILeadRepository leads,
    IRoomConfigurationRepository configurations,
    IUnitOfWork unitOfWork) : ISubmissionService
{
    // The same person doing the same thing within this time is treated as a repeat (double click, spam)
    private static readonly TimeSpan RepeatWindow = TimeSpan.FromMinutes(5);

    public async Task<SubmitResponse> SubmitAsync(SubmitRequest request, CancellationToken ct)
    {
        var phone = Phones.Normalize(request.Phone)
            ?? throw new ValidationFailedException("phone", "Enter a valid phone number.");
        var type = request.Type
            ?? throw new ValidationFailedException("type", "Missing activity type.");
        var name = Clean(request.Name)
            ?? throw new ValidationFailedException("name", "Enter your name.");

        var email = Clean(request.Email);
        var now = DateTime.UtcNow;

        // "Any" or empty means no preference
        int? configurationId = null;
        var configurationName = Clean(request.Configuration);
        if (configurationName is not null && !configurationName.Equals("Any", StringComparison.OrdinalIgnoreCase))
        {
            var configuration = await configurations.FindByNameAsync(configurationName, ct)
                ?? throw new ValidationFailedException("configuration", "Unknown room configuration.");
            configurationId = configuration.Id;
        }

        var lead = await leads.GetByPhoneAsync(phone, ct);

        if (lead is not null)
        {
            // for floor plans, "the same thing" also means the same plan
            var detailToMatch = type == ActivityType.FloorPlanDownload ? Clean(request.Detail) : null;
            if (await leads.HasRecentActivityAsync(lead.Id, type, detailToMatch, now - RepeatWindow, ct))
                return new SubmitResponse(lead.Id, AlreadyReceived: true);

            lead.Name = name;
            if (email is not null) lead.Email = email;
            lead.UpdatedAt = now;
        }

        var activity = new LeadActivity
        {
            Type = type,
            ConfigurationId = configurationId,
            HomeLoan = request.HomeLoan,
            Message = Clean(request.Message),
            Detail = Clean(request.Detail),
            CreatedAt = now,
        };

        if (type == ActivityType.SiteVisit)
        {
            var visit = request.VisitDetails
                ?? throw new ValidationFailedException("visitDetails", "Visit details are required.");

            activity.Visit = new Visit
            {
                PreferredDate = visit.PreferredDate
                    ?? throw new ValidationFailedException("visitDetails", "Pick a preferred date."),
                TimeSlot = Clean(visit.TimeSlot)
                    ?? throw new ValidationFailedException("visitDetails", "Pick a time slot."),
                Visitors = visit.Visitors,
                CreatedAt = now,
            };
        }

        if (lead is null)
        {
            lead = new Lead
            {
                Name = name,
                Phone = phone,
                Email = email,
                CreatedAt = now,
                UpdatedAt = now,
                Activities = [activity],
            };
            await leads.AddAsync(lead, ct);
        }
        else
        {
            activity.LeadId = lead.Id;
            await leads.AddActivityAsync(activity, ct);
        }

        await unitOfWork.SaveChangesAsync(ct);
        return new SubmitResponse(lead.Id, AlreadyReceived: false);
    }

    private static string? Clean(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}