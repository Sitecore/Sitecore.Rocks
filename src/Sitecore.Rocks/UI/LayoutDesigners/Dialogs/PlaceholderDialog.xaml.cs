// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Xml;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Data.DataServices;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Extensions.StringExtensions;
using Sitecore.Rocks.Extensions.WindowExtensions;
using Sitecore.Rocks.UI.LayoutDesigners.Items;

namespace Sitecore.Rocks.UI.LayoutDesigners.Dialogs
{
    public partial class PlaceholderDialog
    {
        private readonly List<string> placeHolderNames = new List<string>();

        public PlaceholderDialog()
        {
            InitializeComponent();
            this.InitializeDialog();

            Loaded += ControlLoaded;
        }

        [NotNull]
        public string SelectedValue { get; set; }

        [NotNull]
        protected IRenderingContainer Container { get; set; }

        [NotNull]
        protected DatabaseUri DatabaseUri { get; set; }

        public void Initialize([NotNull] string title, [NotNull] string selectedValue, [NotNull] IRenderingContainer container)
        {
            Assert.ArgumentNotNull(title, nameof(title));
            Assert.ArgumentNotNull(selectedValue, nameof(selectedValue));
            Assert.ArgumentNotNull(container, nameof(container));

            Title = title;
            SelectedValue = selectedValue;
            Container = container;

            DatabaseUri = Container.DatabaseUri;
        }

        private void ControlLoaded([NotNull] object sender, [NotNull] RoutedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            Loaded -= ControlLoaded;

            EnableButtons();
            LoadPlaceHolders();
        }

        private void EnableButtons()
        {
            OK.IsEnabled = Placeholders.SelectedItem != null;
        }

        private void FilterTextChanged([NotNull] object sender, [NotNull] EventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            RenderPlaceHolders();
        }

        [NotNull]
        private string GetRenderings([NotNull] IEnumerable<RenderingItem> renderings)
        {
            Debug.ArgumentNotNull(renderings, nameof(renderings));

            var writer = new StringWriter();
            var output = new XmlTextWriter(writer)
            {
                Formatting = Formatting.Indented
            };

            output.WriteStartElement("renderings");

            foreach (var rendering in renderings)
            {
                output.WriteStartElement("rendering");
                output.WriteAttributeString("id", rendering.ItemId);

                foreach (var parameter in rendering.DynamicProperties)
                {
                    var v = string.Empty;

                    var value = parameter.Value;
                    if (value != null)
                    {
                        v = value.ToString();
                    }

                    output.WriteStartElement("parameter");
                    output.WriteAttributeString("name", parameter.Name);
                    output.WriteValue(v);
                    output.WriteEndElement();
                }

                output.WriteEndElement();
            }

            output.WriteEndElement();

            return writer.ToString();
        }

        private void HandleDoubleClick([NotNull] object sender, [NotNull] MouseButtonEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            OkClick(sender, e);
        }

        private void LoadPlaceHolders()
        {
            placeHolderNames.Clear();

            Loading.ShowLoading(Placeholders);
            LayoutSelectorFilter.Visibility = Visibility.Collapsed;

            var renderings = GetRenderings(Container.Renderings);

            ExecuteCompleted callback = delegate(string response, ExecuteResult executeResult)
            {
                Loading.HideLoading(Placeholders);
                LayoutSelectorFilter.Visibility = Visibility.Visible;

                if (!DataService.HandleExecute(response, executeResult))
                {
                    return;
                }

                ParsePlaceHolders(response);
                RenderPlaceHolders();
            };

            DatabaseUri.Site.DataService.ExecuteAsync("GetPlaceholders", callback, DatabaseUri.DatabaseName.Name, Container.Layout, renderings);
        }

        private void OkClick([NotNull] object sender, [NotNull] RoutedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            var selectedItem = Placeholders.SelectedItem as ListBoxItem;
            if (selectedItem != null)
            {
                SelectedValue = selectedItem.Tag as string ?? string.Empty;
            }

            this.Close(true);

            e.Handled = true;
        }

        private void ParsePlaceHolders([NotNull] string response)
        {
            Debug.ArgumentNotNull(response, nameof(response));

            var reader = new StringReader(response);

            var line = reader.ReadLine();
            while (line != null)
            {
                if (!string.IsNullOrEmpty(line))
                {
                    placeHolderNames.Add(line);
                }

                line = reader.ReadLine();
            }
        }

        private void PlaceholderSelectionChanged([NotNull] object sender, [NotNull] SelectionChangedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            EnableButtons();
        }

        private void RenderPlaceHolders()
        {
            var selectedValue = SelectedValue;

            var selectedItem = Placeholders.SelectedItem as ListBoxItem;
            if (selectedItem != null)
            {
                selectedValue = selectedItem.Tag as string ?? string.Empty;
            }

            Placeholders.Items.Clear();

            var filterText = LayoutSelectorFilter.Text;

            foreach (var placeHolderName in placeHolderNames)
            {
                if (!placeHolderName.IsFilterMatch(filterText))
                {
                    continue;
                }

                var listBoxItem = new ListBoxItem
                {
                    Content = placeHolderName,
                    Tag = placeHolderName,
                    IsSelected = placeHolderName == selectedValue
                };

                Placeholders.Items.Add(listBoxItem);
            }
        }
    }
}
