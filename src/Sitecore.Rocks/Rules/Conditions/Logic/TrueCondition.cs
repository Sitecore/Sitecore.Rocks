// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Diagnostics;

namespace Sitecore.Rocks.Rules.Conditions.Logic
{
    [RuleCondition("true", "System")]
    public class TrueCondition : ConditionBase
    {
        protected override bool EvaluateCondition([NotNull] object parameter)
        {
            Assert.ArgumentNotNull(parameter, nameof(parameter));

            return true;
        }
    }
}
