// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Collections.Generic;
using System.Linq;
using Sitecore.Rocks.Commands;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.UI.LayoutDesigners.Commands.Tasks;

namespace Sitecore.Rocks.UI.LayoutDesigners.LayoutListViews.Commands
{
    [Command(Submenu = TasksSubmenu.Name, ExcludeFromSearch = true)]
    public class CopySubmenu : CommandBase
    {
        public CopySubmenu()
        {
            Text = Resources.CopySubmenu_CopySubmenu_Copy_to_Other_Device;
            Group = "Tasks";
            SortingValue = 2000;
        }

        public override bool CanExecute(object parameter)
        {
            var context = parameter as LayoutDesignerContext;
            if (context == null)
            {
                return false;
            }

            var tabsLayoutDesigner = context.LayoutDesigner.LayoutDesignerView as LayoutListView;
            if (tabsLayoutDesigner == null)
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
            Assert.ArgumentNotNull(parameter, nameof(parameter));

            var context = parameter as LayoutDesignerContext;
            if (context == null)
            {
                return Enumerable.Empty<ICommand>();
            }

            var tabsLayoutDesignerView = context.LayoutDesigner.LayoutDesignerView as LayoutListView;
            if (tabsLayoutDesignerView == null)
            {
                return Enumerable.Empty<ICommand>();
            }

            var layoutListView = tabsLayoutDesignerView.GetActiveListView();
            if (layoutListView == null)
            {
                return Enumerable.Empty<ICommand>();
            }

            var result = new List<ICommand>();

            result.AddRange(CommandManager.GetCommands(parameter, "Copy"));

            foreach (var tab in tabsLayoutDesignerView.Tabs)
            {
                if (tab.Equals(layoutListView))
                {
                    continue;
                }

                var copy = new CopyToDevice
                {
                    Text = tab.DeviceName,
                    SourceTab = layoutListView,
                    TargetTab = tab
                };

                result.Add(copy);
            }

            return result;
        }
    }
}
