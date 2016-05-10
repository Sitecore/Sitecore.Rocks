// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Rocks.Commands;
using Sitecore.Rocks.UI.RuleEditors.Dialogs;

namespace Sitecore.Rocks.UI.RuleEditors.Commands
{
    [Command]
    public class TestRule : CommandBase
    {
        public TestRule()
        {
            Text = Resources.TestRule_TestRule_Test_Rules___;
            Group = "Test";
            SortingValue = 500;
        }

        public override bool CanExecute(object parameter)
        {
            var context = parameter as RuleEditorContext;
            if (context == null)
            {
                return false;
            }

            var databaseUri = context.RulePresenter.DatabaseUri;
            if (databaseUri == null)
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

            var databaseUri = context.RulePresenter.DatabaseUri;
            if (databaseUri == null)
            {
                return;
            }

            var ruleModel = context.RulePresenter.RuleModel;

            var dialog = new TestRuleDialog();
            dialog.Initialize(databaseUri, ruleModel);

            AppHost.Shell.ShowDialog(dialog);
        }
    }
}
