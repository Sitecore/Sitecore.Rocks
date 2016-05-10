// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Linq;
using System.Windows;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Diagnostics;

namespace Sitecore.Rocks.UI.StartPage
{
    public partial class StartPageList
    {
        public StartPageList([NotNull] StartPageViewer startPage)
        {
            StartPage = startPage;
            Assert.ArgumentNotNull(startPage, nameof(startPage));

            InitializeComponent();

            Loaded += ControlLoaded;
        }

        [NotNull]
        public string ParentName { get; set; }

        [NotNull]
        public StartPageViewer StartPage { get; }

        private void ControlLoaded([NotNull] object sender, [NotNull] RoutedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            Loaded -= ControlLoaded;

            RenderList();
        }

        private void RenderList()
        {
            foreach (var descriptor in StartPageManager.Controls.OrderBy(t => t.Attribute.Priority))
            {
                if (descriptor.Attribute.ParentName != ParentName)
                {
                    continue;
                }

                var instance = descriptor.GetInstance(StartPage);
                if (instance == null)
                {
                    continue;
                }

                var control = instance.GetControl(instance.ParentName);
                if (control == null)
                {
                    continue;
                }

                List.Children.Add(control);
            }
        }
    }
}
