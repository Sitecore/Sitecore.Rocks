// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.Collections.Generic;
using System.Linq;
using Sitecore.Rocks.Commands;
using Sitecore.Rocks.ContentEditors.Skins;
using Sitecore.Rocks.Diagnostics;

namespace Sitecore.Rocks.ContentEditors.Commands.Views
{
    [Command(ExcludeFromSearch = true)]
    public class ViewSubmenu : CommandBase
    {
        public const string Name = "Views";

        public ViewSubmenu()
        {
            Text = Resources.ViewsSubmenu_ViewsSubmenu_View;
            Group = "Views";
            SortingValue = 9000;
        }

        public override bool CanExecute(object parameter)
        {
            return parameter is ContentEditorContext;
        }

        public override void Execute(object parameter)
        {
        }

        public override IEnumerable<ICommand> GetSubmenuCommands(object parameter)
        {
            Assert.ArgumentNotNull(parameter, nameof(parameter));

            var context = parameter as ContentEditorContext;
            if (context == null)
            {
                return Enumerable.Empty<ICommand>();
            }

            var result = new List<ICommand>();

            result.AddRange(CommandManager.GetCommands(parameter, Name));

            foreach (var type in SkinManager.Types)
            {
                var skinCommand = new SetSkin(type.Key)
                {
                    Text = type.Key,
                    IsChecked = string.Compare(type.Key, context.ContentEditor.AppearanceOptions.SkinName, StringComparison.InvariantCultureIgnoreCase) == 0,
                    SortingValue = 3000
                };

                result.Add(skinCommand);
            }

            return result;
        }
    }
}
