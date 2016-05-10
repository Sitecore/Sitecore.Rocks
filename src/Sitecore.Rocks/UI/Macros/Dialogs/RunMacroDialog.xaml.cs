// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Extensions.WindowExtensions;
using Sitecore.Rocks.UI.Rules;

namespace Sitecore.Rocks.UI.Macros.Dialogs
{
    public partial class RunMacroDialog
    {
        private static string last = string.Empty;

        public RunMacroDialog()
        {
            InitializeComponent();
            this.InitializeDialog();

            LoadMacros();

            Loaded += ControlLoaded;
        }

        [CanBeNull]
        public Macro Macro { get; set; }

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

            FocusMacroList();
        }

        private void EnableButtons()
        {
            OKButton.IsEnabled = MacroList.SelectedItem != null;
        }

        private void FocusMacroList()
        {
            var element = MacroList.SelectedItem as IInputElement ?? MacroList;
            Keyboard.Focus(element);
        }

        private void LoadMacros()
        {
            MacroList.Items.Clear();

            foreach (var macro in MacroManager.GetAllMacros().OrderBy(m => m.Text))
            {
                var listBoxItem = new ListBoxItem
                {
                    Tag = macro,
                    Content = macro.Text,
                    IsSelected = macro.Text == last
                };

                MacroList.Items.Add(listBoxItem);
            }

            if (MacroList.SelectedIndex < 0 && MacroList.Items.Count > 0)
            {
                MacroList.SelectedIndex = 0;
            }

            EnableButtons();
        }

        private void ManageMacros([NotNull] object sender, [NotNull] RoutedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            var d = new ManageMacroDialog();
            AppHost.Shell.ShowDialog(d);

            LoadMacros();

            FocusMacroList();

            EnableButtons();
        }

        private void OkClick([NotNull] object sender, [NotNull] RoutedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            var selectedItem = MacroList.SelectedItem as ListBoxItem;
            if (selectedItem != null)
            {
                var macro = selectedItem.Tag as Macro;
                if (macro != null)
                {
                    Macro = macro;
                    last = macro.Text;
                }
            }

            this.Close(true);
        }

        private void OnMouseDoubleClick([NotNull] object sender, [NotNull] MouseButtonEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            OkClick(sender, e);
        }

        private void RenderRule([NotNull] object sender, [NotNull] SelectionChangedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            Rule.Items.Clear();

            var selectedItem = MacroList.SelectedItem as ListBoxItem;
            if (selectedItem != null)
            {
                var macro = selectedItem.Tag as Macro;

                if (macro != null)
                {
                    RuleRenderer.Render(macro.Rule, Rule);
                }
            }

            EnableButtons();
        }
    }
}
