// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Linq;
using Sitecore.Rocks.Commands;
using Sitecore.Rocks.Data;

namespace Sitecore.Rocks.UI.SitecoreCop.Commands
{
    [Command(Submenu = SitecoreCopSubmenu.Name)]
    public class RunSitecoreCop : CommandBase
    {
        public RunSitecoreCop()
        {
            Text = "Run SitecoreCop";
            Group = "Cops";
            SortingValue = 1000;
        }

        public override bool CanExecute(object parameter)
        {
            var context = parameter as IItemSelectionContext;
            if (context == null)
            {
                return false;
            }

            if (context.Items.Count() != 1)
            {
                return false;
            }

            return true;
        }

        public override void Execute(object parameter)
        {
            var context = parameter as IItemSelectionContext;
            if (context == null)
            {
                return;
            }

            var item = context.Items.FirstOrDefault();
            if (item == null)
            {
                return;
            }

            var viewer = AppHost.GetToolWindow<SitecoreCopViewer>("SitecoreCop");
            if (viewer == null)
            {
                viewer = new SitecoreCopViewer();
                AppHost.OpenToolWindow(viewer, "SitecoreCop");
            }

            viewer.CreateTab(item.ItemUri, false);
        }
    }
}
