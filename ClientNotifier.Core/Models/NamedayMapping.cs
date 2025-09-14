using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClientNotifier.Core.Models
{
    public class NamedayMapping
    {
        public int Id { get; set; }
        
        [Required]
        [StringLength(100, MinimumLength = 2)]
        public string Name { get; set; } = null!;
        
        [Required]
        public int Month { get; set; } // 1-12
        
        [Required]
        public int Day { get; set; }   // 1-31
        
        // This will make it easier to query by date
        public DateTime GetNamedayForYear(int year)
        {
            return new DateTime(year, Month, Day);
        }
        
        // For display purposes
        public string DateDisplay => $"{Day:D2}/{Month:D2}";
    }
}
