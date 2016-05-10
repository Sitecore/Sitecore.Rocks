// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Microsoft.VisualStudio.TextTemplating;
using Microsoft.VisualStudio.TextTemplating.VSHost;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Diagnostics;

namespace Sitecore.Rocks.CodeGeneration.T4
{
    public class TextTemplating
    {
        [NotNull]
        public string Process([NotNull] string templateFileName)
        {
            Assert.ArgumentNotNull(templateFileName, nameof(templateFileName));

            var t4 = SitecorePackage.Instance.GetService<STextTemplating>() as ITextTemplating;
            var sessionHost = t4 as ITextTemplatingSessionHost;
            if (sessionHost == null)
            {
                return "// Failed to instantiate Text Templating Engine";
            }

            var templateContents = AppHost.Files.ReadAllText(templateFileName);

            sessionHost.Session = sessionHost.CreateSession();

            // sessionHost.Session["fileCodeModel"] = new FileCodeModel(fileCodeModel);
            return t4.ProcessTemplate(templateFileName, templateContents);
        }
    }
}
