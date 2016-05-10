// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Data.Items;
using Sitecore.Diagnostics;

namespace Sitecore.Rocks.Server.Validations.Items
{
    [Validation("Control must have help text", "SPEAK")]
    public class ControlHelp : ViewRenderingValidation
    {
        public override bool CanCheck(string contextName, Item item)
        {
            Assert.ArgumentNotNull(contextName, nameof(contextName));
            Assert.ArgumentNotNull(item, nameof(item));

            return contextName == "Site" && IsViewRendering(item);
        }

        public override void Check(ValidationWriter output, Item item)
        {
            Assert.ArgumentNotNull(output, nameof(output));
            Assert.ArgumentNotNull(item, nameof(item));

            if (string.IsNullOrEmpty(item.Help.ToolTip))
            {
                output.Write(SeverityLevel.Suggestion, "Control must have short help text", string.Format("The control '{0}' does not have a short help text. The short help text is part of the documentation and is displayed in the Documentation web site", item.Name), "Write a short help text.", item);
            }

            if (string.IsNullOrEmpty(item.Help.Text))
            {
                output.Write(SeverityLevel.Suggestion, "Control must have long help text", string.Format("The control '{0}' does not have a long help text. The long help text is part of the documentation and is displayed in the Documentation web site", item.Name), "Write a short help text.", item);
            }
        }
    }
}
