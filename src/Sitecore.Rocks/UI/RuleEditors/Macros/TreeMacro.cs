// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.Windows;
using System.Windows.Documents;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.ContentTrees.Items;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Extensions.GuidExtensions;
using Sitecore.Rocks.UI.Dialogs.SelectItemDialogs;

namespace Sitecore.Rocks.UI.RuleEditors.Macros
{
    [RuleEditorMacro("tree")]
    public class TreeMacro : RuleEditorMacroBase
    {
        private Hyperlink control;

        private Run run;

        public override object GetEditableControl()
        {
            var value = GetValue();

            run = new Run(value);
            control = new Hyperlink(run);
            control.Click += Edit;

            if (value.IsGuid())
            {
                GetItemName(value, s => run.Text = s);
            }

            return control;
        }

        private void Edit([NotNull] object sender, [NotNull] RoutedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            Guid guid;
            if (!Guid.TryParse(GetValue(), out guid))
            {
                guid = DatabaseTreeViewItem.RootItemGuid;
            }

            var itemUri = new ItemUri(DatabaseUri, new ItemId(guid));

            var dialog = new SelectItemDialog();
            dialog.Initialize(Resources.TreeMacro_Edit_Select_Item, itemUri);

            if (AppHost.Shell.ShowDialog(dialog) != true)
            {
                return;
            }

            var item = dialog.SelectedItemUri;
            if (item == ItemUri.Empty)
            {
                return;
            }

            Value = item.ItemId.ToString();
            run.Text = dialog.SelectedItemName;
        }
    }
}
