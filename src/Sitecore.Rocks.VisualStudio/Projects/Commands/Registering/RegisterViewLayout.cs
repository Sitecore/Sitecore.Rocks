// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Rocks.Commands;
using Sitecore.Rocks.Shell;

namespace Sitecore.Rocks.Projects.Commands.Registering
{
    [Command, ShellMenuCommand(CommandIds.RegisterViewLayout)]
    public class RegisterViewLayout : RegisterFileBase
    {
        public RegisterViewLayout()
        {
            FileExtension = ".cshtml";
            DialogTitle = "Register View Layout";
            DefaultItemPath = "/sitecore/layout/layouts";
            TemplateItemPath = "/sitecore/templates/System/Layout/Layout";
        }
    }
}
