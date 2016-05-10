// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Xml.Linq;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Commands;
using Sitecore.Rocks.ContentTrees.Commands.Tools;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Data.DataServices;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Extensibility;
using Sitecore.Rocks.Extensions.XElementExtensions;

namespace Sitecore.Rocks.ContentTrees.Commands.Workflows
{
    [Command(Submenu = ToolsSubmenu.Name, ExcludeFromSearch = true), Feature(FeatureNames.Workflow)]
    public class WorkflowSubmenu : CommandBase
    {
        public WorkflowSubmenu()
        {
            Text = Resources.Workflow_Workflow_Workflow;
            Group = "Tools";
            SortingValue = 1600;

            SubmenuOpened = Opened;
        }

        [CanBeNull]
        protected IItemSelectionContext Context { get; set; }

        public override bool CanExecute(object parameter)
        {
            var context = parameter as IItemSelectionContext;
            if (context == null)
            {
                return false;
            }

            if (!context.Items.Any())
            {
                return false;
            }

            string databaseName = null;
            string siteName = null;

            foreach (var item in context.Items)
            {
                if (databaseName == null)
                {
                    databaseName = item.ItemUri.DatabaseName.Name;
                    siteName = item.ItemUri.Site.Name;
                }
                else if (databaseName != item.ItemUri.DatabaseName.Name)
                {
                    return false;
                }
                else if (siteName != item.ItemUri.Site.Name)
                {
                    return false;
                }
            }

            if (!context.Items.All(i => i.ItemUri.Site.DataService.CanExecuteAsync("Workflows.ExecuteWorkflowCommand")))
            {
                return false;
            }

            Context = context;

            return true;
        }

        public override void Execute(object parameter)
        {
        }

        public override IEnumerable<ICommand> GetSubmenuCommands([CanBeNull] object parameter)
        {
            if (parameter == null)
            {
                return Enumerable.Empty<ICommand>();
            }

            return CommandManager.GetCommands(parameter, "Workflows");
        }

        private void ExecuteWorkflowCommand([NotNull] object sender, [NotNull] RoutedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            var itemSelectionContext = Context;
            if (itemSelectionContext == null)
            {
                return;
            }

            var item = itemSelectionContext.Items.FirstOrDefault();
            if (item == null)
            {
                return;
            }

            var menuItem = e.Source as MenuItem;
            if (menuItem == null)
            {
                return;
            }

            var commandId = menuItem.Tag as string;
            var name = menuItem.Header as string ?? string.Empty;

            var comment = AppHost.Prompt(string.Format(Resources.Workflow_ExecuteWorkflowCommand_Comment_for___0___, name), name);
            if (comment == null)
            {
                return;
            }

            string databaseName;
            string list;

            if (!GetList(out databaseName, out list))
            {
                return;
            }

            ExecuteCompleted callback = (response, executeResult) => DataService.HandleExecute(response, executeResult);

            item.ItemUri.Site.DataService.ExecuteAsync("Workflows.ExecuteWorkflowCommand", callback, databaseName, list, commandId, comment);
        }

        private bool GetList([NotNull] out string databaseName, [NotNull] out string list)
        {
            list = string.Empty;
            databaseName = string.Empty;

            var itemSelectionContext = Context;
            if (itemSelectionContext == null)
            {
                return false;
            }

            foreach (var item in itemSelectionContext.Items)
            {
                if (!string.IsNullOrEmpty(list))
                {
                    list += @"|";
                }

                list += item.ItemUri.ItemId.ToString();

                if (string.IsNullOrEmpty(databaseName))
                {
                    databaseName = item.ItemUri.DatabaseName.Name;
                }
            }

            return true;
        }

        private void LoadWorkflowCommands([NotNull] string response, [NotNull] ExecuteResult executeResult, [NotNull] MenuItem menuItem)
        {
            Debug.ArgumentNotNull(response, nameof(response));
            Debug.ArgumentNotNull(executeResult, nameof(executeResult));
            Debug.ArgumentNotNull(menuItem, nameof(menuItem));

            if (!DataService.HandleExecute(response, executeResult))
            {
                return;
            }

            menuItem.Items.RemoveAt(0);

            XElement root;

            try
            {
                var doc = XDocument.Parse(response);
                root = doc.Root;
            }
            catch
            {
                root = null;
            }

            var count = 0;
            if (root != null)
            {
                foreach (var element in root.Elements().Reverse())
                {
                    var name = element.GetAttributeValue("name");
                    var id = element.GetAttributeValue("id");

                    var item = new MenuItem
                    {
                        Header = name,
                        Tag = id
                    };

                    item.Click += ExecuteWorkflowCommand;
                    menuItem.Items.Insert(0, item);
                    count++;
                }
            }

            if (count == 0)
            {
                var item = new MenuItem
                {
                    Header = Resources.Workflow_LoadWorkflowCommands__none_,
                    Foreground = SystemColors.GrayTextBrush
                };

                menuItem.Items.Insert(0, item);
            }
        }

        private void Opened([NotNull] object sender, [NotNull] RoutedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            var itemSelectionContext = Context;
            if (itemSelectionContext == null)
            {
                return;
            }

            var parent = itemSelectionContext.Items.FirstOrDefault();
            if (parent == null)
            {
                return;
            }

            var menuItem = e.Source as MenuItem;
            if (menuItem == null)
            {
                return;
            }

            var m = menuItem.Items[0] as MenuItem;
            if (m == null || m.Tag as string != @"loading")
            {
                return;
            }

            if (menuItem.Items.Count > 1)
            {
                menuItem.Items.Insert(1, new Separator());
            }

            string databaseName;
            string list;

            if (!GetList(out databaseName, out list))
            {
                return;
            }

            parent.ItemUri.Site.DataService.ExecuteAsync("Workflows.GetWorkflowCommands", (response, executeResult) => LoadWorkflowCommands(response, executeResult, menuItem), databaseName, list);
        }
    }
}
