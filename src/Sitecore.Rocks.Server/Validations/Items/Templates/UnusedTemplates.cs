// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using Sitecore.Data.Items;
using Sitecore.Diagnostics;

namespace Sitecore.Rocks.Server.Validations.Items.Templates
{
    [Validation("Unused template", "Templates")]
    public class UnusedTemplates : ItemValidation
    {
        public override bool CanCheck(string contextName, Item item)
        {
            Assert.ArgumentNotNull(contextName, nameof(contextName));
            Assert.ArgumentNotNull(item, nameof(item));

            return contextName == "Site";
        }

        public override void Check(ValidationWriter output, Item item)
        {
            Assert.ArgumentNotNull(output, nameof(output));
            Assert.ArgumentNotNull(item, nameof(item));

            if (item.TemplateID != TemplateIDs.Template)
            {
                return;
            }

            if (item.Paths.Path.StartsWith("/sitecore/templates/system", StringComparison.InvariantCultureIgnoreCase))
            {
                return;
            }

            var count = Globals.LinkDatabase.GetReferrerCount(item);

            if (count == 0)
            {
                output.Write(SeverityLevel.Suggestion, "Unused template", string.Format("The template \"{0}\" is not used by any items and can be deleted.", item.Name), "Delete the template.", item);
            }
        }
    }
}
