// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.Linq;
using Sitecore.Rocks.Data;

namespace Sitecore.Rocks.Rules.Actions.Items
{
    public class Move : RuleAction
    {
        public Move()
        {
            Text = "Move Item";
        }

        public string TargetItemId { get; set; }

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

            if (string.IsNullOrEmpty(TargetItemId))
            {
                return;
            }

            var itemId = new ItemId(new Guid(TargetItemId));

            foreach (var item in context.Items)
            {
                item.ItemUri.Site.DataService.Move(item.ItemUri, itemId);
            }
        }
    }
}
