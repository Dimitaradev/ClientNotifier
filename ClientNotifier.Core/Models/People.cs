using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClientNotifier.Core.Models
{
    public class People
    {
        public int Id { get; set; }
        public string FirstName { get; set; } = null!;
        public string? LastName { get; set; }
        public string EGN { get; set; } = null!;
        public DateTime Birthday { get; set; }   // extracted from EGN
        public DateTime? Nameday { get; set; }   // looked up from mapping
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Notes { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
