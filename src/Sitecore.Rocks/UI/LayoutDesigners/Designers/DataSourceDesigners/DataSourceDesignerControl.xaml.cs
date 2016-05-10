// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.Windows;
using System.Windows.Controls;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Data.DataServices;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Extensions.GuidExtensions;
using Sitecore.Rocks.Extensions.StringExtensions;
using Sitecore.Rocks.UI.Dialogs.SelectItemDialogs;
using Sitecore.Rocks.UI.LayoutDesigners.Items;

namespace Sitecore.Rocks.UI.LayoutDesigners.Designers.DataSourceDesigners
{
    public partial class DataSourceDesignerControl : IDesigner
    {
        private bool activated;

        public DataSourceDesignerControl([NotNull] RenderingItem rendering)
        {
            Assert.ArgumentNotNull(rendering, nameof(rendering));

            InitializeComponent();

            DataContext = rendering;
            Rendering = rendering;
        }

        [NotNull]
        protected RenderingItem Rendering { get; }

        public void Activate()
        {
            if (activated)
            {
                return;
            }

            activated = true;
            EnableButtons();
        }

        public void Close()
        {
        }

        private void Browse([NotNull] object sender, [NotNull] RoutedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            var d = new SelectItemDialog();

            if (DataSourceTextBox.Text.IsGuid())
            {
                var itemUri = new ItemUri(Rendering.ItemUri.DatabaseUri, new ItemId(new Guid(DataSourceTextBox.Text)));
                d.Initialize("Data Source", itemUri);
            }
            else if (Rendering.RenderingContainer != null)
            {
                d.Initialize("Data Source", Rendering.RenderingContainer.DatabaseUri, string.Empty);
            }
            else
            {
                d.Initialize("Data Source", Rendering.ItemUri.DatabaseUri, string.Empty);
            }

            if (AppHost.Shell.ShowDialog(d) != true)
            {
                return;
            }

            Rendering.DataSource = d.SelectedItemUri.ItemId.ToString();

            EnableButtons();
        }

        private void Create([NotNull] object sender, [NotNull] RoutedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            /*
      string name;
      this.Rendering.ParameterDictionary.Parameters.TryGetValue("Id", out name);
      if (string.IsNullOrEmpty(name))
      {
        name = this.Rendering.Name;
      }

      var renderingContainer = this.Rendering.RenderingContainer;
      if (renderingContainer == null)
      {
        return;
      }

      var databaseUri = renderingContainer.DatabaseUri;
      var location = this.Rendering.DataSourceLocation;
      var template = this.Rendering.DataSourceTemplate;

      Site.RequestCompleted completed = delegate(string response)
      {
        var parts = response.Split(',');
        if (parts.Length != 2)
        {
          AppHost.MessageBox("The item was not found.", VisualStudio.Resources.Information, MessageBoxButton.OK, MessageBoxImage.Information);
          return;
        }

        var itemId = parts[0];
        var parentItemId = parts[1];

        Guid guid;
        if (!Guid.TryParse(itemId, out guid))
        {
          AppHost.MessageBox("The item was not found.", VisualStudio.Resources.Information, MessageBoxButton.OK, MessageBoxImage.Information);
          return;
        }

        var newItemUri = new ItemUri(databaseUri, new ItemId(guid));

        this.Rendering.DataSource = itemId;

        var itemVersionUri = new ItemVersionUri(newItemUri, LanguageManager.CurrentLanguage, Data.Version.Latest);

        Notifications.RaiseItemAdded(this, itemVersionUri, new ItemUri(databaseUri, new ItemId(new Guid(parentItemId))));

        this.EnableButtons();

        AppHost.OpenContentEditor(itemVersionUri);
      };

      databaseUri.Site.Execute("LayoutBuilders.CreateDataSource", completed, databaseUri.DatabaseName.ToString(), databaseUri.ItemId.ToString(), name, location, template);
      */
        }

        private void Edit([NotNull] object sender, [NotNull] RoutedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            var text = DataSourceTextBox.Text ?? string.Empty;

            var itemUri = new ItemUri(Rendering.ItemUri.DatabaseUri, new ItemId(new Guid(text)));
            var itemVersionUri = new ItemVersionUri(itemUri, LanguageManager.CurrentLanguage, Data.Version.Latest);

            AppHost.OpenContentEditor(itemVersionUri);
        }

        private void EnableButtons()
        {
            var text = DataSourceTextBox.Text ?? string.Empty;

            EditDataSource.IsEnabled = text.IsGuid();
            CreateDataSource.IsEnabled = string.IsNullOrEmpty(text) && !string.IsNullOrEmpty(Rendering.DataSourceTemplate);
        }

        private void HandleTextChanged([NotNull] object sender, [NotNull] TextChangedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            if (string.IsNullOrEmpty(DataSourceTextBox.Text))
            {
                Path.Text = "[No Data Source]";
                return;
            }

            Path.Text = "[Updating Data Source]";

            ExecuteCompleted completed = delegate(string response, ExecuteResult result)
            {
                if (!DataService.HandleExecute(response, result, true))
                {
                    Path.Text = "[Item not found]";
                    return;
                }

                var element = response.ToXElement();
                if (element == null)
                {
                    return;
                }

                var itemHeader = ItemHeader.Parse(Rendering.ItemUri.DatabaseUri, element);

                Path.Text = itemHeader.Path;
            };

            Rendering.ItemUri.Site.DataService.ExecuteAsync("Items.GetItemHeader", completed, DataSourceTextBox.Text, Rendering.ItemUri.DatabaseName.Name);

            EnableButtons();
        }
    }
}
