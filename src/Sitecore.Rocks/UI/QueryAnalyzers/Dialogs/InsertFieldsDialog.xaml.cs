// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Xml.Linq;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Extensions.DataServiceExtensions;
using Sitecore.Rocks.Extensions.WindowExtensions;
using Sitecore.Rocks.Extensions.XElementExtensions;

namespace Sitecore.Rocks.UI.QueryAnalyzers.Dialogs
{
    public partial class InsertFieldsDialog
    {
        public InsertFieldsDialog([NotNull] DatabaseUri databaseUri)
        {
            Assert.ArgumentNotNull(databaseUri, nameof(databaseUri));

            DatabaseUri = databaseUri;

            InitializeComponent();
            this.InitializeDialog();

            TemplateSelector.DatabaseUri = DatabaseUri;
        }

        [NotNull]
        public IEnumerable<string> SelectedFieldNames
        {
            get
            {
                foreach (var item in CheckBoxList.Items)
                {
                    var listBoxItem = item as ListBoxItem;
                    if (listBoxItem == null)
                    {
                        continue;
                    }

                    if (!listBoxItem.IsSelected)
                    {
                        continue;
                    }

                    var name = listBoxItem.Tag as string;
                    if (name == null)
                    {
                        continue;
                    }

                    yield return name;
                }
            }
        }

        [NotNull]
        protected DatabaseUri DatabaseUri { get; set; }

        private void DisplayFields([NotNull] XElement templateElement)
        {
            Debug.ArgumentNotNull(templateElement, nameof(templateElement));

            foreach (var sectionElement in templateElement.Elements())
            {
                foreach (var fieldElement in sectionElement.Elements())
                {
                    var name = fieldElement.GetAttributeValue("name");

                    var value = name;
                    if (value.Contains(@" "))
                    {
                        value = '#' + value + '#';
                    }

                    value = '@' + value;

                    CheckBoxList.Items.Add(new ListBoxItem
                    {
                        Content = name,
                        Tag = value
                    });
                }
            }

            var foreground = new SolidColorBrush(Color.FromRgb(0x66, 0x66, 0x66));

            var checkBox = new ListBoxItem
            {
                Content = @"Item ID",
                Tag = @"@@id",
                Foreground = foreground
            };
            CheckBoxList.Items.Add(checkBox);

            checkBox = new ListBoxItem
            {
                Content = @"Item Name",
                Tag = @"@@name",
                Foreground = foreground
            };
            CheckBoxList.Items.Add(checkBox);

            checkBox = new ListBoxItem
            {
                Content = @"Item Key",
                Tag = @"@@key",
                Foreground = foreground
            };
            CheckBoxList.Items.Add(checkBox);

            checkBox = new ListBoxItem
            {
                Content = @"Template ID",
                Tag = @"@@templateid",
                Foreground = foreground
            };
            CheckBoxList.Items.Add(checkBox);

            checkBox = new ListBoxItem
            {
                Content = @"Template Name",
                Tag = @"@@templatename",
                Foreground = foreground
            };
            CheckBoxList.Items.Add(checkBox);

            checkBox = new ListBoxItem
            {
                Content = @"Template Key",
                Tag = @"@@templatekey",
                Foreground = foreground
            };
            CheckBoxList.Items.Add(checkBox);
        }

        private void OkClick([NotNull] object sender, [NotNull] RoutedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            this.Close(true);
        }

        private void TemplateSelectionChanged([NotNull] object sender, [NotNull] SelectionChangedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            CheckBoxList.Items.Clear();

            var selectedTemplate = TemplateSelector.SelectedTemplate;
            if (selectedTemplate == null)
            {
                return;
            }

            Loading.ShowLoading(CheckBoxList);

            GetValueCompleted<XDocument> completed = delegate(XDocument value)
            {
                Loading.HideLoading(CheckBoxList);

                var root = value.Root;
                if (root == null)
                {
                    return;
                }

                DisplayFields(root);
            };

            DatabaseUri.Site.DataService.GetTemplateXml(selectedTemplate.TemplateUri, true, completed);
        }
    }
}
