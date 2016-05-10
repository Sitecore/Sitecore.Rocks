// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Linq;
using System.Windows;
using Microsoft.Win32;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Extensions.WindowExtensions;

namespace Sitecore.Rocks.UI.KeyboardSchemes
{
    public partial class KeyboardSchemeDialog
    {
        public KeyboardSchemeDialog()
        {
            InitializeComponent();
            this.InitializeDialog();
        }

        private void LoadScheme(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog
            {
                Title = "Load Keyboard Scheme",
                CheckFileExists = true,
                DefaultExt = "xml",
                Filter = "Xml Files (*.xml)|*.xml"
            };

            if (dialog.ShowDialog() != true)
            {
                return;
            }

            var shortcuts = KeyboardManager.Load(dialog.FileName);

            foreach (var shortcut in Editor.Shortcuts)
            {
                shortcut.FormattedKeys = string.Empty;
            }

            foreach (var shortcut in shortcuts)
            {
                if (!shortcut.Keys.Any())
                {
                    continue;
                }

                var sh = Editor.Shortcuts.FirstOrDefault(s => s.CommandName == shortcut.CommandName);
                if (sh == null)
                {
                    continue;
                }

                sh.FormattedKeys = shortcut.ToString();
            }
        }

        private void OkClick([NotNull] object sender, [NotNull] RoutedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            KeyboardManager.Shortcuts.Clear();
            KeyboardManager.Shortcuts.AddRange(Editor.Shortcuts);
            KeyboardManager.SaveActiveScheme();

            this.Close(true);
        }

        private void SaveScheme(object sender, RoutedEventArgs e)
        {
            var dialog = new SaveFileDialog
            {
                Title = "Save Keyboard Scheme",
                CheckPathExists = true,
                OverwritePrompt = true,
                DefaultExt = "xml",
                Filter = "Xml Files (*.xml)|*.xml"
            };

            if (dialog.ShowDialog() != true)
            {
                return;
            }

            KeyboardManager.Save(dialog.FileName, Editor.Shortcuts);
        }
    }
}
