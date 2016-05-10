// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Collections.Generic;
using System.IO;
using Sitecore.Rocks.Extensibility.Composition;
using Sitecore.Rocks.Extensibility.Managers;

namespace Sitecore.Rocks.UI.LayoutDesigners.Chunks
{
    [Export(typeof(IComposableManagerPopulator<IRenderingChunk>))]
    public class FileRenderingChunkManagerPopulator : IComposableManagerPopulator<IRenderingChunk>
    {
        public IEnumerable<IRenderingChunk> Populate()
        {
            var folder = Path.Combine(AppHost.User.SharedFolder, "RenderingChunks");

            if (!AppHost.Files.FolderExists(folder))
            {
                yield break;
            }

            foreach (var fileName in Directory.GetFiles(folder, "*.chunk.xml", SearchOption.AllDirectories))
            {
                var chunk = new FileBasedRenderingChunk(fileName);

                yield return chunk;
            }
        }
    }
}
