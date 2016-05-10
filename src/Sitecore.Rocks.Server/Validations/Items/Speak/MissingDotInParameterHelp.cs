// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Linq;
using Sitecore.Data.Items;

namespace Sitecore.Rocks.Server.Validations.Items
{
    [Validation("Parameter help text must end with a dot", "SPEAK")]
    public class MissingDotInParameterHelp : ViewRenderingValidation
    {
        public override bool CanCheck(string contextName, Item item)
        {
            return contextName == "Site" && IsViewRendering(item);
        }

        public override void Check(ValidationWriter output, Item item)
        {
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

                if (!string.IsNullOrEmpty(templateFieldItem.ToolTip) && !templateFieldItem.ToolTip.EndsWith("."))
                {
                    output.Write(SeverityLevel.Suggestion, "Parameter help text must end with a dot", string.Format("The help text for the '{0}' parameter in the control '{1}' must be a complete sentence and end with a dot.", templateFieldItem.Name, item.Name), "Add a dot to the help text.", templateFieldItem.InnerItem);
                }
            }
        }
    }
}
