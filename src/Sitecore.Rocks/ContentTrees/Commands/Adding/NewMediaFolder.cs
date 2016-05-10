// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using Sitecore.Rocks.Commands;
using Sitecore.Rocks.ContentTrees.Items;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Shell;

namespace Sitecore.Rocks.ContentTrees.Commands.Adding
{
    [Command, CommandId(CommandIds.SitecoreExplorer.NewFolder, typeof(ContentTreeContext))]
    public class NewMediaFolder : NewFolder
    {
        public NewMediaFolder()
        {
            Text = "New Media Folder";
            SortingValue = 5400;

            TemplateId = new Guid(@"{FE5DD826-48C6-436D-B87A-7C4210C7413B}");
            TemplateName = "Media folder";
        }

        protected override bool CanExecute(ItemTreeViewItem item)
        {
            Debug.ArgumentNotNull(item, nameof(item));

            var path = item.GetPath();

            if (string.Compare(path, "/sitecore/media library", StringComparison.InvariantCultureIgnoreCase) == 0)
            {
                return true;
            }

            if (path.StartsWith("/sitecore/media library/", StringComparison.InvariantCultureIgnoreCase))
            {
                return true;
            }

            return false;
        }
    }
}
