// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Rocks.Commands;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Shell;

namespace Sitecore.Rocks.Projects.Commands.Registering
{
    [Command, ShellMenuCommand(CommandIds.RegisterModel)]
    public class RegisterModel : RegisterFileBase
    {
        public RegisterModel()
        {
            FileExtension = ".cs";
            DialogTitle = "Register Model";
            DefaultItemPath = "/sitecore/layout/Models";
            TemplateItemPath = "{FED6A14F-0D05-4E18-B160-17C0588A2005}";
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

            ItemModifier.Edit(itemUri, "Model Type", className);
        }
    }
}
