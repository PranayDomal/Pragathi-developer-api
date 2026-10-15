using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PragathiApi.Application.Dtos
{
    public class VisitInfo
    {
        [Required(ErrorMessage = "Pick a preferred date.")]
        public DateOnly? PreferredDate { get; set; }

        [Required(ErrorMessage = "Pick a time slot.")]
        [StringLength(20)]
        public string? TimeSlot { get; set; }

        [Range(1, 20, ErrorMessage = "Visitors must be between 1 and 20.")]
        public int Visitors { get; set; } = 1;
    }
}
