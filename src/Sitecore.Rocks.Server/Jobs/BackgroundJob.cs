// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using Sitecore.Diagnostics;
using Sitecore.Rocks.Server.Abstractions.Jobs;

namespace Sitecore.Rocks.Server.Jobs
{
    public class BackgroundJob
    {
        [NotNull]
        public static string Run([NotNull] string jobName, [NotNull] string category, [NotNull] Action action)
        {
            Assert.ArgumentNotNull(jobName, nameof(jobName));
            Assert.ArgumentNotNull(category, nameof(category));
            Assert.ArgumentNotNull(action, nameof(action));

            var jobRunner = VersionSpecific.Services.JobManager;
            var jobOptions = new RunJobOptions
            {
                Name = jobName,
                Category = category,
                Action = action
            };
            return jobRunner.RunJob(jobOptions);
        }
    }
}
