// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.IO;

namespace Sitecore.Rocks.Server.Requests.UI.LogViewer
{
    internal class GetLogItem
    {
        public string Category { get; set; }

        public string Description { get; set; }

        public FileInfo File { get; set; }

        public string Guid { get; set; }

        public int LineNumber { get; set; }

        public string Link { get; set; }

        public DateTime PubDate { get; set; }

        public string Title { get; set; }

        public string UserName { get; set; }
    }
}
