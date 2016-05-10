// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Rocks.Commands;
using Sitecore.Rocks.Shell;

namespace Sitecore.Rocks.Projects.Commands.Registering
{
    [Command, ShellMenuCommand(CommandIds.RegisterViewRendering)]
    public class RegisterViewRendering : RegisterFileBase
    {
        public RegisterViewRendering()
        {
            FileExtension = ".cshtml";
            DialogTitle = "Register View Rendering...";
            DefaultItemPath = "/sitecore/layout/renderings";
            TemplateItemPath = "{99F8905D-4A87-4EB8-9F8B-A9BEBFB3ADD6}";
        }
    }
}
