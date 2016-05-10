// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Data.Items;
using Sitecore.Diagnostics;

namespace Sitecore.Rocks.Server.Validations.Items
{
    [Validation("Empty PageSettings items can safely be deleted", "SPEAK")]
    public class EmptyPageSettings : ViewRenderingValidation
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

            if (item.TemplateID != InvalidPageSettingsName.PageSettingsTemplateId)
            {
                return;
            }

            if (item.HasChildren)
            {
                return;
            }

            output.Write(SeverityLevel.Suggestion, "Empty PageSettings items can be deleted", "PageSettings items, that do not have any children, are not needed and can safely be deleted.", "Delete the item.", item);
        }
    }
}
