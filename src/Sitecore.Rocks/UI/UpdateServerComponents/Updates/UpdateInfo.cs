// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.ComponentModel;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Diagnostics;

namespace Sitecore.Rocks.UI.UpdateServerComponents.Updates
{
    public class UpdateInfo : INotifyPropertyChanged
    {
        private bool isChecked;

        public string Action { get; set; }

        public bool IsChecked
        {
            get { return isChecked; }

            set
            {
                if (value.Equals(isChecked))
                {
                    return;
                }

                isChecked = value;
                OnPropertyChanged("IsChecked");
            }
        }

        public string LocalVersion { get; set; }

        public string Name { get; set; }

        public InstalledPluginInfo Plugin { get; set; }

        [NotNull]
        public string RuntimeVersion { get; set; }

        [CanBeNull]
        public ServerComponentInfo ServerComponent { get; set; }

        public string ServerVersion { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

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
