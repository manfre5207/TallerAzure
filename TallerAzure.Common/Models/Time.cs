using System;

namespace TallerAzure.Common.Models
{
    public class Time
    {
        public int EmployeeId { get; set; }
        public DateTime Date { get; set; }
        public int Type { get; set; }
        public bool IsConsolidate { get; set; }
    }
}
