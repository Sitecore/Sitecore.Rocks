// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Data.Items;
using Sitecore.Diagnostics;
using Sitecore.IO;

namespace Sitecore.Rocks.Server.Validations.Items
{
    // [Validation("Control does not have an Jasmine BDD test file", "SPEAK")]
    public class MissingBddTestFile : ViewRenderingValidation
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

            var fileName = "/sitecore/shell/client/test/renderings/" + item.Parent.Name + "/" + item.Name + ".test.js";
            if (FileUtil.FileExists(fileName))
            {
                return;
            }

            output.Write(SeverityLevel.Suggestion, "Control does not have an Jasmine BDD test file", string.Format("The control '{0}' does not have an Jasmine BDD test file. All controls must have an example.", item.Name), "Create an Jasmine BDD test file in this location: " + fileName, item);
        }
    }
}
