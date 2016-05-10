// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Diagnostics;

namespace Sitecore.Rocks.Shell.ComponentModel
{
    public class DynamicProperty : INotifyPropertyChanged
    {
        private Dictionary<string, object> attributes;

        private object value;

        public DynamicProperty([NotNull] Type componentType, [Localizable(false), NotNull] string name, [NotNull] string displayName, [NotNull] Type type, [NotNull] string typeName, [NotNull] string category, [NotNull] string description, [NotNull] string editor, [CanBeNull, Localizable(false)] object value, [CanBeNull] object tag)
        {
            Assert.ArgumentNotNull(componentType, nameof(componentType));
            Assert.ArgumentNotNull(name, nameof(name));
            Assert.ArgumentNotNull(displayName, nameof(displayName));
            Assert.ArgumentNotNull(type, nameof(type));
            Assert.ArgumentNotNull(typeName, nameof(typeName));
            Assert.ArgumentNotNull(category, nameof(category));
            Assert.ArgumentNotNull(description, nameof(description));

            ComponentType = componentType;
            Name = name;
            DisplayName = displayName;
            Type = type;
            TypeName = typeName;
            Category = category;
            Description = description;
            Editor = editor;
            Value = value;
            Tag = tag;
        }

        [NotNull]
        public Dictionary<string, object> Attributes
        {
            get
            {
                if (attributes == null)
                {
                    attributes = new Dictionary<string, object>();
                }

                return attributes;
            }
        }

        [NotNull]
        public string Category { get; private set; }

        [NotNull]
        public Type ComponentType { get; private set; }

        [CanBeNull]
        public TypeConverter Converter { get; set; }

        [NotNull]
        public string Description { get; private set; }

        [NotNull]
        public string DisplayName { get; private set; }

        [NotNull]
        public string Editor { get; private set; }

        public bool IsHidden { get; set; }

        public bool IsReadOnly { get; set; }

        [NotNull]
        public string Name { get; private set; }

        [CanBeNull]
        public object Tag { get; set; }

        [NotNull]
        public Type Type { get; private set; }

        [NotNull]
        public string TypeName { get; set; }

        [CanBeNull]
        public object Value
        {
            get { return value; }

            set
            {
                if (this.value == value)
                {
                    return;
                }

                this.value = value;
                RaiseModified();

                OnPropertyChanged(nameof(Value));
            }
        }

        public event EventHandler Modified;

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

        protected void RaiseModified()
        {
            var modified = Modified;
            if (modified != null)
            {
                modified(this, EventArgs.Empty);
            }
        }
    }
}
