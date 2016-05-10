// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.CodeGeneration.TemplateClasses.Models;
using Sitecore.Rocks.Data;

namespace Sitecore.Rocks.CodeGeneration.TemplateClasses
{
    public interface IShapeCreator
    {
        [NotNull]
        Template CreateTemplate([NotNull] IShapeCreator shapeCreator, [NotNull] ItemUri templateUri);

        [NotNull]
        TemplateField CreateTemplateField([NotNull] IShapeCreator shapeCreator);

        [NotNull]
        TemplateSection CreateTemplateSection([NotNull] IShapeCreator shapeCreator);
    }
}
