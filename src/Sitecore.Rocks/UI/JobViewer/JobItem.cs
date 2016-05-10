// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using Sitecore.Rocks.Annotations;

namespace Sitecore.Rocks.UI.JobViewer
{
    public class JobItem
    {
        public string Category { get; set; }

        public bool Failed { get; set; }

        public bool IsDone { get; set; }

        public string Name { get; set; }

        public int Processed { get; set; }

        [NotNull, UsedImplicitly]
        public string Progress
        {
            get
            {
                if (Total <= 0 && Processed <= 0)
                {
                    return Resources.N_A;
                }

                if (Total <= 0)
                {
                    return Processed.ToString(@"#,##0");
                }

                return string.Format(Resources.JobItem_Progress__0_____1__of__2__, (Processed / Total * 100).ToString(@"#,##0.0"), Processed, Total);
            }
        }

        public DateTime QueueTime { get; set; }

        public string State { get; set; }

        public int Total { get; set; }
    }
}
