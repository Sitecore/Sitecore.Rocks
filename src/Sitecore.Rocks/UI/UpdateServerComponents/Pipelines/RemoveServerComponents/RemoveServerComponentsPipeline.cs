// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Extensibility.Pipelines;
using Sitecore.Rocks.Sites;

namespace Sitecore.Rocks.UI.UpdateServerComponents.Pipelines.RemoveServerComponents
{
    public class RemoveServerComponentsPipeline : Pipeline<RemoveServerComponentsPipeline>
    {
        [NotNull]
        public DataService DataService { get; set; }

        [CanBeNull]
        public Site Site { get; set; }

        [NotNull]
        public string WebRootPath { get; set; }

        [NotNull]
        public RemoveServerComponentsPipeline WithParameters([NotNull] DataService dataService, [NotNull] string webRootPath, [CanBeNull] Site site)
        {
            Assert.ArgumentNotNull(dataService, nameof(dataService));
            Assert.ArgumentNotNull(webRootPath, nameof(webRootPath));

            DataService = dataService;
            WebRootPath = webRootPath;
            Site = site;

            return Start();
        }
    }
}
