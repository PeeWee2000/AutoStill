using System.Data.Entity;

namespace AutoStillDotNet.Classes
{
    public class StillStatsContext : DbContext
    {
        public StillStatsContext() {}
        public DbSet<RunHeader> Headers { get; set; }
        public DbSet<RunRecord> Records { get; set; }
    }
}
