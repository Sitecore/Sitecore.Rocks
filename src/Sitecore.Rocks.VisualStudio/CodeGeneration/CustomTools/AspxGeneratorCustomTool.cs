// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.IO;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio.Shell;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Data.DataServices;

namespace Sitecore.Rocks.CodeGeneration.CustomTools
{
    [ComVisible(true), Guid(@"503f1375-630b-4e2c-8d07-b6e2ec78eb49"), CustomToolRegistration(@"SitecoreServerPageGenerator", typeof(AspxGeneratorCustomTool)), ProvideObject(typeof(AspxGeneratorCustomTool))]
    public class AspxGeneratorCustomTool : CustomTool
    {
        protected override string GetOutput()
        {
            var site = GetSite();
            if (site == null)
            {
                return "// The Visual Studio project is not connected to a Sitecore web site. Right-click the project, select Sitecore and Connect.";
            }

            string result = null;

            ExecuteCompleted completed = delegate(string response, ExecuteResult executeResult)
            {
                if (!DataService.HandleExecute(response, executeResult))
                {
                    result = string.Empty;
                    return;
                }

                result = response ?? string.Empty;
            };

            site.DataService.ExecuteAsync("CodeGeneration.AspxGenerator", completed, InputFileContents, Path.GetFileName(InputFilePath), string.Empty);

            while (result == null)
            {
                AppHost.DoEvents();
            }

            return result;
        }
    }
}
