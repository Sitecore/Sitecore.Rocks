// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Diagnostics;

namespace Sitecore.Rocks.Rules.Conditions.Logic
{
    public abstract class UnaryCondition : RuleCondition
    {
        private readonly RuleCondition operand;

        protected UnaryCondition([NotNull] RuleCondition operand)
        {
            Assert.ArgumentNotNull(operand, nameof(operand));

            this.operand = operand;
        }

        [NotNull]
        protected RuleCondition Operand
        {
            get { return operand; }
        }

        public override void Evaluate([CanBeNull] object parameter, [NotNull] RuleStack stack)
        {
            Assert.ArgumentNotNull(stack, nameof(stack));

            Operand.Evaluate(parameter, stack);
        }
    }
}
