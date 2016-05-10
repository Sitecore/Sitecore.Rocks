// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Sitecore.Diagnostics;
using Sitecore.IO;
using Sitecore.Rocks.Server.Extensibility.Composition;

namespace Sitecore.Rocks.Server.Extensibility
{
    public static class ExtensibilityLoader
    {
        private static bool isInitialized;

        public static void Initialize()
        {
            if (isInitialized)
            {
                return;
            }

            isInitialized = true;

            var types = LoadExtensibilityTypes();

            UnexportTypes(types);

            Load(types, (attribute, type) => attribute.PreInitialize(type));
            Load(types, (attribute, type) => attribute.Initialize(type));
            Load(types, (attribute, type) => attribute.PostInitialize(type));
        }

        private static void Load([NotNull] List<Tuple<Type, object[]>> extensibilityTypes, [NotNull] LoadDelegate load)
        {
            Debug.ArgumentNotNull(extensibilityTypes, nameof(extensibilityTypes));
            Debug.ArgumentNotNull(load, nameof(load));

            foreach (var extensibilityType in extensibilityTypes)
            {
                foreach (var obj in extensibilityType.Item2)
                {
                    var attribute = obj as ExtensibilityAttribute;
                    if (attribute != null)
                    {
                        load(attribute, extensibilityType.Item1);
                    }
                }
            }
        }

        [NotNull]
        private static List<Tuple<Type, object[]>> LoadExtensibilityTypes()
        {
            var result = new List<Tuple<Type, object[]>>();

            LoadExtensibilityTypes(result);

            return result;
        }

        private static void LoadExtensibilityTypes([NotNull] List<Tuple<Type, object[]>> types, [NotNull] Assembly assembly)
        {
            Debug.ArgumentNotNull(types, nameof(types));
            Debug.ArgumentNotNull(assembly, nameof(assembly));

            foreach (var type in assembly.GetTypes())
            {
                var attributes = type.GetCustomAttributes(typeof(ExtensibilityAttribute), true);
                if (attributes.Length != 0)
                {
                    types.Add(new Tuple<Type, object[]>(type, attributes));
                }
            }
        }

        private static void LoadExtensibilityTypes([NotNull] List<Tuple<Type, object[]>> types)
        {
            Debug.ArgumentNotNull(types, nameof(types));

            var files = new List<string>();

            // var folder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var folder = FileUtil.MapPath("/bin");
            files.AddRange(Directory.GetFiles(folder, @"Sitecore.Rocks.Server*.dll"));

            files.Sort();

            foreach (var file in files)
            {
                try
                {
                    var assembly = Assembly.LoadFrom(file);
                    if (assembly != null)
                    {
                        LoadExtensibilityTypes(types, assembly);
                    }
                }
                catch
                {
                    continue;
                }
            }
        }

        private static void UnexportTypes([NotNull] List<Tuple<Type, object[]>> types)
        {
            var list = types.SelectMany(t => t.Item2.OfType<UnexportAttribute>()).ToList();
            foreach (var attribute in list)
            {
                var type = attribute.Type;
                types.RemoveAll(t => t.Item1 == type);
            }
        }

        private delegate void LoadDelegate(ExtensibilityAttribute attribute, Type type);

        private class Tuple<T1, T2>
        {
            public Tuple(T1 item1, T2 item2)
            {
                Item1 = item1;
                Item2 = item2;
            }

            public T1 Item1 { get; }

            public T2 Item2 { get; }
        }
    }
}
