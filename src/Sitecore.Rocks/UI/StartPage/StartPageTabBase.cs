// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Windows;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Diagnostics;

namespace Sitecore.Rocks.UI.StartPage
{
    public abstract class StartPageTabBase : IStartPageControl
    {
        protected StartPageTabBase([NotNull] StartPageViewer startPage, [NotNull] string parentName)
        {
            Debug.ArgumentNotNull(startPage, nameof(startPage));
            Debug.ArgumentNotNull(parentName, nameof(parentName));

            StartPage = startPage;
            ParentName = parentName;
        }

        public string ParentName { get; }

        [NotNull]
        public StartPageViewer StartPage { get; set; }

        public TabStyle TabStyle { get; set; }

        public string Text { get; set; }

        public virtual FrameworkElement GetControl(string parentName)
        {
            Assert.ArgumentNotNull(parentName, nameof(parentName));

            var result = new StartPageTabControl(StartPage)
            {
                ParentName = parentName,
                TabStyle = TabStyle
            };

            return result;
        }
    }
}
