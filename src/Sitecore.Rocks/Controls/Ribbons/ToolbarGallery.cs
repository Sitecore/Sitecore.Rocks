// © 2015 Sitecore Corporation A/S. All rights reserved.

using System;
using System.Windows;
using Fluent;
using Sitecore.VisualStudio.UI.Toolbars;

namespace Sitecore.Rocks.Controls.Toolbars
{
    public class ToolbarGallery : InRibbonGallery, IToolbarGallery
    {
        public IPopupGallery NewPopupGallery()
        {
            return new InRibbonGallery();
        }

        public void SetCategory(object dependencyObject, string view)
        {
            ((DependencyObject)dependencyObject).SetValue(PopupGallery.CategoryProperty, "View");
        }

        event EventHandler<object> IToolbarGallery.ItemClick
        {
            add { base.ItemClick += delegate(object sender, ObjectItemRoutedEventArgs args) { value(sender, args.Item); }; }
            remove { }
        }
    }
}
