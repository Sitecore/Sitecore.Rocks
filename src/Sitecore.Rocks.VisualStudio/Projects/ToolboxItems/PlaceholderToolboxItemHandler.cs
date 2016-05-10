// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

namespace Sitecore.Rocks.Projects.ToolboxItems
{
    [ToolboxItemHandler]
    public class PlaceholderToolboxItemHandler : HtmlToolboxItemHandler
    {
        public override void AddToToolbox()
        {
            AddHtml(Resources.PlaceholderToolboxItemHandler_AddToToolbox_Placeholder, "<sc:placeholder runat=\"server\" key=\"content\" />");
        }
    }
}
