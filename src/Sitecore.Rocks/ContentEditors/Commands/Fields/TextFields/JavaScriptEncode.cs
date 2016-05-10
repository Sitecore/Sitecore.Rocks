// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Web;
using Sitecore.Rocks.Commands;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.UI.Toolbars;

namespace Sitecore.Rocks.ContentEditors.Commands.Fields.TextFields
{
    [ToolbarElement(typeof(ContentEditorFieldContext), 2591, "Text", "Encoding", ElementType = RibbonElementType.SmallButton), Command]
    public class JavaScriptEncode : TextOperation, IToolbarElement
    {
        public JavaScriptEncode()
        {
            Text = "JavaScript Encode";
            Group = "Text";
            SortingValue = 2591;
        }

        protected override string ProcessText(string value)
        {
            Debug.ArgumentNotNull(value, nameof(value));

            return HttpUtility.UrlEncode(value) ?? string.Empty;
        }
    }
}
