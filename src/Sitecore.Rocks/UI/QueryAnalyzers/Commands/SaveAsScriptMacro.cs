// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Collections.Generic;
using Sitecore.Rocks.Commands;
using Sitecore.Rocks.Rules;
using Sitecore.Rocks.Rules.Actions.Scripts;
using Sitecore.Rocks.Rules.Conditions.Logic;
using Sitecore.Rocks.UI.Macros;

namespace Sitecore.Rocks.UI.QueryAnalyzers.Commands
{
    [Command]
    public class SaveAsScriptMacro : CommandBase
    {
        public SaveAsScriptMacro()
        {
            Text = Resources.SaveAsScriptMacro_SaveAsScriptMacro_Save_as_Script_Macro___;
            Group = "Files";
            SortingValue = 2300;
        }

        public override bool CanExecute(object parameter)
        {
            var context = parameter as QueryAnalyzerContext;
            if (context == null)
            {
                return false;
            }

            if (string.IsNullOrEmpty(context.Script))
            {
                return false;
            }

            return true;
        }

        public override void Execute(object parameter)
        {
            var context = parameter as QueryAnalyzerContext;
            if (context == null)
            {
                return;
            }

            var name = AppHost.Prompt("Enter the name of the script macro:", "Save as Script Macro");
            if (name == null)
            {
                return;
            }

            var conditionDescriptors = new List<RuleConditionDescriptor>
            {
                new RuleConditionDescriptor
                {
                    Condition = new TrueCondition()
                }
            };

            var ruleActionDescriptor = new RuleActionDescriptor
            {
                Action = new ScriptMacroAction()
            };

            ruleActionDescriptor.Parameters[@"Script"] = context.Script;

            var actionDescriptors = new List<RuleActionDescriptor>
            {
                ruleActionDescriptor
            };

            var rule = new Rule(conditionDescriptors, actionDescriptors);

            var macro = new Macro(rule, name);

            MacroManager.Add(macro);
        }
    }
}
