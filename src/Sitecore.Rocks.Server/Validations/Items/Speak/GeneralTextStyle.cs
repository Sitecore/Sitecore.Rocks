// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Xml.Linq;
using Sitecore.Data.Items;
using Sitecore.Diagnostics;

namespace Sitecore.Rocks.Server.Validations.Items
{
    [Validation("General text should be written in sentence style", "SPEAK")]
    public class GeneralTextStyle : SpeakRenderingValidation
    {
        public override bool CanCheck(string contextName, Item item, XElement renderingElement, Item renderingItem)
        {
            Assert.ArgumentNotNull(contextName, nameof(contextName));
            Assert.ArgumentNotNull(item, nameof(item));
            Assert.ArgumentNotNull(renderingElement, nameof(renderingElement));
            Assert.ArgumentNotNull(renderingItem, nameof(renderingItem));

            return contextName == "Site" && !IsHeadingRendering(renderingElement, renderingItem);
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

                if (IsSentenceStyle(fieldValue))
                {
                    continue;
                }

                output.Write(SeverityLevel.Warning, "General text should be written in sentence style", string.Format("The general text \"{0}\" is not written in sentence style [{1}, {2}].", fieldValue, GetFieldValue("Id", renderingElement, renderingItem), field.Name), "If the text is not being used as a heading, it must be written in sentence style. This means that only the first letter of the first word of each sentence is capitalized. For example, The quick brown fox jumps over the lazy dog.", item);
            }
        }
    }
}
