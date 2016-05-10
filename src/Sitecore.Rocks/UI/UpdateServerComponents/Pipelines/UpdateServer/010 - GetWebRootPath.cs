// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Extensibility.Pipelines;
using Sitecore.Rocks.Sites;
using Sitecore.Rocks.Sites.Connections;

namespace Sitecore.Rocks.UI.UpdateServerComponents.Pipelines.UpdateServer
{
    [Pipeline(typeof(UpdateServerPipeline), 1000)]
    public class GetWebRootPath : PipelineProcessor<UpdateServerPipeline>
    {
        protected override void Process(UpdateServerPipeline pipeline)
        {
            Debug.ArgumentNotNull(pipeline, nameof(pipeline));

            if (!string.IsNullOrEmpty(pipeline.WebRootPath))
            {
                return;
            }

            var siteName = string.Empty;
            var site = pipeline.Site;
            if (site != null)
            {
                siteName = site.Name;
            }

            pipeline.WebRootPath = GetWebSiteLocation(Resources.GetRoot_Process_Select_the_root_folder_of_the_web_site_to_update_, siteName);
            if (string.IsNullOrEmpty(pipeline.WebRootPath))
            {
                pipeline.Abort();
                return;
            }

            if (pipeline.Site == null)
            {
                return;
            }

            pipeline.Site.SetWebRootPath(pipeline.WebRootPath);

            ConnectionManager.Save();
        }

        [NotNull]
        private static string GetWebSiteLocation([NotNull] string description, [NotNull] string siteName)
        {
            Debug.ArgumentNotNull(description, nameof(description));
            Debug.ArgumentNotNull(siteName, nameof(siteName));

            return SiteHelper.BrowseWebRootPath(string.Empty, siteName) ?? string.Empty;
        }
    }
}
