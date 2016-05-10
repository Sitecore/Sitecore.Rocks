// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Linq;
using Sitecore.Rocks.Commands;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Shell;
using Sitecore.Rocks.UI.Toolbars;

namespace Sitecore.Rocks.ContentTrees.Commands.Tasks
{
    [Command(Submenu = TasksSubmenu.Name), CommandId(CommandIds.SitecoreExplorer.DesignTemplate, typeof(ContentTreeContext), ToolBar = ToolBars.SitecoreExplorer.Id, Group = ToolBars.SitecoreExplorer.Templates, Icon = "Resources/16x16/template_design.png", Priority = 0x0300), ToolbarElement(typeof(IItemSelectionContext), 2100, "Template", "Design", Text = "Design", Icon = "Resources/32x32/Template-Edit.png")]
    public class DesignTemplate : CommandBase, IToolbarElement
    {
        public DesignTemplate()
        {
            Text = Resources.Design_Template;
            Group = "Templates";
            SortingValue = 6000;
            Icon = new Icon("Resources/16x16/designtemplate.png");
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

            var item = context.Items.First();
            if (item == null)
            {
                return false;
            }

            return (item.ItemUri.Site.DataService.FeatureCapabilities & DataServiceFeatureCapabilities.EditTemplate) == DataServiceFeatureCapabilities.EditTemplate;
        }

        public override void Execute(object parameter)
        {
            var context = parameter as IItemSelectionContext;
            if (context == null)
            {
                return;
            }

            var item = context.Items.First();
            if (item == null)
            {
                return;
            }

            var templatedItem = item as ITemplatedItem;
            if (templatedItem != null)
            {
                AppHost.Windows.OpenTemplateDesigner(new ItemUri(item.ItemUri.DatabaseUri, templatedItem.TemplateId));
                return;
            }

            GetValueCompleted<ItemHeader> completed = value => AppHost.Windows.OpenTemplateDesigner(new ItemUri(item.ItemUri.DatabaseUri, value.TemplateId));

            AppHost.Server.GetItemHeader(item.ItemUri, completed);
        }
    }
}
