// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Rocks.Commands;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.UI.Toolbars;

namespace Sitecore.Rocks.ContentEditors.Commands.Fields.TextFields
{
    [ToolbarElement(typeof(ContentEditorFieldContext), 2595, "Text", "Transform", ElementType = RibbonElementType.SmallButton), Command]
    public class Lowercase : TextOperation, IToolbarElement
    {
        public Lowercase()
        {
            Text = "Make Lowercase";
            Group = "Text";
            SortingValue = 2575;
        }

        protected override string ProcessText(string value)
        {
            Debug.ArgumentNotNull(value, nameof(value));

            return value.ToLowerInvariant();
        }
    }
}
