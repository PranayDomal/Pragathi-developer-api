using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PragathiApi.Domain.Enums;

namespace PragathiApi.Domain.Entities
{
    public class LeadActivity
    {
        public int Id { get; set; }

        public int LeadId { get; set; }
        public Lead Lead { get; set; } = null!;

        public ActivityType Type { get; set; }

        public int? ConfigurationId { get; set; }        // empty when "Any"
        public RoomConfiguration? Configuration { get; set; }

        public bool? HomeLoan { get; set; }
        public string? Message { get; set; }             // contact page message
        public string? Detail { get; set; }              // e.g. floor plan name
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public Visit? Visit { get; set; }
    }
}
