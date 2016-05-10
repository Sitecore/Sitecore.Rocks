// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Rocks.Commands;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Shell;

namespace Sitecore.Rocks.Projects.Commands.Registering
{
    [Command, ShellMenuCommand(CommandIds.RegisterController)]
    public class RegisterController : RegisterFileBase
    {
        public RegisterController()
        {
            FileExtension = ".cs";
            DialogTitle = "Register Controller Action";
            DefaultItemPath = "/sitecore/layout/Controllers";
            TemplateItemPath = "{473F22C2-3C94-4D1D-B9AE-3D1FC3D3D3F2}";
        }

        protected override void UpdateFields(EnvDTE.ProjectItem projectItem, ItemUri itemUri, string fileName)
        {
            Debug.ArgumentNotNull(projectItem, nameof(projectItem));
            Debug.ArgumentNotNull(itemUri, nameof(itemUri));
            Debug.ArgumentNotNull(fileName, nameof(fileName));

            var className = GetFirstClass(projectItem);
            if (string.IsNullOrEmpty(className))
            {
                return;
            }

            ItemModifier.Edit(itemUri, "Controller Name", className);
        }
    }
}
