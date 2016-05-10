// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Commands;
using Sitecore.Rocks.Diagnostics;

namespace Sitecore.Rocks.CodeGeneration.DesignSurfaces
{
    public class DesignSurfaceContext : ICommandContext
    {
        public DesignSurfaceContext([NotNull] DesignSurface designSurface)
        {
            Assert.ArgumentNotNull(designSurface, nameof(designSurface));

            DesignSurface = designSurface;
        }

        [NotNull]
        public DesignSurface DesignSurface { get; private set; }
    }
}
