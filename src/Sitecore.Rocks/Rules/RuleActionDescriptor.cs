// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Collections.Generic;
using Sitecore.Rocks.Annotations;

namespace Sitecore.Rocks.Rules
{
    public class RuleActionDescriptor
    {
        public RuleActionDescriptor()
        {
            Parameters = new Dictionary<string, string>();
        }

        [NotNull]
        public IRuleAction Action { get; set; }

        [NotNull]
        public string DisplayText
        {
            get
            {
                var attributes = Action.GetType().GetCustomAttributes(typeof(RuleActionAttribute), true);

                var attribute = (RuleActionAttribute)attributes[0];

                return attribute.DisplayText;
            }
        }

        public Dictionary<string, string> Parameters { get; private set; }
    }
}
