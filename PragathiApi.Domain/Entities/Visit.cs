using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PragathiApi.Domain.Enums;

namespace PragathiApi.Domain.Entities
{
    public class Visit
    {
        public int Id { get; set; }

        public int ActivityId { get; set; }
        public LeadActivity Activity { get; set; } = null!;

        public DateOnly PreferredDate { get; set; }
        public string TimeSlot { get; set; } = "";       // "10:00 AM", "2:00 PM", ...
        public int Visitors { get; set; } = 1;
        public VisitStatus Status { get; set; } = VisitStatus.Requested;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
