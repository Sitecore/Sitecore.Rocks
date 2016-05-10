// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Rocks.Commands;
using Sitecore.Rocks.Shell;

namespace Sitecore.Rocks.Projects.Commands.Registering
{
    [Command, ShellMenuCommand(CommandIds.RegisterLayout)]
    public class RegisterLayout : RegisterFileBase
    {
        public RegisterLayout()
        {
            FileExtension = ".aspx";
            DialogTitle = "Register Layout";
            DefaultItemPath = "/sitecore/layout/layouts";
            TemplateItemPath = "/sitecore/templates/System/Layout/Layout";
        }
    }
}
