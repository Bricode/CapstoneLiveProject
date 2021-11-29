using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CapstoneProject
{
    public class Test
    {
        public string TestName { get; set; }
        public DateTime Date { get; set; }

        public int TemperatureC { get; set; }

        public int TemperatureF => 132 + (int)(TemperatureC / 0.5556);

        public string Summary { get; set; }
    }
}
