// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Web;
using Sitecore.Rocks.Commands;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.UI.Toolbars;

namespace Sitecore.Rocks.ContentEditors.Commands.Fields.TextFields
{
    [ToolbarElement(typeof(ContentEditorFieldContext), 2590, "Text", "Encoding", ElementType = RibbonElementType.SmallButton), Command]
    public class HtmlEncode : TextOperation, IToolbarElement
    {
        public HtmlEncode()
        {
            Text = "Html Encode";
            Group = "Text";
            SortingValue = 2590;
        }

        protected override string ProcessText(string value)
        {
            Debug.ArgumentNotNull(value, nameof(value));

            return HttpUtility.HtmlEncode(value) ?? string.Empty;
        }
    }
}
