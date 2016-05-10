// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Xml.Linq;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Data.DataServices;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Extensions.StringExtensions;
using Sitecore.Rocks.Extensions.WindowExtensions;
using Sitecore.Rocks.Extensions.XElementExtensions;

namespace Sitecore.Rocks.ContentTrees.Dialogs
{
    public partial class SetDisplayNamesDialog
    {
        private readonly List<TextBox> textBoxes = new List<TextBox>();

        public SetDisplayNamesDialog([NotNull] ItemUri itemUri)
        {
            Assert.ArgumentNotNull(itemUri, nameof(itemUri));

            InitializeComponent();
            this.InitializeDialog();

            ItemUri = itemUri;

            Loaded += ControlLoaded;
        }

        [NotNull]
        public IEnumerable<Tuple<string, string>> DisplayNames
        {
            get
            {
                foreach (var textBox in textBoxes)
                {
                    yield return new Tuple<string, string>(textBox.Tag as string ?? string.Empty, textBox.Text);
                }
            }
        }

        [NotNull]
        protected ItemUri ItemUri { get; set; }

        private void ControlLoaded([NotNull] object sender, [NotNull] RoutedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            Loaded -= ControlLoaded;

            ExecuteCompleted completed = delegate(string response, ExecuteResult result)
            {
                if (!DataService.HandleExecute(response, result))
                {
                    return;
                }

                var root = response.ToXElement();
                if (root == null)
                {
                    return;
                }

                Display(root);

                EnableButtons();

                Loading.HideLoading(GridViewer);
                if (!textBoxes.Any())
                {
                    return;
                }

                var textBox = textBoxes.First();
                textBox.Focus();
                textBox.SelectAll();
                Keyboard.Focus(textBox);
            };

            ItemUri.Site.DataService.ExecuteAsync("Items.GetDisplayNames", completed, ItemUri.ItemId.ToString(), ItemUri.DatabaseName.ToString());

            EnableButtons();
        }

        private void Display([NotNull] XElement root)
        {
            Debug.ArgumentNotNull(root, nameof(root));

            textBoxes.Clear();
            Grid.Children.Clear();

            var row = 0;

            foreach (var element in root.Elements().OrderBy(e => e.GetAttributeValue("language")))
            {
                var languageName = element.GetAttributeValue("language");
                var displayName = element.GetAttributeValue("name");

                var textBlock = new TextBlock(new Run(languageName + @":"));
                textBlock.SetValue(Grid.ColumnProperty, 0);
                textBlock.SetValue(Grid.RowProperty, row);
                textBlock.VerticalAlignment = VerticalAlignment.Center;
                textBlock.Margin = new Thickness(0, 2, 8, 2);
                Grid.Children.Add(textBlock);

                var textBox = new TextBox();
                textBox.Text = displayName;
                textBox.Tag = languageName;
                textBox.VerticalAlignment = VerticalAlignment.Center;
                textBox.Margin = new Thickness(0, 2, 0, 2);
                textBox.SetValue(Grid.ColumnProperty, 1);
                textBox.SetValue(Grid.RowProperty, row);
                Grid.Children.Add(textBox);
                textBoxes.Add(textBox);

                Grid.RowDefinitions.Add(new RowDefinition());

                row++;
            }
        }

        private void EnableButtons()
        {
        }

        private void OkClick([NotNull] object sender, [NotNull] RoutedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            this.Close(true);
        }
    }
}
