// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using Sitecore.Data;
using Sitecore.Data.Items;
using Sitecore.Diagnostics;
using Sitecore.IO;

namespace Sitecore.Rocks.Server.Validations.Items.Presentation
{
    [Validation("Valid Xslt Rendering file", "Presentation")]
    public class XsltFiles : ItemValidation
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

            if (item.TemplateID != TemplateIDs.XSLRendering)
            {
                return;
            }

            if (StandardValuesManager.IsStandardValuesHolder(item))
            {
                return;
            }

            if (item.Paths.Path.StartsWith("/sitecore/templates/branches", StringComparison.InvariantCultureIgnoreCase))
            {
                return;
            }

            var path = item["Path"];
            if (string.IsNullOrEmpty(path))
            {
                return;
            }

            if (path.EndsWith("$name.xslt", StringComparison.InvariantCultureIgnoreCase))
            {
                return;
            }

            if (FileUtil.Exists(path))
            {
                return;
            }

            output.Write(SeverityLevel.Error, "Valid Xslt Rendering file", string.Format("The \"Path\" field points to the non-existing file: {0}", path), "Either point the \"Path\" field to another file or create a new file.", item);
        }
    }
}
