// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.CodeGeneration.CodeFirstTemplates.Templates;

namespace Sitecore.Rocks.CodeGeneration.CodeFirstTemplates.FieldTypes
{
    public interface IFieldTypeHandler
    {
        bool CanHandle([NotNull] string type);

        void Handle([NotNull] string type, [NotNull] TemplateField field);
    }
}
