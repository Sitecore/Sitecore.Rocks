// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Collections.Generic;
using System.Linq;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Commands;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Diagnostics;

namespace Sitecore.Rocks.UI.Macros.Commands
{
    public class MacroCommand : CommandBase
    {
        public Macro Macro { get; set; }

        public override bool CanExecute(object parameter)
        {
            if (Macro.Scope == MacroScope.Once)
            {
                return true;
            }

            var context = parameter as IItemSelectionContext;
            if (context == null)
            {
                return false;
            }

            return context.Items.Any();
        }

        public override void Execute(object parameter)
        {
            Macro.Run(parameter);
        }

        public class ItemContext : IItemSelectionContext
        {
            public ItemContext([NotNull] IItem item)
            {
                Assert.ArgumentNotNull(item, nameof(item));

                Item = item;
            }

            [NotNull]
            public IItem Item { get; set; }

            [NotNull]
            public IEnumerable<IItem> Items
            {
                get { yield return Item; }
            }
        }
    }
}
