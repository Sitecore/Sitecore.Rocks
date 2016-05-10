// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.ComponentModel;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Commands;

namespace Sitecore.Rocks.UI.LogViewer.Commands
{
    public abstract class CategoryCommand : CommandBase
    {
        [Localizable(false)]
        public string CategoryCode { get; set; }

        public override bool CanExecute(object parameter)
        {
            var context = parameter as LogViewerContext;
            if (context == null)
            {
                return false;
            }

            IsChecked = context.LogViewer.Categories.IndexOf(CategoryCode, StringComparison.InvariantCultureIgnoreCase) >= 0;

            return true;
        }

        public override void Execute([CanBeNull] object parameter)
        {
            var context = parameter as LogViewerContext;
            if (context == null)
            {
                return;
            }

            if (context.LogViewer.Categories.IndexOf(CategoryCode, StringComparison.InvariantCultureIgnoreCase) >= 0)
            {
                context.LogViewer.Categories = context.LogViewer.Categories.Replace(CategoryCode, string.Empty);
            }
            else
            {
                context.LogViewer.Categories += CategoryCode;
            }

            context.LogViewer.Stop();

            AppHost.Settings.Set("Log Viewer", "Categories", context.LogViewer.Categories);

            context.LogViewer.IsRunning = true;
            context.LogViewer.LoadLog();
        }
    }
}
