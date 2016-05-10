// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using Sitecore.Rocks.Commands;
using Sitecore.Rocks.ContentTrees.Items;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Shell;

namespace Sitecore.Rocks.ContentTrees.Commands.Adding
{
    [Command, CommandId(CommandIds.SitecoreExplorer.NewFolder, typeof(ContentTreeContext))]
    public class NewTemplateFolder : NewFolder
    {
        public NewTemplateFolder()
        {
            Text = "New Template Folder";
            SortingValue = 5400;

            TemplateId = new Guid(@"{0437FEE2-44C9-46A6-ABE9-28858D9FEE8C}");
            TemplateName = "Template folder";
        }

        protected override bool CanExecute(ItemTreeViewItem item)
        {
            Debug.ArgumentNotNull(item, nameof(item));

            var path = item.GetPath();

            if (string.Compare(path, "/sitecore/templates", StringComparison.InvariantCultureIgnoreCase) == 0)
            {
                return true;
            }

            if (path.StartsWith("/sitecore/templates/", StringComparison.InvariantCultureIgnoreCase))
            {
                return true;
            }

            return false;
        }
    }
}
