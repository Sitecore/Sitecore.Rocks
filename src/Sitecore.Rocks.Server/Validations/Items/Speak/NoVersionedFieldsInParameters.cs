// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Web;
using System.Xml.Linq;
using Sitecore.Data.Items;
using Sitecore.Diagnostics;
using Sitecore.Rocks.Server.Extensions.XElementExtensions;
using Sitecore.Text;

namespace Sitecore.Rocks.Server.Validations.Items
{
    [Validation("Versionable parameters should be specified in the configuration items", "SPEAK")]
    public class NoVersionedFieldsInParameters : SpeakRenderingValidation
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

            var parameters = new UrlString(renderingElement.GetAttributeValue("par"));

            foreach (var field in GetParametersTemplateFields(renderingItem))
            {
                if (!FieldContainsText(field, renderingElement, renderingItem))
                {
                    continue;
                }

                var result = HttpUtility.UrlDecode(parameters[field.Name]);
                if (string.IsNullOrEmpty(result))
                {
                    continue;
                }

                output.Write(SeverityLevel.Warning, "Versionable parameters should be specified in the configuration items.", string.Format("The field \"{0}\" is versionable and is specified in the layout [{1}].", field.Name, GetFieldValue("Id", renderingElement, renderingItem)), "Enter all UI text in the standard item fields of configuration items.", item);
            }
        }
    }
}
