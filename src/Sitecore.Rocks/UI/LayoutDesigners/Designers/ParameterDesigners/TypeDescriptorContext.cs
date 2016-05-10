// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.ComponentModel;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Diagnostics;

namespace Sitecore.Rocks.UI.LayoutDesigners.Designers.ParameterDesigners
{
    public class TypeDescriptorContext : ITypeDescriptorContext
    {
        public TypeDescriptorContext([NotNull] PropertyDescriptor propertyDescriptor)
        {
            Assert.ArgumentNotNull(propertyDescriptor, nameof(propertyDescriptor));

            PropertyDescriptor = propertyDescriptor;
        }

        public TypeDescriptorContext([NotNull] PropertyDescriptor propertyDescriptor, [NotNull] object instance)
        {
            Assert.ArgumentNotNull(propertyDescriptor, nameof(propertyDescriptor));
            Assert.ArgumentNotNull(instance, nameof(instance));

            Instance = instance;
            PropertyDescriptor = propertyDescriptor;
        }

        [CanBeNull]
        public IContainer Container => null;

        [CanBeNull]
        public object Instance { get; set; }

        public PropertyDescriptor PropertyDescriptor { get; }

        [CanBeNull]
        public object GetService([NotNull] Type serviceType)
        {
            Assert.ArgumentNotNull(serviceType, nameof(serviceType));

            return null;
        }

        public void OnComponentChanged()
        {
        }

        public bool OnComponentChanging()
        {
            return false;
        }
    }
}
