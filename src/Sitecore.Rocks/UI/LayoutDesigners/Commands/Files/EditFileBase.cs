// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.IO;
using System.Linq;
using System.Windows;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Commands;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Extensions.StringExtensions;
using Sitecore.Rocks.UI.LayoutDesigners.Items;

namespace Sitecore.Rocks.UI.LayoutDesigners.Commands.Files
{
    public abstract class EditFileBase : CommandBase
    {
        public override bool CanExecute(object parameter)
        {
            var context = parameter as LayoutDesignerContext;
            if (context == null)
            {
                return false;
            }

            var databaseUri = context.LayoutDesigner.DatabaseUri;
            if (string.IsNullOrEmpty(databaseUri.Site.WebRootPath))
            {
                return false;
            }

            if (context.SelectedItems.Count() != 1)
            {
                return false;
            }

            var item = context.SelectedItems.FirstOrDefault() as RenderingItem;
            if (item == null)
            {
                return false;
            }

            var filePath = GetFilePath(item, context);
            if (string.IsNullOrEmpty(filePath))
            {
                return false;
            }

            if (!AppHost.Files.FileExists(filePath))
            {
                return false;
            }

            return !string.IsNullOrEmpty(item.FilePath);
        }

        public override void Execute(object parameter)
        {
            var context = parameter as LayoutDesignerContext;
            if (context == null)
            {
                return;
            }

            var item = context.SelectedItems.FirstOrDefault() as RenderingItem;
            if (item == null)
            {
                return;
            }

            var filePath = GetFilePath(item, context);
            if (string.IsNullOrEmpty(filePath))
            {
                return;
            }

            if (!AppHost.Files.FileExists(filePath))
            {
                return;
            }

            AppHost.Files.OpenFile(filePath);
        }

        [NotNull]
        protected abstract string GetPath([NotNull] RenderingItem renderingItem, [NotNull] string filePath);

        [NotNull]
        private string GetFilePath([NotNull] RenderingItem renderingItem, [NotNull] LayoutDesignerContext context)
        {
            Debug.ArgumentNotNull(context, nameof(context));
            Debug.ArgumentNotNull(renderingItem, nameof(renderingItem));

            var filePath = renderingItem.FilePath;
            if (string.IsNullOrEmpty(filePath))
            {
                return string.Empty;
            }

            if (filePath.StartsWith(@"/"))
            {
                filePath = filePath.Mid(1);
            }

            try
            {
                filePath = Path.Combine(context.LayoutDesigner.DatabaseUri.Site.WebRootPath, filePath);
            }
            catch (ArgumentException ex)
            {
                AppHost.MessageBox(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            return GetPath(renderingItem, filePath);
        }
    }
}
