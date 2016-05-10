// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Windows;
using System.Windows.Controls;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Data.DataServices;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Extensions.StringExtensions;
using Sitecore.Rocks.UI.LayoutDesigners.Dialogs;
using Sitecore.Rocks.UI.LayoutDesigners.LayoutTreeViews.Models;

namespace Sitecore.Rocks.UI.LayoutDesigners.Designers.LayoutDesigners
{
    public partial class LayoutDesignerControl : IDesigner
    {
        public LayoutDesignerControl([NotNull] DeviceModel device)
        {
            Assert.ArgumentNotNull(device, nameof(device));

            InitializeComponent();

            Device = device;
            LayoutSelector.Text = Device.LayoutId;
        }

        [NotNull]
        public DeviceModel Device { get; }

        public void Activate()
        {
        }

        public void Close()
        {
        }

        private void Browse([NotNull] object sender, [NotNull] RoutedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            var d = new LayoutBrowserDialog();
            d.Initialize(Rocks.Resources.Browse, Device.DatabaseUri);
            if (AppHost.Shell.ShowDialog(d) != true)
            {
                return;
            }

            var selectedLayout = d.SelectedLayout;
            if (selectedLayout == null)
            {
                return;
            }

            LayoutSelector.Text = Device.LayoutId;
            Device.LayoutId = selectedLayout.LayoutId.ToString();
            Device.PageModel.RaiseModified();
        }

        private void ClearLayout([NotNull] object sender, [NotNull] RoutedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            Device.LayoutId = string.Empty;
            LayoutSelector.Text = string.Empty;
            Device.PageModel.RaiseModified();
        }

        private void HandleTextChanged([NotNull] object sender, [NotNull] TextChangedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            if (string.IsNullOrEmpty(LayoutSelector.Text))
            {
                Path.Text = "[No Layout]";
                return;
            }

            Path.Text = "[Updating Layout]";

            ExecuteCompleted completed = delegate(string response, ExecuteResult result)
            {
                if (!DataService.HandleExecute(response, result, true))
                {
                    Path.Text = "[Layout not found]";
                    return;
                }

                var element = response.ToXElement();
                if (element == null)
                {
                    return;
                }

                var itemHeader = ItemHeader.Parse(Device.DatabaseUri, element);

                Path.Text = itemHeader.Path;
            };

            Device.DatabaseUri.Site.DataService.ExecuteAsync("Items.GetItemHeader", completed, LayoutSelector.Text, Device.DatabaseUri.DatabaseName.Name);
        }
    }
}
