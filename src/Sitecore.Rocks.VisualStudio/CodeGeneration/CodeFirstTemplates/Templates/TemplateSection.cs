// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Collections.Generic;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Data;

namespace Sitecore.Rocks.CodeGeneration.CodeFirstTemplates.Templates
{
    public class TemplateSection
    {
        private readonly List<TemplateField> fields = new List<TemplateField>();

        public TemplateSection()
        {
            Name = string.Empty;
            TemplateSectionItemId = ItemId.Empty;
            Icon = string.Empty;
        }

        [NotNull]
        public ICollection<TemplateField> Fields
        {
            get { return fields; }
        }

        [NotNull]
        public string Icon { get; set; }

        [NotNull]
        public string Name { get; set; }

        [NotNull]
        public ItemId TemplateSectionItemId { get; set; }
    }
}
