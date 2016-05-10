// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Collections.Generic;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Controls;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Extensibility.Composition;

namespace Sitecore.Rocks.UI.Dialogs.SelectItemDialogs.Panes.ContentTreePanes
{
    [Export(typeof(IShortcut), Priority = 3000)]
    public class RecentShortcut : IShortcut
    {
        private StringJournal journal;

        public RecentShortcut()
        {
            Header = "Recent";
        }

        public string Header { get; }

        public void Close(SelectItemDialog selectItemDialog)
        {
            Assert.ArgumentNotNull(selectItemDialog, nameof(selectItemDialog));

            var entry = selectItemDialog.SelectedItemName + "^" + selectItemDialog.SelectedItemUri;

            journal.Push(entry);
            journal.Save();
        }

        public IEnumerable<IItem> GetItems(DatabaseUri databaseUri)
        {
            Assert.ArgumentNotNull(databaseUri, nameof(databaseUri));

            journal = new StringJournal("SelectItem", databaseUri.ToString())
            {
                Max = 8,
                RemoveDuplicates = true
            };

            foreach (var entry in journal.GetHistory())
            {
                var parts = entry.Split('^');
                if (parts.Length != 2)
                {
                    continue;
                }

                ItemUri itemUri;
                if (!ItemUri.TryParse(parts[1], out itemUri))
                {
                    continue;
                }

                var name = parts[0];

                yield return new HistoryEntry(itemUri, name);
            }
        }

        public class HistoryEntry : IItem
        {
            public HistoryEntry([NotNull] ItemUri itemUri, [NotNull] string name)
            {
                Assert.ArgumentNotNull(itemUri, nameof(itemUri));
                Assert.ArgumentNotNull(name, nameof(name));

                ItemUri = itemUri;
                Name = name;
                Icon = Icon.Empty;
            }

            public Icon Icon { get; }

            public ItemUri ItemUri { get; }

            public string Name { get; }
        }
    }
}
