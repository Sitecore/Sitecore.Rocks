// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Collections.Generic;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.ContentTrees;
using Sitecore.Rocks.ContentTrees.Items;
using Sitecore.Rocks.Controls;
using Sitecore.Rocks.Data;

namespace Sitecore.Rocks.UI.Dialogs.SelectItemDialogs
{
    public class SelectItemDialogContext : ContentTreeContext, IDatabaseUriContext
    {
        public SelectItemDialogContext([NotNull] SelectItemDialog dialog, [NotNull] ItemTreeView contentTree, [NotNull] IEnumerable<BaseTreeViewItem> selectedItems) : base(contentTree, selectedItems)
        {
            Dialog = dialog;
        }

        [NotNull]
        public SelectItemDialog Dialog { get; }

        public void SetDatabaseUri(DatabaseUri databaseUri)
        {
            Dialog.SetDatabaseUri(databaseUri);
        }
    }
}
