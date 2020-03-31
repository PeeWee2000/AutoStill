namespace AutoStillWPF
{
    /// <summary>
    /// Varaiables written to and read by all the various loops -- Assumes the still is empty and all periphrials are off when starting up
    /// </summary>
    public class StateVariables
    {
        /// <summary>
        /// Varaiables written to and read by all the various loops -- Assumes the still is empty and all periphrials are off when starting up
        /// </summary>
        public StateVariables()
        { }

        public decimal ColumnTemp { get; set; }
        public decimal StillFluidTemp { get; set; }
        public decimal Pressure { get; set; } = 101.325M;
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
        public int Phase { get; set; } = -1; //Used to control the main still control background worker and report progress -1 = initializing, 0 = filling still and checking values, 1 = heating / vacuuming, 2 = distilling, 3 = draining
        public int RunID { get; set; }
        public bool Run { get; set; } = true; //Used to shut down or start the whole process
        public decimal TheoreticalBoilingPoint { get; set; }
    }
}
