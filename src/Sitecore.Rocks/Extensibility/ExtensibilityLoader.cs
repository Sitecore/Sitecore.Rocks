// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Applications.Storages;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Extensibility.Composition;

namespace Sitecore.Rocks.Extensibility
{
    public static class ExtensibilityLoader
    {
        [NotNull]
        public static IEnumerable<string> GetFeatureAssemblies()
        {
            var folder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? string.Empty;

            var files = new List<string>();

            files.Add(Path.Combine(folder, @"Sitecore.Rocks.dll"));
            files.AddRange(AppHost.Files.GetFiles(folder, @"Sitecore.Rocks.*.dll"));
            files.AddRange(AppHost.Files.GetFiles(folder, @"Sitecore.Rocks.*.exe"));

            return files;
        }

        public static void Initialize()
        {
            Trace.WriteLine("Initializing ExtensibilityLoader");

            Trace.Indent();

            AppHost.Features.Clear();

            var assemblies = LoadAssemblies();
            var types = LoadTypes(assemblies);

            FilterFeatures(types);

            Trace.Unindent();

            AppHost.Extensibility.Catalog.Clear();

            UnexportTypes(types);

            Load(types, (attribute, type) => attribute.PreInitialize(type));
            Load(types, (attribute, type) => attribute.Initialize(type));
            Load(types, (attribute, type) => attribute.PostInitialize(type));
        }

        private static void FilterFeatures([NotNull] List<Tuple<Type, object[]>> types)
        {
            Debug.ArgumentNotNull(types, nameof(types));

            var features = AppHost.Features.GetFeatureStates();
            var attributeType = typeof(FeatureAttribute);

            for (var i = types.Count - 1; i >= 0; i--)
            {
                var type = types[i].Item1;

                var attributes = type.GetCustomAttributes(attributeType, true);
                if (attributes.Length == 0)
                {
                    continue;
                }

                var featureAttribute = attributes[0] as FeatureAttribute;
                if (featureAttribute == null)
                {
                    continue;
                }

                bool isEnabled;
                if (!features.TryGetValue(featureAttribute.FeatureName, out isEnabled))
                {
                    isEnabled = AppHost.Features.Add(featureAttribute.FeatureName).IsEnabled;

                    features[featureAttribute.FeatureName] = isEnabled;
                }

                if (!isEnabled)
                {
                    types.Remove(types[i]);
                }
            }
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
        private static IEnumerable<Assembly> LoadAssemblies()
        {
            var assemblies = new List<Assembly>();

            LoadFeatureAssemblies(assemblies);

            LoadPluginAssemblies(assemblies);

            LoadRegisteredAssemblies(assemblies);

            return assemblies;
        }

        private static void LoadFeatureAssemblies([NotNull] ICollection<Assembly> assemblies)
        {
            Debug.ArgumentNotNull(assemblies, nameof(assemblies));

            var fileNames = GetFeatureAssemblies();
            foreach (var file in fileNames.OrderBy(s => s))
            {
                var fileName = Path.GetFileNameWithoutExtension(file) ?? string.Empty;
                if (fileName.StartsWith(Constants.SitecoreRocksServer, StringComparison.InvariantCultureIgnoreCase))
                {
                    continue;
                }

                if (!AppHost.Features.IsFeatureAssemblyEnabled(file))
                {
                    continue;
                }

                var assemblyFileName = Path.GetFileName(file);
                if (assemblies.Any(a => string.Equals(Path.GetFileName(a.Location), assemblyFileName, StringComparison.OrdinalIgnoreCase)))
                {
                    continue;
                }

                var assembly = AppHost.Plugins.SafeLoadAssembly(file);

                if (assembly != null && !assemblies.Contains(assembly))
                {
                    assemblies.Add(assembly);
                }
            }
        }

        private static void LoadPluginAssemblies([NotNull] ICollection<Assembly> assemblies)
        {
            Debug.ArgumentNotNull(assemblies, nameof(assemblies));

            var fileNames = new List<string>();

            AppHost.Plugins.GetAssemblies(fileNames, true, true, false);

            foreach (var fileName in fileNames)
            {
                if (AppHost.Plugins.IsServerComponent(fileName))
                {
                    continue;
                }

                var assemblyFileName = Path.GetFileName(fileName);
                if (assemblies.Any(a => string.Equals(Path.GetFileName(a.Location), assemblyFileName, StringComparison.OrdinalIgnoreCase)))
                {
                    continue;
                }

                var assembly = AppHost.Plugins.SafeLoadAssembly(fileName);

                if (assembly != null && !assemblies.Contains(assembly))
                {
                    assemblies.Add(assembly);
                }
            }
        }

        private static void LoadRegisteredAssemblies([NotNull] ICollection<Assembly> assemblies)
        {
            Debug.ArgumentNotNull(assemblies, nameof(assemblies));

            foreach (var key in Storage.GetKeys(@"Extensibility\Assemblies"))
            {
                // TODO: This is not working
                var fileName = AppHost.Settings.Get(@"Extensibility\Assemblies", key, null) as string ?? string.Empty;
                if (string.IsNullOrEmpty(fileName))
                {
                    continue;
                }

                var assemblyFileName = Path.GetFileName(fileName);
                if (assemblies.Any(a => string.Equals(Path.GetFileName(a.Location), assemblyFileName, StringComparison.OrdinalIgnoreCase)))
                {
                    continue;
                }

                var assembly = AppHost.Plugins.SafeLoadAssembly(fileName);
                if (assembly != null && !assemblies.Contains(assembly))
                {
                    assemblies.Add(assembly);
                }
            }
        }

        [NotNull]
        private static List<Tuple<Type, object[]>> LoadTypes([NotNull] IEnumerable<Assembly> assemblies)
        {
            Debug.ArgumentNotNull(assemblies, nameof(assemblies));

            var types = new List<Tuple<Type, object[]>>();

            foreach (var assembly in assemblies)
            {
                try
                {
                    LoadTypesFromAssembly(types, assembly);
                }
                catch (Exception ex)
                {
                    AppHost.Output.LogException(ex);
                }
            }

            return types;
        }

        private static void LoadTypesFromAssembly([NotNull] ICollection<Tuple<Type, object[]>> types, [NotNull] Assembly assembly)
        {
            Debug.ArgumentNotNull(types, nameof(types));
            Debug.ArgumentNotNull(assembly, nameof(assembly));

            Trace.WriteLine(@"Loading: {0} ({1})", assembly.FullName, assembly.Location);

            foreach (var type in assembly.GetTypes())
            {
                var attributes = type.GetCustomAttributes(typeof(ExtensibilityAttribute), true);
                if (attributes.Length != 0)
                {
                    types.Add(new Tuple<Type, object[]>(type, attributes));
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
    }
}
