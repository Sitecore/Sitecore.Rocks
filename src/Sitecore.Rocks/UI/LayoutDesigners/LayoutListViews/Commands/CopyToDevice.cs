// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.IO;
using System.Xml;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Commands;
using Sitecore.Rocks.Extensions.StringExtensions;

namespace Sitecore.Rocks.UI.LayoutDesigners.LayoutListViews.Commands
{
    public class CopyToDevice : CommandBase
    {
        [NotNull]
        public LayoutListViewTab SourceTab { get; set; }

        [NotNull]
        public LayoutListViewTab TargetTab { get; set; }

        public override bool CanExecute(object parameter)
        {
            var context = parameter as LayoutDesignerContext;
            if (context == null)
            {
                return false;
            }

            return true;
        }

        public override void Execute(object parameter)
        {
            var context = parameter as LayoutDesignerContext;
            if (context == null)
            {
                return;
            }

            var tabsLayoutDesigner = context.LayoutDesigner.LayoutDesignerView as LayoutListView;
            if (tabsLayoutDesigner == null)
            {
                return;
            }

            var writer = new StringWriter();
            var output = new XmlTextWriter(writer);

            SourceTab.CommitChanges();
            SourceTab.SaveLayout(output, TargetTab.DeviceId);

            var root = writer.ToString().ToXElement();
            if (root == null)
            {
                return;
            }

            TargetTab.LoadDevice(root);

            tabsLayoutDesigner.Activate(TargetTab.DeviceName);
        }
    }
}
