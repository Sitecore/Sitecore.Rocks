// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.ComponentModel;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Diagnostics;

namespace Sitecore.Rocks.UI.LayoutDesigners.Properties.DataBindings
{
    [TypeConverter(typeof(DataBindingConverter))]
    public class DataBinding : INotifyPropertyChanged
    {
        private readonly string binding;

        public DataBinding([CanBeNull] string binding)
        {
            this.binding = binding;
        }

        [CanBeNull]
        public string Binding => binding;

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }

            if (ReferenceEquals(this, obj))
            {
                return true;
            }

            if (obj.GetType() != GetType())
            {
                return false;
            }

            return Equals((DataBinding)obj);
        }

        public override int GetHashCode()
        {
            return binding != null ? binding.GetHashCode() : 0;
        }

        public static bool operator ==([CanBeNull] DataBinding left, [CanBeNull] DataBinding right)
        {
            return Equals(left, right);
        }

        public static bool operator !=([CanBeNull] DataBinding left, [CanBeNull] DataBinding right)
        {
            return !Equals(left, right);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public override string ToString()
        {
            return binding ?? string.Empty;
        }

        protected bool Equals(DataBinding other)
        {
            return string.Equals(binding, other.binding);
        }

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
