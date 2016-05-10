// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.Collections.Generic;
using System.Linq;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Commands;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Media.Skins;

namespace Sitecore.Rocks.Media.Commands
{
    [Command(ExcludeFromSearch = true)]
    public class SkinSubmenu : CommandBase, IComparer<MediaSkinManager.MediaSkin>
    {
        public SkinSubmenu()
        {
            Text = Resources.ViewsSubmenu_ViewsSubmenu_View;
            Group = "Views";
            SortingValue = 5000;
        }

        public override bool CanExecute(object parameter)
        {
            return parameter is MediaContext;
        }

        public int Compare([NotNull] MediaSkinManager.MediaSkin x, [NotNull] MediaSkinManager.MediaSkin y)
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

            return string.Compare(x.SkinName, y.SkinName, StringComparison.InvariantCultureIgnoreCase);
        }

        public override void Execute(object parameter)
        {
        }

        public override IEnumerable<ICommand> GetSubmenuCommands(object parameter)
        {
            Assert.ArgumentNotNull(parameter, nameof(parameter));

            var context = parameter as MediaContext;
            if (context == null)
            {
                return Enumerable.Empty<ICommand>();
            }

            var result = new List<ICommand>();

            result.AddRange(CommandManager.GetCommands(parameter, "Views"));

            var list = new List<MediaSkinManager.MediaSkin>(MediaSkinManager.Skins.Values);

            list.Sort(this);

            var n = 0;
            foreach (var type in list)
            {
                var skinCommand = new SetMediaSkin
                {
                    SkinName = type.SkinName,
                    Text = type.SkinName,
                    IsChecked = string.Compare(type.SkinName, context.MediaViewer.SkinName, StringComparison.InvariantCultureIgnoreCase) == 0,
                    SortingValue = 3000 + n
                };

                result.Add(skinCommand);

                n++;
            }

            return result;
        }
    }
}
