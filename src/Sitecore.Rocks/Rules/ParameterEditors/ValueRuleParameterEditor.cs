// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Rocks.Diagnostics;

namespace Sitecore.Rocks.Rules.ParameterEditors
{
    [RuleParameter("Value")]
    public class ValueRuleParameterEditor : IRuleParameterEditor
    {
        public string GetValue(string defaultValue, object parameter)
        {
            Assert.ArgumentNotNull(defaultValue, nameof(defaultValue));

            var result = AppHost.Prompt("Enter a value for the parameter:", "Parameter", defaultValue);

            return result ?? defaultValue;
        }
    }
}
