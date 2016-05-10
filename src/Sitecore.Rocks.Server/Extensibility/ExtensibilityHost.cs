// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web.Routing;
using Sitecore.Diagnostics;
using Sitecore.Rocks.Server.Extensibility.Composition;

namespace Sitecore.Rocks.Server.Extensibility
{
    public class ExtensibilityHost
    {
        private readonly List<ExportDefinition> catalog = new List<ExportDefinition>();

        [NotNull]
        public IList<ExportDefinition> Catalog
        {
            get { return catalog; }
        }

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

            return catalog.Where(e => e.Attribute.ContractType == contractType).OrderBy(e => e.Attribute.Priority).ThenBy(e => e.Attribute.SortName).Select(e => e.GetInstance<T>(p));
        }

        [NotNull]
        public IEnumerable<T> GetExports<T>([NotNull] string contractName, [CanBeNull] object parameters = null) where T : class, new()
        {
            Assert.ArgumentNotNull(contractName, nameof(contractName));

            var p = parameters == null ? new RouteValueDictionary() : new RouteValueDictionary(parameters);

            return catalog.Where(e => e.Attribute.ContractName == contractName).OrderBy(e => e.Attribute.Priority).ThenBy(e => e.Attribute.SortName).Select(e => e.GetInstance<T>(p));
        }

        [NotNull]
        public IEnumerable<T> GetExports<T>([NotNull] Type contractType, [NotNull] string contractName, [CanBeNull] object parameters = null) where T : class, new()
        {
            Assert.ArgumentNotNull(contractType, nameof(contractType));
            Assert.ArgumentNotNull(contractName, nameof(contractName));

            var p = parameters == null ? new RouteValueDictionary() : new RouteValueDictionary(parameters);

            return catalog.Where(e => e.Attribute.ContractType == contractType && e.Attribute.ContractName == contractName).OrderBy(e => e.Attribute.Priority).ThenBy(e => e.Attribute.SortName).Select(e => e.GetInstance<T>(p));
        }

        public void RegisterExport([NotNull] ExportAttribute exportAttribute, [NotNull] Type type)
        {
            Assert.ArgumentNotNull(exportAttribute, nameof(exportAttribute));
            Assert.ArgumentNotNull(type, nameof(type));

            catalog.Add(new ExportDefinition(exportAttribute, type));
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

                    var list = CreateList(fieldInfo.FieldType);
                    fieldInfo.SetValue(attributedObject, list);

                    PopulateList(list, importManyAttribute, parameters);
                }

                customAttributes = fieldInfo.GetCustomAttributes(typeof(ImportAttribute), true);
                if (customAttributes.Length > 0)
                {
                    var importAttribute = customAttributes[0] as ImportAttribute;
                    if (importAttribute == null)
                    {
                        continue;
                    }

                    var values = catalog.Where(export => MatchesImport(export, importAttribute)).ToArray();
                    if (values.Length != 1)
                    {
                        throw new InvalidOperationException(string.Format("More than one export matches the import"));
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

                    var list = CreateList(propertyInfo.PropertyType);
                    propertyInfo.SetValue(attributedObject, list, null);

                    PopulateList(list, importManyAttribute, parameters);
                }

                customAttributes = propertyInfo.GetCustomAttributes(typeof(ImportAttribute), true);
                if (customAttributes.Length > 0)
                {
                    var importAttribute = customAttributes[0] as ImportAttribute;
                    if (importAttribute == null)
                    {
                        continue;
                    }

                    var values = catalog.Where(export => MatchesImport(export, importAttribute)).ToArray();
                    if (values.Length != 1)
                    {
                        throw new InvalidOperationException(string.Format("More than one export matches the import"));
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

        private bool MatchesImport([NotNull] ExportDefinition exportDefinition, [NotNull] ImportManyAttribute importManyAttribute)
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

            return true;
        }

        private void PopulateList([NotNull] IList list, [NotNull] ImportManyAttribute attribute, [NotNull] RouteValueDictionary parameters)
        {
            Debug.ArgumentNotNull(list, nameof(list));
            Debug.ArgumentNotNull(attribute, nameof(attribute));
            Debug.ArgumentNotNull(parameters, nameof(parameters));

            foreach (var item in catalog.Where(export => MatchesImport(export, attribute)).OrderBy(e => e.Attribute.Priority).ThenBy(e => e.Attribute.SortName).Select(e => e.GetInstance(parameters)))
            {
                list.Add(item);
            }
        }
    }
}
