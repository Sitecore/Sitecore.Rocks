// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Xml.Linq;
using Sitecore.Data.Items;
using Sitecore.Diagnostics;

namespace Sitecore.Rocks.Server.Validations.Items
{
    [Validation("Label texts should end with a colon", "SPEAK")]
    public class LabelMustEndWithColon : SpeakRenderingValidation
    {
        public override bool CanCheck(string contextName, Item item, XElement renderingElement, Item renderingItem)
        {
            Assert.ArgumentNotNull(contextName, nameof(contextName));
            Assert.ArgumentNotNull(item, nameof(item));
            Assert.ArgumentNotNull(renderingElement, nameof(renderingElement));
            Assert.ArgumentNotNull(renderingItem, nameof(renderingItem));

            return contextName == "Site" && IsLabelRendering(renderingElement, renderingItem);
        }

        public override void Check(ValidationWriter output, Item item, XElement renderingElement, Item renderingItem)
        {
            Assert.ArgumentNotNull(output, nameof(output));
            Assert.ArgumentNotNull(item, nameof(item));
            Assert.ArgumentNotNull(renderingElement, nameof(renderingElement));
            Assert.ArgumentNotNull(renderingItem, nameof(renderingItem));

            foreach (var field in GetParametersTemplateFields(renderingItem))
            {
                if (!FieldContainsText(field, renderingElement, renderingItem))
                {
                    continue;
                }

                var fieldValue = GetFieldValue(field.Name, renderingElement, renderingItem);
                if (string.IsNullOrEmpty(fieldValue))
                {
                    continue;
                }

                if (fieldValue.EndsWith(":") && !fieldValue.EndsWith(" :"))
                {
                    continue;
                }

                output.Write(SeverityLevel.Warning, "Label texts should end with a colon", string.Format("The label text \"{0}\" should end with a colon [{1}, {2}].", fieldValue, GetFieldValue("Id", renderingElement, renderingItem), field.Name), "Add a colon to the label, without a space between the label and the colon.", item);
            }
        }
    }
}
