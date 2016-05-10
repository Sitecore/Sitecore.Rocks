// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Xml.Linq;
using Sitecore.Data;
using Sitecore.Data.Items;
using Sitecore.Data.Managers;
using Sitecore.Data.Templates;
using Sitecore.Diagnostics;
using Sitecore.Extensions.XElementExtensions;
using Sitecore.Text;

namespace Sitecore.Rocks.Server.Validations
{
    public abstract class SpeakRenderingValidation : RenderingValidation
    {
        private static string[] headingStyleExceptions;

        public static readonly ID ParametersTemplateFieldId = new ID("{7D24E54F-5C16-4314-90C9-6051AA1A7DA1}");

        private static readonly char[] separator =
        {
            ' ',
            '\r',
            '\n'
        };

        public static readonly ID ViewRenderingTemplateId = new ID("{99F8905D-4A87-4EB8-9F8B-A9BEBFB3ADD6}");

        protected bool FieldContainsText([NotNull] TemplateField field, [NotNull] XElement renderingElement, [NotNull] Item renderingItem)
        {
            Debug.ArgumentNotNull(field, nameof(field));
            Debug.ArgumentNotNull(renderingElement, nameof(renderingElement));
            Debug.ArgumentNotNull(renderingItem, nameof(renderingItem));

            if (field.IsShared)
            {
                return false;
            }

            if (string.Compare(field.Name, "Tooltip", StringComparison.InvariantCultureIgnoreCase) == 0)
            {
                return false;
            }

            var value = GetFieldValue(field.Name, renderingElement, renderingItem);
            if (value.Trim().StartsWith("{Binding", StringComparison.InvariantCultureIgnoreCase))
            {
                return false;
            }

            if (ID.IsID(value))
            {
                return false;
            }

            if (value.StartsWith("/sitecore/", StringComparison.InvariantCultureIgnoreCase))
            {
                return false;
            }

            return true;
        }

        [NotNull]
        protected string GetFieldValue([NotNull] string fieldName, [NotNull] XElement renderingElement, [NotNull] Item renderingItem)
        {
            Debug.ArgumentNotNull(fieldName, nameof(fieldName));
            Debug.ArgumentNotNull(renderingElement, nameof(renderingElement));
            Debug.ArgumentNotNull(renderingItem, nameof(renderingItem));

            var parameters = new UrlString(renderingElement.GetAttributeValue("par"));
            var result = HttpUtility.UrlDecode(parameters[fieldName]);
            if (!string.IsNullOrEmpty(result))
            {
                return result;
            }

            var dataSource = renderingElement.GetAttributeValue("ds");
            if (string.IsNullOrEmpty(dataSource))
            {
                return string.Empty;
            }

            var dataSourceItem = renderingItem.Database.GetItem(dataSource);
            if (dataSourceItem == null)
            {
                return string.Empty;
            }

            return dataSourceItem[fieldName];
        }

        [NotNull]
        protected IEnumerable<TemplateField> GetParametersTemplateFields([NotNull] Item item)
        {
            Debug.ArgumentNotNull(item, nameof(item));

            var parametersTemplateId = item[ParametersTemplateFieldId];
            if (string.IsNullOrEmpty(parametersTemplateId))
            {
                yield break;
            }

            var parameterTemplateItem = item.Database.GetItem(parametersTemplateId);
            if (parameterTemplateItem == null)
            {
                yield break;
            }

            var template = TemplateManager.GetTemplate(parameterTemplateItem.ID, parameterTemplateItem.Database);
            if (template == null)
            {
                yield break;
            }

            var fields = template.GetFields(true).OrderBy(f => f.Name).ToList();

            foreach (var field in fields)
            {
                if (field.Template.BaseIDs.Length != 0)
                {
                    yield return field;
                }
            }
        }

        [NotNull]
        protected string GetPlaceholder([NotNull] XElement renderingElement)
        {
            Debug.ArgumentNotNull(renderingElement, nameof(renderingElement));

            return renderingElement.GetAttributeValue("ph");
        }

        protected bool IsHeadingRendering([NotNull] XElement renderingElement, [NotNull] Item renderingItem)
        {
            Debug.ArgumentNotNull(renderingElement, nameof(renderingElement));
            Debug.ArgumentNotNull(renderingItem, nameof(renderingItem));

            if (renderingItem.Name.IndexOf("heading", StringComparison.InvariantCultureIgnoreCase) >= 0)
            {
                return true;
            }

            if (renderingItem.Name.IndexOf("header", StringComparison.InvariantCultureIgnoreCase) >= 0)
            {
                return true;
            }

            /*
      var placeholderName = this.GetPlaceholder(renderingElement);
      if (placeholderName.IndexOf("heading", StringComparison.InvariantCultureIgnoreCase) >= 0)
      {
        return true;
      }
      if (placeholderName.IndexOf("header", StringComparison.InvariantCultureIgnoreCase) >= 0)
      {
        return true;
      }
      */

            return false;
        }

        protected bool IsHeadingStyle([NotNull] string fieldValue)
        {
            Debug.ArgumentNotNull(fieldValue, nameof(fieldValue));

            if (headingStyleExceptions == null)
            {
                headingStyleExceptions = LoadHeadingStyleExceptions();
            }

            var startOfSentence = true;

            var parts = fieldValue.Split(separator, StringSplitOptions.RemoveEmptyEntries);
            foreach (var p in parts)
            {
                var part = p.Trim();
                if (string.IsNullOrEmpty(part))
                {
                    continue;
                }

                if (startOfSentence)
                {
                    startOfSentence = false;
                    if (char.IsLower(part, 0))
                    {
                        return false;
                    }

                    continue;
                }

                if (headingStyleExceptions.Contains(part.ToLowerInvariant()))
                {
                    if (char.IsUpper(part, 0))
                    {
                        return false;
                    }
                }
                else
                {
                    if (char.IsLower(part, 0))
                    {
                        return false;
                    }
                }

                if (part.EndsWith("."))
                {
                    startOfSentence = true;
                }
            }

            return true;
        }

        protected bool IsLabelRendering([NotNull] XElement renderingElement, [NotNull] Item renderingItem)
        {
            Debug.ArgumentNotNull(renderingElement, nameof(renderingElement));
            Debug.ArgumentNotNull(renderingItem, nameof(renderingItem));

            if (renderingItem.Name.IndexOf("label", StringComparison.InvariantCultureIgnoreCase) >= 0)
            {
                return true;
            }

            return false;
        }

        protected bool IsSentenceStyle([NotNull] string text)
        {
            Debug.ArgumentNotNull(text, nameof(text));

            if (string.IsNullOrEmpty(text))
            {
                return true;
            }

            var startOfSentence = true;

            for (var n = 0; n < text.Length; n++)
            {
                if (char.IsWhiteSpace(text, n))
                {
                    continue;
                }

                if (startOfSentence)
                {
                    startOfSentence = false;
                    if (char.IsLower(text, 0))
                    {
                        return false;
                    }

                    continue;
                }

                if (text[n] == '.')
                {
                    startOfSentence = true;
                    continue;
                }

                if (char.IsUpper(text, n))
                {
                    return false;
                }
            }

            return true;
        }

        protected bool IsViewRendering([NotNull] Item item)
        {
            Debug.ArgumentNotNull(item, nameof(item));

            if (item.TemplateID != ViewRenderingTemplateId)
            {
                return false;
            }

            return true;
        }

        [NotNull]
        private string[] LoadHeadingStyleExceptions()
        {
            var fileName = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? string.Empty, "Sitecore.Rocks.Server.Speak.HeadingStyleExceptions.txt");
            if (File.Exists(fileName))
            {
                return File.ReadAllLines(fileName);
            }

            var assembly = Assembly.GetExecutingAssembly();
            using (var stream = assembly.GetManifestResourceStream("Sitecore.Rocks.Server.Resources.HeadingStyleExceptions.txt"))
            {
                if (stream == null)
                {
                    return new string[0];
                }

                var lines = new List<string>();

                using (var reader = new StreamReader(stream))
                {
                    while (!reader.EndOfStream)
                    {
                        var line = reader.ReadLine();
                        if (!string.IsNullOrEmpty(line))
                        {
                            lines.Add(line);
                        }
                    }
                }

                return lines.ToArray();
            }
        }
    }
}
