// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.Collections.Generic;
using Sitecore.Diagnostics;
using Sitecore.Rocks.Server.Extensibility;

namespace Sitecore.Rocks.Server.Validations
{
    [ExtensibilityInitialization(PreInit = "Clear")]
    public static class ValidationManager
    {
        private static readonly string ItemValidationInterfaceName;

        private static readonly List<ItemValidationDescriptor> itemValidations = new List<ItemValidationDescriptor>();

        private static readonly string RenderingValidationInterfaceName;

        private static readonly List<RenderingValidationDescriptor> renderingValidations = new List<RenderingValidationDescriptor>();

        private static readonly List<ValidationDescriptor> validations = new List<ValidationDescriptor>();

        static ValidationManager()
        {
            ItemValidationInterfaceName = typeof(IItemValidation).FullName;
            RenderingValidationInterfaceName = typeof(IRenderingValidation).FullName;
        }

        [NotNull]
        public static IEnumerable<ItemValidationDescriptor> ItemValidations
        {
            get { return itemValidations; }
        }

        [NotNull]
        public static IEnumerable<RenderingValidationDescriptor> RenderingValidations
        {
            get { return renderingValidations; }
        }

        [NotNull]
        public static IEnumerable<ValidationDescriptor> Validations
        {
            get { return validations; }
        }

        public static void Clear()
        {
            validations.Clear();
        }

        public static void LoadType([NotNull] Type type, [NotNull] ValidationAttribute validationAttribute)
        {
            Assert.ArgumentNotNull(type, nameof(type));
            Assert.ArgumentNotNull(validationAttribute, nameof(validationAttribute));

            var i = type.GetInterface(ItemValidationInterfaceName);
            if (i != null)
            {
                var constructor = type.GetConstructor(Type.EmptyTypes);
                if (constructor == null)
                {
                    return;
                }

                var instance = constructor.Invoke(null) as IItemValidation;
                if (instance == null)
                {
                    return;
                }

                var itemValidationDescriptor = new ItemValidationDescriptor(validationAttribute, instance);
                itemValidations.Add(itemValidationDescriptor);
                return;
            }

            i = type.GetInterface(RenderingValidationInterfaceName);
            if (i != null)
            {
                var constructor = type.GetConstructor(Type.EmptyTypes);
                if (constructor == null)
                {
                    return;
                }

                var instance = constructor.Invoke(null) as IRenderingValidation;
                if (instance == null)
                {
                    return;
                }

                var renderingValidationDescriptor = new RenderingValidationDescriptor(validationAttribute, instance);
                renderingValidations.Add(renderingValidationDescriptor);
                return;
            }

            var validationDescriptor = new ValidationDescriptor(validationAttribute, type);
            validations.Add(validationDescriptor);
        }

        public class ItemValidationDescriptor
        {
            public ItemValidationDescriptor([NotNull] ValidationAttribute attribute, [NotNull] IItemValidation instance)
            {
                Assert.ArgumentNotNull(attribute, nameof(attribute));
                Assert.ArgumentNotNull(instance, nameof(instance));

                Attribute = attribute;
                Instance = instance;
            }

            [NotNull]
            public ValidationAttribute Attribute { get; private set; }

            [NotNull]
            public IItemValidation Instance { get; private set; }
        }

        public class RenderingValidationDescriptor
        {
            public RenderingValidationDescriptor([NotNull] ValidationAttribute attribute, [NotNull] IRenderingValidation instance)
            {
                Assert.ArgumentNotNull(attribute, nameof(attribute));
                Assert.ArgumentNotNull(instance, nameof(instance));

                Attribute = attribute;
                Instance = instance;
            }

            [NotNull]
            public ValidationAttribute Attribute { get; private set; }

            [NotNull]
            public IRenderingValidation Instance { get; private set; }
        }

        public class ValidationDescriptor
        {
            public ValidationDescriptor([NotNull] ValidationAttribute attribute, [NotNull] Type type)
            {
                Assert.ArgumentNotNull(attribute, nameof(attribute));
                Assert.ArgumentNotNull(type, nameof(type));

                Attribute = attribute;
                Type = type;
            }

            [NotNull]
            public ValidationAttribute Attribute { get; private set; }

            [NotNull]
            public Type Type { get; }

            [CanBeNull]
            public IValidation GetInstance()
            {
                var constructor = Type.GetConstructor(Type.EmptyTypes);
                if (constructor == null)
                {
                    return null;
                }

                return constructor.Invoke(null) as IValidation;
            }
        }
    }
}
