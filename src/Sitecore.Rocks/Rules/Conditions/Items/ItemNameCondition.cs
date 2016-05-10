// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Linq;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Data;

namespace Sitecore.Rocks.Rules.Conditions.Items
{
    [RuleCondition("item name equals [ItemName,Value,,specific value]", "Items")]
    public class ItemNameCondition : ConditionBase
    {
        [UsedImplicitly]
        public string ItemName { get; set; }

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

            return context.Items.All(item => Compare(item.Name, ItemName));
        }
    }
}
