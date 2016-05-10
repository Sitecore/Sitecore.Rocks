// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Data;
using Sitecore.Data.Items;
using Sitecore.Data.Managers;
using Sitecore.Diagnostics;
using Sitecore.Text;

namespace Sitecore.Rocks.Server.Validations.Items
{
    [Validation("Control has invalid default parameter", "SPEAK")]
    public class InvalidDefaultParameter : ViewRenderingValidation
    {
        public override bool CanCheck(string contextName, Item item)
        {
            Assert.ArgumentNotNull(contextName, nameof(contextName));
            Assert.ArgumentNotNull(item, nameof(item));

            return contextName == "Site" && IsViewRendering(item);
        }

        public override void Check(ValidationWriter output, Item item)
        {
            Assert.ArgumentNotNull(output, nameof(output));
            Assert.ArgumentNotNull(item, nameof(item));

            var parametersTemplateId = item[ParametersTemplateFieldId];
            if (string.IsNullOrEmpty(parametersTemplateId))
            {
                return;
            }

            var template = TemplateManager.GetTemplate(new ID(parametersTemplateId), item.Database);
            if (template == null)
            {
                return;
            }

            var defaultParameters = new UrlString(item["Default Parameters"]);

            foreach (string key in defaultParameters.Parameters.Keys)
            {
                if (string.IsNullOrEmpty(key))
                {
                    continue;
                }

                if (key == "Placeholder")
                {
                    continue;
                }

                if (template.GetField(key) == null)
                {
                    output.Write(SeverityLevel.Warning, "Control has an invalid default parameter", string.Format("The control '{1}' defines the default parameter '{0}', but this parameter does not exist in the controls Parameter Template.", key, item.Name), "Remove the default parameter or add it to the Parameter Template.", item);
                }
            }
        }
    }
}
