// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.IO;
using System.Xml.Linq;
using Sitecore.Diagnostics;
using Sitecore.IO;

namespace Sitecore.Rocks.Server.Validations.Configuration
{
    [Validation("Wellformed include config files", "Configuration")]
    public class WellformedIncludeFiles : Validation
    {
        public override bool CanCheck(string contextName)
        {
            Assert.ArgumentNotNull(contextName, nameof(contextName));

            return contextName == "Site";
        }

        public override void Check(ValidationWriter output)
        {
            Assert.ArgumentNotNull(output, nameof(output));

            LoadConfigFiles(output, MainUtil.MapPath("/App_Config/Include"));
        }

        private void LoadConfigFiles([NotNull] ValidationWriter output, [NotNull] string folder)
        {
            Debug.ArgumentNotNull(output, nameof(output));
            Debug.ArgumentNotNull(folder, nameof(folder));

            foreach (var fileName in Directory.GetFiles(folder, "*.config"))
            {
                if ((File.GetAttributes(fileName) & FileAttributes.Hidden) != 0)
                {
                    continue;
                }

                try
                {
                    XDocument.Load(fileName);
                }
                catch (Exception ex)
                {
                    output.Write(SeverityLevel.Error, "Wellformed include config files", string.Format("The include config file \"{0}\" is not wellformed: {1}", FileUtil.UnmapPath(fileName, false), ex.Message), "Correct the error or delete the file.");
                }
            }

            foreach (var subfolder in Directory.GetDirectories(folder))
            {
                LoadConfigFiles(output, subfolder);
            }
        }
    }
}
