// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.ComponentModel;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Applications;
using Sitecore.Rocks.Applications.Storages;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Sites;

namespace Sitecore.Rocks.Shell.Environment
{
    public class SettingsHost
    {
        public SettingsHost()
        {
            Storage = new RegistryStorage("Software\\Sitecore\\Sitecore.Rocks.VisualStudio\\");
            Options = new Options("Windows");
        }

        [NotNull]
        public virtual DatabaseUri ActiveDatabaseUri
        {
            get
            {
                var databaseName = AppHost.Settings.GetString("Sites", "ActiveDatabaseName", string.Empty);
                if (string.IsNullOrEmpty(databaseName))
                {
                    return DatabaseUri.Empty;
                }

                var siteName = AppHost.Settings.GetString("Sites", "ActiveSiteName", string.Empty);

                var site = SiteManager.GetSite(siteName);
                if (site == null)
                {
                    return DatabaseUri.Empty;
                }

                return new DatabaseUri(site, new DatabaseName(databaseName));
            }

            set
            {
                Assert.ArgumentNotNull(value, nameof(value));

                var oldDatabase = ActiveDatabaseUri;

                var siteName = value == DatabaseUri.Empty ? string.Empty : value.Site.Name;
                var databaseName = value == DatabaseUri.Empty ? string.Empty : value.DatabaseName.ToString();

                AppHost.Settings.SetString("Sites", "ActiveSiteName", siteName);
                AppHost.Settings.SetString("Sites", "ActiveDatabaseName", databaseName);

                Notifications.RaiseActiveDatabaseChanged(oldDatabase, ActiveDatabaseUri);
            }
        }

        [NotNull]
        public IOptions Options { get; protected set; }

        [NotNull]
        public BaseStorage Storage { get; protected set; }

        public virtual void Delete([NotNull] string path)
        {
            Assert.ArgumentNotNull(path, nameof(path));

            Storage.Delete(path);
        }

        public virtual void Delete([NotNull] string path, [NotNull] string key)
        {
            Assert.ArgumentNotNull(path, nameof(path));
            Assert.ArgumentNotNull(key, nameof(key));

            Storage.Delete(path, key);
        }

        [CanBeNull]
        public virtual object Get([NotNull, Localizable(false)] string path, [NotNull, Localizable(false)] string key, [CanBeNull, Localizable(false)] object defaultValue)
        {
            Assert.ArgumentNotNull(path, nameof(path));
            Assert.ArgumentNotNull(key, nameof(key));

            return Storage.Read(path, key, defaultValue);
        }

        public virtual bool GetBool([Localizable(false), NotNull] string path, [Localizable(false), NotNull] string key, bool defaultValue)
        {
            Assert.ArgumentNotNull(path, nameof(path));
            Assert.ArgumentNotNull(key, nameof(key));

            return Applications.Storages.Storage.ReadBool(path, key, defaultValue);
        }

        public virtual double GetDouble([Localizable(false), NotNull] string path, [Localizable(false), NotNull] string key, double defaultValue)
        {
            Assert.ArgumentNotNull(path, nameof(path));
            Assert.ArgumentNotNull(key, nameof(key));

            return Applications.Storages.Storage.ReadDouble(path, key, defaultValue);
        }

        public virtual int GetInt([Localizable(false), NotNull] string path, [Localizable(false), NotNull] string key, int defaultValue)
        {
            Assert.ArgumentNotNull(path, nameof(path));
            Assert.ArgumentNotNull(key, nameof(key));

            return Applications.Storages.Storage.ReadInt(path, key, defaultValue);
        }

        [NotNull]
        public virtual BaseStorage GetStorage()
        {
            return Storage;
        }

        [NotNull]
        public virtual string GetString([Localizable(false), NotNull] string path, [Localizable(false), NotNull] string key, [Localizable(false), NotNull] string defaultValue)
        {
            Assert.ArgumentNotNull(path, nameof(path));
            Assert.ArgumentNotNull(key, nameof(key));
            Assert.ArgumentNotNull(defaultValue, nameof(defaultValue));

            return Applications.Storages.Storage.ReadString(path, key, defaultValue);
        }

        public virtual void Set([NotNull, Localizable(false)] string path, [NotNull, Localizable(false)] string key, [CanBeNull, Localizable(false)] object value)
        {
            Assert.ArgumentNotNull(path, nameof(path));
            Assert.ArgumentNotNull(key, nameof(key));

            Storage.Write(path, key, value);
        }

        public virtual void SetBool([Localizable(false), NotNull] string path, [Localizable(false), NotNull] string key, bool value)
        {
            Assert.ArgumentNotNull(path, nameof(path));
            Assert.ArgumentNotNull(key, nameof(key));

            Applications.Storages.Storage.WriteBool(path, key, value);
        }

        public virtual void SetDouble([Localizable(false), NotNull] string path, [Localizable(false), NotNull] string key, double value)
        {
            Assert.ArgumentNotNull(path, nameof(path));
            Assert.ArgumentNotNull(key, nameof(key));

            Storage.Write(path, key, value);
        }

        public virtual void SetInt([Localizable(false), NotNull] string path, [Localizable(false), NotNull] string key, int value)
        {
            Assert.ArgumentNotNull(path, nameof(path));
            Assert.ArgumentNotNull(key, nameof(key));

            Storage.Write(path, key, value);
        }

        public virtual void SetString([Localizable(false), NotNull] string path, [Localizable(false), NotNull] string key, [Localizable(false), NotNull] string value)
        {
            Assert.ArgumentNotNull(path, nameof(path));
            Assert.ArgumentNotNull(key, nameof(key));
            Assert.ArgumentNotNull(value, nameof(value));

            Storage.Write(path, key, value);
        }
    }
}
