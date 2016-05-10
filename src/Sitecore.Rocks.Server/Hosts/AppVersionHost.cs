// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.IO;
using System.Reflection;
using Sitecore.Configuration;
using Sitecore.Diagnostics;
using Sitecore.IO;

namespace Sitecore.Rocks.Server.Hosts
{
    public class AppVersionHost
    {
        [NotNull]
        public T Get<T>([NotNull] T defaultInstance, [NotNull] params Version[] versions) where T : class
        {
            Assert.ArgumentNotNull(defaultInstance, nameof(defaultInstance));
            Assert.ArgumentNotNull(versions, nameof(versions));

            var sitecoreVersion = new Version(About.GetVersionNumber(false));
            var version = new Version(0, 0);

            foreach (var v in versions)
            {
                if (v > version && v < sitecoreVersion)
                {
                    version = v;
                }
            }

            if (version.Major <= 0)
            {
                return defaultInstance;
            }

            return LoadInstance(defaultInstance, version);
        }

        [NotNull]
        private T LoadInstance<T>([NotNull] T defaultInstance, [NotNull] Version version) where T : class
        {
            Debug.ArgumentNotNull(defaultInstance, nameof(defaultInstance));
            Debug.ArgumentNotNull(version, nameof(version));

            var fileName = FileUtil.MapPath("/bin");
            fileName = Path.Combine(fileName, "Sitecore.Rocks.Server.Cms" + version.Major + ".dll");

            if (!File.Exists(fileName))
            {
                return defaultInstance;
            }

            Assembly assembly;
            try
            {
                assembly = Assembly.LoadFrom(fileName);
            }
            catch (Exception ex)
            {
                Log.Error("Failed to load assembly: " + fileName, ex, this);
                throw;
            }

            var typeName = typeof(T).FullName + version.Major + version.Minor;

            var type = assembly.GetType(typeName);
            if (type == null)
            {
                throw new InvalidOperationException("Type not found: " + typeName);
            }

            var result = Activator.CreateInstance(type) as T;
            if (result == null)
            {
                throw new InvalidOperationException("Type could not be instantiated: " + typeName);
            }

            return result;
        }
    }
}
