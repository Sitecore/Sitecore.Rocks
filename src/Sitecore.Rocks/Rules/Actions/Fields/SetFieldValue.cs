// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Linq;
using Sitecore.Rocks.Data;

namespace Sitecore.Rocks.Rules.Actions.Fields
{
    [RuleAction("set field [FieldName,value,,value] to [Value,value,,specific value]", "Fields")]
    public class SetFieldValue : RuleAction
    {
        public SetFieldValue()
        {
            Text = "Set Field";
        }

        public string FieldName { get; set; }

        public string Value { get; set; }

        public override bool CanExecute(object parameter)
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

            return true;
        }

        public override void Execute(object parameter)
        {
            var context = parameter as IItemSelectionContext;
            if (context == null)
            {
                return;
            }

            foreach (var item in context.Items)
            {
                ItemModifier.Edit(new ItemVersionUri(item.ItemUri, LanguageManager.CurrentLanguage, Version.Latest), FieldName, Value);
            }
        }
    }
}
