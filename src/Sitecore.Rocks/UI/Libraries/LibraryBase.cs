// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Collections.ObjectModel;
using System.ComponentModel;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Diagnostics;

namespace Sitecore.Rocks.UI.Libraries
{
    public abstract class LibraryBase : INotifyPropertyChanged
    {
        private bool isExpanded;

        private string name;

        protected LibraryBase([NotNull] string fileName, [NotNull] string name)
        {
            Debug.ArgumentNotNull(fileName, nameof(fileName));
            Debug.ArgumentNotNull(name, nameof(name));

            FileName = fileName;
            Name = name;
            IsExpanded = AppHost.Settings.GetBool("Folder\\Expanders", Name, false);
            Icon = Icon.Empty;
        }

        [NotNull]
        public string FileName { get; private set; }

        public Icon Icon { get; protected set; }

        public bool IsExpanded
        {
            get { return isExpanded; }

            set
            {
                if (isExpanded == value)
                {
                    return;
                }

                isExpanded = value;
                OnPropertyChanged("IsExpanded");
            }
        }

        [NotNull]
        public abstract ObservableCollection<IItem> Items { get; }

        [NotNull]
        public string Name
        {
            get { return name; }

            set
            {
                Assert.ArgumentNotNull(value, nameof(value));

                if (name == value)
                {
                    return;
                }

                name = value;
                OnPropertyChanged("Name");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public abstract void Save();

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
