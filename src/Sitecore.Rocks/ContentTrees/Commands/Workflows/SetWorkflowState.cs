// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.Linq;
using System.Windows;
using Sitecore.Rocks.Commands;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Data.DataServices;
using Sitecore.Rocks.Extensibility;
using Sitecore.Rocks.Extensions.IItemSelectionContextExtensions;
using Sitecore.Rocks.Shell;
using Sitecore.Rocks.UI.Dialogs.SelectItemDialogs;

namespace Sitecore.Rocks.ContentTrees.Commands.Workflows
{
    [Command(Submenu = "Workflows"), CommandId(CommandIds.SitecoreExplorer.SetWorkflowState, typeof(ContentTreeContext)), Feature(FeatureNames.Workflow)]
    public class SetWorkflowState : CommandBase
    {
        private static readonly ItemId workflowStateId = new ItemId(new Guid(@"{4B7E2DA9-DE43-4C83-88C3-02F042031D04}"));

        public SetWorkflowState()
        {
            Text = Resources.SetWorkflowState_SetWorkflowState_Set_Workflow_State___;
            Group = "WorkflowStates";
            SortingValue = 1000;
        }

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

            if (!context.IsSameDatabase())
            {
                return false;
            }

            if (context.Items.Any(i => !i.ItemUri.Site.DataService.CanExecuteAsync("Workflows.SetWorkflowState")))
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

            var itemId = IdManager.GetItemId("/sitecore/system/Workflows");
            SelectItemDialog dialog;

            while (true)
            {
                dialog = new SelectItemDialog();

                dialog.Initialize(Resources.SetWorkflowState_Execute_Select_Workflow_State, new ItemUri(item.ItemUri.DatabaseUri, itemId));

                if (AppHost.Shell.ShowDialog(dialog) != true)
                {
                    return;
                }

                itemId = dialog.SelectedItemUri.ItemId;

                if (dialog.SelectedItemTemplateId == workflowStateId)
                {
                    break;
                }

                if (AppHost.MessageBox(Resources.SetWorkflowState_Execute_, Resources.Error, MessageBoxButton.OKCancel, MessageBoxImage.Question) != MessageBoxResult.OK)
                {
                    return;
                }
            }

            var stateId = dialog.SelectedItemUri.ItemId;

            var itemIdList = context.GetItemIdList();
            var databaseName = context.Items.First().ItemUri.DatabaseName.ToString();

            ExecuteCompleted completed = delegate(string response, ExecuteResult result)
            {
                if (!DataService.HandleExecute(response, result))
                {
                    return;
                }

                var workflowFieldId = IdManager.GetFieldId("/sitecore/templates/System/Templates/Sections/Workflow/Workflow/__Workflow");
                var stateFieldId = IdManager.GetFieldId("/sitecore/templates/System/Templates/Sections/Workflow/Workflow/__Workflow state");

                foreach (var i in context.Items)
                {
                    var itemVersionUri = new ItemVersionUri(i.ItemUri, LanguageManager.CurrentLanguage, Data.Version.Latest);

                    var fieldUri = new FieldUri(itemVersionUri, stateFieldId);
                    Notifications.RaiseFieldChanged(this, fieldUri, stateId.ToString());

                    fieldUri = new FieldUri(itemVersionUri, workflowFieldId);
                    Notifications.RaiseFieldChanged(this, fieldUri, response);
                }
            };

            item.ItemUri.Site.DataService.ExecuteAsync("Workflows.SetWorkflowState", completed, databaseName, itemIdList, stateId.ToString());
        }
    }
}
