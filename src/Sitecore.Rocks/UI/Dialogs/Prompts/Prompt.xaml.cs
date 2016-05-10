// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Windows;
using System.Windows.Input;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Extensions.WindowExtensions;

namespace Sitecore.Rocks.UI.Dialogs.Prompts
{
    public partial class Prompt
    {
        public Prompt()
        {
            InitializeComponent();
            this.InitializeDialog();

            Loaded += ControlLoaded;
        }

        [NotNull]
        public string Text
        {
            get { return TextLabel.Content as string ?? string.Empty; }

            set
            {
                Assert.ArgumentNotNull(value, nameof(value));

                TextLabel.Content = value;
            }
        }

        [NotNull]
        public string Value
        {
            get { return TextBox.Text ?? string.Empty; }

            set
            {
                Assert.ArgumentNotNull(value, nameof(value));

                TextBox.Text = value;
            }
        }

        [CanBeNull]
        public static string Show([NotNull] string text)
        {
            Assert.ArgumentNotNull(text, nameof(text));

            return Show(text, Rocks.Resources.Prompt);
        }

        [CanBeNull]
        public static string Show([NotNull] string text, [NotNull] string title)
        {
            Assert.ArgumentNotNull(text, nameof(text));
            Assert.ArgumentNotNull(title, nameof(title));

            return Show(text, title, string.Empty);
        }

        [CanBeNull]
        public static string Show([NotNull] string text, [NotNull] string title, [NotNull] string value)
        {
            Assert.ArgumentNotNull(text, nameof(text));
            Assert.ArgumentNotNull(title, nameof(title));
            Assert.ArgumentNotNull(value, nameof(value));

            var d = new Prompt
            {
                Title = title,
                Text = text,
                Value = value
            };

            if (AppHost.Shell.ShowDialog(d) != true)
            {
                return null;
            }

            return d.Value;
        }

        private void CancelClick([NotNull] object sender, [NotNull] RoutedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            this.Close(false);
        }

        private void ControlLoaded([NotNull] object sender, [NotNull] RoutedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            Loaded -= ControlLoaded;

            TextBox.Focus();
            TextBox.SelectAll();
            Keyboard.Focus(TextBox);
        }

        private void OkClick([NotNull] object sender, [NotNull] RoutedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            this.Close(true);
        }
    }
}
