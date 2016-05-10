// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Linq;
using Sitecore.Rocks.Commands;
using Sitecore.Rocks.ContentEditors.Dialogs;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Data.DataServices;
using Sitecore.Rocks.Extensions.DataServiceExtensions;
using Sitecore.Rocks.Shell;
using Sitecore.Rocks.UI.Toolbars;

namespace Sitecore.Rocks.ContentTrees.Commands.Tasks
{
    [Command(Submenu = TasksSubmenu.Name), CommandId(CommandIds.ItemEditor.ChangeTemplate, typeof(ContentTreeContext), ToolBar = ToolBars.ItemEditor.Id, Group = ToolBars.ItemEditor.Templates, Icon = "Resources/16x16/template_change.png", Priority = 0x0300), ToolbarElement(typeof(IItemSelectionContext), 2110, "Template", "Design", Text = "Change", Icon = "Resources/32x32/Template-Wizard.png")]
    public class ChangeTemplate : CommandBase, IToolbarElement
    {
        public ChangeTemplate()
        {
            Text = Resources.Change_Template;
            Group = "Templates";
            SortingValue = 6100;
        }

        public override bool CanExecute(object parameter)
        {
            var context = parameter as IItemSelectionContext;
            if (context == null)
            {
                return false;
            }

            var item = context.Items.FirstOrDefault();
            if (item == null)
            {
                return false;
            }

            return item.ItemUri.Site.CanExecute;
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

            var templateId = ItemId.Empty;
            var templateName = string.Empty;

            var templatedItem = item as ITemplatedItem;
            if (templatedItem != null)
            {
                templateId = templatedItem.TemplateId;
                templateName = templatedItem.TemplateName;
            }

            var dialog = new ChangeTemplateDialog(item.ItemUri.DatabaseUri, templateId, templateName);
            if (AppHost.Shell.ShowDialog(dialog) != true)
            {
                return;
            }

            var template = dialog.SelectedTemplate;
            if (template == null)
            {
                return;
            }

            var uriList = context.Items.Select(i => i.ItemUri);

            ExecuteCompleted callback = delegate(string response, ExecuteResult executeResult)
            {
                if (!DataService.HandleExecute(response, executeResult))
                {
                    return;
                }

                foreach (var itemUri in uriList)
                {
                    Notifications.RaiseItemTemplateChanged(this, itemUri, template.TemplateUri);
                }
            };

            item.ItemUri.Site.DataService.ChangeTemplates(uriList, template.TemplateUri, callback);
        }
    }
}
