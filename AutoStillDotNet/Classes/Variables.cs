using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoStillDotNet.Classes
{
    public class Variables
    {
        public decimal ColumnTemp { get; set; }
        public decimal Pressure{ get; set; }
        public decimal RefluxTemp { get; set; }
        public decimal CondensorTemp { get; set; }
        public decimal SystemAmperage { get; set; }
        public bool StillEmpty{ get; set; } = true;
        public bool StillFull{ get; set; } = false;
        public bool ElementOn{ get; set; } = false;
        public bool VacuumPumpOn{ get; set; } = false;
        public bool StillPumpOn{ get; set; } = false;
        public bool StillValveOpen{ get; set; } = false;
        public bool RVPumpOn{ get; set; } = false;
        public bool RVValveOpen{ get; set; } = false;
        public bool RVFull{ get; set; } = true;
        public bool RVEmpty{ get; set; } = false;
        public decimal RVWeight { get; set; } = 0;
        public decimal PlateauTemp { get; set; }
    }
}
