using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sitecore.Rocks.Server.Abstractions.Jobs
{
    public interface IJobManager
    {
        string RunJob(RunJobOptions options);
        IEnumerable<Job> GetJobs();
    }
}
