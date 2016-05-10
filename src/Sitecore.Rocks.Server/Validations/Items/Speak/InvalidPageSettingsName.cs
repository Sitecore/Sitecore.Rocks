// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Data;
using Sitecore.Data.Items;

namespace Sitecore.Rocks.Server.Validations.Items
{
    [Validation("PageSettings items must be named 'PageSettings'", "SPEAK")]
    public class InvalidPageSettingsName : ViewRenderingValidation
    {
        public static readonly ID PageSettingsTemplateId = new ID("{E35EAA2D-EDEA-431C-A9E6-488272B53782}");

        public override bool CanCheck(string contextName, Item item)
        {
            return contextName == "Site" && IsViewRendering(item);
        }

        public override void Check(ValidationWriter output, Item item)
        {
            if (item.TemplateID != PageSettingsTemplateId)
            {
                return;
            }

            if (item.Name == "PageSettings")
            {
                return;
            }

            output.Write(SeverityLevel.Suggestion, "PageSettings items must be named 'PageSettings'", "By convention PageSettings items must always be named 'PageSettings'", "Rename the item to 'PageSettings'.", item);
        }
    }
}
