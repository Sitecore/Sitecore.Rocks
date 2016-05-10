// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Diagnostics;

namespace Sitecore.Rocks.Rules.Conditions.Logic
{
    public class NotCondition : UnaryCondition
    {
        public NotCondition([NotNull] RuleCondition operand) : base(operand)
        {
            Assert.ArgumentNotNull(operand, nameof(operand));
        }

        public override void Evaluate([CanBeNull] object parameter, [NotNull] RuleStack stack)
        {
            Assert.ArgumentNotNull(stack, nameof(stack));

            Operand.Evaluate(parameter, stack);

            var result = (bool)stack.Pop();
            stack.Push(!result);
        }
    }
}
