// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.IO;
using System.Linq;
using Sitecore.Rocks.ContentTrees.Items;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Extensibility.Pipelines;
using Sitecore.Rocks.Shell;
using Sitecore.Rocks.Sites;
using Sitecore.Rocks.Sites.Connections;

namespace Sitecore.Rocks.ContentTrees.Pipelines.GetChildren
{
    [Pipeline(typeof(GetChildrenPipeline), 1500)]
    public class GetConnectionFolders : PipelineProcessor<GetChildrenPipeline>
    {
        protected override void Process(GetChildrenPipeline pipeline)
        {
            Debug.ArgumentNotNull(pipeline, nameof(pipeline));

            if (pipeline.BaseFolder == null)
            {
                return;
            }

            if (!Directory.Exists(pipeline.BaseFolder))
            {
                return;
            }

            SiteManager.LoadSites();
            var subfolders = AppHost.Files.GetDirectories(pipeline.BaseFolder).ToList();

            var localConnectionFolder = ConnectionManager.GetLocalConnectionFolder();
            if (subfolders.Any(f => string.Compare(f, localConnectionFolder, StringComparison.InvariantCultureIgnoreCase) == 0))
            {
                if (AppHost.Files.GetFiles(localConnectionFolder).Length != 0 || AppHost.Files.GetDirectories(localConnectionFolder).Length != 0 || WebAdministration.CanAdminister)
                {
                    var item = new LocalConnectionFolderTreeViewItem(localConnectionFolder);

                    item.MakeExpandable();

                    pipeline.Items.Add(item);
                }

                subfolders.Remove(localConnectionFolder);
            }

            foreach (var folder in subfolders.OrderBy(f => f))
            {
                var item = new ConnectionFolderTreeViewItem(folder);

                if (AppHost.Files.GetFiles(folder).Length > 0 || AppHost.Files.GetDirectories(folder).Length > 0)
                {
                    item.MakeExpandable();
                }

                pipeline.Items.Add(item);
            }
        }
    }
}
