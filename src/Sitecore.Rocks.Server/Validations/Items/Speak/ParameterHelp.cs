// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Linq;
using Sitecore.Data.Items;
using Sitecore.Diagnostics;

namespace Sitecore.Rocks.Server.Validations.Items
{
    [Validation("Parameter must have help text", "SPEAK")]
    public class ParameterHelp : ViewRenderingValidation
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

            var parameterTemplateItem = item.Database.GetItem(parametersTemplateId);
            if (parameterTemplateItem == null)
            {
                return;
            }

            var template = new TemplateItem(parameterTemplateItem);

            foreach (var templateFieldItem in template.Fields)
            {
                var templateItem = new TemplateItem(templateFieldItem.InnerItem.Parent.Parent);
                if (!templateItem.BaseTemplates.Any())
                {
                    continue;
                }

                if (string.IsNullOrEmpty(templateFieldItem.ToolTip))
                {
                    output.Write(SeverityLevel.Suggestion, "Parameter must have help text", string.Format("The parameter '{0}' in the '{1}' control does not have a short help text. The short help text is part of documentation and is displayed in the Documentation web site", templateFieldItem.Name, item.Name), "Write a help text.", templateFieldItem.InnerItem);
                }
            }
        }
    }
}
