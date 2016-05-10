// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Web.Routing;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Extensibility.Composition;
using Sitecore.Rocks.Shell.Pipelines.Initialization;
using Sitecore.Rocks.Shell.Pipelines.Uninitialization;

namespace Sitecore.Rocks.Extensibility
{
    public class ExtensibilityHost
    {
        public delegate void ResetEventHandler();

        private readonly List<ExportDefinition> _catalog = new List<ExportDefinition>();

        [NotNull]
        public IList<ExportDefinition> Catalog => _catalog;

        [NotNull]
        public string PluginFolder => Path.Combine(AppHost.User.UserFolder, @"Plugins");

        public void ComposeParts([NotNull] object attributedObject, [CanBeNull] object parameters = null)
        {
            Assert.ArgumentNotNull(attributedObject, nameof(attributedObject));

            var p = parameters == null ? new RouteValueDictionary() : new RouteValueDictionary(parameters);

            ComposeProperties(attributedObject, p);
            ComposeFields(attributedObject, p);
        }

        [NotNull]
        public IEnumerable<T> GetExports<T>([NotNull] Type contractType, [CanBeNull] object parameters = null) where T : class, new()
        {
            Assert.ArgumentNotNull(contractType, nameof(contractType));

            var p = parameters == null ? new RouteValueDictionary() : new RouteValueDictionary(parameters);

            return _catalog.Where(e => e.Attribute.ContractType == contractType).OrderBy(e => e.Attribute.Priority).ThenBy(e => e.Attribute.SortName).Select(e => e.GetInstance<T>(p));
        }

        [NotNull]
        public IEnumerable<T> GetExports<T>([NotNull] string contractName, [CanBeNull] object parameters = null) where T : class, new()
        {
            Assert.ArgumentNotNull(contractName, nameof(contractName));

            var p = parameters == null ? new RouteValueDictionary() : new RouteValueDictionary(parameters);

            return _catalog.Where(e => e.Attribute.ContractName == contractName).OrderBy(e => e.Attribute.Priority).ThenBy(e => e.Attribute.SortName).Select(e => e.GetInstance<T>(p));
        }

        [NotNull]
        public IEnumerable<T> GetExports<T>([NotNull] Type contractType, [NotNull] string contractName, [CanBeNull] object parameters = null) where T : class, new()
        {
            Assert.ArgumentNotNull(contractType, nameof(contractType));
            Assert.ArgumentNotNull(contractName, nameof(contractName));

            var p = parameters == null ? new RouteValueDictionary() : new RouteValueDictionary(parameters);

            return _catalog.Where(e => e.Attribute.ContractType == contractType && e.Attribute.ContractName == contractName).OrderBy(e => e.Attribute.Priority).ThenBy(e => e.Attribute.SortName).Select(e => e.GetInstance<T>(p));
        }

        public void RegisterExport([NotNull] ExportAttribute exportAttribute, [NotNull] Type type)
        {
            Assert.ArgumentNotNull(exportAttribute, nameof(exportAttribute));
            Assert.ArgumentNotNull(type, nameof(type));

            _catalog.Add(new ExportDefinition(exportAttribute, type));
        }

        public void Reinitialize()
        {
            UninitializationPipeline.Run().WithParameters();
            OnReset();

            ExtensibilityLoader.Initialize();
            InitializationPipeline.Run().WithParameters(false);
        }

        public event ResetEventHandler Reset;

        protected virtual void OnReset()
        {
            var handler = Reset;

            if (handler != null)
            {
                handler();
            }
        }

        private void ComposeFields([NotNull] object attributedObject, [NotNull] RouteValueDictionary parameters)
        {
            Debug.ArgumentNotNull(attributedObject, nameof(attributedObject));
            Debug.ArgumentNotNull(parameters, nameof(parameters));

            foreach (var fieldInfo in attributedObject.GetType().GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public))
            {
                var customAttributes = fieldInfo.GetCustomAttributes(typeof(ImportManyAttribute), true);
                if (customAttributes.Length > 0)
                {
                    var importManyAttribute = customAttributes[0] as ImportManyAttribute;
                    if (importManyAttribute == null)
                    {
                        continue;
                    }

                    var genericTypes = fieldInfo.FieldType.IsGenericType ? fieldInfo.FieldType.GetGenericArguments() : null;

                    var list = fieldInfo.GetValue(attributedObject) as IList;
                    if (list == null)
                    {
                        list = CreateList(fieldInfo.FieldType);
                        fieldInfo.SetValue(attributedObject, list);
                    }
                    else
                    {
                        list.Clear();
                    }

                    PopulateList(list, importManyAttribute, parameters, genericTypes);
                }

                customAttributes = fieldInfo.GetCustomAttributes(typeof(ImportAttribute), true);
                if (customAttributes.Length > 0)
                {
                    var importAttribute = customAttributes[0] as ImportAttribute;
                    if (importAttribute == null)
                    {
                        continue;
                    }

                    var values = _catalog.Where(export => MatchesImport(export, importAttribute)).ToArray();
                    if (values.Length != 1)
                    {
                        throw new InvalidOperationException("More than one export matches the import");
                    }

                    var value = values[0].GetInstance(parameters);
                    fieldInfo.SetValue(attributedObject, value);
                }
            }
        }

        private void ComposeProperties([NotNull] object attributedObject, [NotNull] RouteValueDictionary parameters)
        {
            Debug.ArgumentNotNull(attributedObject, nameof(attributedObject));
            Debug.ArgumentNotNull(parameters, nameof(parameters));

            foreach (var propertyInfo in attributedObject.GetType().GetProperties(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public))
            {
                var customAttributes = propertyInfo.GetCustomAttributes(typeof(ImportManyAttribute), true);
                if (customAttributes.Length > 0)
                {
                    var importManyAttribute = customAttributes[0] as ImportManyAttribute;
                    if (importManyAttribute == null)
                    {
                        continue;
                    }

                    var genericTypes = propertyInfo.PropertyType.IsGenericType ? propertyInfo.PropertyType.GetGenericArguments() : null;

                    var list = propertyInfo.GetValue(attributedObject, null) as IList;
                    if (list == null)
                    {
                        list = CreateList(propertyInfo.PropertyType);
                        propertyInfo.SetValue(attributedObject, list, null);
                    }
                    else
                    {
                        list.Clear();
                    }

                    PopulateList(list, importManyAttribute, parameters, genericTypes);
                }

                customAttributes = propertyInfo.GetCustomAttributes(typeof(ImportAttribute), true);
                if (customAttributes.Length > 0)
                {
                    var importAttribute = customAttributes[0] as ImportAttribute;
                    if (importAttribute == null)
                    {
                        continue;
                    }

                    var values = _catalog.Where(export => MatchesImport(export, importAttribute)).ToArray();
                    if (values.Length != 1)
                    {
                        throw new InvalidOperationException("More than one export matches the import");
                    }

                    var value = values[0].GetInstance(parameters);
                    propertyInfo.SetValue(attributedObject, value, null);
                }
            }
        }

        [NotNull]
        private IList CreateList([NotNull] Type memberType)
        {
            Debug.ArgumentNotNull(memberType, nameof(memberType));

            var listType = memberType.IsGenericType ? typeof(List<>).MakeGenericType(memberType.GetGenericArguments()) : memberType;

            return (IList)Activator.CreateInstance(listType);
        }

        private bool MatchesImport([NotNull] ExportDefinition exportDefinition, [NotNull] ImportAttribute importAttribute)
        {
            Debug.ArgumentNotNull(exportDefinition, nameof(exportDefinition));
            Debug.ArgumentNotNull(importAttribute, nameof(importAttribute));

            if (importAttribute.ContractType != null && importAttribute.ContractType != exportDefinition.Attribute.ContractType)
            {
                return false;
            }

            if (importAttribute.ContractName != null && importAttribute.ContractName != exportDefinition.Attribute.ContractName)
            {
                return false;
            }

            return true;
        }

        private bool MatchesImport([NotNull] ExportDefinition exportDefinition, [NotNull] ImportManyAttribute importManyAttribute, [CanBeNull] Type[] genericTypes)
        {
            Debug.ArgumentNotNull(exportDefinition, nameof(exportDefinition));
            Debug.ArgumentNotNull(importManyAttribute, nameof(importManyAttribute));

            if (importManyAttribute.ContractType != null && importManyAttribute.ContractType != exportDefinition.Attribute.ContractType)
            {
                return false;
            }

            if (importManyAttribute.ContractName != null && importManyAttribute.ContractName != exportDefinition.Attribute.ContractName)
            {
                return false;
            }

            if (genericTypes != null)
            {
                return genericTypes.Contains(exportDefinition.Attribute.ContractType);
            }

            return true;
        }

        private void PopulateList([NotNull] IList list, [NotNull] ImportManyAttribute attribute, [NotNull] RouteValueDictionary parameters, [CanBeNull] Type[] genericTypes)
        {
            Debug.ArgumentNotNull(list, nameof(list));
            Debug.ArgumentNotNull(attribute, nameof(attribute));
            Debug.ArgumentNotNull(parameters, nameof(parameters));

            foreach (var item in _catalog.Where(export => MatchesImport(export, attribute, genericTypes)).OrderBy(e => e.Attribute.Priority).ThenBy(e => e.Attribute.SortName).Select(e => e.GetInstance(parameters)))
            {
                list.Add(item);
            }
        }
    }
}
