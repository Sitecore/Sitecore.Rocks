// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Extensions.WindowExtensions;

namespace Sitecore.Rocks.UI.Management.ManagementItems.Validations.Dialogs
{
    public partial class ManageCustomValidationsDialog
    {
        public ManageCustomValidationsDialog()
        {
            InitializeComponent();
            this.InitializeDialog();

            Refresh();
        }

        private void Add([NotNull] object sender, [NotNull] RoutedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            var customValidation = new CustomValidation();

            var d = new EditCustomValidationDialog(customValidation);
            if (AppHost.Shell.ShowDialog(d) != true)
            {
                return;
            }

            CustomValidationManager.Add(customValidation);

            CheckList.SelectedIndex = Refresh(customValidation);

            EnableButtons();
        }

        private void CloseClick([NotNull] object sender, [NotNull] RoutedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            this.Close(false);
        }

        private void Delete([NotNull] object sender, [NotNull] RoutedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            var selectedIndex = CheckList.SelectedIndex;
            var selectedItem = CheckList.SelectedItem as ListBoxItem;
            if (selectedItem == null)
            {
                return;
            }

            var customValidation = selectedItem.Tag as CustomValidation;
            if (customValidation == null)
            {
                Trace.Expected(typeof(CustomValidation));
                return;
            }

            if (AppHost.MessageBox(string.Format(Rocks.Resources.MacroOrganizer_DeleteClick_Are_you_sure_you_want_to_delete__0__, customValidation.Title), Rocks.Resources.Confirmation, MessageBoxButton.OKCancel, MessageBoxImage.Question) != MessageBoxResult.OK)
            {
                return;
            }

            CustomValidationManager.Delete(customValidation);

            CheckList.Items.Remove(selectedItem);
            selectedIndex--;

            if (selectedIndex < 0)
            {
                selectedIndex = 0;
            }

            if (selectedIndex < CheckList.Items.Count)
            {
                CheckList.SelectedIndex = selectedIndex;
            }

            EnableButtons();
        }

        private void Edit([NotNull] object sender, [NotNull] RoutedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            var selectedItem = CheckList.SelectedItem as ListBoxItem;
            if (selectedItem == null)
            {
                return;
            }

            var customValidation = selectedItem.Tag as CustomValidation;
            if (customValidation == null)
            {
                Trace.Expected(typeof(CustomValidation));
                return;
            }

            var d = new EditCustomValidationDialog(customValidation);
            if (AppHost.Shell.ShowDialog(d) != true)
            {
                return;
            }

            CustomValidationManager.Update(customValidation);

            selectedItem.Content = customValidation.Title;

            EnableButtons();
        }

        private void EnableButtons([NotNull] object sender, [NotNull] SelectionChangedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            EnableButtons();
        }

        private void EnableButtons()
        {
            EditButton.IsEnabled = CheckList.SelectedIndex >= 0;
            DeleteButton.IsEnabled = CheckList.SelectedIndex >= 0;
        }

        private void OnMouseDoubleClick([NotNull] object sender, [NotNull] MouseButtonEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            Edit(sender, e);
        }

        private void Refresh()
        {
            CheckList.Items.Clear();

            foreach (var customValidation in CustomValidationManager.CustomValidations)
            {
                Refresh(customValidation);
            }

            EnableButtons();
        }

        private int Refresh([NotNull] CustomValidation customValidation)
        {
            Debug.ArgumentNotNull(customValidation, nameof(customValidation));

            var listBoxItem = new ListBoxItem
            {
                Tag = customValidation,
                Content = customValidation.Title
            };

            return CheckList.Items.Add(listBoxItem);
        }
    }
}
