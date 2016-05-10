// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Xml.Linq;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Applications.Storages;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Data.DataServices;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Extensions.DataServiceExtensions;
using Sitecore.Rocks.Shell;
using Sitecore.Rocks.UI.Dialogs.SelectItemDialogs;

namespace Sitecore.Rocks.UI.SearchAndReplace
{
    public partial class SearchAndReplacePanel
    {
        public SearchAndReplacePanel()
        {
            InitializeComponent();

            EnableButtons();
        }

        [NotNull]
        public IPane Pane { get; set; }

        [NotNull]
        protected DatabaseUri DatabaseUri { get; set; }

        [NotNull]
        protected ItemUri RootItemUri { get; set; }

        public void Initialize([NotNull] DatabaseUri databaseUri)
        {
            Assert.ArgumentNotNull(databaseUri, nameof(databaseUri));

            DatabaseUri = databaseUri;
            RootItemUri = new ItemUri(databaseUri, ItemId.Empty);

            Pane.Caption = string.Format(@"Search And Replace [{0}/{1}]", databaseUri.DatabaseName, databaseUri.Site.Name);

            LoadHistory();
        }

        public void Initialize([NotNull] ItemUri itemUri)
        {
            Assert.ArgumentNotNull(itemUri, nameof(itemUri));

            DatabaseUri = itemUri.DatabaseUri;
            RootItemUri = itemUri;

            Pane.Caption = string.Format(@"Search And Replace [{0}/{1}]", itemUri.DatabaseName, itemUri.Site.Name);

            LoadHistory();

            GetValueCompleted<ItemHeader> completed = delegate(ItemHeader item) { Root.Text = item.Path; };

            RootItemUri.Site.DataService.GetItemHeader(RootItemUri, completed);
        }

        private void Browse([NotNull] object sender, [NotNull] RoutedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            var itemUri = RootItemUri;

            var dialog = new SelectItemDialog();

            dialog.Initialize(Rocks.Resources.Browse, itemUri);
            if (AppHost.Shell.ShowDialog(dialog) != true)
            {
                return;
            }

            RootItemUri = dialog.SelectedItemUri;

            dialog.GetSelectedItemPath(itemPath => Root.Text = itemPath);
        }

        private void CheckboxChecked([NotNull] object sender, [NotNull] RoutedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            if (ReplaceButton == null || ScriptEditor == null)
            {
                return;
            }

            RefreshScript();
            EnableButtons();
        }

        private void EnableButtons()
        {
            ReplaceButton.IsEnabled = !string.IsNullOrEmpty(Find.Text) && !string.IsNullOrEmpty(Replace.Text);
            ScriptTab.IsEnabled = ReplaceButton.IsEnabled;
            WholeField.IsEnabled = !string.IsNullOrEmpty(FieldName.Text);
        }

        private void LoadHistory()
        {
            LoadHistory("Find", FindCombo.Items);
            LoadHistory("Replace", ReplaceCombo.Items);
            LoadHistory("Field", FieldNameCombo.Items);
            LoadHistory("Template", TemplateNameCombo.Items);
        }

        private void LoadHistory([NotNull] string key, [NotNull] ItemCollection items)
        {
            Debug.ArgumentNotNull(key, nameof(key));
            Debug.ArgumentNotNull(items, nameof(items));

            key = "Search And Replace\\" + key;

            for (var n = 0; n < 25; n++)
            {
                var entry = AppHost.Settings.Get(key, "Item" + n, string.Empty) as string ?? string.Empty;
                if (string.IsNullOrEmpty(entry))
                {
                    break;
                }

                items.Add(entry);
            }
        }

        private void RefreshScript([NotNull] object sender, [NotNull] TextChangedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            RefreshScript();
            EnableButtons();
        }

        [Localizable(false)]
        private void RefreshScript()
        {
            var path = @"//*";
            if (!string.IsNullOrEmpty(Root.Text))
            {
                path = string.Empty;

                var parts = Root.Text.Split('/');
                foreach (var part in parts)
                {
                    if (!string.IsNullOrEmpty(part))
                    {
                        path += "#" + part + "#";
                    }

                    path += "/";
                }

                path = path + @"/*";
            }

            var field = string.Empty;
            if (!string.IsNullOrEmpty(FieldName.Text))
            {
                field = string.Format("\nin @#{0}#", FieldName.Text);
            }

            var fieldCondition = string.Empty;
            if (!string.IsNullOrEmpty(FieldName.Text))
            {
                if (WholeField.IsChecked == true)
                {
                    fieldCondition = string.Format("@#{0}# = \"{1}\"", FieldName.Text, Find.Text);
                }
                else
                {
                    fieldCondition = string.Format("contains(@#{0}#, \"{1}\")", FieldName.Text, Find.Text);
                }
            }

            var templateCondition = string.Empty;
            if (!string.IsNullOrEmpty(TemplateName.Text))
            {
                templateCondition = string.Format("@@templatename=\"{0}\"", TemplateName.Text);
            }

            var from = path;
            if (!string.IsNullOrEmpty(fieldCondition) || !string.IsNullOrEmpty(templateCondition))
            {
                from += "[";

                if (!string.IsNullOrEmpty(fieldCondition))
                {
                    from += fieldCondition;
                }

                if (!string.IsNullOrEmpty(fieldCondition) && !string.IsNullOrEmpty(templateCondition))
                {
                    from += " and ";
                }

                if (!string.IsNullOrEmpty(templateCondition))
                {
                    from += templateCondition;
                }

                from += "]";
            }

            var script = string.Format("replace \"{0}\" with \"{1}\"{2}\nfrom {3}", Find.Text, Replace.Text, field, from);

            ScriptEditor.Text = script;
        }

        private void ReplaceClick([NotNull] object sender, [NotNull] RoutedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            var script = ScriptEditor.Text;

            ExecuteCompleted c = delegate(string response, ExecuteResult result)
            {
                if (!DataService.HandleExecute(response, result))
                {
                    return;
                }

                XDocument doc;
                try
                {
                    doc = XDocument.Parse(response);
                }
                catch (Exception)
                {
                    AppHost.MessageBox(Rocks.Resources.SearchAndReplacePanel_ReplaceClick__0_rows_affected, Rocks.Resources.SearchAndReplacePanel_ReplaceClick_Search_And_Replace, MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                var root = doc.Root;
                if (root == null)
                {
                    AppHost.MessageBox(Rocks.Resources.SearchAndReplacePanel_ReplaceClick__0_rows_affected, Rocks.Resources.SearchAndReplacePanel_ReplaceClick_Search_And_Replace, MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                var table = root.Element(@"table");
                if (table == null)
                {
                    AppHost.MessageBox(Rocks.Resources.SearchAndReplacePanel_ReplaceClick__0_rows_affected, Rocks.Resources.SearchAndReplacePanel_ReplaceClick_Search_And_Replace, MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                var value = table.Element(@"value");
                if (value == null)
                {
                    AppHost.MessageBox(Rocks.Resources.SearchAndReplacePanel_ReplaceClick__0_rows_affected, Rocks.Resources.SearchAndReplacePanel_ReplaceClick_Search_And_Replace, MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                var text = value.Value.Replace(@"(", string.Empty).Replace(@")", string.Empty);
                AppHost.MessageBox(text, Rocks.Resources.SearchAndReplacePanel_ReplaceClick_Search_And_Replace, MessageBoxButton.OK, MessageBoxImage.Information);
            };

            SaveHistory();

            DatabaseUri.Site.DataService.ExecuteAsync("QueryAnalyzer.Run", c, DatabaseUri.DatabaseName.ToString(), string.Empty, script, "0");
        }

        private void SaveHistory()
        {
            SaveHistory("Find", FindCombo, FindCombo.Text);
            SaveHistory("Replace", ReplaceCombo, ReplaceCombo.Text);
            SaveHistory("Field", FieldNameCombo, FieldNameCombo.Text);
            SaveHistory("Template", TemplateNameCombo, TemplateNameCombo.Text);
        }

        private void SaveHistory([NotNull] string key, [NotNull] ComboBox comboBox, [NotNull] string text)
        {
            Debug.ArgumentNotNull(key, nameof(key));
            Debug.ArgumentNotNull(comboBox, nameof(comboBox));
            Debug.ArgumentNotNull(text, nameof(text));

            if (string.IsNullOrEmpty(text))
            {
                return;
            }

            comboBox.Items.Remove(text);
            comboBox.Items.Insert(0, text);
            comboBox.Text = text;

            while (comboBox.Items.Count > 25)
            {
                comboBox.Items.RemoveAt(25);
            }

            key = "Search And Replace\\" + key;
            Storage.Delete(key);

            for (var index = 0; index < comboBox.Items.Count; index++)
            {
                var item = comboBox.Items[index] as string ?? string.Empty;

                AppHost.Settings.Set(key, "Item" + index, item);
            }
        }
    }
}
