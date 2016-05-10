// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Rocks.Commands;

namespace Sitecore.Rocks.UI.RuleEditors.Commands
{
    [Command]
    public class AddRule : CommandBase
    {
        public AddRule()
        {
            Text = Resources.AddRule_AddRule_Add_New_Rule;
            Group = "Editing";
            SortingValue = 1000;
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

            return true;
        }

        public override void Execute(object parameter)
        {
            var context = parameter as RuleEditorContext;
            if (context == null)
            {
                return;
            }

            context.RulePresenter.AddRule();
        }
    }
}
