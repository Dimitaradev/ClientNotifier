using ClientNotifier.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClientNotifier.Core.Services
{
    public class NamedayService
    {
        private readonly List<NamedayMapping> _namedayMappings;

        public NamedayService(List<NamedayMapping> namedayMappings)
        {
            _namedayMappings = namedayMappings;
        }

        public DateTime? GetNamedayForPerson(People person)
        {
            if (string.IsNullOrWhiteSpace(person.FirstName))
                return null;

            // Try exact match first
            var mapping = _namedayMappings.FirstOrDefault(m => 
                m.Name.Equals(person.FirstName, StringComparison.OrdinalIgnoreCase));

            if (mapping != null)
            {
                var currentYear = DateTime.Now.Year;
                return mapping.GetNamedayForYear(currentYear);
            }

            // Try partial match (for diminutive forms)
            // For example: "Иванка" might match "Иван"
            mapping = _namedayMappings.FirstOrDefault(m => 
                person.FirstName.StartsWith(m.Name, StringComparison.OrdinalIgnoreCase) ||
                m.Name.StartsWith(person.FirstName, StringComparison.OrdinalIgnoreCase));

            if (mapping != null)
            {
                var currentYear = DateTime.Now.Year;
                return mapping.GetNamedayForYear(currentYear);
            }

            return null;
        }

        public List<People> GetPeopleWithNamedaysToday(List<People> people)
        {
            var today = DateTime.Today;
            return people.Where(p => 
                p.Nameday.HasValue && 
                p.Nameday.Value.Month == today.Month && 
                p.Nameday.Value.Day == today.Day &&
                p.NotificationsEnabled
            ).ToList();
        }

        public List<People> GetPeopleWithBirthdaysToday(List<People> people)
        {
            var today = DateTime.Today;
            return people.Where(p => 
                p.Birthday.Month == today.Month && 
                p.Birthday.Day == today.Day &&
                p.NotificationsEnabled
            ).ToList();
        }

        public List<NamedayMapping> GetAllNamedaysForDate(int month, int day)
        {
            return _namedayMappings.Where(m => m.Month == month && m.Day == day).ToList();
        }

        public bool UpdatePersonNameday(People person, List<NamedayMapping> mappings)
        {
            _namedayMappings.Clear();
            _namedayMappings.AddRange(mappings);
            
            var nameday = GetNamedayForPerson(person);
            if (nameday.HasValue)
            {
                person.Nameday = nameday.Value;
                return true;
            }
            return false;
        }
    }
}
