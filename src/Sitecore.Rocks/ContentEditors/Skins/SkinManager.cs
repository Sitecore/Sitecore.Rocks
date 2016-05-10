// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.Collections.Generic;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.ContentEditors.Skins.Default;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Extensibility;

namespace Sitecore.Rocks.ContentEditors.Skins
{
    [ExtensibilityInitialization(PreInit = "Clear")]
    public static class SkinManager
    {
        private static readonly string skinInterface = typeof(ISkin).FullName;

        private static readonly Dictionary<string, Type> types = new Dictionary<string, Type>();

        [NotNull]
        public static Dictionary<string, Type> Types
        {
            get { return types; }
        }

        public static void Add([NotNull] string typeName, [NotNull] Type type)
        {
            Assert.ArgumentNotNull(typeName, nameof(typeName));
            Assert.ArgumentNotNull(type, nameof(type));

            Types[typeName] = type;
        }

        public static void Clear()
        {
            Types.Clear();
        }

        [NotNull]
        public static ISkin GetDefaultInstance()
        {
            return new DefaultSkin();
        }

        [CanBeNull]
        public static ISkin GetInstance([NotNull] string typeName)
        {
            Assert.ArgumentNotNull(typeName, nameof(typeName));

            Type type;

            if (!types.TryGetValue(typeName, out type))
            {
                return null;
            }

            try
            {
                return Activator.CreateInstance(type) as ISkin;
            }
            catch (Exception ex)
            {
                AppHost.Output.LogException(ex);
                return null;
            }
        }

        public static void LoadType([NotNull] Type type, [NotNull] SkinAttribute attribute)
        {
            Assert.ArgumentNotNull(type, nameof(type));
            Assert.ArgumentNotNull(attribute, nameof(attribute));

            var i = type.GetInterface(skinInterface);
            if (i != null)
            {
                Add(attribute.SkinName, type);
            }
        }
    }
}
