// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.ContentEditors.Fields;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Extensibility;

namespace Sitecore.Rocks.ContentEditors
{
    [ExtensibilityInitialization(PreInit = "Clear")]
    public static class FieldTypeManager
    {
        private static readonly string fieldControlInterface = typeof(IFieldControl).FullName;

        private static readonly Dictionary<string, List<IFieldControl>> fieldControls = new Dictionary<string, List<IFieldControl>>();

        private static readonly Dictionary<string, ConstructorInfo> types = new Dictionary<string, ConstructorInfo>();

        [NotNull]
        public static Dictionary<string, ConstructorInfo> Types
        {
            get { return types; }
        }

        public static void Add([NotNull] string typeName, [NotNull] Type type)
        {
            Assert.ArgumentNotNull(typeName, nameof(typeName));
            Assert.ArgumentNotNull(type, nameof(type));

            var constructorInfo = type.GetConstructor(Type.EmptyTypes);

            Types[typeName.ToLowerInvariant()] = constructorInfo;
        }

        [UsedImplicitly]
        public static void Clear()
        {
            Types.Clear();
        }

        [NotNull]
        public static IFieldControl GetInstance([NotNull] string fieldType)
        {
            Assert.ArgumentNotNull(fieldType, nameof(fieldType));

            fieldType = fieldType.ToLowerInvariant();

            var result = Get(fieldType);
            if (result != null)
            {
                return result;
            }

            ConstructorInfo constructorInfo;
            if (!types.TryGetValue(fieldType, out constructorInfo))
            {
                return new TextField
                {
                    TextWrapping = TextWrapping.Wrap
                };
            }

            IFieldControl control;
            try
            {
                control = constructorInfo.Invoke(null) as IFieldControl;
            }
            catch (Exception ex)
            {
                control = null;

                var sb = new StringBuilder();

                BuildException(sb, ex);

                AppHost.MessageBox(string.Format("Failed to create field type: {0}\n\n{1}", fieldType, sb), "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            return control ?? new TextField
            {
                TextWrapping = TextWrapping.Wrap
            };
        }

        public static void LoadType([NotNull] Type type, [NotNull] FieldControlAttribute attribute)
        {
            Assert.ArgumentNotNull(type, nameof(type));
            Assert.ArgumentNotNull(attribute, nameof(attribute));

            var i = type.GetInterface(fieldControlInterface);
            if (i != null)
            {
                Add(attribute.TypeName, type);
            }
        }

        public static void Reuse([NotNull] string fieldType, [NotNull] IFieldControl control)
        {
            Assert.ArgumentNotNull(fieldType, nameof(fieldType));
            Assert.ArgumentNotNull(control, nameof(control));

            List<IFieldControl> controls;
            if (!fieldControls.TryGetValue(fieldType, out controls))
            {
                controls = new List<IFieldControl>();
                fieldControls[fieldType] = controls;
            }

            controls.Add(control);

            control.GetControl().IsEnabled = true;
        }

        private static void BuildException([NotNull] StringBuilder sb, [NotNull] Exception exception)
        {
            Debug.ArgumentNotNull(sb, nameof(sb));
            Debug.ArgumentNotNull(exception, nameof(exception));

            if (exception.InnerException != null)
            {
                BuildException(sb, exception.InnerException);
            }

            sb.Append(@"-------------------\n");
            sb.Append(exception.Message);
            sb.Append(@"\n\n");
            sb.Append(exception.StackTrace);
            sb.Append(@"\n\n");
        }

        [CanBeNull]
        private static IFieldControl Get([NotNull] string typeName)
        {
            Debug.ArgumentNotNull(typeName, nameof(typeName));

            List<IFieldControl> controls;
            if (!fieldControls.TryGetValue(typeName, out controls))
            {
                return null;
            }

            var result = controls.FirstOrDefault();
            if (result == null)
            {
                return null;
            }

            controls.Remove(result);

            return result;
        }
    }
}
