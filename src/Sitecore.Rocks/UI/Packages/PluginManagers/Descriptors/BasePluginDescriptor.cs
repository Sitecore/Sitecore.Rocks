// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.ComponentModel;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Diagnostics;

namespace Sitecore.Rocks.UI.Packages.PluginManagers.Descriptors
{
    public abstract class BasePluginDescriptor : INotifyPropertyChanged
    {
        protected BasePluginDescriptor()
        {
            Author = string.Empty;
            Copyright = string.Empty;
            Description = string.Empty;
            DownloadCount = "-1";
            IconUrl = null;
            LicenseUrl = null;
            ReleaseNotes = string.Empty;
            RequireLicenseAcceptance = false;
            Summary = string.Empty;
            Tags = string.Empty;
            Title = string.Empty;
            Version = string.Empty;
            Location = string.Empty;
        }

        [NotNull]
        public string Author { get; protected set; }

        [NotNull]
        public string Copyright { get; protected set; }

        [NotNull]
        public string Description { get; protected set; }

        [NotNull]
        public string DownloadCount { get; protected set; }

        [CanBeNull]
        public Uri IconUrl { get; protected set; }

        [CanBeNull]
        public Uri LicenseUrl { get; protected set; }

        [NotNull]
        public string Location { get; protected set; }

        [NotNull]
        public string ReleaseNotes { get; protected set; }

        public bool RequireLicenseAcceptance { get; protected set; }

        [NotNull]
        public string Summary { get; protected set; }

        [NotNull]
        public string Tags { get; protected set; }

        [NotNull]
        public string Title { get; protected set; }

        [NotNull]
        public string Version { get; protected set; }

        public abstract void Install([NotNull] Action completed);

        public event PropertyChangedEventHandler PropertyChanged;

        public abstract void Uninstall([NotNull] Action completed);

        public abstract void Update([NotNull] Action completed);

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([NotNull] string propertyName)
        {
            Debug.ArgumentNotNull(propertyName, nameof(propertyName));

            var handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}
