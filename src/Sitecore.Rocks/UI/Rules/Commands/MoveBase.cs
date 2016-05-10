// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Windows.Controls;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Commands;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Rules;

namespace Sitecore.Rocks.UI.Rules.Commands
{
    public abstract class MoveBase : CommandBase
    {
        public override bool CanExecute(object parameter)
        {
            return MoveElement(parameter, true);
        }

        public override void Execute(object parameter)
        {
            var context = parameter as RuleDesignerContext;
            if (context == null)
            {
                return;
            }

            var result = MoveElement(parameter, false);
            if (result)
            {
                context.RuleDesigner.RenderRule(context.RuleDesigner.Rule);
            }
        }

        protected abstract int GetOffset();

        private bool MoveAction([NotNull] Rule rule, [NotNull] RuleActionDescriptor action, bool dryRun)
        {
            Debug.ArgumentNotNull(rule, nameof(rule));
            Debug.ArgumentNotNull(action, nameof(action));

            var index = rule.ActionDescriptors.IndexOf(action) + GetOffset();
            if (index < 0 || index >= rule.ActionDescriptors.Count)
            {
                return false;
            }

            if (!dryRun)
            {
                rule.ActionDescriptors.Remove(action);
                rule.ActionDescriptors.Insert(index, action);
            }

            return true;
        }

        private bool MoveCondition([NotNull] Rule rule, [NotNull] RuleConditionDescriptor condition, bool dryRun)
        {
            Debug.ArgumentNotNull(rule, nameof(rule));
            Debug.ArgumentNotNull(condition, nameof(condition));

            var index = rule.ConditionDescriptors.IndexOf(condition) + GetOffset();
            if (index < 0 || index >= rule.ConditionDescriptors.Count)
            {
                return false;
            }

            if (!dryRun)
            {
                rule.ConditionDescriptors.Remove(condition);
                rule.ConditionDescriptors.Insert(index, condition);
            }

            return true;
        }

        private bool MoveElement([CanBeNull] object parameter, bool dryRun)
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

            var item = context.Description.SelectedItem as ListBoxItem;
            if (item == null)
            {
                Trace.Expected(typeof(ListBoxItem));
                return false;
            }

            var condition = item.Tag as RuleConditionDescriptor;
            if (condition != null)
            {
                return MoveCondition(context.RuleDesigner.Rule, condition, dryRun);
            }

            var action = item.Tag as RuleActionDescriptor;
            if (action != null)
            {
                return MoveAction(context.RuleDesigner.Rule, action, dryRun);
            }

            return false;
        }
    }
}
