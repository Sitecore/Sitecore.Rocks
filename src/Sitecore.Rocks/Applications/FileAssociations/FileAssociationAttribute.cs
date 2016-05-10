// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Extensibility;

namespace Sitecore.Rocks.Applications.FileAssociations
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = true), BaseTypeRequired(typeof(IFileAssociation)), MeansImplicitUse]
    public class FileAssociationAttribute : ExtensibilityAttribute
    {
        public FileAssociationAttribute([NotNull] string extension)
        {
            Assert.ArgumentNotNull(extension, nameof(extension));

            Extension = extension;
        }

        [NotNull]
        public string Extension { get; private set; }

        public override void Initialize([NotNull] Type type)
        {
            Assert.ArgumentNotNull(type, nameof(type));

            FileAssociationManager.LoadType(type, this);
        }
    }
}
