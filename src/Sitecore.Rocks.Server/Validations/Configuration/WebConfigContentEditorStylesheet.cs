// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Configuration;
using Sitecore.Diagnostics;
using Sitecore.IO;

namespace Sitecore.Rocks.Server.Validations.Configuration
{
    [Validation("Valid content editor stylesheet file", "Configuration")]
    public class WebConfigContentEditorStylesheet : Validation
    {
        public override bool CanCheck(string contextName)
        {
            Assert.ArgumentNotNull(contextName, nameof(contextName));

            return contextName == "Site";
        }

        public override void Check(ValidationWriter output)
        {
            Assert.ArgumentNotNull(output, nameof(output));

            if (string.IsNullOrEmpty(Settings.WebEdit.ContentEditorStylesheet))
            {
                return;
            }

            if (FileUtil.Exists(Settings.WebEdit.ContentEditorStylesheet))
            {
                return;
            }

            output.Write(SeverityLevel.Suggestion, "Valid content editor web stylesheet file", string.Format("The \"WebEdit.ContentEditorStylesheet\" setting in the web.config points to the non-existing file: \"{0}\"", Settings.WebEdit.ContentEditorStylesheet), "Either create the file or set the setting \"WebEdit.ContentEditorStylesheet\" to blank.");
        }
    }
}
