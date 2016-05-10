// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Runtime.InteropServices;
using Microsoft.VisualStudio.Shell;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Data.DataServices;

namespace Sitecore.Rocks.CodeGeneration.CustomTools
{
    [ComVisible(true), Guid(@"e5ca4e64-824f-495f-ad82-ef6a6e89ca12"), CustomToolRegistration(@"SitecoreServerCodeGenerator", typeof(CompiledCodeGeneratorCustomTool)), ProvideObject(typeof(CompiledCodeGeneratorCustomTool))]
    public class CompiledCodeGeneratorCustomTool : CustomTool
    {
        [CanBeNull]
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

            site.DataService.ExecuteAsync("CodeGeneration.CompiledCodeGenerator", completed, InputFileContents);

            while (result == null)
            {
                AppHost.DoEvents();
            }

            return result;
        }
    }
}
