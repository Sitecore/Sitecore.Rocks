// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Collections.Generic;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Diagnostics;

namespace Sitecore.Rocks.CodeGeneration.CodeFirstTemplates.Templates
{
    public class Template
    {
        private readonly List<TemplateSection> sections = new List<TemplateSection>();

        public Template([NotNull] object codeElement)
        {
            Assert.ArgumentNotNull(codeElement, nameof(codeElement));

            CodeElement = codeElement;
            Name = string.Empty;
            TemplateItemId = ItemId.Empty;
            Icon = string.Empty;
            BaseTemplates = string.Empty;
            ParentPath = "/sitecore/templates/user defined";
        }

        [NotNull]
        public string BaseTemplates { get; set; }

        [NotNull]
        public object CodeElement { get; set; }

        [NotNull]
        public string Icon { get; set; }

        [NotNull]
        public string Name { get; set; }

        [NotNull]
        public string ParentPath { get; set; }

        [NotNull]
        public ICollection<TemplateSection> Sections
        {
            get { return sections; }
        }

        [NotNull]
        public ItemId TemplateItemId { get; set; }
    }
}
