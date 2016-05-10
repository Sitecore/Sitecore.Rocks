// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Diagnostics;

namespace Sitecore.Rocks.Applications.Storages
{
    public static class Storage
    {
        private static readonly BaseStorage _storage;

        static Storage()
        {
            _storage = AppHost.Settings.GetStorage();
        }

        public static void Delete([NotNull] string path)
        {
            Assert.ArgumentNotNull(path, nameof(path));

            _storage.Delete(path);
        }

        public static void Delete([NotNull] string path, [NotNull] string key)
        {
            Assert.ArgumentNotNull(path, nameof(path));
            Assert.ArgumentNotNull(key, nameof(key));

            _storage.Delete(path, key);
        }

        [NotNull]
        public static IEnumerable<string> GetKeys([NotNull] string path)
        {
            Assert.ArgumentNotNull(path, nameof(path));

            return _storage.GetKeys(path);
        }

        [NotNull]
        public static IEnumerable<string> GetSubPaths([NotNull] string path)
        {
            Assert.ArgumentNotNull(path, nameof(path));

            return _storage.GetSubPaths(path);
        }

        [CanBeNull]
        public static object Read([NotNull, Localizable(false)] string path, [NotNull, Localizable(false)] string key, [CanBeNull, Localizable(false)] object defaultValue)
        {
            Assert.ArgumentNotNull(path, nameof(path));
            Assert.ArgumentNotNull(key, nameof(key));

            if (_storage == null)
            {
                return defaultValue;
            }

            return _storage.Read(path, key, defaultValue);
        }

        public static bool ReadBool([NotNull, Localizable(false)] string path, [NotNull, Localizable(false)] string key, bool defaultValue)
        {
            Assert.ArgumentNotNull(path, nameof(path));
            Assert.ArgumentNotNull(key, nameof(key));

            var value = Read(path, key, null) as string;
            if (value == null)
            {
                return defaultValue;
            }

            return string.Compare(value, "True", StringComparison.InvariantCultureIgnoreCase) == 0;
        }

        public static double ReadDouble([NotNull, Localizable(false)] string path, [NotNull, Localizable(false)] string key, double defaultValue)
        {
            Assert.ArgumentNotNull(path, nameof(path));
            Assert.ArgumentNotNull(key, nameof(key));

            var value = Read(path, key, null) as string;
            if (value == null)
            {
                return defaultValue;
            }

            double result;
            if (!double.TryParse(value, out result))
            {
                return defaultValue;
            }

            return result;
        }

        public static int ReadInt([NotNull, Localizable(false)] string path, [NotNull, Localizable(false)] string key, int defaultValue)
        {
            Assert.ArgumentNotNull(path, nameof(path));
            Assert.ArgumentNotNull(key, nameof(key));

            var value = Read(path, key, null) as string;
            if (value == null)
            {
                return defaultValue;
            }

            int result;
            if (!int.TryParse(value, out result))
            {
                return defaultValue;
            }

            return result;
        }

        [NotNull]
        public static string ReadString([Localizable(false), NotNull] string path, [Localizable(false), NotNull] string key, [NotNull] string defaultValue)
        {
            Assert.ArgumentNotNull(path, nameof(path));
            Assert.ArgumentNotNull(key, nameof(key));
            Assert.ArgumentNotNull(defaultValue, nameof(defaultValue));

            return Read(path, key, defaultValue) as string ?? defaultValue;
        }

        public static void Write([NotNull, Localizable(false)] string path, [NotNull, Localizable(false)] string key, [CanBeNull, Localizable(false)] object value)
        {
            Assert.ArgumentNotNull(path, nameof(path));
            Assert.ArgumentNotNull(key, nameof(key));

            _storage.Write(path, key, value);
        }

        public static void WriteBool([NotNull, Localizable(false)] string path, [NotNull, Localizable(false)] string key, bool value)
        {
            Assert.ArgumentNotNull(path, nameof(path));
            Assert.ArgumentNotNull(key, nameof(key));

            _storage.Write(path, key, value ? @"True" : @"False");
        }
    }
}
