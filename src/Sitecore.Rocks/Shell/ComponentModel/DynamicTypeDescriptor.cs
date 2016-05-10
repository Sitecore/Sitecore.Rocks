// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing.Design;
using System.Linq;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Extensions.StringExtensions;
using Sitecore.Rocks.UI.LayoutDesigners.Editors;
using Sitecore.Rocks.UI.LayoutDesigners.Properties;

namespace Sitecore.Rocks.Shell.ComponentModel
{
    public class DynamicTypeDescriptor : ICustomTypeDescriptor
    {
        [NotNull]
        private readonly List<DynamicProperty> dynamicProperties = new List<DynamicProperty>();

        private PropertyDescriptorCollection propertyDescriptors;

        [NotNull, Browsable(false)]
        public ICollection<DynamicProperty> DynamicProperties
        {
            get { return dynamicProperties; }
        }

        protected void ReloadPropertyDescriptors()
        {
            propertyDescriptors = null;
        }

        [NotNull]
        AttributeCollection ICustomTypeDescriptor.GetAttributes()
        {
            return new AttributeCollection(null);
        }

        [CanBeNull]
        string ICustomTypeDescriptor.GetClassName()
        {
            return null;
        }

        [CanBeNull]
        string ICustomTypeDescriptor.GetComponentName()
        {
            return null;
        }

        [CanBeNull]
        TypeConverter ICustomTypeDescriptor.GetConverter()
        {
            return null;
        }

        [CanBeNull]
        EventDescriptor ICustomTypeDescriptor.GetDefaultEvent()
        {
            return null;
        }

        [CanBeNull]
        PropertyDescriptor ICustomTypeDescriptor.GetDefaultProperty()
        {
            return null;
        }

        [CanBeNull]
        object ICustomTypeDescriptor.GetEditor([CanBeNull] Type editorBaseType)
        {
            return null;
        }

        [NotNull]
        EventDescriptorCollection ICustomTypeDescriptor.GetEvents()
        {
            return new EventDescriptorCollection(null);
        }

        [NotNull]
        EventDescriptorCollection ICustomTypeDescriptor.GetEvents([CanBeNull] Attribute[] attributes)
        {
            return new EventDescriptorCollection(null);
        }

        [NotNull]
        PropertyDescriptorCollection ICustomTypeDescriptor.GetProperties()
        {
            return ((ICustomTypeDescriptor)this).GetProperties(null);
        }

        [NotNull]
        PropertyDescriptorCollection ICustomTypeDescriptor.GetProperties([CanBeNull] Attribute[] attributes)
        {
            if (propertyDescriptors == null)
            {
                LoadPropertyDescriptors();
            }

            return propertyDescriptors;
        }

        [NotNull]
        object ICustomTypeDescriptor.GetPropertyOwner([CanBeNull] PropertyDescriptor pd)
        {
            return this;
        }

        private void LoadPropertyDescriptors()
        {
            propertyDescriptors = new PropertyDescriptorCollection(null);

            foreach (var property in TypeDescriptor.GetProperties(this, true).OfType<PropertyDescriptor>())
            {
                propertyDescriptors.Add(property);
            }

            foreach (var property in dynamicProperties)
            {
                var attrs = new List<Attribute>
                {
                    new CategoryAttribute(property.Category)
                };

                if (!string.IsNullOrEmpty(property.Description))
                {
                    attrs.Add(new DescriptionAttribute(property.Description));
                }

                if (!string.IsNullOrEmpty(property.DisplayName))
                {
                    attrs.Add(new DisplayNameAttribute(property.DisplayName));
                }

                if (property.IsHidden)
                {
                    attrs.Add(new BrowsableAttribute(false));
                }

                switch (property.TypeName.ToLowerInvariant())
                {
                    case "server file":
                        attrs.Add(new EditorAttribute(typeof(FilePathEditor), typeof(UITypeEditor)));
                        break;
                    case "internal link":
                    case "droptree":
                        attrs.Add(new EditorAttribute(typeof(ItemIdEditor), typeof(UITypeEditor)));
                        break;
                    case "templateid":
                        attrs.Add(new EditorAttribute(typeof(TemplateIdEditor), typeof(UITypeEditor)));
                        break;
                }

                UITypeEditor uiTypeEditor = null;

                if (!string.IsNullOrEmpty(property.Editor))
                {
                    var editor = property.Editor;
                    var assemblyName = string.Empty;

                    var n = editor.IndexOf(',');
                    if (n >= 0)
                    {
                        assemblyName = editor.Mid(n + 1).Trim();
                        editor = editor.Left(n).Trim();
                    }

                    Type editorType = null;
                    if (!string.IsNullOrEmpty(assemblyName))
                    {
                        assemblyName += ",";
                        var assembly = AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(a => a.FullName.StartsWith(assemblyName));
                        if (assembly != null)
                        {
                            editorType = assembly.GetType(editor);
                        }
                    }
                    else
                    {
                        editorType = Type.GetType(editor);
                    }

                    if (editorType != null)
                    {
                        attrs.Add(new EditorAttribute(typeof(EditorWrapper), typeof(UITypeEditor)));
                        uiTypeEditor = Activator.CreateInstance(editorType) as UITypeEditor;
                    }
                    else
                    {
                        AppHost.Output.Log("Editor for field not found: " + property.Editor);
                    }
                }

                var descriptor = new DynamicPropertyDescriptor(property, attrs.ToArray(), property.Converter);

                descriptor.UITypeEditor = uiTypeEditor;

                propertyDescriptors.Add(descriptor);
            }
        }
    }
}
