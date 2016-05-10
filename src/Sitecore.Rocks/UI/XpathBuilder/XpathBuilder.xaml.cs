// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;
using System.Xml.Linq;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Data.DataServices;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Extensions.XElementExtensions;
using Sitecore.Rocks.Shell;
using Sitecore.Rocks.UI.Dialogs.SelectItemDialogs;

namespace Sitecore.Rocks.UI.XpathBuilder
{
    public partial class XpathBuilder
    {
        public XpathBuilder()
        {
            InitializeComponent();

            ContextNode.Text = AppHost.Settings.Get("XPath Builder", "Context Node", "/") as string ?? string.Empty;
            TextDocument.Text = AppHost.Settings.Get("XPath Builder", "XPath", "//*") as string ?? string.Empty;

            EnableButtons();
        }

        [NotNull]
        public DatabaseUri DatabaseUri { get; set; }

        [NotNull]
        public IEditorPane Pane { get; set; }

        public void Initialize([NotNull] DatabaseUri databaseUri)
        {
            Assert.ArgumentNotNull(databaseUri, nameof(databaseUri));

            DatabaseUri = databaseUri;
            Pane.Caption = string.Format(@"XPath Builder [{0}/{1}]", databaseUri.DatabaseName, databaseUri.Site.Name);
        }

        private void Browse([NotNull] object sender, [NotNull] RoutedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            var dialog = new SelectItemDialog();

            dialog.Initialize(Rocks.Resources.Browse, DatabaseUri, ContextNode.Text);

            if (AppHost.Shell.ShowDialog(dialog) != true)
            {
                return;
            }

            Action<string> setContextNode = delegate(string s) { ContextNode.Text = s; };

            dialog.GetSelectedItemPath(setContextNode);
        }

        private void EnableButtons()
        {
        }

        private void EvaluateXpath([NotNull] object sender, [NotNull] RoutedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            Execute();
        }

        private void Execute()
        {
            Result.ItemsSource = null;

            var contextNode = ContextNode.Text;
            if (string.Compare(contextNode, "sitecore", StringComparison.InvariantCultureIgnoreCase) == 0)
            {
                ContextNode.Text = "/" + ContextNode.Text;
                contextNode = ContextNode.Text;
            }

            if (contextNode.IndexOf("/", StringComparison.Ordinal) >= 0 && !contextNode.StartsWith("/"))
            {
                ContextNode.Text = "/" + ContextNode.Text;
                contextNode = ContextNode.Text;
            }

            var databaseName = DatabaseUri.DatabaseName.Name;
            var expression = TextDocument.Text;
            var mode = SitecoreXpath.IsChecked == true ? @"sitecore" : @"real";

            AppHost.Settings.Set("XPath Builder", "Context Node", contextNode);
            AppHost.Settings.Set("XPath Builder", "XPath", expression);

            ExecuteCompleted callback = delegate(string response, ExecuteResult executeResult)
            {
                Evaluate.IsEnabled = true;

                if (!DataService.HandleExecute(response, executeResult))
                {
                    return;
                }

                RenderItems(response);
            };

            NoItems.Visibility = Visibility.Collapsed;
            Evaluate.IsEnabled = false;

            DatabaseUri.Site.DataService.ExecuteAsync("UI.XpathBuilder.Evaluate", callback, contextNode, databaseName, expression, mode);
        }

        private void HandleKeyDown([NotNull] object sender, [NotNull] KeyEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            if (e.Key == Key.E && Keyboard.Modifiers == ModifierKeys.Control)
            {
                Execute();
                e.Handled = true;
            }
        }

        private void ParseResults([NotNull] string response, [NotNull] List<Hit> hits, [NotNull] out string elapsed)
        {
            Debug.ArgumentNotNull(response, nameof(response));
            Debug.ArgumentNotNull(hits, nameof(hits));

            response = response.Trim();

            elapsed = string.Empty;

            if (string.IsNullOrEmpty(response))
            {
                NoItems.Visibility = Visibility.Visible;
                return;
            }

            XDocument doc;
            try
            {
                doc = XDocument.Parse(response);
            }
            catch
            {
                AppHost.MessageBox(response, Rocks.Resources.Error, MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            var root = doc.Root;
            if (root == null)
            {
                return;
            }

            var index = 1;

            foreach (var element in root.Elements())
            {
                var hit = new Hit
                {
                    Index = index,
                    Name = element.GetAttributeValue("name"),
                    Path = element.GetAttributeValue("path")
                };

                index++;

                hits.Add(hit);
            }

            elapsed = root.GetAttributeValue("elapsed");
        }

        private void RenderItems([NotNull] string response)
        {
            Debug.ArgumentNotNull(response, nameof(response));

            var hits = new List<Hit>();
            string elapsed;

            ParseResults(response, hits, out elapsed);

            Result.ItemsSource = hits;

            if (hits.Count == 0)
            {
                Status.Text = Rocks.Resources.XpathBuilder_RenderItems_The_query_did_not_return_any_results_;
                return;
            }

            Status.Text = string.Format(Rocks.Resources.XpathBuilder_RenderItems_Found__0__result_s__in__1_ms, hits.Count.ToString(@"#,##0"), elapsed);
        }

        public class Hit
        {
            public int Index { get; set; }

            [NotNull]
            public string Name { get; set; }

            [NotNull]
            public string Path { get; set; }
        }
    }
}
