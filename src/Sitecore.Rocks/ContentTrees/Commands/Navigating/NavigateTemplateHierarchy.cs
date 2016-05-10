// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Linq;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Commands;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Extensibility;
using Sitecore.Rocks.Shell;
using Sitecore.Rocks.UI.Toolbars;

namespace Sitecore.Rocks.ContentTrees.Commands.Navigating
{
    [Command(Submenu = "Navigate"), CommandId(CommandIds.SitecoreExplorer.NavigateTemplateHierarchy, typeof(ContentTreeContext)), Feature(FeatureNames.AdvancedNavigation), ToolbarElement(typeof(IItemSelectionContext), 2500, "Template", "Tools", Icon = "Resources/16x16/Flow-Chart.png", ElementType = RibbonElementType.SmallButton)]
    public class NavigateTemplateHierarchy : CommandBase, IToolbarElement
    {
        public NavigateTemplateHierarchy()
        {
            Text = Resources.NavigateTemplateHierarchy_NavigateTemplateHierarchy_Template_Hierarchy;
            Group = "Links";
            SortingValue = 2500;
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

            return context.Items.First().ItemUri.Site.DataService.CanExecuteAsync("Templates.GetTemplateXml");
        }

        public override void Execute(object parameter)
        {
            var selection = parameter as IItemSelectionContext;
            if (selection == null)
            {
                return;
            }

            var item = selection.Items.First();

            var templatedItem = item as ITemplatedItem;
            if (templatedItem != null)
            {
                OpenViewer(item.ItemUri, templatedItem.TemplateId);
                return;
            }

            GetValueCompleted<ItemHeader> completed = value => OpenViewer(value.ItemUri, value.TemplateId);

            AppHost.Server.GetItemHeader(item.ItemUri, completed);
        }

        private void OpenViewer([NotNull] ItemUri itemUri, [NotNull] ItemId templateId)
        {
            Debug.ArgumentNotNull(itemUri, nameof(itemUri));
            Debug.ArgumentNotNull(templateId, nameof(templateId));

            ItemUri templateUri;

            var templateType = IdManager.GetTemplateType(templateId);
            if (templateType == "template")
            {
                templateUri = itemUri;
            }
            else
            {
                templateUri = new ItemUri(itemUri.DatabaseUri, templateId);
            }

            var viewer = AppHost.Windows.OpenTemplateHierarchyViewer();
            if (viewer == null)
            {
                return;
            }

            var tab = viewer.CreateTab(templateUri);

            tab.Initialize(templateUri);
        }
    }
}
