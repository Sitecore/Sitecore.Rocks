// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Data.Items;
using Sitecore.Diagnostics;

namespace Sitecore.Rocks.Server.Validations.Items.Templates
{
    [Validation("Use IDs instead of paths in template fields", "Templates")]
    public class UseIdInsteadOfPaths : ItemValidation
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

            if (item.TemplateID != TemplateIDs.TemplateField)
            {
                return;
            }

            var source = item[FieldIDs.Source];
            if (string.IsNullOrEmpty(source))
            {
                return;
            }

            if (!source.StartsWith("/sitecore"))
            {
                return;
            }

            var sourceItem = item.Database.GetItem(source);
            if (sourceItem == null)
            {
                return;
            }

            output.Write(SeverityLevel.Suggestion, "Use IDs instead of paths in template fields", string.Format("The template field Source field contains the path \"{0}\". It is recommended to use IDs instead.", source), string.Format("Replace the path with the ID \"{0}\".", sourceItem.ID.ToString()), item);
        }
    }
}
