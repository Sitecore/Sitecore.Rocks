// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Collections.Generic;
using System.Linq;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Diagnostics;

namespace Sitecore.Rocks.ContentTrees.Commands.Scripts
{
    public abstract class ScriptItemsCommand : ScriptCommand
    {
        public override void Execute(object parameter)
        {
            var context = parameter as IItemSelectionContext;
            if (context == null)
            {
                return;
            }

            var items = GetItems(context);
            var itemList = new List<Item>();

            GetValueCompleted<Item> c = delegate(Item value)
            {
                itemList.Add(value);
                if (itemList.Count != items.Count)
                {
                    return;
                }

                var databaseUri = items.First().ItemUri.DatabaseUri;

                var script = GetScript(itemList);

                OpenQueryAnalyzer(databaseUri, script);
            };

            foreach (var item in items)
            {
                item.ItemUri.Site.DataService.GetItemFieldsAsync(new ItemVersionUri(item.ItemUri, LanguageManager.CurrentLanguage, Version.Latest), c);
            }
        }

        [NotNull]
        protected abstract string GetScript([NotNull] List<Item> items);

        [NotNull]
        protected override string GetScript(List<IItem> items)
        {
            Debug.ArgumentNotNull(items, nameof(items));

            return string.Empty;
        }
    }
}
