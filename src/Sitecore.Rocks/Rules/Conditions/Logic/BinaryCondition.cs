// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Diagnostics;

namespace Sitecore.Rocks.Rules.Conditions.Logic
{
    public abstract class BinaryCondition : RuleCondition
    {
        private readonly RuleCondition leftOperand;

        private RuleCondition rightOperand;

        protected BinaryCondition([NotNull] RuleCondition leftOperand, [NotNull] RuleCondition rightOperand)
        {
            Assert.ArgumentNotNull(leftOperand, nameof(leftOperand));
            Assert.ArgumentNotNull(rightOperand, nameof(rightOperand));

            this.leftOperand = leftOperand;
            this.rightOperand = rightOperand;
        }

        [NotNull]
        public RuleCondition LeftOperand
        {
            get { return leftOperand; }
        }

        [NotNull]
        public RuleCondition RightOperand
        {
            get { return rightOperand; }

            set
            {
                Assert.ArgumentNotNull(value, nameof(value));

                rightOperand = value;
            }
        }

        public override void Evaluate([CanBeNull] object parameter, [NotNull] RuleStack stack)
        {
            Assert.ArgumentNotNull(parameter, nameof(parameter));
            Assert.ArgumentNotNull(stack, nameof(stack));

            LeftOperand.Evaluate(parameter, stack);
            RightOperand.Evaluate(parameter, stack);
        }
    }
}
