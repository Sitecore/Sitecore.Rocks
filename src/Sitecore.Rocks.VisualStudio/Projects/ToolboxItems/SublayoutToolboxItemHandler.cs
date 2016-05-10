// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.IO;
using Sitecore.Rocks.Diagnostics;

namespace Sitecore.Rocks.Projects.ToolboxItems
{
    [ToolboxItemHandler(".ascx")]
    public class SublayoutToolboxItemHandler : HtmlToolboxItemHandler
    {
        public override void AddToToolbox(ProjectItem projectItem)
        {
            Assert.ArgumentNotNull(projectItem, nameof(projectItem));

            var name = Path.GetFileNameWithoutExtension(projectItem.Path) ?? string.Empty;

            var path = projectItem.Path;

            path = path.Replace('\\', '/');

            if (!path.StartsWith(@"/"))
            {
                path = '/' + path;
            }

            const string Html = "<sc:sublayout runat=\"server\" path=\"{0}\"{1} />";

            var renderingId = string.Empty;

            var projectFile = projectItem as ProjectFileItem;
            if (projectFile != null && projectFile.Items.Count > 0)
            {
                renderingId = string.Format(" renderingId=\"{0}\"", projectFile.Items[0].ItemId);
            }

            AddHtml(name, string.Format(Html, path, renderingId));
        }
    }
}
