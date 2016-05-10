// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.IO;
using System.Linq;
using System.Xml;
using Sitecore.Rocks.Commands;
using Sitecore.Rocks.Extensions.StringExtensions;
using Sitecore.Rocks.UI.LayoutDesigners.Items;
using Sitecore.Rocks.UI.Toolbars;

namespace Sitecore.Rocks.UI.LayoutDesigners.LayoutListViews.Commands
{
    [Command, ToolbarElement(typeof(LayoutDesignerContext), 1005, "Home", "Editing", Icon = "Resources/16x16/Slide-Duplicate.png", ElementType = RibbonElementType.SmallButton)]
    public class Duplicate : CommandBase, IDynamicToolbarElement
    {
        public Duplicate()
        {
            Text = "Duplicate";
            Group = "Edit";
            SortingValue = 2000;
        }

        public override bool CanExecute(object parameter)
        {
            var context = parameter as LayoutDesignerContext;
            if (context == null)
            {
                return false;
            }

            var tabsLayoutDesignerView = context.LayoutDesigner.LayoutDesignerView as LayoutListView;
            if (tabsLayoutDesignerView == null)
            {
                return false;
            }

            var layoutListView = tabsLayoutDesignerView.GetActiveListView();
            if (layoutListView == null)
            {
                return false;
            }

            return context.SelectedItems.Count() == 1 && context.SelectedItems.All(r => r is RenderingItem);
        }

        public override void Execute(object parameter)
        {
            var context = parameter as LayoutDesignerContext;
            if (context == null)
            {
                return;
            }

            var renderingItem = context.SelectedItems.FirstOrDefault() as RenderingItem;
            if (renderingItem == null)
            {
                return;
            }

            var tabsLayoutDesignerView = context.LayoutDesigner.LayoutDesignerView as LayoutListView;
            if (tabsLayoutDesignerView == null)
            {
                return;
            }

            var layoutListView = tabsLayoutDesignerView.GetActiveListView();
            if (layoutListView == null)
            {
                return;
            }

            var index = -1;
            var item = context.SelectedItem;
            if (item != null)
            {
                index = layoutListView.List.Items.IndexOf(item);
                if (index < 0)
                {
                    index = -1;
                }
            }

            renderingItem.Commit();

            var writer = new StringWriter();
            var output = new XmlTextWriter(writer);

            output.WriteStartElement("renderings");
            renderingItem.Write(output, true);
            output.WriteEndElement();

            var root = writer.ToString().ToXElement();
            if (root == null)
            {
                return;
            }

            var newRenderingItem = root.Elements().Select(element => new RenderingItem(renderingItem.RenderingContainer, renderingItem.ItemUri.DatabaseUri, element, true)).FirstOrDefault();
            if (newRenderingItem == null)
            {
                return;
            }

            layoutListView.AddRendering(newRenderingItem, index, index);

            layoutListView.List.SelectedIndex = index;
            context.LayoutDesigner.Modified = true;
        }

        bool IDynamicToolbarElement.CanRender(object parameter)
        {
            var context = parameter as LayoutDesignerContext;
            return context != null && context.LayoutDesigner.LayoutDesignerView is LayoutListView;
        }
    }
}
