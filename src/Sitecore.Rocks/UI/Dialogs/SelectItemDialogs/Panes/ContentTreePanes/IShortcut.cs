// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Collections.Generic;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Data;

namespace Sitecore.Rocks.UI.Dialogs.SelectItemDialogs.Panes.ContentTreePanes
{
    public interface IShortcut
    {
        [NotNull]
        string Header { get; }

        void Close([NotNull] SelectItemDialog selectItemDialog);

        [NotNull]
        IEnumerable<IItem> GetItems([NotNull] DatabaseUri databaseUri);
    }
}
