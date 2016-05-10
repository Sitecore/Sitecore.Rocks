// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Collections.Generic;
using Sitecore.Rocks.Annotations;

namespace Sitecore.Rocks.Rules
{
    public class RuleConditionDescriptor
    {
        public RuleConditionDescriptor()
        {
            Parameters = new Dictionary<string, string>();
        }

        [NotNull]
        public RuleCondition Condition { get; set; }

        [NotNull]
        public string DisplayText
        {
            get
            {
                var attributes = Condition.GetType().GetCustomAttributes(typeof(RuleConditionAttribute), true);

                var attribute = (RuleConditionAttribute)attributes[0];

                return attribute.DisplayText;
            }
        }

        public bool IsNot { get; set; }

        public RuleConditionDescriptorOperator Operator { get; set; }

        public Dictionary<string, string> Parameters { get; private set; }
    }
}
