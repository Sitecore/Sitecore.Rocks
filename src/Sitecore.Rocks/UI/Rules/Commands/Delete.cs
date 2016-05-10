// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Windows.Controls;
using Sitecore.Rocks.Commands;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Rules;

namespace Sitecore.Rocks.UI.Rules.Commands
{
    [Command]
    public class Delete : CommandBase
    {
        public Delete()
        {
            Text = Resources.Delete_Delete_Delete;
            Group = "Edit";
            SortingValue = 1000;
        }

        public override bool CanExecute(object parameter)
        {
            var context = parameter as RuleDesignerContext;
            if (context == null)
            {
                return false;
            }

            if (context.Description == null)
            {
                return false;
            }

            if (context.Description.SelectedItem == null)
            {
                return false;
            }

            return true;
        }

        public override void Execute(object parameter)
        {
            var context = parameter as RuleDesignerContext;
            if (context == null)
            {
                return;
            }

            if (context.Description == null)
            {
                return;
            }

            if (context.Description.SelectedItem == null)
            {
                return;
            }

            var item = context.Description.SelectedItem as ListBoxItem;
            if (item == null)
            {
                Trace.Expected(typeof(ListBoxItem));
                return;
            }

            var condition = item.Tag as RuleConditionDescriptor;
            if (condition != null)
            {
                context.RuleDesigner.Rule.ConditionDescriptors.Remove(condition);
                context.Description.Items.Remove(item);
                return;
            }

            var action = item.Tag as RuleActionDescriptor;
            if (action == null)
            {
                Trace.TraceError("Expected RuleActionDescriptor or RuleConditionDescriptor");
                return;
            }

            context.RuleDesigner.Rule.ActionDescriptors.Remove(action);
            context.Description.Items.Remove(item);
        }
    }
}
