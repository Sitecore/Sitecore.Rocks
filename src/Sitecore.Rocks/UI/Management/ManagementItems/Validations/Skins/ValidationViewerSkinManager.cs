// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.Collections.Generic;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Extensibility;
using Sitecore.Rocks.UI.Management.ManagementItems.Validations.Skins.ActionSkins;

namespace Sitecore.Rocks.UI.Management.ManagementItems.Validations.Skins
{
    [ExtensibilityInitialization(PreInit = "Clear")]
    public static class ValidationViewerSkinManager
    {
        private static readonly string mediaSkinInterface = typeof(IValidationViewerSkin).FullName;

        private static readonly Dictionary<string, SkinDescriptor> types = new Dictionary<string, SkinDescriptor>();

        [NotNull]
        public static Dictionary<string, SkinDescriptor> Skins
        {
            get { return types; }
        }

        public static void Add([NotNull] string typeName, [NotNull] SkinDescriptor skin)
        {
            Assert.ArgumentNotNull(typeName, nameof(typeName));
            Assert.ArgumentNotNull(skin, nameof(skin));

            Skins.Add(typeName, skin);
        }

        [UsedImplicitly]
        public static void Clear()
        {
            Skins.Clear();
        }

        [NotNull]
        public static IValidationViewerSkin GetDefaultInstance()
        {
            return new ActionSkin();
        }

        [CanBeNull]
        public static IValidationViewerSkin GetInstance([NotNull] string typeName)
        {
            Assert.ArgumentNotNull(typeName, nameof(typeName));

            SkinDescriptor mediaSkin;

            if (!types.TryGetValue(typeName, out mediaSkin))
            {
                return null;
            }

            var constructor = mediaSkin.Type.GetConstructor(Type.EmptyTypes);
            if (constructor == null)
            {
                return null;
            }

            return constructor.Invoke(null) as IValidationViewerSkin;
        }

        public static void LoadType([NotNull] Type type, [NotNull] ValidationViewerSkinAttribute attribute)
        {
            Assert.ArgumentNotNull(type, nameof(type));
            Assert.ArgumentNotNull(attribute, nameof(attribute));

            var i = type.GetInterface(mediaSkinInterface);
            if (i == null)
            {
                return;
            }

            var mediaSkin = new SkinDescriptor(type, attribute.SkinName, attribute.Priority);

            Add(attribute.SkinName, mediaSkin);
        }

        public class SkinDescriptor
        {
            public SkinDescriptor([NotNull] Type type, [NotNull] string skinName, double priority)
            {
                Assert.ArgumentNotNull(type, nameof(type));
                Assert.ArgumentNotNull(skinName, nameof(skinName));

                Priority = priority;
                SkinName = skinName;
                Type = type;
            }

            public double Priority { get; private set; }

            [NotNull]
            public string SkinName { get; private set; }

            [NotNull]
            public Type Type { get; }
        }
    }
}
