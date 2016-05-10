// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Collections.Generic;
using System.Linq;
using Microsoft.Win32;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Diagnostics;

namespace Sitecore.Rocks.Applications.Storages
{
    public class RegistryStorage : BaseStorage
    {
        public RegistryStorage([NotNull] string registryKey)
        {
            Assert.ArgumentNotNull(registryKey, nameof(registryKey));

            RegistryKey = registryKey;
        }

        [NotNull]
        protected string RegistryKey { get; set; }

        public override void Delete(string path)
        {
            Assert.ArgumentNotNull(path, nameof(path));

            var registryPath = RegistryKey + path;

            Registry.CurrentUser.DeleteSubKeyTree(registryPath, false);

            Notifications.RaiseSettingChanged(path, "DeleteSubKeyTree");
        }

        public override void Delete(string path, string key)
        {
            Assert.ArgumentNotNull(path, nameof(path));
            Assert.ArgumentNotNull(key, nameof(key));

            var registryPath = RegistryKey + path;

            var registryKey = Registry.CurrentUser.OpenSubKey(registryPath);
            if (registryKey == null)
            {
                return;
            }

            registryKey.DeleteValue(key, false);
            Notifications.RaiseSettingChanged(path, key);
        }

        public override IEnumerable<string> GetKeys(string path)
        {
            Assert.ArgumentNotNull(path, nameof(path));

            if (!string.IsNullOrEmpty(path))
            {
                path = RegistryKey + path;
            }
            else
            {
                path = RegistryKey;
            }

            var registryKey = Registry.CurrentUser.OpenSubKey(path);
            if (registryKey == null)
            {
                return Enumerable.Empty<string>();
            }

            return registryKey.GetValueNames();
        }

        public override IEnumerable<string> GetSubPaths(string path)
        {
            Assert.ArgumentNotNull(path, nameof(path));

            if (!string.IsNullOrEmpty(path))
            {
                path = RegistryKey + path;
            }
            else
            {
                path = RegistryKey;
            }

            var registryKey = Registry.CurrentUser.OpenSubKey(path);
            if (registryKey == null)
            {
                return Enumerable.Empty<string>();
            }

            return registryKey.GetSubKeyNames();
        }

        public override object Read(string path, string key, object defaultValue)
        {
            Assert.ArgumentNotNull(path, nameof(path));
            Assert.ArgumentNotNull(key, nameof(key));

            path = RegistryKey + path;

            var registryKey = Registry.CurrentUser.OpenSubKey(path);
            if (registryKey == null)
            {
                return null;
            }

            return registryKey.GetValue(key, defaultValue);
        }

        public override void Write(string path, string key, object value)
        {
            Assert.ArgumentNotNull(path, nameof(path));
            Assert.ArgumentNotNull(key, nameof(key));

            var registryPath = RegistryKey + path;

            var registryKey = Registry.CurrentUser.OpenSubKey(registryPath, true) ?? Registry.CurrentUser.CreateSubKey(registryPath);
            if (registryKey == null)
            {
                return;
            }

            if (value == null)
            {
                return;
            }

            registryKey.SetValue(key, value);
            Notifications.RaiseSettingChanged(path, key);
        }
    }
}
