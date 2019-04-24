// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Collections.Generic;
using System.IO;
using System.Xml;
using Sitecore.Diagnostics;
using Sitecore.Jobs;
using Job = Sitecore.Abstractions.BaseJob;

namespace Sitecore.Rocks.Server.Requests.UI.JobViewer
{
    public class GetJobs : IComparer<Job>
    {
        [NotNull]
        public string Execute()
        {
            var jobs = new List<Job>(JobManager.GetJobs());

            jobs.Sort(this);

            var writer = new StringWriter();
            var output = new XmlTextWriter(writer);

            RenderJobs(output, jobs);

            return writer.ToString();
        }

        int IComparer<Job>.Compare([NotNull] Job x, [NotNull] Job y)
        {
            Debug.ArgumentNotNull(x, nameof(x));
            Debug.ArgumentNotNull(y, nameof(y));

            return x.Name.CompareTo(y.Name);
        }

        private void RenderJob([NotNull] XmlTextWriter output, [NotNull] Job job)
        {
            Debug.ArgumentNotNull(output, nameof(output));
            Debug.ArgumentNotNull(job, nameof(job));

            string state;

            switch (job.Status.State)
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

            output.WriteStartElement("job");

            output.WriteAttributeString("name", job.Name);
            output.WriteAttributeString("queuetime", DateUtil.ToIsoDate(job.QueueTime));
            output.WriteAttributeString("category", job.Category);
            output.WriteAttributeString("isdone", job.IsDone ? "1" : "0");
            output.WriteAttributeString("processed", job.Status.Processed.ToString());
            output.WriteAttributeString("total", job.Status.Total.ToString());
            output.WriteAttributeString("failed", job.Status.Failed ? "1" : "0");
            output.WriteAttributeString("state", state);

            output.WriteEndElement();
        }

        private void RenderJobs([NotNull] XmlTextWriter output, [NotNull] List<Job> jobs)
        {
            Debug.ArgumentNotNull(output, nameof(output));
            Debug.ArgumentNotNull(jobs, nameof(jobs));

            output.WriteStartElement("jobs");

            foreach (var job in jobs)
            {
                RenderJob(output, job);
            }

            output.WriteEndElement();
        }
    }
}
