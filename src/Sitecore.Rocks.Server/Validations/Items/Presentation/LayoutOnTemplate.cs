// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Data.Items;
using Sitecore.Diagnostics;
using Sitecore.Rocks.Server.Pipelines.GetFieldValue;

namespace Sitecore.Rocks.Server.Validations.Items.Presentation
{
    [Validation("Layout on template", "Presentation")]
    public class LayoutOnTemplate : ItemValidation
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

            var layoutField = item.Fields[FieldIDs.LayoutField];
            if (layoutField.ContainsStandardValue)
            {
                return;
            }

            var value = GetFieldValuePipeline.Run().WithParameters(layoutField).Value;
            if (string.IsNullOrEmpty(value))
            {
                return;
            }

            output.Write(SeverityLevel.Suggestion, "Layout on template", "Assigning layouts on templates are no longer recommended.", "Assign the layout on the standard values item.", item);
        }
    }
}
