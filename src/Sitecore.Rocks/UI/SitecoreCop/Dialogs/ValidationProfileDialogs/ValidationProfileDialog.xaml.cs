// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Xml.Linq;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Applications.Storages;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Data.DataServices;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Extensions.StringExtensions;
using Sitecore.Rocks.Extensions.WindowExtensions;
using Sitecore.Rocks.Extensions.XElementExtensions;
using Sitecore.Rocks.Sites;
using Sitecore.Rocks.UI.Management.ManagementItems.Validations;

namespace Sitecore.Rocks.UI.SitecoreCop.Dialogs.ValidationProfileDialogs
{
    public partial class ValidationProfileDialog
    {
        private readonly List<Validation> validations = new List<Validation>();

        public ValidationProfileDialog([NotNull] Site site, [NotNull] string settingsKey)
        {
            Assert.ArgumentNotNull(site, nameof(site));
            Assert.ArgumentNotNull(settingsKey, nameof(settingsKey));

            InitializeComponent();
            this.InitializeDialog();

            Site = site;
            SettingsKey = settingsKey;

            Loaded += ControlLoaded;
        }

        [CanBeNull]
        public string SelectedValue { get; set; }

        [NotNull]
        public string SettingsKey { get; }

        [NotNull]
        public Site Site { get; }

        protected bool IsChanging { get; set; }

        private void AddProfile([NotNull] object sender, [NotNull] RoutedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            var name = AppHost.Prompt("Enter the name of the profile:", "New Profile", "Profile");
            var listBoxItem = new ListBoxItem
            {
                Content = name,
                Tag = string.Empty
            };

            ProfileListBox.Items.Add(listBoxItem);

            listBoxItem.IsSelected = true;
            Keyboard.Focus(listBoxItem);

            EnableButtons();
        }

        private void CheckAll([NotNull] object sender, [NotNull] RoutedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            SetCheckBoxes((validation, checkBox) => checkBox.IsChecked = true);
        }

        private void CheckBoxChanged([NotNull] object sender, [NotNull] RoutedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            Commit();
            UpdateSelectedCount();
        }

        private void Commit()
        {
            if (IsChanging)
            {
                return;
            }

            var listBoxItem = ProfileListBox.SelectedItem as ListBoxItem;
            if (listBoxItem == null)
            {
                return;
            }

            var inactiveValidations = string.Empty;

            foreach (var child in ValidationList.Children)
            {
                var checkbox = child as CheckBox;
                if (checkbox == null)
                {
                    continue;
                }

                var item = checkbox.Tag as Validation;
                if (item == null)
                {
                    continue;
                }

                if (checkbox.IsChecked == true)
                {
                    continue;
                }

                inactiveValidations += "[" + item.Name + "]";
            }

            listBoxItem.Tag = inactiveValidations;
        }

        private void ControlLoaded([NotNull] object sender, [NotNull] RoutedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            Loaded -= ControlLoaded;

            LoadProfiles();

            EnableButtons();

            ExecuteCompleted c = delegate(string response, ExecuteResult result)
            {
                if (!DataService.HandleExecute(response, result))
                {
                    Loading.HideLoading(Editor);
                    return;
                }

                var root = response.ToXElement();
                if (root == null)
                {
                    Loading.HideLoading(Editor);
                    return;
                }

                ParseValidations(root);

                if (SettingsKey != "Items")
                {
                    GetCustomValidations();
                }

                RenderValidations();
                EnableButtons();
                UpdateProfile();
            };

            Site.DataService.ExecuteAsync("Validations.GetValidations", c, "Site");
        }

        private void DeleteProfile([NotNull] object sender, [NotNull] RoutedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            var selectedItem = ProfileListBox.SelectedItem as ListBoxItem;
            if (selectedItem == null)
            {
                return;
            }

            if (AppHost.MessageBox(string.Format("Are you sure you want to delete '{0}'?", selectedItem.Content as string), "Confirm", MessageBoxButton.OKCancel, MessageBoxImage.Question) != MessageBoxResult.OK)
            {
                return;
            }

            var index = ProfileListBox.SelectedIndex;

            ProfileListBox.Items.Remove(selectedItem);

            if (ProfileListBox.Items.Count == 0)
            {
                return;
            }

            if (index >= ProfileListBox.Items.Count)
            {
                index = ProfileListBox.Items.Count - 1;
            }

            ProfileListBox.SelectedItem = ProfileListBox.Items[index];
        }

        private void EnableButtons()
        {
            DeleteButton.IsEnabled = ProfileListBox.SelectedItem != null;
        }

        private void GetCustomValidations()
        {
            foreach (var customValidation in CustomValidationManager.CustomValidations)
            {
                var item = new Validation(customValidation.Category, customValidation.Title);

                validations.Add(item);
            }
        }

        private void LoadProfiles()
        {
            var keys = Storage.GetKeys("Validation\\" + SettingsKey + "\\Profiles");

            foreach (var key in keys)
            {
                var listBoxItem = new ListBoxItem
                {
                    Content = key,
                    Tag = AppHost.Settings.GetString("Validation\\" + SettingsKey + "\\Profiles", key, string.Empty)
                };

                ProfileListBox.Items.Add(listBoxItem);
            }

            if (ProfileListBox.Items.Count > 0)
            {
                ProfileListBox.SelectedIndex = 0;
            }
        }

        private void OkClick([NotNull] object sender, [NotNull] RoutedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            SaveProfiles();
            this.Close(true);
        }

        private void ParseValidations([NotNull] XElement root)
        {
            Debug.ArgumentNotNull(root, nameof(root));

            foreach (var element in root.Elements())
            {
                var name = element.GetAttributeValue("name");
                var category = element.GetAttributeValue("category");

                var item = new Validation(category, name);

                validations.Add(item);
            }
        }

        private void RenderValidations()
        {
            ValidationList.Children.Clear();

            string category = null;

            foreach (var item in validations.OrderBy(i0 => i0.Category).ThenBy(i1 => i1.Name))
            {
                if (category != item.Category)
                {
                    var textBlock = new TextBlock
                    {
                        Text = item.Category
                    };

                    ValidationList.Children.Add(textBlock);

                    category = item.Category;
                }

                var checkBox = new CheckBox
                {
                    Content = item.Name,
                    Margin = new Thickness(16, 2, 0, 2),
                    Tag = item
                };

                checkBox.Checked += CheckBoxChanged;
                checkBox.Unchecked += CheckBoxChanged;

                ValidationList.Children.Add(checkBox);
            }

            UpdateSelectedCount();
            Loading.HideLoading(Checkers);
        }

        private void SaveProfiles()
        {
            AppHost.Settings.Delete("Validation\\" + SettingsKey + "\\Profiles");

            foreach (var item in ProfileListBox.Items)
            {
                var selectedItem = item as ListBoxItem;
                if (selectedItem == null)
                {
                    continue;
                }

                var name = selectedItem.Content as string ?? string.Empty;
                var inactive = selectedItem.Tag as string ?? string.Empty;

                AppHost.Settings.SetString("Validation\\" + SettingsKey + "\\Profiles", name, inactive);
            }
        }

        private void SetCheckBoxes([NotNull] Action<Validation, CheckBox> action)
        {
            Debug.ArgumentNotNull(action, nameof(action));

            var list = ValidationList;
            if (list == null)
            {
                return;
            }

            foreach (var child in list.Children)
            {
                var checkBox = child as CheckBox;
                if (checkBox == null)
                {
                    continue;
                }

                var validation = checkBox.Tag as Validation;
                if (validation == null)
                {
                    continue;
                }

                action(validation, checkBox);
            }
        }

        private void UncheckAll([NotNull] object sender, [NotNull] RoutedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            SetCheckBoxes((validation, checkBox) => checkBox.IsChecked = false);
        }

        private void UpdateProfile()
        {
            IsChanging = true;

            var inactive = string.Empty;
            var selectedItem = ProfileListBox.SelectedItem as ListBoxItem;
            if (selectedItem != null)
            {
                inactive = selectedItem.Tag as string ?? string.Empty;
            }

            foreach (var item in validations.OrderBy(i0 => i0.Category).ThenBy(i1 => i1.Name))
            {
                var isChecked = !inactive.Contains("[" + item.Name + "]");

                var checkBox = ValidationList.Children.OfType<CheckBox>().FirstOrDefault(i => i.Tag == item);
                if (checkBox == null)
                {
                    continue;
                }

                checkBox.IsChecked = isChecked;
                checkBox.IsEnabled = selectedItem != null;
            }

            IsChanging = false;
        }

        private void UpdateProfile([NotNull] object sender, [NotNull] SelectionChangedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            var selectedItem = ProfileListBox.SelectedItem as ListBoxItem;
            SelectedValue = selectedItem != null ? selectedItem.Content as string ?? string.Empty : string.Empty;

            UpdateProfile();
            EnableButtons();
        }

        private void UpdateSelectedCount()
        {
            var on = 0;
            var count = 0;

            SetCheckBoxes(delegate(Validation item, CheckBox box)
            {
                if (box.IsChecked == true)
                {
                    on++;
                }

                count++;
            });

            SelectCount.Text = string.Format("{0} of {1} selected", on, count);
        }

        public class Validation
        {
            public Validation([NotNull] string category, [NotNull] string name)
            {
                Assert.ArgumentNotNull(category, nameof(category));
                Assert.ArgumentNotNull(name, nameof(name));

                Category = category;
                Name = name;
            }

            [NotNull]
            public string Category { get; }

            [NotNull]
            public string Name { get; }
        }
    }
}
