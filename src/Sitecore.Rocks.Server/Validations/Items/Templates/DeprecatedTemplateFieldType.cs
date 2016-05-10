// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using Sitecore.Data.Items;
using Sitecore.Diagnostics;

namespace Sitecore.Rocks.Server.Validations.Items.Templates
{
    [Validation("Deprecated template field type", "Templates")]
    public class DeprecatedTemplateFieldType : ItemValidation
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

            if (item.Paths.Path.StartsWith("/sitecore/templates/system", StringComparison.InvariantCultureIgnoreCase))
            {
                return;
            }

            var type = item["Type"];
            if (string.IsNullOrEmpty(type))
            {
                type = item.DisplayName;
            }

            string newType = null;

            switch (type.ToLowerInvariant())
            {
                case "text":
                    newType = "Single-Line Text";
                    break;
                case "html":
                    newType = "Rich Text";
                    break;
                case "link":
                    newType = "General Link";
                    break;
                case "lookup":
                    newType = "Droplink";
                    break;
                case "memo":
                    newType = "Multi-Line Text";
                    break;
                case "reference":
                    newType = "Droptree";
                    break;
                case "server file":
                    newType = "Single-Line Text";
                    break;
                case "tree":
                    newType = "Droptree";
                    break;
                case "treelist":
                    newType = "TreelistEx";
                    break;
                case "valuelookup":
                    newType = "Droplist";
                    break;
            }

            if (string.IsNullOrEmpty(newType))
            {
                return;
            }

            output.Write(SeverityLevel.Suggestion, "Deprecated template field type", string.Format("The template field type \"{0}\" is deprecated.", type), string.Format("Use the \"{0}\" field type.", newType), item);
        }
    }
}
