using System.ComponentModel.DataAnnotations;
using PragathiApi.Domain.Enums;

namespace PragathiApi.Application.Dtos;

public class UpdateLeadRequest
{
    [Required, StringLength(100, MinimumLength = 2)] public string Name { get; set; } = "";
    [Required] public string Phone { get; set; } = "";
    [EmailAddress, StringLength(200)] public string? Email { get; set; }
}

public class UpdateActivityRequest
{
    [Required] public ActivityType? Type { get; set; }
    [StringLength(60)] public string? Configuration { get; set; }   // room name, or "Any"/empty for none
    public bool? HomeLoan { get; set; }
    [StringLength(2000)] public string? Message { get; set; }
    [StringLength(200)] public string? Detail { get; set; }
}

public class UpdateVisitRequest
{
    [Required] public DateOnly? PreferredDate { get; set; }
    [Required, StringLength(20)] public string TimeSlot { get; set; } = "";
    [Range(1, 20)] public int Visitors { get; set; } = 1;
    [Required] public VisitStatus? Status { get; set; }
}
