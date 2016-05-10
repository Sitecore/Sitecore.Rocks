// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Rocks.CodeGeneration.MvcModels.Models;
using Sitecore.Rocks.CodeGeneration.TemplateClasses;
using Sitecore.Rocks.CodeGeneration.TemplateClasses.Models;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Diagnostics;

namespace Sitecore.Rocks.CodeGeneration.MvcModels
{
    [CodeGenerator("MVC Model")]
    public class MvcModelCodeGenerator : TemplateClassesCodeGenerator
    {
        public override Template CreateTemplate(IShapeCreator shapeCreator, ItemUri templateUri)
        {
            Assert.ArgumentNotNull(shapeCreator, nameof(shapeCreator));
            Assert.ArgumentNotNull(templateUri, nameof(templateUri));

            return new MvcModelTemplate(shapeCreator, templateUri);
        }

        public override TemplateField CreateTemplateField(IShapeCreator shapeCreator)
        {
            Assert.ArgumentNotNull(shapeCreator, nameof(shapeCreator));

            return new MvcModelTemplateField();
        }

        public override TemplateSection CreateTemplateSection(IShapeCreator shapeCreator)
        {
            Assert.ArgumentNotNull(shapeCreator, nameof(shapeCreator));

            return new MvcModelTemplateSection(shapeCreator);
        }
    }
}
