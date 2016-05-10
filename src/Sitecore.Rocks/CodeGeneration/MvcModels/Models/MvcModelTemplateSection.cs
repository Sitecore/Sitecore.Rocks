// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.CodeGeneration.TemplateClasses;
using Sitecore.Rocks.CodeGeneration.TemplateClasses.Models;
using Sitecore.Rocks.Diagnostics;

namespace Sitecore.Rocks.CodeGeneration.MvcModels.Models
{
    public class MvcModelTemplateSection : TemplateSection
    {
        public MvcModelTemplateSection([NotNull] IShapeCreator shapeCreator) : base(shapeCreator)
        {
            Assert.ArgumentNotNull(shapeCreator, nameof(shapeCreator));
        }
    }
}
