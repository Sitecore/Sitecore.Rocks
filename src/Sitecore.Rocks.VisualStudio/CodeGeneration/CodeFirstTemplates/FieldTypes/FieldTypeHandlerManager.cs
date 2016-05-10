// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.Collections.Generic;
using System.Linq;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Extensibility;

namespace Sitecore.Rocks.CodeGeneration.CodeFirstTemplates.FieldTypes
{
    [ExtensibilityInitialization(PreInit = @"Clear")]
    public static class FieldTypeHandlerManager
    {
        private static readonly List<FieldTypeHandlerDescriptor> fieldTypes = new List<FieldTypeHandlerDescriptor>();

        [NotNull]
        public static IEnumerable<IFieldTypeHandler> FieldTypes
        {
            get { return fieldTypes.Select(h => h.Handler); }
        }

        [UsedImplicitly]
        public static void Clear()
        {
            fieldTypes.Clear();
        }

        internal static void LoadType([NotNull] Type type, [NotNull] FieldTypeHandlerAttribute attribute)
        {
            Debug.ArgumentNotNull(type, nameof(type));
            Debug.ArgumentNotNull(attribute, nameof(attribute));

            var instance = Activator.CreateInstance(type) as IFieldTypeHandler;
            if (instance == null)
            {
                Trace.TraceWarning(string.Format("Type {0} does not implement IFieldTypeHandler", type.FullName));
                return;
            }

            var descriptor = new FieldTypeHandlerDescriptor(instance);

            fieldTypes.Add(descriptor);
        }

        internal class FieldTypeHandlerDescriptor
        {
            public FieldTypeHandlerDescriptor([NotNull] IFieldTypeHandler handler)
            {
                Assert.ArgumentNotNull(handler, nameof(handler));

                Handler = handler;
            }

            [NotNull]
            public IFieldTypeHandler Handler { get; }
        }
    }
}
