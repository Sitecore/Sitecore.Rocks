// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.Runtime.InteropServices;
using EnvDTE80;
using Microsoft.VisualStudio.Shell;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.CodeGeneration.CodeFirstTemplates.Pipelines.ProcessCodeFirstTemplates;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Sites;

namespace Sitecore.Rocks.CodeGeneration.CodeFirstTemplates
{
    [ComVisible(true), Guid(@"2774f035-c60c-49ae-a0fb-a8b1dc10ee72"), CustomToolRegistration(@"CodeFirstTemplate", typeof(FileCodeModelCustomTool)), ProvideObject(typeof(FileCodeModelCustomTool))]
    public class FileCodeModelCustomTool : CustomTool
    {
        protected override string GetOutput()
        {
            var site = GetSite(InputFilePath);
            if (site == Sites.Site.Empty)
            {
                return "// The Visual Studio project is not connected to a Sitecore web site. Right-click the project, select Sitecore and Connect.";
            }

            var projectItem = SitecorePackage.Instance.Dte.Solution.FindProjectItem(InputFilePath);
            if (projectItem == null)
            {
                return "// Project Item not found";
            }

            var fileCodeModel = projectItem.FileCodeModel as FileCodeModel2;
            if (fileCodeModel == null)
            {
                return "// File Code Model not found";
            }

            try
            {
                var model = new FileCodeModel(fileCodeModel);

                var pipeline = ProcessCodeFirstTemplatesPipeline.Run().WithParameters(site, model);

                return pipeline.DesignerFile;
            }
            catch (Exception ex)
            {
                return string.Format("/* An exception occured:\r\n{0}\r\n{1}\r\n*/", ex.Message, ex.StackTrace);
            }
        }

        [NotNull]
        private Site GetSite([NotNull] string fileName)
        {
            Debug.ArgumentNotNull(fileName, nameof(fileName));

            foreach (var site in AppHost.Sites)
            {
                if (string.IsNullOrEmpty(site.WebRootPath))
                {
                    continue;
                }

                if (fileName.StartsWith(site.WebRootPath, StringComparison.InvariantCultureIgnoreCase))
                {
                    return site;
                }
            }

            return Sites.Site.Empty;
        }
    }
}
