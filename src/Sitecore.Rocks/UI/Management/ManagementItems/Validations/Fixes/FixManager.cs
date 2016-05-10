// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.Collections.Generic;
using System.Linq;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Extensibility;

namespace Sitecore.Rocks.UI.Management.ManagementItems.Validations.Fixes
{
    [ExtensibilityInitialization(PreInit = "Clear")]
    public static class FixManager
    {
        private static readonly List<FixDescriptor> fixes = new List<FixDescriptor>();

        private static readonly string fixInterface = typeof(IFix).FullName;

        [NotNull]
        public static IEnumerable<IFix> Fixes
        {
            get { return fixes.Select(f => f.Fix); }
        }

        public static void Add([NotNull] FixDescriptor fixDescriptor)
        {
            Assert.ArgumentNotNull(fixDescriptor, nameof(fixDescriptor));

            fixes.Add(fixDescriptor);
        }

        [UsedImplicitly]
        public static void Clear()
        {
            fixes.Clear();
        }

        [CanBeNull]
        public static IFix GetFix([NotNull] ValidationDescriptor validationDescriptor)
        {
            Assert.ArgumentNotNull(validationDescriptor, nameof(validationDescriptor));

            return Fixes.FirstOrDefault(f => f.CanFix(validationDescriptor));
        }

        public static void LoadType([NotNull] Type type, [NotNull] FixAttribute attribute)
        {
            Assert.ArgumentNotNull(type, nameof(type));
            Assert.ArgumentNotNull(attribute, nameof(attribute));

            var i = type.GetInterface(fixInterface);
            if (i == null)
            {
                return;
            }

            IFix fix;
            try
            {
                var constructorInfo = type.GetConstructor(Type.EmptyTypes);
                if (constructorInfo == null)
                {
                    return;
                }

                fix = constructorInfo.Invoke(null) as IFix;
            }
            catch
            {
                Trace.TraceError("Fix threw an exception in the constructor");
                return;
            }

            if (fix == null)
            {
                Trace.TraceError("Fix does not have a parameterless constructor");
                return;
            }

            var descriptor = new FixDescriptor(attribute, fix);

            Add(descriptor);
        }

        public class FixDescriptor
        {
            public FixDescriptor([NotNull] FixAttribute attribute, [NotNull] IFix fix)
            {
                Assert.ArgumentNotNull(attribute, nameof(attribute));
                Assert.ArgumentNotNull(fix, nameof(fix));

                Attribute = attribute;
                Fix = fix;
            }

            [NotNull]
            public FixAttribute Attribute { get; private set; }

            [NotNull]
            public IFix Fix { get; }
        }
    }
}
