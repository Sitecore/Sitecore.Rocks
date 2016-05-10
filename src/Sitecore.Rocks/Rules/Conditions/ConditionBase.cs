// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Diagnostics;

namespace Sitecore.Rocks.Rules.Conditions
{
    public abstract class ConditionBase : RuleCondition
    {
        public override void Evaluate([CanBeNull] object parameter, [NotNull] RuleStack stack)
        {
            Assert.ArgumentNotNull(stack, nameof(stack));

            stack.Push(EvaluateCondition(parameter));
        }

        protected abstract bool EvaluateCondition([CanBeNull] object parameter);
    }
}
