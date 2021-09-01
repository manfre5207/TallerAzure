using Microsoft.WindowsAzure.Storage.Table;
using System;

namespace TallerAzure.Functions.Entities
{
    public class TimeEntity : TableEntity
    {
        public int EmployeeId { get; set; }
        public DateTime Date { get; set; }
        public int Type { get; set; }
        public bool IsConsolidate { get; set; }
    }
}
