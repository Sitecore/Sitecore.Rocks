// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.CodeGeneration.DesignSurfaces;
using Sitecore.Rocks.Diagnostics;

namespace Sitecore.Rocks.CodeGeneration.TemplateClasses
{
    public class TemplateClassesContext : DesignSurfaceContext
    {
        public TemplateClassesContext([NotNull] DesignSurface designSurface) : base(designSurface)
        {
            Assert.ArgumentNotNull(designSurface, nameof(designSurface));
        }
    }
}
