// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using Sitecore.Configuration;
using Sitecore.Data.Items;
using Sitecore.Diagnostics;
using Sitecore.Extensions.StringExtensions;
using Sitecore.Globalization;
using Sitecore.IO;
using Sitecore.Rocks.Server.Extensions.XElementExtensions;
using Sitecore.Rocks.Server.Pipelines.GetFieldValue;
using Sitecore.SecurityModel;

namespace Sitecore.Rocks.Server.Validations
{
    public class ValidationAnalyzer
    {
        public void Process([NotNull] XmlTextWriter output, [NotNull] ValidationAnalyzerOptions validationAnalyzerOptions)
        {
            Assert.ArgumentNotNull(output, nameof(output));
            Assert.ArgumentNotNull(validationAnalyzerOptions, nameof(validationAnalyzerOptions));

            output.WriteStartElement("validations");
            output.WriteAttributeString("timestamp", DateUtil.ToIsoDate(DateTime.UtcNow));

            using (new SecurityDisabler())
            {
                if (validationAnalyzerOptions.ProcessValidations)
                {
                    ProcessValidations(output, validationAnalyzerOptions);
                }

                if (validationAnalyzerOptions.ProcessItems)
                {
                    ProcessItems(output, validationAnalyzerOptions);
                }

                if (validationAnalyzerOptions.ProcessCustomValidations)
                {
                    ProcessCustomValidations(output, validationAnalyzerOptions);
                }
            }

            output.WriteEndElement();
        }

        private void ProcessCustomValidations([NotNull] XmlTextWriter output, [NotNull] ValidationAnalyzerOptions options)
        {
            Debug.ArgumentNotNull(output, nameof(output));
            Debug.ArgumentNotNull(options, nameof(options));

            if (string.IsNullOrEmpty(options.CustomValidations))
            {
                return;
            }

            XDocument doc;
            try
            {
                doc = XDocument.Parse(options.CustomValidations);
            }
            catch (Exception ex)
            {
                Log.Error("Could not parse validations: " + ex.Message, GetType());
                return;
            }

            var root = doc.Root;
            if (root == null)
            {
                return;
            }

            var webConfig = new XmlDocument();
            webConfig.Load(FileUtil.MapPath("/web.config"));
            var expandedWebConfig = Factory.GetConfiguration();

            var webFolder = FileUtil.MapPath("/");
            if (webFolder.EndsWith("\\"))
            {
                webFolder = webFolder.Left(webFolder.Length - 1);
            }

            var webFileSystem = new FileSystemNavigator(webFolder);
            webFileSystem.MoveToFirstChild();

            var dataFolder = FileUtil.MapPath(Settings.DataFolder);
            if (dataFolder.EndsWith("\\"))
            {
                dataFolder = dataFolder.Left(dataFolder.Length - 1);
            }

            var dataFileSystem = new FileSystemNavigator(dataFolder);
            dataFileSystem.MoveToFirstChild();

            var writer = new ValidationWriter();

            foreach (var element in root.Elements())
            {
                webFileSystem.MoveToRoot();
                dataFileSystem.MoveToRoot();
                ProcessCustomValidations(output, writer, element, options, webConfig, expandedWebConfig, webFileSystem, dataFileSystem);
            }
        }

        private void ProcessCustomValidations([NotNull] XmlTextWriter output, [NotNull] ValidationWriter writer, [NotNull] XElement element, [NotNull] ValidationAnalyzerOptions options, [NotNull] XmlDocument webConfig, [NotNull] XmlDocument expandedWebConfig, [NotNull] FileSystemNavigator webFileSystem, [NotNull] FileSystemNavigator dataFileSystem)
        {
            Debug.ArgumentNotNull(output, nameof(output));
            Debug.ArgumentNotNull(writer, nameof(writer));
            Debug.ArgumentNotNull(element, nameof(element));
            Debug.ArgumentNotNull(options, nameof(options));
            Debug.ArgumentNotNull(webConfig, nameof(webConfig));
            Debug.ArgumentNotNull(expandedWebConfig, nameof(expandedWebConfig));
            Debug.ArgumentNotNull(webFileSystem, nameof(webFileSystem));
            Debug.ArgumentNotNull(dataFileSystem, nameof(dataFileSystem));

            var customValidation = new CustomValidation();

            customValidation.Load(element);

            writer.Clear();

            customValidation.Process(writer, options, webConfig, expandedWebConfig, webFileSystem, dataFileSystem);

            writer.Write(output, customValidation.Category, customValidation.Title);
        }

        private void ProcessItem([NotNull] XmlTextWriter output, [NotNull] ValidationWriter writer, [NotNull] ValidationManager.ItemValidationDescriptor descriptor, [NotNull] Item item)
        {
            Debug.ArgumentNotNull(output, nameof(output));
            Debug.ArgumentNotNull(writer, nameof(writer));
            Debug.ArgumentNotNull(descriptor, nameof(descriptor));
            Debug.ArgumentNotNull(item, nameof(item));

            try
            {
                writer.Clear();

                descriptor.Instance.Check(writer, item);

                writer.Write(output, descriptor.Attribute.Category, descriptor.Attribute.Name);
            }
            catch (Exception ex)
            {
                Log.Error("Validations", ex, GetType());
            }
        }

        private void ProcessItem([NotNull] XmlTextWriter output, [NotNull] ValidationWriter writer, [NotNull] ValidationAnalyzerOptions options, [NotNull] IEnumerable<Language> languages, [NotNull] Item item)
        {
            Debug.ArgumentNotNull(output, nameof(output));
            Debug.ArgumentNotNull(writer, nameof(writer));
            Debug.ArgumentNotNull(options, nameof(options));
            Debug.ArgumentNotNull(languages, nameof(languages));
            Debug.ArgumentNotNull(item, nameof(item));

            ProcessItemValidations(output, writer, options, languages, item);
            ProcessLayout(output, writer, options, item);

            if (!options.Deep)
            {
                return;
            }

            foreach (Item child in item.Children)
            {
                ProcessItem(output, writer, options, languages, child);
            }
        }

        private void ProcessItems([NotNull] XmlTextWriter output, [NotNull] ValidationAnalyzerOptions options)
        {
            Debug.ArgumentNotNull(output, nameof(output));
            Debug.ArgumentNotNull(options, nameof(options));

            var writer = new ValidationWriter();

            foreach (var database in options.Databases)
            {
                var languages = options.DatabasesAndLanguages.Where(d => d.Database == database).Select(d => d.Language).ToList();

                var item = options.RootItem ?? database.GetRootItem();

                ProcessItem(output, writer, options, languages, item);
            }
        }

        private void ProcessItemValidation([NotNull] XmlTextWriter output, [NotNull] ValidationWriter writer, [NotNull] ValidationAnalyzerOptions options, [NotNull] IEnumerable<Language> languages, [NotNull] Item item, [NotNull] ValidationManager.ItemValidationDescriptor itemValidationDescriptor)
        {
            Debug.ArgumentNotNull(output, nameof(output));
            Debug.ArgumentNotNull(writer, nameof(writer));
            Debug.ArgumentNotNull(options, nameof(options));
            Debug.ArgumentNotNull(languages, nameof(languages));
            Debug.ArgumentNotNull(item, nameof(item));
            Debug.ArgumentNotNull(itemValidationDescriptor, nameof(itemValidationDescriptor));

            if (options.InactiveValidations.Contains("[" + itemValidationDescriptor.Attribute.Name + "]"))
            {
                return;
            }

            if (!itemValidationDescriptor.Instance.CanCheck(options.ContextName, item))
            {
                return;
            }

            if (itemValidationDescriptor.Attribute.ExecutePerLanguage)
            {
                foreach (var language in languages)
                {
                    using (new LanguageSwitcher(language))
                    {
                        ProcessItem(output, writer, itemValidationDescriptor, item);
                    }
                }
            }
            else
            {
                ProcessItem(output, writer, itemValidationDescriptor, item);
            }
        }

        private void ProcessItemValidations([NotNull] XmlTextWriter output, [NotNull] ValidationWriter writer, [NotNull] ValidationAnalyzerOptions options, [NotNull] IEnumerable<Language> languages, [NotNull] Item item)
        {
            Debug.ArgumentNotNull(output, nameof(output));
            Debug.ArgumentNotNull(writer, nameof(writer));
            Debug.ArgumentNotNull(options, nameof(options));
            Debug.ArgumentNotNull(languages, nameof(languages));
            Debug.ArgumentNotNull(item, nameof(item));

            foreach (var itemValidationDescriptor in ValidationManager.ItemValidations)
            {
                ProcessItemValidation(output, writer, options, languages, item, itemValidationDescriptor);
            }
        }

        private void ProcessLayout([NotNull] XmlTextWriter output, [NotNull] ValidationWriter writer, [NotNull] ValidationAnalyzerOptions options, [NotNull] Item item)
        {
            Debug.ArgumentNotNull(output, nameof(output));
            Debug.ArgumentNotNull(writer, nameof(writer));
            Debug.ArgumentNotNull(options, nameof(options));
            Debug.ArgumentNotNull(item, nameof(item));

            var layout = GetFieldValuePipeline.Run().WithParameters(item.Fields[FieldIDs.LayoutField]).Value ?? string.Empty;
            if (string.IsNullOrEmpty(layout))
            {
                return;
            }

            XDocument doc;
            try
            {
                doc = XDocument.Parse(layout);
            }
            catch
            {
                return;
            }

            var root = doc.Root;
            if (root == null)
            {
                return;
            }

            foreach (var renderingElement in root.Elements().Elements())
            {
                ProcessLayoutRendering(output, writer, options, item, renderingElement);
            }
        }

        private void ProcessLayoutRendering([NotNull] XmlTextWriter output, [NotNull] ValidationWriter writer, [NotNull] ValidationAnalyzerOptions options, [NotNull] Item item, [NotNull] XElement renderingElement)
        {
            Debug.ArgumentNotNull(output, nameof(output));
            Debug.ArgumentNotNull(writer, nameof(writer));
            Debug.ArgumentNotNull(options, nameof(options));
            Debug.ArgumentNotNull(item, nameof(item));
            Debug.ArgumentNotNull(renderingElement, nameof(renderingElement));

            var renderingItemId = renderingElement.GetAttributeValue("id");
            if (string.IsNullOrEmpty(renderingItemId))
            {
                return;
            }

            var renderingItem = item.Database.GetItem(renderingItemId);
            if (renderingItem == null)
            {
                return;
            }

            foreach (var renderingValidationDescriptor in ValidationManager.RenderingValidations)
            {
                if (options.InactiveValidations.Contains("[" + renderingValidationDescriptor.Attribute.Name + "]"))
                {
                    continue;
                }

                if (!renderingValidationDescriptor.Instance.CanCheck(options.ContextName, item, renderingElement, renderingItem))
                {
                    continue;
                }

                try
                {
                    writer.Clear();

                    renderingValidationDescriptor.Instance.Check(writer, item, renderingElement, renderingItem);

                    writer.Write(output, renderingValidationDescriptor.Attribute.Category, renderingValidationDescriptor.Attribute.Name);
                }
                catch (Exception ex)
                {
                    Log.Error("Validations", ex, GetType());
                }
            }
        }

        private void ProcessValidations([NotNull] XmlTextWriter output, [NotNull] ValidationAnalyzerOptions options)
        {
            Debug.ArgumentNotNull(output, nameof(output));
            Debug.ArgumentNotNull(options, nameof(options));

            var writer = new ValidationWriter();

            foreach (var definition in ValidationManager.Validations.OrderBy(v => v.Attribute.Category).ThenBy(v => v.Type.Name))
            {
                if (options.InactiveValidations.Contains("[" + definition.Attribute.Name + "]"))
                {
                    continue;
                }

                var instance = definition.GetInstance();
                if (instance == null)
                {
                    continue;
                }

                if (!instance.CanCheck(options.ContextName))
                {
                    continue;
                }

                try
                {
                    writer.Clear();

                    instance.Check(writer);

                    writer.Write(output, definition.Attribute.Category, definition.Attribute.Name);
                }
                catch (Exception ex)
                {
                    Log.Error("Validations", ex, GetType());
                }
            }
        }
    }
}
