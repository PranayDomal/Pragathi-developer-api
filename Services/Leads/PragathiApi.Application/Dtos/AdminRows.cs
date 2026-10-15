using PragathiApi.Domain.Enums;

namespace PragathiApi.Application.Dtos;

public record LeadRow(int Id, string Name, string Phone, string? Email, DateTime CreatedAt, DateTime UpdatedAt);

public record ActivityRow(
    int Id, int LeadId, string LeadName, ActivityType Type, string? Configuration,
    bool? HomeLoan, string? Message, string? Detail, DateTime CreatedAt);

public record VisitRow(
    int Id, int ActivityId, string LeadName, DateOnly PreferredDate, string TimeSlot,
    int Visitors, VisitStatus Status, DateTime CreatedAt);

public record ConfigurationRow(int Id, string Name, int SortOrder);