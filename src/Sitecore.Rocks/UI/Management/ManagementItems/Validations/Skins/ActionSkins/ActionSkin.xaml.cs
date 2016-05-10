// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Extensions.FrameworkElementExtensions;
using Sitecore.Rocks.Extensions.StringExtensions;

namespace Sitecore.Rocks.UI.Management.ManagementItems.Validations.Skins.ActionSkins
{
    [ValidationViewerSkin("Action Center", 1000)]
    public partial class ActionSkin : IValidationViewerSkin
    {
        private readonly List<ValidationPresenter> entries = new List<ValidationPresenter>();

        public ActionSkin()
        {
            InitializeComponent();
        }

        public IValidationViewer ValidationViewer { get; set; }

        public Control GetControl()
        {
            return this;
        }

        public void Hide(ValidationDescriptor item)
        {
            Assert.ArgumentNotNull(item, nameof(item));

            ValidationViewer.Hide(item);

            var control = entries.FirstOrDefault(e => e.Item == item);
            if (control == null)
            {
                return;
            }

            entries.Remove(control);

            var header = control.GetAncestor<CategoryHeader>();
            Assert.IsNotNull(header, "header");

            header.List.Children.Remove(control);

            if (header.List.Children.Count == 0)
            {
                header.Visibility = Visibility.Collapsed;
            }
            else
            {
                var text = header.HeaderField.Text;

                var n = text.LastIndexOf(" (", StringComparison.Ordinal);
                if (n >= 0)
                {
                    text = text.Left(n);
                }

                text += " (" + header.List.Children.Count + ")";

                header.HeaderField.Text = text;
            }
        }

        public void RenderValidations(IEnumerable<ValidationDescriptor> items)
        {
            Assert.ArgumentNotNull(items, nameof(items));

            entries.Clear();
            StackPanel list = null;
            CategoryHeader categoryHeader = null;
            string category = null;

            foreach (var item in items)
            {
                if (category != item.Category)
                {
                    if (categoryHeader != null)
                    {
                        categoryHeader.Header += " (" + list.Children.Count + ")";
                    }

                    categoryHeader = new CategoryHeader(item.Category, item.Category);

                    Items.Children.Add(categoryHeader);

                    category = item.Category;
                    list = categoryHeader.List;

                    categoryHeader.IsExpanded = (AppHost.Settings.Get("Management\\Validation\\Categories", category, "1") as string ?? "1") == "1";
                }

                var entry = new ValidationPresenter(this, item);
                entries.Add(entry);
                list.Children.Add(entry);
            }

            if (categoryHeader != null)
            {
                categoryHeader.Header += " (" + list.Children.Count + ")";
            }
        }
    }
}
