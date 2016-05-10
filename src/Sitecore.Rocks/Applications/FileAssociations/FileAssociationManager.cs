// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.Collections.Generic;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Diagnostics;

namespace Sitecore.Rocks.Applications.FileAssociations
{
    public static class FileAssociationManager
    {
        private static readonly List<FileAssociationDescriptor> associations = new List<FileAssociationDescriptor>();

        [NotNull]
        public static IEnumerable<FileAssociationDescriptor> Associations
        {
            get { return associations; }
        }

        public static void LoadType([NotNull] Type type, [NotNull] FileAssociationAttribute attribute)
        {
            Assert.ArgumentNotNull(type, nameof(type));
            Assert.ArgumentNotNull(attribute, nameof(attribute));

            var instance = Activator.CreateInstance(type) as IFileAssociation;
            if (instance == null)
            {
                Trace.TraceError(string.Format("{0} does not implement interface 'IFileAssociation'", type.FullName));
                return;
            }

            var descriptor = new FileAssociationDescriptor(instance, attribute);

            associations.Add(descriptor);
        }

        [CanBeNull]
        public static IFileAssociation OpenFile([NotNull] string fileName)
        {
            Assert.ArgumentNotNull(fileName, nameof(fileName));

            foreach (var association in associations)
            {
                if (fileName.EndsWith(association.Attribute.Extension, StringComparison.InvariantCultureIgnoreCase))
                {
                    association.Instance.Open(fileName);

                    return association.Instance;
                }
            }

            return null;
        }

        public class FileAssociationDescriptor
        {
            public FileAssociationDescriptor([NotNull] IFileAssociation instance, [NotNull] FileAssociationAttribute attribute)
            {
                Assert.ArgumentNotNull(instance, nameof(instance));
                Assert.ArgumentNotNull(attribute, nameof(attribute));

                Instance = instance;
                Attribute = attribute;
            }

            [NotNull]
            public FileAssociationAttribute Attribute { get; }

            [NotNull]
            public IFileAssociation Instance { get; set; }
        }
    }
}
