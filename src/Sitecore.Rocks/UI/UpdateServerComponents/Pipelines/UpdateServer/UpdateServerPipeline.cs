// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Collections.Generic;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Extensibility.Pipelines;
using Sitecore.Rocks.Sites;
using Sitecore.Rocks.UI.UpdateServerComponents.Updates;

namespace Sitecore.Rocks.UI.UpdateServerComponents.Pipelines.UpdateServer
{
    public class UpdateServerPipeline : Pipeline<UpdateServerPipeline>
    {
        [NotNull]
        public DataService DataService { get; set; }

        [CanBeNull]
        public Site Site { get; set; }

        [NotNull]
        public List<UpdateInfo> Updates { get; set; }

        [NotNull]
        public string WebRootPath { get; set; }

        [NotNull]
        public UpdateServerPipeline WithParameters([NotNull] List<UpdateInfo> updates, [NotNull] DataService dataService, [NotNull] string webRootPath, [CanBeNull] Site site)
        {
            Assert.ArgumentNotNull(updates, nameof(updates));
            Assert.ArgumentNotNull(dataService, nameof(dataService));
            Assert.ArgumentNotNull(webRootPath, nameof(webRootPath));

            Updates = updates;
            DataService = dataService;
            WebRootPath = webRootPath;
            Site = site;

            return Start();
        }
    }
}
