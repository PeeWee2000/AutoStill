//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace AutoStillWPF
{
    using System;
    using System.Collections.Generic;
    
    public partial class RunRecord
    {
        public int rrID { get; set; }
        public int rrRHID { get; set; }
        public System.DateTime rrTime { get; set; }
        public decimal rrColumnHeadTemp { get; set; }
        public decimal rrStillTemp { get; set; }
        public decimal rrTempDelta { get; set; }
        public decimal rrPressure { get; set; }
        public int rrPhase { get; set; }
        public Nullable<decimal> rrAmperage { get; set; }
        public Nullable<decimal> rrRefluxTemp { get; set; }
        public Nullable<decimal> rrCondensorTemp { get; set; }
    
        public virtual RunHeader RunHeader { get; set; }
    }
}
