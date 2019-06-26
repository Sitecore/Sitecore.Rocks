using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sitecore.Rocks.Server.Abstractions.Jobs
{
    public class Job
    {
        public string Name { get; set; }
        public DateTime QueueTime { get; set; }
        public string Category { get; set; }
        public bool IsDone { get; set; }
        public bool IsFailed { get; set; }
        public long ProcessedCount { get; set; }
        public long TotalCount { get; set; }
        public string State { get; set; }
    }
}
