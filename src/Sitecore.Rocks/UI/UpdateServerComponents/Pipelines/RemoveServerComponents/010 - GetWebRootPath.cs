// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Extensibility.Pipelines;
using Sitecore.Rocks.Sites;
using Sitecore.Rocks.Sites.Connections;

namespace Sitecore.Rocks.UI.UpdateServerComponents.Pipelines.RemoveServerComponents
{
    [Pipeline(typeof(RemoveServerComponentsPipeline), 1000)]
    public class GetRoot : PipelineProcessor<RemoveServerComponentsPipeline>
    {
        protected override void Process(RemoveServerComponentsPipeline pipeline)
        {
            Debug.ArgumentNotNull(pipeline, nameof(pipeline));

            if (!string.IsNullOrEmpty(pipeline.WebRootPath))
            {
                return;
            }

            pipeline.WebRootPath = GetWebRootPath(Resources.GetRoot_Process_Select_the_root_folder_of_the_web_site_to_remove_server_components_from_, pipeline.Site.Name);
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
        private string GetWebRootPath([NotNull] string description, [NotNull] string siteName)
        {
            Debug.ArgumentNotNull(description, nameof(description));
            Debug.ArgumentNotNull(siteName, nameof(siteName));

            return SiteHelper.BrowseWebRootPath(string.Empty, siteName) ?? string.Empty;
        }
    }
}
