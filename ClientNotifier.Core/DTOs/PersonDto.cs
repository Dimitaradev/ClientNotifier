using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClientNotifier.Core.DTOs
{
    public class PersonDto
    {
        public int Id { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string? LastName { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string EGN { get; set; } = string.Empty;
        public DateTime Birthday { get; set; }
        public DateTime? Nameday { get; set; }
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Notes { get; set; }
        public bool NotificationsEnabled { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public int Age { get; set; }
        public string? NamedayFormatted => Nameday?.ToString("dd MMMM");
        public string BirthdayFormatted => Birthday.ToString("dd MMMM yyyy");
    }

    public class CreatePersonDto
    {
        [Required(ErrorMessage = "First name is required")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "First name must be between 2 and 100 characters")]
        public string FirstName { get; set; } = string.Empty;

        [StringLength(100, ErrorMessage = "Last name cannot exceed 100 characters")]
        public string? LastName { get; set; }

        [Required(ErrorMessage = "EGN is required")]
        [StringLength(10, MinimumLength = 10, ErrorMessage = "EGN must be exactly 10 digits")]
        [RegularExpression(@"^\d{10}$", ErrorMessage = "EGN must contain only digits")]
        public string EGN { get; set; } = string.Empty;

        [EmailAddress(ErrorMessage = "Invalid email format")]
        [StringLength(255, ErrorMessage = "Email cannot exceed 255 characters")]
        public string? Email { get; set; }

        [Phone(ErrorMessage = "Invalid phone number format")]
        [StringLength(20, ErrorMessage = "Phone number cannot exceed 20 characters")]
        public string? PhoneNumber { get; set; }

        [StringLength(1000, ErrorMessage = "Notes cannot exceed 1000 characters")]
        public string? Notes { get; set; }

        public bool NotificationsEnabled { get; set; } = true;
    }

    public class UpdatePersonDto : CreatePersonDto
    {
        public int Id { get; set; }
    }

    public class PersonListDto
    {
        public int Id { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string EGN { get; set; } = string.Empty;
        public DateTime Birthday { get; set; }
        public DateTime? Nameday { get; set; }
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }
        public bool NotificationsEnabled { get; set; }
        public bool HasBirthdayToday { get; set; }
        public bool HasNamedayToday { get; set; }
        public bool HasBirthdayThisWeek { get; set; }
        public bool HasNamedayThisWeek { get; set; }
    }
}
