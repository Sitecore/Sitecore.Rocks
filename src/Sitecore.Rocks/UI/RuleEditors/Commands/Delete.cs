// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Xml.Linq;
using Sitecore.Rocks.Commands;
using Sitecore.Rocks.Data;

namespace Sitecore.Rocks.UI.RuleEditors.Commands
{
    [Command]
    public class Delete : CommandBase
    {
        public Delete()
        {
            Text = Resources.Delete_Delete_Delete;
            Group = "Editing";
            SortingValue = 1100;
            Icon = new Icon("Resources/16x16/delete.png");
        }

        public override bool CanExecute(object parameter)
        {
            var context = parameter as RuleEditorContext;
            if (context == null)
            {
                return false;
            }

            if (!context.RulePresenter.IsEditable)
            {
                return false;
            }

            var listBoxItem = context.SelectedRuleEntry;
            if (listBoxItem == null)
            {
                return false;
            }

            var element = listBoxItem.Tag as XElement;
            if (element == null)
            {
                return false;
            }

            var action = listBoxItem.Content as ActionListBoxItem;
            if (action != null)
            {
                return true;
            }

            var condition = listBoxItem.Content as ConditionListBoxItem;
            if (condition != null)
            {
                return true;
            }

            var rule = listBoxItem.Content as RuleListBoxItem;
            if (rule != null)
            {
                return true;
            }

            return false;
        }

        public override void Execute(object parameter)
        {
            var context = parameter as RuleEditorContext;
            if (context == null)
            {
                return;
            }

            var listBoxItem = context.SelectedRuleEntry;
            if (listBoxItem == null)
            {
                return;
            }

            var element = listBoxItem.Tag as XElement;
            if (element == null)
            {
                return;
            }

            var action = listBoxItem.Content as ActionListBoxItem;
            if (action != null)
            {
                context.RulePresenter.DeleteAction(element);
            }

            var condition = listBoxItem.Content as ConditionListBoxItem;
            if (condition != null)
            {
                context.RulePresenter.DeleteCondition(element);
            }

            var rule = listBoxItem.Content as RuleListBoxItem;
            if (rule != null)
            {
                context.RulePresenter.DeleteRule(element);
            }
        }
    }
}
