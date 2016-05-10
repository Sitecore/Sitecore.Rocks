// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.CodeGeneration.TemplateClasses;
using Sitecore.Rocks.CodeGeneration.TemplateClasses.Models;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Diagnostics;

namespace Sitecore.Rocks.CodeGeneration.MvcModels.Models
{
    public class MvcModelTemplate : Template
    {
        public MvcModelTemplate([NotNull] IShapeCreator shapeCreator, [NotNull] ItemUri templateUri) : base(shapeCreator, templateUri)
        {
            Assert.ArgumentNotNull(shapeCreator, nameof(shapeCreator));
            Assert.ArgumentNotNull(templateUri, nameof(templateUri));
        }
    }
}
