using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClientNotifier.Core.Services
{
    public static class EgnUtils
    {
        public static DateTime ExtractBirthday(string egn)
        {
            if (egn.Length < 6)
                throw new ArgumentException("Invalid EGN");

            int year = int.Parse(egn.Substring(0, 2));
            int month = int.Parse(egn.Substring(2, 2));
            int day = int.Parse(egn.Substring(4, 2));

            // Handle century encoding in EGN
            if (month > 40) { year += 2000; month -= 40; }
            else if (month > 20) { year += 1800; month -= 20; }
            else { year += 1900; }

            return new DateTime(year, month, day);
        }
    }
}
