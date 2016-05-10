// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Rules;

namespace Sitecore.Rocks.UI.Macros.Dialogs
{
    public partial class MacroEditor
    {
        public MacroEditor()
        {
            InitializeComponent();

            LoadMacros();

            var macroName = GetPostSaveMacroName();
            LoadPostSaveMacro(macroName);

            Loaded += ControlLoaded;
        }

        private void AddClick([NotNull] object sender, [NotNull] RoutedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            var macro = new Macro(new Rule(), Rocks.Resources.MacroOrganizer_AddClick_New_Macro);

            var d = new MacroDesigner();
            d.Initialize(macro, null);

            if (AppHost.Shell.ShowDialog(d) != true)
            {
                return;
            }

            MacroManager.Add(macro);
            MacroList.SelectedIndex = LoadMacro(macro);

            EnableButtons();
        }

        private void ControlLoaded([NotNull] object sender, [NotNull] RoutedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            Loaded -= ControlLoaded;
            FocusMacroList();
        }

        private void DeleteClick([NotNull] object sender, [NotNull] RoutedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            var selectedIndex = MacroList.SelectedIndex;
            var selectedItem = MacroList.SelectedItem as ListBoxItem;
            if (selectedItem == null)
            {
                return;
            }

            var macro = selectedItem.Tag as Macro;
            if (macro == null)
            {
                Trace.Expected(typeof(Macro));
                return;
            }

            if (AppHost.MessageBox(string.Format(Rocks.Resources.MacroOrganizer_DeleteClick_Are_you_sure_you_want_to_delete__0__, macro.Text), Rocks.Resources.Confirmation, MessageBoxButton.OKCancel, MessageBoxImage.Question) != MessageBoxResult.OK)
            {
                return;
            }

            MacroManager.Delete(macro);

            MacroList.Items.Remove(selectedItem);
            selectedIndex--;

            if (selectedIndex < 0)
            {
                selectedIndex = 0;
            }

            if (selectedIndex < MacroList.Items.Count)
            {
                MacroList.SelectedIndex = selectedIndex;
            }

            var postSaveMacro = GetPostSaveMacroName();
            if (MacroManager.Macros.FirstOrDefault(m => m.Text == postSaveMacro) == null)
            {
                SetPostSaveMacroName(string.Empty);
                PostSaveMacros.Items.Clear();
            }

            EnableButtons();
        }

        private void EditClick([NotNull] object sender, [NotNull] RoutedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            var selectedItem = MacroList.SelectedItem as ListBoxItem;
            if (selectedItem == null)
            {
                return;
            }

            var macro = selectedItem.Tag as Macro;
            if (macro == null)
            {
                Trace.Expected(typeof(Macro));
                return;
            }

            var oldName = macro.Text;

            var d = new MacroDesigner();
            d.Initialize(macro, null);

            if (AppHost.Shell.ShowDialog(d) != true)
            {
                return;
            }

            MacroManager.Save();
            selectedItem.Content = macro.Text;

            if (GetPostSaveMacroName() == oldName)
            {
                SetPostSaveMacroName(macro.Text);
                LoadPostSaveMacro(macro.Text);
            }

            EnableButtons();
        }

        private void EnableButtons()
        {
            AddButton.IsEnabled = true;
            EditButton.IsEnabled = MacroList.SelectedItem != null;
            DeleteButton.IsEnabled = MacroList.SelectedItem != null;
        }

        private void EnableButtons([NotNull] object sender, [NotNull] SelectionChangedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            EnableButtons();
        }

        private void FocusMacroList()
        {
            var element = MacroList.SelectedItem as IInputElement ?? MacroList;
            Keyboard.Focus(element);
        }

        [NotNull]
        private string GetPostSaveMacroName()
        {
            return AppHost.Settings.GetString(@"Content Editor", "Post Save Macro", string.Empty);
        }

        private int LoadMacro([NotNull] Macro macro)
        {
            Debug.ArgumentNotNull(macro, nameof(macro));

            var listBoxItem = new ListBoxItem
            {
                Tag = macro,
                Content = macro.Text
            };

            return MacroList.Items.Add(listBoxItem);
        }

        private void LoadMacros()
        {
            MacroList.Items.Clear();

            foreach (var macro in MacroManager.GetAllMacros().OrderBy(m => m.Text))
            {
                LoadMacro(macro);
            }

            EnableButtons();
        }

        private void LoadPostSaveMacro([NotNull] string macroName)
        {
            Debug.ArgumentNotNull(macroName, nameof(macroName));

            PostSaveMacros.Items.Clear();
            if (string.IsNullOrEmpty(macroName))
            {
                return;
            }

            var macro = MacroManager.Macros.FirstOrDefault(m => m.Text == macroName);
            if (macro == null)
            {
                return;
            }

            var comboBoxItem = new ComboBoxItem
            {
                Content = macro.Text,
                Tag = macro
            };

            PostSaveMacros.SelectedIndex = PostSaveMacros.Items.Add(comboBoxItem);
        }

        private void LoadPostSaveMacros([NotNull] object sender, [NotNull] EventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            var macroName = GetPostSaveMacroName();

            PostSaveMacros.Items.Clear();

            PostSaveMacros.Items.Add(string.Empty);

            foreach (var macro in MacroManager.GetAllMacros().OrderBy(m => m.Text))
            {
                var comboBoxItem = new ComboBoxItem
                {
                    Tag = macro,
                    Content = macro.Text,
                    IsSelected = macro.Text == macroName
                };

                PostSaveMacros.Items.Add(comboBoxItem);
            }
        }

        private void OnMouseDoubleClick([NotNull] object sender, [NotNull] MouseButtonEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            EditClick(sender, e);
        }

        private void SetPostSaveMacro([NotNull] object sender, [NotNull] SelectionChangedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            var selectedItem = PostSaveMacros.SelectedItem as ComboBoxItem;

            if (selectedItem == null)
            {
                SetPostSaveMacroName(string.Empty);
                return;
            }

            var macro = selectedItem.Tag as Macro;
            if (macro == null)
            {
                return;
            }

            SetPostSaveMacroName(macro.Text);
        }

        private void SetPostSaveMacroName([NotNull] string name)
        {
            Debug.ArgumentNotNull(name, nameof(name));

            AppHost.Settings.Set(@"Content Editor", "Post Save Macro", name);
        }
    }
}
