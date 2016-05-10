// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Linq;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Data.DataServices;

namespace Sitecore.Rocks.Rules.Actions.Scripts
{
    [RuleAction("execute the script: [Script,value,,script text]", "Scripts")]
    public class ScriptMacroAction : RuleAction
    {
        public ScriptMacroAction()
        {
            Text = "Execute Script";
        }

        public string Script { get; set; }

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

            if (string.IsNullOrEmpty(Script))
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

            if (string.IsNullOrEmpty(Script))
            {
                return;
            }

            var isBusy = true;
            var item = context.Items.First();

            ExecuteCompleted c = delegate(string response, ExecuteResult result)
            {
                DataService.HandleExecute(response, result, true);
                isBusy = false;
            };

            item.ItemUri.DatabaseUri.Site.DataService.ExecuteAsync("QueryAnalyzer.Run", c, item.ItemUri.DatabaseUri.DatabaseName.ToString(), item.ItemUri.ItemId.ToString(), Script, "0");

            // TODO: do timeout
            while (isBusy)
            {
                AppHost.DoEvents();
            }
        }
    }
}
