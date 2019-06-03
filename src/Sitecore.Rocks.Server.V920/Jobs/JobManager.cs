using System;
using System.Collections.Generic;
using System.Linq;
using Sitecore.Diagnostics;
using Sitecore.Jobs;
using Sitecore.Rocks.Server.Abstractions.Jobs;
using JobOptions = Sitecore.Jobs.DefaultJobOptions;
using SitecoreJob = Sitecore.Abstractions.BaseJob;

namespace Sitecore.Rocks.Server.V920.Jobs
{
    public class JobManager : IJobManager
    {
        public string RunJob(Abstractions.Jobs.RunJobOptions options)
        {
            Assert.ArgumentNotNull(options, nameof(options));
            var jobOptions = new JobOptions(options.Name, options.Category, Client.Site.Name, new JobRunner(options.Action), nameof(JobRunner.RunJob))
            {
                AfterLife = TimeSpan.FromMinutes(1),
                ContextUser = Sitecore.Context.User
            };
            var job = Sitecore.Jobs.JobManager.Start(jobOptions);
            return job.Handle.ToString();
        }

        public IEnumerable<Job> GetJobs()
        {
            return Sitecore.Jobs.JobManager.GetJobs().Select(MapJob);
        }

        public Job MapJob(SitecoreJob sitecoreJob)
        {
            string state;
            switch (sitecoreJob.Status.State)
            {
                case JobState.Initializing:
                    state = "Initializing";
                    break;
                case JobState.Queued:
                    state = "Queued";
                    break;
                case JobState.Running:
                    state = "Running";
                    break;
                case JobState.Finished:
                    state = "Finished";
                    break;
                default:
                    state = "Unknown";
                    break;
            }

            return new Job
            {
                Name = sitecoreJob.Name,
                QueueTime = sitecoreJob.QueueTime,
                Category = sitecoreJob.Category,
                IsDone = sitecoreJob.IsDone,
                IsFailed = sitecoreJob.Status.Failed,
                ProcessedCount = sitecoreJob.Status.Processed,
                TotalCount = sitecoreJob.Status.Total,
                State = state
            };
        }

        protected class JobRunner
        {
            private readonly Action action;

            public JobRunner([NotNull] Action action)
            {
                Debug.ArgumentNotNull(action, nameof(action));
                this.action = action;
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
}
