// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Data.Items;
using Sitecore.Diagnostics;

namespace Sitecore.Rocks.Server.Validations.Items
{
    [Validation("Control does not have an Parameter Template", "SPEAK")]
    public class MissingParametersTemplate : ViewRenderingValidation
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
                output.Write(SeverityLevel.Warning, "Control does not have a Parameter Template", string.Format("The control '{0}' does not have a Parameter Template. Parameter Templates define the parameters that the control accepts. These help the user set parameters correctly and appear in Property Windows when configuring the control in Sitecore Rocks or the Page Editor.", item.Name), "Create an Parameter Template item and set the Parameter Template field in the control.", item);
                return;
            }

            var parameterTemplate = item.Database.GetItem(parametersTemplateId);
            if (parameterTemplate == null)
            {
                output.Write(SeverityLevel.Error, "Parameter Template does not exist", string.Format("The control '{0}' points to a Parameter Templates item that does not exist.", item.Name), "Create an Parameter Template item or remove the reference.", item);
            }
        }
    }
}
