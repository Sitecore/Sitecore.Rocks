// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Diagnostics;

namespace Sitecore.Rocks.UI.TemplateDesigner.Dialogs.BuildSourceDialogs
{
    public class BuildSourceField
    {
        public BuildSourceField([NotNull] string type, [NotNull] string source)
        {
            Assert.ArgumentNotNull(type, nameof(type));
            Assert.ArgumentNotNull(source, nameof(source));

            Type = type;
            Source = source;
        }

        [CanBeNull]
        public string Id { get; set; }

        [CanBeNull]
        public string Name { get; set; }

        [CanBeNull]
        public string SectionName { get; set; }

        public bool Shared { get; set; }

        [NotNull]
        public string Source { get; private set; }

        public string Type { get; private set; }

        public bool Unversioned { get; set; }

        [CanBeNull]
        public string Validations { get; set; }
    }
}
