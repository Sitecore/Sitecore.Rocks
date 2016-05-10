// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Rocks.Commands;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.UI.Toolbars;

namespace Sitecore.Rocks.ContentEditors.Commands.Fields.TextFields
{
    [ToolbarElement(typeof(ContentEditorFieldContext), 2598, "Text", "Add", ElementType = RibbonElementType.SmallButton), Command]
    public class LoremIpsum : TextOperation, IToolbarElement
    {
        public LoremIpsum()
        {
            Text = Resources.Add_Lorem_Ipsum;
            Group = "Test";
            SortingValue = 2600;
        }

        protected override string ProcessText(string value)
        {
            Debug.ArgumentNotNull(value, nameof(value));

            return value + @"Lorem ipsum dolor sit amet, consectetur adipiscing elit. Mauris congue vulputate erat hendrerit tincidunt.";
        }
    }
}
