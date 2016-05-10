// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using EnvDTE80;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Diagnostics;

namespace Sitecore.Rocks.CodeGeneration.CodeFirstTemplates.Templates
{
    public class TemplateField
    {
        public TemplateField([NotNull] CodeProperty2 property)
        {
            Assert.ArgumentNotNull(property, nameof(property));

            Property = property;
            Name = string.Empty;
            TemplateFieldItemId = ItemId.Empty;
            Icon = string.Empty;
            Type = string.Empty;
            Source = string.Empty;
            Title = string.Empty;
            ValidatorBar = string.Empty;
        }

        [NotNull]
        public string Icon { get; set; }

        [NotNull]
        public string Name { get; set; }

        [NotNull]
        public CodeProperty2 Property { get; private set; }

        public bool Shared { get; set; }

        [NotNull]
        public string Source { get; set; }

        [NotNull]
        public ItemId TemplateFieldItemId { get; set; }

        [NotNull]
        public string Title { get; set; }

        [NotNull]
        public string Type { get; set; }

        public bool Unversioned { get; set; }

        [NotNull]
        public string ValidatorBar { get; set; }
    }
}
