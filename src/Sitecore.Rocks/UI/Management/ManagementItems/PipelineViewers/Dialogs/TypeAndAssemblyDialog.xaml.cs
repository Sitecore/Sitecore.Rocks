// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Windows;
using System.Windows.Controls;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Data.DataServices;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Extensions.WindowExtensions;
using Sitecore.Rocks.Sites;

namespace Sitecore.Rocks.UI.Management.ManagementItems.PipelineViewers.Dialogs
{
    public partial class TypeAndAssemblyDialog
    {
        public TypeAndAssemblyDialog([NotNull] Site site)
        {
            Assert.ArgumentNotNull(site, nameof(site));

            InitializeComponent();
            this.InitializeDialog();

            Site = site;
            EnableButtons();
        }

        [NotNull]
        protected Site Site { get; set; }

        private void CancelClick([NotNull] object sender, [NotNull] RoutedEventArgs e)
        {
            Assert.ArgumentNotNull(sender, nameof(sender));
            Assert.ArgumentNotNull(e, nameof(e));

            this.Close(false);
        }

        private void EnableButtons()
        {
            OK.IsEnabled = !string.IsNullOrEmpty(ClassName.Text);
        }

        private void OkClick([NotNull] object sender, [NotNull] RoutedEventArgs e)
        {
            Assert.ArgumentNotNull(sender, nameof(sender));
            Assert.ArgumentNotNull(e, nameof(e));

            this.Close(true);
        }

        private void TestClick([NotNull] object sender, [NotNull] RoutedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            var type = ClassName.Text;
            if (!string.IsNullOrEmpty(AssemblyName.Text))
            {
                type += "," + AssemblyName.Text;
            }

            ExecuteCompleted completed = delegate(string response, ExecuteResult result)
            {
                if (!DataService.HandleExecute(response, result))
                {
                    return;
                }

                if (response == "ok")
                {
                    AppHost.MessageBox("Yes, it works!", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    AppHost.MessageBox("Nope, doesn't work!", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            };

            Site.DataService.ExecuteAsync("Sites.TestTypeAndAssembly", completed, type);
        }

        private void TextChanged([NotNull] object sender, [NotNull] TextChangedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            EnableButtons();
        }
    }
}
