// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Rocks.Commands;
using Sitecore.Rocks.Shell;

namespace Sitecore.Rocks.Projects.Commands.Registering
{
    [Command, ShellMenuCommand(CommandIds.RegisterSublayout)]
    public class RegisterSublayout : RegisterFileBase
    {
        public RegisterSublayout()
        {
            FileExtension = ".ascx";
            DialogTitle = "Register Sublayout";
            DefaultItemPath = "/sitecore/layout/sublayouts";
            TemplateItemPath = "/sitecore/templates/System/Renderings/Sublayout";
        }
    }
}
