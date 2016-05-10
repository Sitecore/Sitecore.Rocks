// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Windows;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Extensions.WindowExtensions;

namespace Sitecore.Rocks.UI.LogViewer
{
    public partial class LogDetailsWindow
    {
        public LogDetailsWindow()
        {
            InitializeComponent();
            this.InitializeDialog();
        }

        protected LogItem LogItem { get; set; }

        public void Initialize([NotNull] LogItem logItem)
        {
            Assert.ArgumentNotNull(logItem, nameof(logItem));

            LogItem = logItem;

            LogTitle.Text = logItem.Title;
            Category.Text = logItem.Category;
            DateTime.Text = logItem.PublishDate.ToString();

            Frame.NavigateToString(logItem.Description);
        }

        private void OkClick([NotNull] object sender, [NotNull] RoutedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            this.Close(true);
        }
    }
}
