// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.Collections.Generic;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Extensibility;

namespace Sitecore.Rocks.Media.Skins
{
    [ExtensibilityInitialization(PreInit = "Clear")]
    public static class MediaSkinManager
    {
        private static readonly string mediaSkinInterface = typeof(IMediaSkin).FullName;

        private static readonly Dictionary<string, MediaSkin> types = new Dictionary<string, MediaSkin>();

        [NotNull]
        public static Dictionary<string, MediaSkin> Skins
        {
            get { return types; }
        }

        public static void Add([NotNull] string typeName, [NotNull] MediaSkin skin)
        {
            Assert.ArgumentNotNull(typeName, nameof(typeName));
            Assert.ArgumentNotNull(skin, nameof(skin));

            Skins[typeName] = skin;
        }

        [UsedImplicitly]
        public static void Clear()
        {
            Skins.Clear();
        }

        [NotNull]
        public static IMediaSkin GetDefaultInstance()
        {
            return new ExtraLarge.ExtraLarge();
        }

        [CanBeNull]
        public static IMediaSkin GetInstance([NotNull] string typeName)
        {
            Assert.ArgumentNotNull(typeName, nameof(typeName));

            MediaSkin mediaSkin;

            if (!types.TryGetValue(typeName, out mediaSkin))
            {
                return null;
            }

            var constructor = mediaSkin.Type.GetConstructor(Type.EmptyTypes);
            if (constructor == null)
            {
                return null;
            }

            return constructor.Invoke(null) as IMediaSkin;
        }

        public static void LoadType([NotNull] Type type, [NotNull] MediaSkinAttribute attribute)
        {
            Assert.ArgumentNotNull(type, nameof(type));
            Assert.ArgumentNotNull(attribute, nameof(attribute));

            var i = type.GetInterface(mediaSkinInterface);
            if (i == null)
            {
                return;
            }

            var mediaSkin = new MediaSkin
            {
                Type = type,
                Priority = attribute.Priority,
                SkinName = attribute.SkinName
            };

            Add(attribute.SkinName, mediaSkin);
        }

        public class MediaSkin
        {
            public double Priority { get; set; }

            public string SkinName { get; set; }

            public Type Type { get; set; }
        }
    }
}
