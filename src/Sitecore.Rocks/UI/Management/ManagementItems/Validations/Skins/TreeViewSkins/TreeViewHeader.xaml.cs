// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.Windows;
using System.Windows.Media.Imaging;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Extensions.BitmapImageExtensions;

namespace Sitecore.Rocks.UI.Management.ManagementItems.Validations.Skins.TreeViewSkins
{
    public partial class TreeViewHeader
    {
        private int count;

        public TreeViewHeader(SeverityLevel severity, [NotNull] string text)
        {
            Assert.ArgumentNotNull(text, nameof(text));

            InitializeComponent();

            HeaderText.Text = text;

            if (severity == SeverityLevel.None)
            {
                Icon.Visibility = Visibility.Collapsed;
                return;
            }

            Uri uri;
            switch (severity)
            {
                case SeverityLevel.Error:
                    uri = new Uri(@"pack://application:,,,/Sitecore.Rocks;component/Resources/16x16/bullet_square_red.png");
                    break;
                case SeverityLevel.Warning:
                    uri = new Uri(@"pack://application:,,,/Sitecore.Rocks;component/Resources/16x16/bullet_square_yellow.png");
                    break;
                case SeverityLevel.Suggestion:
                    uri = new Uri(@"pack://application:,,,/Sitecore.Rocks;component/Resources/16x16/bullet_square_blue.png");
                    break;
                case SeverityLevel.Hint:
                    uri = new Uri(@"pack://application:,,,/Sitecore.Rocks;component/Resources/16x16/bullet_square_blue.png");
                    break;
                default:
                    uri = new Uri(@"pack://application:,,,/Sitecore.Rocks;component/Resources/16x16/bullet_square_grey.png");
                    break;
            }

            var bitmapImage = new BitmapImage();
            bitmapImage.LoadIgnoreCache(uri);

            Icon.Source = bitmapImage;
        }

        public int Count
        {
            get { return count; }

            set
            {
                count = value;

                if (count == 0)
                {
                    CountText.Text = string.Empty;
                }
                else
                {
                    CountText.Text = string.Format("({0})", count);
                }
            }
        }

        [NotNull]
        public string Text
        {
            get { return HeaderText.Text; }
        }
    }
}
