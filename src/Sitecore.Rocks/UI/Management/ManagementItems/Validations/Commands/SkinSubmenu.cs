// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.Collections.Generic;
using System.Linq;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Commands;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.UI.Management.ManagementItems.Validations.Skins;

namespace Sitecore.Rocks.UI.Management.ManagementItems.Validations.Commands
{
    [Command(ExcludeFromSearch = true)]
    public class SkinSubmenu : CommandBase, IComparer<ValidationViewerSkinManager.SkinDescriptor>
    {
        public SkinSubmenu()
        {
            Text = Resources.ViewsSubmenu_ViewsSubmenu_View;
            Group = "Views";
            SortingValue = 5000;
        }

        public override bool CanExecute(object parameter)
        {
            return parameter is ValidationContext;
        }

        public int Compare([NotNull] ValidationViewerSkinManager.SkinDescriptor x, [NotNull] ValidationViewerSkinManager.SkinDescriptor y)
        {
            Assert.ArgumentNotNull(x, nameof(x));
            Assert.ArgumentNotNull(y, nameof(y));

            if (x.Priority < y.Priority)
            {
                return -1;
            }

            if (x.Priority > y.Priority)
            {
                return 1;
            }

            return x.SkinName.CompareTo(y.SkinName);
        }

        public override void Execute(object parameter)
        {
        }

        public override IEnumerable<ICommand> GetSubmenuCommands(object parameter)
        {
            Assert.ArgumentNotNull(parameter, nameof(parameter));

            var context = parameter as ValidationContext;
            if (context == null)
            {
                return Enumerable.Empty<ICommand>();
            }

            var result = new List<ICommand>();

            result.AddRange(CommandManager.GetCommands(parameter, "Views"));

            var list = new List<ValidationViewerSkinManager.SkinDescriptor>(ValidationViewerSkinManager.Skins.Values);

            list.Sort(this);

            var n = 0;
            foreach (var type in list)
            {
                var skinCommand = new SetValidationViewerSkin
                {
                    SkinName = type.SkinName,
                    Text = type.SkinName,
                    IsChecked = string.Compare(type.SkinName, context.ValidationViewer.SkinName, StringComparison.InvariantCultureIgnoreCase) == 0,
                    SortingValue = 3000 + n
                };

                result.Add(skinCommand);

                n++;
            }

            return result;
        }
    }
}
