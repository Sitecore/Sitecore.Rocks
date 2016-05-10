// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using Sitecore.Diagnostics;
using Sitecore.Jobs;

namespace Sitecore.Rocks.Server.Jobs
{
    public class BackgroundJob
    {
        private readonly Action action;

        private BackgroundJob([NotNull] Action action)
        {
            Debug.ArgumentNotNull(action, nameof(action));

            this.action = action;
        }

        [NotNull]
        public static string Run([NotNull] string jobName, [NotNull] string category, [NotNull] Action action)
        {
            Assert.ArgumentNotNull(jobName, nameof(jobName));
            Assert.ArgumentNotNull(category, nameof(category));
            Assert.ArgumentNotNull(action, nameof(action));

            var jobOptions = new JobOptions(jobName, category, Client.Site.Name, new BackgroundJob(action), "RunJob")
            {
                AfterLife = TimeSpan.FromMinutes(1),
                ContextUser = Context.User
            };

            var job = JobManager.Start(jobOptions);

            return job.Handle.ToString();
        }

        public void RunJob()
        {
            var job = Context.Job;
            if (job == null)
            {
                return;
            }

            try
            {
                action();
            }
            catch (Exception ex)
            {
                job.Status.Failed = true;
                job.Status.Messages.Add(ex.ToString());
            }

            job.Status.State = JobState.Finished;
        }
    }
}
