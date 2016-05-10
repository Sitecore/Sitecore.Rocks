// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Linq;
using Sitecore.Data;
using Sitecore.Data.Items;
using Sitecore.Diagnostics;

namespace Sitecore.Rocks.Server.Validations.Items.Templates
{
    [Validation("Friendly template field name", "Templates")]
    public class FriendlyTemplateFieldName : ItemValidation
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

            var templateItem = item.Parent.Parent;
            if (templateItem[FieldIDs.BaseTemplate] == ID.Null.ToString())
            {
                return;
            }

            var title = item["Title"];
            if (string.IsNullOrEmpty(title))
            {
                title = item.DisplayName;
            }

            if (string.IsNullOrEmpty(title))
            {
                title = item.Name;
            }

            if (string.IsNullOrEmpty(title))
            {
                return;
            }

            if (title.Contains("_"))
            {
                output.Write(SeverityLevel.Suggestion, "Friendly template field name", string.Format("The template field name \"{0}\" contains an underscore.", title), "Use the \"Title\" field to give the field a friendly name.", item);
                return;
            }

            if (!char.IsLower(title[0]))
            {
                return;
            }

            if (title.IndexOf(' ') >= 0)
            {
                return;
            }

            if (title.IndexOf(' ') >= 0)
            {
                return;
            }

            if (!title.ToCharArray().Any(char.IsUpper))
            {
                return;
            }

            output.Write(SeverityLevel.Hint, "Friendly template field name", string.Format("The template field name \"{0}\" looks like camelCasing.", title), "Use the \"Title\" field to give the field a friendly name.", item);
        }
    }
}
