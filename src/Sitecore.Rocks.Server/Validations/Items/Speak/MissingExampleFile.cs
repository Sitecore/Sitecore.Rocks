// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Data.Items;
using Sitecore.IO;

namespace Sitecore.Rocks.Server.Validations.Items
{
    // [Validation("Control does not have an Example file", "SPEAK")]
    public class MissingExampleFile : ViewRenderingValidation
    {
        public override bool CanCheck(string contextName, Item item)
        {
            return contextName == "Site" && IsViewRendering(item);
        }

        public override void Check(ValidationWriter output, Item item)
        {
            var fileName = "/sitecore/shell/client/Sitecore/Content/Speak/Documentation/Examples/" + item.Name + ".cshtml";
            if (FileUtil.FileExists(fileName))
            {
                return;
            }

            output.Write(SeverityLevel.Suggestion, "Control does not have an Example file", string.Format("The control '{0}' does not have an example file in the Documentation web site. All controls must have an example.", item.Name), "Create an Example file in this location: " + fileName, item);
        }
    }
}
