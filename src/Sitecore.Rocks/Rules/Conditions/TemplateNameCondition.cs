// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Linq;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Data;

namespace Sitecore.Rocks.Rules.Conditions
{
    [RuleCondition("template name equals [TemplateName,Value,,specific value]", "Items")]
    public class TemplateNameCondition : ConditionBase
    {
        [UsedImplicitly]
        public string TemplateName { get; set; }

        protected virtual bool Compare([CanBeNull] string value1, [CanBeNull] string value2)
        {
            return value1 == value2;
        }

        protected override bool EvaluateCondition(object parameter)
        {
            var context = parameter as IItemSelectionContext;
            if (context == null)
            {
                return false;
            }

            if (!context.Items.Any())
            {
                return false;
            }

            foreach (var item in context.Items)
            {
                var templatedItem = item as ITemplatedItem;
                if (templatedItem == null)
                {
                    return false;
                }
            }

            foreach (var item in context.Items)
            {
                var templatedItem = item as ITemplatedItem;
                if (templatedItem == null)
                {
                    return false;
                }

                if (!Compare(TemplateName, templatedItem.TemplateName))
                {
                    return false;
                }
            }

            return true;
        }
    }
}
