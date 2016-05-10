// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Linq;
using System.Xml.Linq;
using Sitecore.Data.Items;
using Sitecore.Diagnostics;

namespace Sitecore.Rocks.Server.Validations.Items
{
    [Validation("Do not use all caps", "SPEAK")]
    public class DoNotUseUppercase : SpeakRenderingValidation
    {
        public override bool CanCheck(string contextName, Item item, XElement renderingElement, Item renderingItem)
        {
            Assert.ArgumentNotNull(contextName, nameof(contextName));
            Assert.ArgumentNotNull(item, nameof(item));
            Assert.ArgumentNotNull(renderingElement, nameof(renderingElement));
            Assert.ArgumentNotNull(renderingItem, nameof(renderingItem));

            return contextName == "Site";
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

                if (fieldValue == "$itemName")
                {
                    continue;
                }

                if (!fieldValue.Where((t, n) => char.IsLetter(fieldValue, n)).Any())
                {
                    continue;
                }

                if (fieldValue == fieldValue.ToUpperInvariant())
                {
                    output.Write(SeverityLevel.Warning, "Do not use all caps", string.Format("The text \"{0}\" have hard coded all caps (all capital letters or upper case text) in the UI. All caps is controlled by the style sheet [{1}, {2}].", fieldValue, GetFieldValue("Id", renderingElement, renderingItem), field.Name), "Write all UI text in either heading or sentence style.", item);
                }
            }
        }
    }
}
