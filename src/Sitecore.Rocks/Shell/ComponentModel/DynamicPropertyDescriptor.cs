// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.ComponentModel;
using System.Drawing.Design;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Diagnostics;

namespace Sitecore.Rocks.Shell.ComponentModel
{
    public class DynamicPropertyDescriptor : PropertyDescriptor
    {
        private readonly TypeConverter converter;

        public DynamicPropertyDescriptor([NotNull] DynamicProperty dynamicProperty, [NotNull] Attribute[] attrs, [CanBeNull] TypeConverter converter) : base(dynamicProperty.Name, attrs)
        {
            Assert.ArgumentNotNull(dynamicProperty, nameof(dynamicProperty));
            Assert.ArgumentNotNull(attrs, nameof(attrs));

            DynamicProperty = dynamicProperty;
            this.converter = converter;
        }

        [NotNull]
        public override Type ComponentType
        {
            get { return DynamicProperty.ComponentType; }
        }

        [CanBeNull]
        public override TypeConverter Converter
        {
            get { return converter; }
        }

        [NotNull]
        public DynamicProperty DynamicProperty { get; set; }

        public override bool IsReadOnly
        {
            get { return DynamicProperty.IsReadOnly; }
        }

        [NotNull]
        public override Type PropertyType
        {
            get { return DynamicProperty.Type; }
        }

        [CanBeNull]
        public UITypeEditor UITypeEditor { get; set; }

        public override bool CanResetValue([CanBeNull] object component)
        {
            return true;
        }

        public override object GetValue([CanBeNull] object component)
        {
            return DynamicProperty.Value;
        }

        public override void ResetValue(object component)
        {
            Assert.ArgumentNotNull(component, nameof(component));

            DynamicProperty.Value = null;
        }

        public override void SetValue([NotNull] object component, [CanBeNull] object value)
        {
            Assert.ArgumentNotNull(component, nameof(component));

            DynamicProperty.Value = value;

            OnValueChanged(component, EventArgs.Empty);
        }

        public override bool ShouldSerializeValue([CanBeNull] object component)
        {
            return false;
        }
    }
}
