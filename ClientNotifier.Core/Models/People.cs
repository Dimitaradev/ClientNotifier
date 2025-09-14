using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClientNotifier.Core.Models
{
    public class People
    {
        public int Id { get; set; }
        
        [Required]
        [StringLength(100, MinimumLength = 2)]
        public string FirstName { get; set; } = null!;
        
        [StringLength(100)]
        public string? LastName { get; set; }
        
        [Required]
        [StringLength(10, MinimumLength = 10)]
        [RegularExpression(@"^\d{10}$", ErrorMessage = "EGN must be exactly 10 digits")]
        public string EGN { get; set; } = null!;
        
        public DateTime Birthday { get; set; }   // extracted from EGN
        
        public DateTime? Nameday { get; set; }   // looked up from mapping
        
        [EmailAddress]
        [StringLength(255)]
        public string? Email { get; set; }
        
        [Phone]
        [StringLength(20)]
        public string? PhoneNumber { get; set; }
        
        [StringLength(1000)]
        public string? Notes { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        public DateTime? UpdatedAt { get; set; }
        
        // Computed property for full name
        public string FullName => string.IsNullOrWhiteSpace(LastName) 
            ? FirstName 
            : $"{FirstName} {LastName}";
            
        // Flag to enable/disable notifications
        public bool NotificationsEnabled { get; set; } = true;
    }
}
