// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.Xml.Linq;
using Sitecore.Data.Items;
using Sitecore.Diagnostics;

namespace Sitecore.Rocks.Server.Validations.Items
{
    [Validation("HTML code should not be embedded in the UI text labels", "SPEAK")]
    public class DoNotUseHtmlInLabels : SpeakRenderingValidation
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

                if (fieldValue.IndexOf("<", StringComparison.InvariantCultureIgnoreCase) < 0)
                {
                    continue;
                }

                if (fieldValue.IndexOf(">", StringComparison.InvariantCultureIgnoreCase) < 0)
                {
                    continue;
                }

                output.Write(SeverityLevel.Warning, "HTML code should not be embedded in the UI text labels.", string.Format("The label text \"{0}\" has Html code embedded [{1}, {2}].", fieldValue, GetFieldValue("Id", renderingElement, renderingItem), field.Name), "Remove HTML code from labels and only use the SPEAK style sheet for UI labels.", item);
            }
        }
    }
}
