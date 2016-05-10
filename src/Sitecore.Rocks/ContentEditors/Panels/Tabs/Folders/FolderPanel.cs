// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Rocks.Data;
using Sitecore.Rocks.Diagnostics;

namespace Sitecore.Rocks.ContentEditors.Panels.Tabs.Folders
{
    // [Panel("Folder View", 500, EnabledByDefault = true)]
    public class FolderPanel : PanelBase
    {
        public override bool CanRender(PanelContext context)
        {
            Assert.ArgumentNotNull(context, nameof(context));

            var contentModel = context.ContentModel;

            if (!contentModel.IsSingle)
            {
                return false;
            }

            var type = IdManager.GetTemplateType(contentModel.FirstItem.TemplateId);

            return type == @"folder";
        }

        public override void Render(PanelContext context)
        {
            Assert.ArgumentNotNull(context, nameof(context));

            var tab = new FolderPanelTab();

            tab.Initialize(context);

            AddTab(context, "Folder", 500, tab);
        }
    }
}
