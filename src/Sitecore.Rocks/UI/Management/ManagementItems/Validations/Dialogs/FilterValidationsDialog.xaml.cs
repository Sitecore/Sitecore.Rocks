// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Xml.Linq;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Data.DataServices;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Extensions.StringExtensions;
using Sitecore.Rocks.Extensions.WindowExtensions;
using Sitecore.Rocks.Extensions.XElementExtensions;
using Sitecore.Rocks.Sites;

namespace Sitecore.Rocks.UI.Management.ManagementItems.Validations.Dialogs
{
    public partial class FilterValidationsDialog
    {
        private readonly List<Validation> validations = new List<Validation>();

        public FilterValidationsDialog([NotNull] Site site)
        {
            Assert.ArgumentNotNull(site, nameof(site));

            InitializeComponent();
            this.InitializeDialog();

            Site = site;

            Loaded += ControlLoaded;
        }

        [NotNull]
        protected Site Site { get; }

        private void CancelClick([NotNull] object sender, [NotNull] RoutedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            this.Close(false);
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

            UpdateSelectedCount();
        }

        private void ControlLoaded([NotNull] object sender, [NotNull] RoutedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            Loaded -= ControlLoaded;

            ExecuteCompleted c = delegate(string response, ExecuteResult result)
            {
                if (!DataService.HandleExecute(response, result))
                {
                    Loading.HideLoading(ValidationList);
                    return;
                }

                var root = response.ToXElement();
                if (root == null)
                {
                    Loading.HideLoading(ValidationList);
                    return;
                }

                ParseValidations(root);
                GetCustomValidations();
                RenderValidations();
            };

            Site.DataService.ExecuteAsync("Validations.GetValidations", c, "Site");
        }

        private void GetCustomValidations()
        {
            foreach (var customValidation in CustomValidationManager.CustomValidations)
            {
                var item = new Validation(customValidation.Title, customValidation.Category);

                validations.Add(item);
            }
        }

        private void OkClick([NotNull] object sender, [NotNull] RoutedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

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

            AppHost.Settings.Set("Management\\Validation", "InactiveValidations", inactiveValidations);

            this.Close(true);
        }

        private void ParseValidations([NotNull] XElement root)
        {
            Debug.ArgumentNotNull(root, nameof(root));

            foreach (var element in root.Elements())
            {
                var name = element.GetAttributeValue("name");
                var category = element.GetAttributeValue("category");

                var item = new Validation(name, category);

                validations.Add(item);
            }
        }

        private void RenderValidations()
        {
            ValidationList.Children.Clear();

            var inactive = AppHost.Settings.Get("Management\\Validation", "InactiveValidations", string.Empty) as string ?? string.Empty;

            string category = null;

            foreach (var item in validations.OrderBy(i0 => i0.Category).ThenBy(i1 => i1.Name))
            {
                if (category != item.Category)
                {
                    var textBlock = new TextBlock
                    {
                        Text = item.Category,
                        FontWeight = FontWeights.Bold,
                        Margin = new Thickness(0, 8, 0, 2)
                    };

                    ValidationList.Children.Add(textBlock);

                    category = item.Category;
                }

                var isVisible = !inactive.Contains("[" + item.Name + "]");

                var checkBox = new CheckBox
                {
                    Content = item.Name,
                    Margin = new Thickness(0, 2, 0, 2),
                    Tag = item,
                    IsChecked = isVisible
                };

                checkBox.Checked += CheckBoxChanged;
                checkBox.Unchecked += CheckBoxChanged;

                ValidationList.Children.Add(checkBox);
            }

            UpdateSelectedCount();
            Loading.HideLoading(Checkers);
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
