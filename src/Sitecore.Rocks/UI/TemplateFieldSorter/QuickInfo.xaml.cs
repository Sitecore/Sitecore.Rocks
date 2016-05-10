// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Linq;
using System.Windows;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Data.DataServices;
using Sitecore.Rocks.Diagnostics;

namespace Sitecore.Rocks.UI.TemplateFieldSorter
{
    public partial class QuickInfo
    {
        public QuickInfo()
        {
            InitializeComponent();
        }

        public TemplateFieldSorter.TemplateFields TemplateFields { get; private set; }

        public TemplateFieldSorter TemplateFieldSorter { get; private set; }

        public void Initialize([NotNull] TemplateFieldSorter templateFieldSorter, [NotNull] TemplateFieldSorter.TemplateFields template, bool isCloseButtonVisible)
        {
            Assert.ArgumentNotNull(templateFieldSorter, nameof(templateFieldSorter));
            Assert.ArgumentNotNull(template, nameof(template));

            TemplateFields = template;

            ItemName.Text = template.Name;
            Icon.Source = template.Icon.GetSource();
            TemplateFieldSorter = templateFieldSorter;
            CloseButton.Visibility = isCloseButtonVisible ? Visibility.Visible : Visibility.Collapsed;

            // this.ItemId.Text = template.TemplateUri.ItemId.ToString();
            template.TemplateUri.Site.DataService.ExecuteAsync("Links.GetTemplateInstances", RenderInstances, template.TemplateUri.DatabaseName.ToString(), template.TemplateUri.ItemId.ToString());
        }

        private void CloseClick([NotNull] object sender, [NotNull] RoutedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            if (TemplateFields.Fields.Any(field => field.Modified))
            {
                switch (AppHost.MessageBox("Some of the fields have been modified.\n\nDo you want to save changes?", "Confirmation", MessageBoxButton.YesNoCancel, MessageBoxImage.Question))
                {
                    case MessageBoxResult.Yes:
                        TemplateFieldSorter.Save();
                        break;
                    case MessageBoxResult.Cancel:
                        return;
                }
            }

            TemplateFieldSorter.RemoveTemplate(TemplateFields);
        }

        private void RenderInstances([NotNull] string response, [NotNull] ExecuteResult executeresult)
        {
            Debug.ArgumentNotNull(response, nameof(response));
            Debug.ArgumentNotNull(executeresult, nameof(executeresult));

            DataService.HandleExecute(response, executeresult);

            int count;

            int.TryParse(response, out count);

            Loading.Text = null;
            Instances.Inlines.Add(count == 1 ? Rocks.Resources.QuickInfo_RenderInstances__1_instance : string.Format(Rocks.Resources.QuickInfo_RenderInstances__0__instances, count));
        }
    }
}
