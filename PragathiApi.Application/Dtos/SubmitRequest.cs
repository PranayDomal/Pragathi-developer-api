using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PragathiApi.Domain.Enums;

namespace PragathiApi.Application.Dtos
{
    public class SubmitRequest
    {

        [Required(ErrorMessage = "Enter your name.")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Enter your full name.")]
        public string? Name { get; set; }

        [Required(ErrorMessage = "Enter your phone number.")]
        [StringLength(30)]
        public string? Phone { get; set; }

        [EmailAddress(ErrorMessage = "Enter a valid email.")]
        [StringLength(200)]
        public string? Email { get; set; }

        [Required(ErrorMessage = "Missing activity type.")]
        public ActivityType? Type { get; set; }

        /// <summary>Room configuration name, e.g. "2 BHK Apartment". Empty or "Any" means no preference.</summary>
        [StringLength(60)]
        public string? Configuration { get; set; }

        public bool? HomeLoan { get; set; }

        [StringLength(2000, ErrorMessage = "Message is too long.")]
        public string? Message { get; set; }

        /// <summary>Extra info, e.g. the floor plan name for a floor plan download.</summary>
        [StringLength(200)]
        public string? Detail { get; set; }

        public VisitInfo? VisitDetails { get; set; }

        // Rules that depend on more than one field
        public IEnumerable<ValidationResult> Validate(ValidationContext context)
        {
            if (Type == ActivityType.SiteVisit)
            {
                if (VisitDetails is null)
                {
                    yield return new ValidationResult("Visit details are required.", [nameof(VisitDetails)]);
                }
                else if (VisitDetails.PreferredDate is { } date)
                {
                    var todayInIndia = DateOnly.FromDateTime(DateTime.UtcNow.AddHours(5.5));
                    if (date < todayInIndia)
                        yield return new ValidationResult("Pick today or a later date.", [nameof(VisitDetails)]);
                }
            }

            if (Type == ActivityType.FloorPlanDownload && string.IsNullOrWhiteSpace(Detail))
            {
                yield return new ValidationResult("Choose a floor plan.", [nameof(Detail)]);
            }

        }
    }
}
