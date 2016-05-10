// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.ContentTrees.Commands.Tasks;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Extensions.WindowExtensions;

namespace Sitecore.Rocks.ContentTrees.Dialogs.FieldPainters
{
    public partial class FieldPainterDialog
    {
        public FieldPainterDialog([NotNull] ItemUri itemUri)
        {
            Assert.ArgumentNotNull(itemUri, nameof(itemUri));

            InitializeComponent();
            this.InitializeDialog();

            ItemUri = itemUri;

            Loaded += ControlLoaded;
        }

        [NotNull]
        public ItemUri ItemUri { get; }

        internal void ControlLoaded([NotNull] object sender, [NotNull] RoutedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            Loaded -= ControlLoaded;

            ItemUri.Site.DataService.GetItemFieldsAsync(new ItemVersionUri(ItemUri, LanguageManager.CurrentLanguage, Data.Version.Latest), RenderFields);
        }

        private void Checked([NotNull] object sender, [NotNull] RoutedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            var checkBox = sender as CheckBox;
            if (checkBox == null)
            {
                return;
            }

            var field = checkBox.Tag as Field;
            if (field == null)
            {
                return;
            }

            var stackPanel = new StackPanel
            {
                Tag = field
            };

            stackPanel.Children.Add(new Label()
            {
                Content = field.Name
            });

            stackPanel.Children.Add(new TextBox()
            {
                Text = field.Value,
                TextWrapping = TextWrapping.Wrap
            });

            TargetStackPanel.Children.Add(stackPanel);
        }

        private void OkClick([NotNull] object sender, [NotNull] RoutedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            ApplyFieldPainter.Fields.Clear();

            foreach (var stackPanel in TargetStackPanel.Children.OfType<StackPanel>())
            {
                var field = stackPanel.Tag as Field;
                if (field == null)
                {
                    continue;
                }

                var textBox = stackPanel.Children[1] as TextBox;
                if (textBox == null)
                {
                    continue;
                }

                var text = textBox.Text;

                ApplyFieldPainter.Fields.Add(new Tuple<Field, string>(field, text));
            }

            this.Close(true);
        }

        private void RenderFields([NotNull] Item item)
        {
            Debug.ArgumentNotNull(item, nameof(item));

            string section = null;

            foreach (var field in item.Fields.OrderBy(f => f.Section.SortOrder).ThenBy(f => f.Section.Name).ThenBy(f => f.SortOrder).ThenBy(f => f.Name))
            {
                var checkBox = new CheckBox
                {
                    Tag = field,
                    Content = field.Name
                };

                checkBox.Checked += Checked;
                checkBox.Unchecked += Unchecked;

                if (field.Section.Name != section)
                {
                    SourceStackPanel.Children.Add(new TextBlock()
                    {
                        Text = field.Section.Name,
                        Foreground = SystemColors.HighlightBrush,
                        Margin = new Thickness(0, 4, 0, 0)
                    });

                    section = field.Section.Name;
                }

                SourceStackPanel.Children.Add(checkBox);
            }

            Loading.HideLoading(FieldsGrid);
            OkButton.IsEnabled = true;
        }

        private void Unchecked([NotNull] object sender, [NotNull] RoutedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            var checkBox = sender as CheckBox;
            if (checkBox == null)
            {
                return;
            }

            var field = checkBox.Tag as Field;
            if (field == null)
            {
                return;
            }

            var stackPanel = TargetStackPanel.Children.OfType<StackPanel>().FirstOrDefault(c => c.Tag == field);
            if (stackPanel == null)
            {
                return;
            }

            stackPanel.Tag = null;
            TargetStackPanel.Children.Remove(stackPanel);
        }
    }
}
