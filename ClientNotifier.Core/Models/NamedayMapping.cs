using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClientNotifier.Core.Models
{
    public class NamedayMapping
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public DateTime Nameday { get; set; }
    }
}
