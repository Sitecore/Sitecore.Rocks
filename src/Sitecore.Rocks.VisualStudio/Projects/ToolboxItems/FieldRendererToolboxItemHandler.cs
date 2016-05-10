// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

namespace Sitecore.Rocks.Projects.ToolboxItems
{
    [ToolboxItemHandler]
    public class FieldRendererToolboxItemHandler : HtmlToolboxItemHandler
    {
        public override void AddToToolbox()
        {
            AddHtml(Resources.FieldRendererToolboxItemHandler_AddToToolbox_FieldRenderer, "<sc:fieldrenderer runat=\"server\" renderingid=\"{E1AF4AA3-3B5D-4611-8C71-959AD261E5B7}\" />");
        }
    }
}
