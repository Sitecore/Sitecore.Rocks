// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.Windows;
using System.Windows.Controls;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Sites;

namespace Sitecore.Rocks.ContentTrees
{
    public static class ToolTipBuilder
    {
        [NotNull]
        public static object BuildToolTip([NotNull] Site site)
        {
            Assert.ArgumentNotNull(site, nameof(site));

            var stackPanel = new StackPanel();

            stackPanel.Children.Add(new TextBlock
            {
                Text = site.Name,
                FontWeight = FontWeights.Bold,
                Margin = new Thickness(8, 4, 4, 0)
            });

            stackPanel.Children.Add(new TextBlock
            {
                Text = @"Host: " + site.GetHost(),
                Margin = new Thickness(24, 4, 0, 0)
            });

            if (!string.IsNullOrEmpty(site.DataService.SitecoreVersionString))
            {
                stackPanel.Children.Add(new TextBlock
                {
                    Text = @"Sitecore: " + site.DataService.SitecoreVersionString,
                    Margin = new Thickness(24, 4, 0, 0)
                });
            }

            stackPanel.Children.Add(new TextBlock
            {
                Text = @"User: " + site.Credentials.UserName,
                Margin = new Thickness(24, 4, 0, 0)
            });

            stackPanel.Children.Add(new TextBlock
            {
                Text = string.Format(@"Data Service: {0} - {1}", site.DataServiceName, string.IsNullOrEmpty(site.DataService.WebServiceVersion) ? "N/A" : site.DataService.WebServiceVersion),
                Margin = new Thickness(24, 4, 0, 0)
            });

            if (!string.IsNullOrEmpty(site.WebRootPath))
            {
                stackPanel.Children.Add(new TextBlock
                {
                    Text = @"Path: " + site.WebRootPath,
                    Margin = new Thickness(24, 4, 0, 0)
                });
            }

            return stackPanel;
        }

        [NotNull]
        public static object BuildToolTip([NotNull] IItem item)
        {
            Assert.ArgumentNotNull(item, nameof(item));

            var stackPanel = new StackPanel();

            stackPanel.Children.Add(new TextBlock
            {
                Text = item.Name,
                FontWeight = FontWeights.Bold,
                Margin = new Thickness(8, 4, 4, 0)
            });

            stackPanel.Children.Add(new TextBlock
            {
                Text = @"ID: " + item.ItemUri.ItemId,
                Margin = new Thickness(24, 4, 0, 0)
            });

            stackPanel.Children.Add(new TextBlock
            {
                Text = @"Database: " + item.ItemUri.DatabaseName,
                Margin = new Thickness(24, 4, 0, 0)
            });

            stackPanel.Children.Add(new TextBlock
            {
                Text = @"Site: " + item.ItemUri.Site.Name,
                Margin = new Thickness(24, 4, 0, 0)
            });

            return stackPanel;
        }

        [NotNull]
        public static object BuildToolTip([NotNull] ItemHeader itemHeader)
        {
            Assert.ArgumentNotNull(itemHeader, nameof(itemHeader));

            var stackPanel = new StackPanel();

            stackPanel.Children.Add(new TextBlock
            {
                Text = itemHeader.Path,
                FontWeight = FontWeights.Bold,
                Margin = new Thickness(8, 4, 4, 0)
            });

            stackPanel.Children.Add(new TextBlock
            {
                Text = @"ID: " + itemHeader.ItemUri.ItemId,
                Margin = new Thickness(24, 4, 0, 0)
            });

            stackPanel.Children.Add(new TextBlock
            {
                Text = @"Template: " + itemHeader.TemplateName,
                Margin = new Thickness(24, 4, 0, 0)
            });

            stackPanel.Children.Add(new TextBlock
            {
                Text = @"Template ID: " + itemHeader.TemplateId,
                Margin = new Thickness(24, 4, 0, 0)
            });

            stackPanel.Children.Add(new TextBlock
            {
                Text = @"Sort Order: " + itemHeader.SortOrder,
                Margin = new Thickness(24, 4, 0, 0)
            });

            if (itemHeader.Updated != DateTime.MinValue)
            {
                stackPanel.Children.Add(new TextBlock
                {
                    Text = string.Format(@"Updated: {0} by {1}", itemHeader.Updated, itemHeader.UpdatedBy),
                    Margin = new Thickness(24, 4, 0, 0)
                });
            }

            foreach (var gutterDescriptor in itemHeader.Gutters)
            {
                stackPanel.Children.Add(new TextBlock
                {
                    Text = gutterDescriptor.ToolTip,
                    Margin = new Thickness(24, 4, 0, 0)
                });
            }

            return stackPanel;
        }
    }
}
