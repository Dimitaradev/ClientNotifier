using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ClientNotifier.Core.Services
{
    public static class EgnUtils
    {
        private static readonly int[] Weights = { 2, 4, 8, 5, 10, 9, 7, 3, 6 };

        public static bool IsValidEgn(string egn)
        {
            if (string.IsNullOrWhiteSpace(egn) || egn.Length != 10)
                return false;

            // Check if all characters are digits
            if (!Regex.IsMatch(egn, @"^\d{10}$"))
                return false;

            try
            {
                // Try to extract the birthday - if it fails, EGN is invalid
                ExtractBirthday(egn);
                
                // Validate checksum
                int checksum = 0;
                for (int i = 0; i < 9; i++)
                {
                    checksum += int.Parse(egn[i].ToString()) * Weights[i];
                }
                
                int lastDigit = checksum % 11;
                if (lastDigit == 10) lastDigit = 0;
                
                return lastDigit == int.Parse(egn[9].ToString());
            }
            catch
            {
                return false;
            }
        }

        public static DateTime ExtractBirthday(string egn)
        {
            if (!Regex.IsMatch(egn, @"^\d{10}$"))
                throw new ArgumentException("EGN must be exactly 10 digits");

            int year = int.Parse(egn.Substring(0, 2));
            int month = int.Parse(egn.Substring(2, 2));
            int day = int.Parse(egn.Substring(4, 2));

            // Handle century encoding in EGN
            if (month > 40) 
            { 
                year += 2000; 
                month -= 40; 
            }
            else if (month > 20) 
            { 
                year += 1800; 
                month -= 20; 
            }
            else 
            { 
                year += 1900; 
            }

            // Validate the date
            if (month < 1 || month > 12)
                throw new ArgumentException("Invalid month in EGN");
                
            if (day < 1 || day > DateTime.DaysInMonth(year, month))
                throw new ArgumentException("Invalid day in EGN");

            return new DateTime(year, month, day);
        }

        public static string GetGender(string egn)
        {
            if (!IsValidEgn(egn))
                throw new ArgumentException("Invalid EGN");

            int genderDigit = int.Parse(egn[8].ToString());
            return genderDigit % 2 == 0 ? "Male" : "Female";
        }

        public static int GetAge(string egn)
        {
            var birthday = ExtractBirthday(egn);
            var today = DateTime.Today;
            var age = today.Year - birthday.Year;
            
            if (birthday.Date > today.AddYears(-age))
                age--;
                
            return age;
        }
    }
}
