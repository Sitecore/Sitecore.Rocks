// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Extensions.WindowExtensions;

namespace Sitecore.Rocks.Controls.Dialogs.BrowseTypeName
{
    public partial class BrowseTypeNameDialog
    {
        private readonly List<TextBox> itemNames = new List<TextBox>();

        private string selectedTypeName;

        public BrowseTypeNameDialog([NotNull] string selectedTypeName)
        {
            Assert.ArgumentNotNull(selectedTypeName, nameof(selectedTypeName));

            InitializeComponent();
            this.InitializeDialog();

            TypeNameSelector.InitialTypeName = selectedTypeName;

            Loaded += ControlLoaded;
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

        [NotNull]
        public string SelectedTypeName
        {
            get
            {
                var typeNameDescriptor = TypeNameSelector.SelectedTypeName;
                if (typeNameDescriptor == null)
                {
                    return string.Empty;
                }

                return typeNameDescriptor.FullName;
            }

            set
            {
                Assert.ArgumentNotNull(value, nameof(value));

                selectedTypeName = value;
            }
        }

        private void ControlLoaded([NotNull] object sender, [NotNull] RoutedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            Loaded -= ControlLoaded;

            Keyboard.Focus(TypeNameSelector.TemplateSelectorFilter.TextBox);

            EnableButtons();
        }

        private void EnableButtons()
        {
            OK.IsEnabled = TypeNameSelector.SelectedTypeName != null;
        }

        private void EnableButtons([NotNull] object sender, [NotNull] SelectionChangedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            EnableButtons();
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

            this.Close(true);
        }
    }
}
