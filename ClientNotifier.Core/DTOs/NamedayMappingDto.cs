using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClientNotifier.Core.DTOs
{
    public class NamedayMappingDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int Month { get; set; }
        public int Day { get; set; }
        public string DateDisplay { get; set; } = string.Empty;
        public DateTime NextOccurrence { get; set; }
    }

    public class CreateNamedayMappingDto
    {
        [Required(ErrorMessage = "Name is required")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Name must be between 2 and 100 characters")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Month is required")]
        [Range(1, 12, ErrorMessage = "Month must be between 1 and 12")]
        public int Month { get; set; }

        [Required(ErrorMessage = "Day is required")]
        [Range(1, 31, ErrorMessage = "Day must be between 1 and 31")]
        public int Day { get; set; }
    }

    public class UpdateNamedayMappingDto : CreateNamedayMappingDto
    {
        public int Id { get; set; }
    }

    public class NamedayGroupDto
    {
        public int Month { get; set; }
        public int Day { get; set; }
        public string DateDisplay { get; set; } = string.Empty;
        public List<string> Names { get; set; } = new();
        public int PeopleCount { get; set; }
    }
}
