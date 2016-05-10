// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.ComponentModel;
using System.Drawing;
using System.Drawing.Design;
using System.IO;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Diagnostics;

namespace Sitecore.Rocks.Projects.ToolboxItems
{
    public abstract class ToolboxItemHandler
    {
        public virtual void AddToToolbox()
        {
        }

        public virtual void AddToToolbox([NotNull] ProjectItem projectItem)
        {
            Assert.ArgumentNotNull(projectItem, nameof(projectItem));

            var toolboxService = SitecorePackage.Instance.GetService<IToolboxService>();
            if (toolboxService == null)
            {
                return;
            }

            var newFilter = new ToolboxItemFilterAttribute("System.Web.UI", ToolboxItemFilterType.Allow);

            var toolboxItem = new ToolboxItem
            {
                Bitmap = new Bitmap(16, 16),
                Description = projectItem.Path,
                DisplayName = Path.GetFileNameWithoutExtension(projectItem.Path),
                IsTransient = true,
                Filter = new[]
                {
                    newFilter
                }
            };

            toolboxService.AddToolboxItem(toolboxItem, "Sitecore");
        }
    }
}
