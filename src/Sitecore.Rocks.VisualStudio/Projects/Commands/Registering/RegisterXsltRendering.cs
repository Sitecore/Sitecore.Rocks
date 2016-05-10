// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Rocks.Commands;
using Sitecore.Rocks.Shell;

namespace Sitecore.Rocks.Projects.Commands.Registering
{
    [Command, ShellMenuCommand(CommandIds.RegisterXsltRendering)]
    public class RegisterXsltRendering : RegisterFileBase
    {
        public RegisterXsltRendering()
        {
            FileExtension = ".xslt";
            DialogTitle = "Register Xsl Rendering";
            DefaultItemPath = "/sitecore/layout/renderings";
            TemplateItemPath = "/sitecore/templates/System/Layout/Renderings/Xsl Rendering";
        }
    }
}
