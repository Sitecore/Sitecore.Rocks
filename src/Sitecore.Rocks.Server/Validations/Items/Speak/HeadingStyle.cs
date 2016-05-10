// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Xml.Linq;
using Sitecore.Data.Items;
using Sitecore.Diagnostics;

namespace Sitecore.Rocks.Server.Validations.Items
{
    [Validation("Heading text should be written in heading style", "SPEAK")]
    public class HeadingStyle : SpeakRenderingValidation
    {
        public override bool CanCheck(string contextName, Item item, XElement renderingElement, Item renderingItem)
        {
            Assert.ArgumentNotNull(contextName, nameof(contextName));
            Assert.ArgumentNotNull(item, nameof(item));
            Assert.ArgumentNotNull(renderingElement, nameof(renderingElement));
            Assert.ArgumentNotNull(renderingItem, nameof(renderingItem));

            return contextName == "Site" && IsHeadingRendering(renderingElement, renderingItem);
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

                if (IsHeadingStyle(fieldValue))
                {
                    continue;
                }

                output.Write(SeverityLevel.Warning, "Heading text should be written in heading style", string.Format("The heading text \"{0}\"is not written in heading style [{1}, {2}].", fieldValue, GetFieldValue("Id", renderingElement, renderingItem), field.Name), "Write all headings in heading style using heading capitalization. This means that the first letter of all nouns, verbs, and adverbs (but not articles, conjunctions, or prepositions) are capitalized. For example, The Quick Brown Fox Jumps over the Lazy Dog.", item);
            }
        }
    }
}
