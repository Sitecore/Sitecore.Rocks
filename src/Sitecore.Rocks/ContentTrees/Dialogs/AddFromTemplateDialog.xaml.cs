// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Extensions.StringExtensions;
using Sitecore.Rocks.Extensions.WindowExtensions;

namespace Sitecore.Rocks.ContentTrees.Dialogs
{
    public partial class AddFromTemplateDialog
    {
        private readonly List<TextBox> itemNames = new List<TextBox>();

        public AddFromTemplateDialog()
        {
            InitializeComponent();
            this.InitializeDialog();

            Loaded += ControlLoaded;

            itemNames.Add(NewItemName);
        }

        [NotNull]
        public DatabaseUri DatabaseUri
        {
            get { return TemplateSelector.DatabaseUri; }

            set
            {
                Assert.ArgumentNotNull(value, nameof(value));

                TemplateSelector.DatabaseUri = value;
            }
        }

        [NotNull]
        public string ItemName
        {
            get { return NewItemName.Text ?? string.Empty; }
        }

        [NotNull]
        public IEnumerable<string> ItemNames
        {
            get
            {
                foreach (var textBox in itemNames)
                {
                    var text = textBox.Text;
                    if (!string.IsNullOrEmpty(text))
                    {
                        yield return text;
                    }
                }
            }
        }

        [CanBeNull]
        public TemplateHeader SelectedTemplate
        {
            get { return TemplateSelector.SelectedTemplate; }
        }

        private void AddMore([NotNull] object sender, [NotNull] RoutedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            TextBox textBox = null;

            for (var index = 0; index < Count.Value; index++)
            {
                textBox = new TextBox
                {
                    Margin = new Thickness(0, 4, 0, 0),
                    Text = GetDefaultName()
                };

                itemNames.Add(textBox);

                ItemNamesStack.Children.Add(textBox);

                ScrollViewer.ScrollToBottom();
            }

            Keyboard.Focus(textBox);
        }

        private void ControlLoaded([NotNull] object sender, [NotNull] RoutedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            Loaded -= ControlLoaded;

            Keyboard.Focus(TemplateSelector.FilterTextBox.TextBox);
            TemplateSelector.FilterTextBox.TextBox.SelectAll();

            EnableButtons();
        }

        private void EnableButtons()
        {
            OK.IsEnabled = TemplateSelector.SelectedTemplate != null;
        }

        [NotNull]
        private string GetDefaultName()
        {
            var last = ItemNamesStack.Children.OfType<TextBox>().LastOrDefault(t => !string.IsNullOrEmpty(t.Text));
            if (last == null)
            {
                return string.Empty;
            }

            var text = last.Text;
            var charArray = text.ToCharArray();

            var firstDigit = -1;
            for (var index = charArray.Length - 1; index >= 0; index--)
            {
                var c = charArray[index];

                if (!char.IsDigit(c))
                {
                    firstDigit = index + 1;
                    break;
                }
            }

            if (firstDigit < 0)
            {
                return text + @" 2";
            }

            var s = text.Mid(firstDigit);
            if (string.IsNullOrEmpty(s))
            {
                return text + @" 2";
            }

            int value;
            if (!int.TryParse(s, out value))
            {
                return text + @" 2";
            }

            return text.Left(firstDigit).TrimEnd() + @" " + (value + 1);
        }

        private void HandleDoubleClick([NotNull] object sender, [NotNull] MouseButtonEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            OkClick(sender, e);
        }

        private void OkClick([NotNull] object sender, [NotNull] RoutedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            var selectedTemplate = SelectedTemplate;
            if (selectedTemplate != null)
            {
                TemplateSelector.AddToRecent(selectedTemplate);
            }

            this.Close(true);
        }

        private void TemplateSelectionChanged([NotNull] object sender, [NotNull] SelectionChangedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            EnableButtons();

            var template = SelectedTemplate;

            if (template != null && NewItemName.Text == @"New Template")
            {
                NewItemName.Text = template.Name;
            }
        }
    }
}
