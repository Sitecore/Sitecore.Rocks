// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Collections;
using System.ComponentModel;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Diagnostics;

namespace Sitecore.Rocks.UI.Libraries
{
    public class LibraryItemDescriptor : IItem, ICanDeleteWithText, INotifyPropertyChanged
    {
        private readonly IList list;

        private string name;

        public LibraryItemDescriptor([NotNull] IList list, [NotNull] ItemUri itemUri, [NotNull] string name, [NotNull] Icon icon)
        {
            Assert.ArgumentNotNull(list, nameof(list));
            Assert.ArgumentNotNull(itemUri, nameof(itemUri));
            Assert.ArgumentNotNull(name, nameof(name));
            Assert.ArgumentNotNull(icon, nameof(icon));

            this.list = list;
            ItemUri = itemUri;
            Icon = icon;
            Name = name;
        }

        public string CommandText
        {
            get { return "Remove from List"; }
        }

        public Icon Icon { get; }

        public ItemUri ItemUri { get; }

        public string MultipleText
        {
            get { return "Are you sure you want to remove these '{0}' items from this list?"; }
        }

        public string Name
        {
            get { return name; }

            set
            {
                Assert.ArgumentNotNull(value, nameof(value));

                name = value;
                OnPropertyChanged("Name");
            }
        }

        public string SingleText
        {
            get { return "Are you sure you want to remove '{0}' from this list?"; }
        }

        [NotNull]
        string ICanDelete.Text
        {
            get { return Name; }
        }

        public void Delete(bool deleteFiles)
        {
            list.Remove(this);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([NotNull] string name)
        {
            Debug.ArgumentNotNull(name, nameof(name));

            var handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(name));
            }
        }
    }
}
