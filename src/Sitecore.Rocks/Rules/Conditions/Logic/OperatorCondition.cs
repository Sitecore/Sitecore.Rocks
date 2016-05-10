// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

namespace Sitecore.Rocks.Rules.Conditions.Logic
{
    public enum ConditionOperator
    {
        Unknown,

        Equal,

        GreaterThanOrEqual,

        GreaterThan,

        LessThanOrEqual,

        LessThan,

        NotEqual
    }

    public abstract class OperatorCondition : ConditionBase
    {
        public ConditionOperator Operator { get; set; }
    }
}
