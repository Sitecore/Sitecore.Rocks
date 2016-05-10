// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Collections.Generic;
using System.Linq;
using Sitecore.Rocks.Commands;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.UI.Toolbars;

namespace Sitecore.Rocks.ContentEditors.Commands.Versions
{
    [Command(ExcludeFromSearch = true), ToolbarElement(typeof(ContentEditorContext), 1510, "Home", "Versions", Icon = "Resources/32x32/Versions.png", ElementType = RibbonElementType.LargeDropDown)]
    public class VersionsSubmenu : CommandBase, IToolbarElement
    {
        public VersionsSubmenu()
        {
            Text = Resources.Versions;
            Group = "Versions";
            SortingValue = 3100;
        }

        public override bool CanExecute(object parameter)
        {
            var context = parameter as ContentEditorContext;
            if (context == null)
            {
                return false;
            }

            if (context.ContentEditor.ContentModel.IsEmpty)
            {
                return false;
            }

            return true;
        }

        public override void Execute(object parameter)
        {
        }

        public override IEnumerable<ICommand> GetSubmenuCommands(object parameter)
        {
            var context = parameter as ContentEditorContext;
            if (context == null)
            {
                return Enumerable.Empty<ICommand>();
            }

            var result = new List<ICommand>(CommandManager.GetCommands(parameter, "Versions"));

            if (context.ContentEditor.ContentModel.IsMultiple || context.ContentEditor.ContentModel.IsEmpty)
            {
                return result;
            }

            var item = context.ContentEditor.ContentModel.FirstItem;

            var selectedVersion = item.Uri.Version;
            var sortingValue = 10000;

            foreach (var versionNumber in item.Versions)
            {
                var version = new Version(versionNumber);

                var command = new SetVersion(version)
                {
                    Text = version.ToString(),
                    IsChecked = version == selectedVersion,
                    SortingValue = sortingValue
                };

                result.Add(command);

                sortingValue--;
            }

            return result;
        }
    }
}
