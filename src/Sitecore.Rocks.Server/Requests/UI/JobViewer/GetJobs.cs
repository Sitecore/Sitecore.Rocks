// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Collections.Generic;
using System.IO;
using System.Xml;
using Sitecore.Diagnostics;
using Sitecore.Rocks.Server.Abstractions.Jobs;

namespace Sitecore.Rocks.Server.Requests.UI.JobViewer
{
    public class GetJobs : IComparer<Job>
    {
        [NotNull]
        public string Execute()
        {
            var jobManager = Sitecore.Rocks.Server.VersionSpecific.Services.JobManager;
            var jobs = new List<Job>(jobManager.GetJobs());
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

            output.WriteStartElement("job");

            output.WriteAttributeString("name", job.Name);
            output.WriteAttributeString("queuetime", DateUtil.ToIsoDate(job.QueueTime));
            output.WriteAttributeString("category", job.Category);
            output.WriteAttributeString("isdone", job.IsDone ? "1" : "0");
            output.WriteAttributeString("processed", job.ProcessedCount.ToString());
            output.WriteAttributeString("total", job.TotalCount.ToString());
            output.WriteAttributeString("failed", job.IsFailed ? "1" : "0");
            output.WriteAttributeString("state", job.State);

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
