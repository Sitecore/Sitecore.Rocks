// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using Sitecore.Configuration;
using Sitecore.Diagnostics;
using Sitecore.IO;

namespace Sitecore.Rocks.Server.Validations.Security
{
    [Validation("Data folder location", "Security")]
    public class DataFolderLocation : Validation
    {
        public override bool CanCheck(string contextName)
        {
            Assert.ArgumentNotNull(contextName, nameof(contextName));

            return contextName == "Site";
        }

        public override void Check(ValidationWriter output)
        {
            Assert.ArgumentNotNull(output, nameof(output));

            if (!FileUtil.MapPath(Settings.DataFolder).StartsWith(FileUtil.MapPath("/"), StringComparison.InvariantCultureIgnoreCase))
            {
                return;
            }

            output.Write(SeverityLevel.Warning, "Data folder location", string.Format("The Data folder is located inside the web site root in \"{0}\". This is a potential security risk.", Settings.DataFolder), "Move the Data folder outside the web site root and change the \"DataFolder\" setting in the web.config to point to the new location.");
        }
    }
}
