using System;

namespace TallerAzure.Common.Models
{
    public class Consolidate
    {
        public int EmployeeId { get; set; }
        public DateTime Date { get; set; }
        public double MinutesWork { get; set; }
        //public string PartitionKey { get; internal set; }
        //public string RowKey { get; internal set; }
        //public string ETag { get; internal set; }
    }
}
