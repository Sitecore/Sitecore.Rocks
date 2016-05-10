// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Data.DataServices;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Extensions.StringExtensions;
using Sitecore.Rocks.Extensions.XElementExtensions;
using Sitecore.Rocks.UI.LayoutDesigners.Items;

namespace Sitecore.Rocks.UI.LayoutDesigners.Chunks
{
    public class FileBasedRenderingChunk : RenderingChunkBase
    {
        public FileBasedRenderingChunk([NotNull] string fileName)
        {
            Assert.ArgumentNotNull(fileName, nameof(fileName));

            FileName = fileName;

            Name = Path.GetFileNameWithoutExtension(fileName) ?? string.Empty;
            if (Name.EndsWith(".chunk", StringComparison.InvariantCultureIgnoreCase))
            {
                Name = Name.Left(Name.Length - 6);
            }

            Group = "Files";
        }

        [NotNull]
        public string FileName { get; }

        public override void GetRenderings(IRenderingContainer renderingContainer, Action<IEnumerable<RenderingItem>> completed)
        {
            Assert.ArgumentNotNull(renderingContainer, nameof(renderingContainer));
            Assert.ArgumentNotNull(completed, nameof(completed));

            var text = AppHost.Files.ReadAllText(FileName);

            ExecuteCompleted getLayoutCompleted = delegate(string response, ExecuteResult executeResult)
            {
                var root = response.ToXElement();
                if (root == null)
                {
                    completed(Enumerable.Empty<RenderingItem>());
                    return;
                }

                var result = Enumerable.Empty<RenderingItem>();

                var layoutElement = root.Element("layout");
                if (layoutElement != null)
                {
                    var deviceElement = layoutElement.Elements().FirstOrDefault(d => d.HasElements);
                    if (deviceElement != null)
                    {
                        result = deviceElement.Elements().Select(element => new RenderingItem(renderingContainer, renderingContainer.DatabaseUri, element)).ToList();
                    }
                }

                completed(result);
            };

            ExecuteCompleted getDevicesCompleted = delegate(string response, ExecuteResult result)
            {
                if (!DataService.HandleExecute(response, result))
                {
                    return;
                }

                var root = response.ToXElement();
                if (root == null)
                {
                    return;
                }

                var deviceElement = root.Elements().FirstOrDefault();
                if (deviceElement == null)
                {
                    return;
                }

                var deviceId = deviceElement.GetAttributeValue("id");

                text = text.Replace("<d>", "<d id=\"" + deviceId + "\">");

                AppHost.Server.GetLayout(text, renderingContainer.DatabaseUri, getLayoutCompleted);
            };

            AppHost.Server.Layouts.GetDevices(renderingContainer.DatabaseUri, getDevicesCompleted);
        }
    }
}
