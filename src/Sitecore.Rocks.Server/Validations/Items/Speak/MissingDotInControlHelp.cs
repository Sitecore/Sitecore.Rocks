// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Data.Items;

namespace Sitecore.Rocks.Server.Validations.Items
{
    [Validation("Control help text must end with a dot", "SPEAK")]
    public class MissingDotInControlHelp : ViewRenderingValidation
    {
        public override bool CanCheck(string contextName, Item item)
        {
            return contextName == "Site" && IsViewRendering(item);
        }

        public override void Check(ValidationWriter output, Item item)
        {
            if (!string.IsNullOrEmpty(item.Help.ToolTip) && !item.Help.ToolTip.EndsWith("."))
            {
                output.Write(SeverityLevel.Suggestion, "Control short help text must end with a dot", string.Format("The short help text for the control '{0}' must be a complete sentence and end with a dot.", item.Name), "Add a dot to the short help text.", item);
            }

            if (!string.IsNullOrEmpty(item.Help.Text) && !item.Help.Text.EndsWith("."))
            {
                output.Write(SeverityLevel.Suggestion, "Control long help text must end with a dot", string.Format("The long help text for the control '{0}' must be a complete sentence and end with a dot.", item.Name), "Add a dot to the long help text.", item);
            }
        }
    }
}
