// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using Sitecore.Data.Fields;
using Sitecore.Data.Items;
using Sitecore.Diagnostics;

namespace Sitecore.Rocks.Server.Validations.Items.Fields
{
    [Validation("Lorem Ipsum text", "Fields", ExecutePerLanguage = true)]
    public class LoremIpsumText : ItemValidation
    {
        public override bool CanCheck(string contextName, Item item)
        {
            Assert.ArgumentNotNull(contextName, nameof(contextName));
            Assert.ArgumentNotNull(item, nameof(item));

            return contextName == "Site";
        }

        public override void Check(ValidationWriter output, Item item)
        {
            Assert.ArgumentNotNull(output, nameof(output));
            Assert.ArgumentNotNull(item, nameof(item));

            foreach (Field field in item.Fields)
            {
                if (field.Value.IndexOf("Lorem Ipsum", StringComparison.InvariantCultureIgnoreCase) < 0)
                {
                    continue;
                }

                output.Write(SeverityLevel.Suggestion, "Lorem Ipsum text", string.Format("The field \"{0}\" contains the test data text: \"Lorem Ipsum...\".", field.Name), "Replace or remove the text data.", item);
            }
        }
    }
}
