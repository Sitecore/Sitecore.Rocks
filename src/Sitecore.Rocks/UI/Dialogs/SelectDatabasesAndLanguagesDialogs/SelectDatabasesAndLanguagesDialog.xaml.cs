// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Data.DataServices;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Extensions.StringExtensions;
using Sitecore.Rocks.Extensions.WindowExtensions;
using Sitecore.Rocks.Extensions.XElementExtensions;
using Sitecore.Rocks.Sites;

namespace Sitecore.Rocks.UI.Dialogs.SelectDatabasesAndLanguagesDialogs
{
    public partial class SelectDatabasesAndLanguagesDialog
    {
        public SelectDatabasesAndLanguagesDialog([NotNull] Site site, [NotNull] string selected)
        {
            Assert.ArgumentNotNull(site, nameof(site));
            Assert.ArgumentNotNull(selected, nameof(selected));

            InitializeComponent();
            this.InitializeDialog();

            Selected = selected;
            LoadDatabases(site, selected);
        }

        [CanBeNull]
        public string Selected { get; set; }

        private void CheckLanguage([NotNull] object sender, [NotNull] RoutedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            var checkBox = sender as CheckBox;
            if (checkBox == null)
            {
                return;
            }

            var databaseBox = checkBox.Tag as CheckBox;
            if (databaseBox == null)
            {
                return;
            }

            databaseBox.IsChecked = true;
        }

        private void LoadDatabases([NotNull] Site site, [NotNull] string selected)
        {
            Debug.ArgumentNotNull(site, nameof(site));
            Debug.ArgumentNotNull(selected, nameof(selected));

            ExecuteCompleted completed = delegate(string response, ExecuteResult result)
            {
                if (!DataService.HandleExecute(response, result))
                {
                    Loading.Swap(Border);
                    return;
                }

                var root = response.ToXElement();
                if (root == null)
                {
                    return;
                }

                var selectedBoxes = new Dictionary<string, List<string>>();
                foreach (var parts in selected.Split('|'))
                {
                    if (string.IsNullOrEmpty(parts))
                    {
                        continue;
                    }

                    var p = parts.Split('^');
                    var languages = new List<string>();

                    for (var i = 1; i < p.Length; i++)
                    {
                        languages.Add(p[i]);
                    }

                    selectedBoxes[p[0].ToLowerInvariant()] = languages;
                }

                foreach (var databaseElement in root.Elements())
                {
                    var languages = new List<CheckBox>();
                    var database = databaseElement.GetAttributeValue("name").ToLowerInvariant();
                    var selectedDatabase = true;

                    List<string> selectedLanguages;
                    if (!selectedBoxes.TryGetValue(database, out selectedLanguages))
                    {
                        selectedDatabase = false;
                        selectedLanguages = new List<string>();
                    }

                    var databaseBox = new CheckBox
                    {
                        Content = database.Capitalize(),
                        Tag = languages,
                        Margin = new Thickness(0, 8, 0, 2),
                        IsChecked = selectedDatabase
                    };

                    databaseBox.Unchecked += UncheckDatabase;
                    List.Children.Add(databaseBox);

                    foreach (var languageElement in databaseElement.Elements())
                    {
                        var languageName = languageElement.GetAttributeValue("name");
                        var languageDisplayName = languageName + " - " + languageElement.GetAttributeValue("displayname");
                        var languageBox = new CheckBox
                        {
                            Content = languageDisplayName,
                            Tag = databaseBox,
                            Margin = new Thickness(16, 1, 0, 1),
                            IsChecked = selectedLanguages.Contains(languageName)
                        };

                        languageBox.Checked += CheckLanguage;
                        List.Children.Add(languageBox);
                        languages.Add(languageBox);
                    }
                }

                Loading.Swap(Border);
            };

            List.Children.Clear();

            site.DataService.ExecuteAsync("Sites.GetDatabasesAndLanguages", completed);
        }

        private void OkClicked([NotNull] object sender, [NotNull] RoutedEventArgs routedEventArgs)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(routedEventArgs, nameof(routedEventArgs));

            var sb = new StringBuilder();

            foreach (var child in List.Children)
            {
                var checkBox = child as CheckBox;
                if (checkBox == null)
                {
                    continue;
                }

                var list = checkBox.Tag as List<CheckBox>;
                if (list == null)
                {
                    continue;
                }

                if (checkBox.IsChecked != true)
                {
                    continue;
                }

                if (sb.Length > 0)
                {
                    sb.Append('|');
                }

                sb.Append((checkBox.Content as string ?? string.Empty).ToLowerInvariant());

                foreach (var box in list)
                {
                    if (box.IsChecked != true)
                    {
                        continue;
                    }

                    var content = box.Content as string ?? string.Empty;
                    var n = content.IndexOf(" - ", StringComparison.InvariantCultureIgnoreCase);
                    if (n >= 0)
                    {
                        content = content.Left(n);
                    }

                    sb.Append("^");
                    sb.Append(content);
                }
            }

            Selected = sb.ToString();

            this.Close(true);
        }

        private void UncheckDatabase([NotNull] object sender, [NotNull] RoutedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            var checkBox = sender as CheckBox;
            if (checkBox == null)
            {
                return;
            }

            var languages = checkBox.Tag as List<CheckBox>;
            if (languages == null)
            {
                return;
            }

            foreach (var languageBox in languages)
            {
                languageBox.IsChecked = false;
            }
        }
    }
}
