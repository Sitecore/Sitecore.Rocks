// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Linq;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Commands;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Shell;
using Sitecore.Rocks.UI;
using Sitecore.Rocks.UI.Toolbars;

namespace Sitecore.Rocks.ContentTrees.Commands.Navigating
{
    [Command(Submenu = "Navigate"), CommandId(CommandIds.SitecoreExplorer.NavigateTemplate, typeof(ContentTreeContext), Text = "Navigate to Template", ToolBar = ToolBars.SitecoreExplorer.Id, Group = ToolBars.SitecoreExplorer.Navigating, Icon = "Resources/16x16/template_navigate.png", Priority = 0x0200), ToolbarElement(typeof(IItemSelectionContext), 1010, "Home", "Navigate", Icon = "Resources/16x16/template_navigate.png", Text = "Template", ElementType = RibbonElementType.SmallButton)]
    public class NavigateTemplate : CommandBase, IToolbarElement
    {
        public NavigateTemplate()
        {
            Text = Resources.NavigateTemplate_NavigateTemplate_Template;
            Group = "Navigate";
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

            if (ActiveContext.ActiveContentTree == null)
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

            var item = context.Items.First();

            var templatedItem = item as ITemplatedItem;
            if (templatedItem != null)
            {
                Locate(item.ItemUri, templatedItem.TemplateId);
                return;
            }

            GetValueCompleted<ItemHeader> completed = value => Locate(value.ItemUri, value.TemplateId);

            AppHost.Server.GetItemHeader(item.ItemUri, completed);
        }

        private void Locate([NotNull] ItemUri itemUri, [NotNull] ItemId templateId)
        {
            Debug.ArgumentNotNull(itemUri, nameof(itemUri));
            Debug.ArgumentNotNull(templateId, nameof(templateId));

            var contentTree = ActiveContext.ActiveContentTree;
            if (contentTree == null)
            {
                return;
            }

            var templateUri = new ItemUri(itemUri.DatabaseUri, templateId);

            contentTree.Locate(templateUri);
        }
    }
}
