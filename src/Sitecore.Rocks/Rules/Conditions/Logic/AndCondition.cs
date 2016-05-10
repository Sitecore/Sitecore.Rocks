// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Diagnostics;

namespace Sitecore.Rocks.Rules.Conditions.Logic
{
    public class AndCondition : BinaryCondition
    {
        public AndCondition([NotNull] RuleCondition leftOperand, [NotNull] RuleCondition rightOperand) : base(leftOperand, rightOperand)
        {
            Assert.ArgumentNotNull(leftOperand, nameof(leftOperand));
            Assert.ArgumentNotNull(rightOperand, nameof(rightOperand));
        }

        public override void Evaluate([CanBeNull] object parameter, [NotNull] RuleStack stack)
        {
            Assert.ArgumentNotNull(stack, nameof(stack));

            LeftOperand.Evaluate(parameter, stack);

            var left = (bool)stack.Pop();
            if (!left)
            {
                stack.Push(false);
                return;
            }

            RightOperand.Evaluate(parameter, stack);
        }
    }
}
