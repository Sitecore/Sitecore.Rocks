// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.Collections.Generic;
using System.Windows.Controls;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Extensibility;
using Sitecore.Rocks.Sites;
using Sitecore.Rocks.Sites.Connections;

namespace Sitecore.Rocks.Data
{
    [ExtensibilityInitialization(PreInit = "Clear")]
    public static class DataServiceManager
    {
        private static readonly Dictionary<string, DataService> Cache = new Dictionary<string, DataService>();

        private static readonly Type DataServiceType = typeof(DataService);

        private static readonly Dictionary<string, DataServiceDescriptor> TypeDescriptors = new Dictionary<string, DataServiceDescriptor>();

        [NotNull]
        public static Dictionary<string, DataServiceDescriptor> Types
        {
            get { return TypeDescriptors; }
        }

        public static void Add([NotNull] string typeName, [NotNull] Type type, double priority)
        {
            Assert.ArgumentNotNull(typeName, nameof(typeName));
            Assert.ArgumentNotNull(type, nameof(type));

            Types[typeName] = new DataServiceDescriptor(type, priority);
        }

        [UsedImplicitly]
        public static void Clear()
        {
            Types.Clear();
        }

        [NotNull, Obsolete]
        public static DataService GetInstance([NotNull] string typeName, [NotNull] string server, [NotNull] SiteCredentials credentials, [NotNull] string webRootPath)
        {
            Assert.ArgumentNotNull(typeName, nameof(typeName));
            Assert.ArgumentNotNull(server, nameof(server));
            Assert.ArgumentNotNull(credentials, nameof(credentials));
            Assert.ArgumentNotNull(webRootPath, nameof(webRootPath));

            var key = typeName + '|' + server + '|' + credentials.UserName + '|' + credentials.Password;

            DataService dataService;
            if (Cache.TryGetValue(key, out dataService))
            {
                return dataService;
            }

            DataServiceDescriptor descriptor;
            if (!TypeDescriptors.TryGetValue(typeName, out descriptor))
            {
                return DataService.Empty;
            }

            dataService = Activator.CreateInstance(descriptor.Type) as DataService;
            if (dataService == null)
            {
                dataService = DataService.Empty;
            }
            else
            {
                dataService.Initialize(server, credentials);
            }

            Cache[key] = dataService;

            return dataService;
        }

        [NotNull]
        public static DataService GetInstance([NotNull] Connection connection)
        {
            Assert.ArgumentNotNull(connection, nameof(connection));

            var key = connection.DataServiceName + '|' + connection.HostName + '|' + connection.UserName + '|' + connection.Password;

            DataService dataService;
            if (Cache.TryGetValue(key, out dataService))
            {
                return dataService;
            }

            DataServiceDescriptor type;
            if (!TypeDescriptors.TryGetValue(connection.DataServiceName, out type))
            {
                return DataService.Empty;
            }

            dataService = Activator.CreateInstance(type.Type) as DataService;
            if (dataService == null)
            {
                dataService = DataService.Empty;
            }
            else
            {
                dataService.Initialize(connection);
            }

            Cache[key] = dataService;

            return dataService;
        }

        [CanBeNull]
        public static Control GetSiteEditorControl([NotNull] string dataServiceName)
        {
            Assert.ArgumentNotNull(dataServiceName, nameof(dataServiceName));

            DataServiceDescriptor descriptor;
            if (!TypeDescriptors.TryGetValue(dataServiceName, out descriptor))
            {
                return null;
            }

            var dataService = Activator.CreateInstance(descriptor.Type) as DataService;
            if (dataService == null)
            {
                return null;
            }

            return dataService.GetSiteEditorControl();
        }

        public static void LoadType([NotNull] Type type, [NotNull] DataServiceAttribute attribute)
        {
            Assert.ArgumentNotNull(type, nameof(type));
            Assert.ArgumentNotNull(attribute, nameof(attribute));

            if (!(type == DataServiceType || type.IsSubclassOf(DataServiceType)))
            {
                return;
            }

            Add(attribute.TypeName, type, attribute.Priority);
        }

        public class DataServiceDescriptor
        {
            public DataServiceDescriptor([NotNull] Type type, double priority)
            {
                Assert.ArgumentNotNull(type, nameof(type));

                Type = type;
                Priority = priority;
            }

            public double Priority { get; private set; }

            [NotNull]
            public Type Type { get; }
        }
    }
}
