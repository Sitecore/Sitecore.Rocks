// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Linq;
using System.Windows;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Diagnostics;

namespace Sitecore.Rocks.Controls
{
    public partial class Loading
    {
        public Loading()
        {
            InitializeComponent();
        }

        public void HideLoading([NotNull] params UIElement[] elements)
        {
            Assert.ArgumentNotNull(elements, nameof(elements));

            var first = elements.First();

            Visibility = Visibility.Collapsed;
            first.Visibility = Visibility.Visible;

            foreach (var element in elements.Skip(1))
            {
                element.Visibility = Visibility.Collapsed;
            }
        }

        public void ShowLoading([NotNull] params UIElement[] elements)
        {
            Assert.ArgumentNotNull(elements, nameof(elements));

            var first = elements.First();

            Visibility = Visibility.Visible;
            first.Visibility = Visibility.Collapsed;

            foreach (var element in elements.Skip(1))
            {
                element.Visibility = Visibility.Collapsed;
            }
        }

        public void Swap([NotNull] params UIElement[] elements)
        {
            Assert.ArgumentNotNull(elements, nameof(elements));

            var first = elements.First();

            if (Visibility == Visibility.Collapsed)
            {
                Visibility = Visibility.Visible;
                first.Visibility = Visibility.Collapsed;
            }
            else
            {
                first.Visibility = Visibility.Visible;
                Visibility = Visibility.Collapsed;
            }

            foreach (var element in elements.Skip(1))
            {
                element.Visibility = Visibility.Collapsed;
            }
        }
    }
}
