// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Windows;
using Sitecore.Rocks.ContentTrees.Items;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Extensibility.Pipelines;
using Sitecore.Rocks.Sites.Connections;

namespace Sitecore.Rocks.ContentTrees.Pipelines.GetChildren
{
    [Pipeline(typeof(GetChildrenPipeline), 1200)]
    public class GetConnectionFolder : PipelineProcessor<GetChildrenPipeline>
    {
        protected override void Process(GetChildrenPipeline pipeline)
        {
            Debug.ArgumentNotNull(pipeline, nameof(pipeline));

            if (pipeline.BaseFolder != null || pipeline.ParentItem != null)
            {
                return;
            }

            var folder = ConnectionManager.GetConnectionFolder();

            var item = new ConnectionFolderTreeViewItem(folder)
            {
                Margin = new Thickness(0, 16, 0, 0)
            };

            if (AppHost.Files.GetFiles(folder).Length > 0 || AppHost.Files.GetDirectories(folder).Length > 0)
            {
                item.MakeExpandable();
            }

            pipeline.Items.Add(item);

            if (AppHost.Settings.Get(ConnectionFolderTreeViewItem.RegistryPath, ConnectionFolderTreeViewItem.ConnectionsRelativeFolder, null) == null)
            {
                AppHost.Settings.SetBool(ConnectionFolderTreeViewItem.RegistryPath, ConnectionFolderTreeViewItem.ConnectionsRelativeFolder, true);
            }
        }
    }
}
