// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Extensibility.Composition;
using Sitecore.Rocks.Extensions.StringExtensions;
using Sitecore.Rocks.UI.LayoutDesigners.LayoutTreeViews.Models;

namespace Sitecore.Rocks.UI.LayoutDesigners.Designers.ScriptDesigners
{
    [Export(typeof(BaseDesigner), Priority = 3000, CreationPolicy = CreationPolicy.NonShared)]
    public class StylesheetDesigner : BaseDesigner
    {
        public StylesheetDesigner()
        {
            Name = "Stylesheets";
        }

        public override bool CanDesign(object parameter)
        {
            var context = parameter as DeviceContext;
            if (context == null)
            {
                return false;
            }

            if (string.IsNullOrEmpty(context.Device.DatabaseUri.Site.WebRootPath))
            {
                return false;
            }

            return GetFileNames(context.Device).Any();
        }

        public override FrameworkElement GetDesigner(object parameter)
        {
            var context = parameter as DeviceContext;
            if (context == null)
            {
                return null;
            }

            var fileNames = GetFileNames(context.Device).ToList();

            return new FilesDesignerControl(context.Device, fileNames);
        }

        [NotNull]
        private IEnumerable<string> GetFileNames([NotNull] DeviceModel device)
        {
            Debug.ArgumentNotNull(device, nameof(device));

            var webRootPath = device.DatabaseUri.Site.WebRootPath;
            var result = new List<string>();

            foreach (var rendering in device.Renderings)
            {
                var fileName = rendering.FilePath;
                if (string.IsNullOrEmpty(fileName))
                {
                    continue;
                }

                fileName = Path.ChangeExtension(fileName, ".css");
                if (fileName.StartsWith("/"))
                {
                    fileName = fileName.Mid(1);
                }

                fileName = Path.Combine(webRootPath, fileName);

                if (!File.Exists(fileName))
                {
                    continue;
                }

                if (result.Contains(fileName))
                {
                    continue;
                }

                result.Add(fileName);
                yield return fileName;
            }
        }
    }
}
