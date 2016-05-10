// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.Collections.Generic;
using System.IO;
using Sitecore.Data.Items;
using Sitecore.Diagnostics;
using Sitecore.Extensions.StringExtensions;
using Sitecore.Rocks.Server.QueryAnalyzers.Parsers;

namespace Sitecore.Rocks.Server.QueryAnalyzers.ImportSources
{
    [ImportSource("csv", QueryAnalyzerTokenType.Csv)]
    public class CsvImportSource : ImportSourceBase
    {
        public override int Execute(Item item)
        {
            Assert.ArgumentNotNull(item, nameof(item));

            var source = GetSource();
            if (string.IsNullOrEmpty(source))
            {
                return 0;
            }

            var reader = new StringReader(source);
            List<string> columns = null;
            var list = new List<string>();
            var index = 0;
            var line = 0;
            var items = 0;
            var itemNameColumnIndex = -1;
            var templateNameColumnIndex = -1;
            var defaultTemplateName = string.Empty;

            do
            {
                line++;
                if (!ReadLine(reader, list))
                {
                    break;
                }

                if (list.Count == 0)
                {
                    continue;
                }

                if (list[0].StartsWith("<@DefaultTemplate=", StringComparison.InvariantCultureIgnoreCase))
                {
                    var s = list[0].Mid(2, list[0].Length - 4).Trim();
                    defaultTemplateName = GetTemplateName(s.Mid(16).Trim());
                    continue;
                }

                if (columns == null)
                {
                    columns = new List<string>();
                    foreach (var name in list)
                    {
                        columns.Add(name.Trim());
                    }

                    if (columns.Count == 0)
                    {
                        break;
                    }

                    for (var i = 0; i < columns.Count; i++)
                    {
                        if (string.Compare(columns[i], "@ItemName") == 0)
                        {
                            itemNameColumnIndex = i;
                        }

                        if (string.Compare(columns[i], "@TemplateName") == 0)
                        {
                            templateNameColumnIndex = i;
                        }
                    }

                    continue;
                }

                string itemName;
                if (itemNameColumnIndex >= 0 && itemNameColumnIndex < list.Count)
                {
                    itemName = Unquote(list[itemNameColumnIndex]);
                    itemName = ItemUtil.ProposeValidItemName(itemName);
                }
                else
                {
                    itemName = "Item " + index;
                    index++;
                }

                TemplateItem templateItem;
                if (templateNameColumnIndex >= 0 && templateNameColumnIndex < list.Count)
                {
                    templateItem = item.Database.GetItem(GetTemplateName(list[templateNameColumnIndex]));
                }
                else
                {
                    templateItem = item.Database.GetItem(defaultTemplateName);
                }

                if (templateItem == null)
                {
                    throw new Exception(string.Format("No template in line {0}. Use the <@DefaultTemplate@> directive to specify a default template name.", line));
                }

                var newItem = item.Add(itemName, templateItem);
                items++;

                newItem.Editing.BeginEdit();

                for (var i = 0; i < columns.Count; i++)
                {
                    if (i >= list.Count)
                    {
                        break;
                    }

                    var fieldName = columns[i];
                    if (string.Compare(fieldName, "@ItemName") == 0)
                    {
                        continue;
                    }

                    newItem[fieldName] = Unquote(list[i]);
                }

                newItem.Editing.EndEdit();
            }
            while (true);

            return items;
        }

        public override void Parse(Parser parser)
        {
            Assert.ArgumentNotNull(parser, nameof(parser));

            parser.Match(QueryAnalyzerTokenType.Csv, "\"csv\" expected");

            ParseSource(parser);
        }

        public bool ReadLine([NotNull] TextReader reader, [NotNull] List<string> list)
        {
            Assert.ArgumentNotNull(reader, nameof(reader));
            Assert.ArgumentNotNull(list, nameof(list));

            list.Clear();

            if (reader.Peek() < 0)
            {
                return false;
            }

            ReadToList(reader, list, string.Empty, 0);

            return true;
        }

        [NotNull]
        private string GetTemplateName(string templateName)
        {
            if (!templateName.StartsWith("/"))
            {
                templateName = "/" + templateName;
            }

            if (!templateName.StartsWith("/sitecore/templates", StringComparison.InvariantCultureIgnoreCase))
            {
                return "/sitecore/templates" + templateName;
            }

            return templateName;
        }

        private void ReadToList([NotNull] TextReader reader, [NotNull] List<string> list, [CanBeNull] string rest, int quote)
        {
            Debug.ArgumentNotNull(reader, nameof(reader));
            Assert.ArgumentNotNull(list, nameof(list));

            var start = 0;
            var line = reader.ReadLine();

            if (string.IsNullOrEmpty(line))
            {
                return;
            }

            line = line.Trim();

            if (line.StartsWith("<@") && line.EndsWith("@>"))
            {
                list.Add(line);
                return;
            }

            for (var i = 0; i < line.Length; i++)
            {
                switch (line[i])
                {
                    case '"':
                        quote = quote == 2 ? 1 : quote + 1;

                        break;
                    case ',':
                    case ';':
                        if (quote == 0 || quote == 2)
                        {
                            var end = i - 1;

                            if (string.IsNullOrEmpty(rest))
                            {
                                if (quote == 2)
                                {
                                    start++;
                                    end--;
                                }

                                list.Add(line.Substring(start, end - start + 1).Replace("\"\"", "\""));
                            }
                            else
                            {
                                var buf = rest + line.Substring(start, end - start + 1);

                                if (quote == 2)
                                {
                                    buf = buf.Substring(1, buf.Length - 2);
                                }

                                list.Add(buf.Replace("\"\"", "\""));
                                rest = string.Empty;
                            }

                            quote = 0;
                            start = i + 1;
                        }

                        break;
                }
            }

            if (quote != 1)
            {
                var end = line.Length - 1;

                if (quote == 2)
                {
                    start++;
                    end--;
                }

                list.Add(line.Substring(start, end - start + 1).Replace("\"\"", "\""));
            }
            else
            {
                ReadToList(reader, list, rest + line.Substring(start, line.Length - start) + "\r\n", quote);
            }
        }

        [NotNull]
        private string Unquote([NotNull] string value)
        {
            Debug.ArgumentNotNull(value, nameof(value));

            value = value.Trim();

            if (value.StartsWith("\"") && value.EndsWith("\""))
            {
                value = value.Mid(1, value.Length - 2);
            }

            if (value.StartsWith("'") && value.EndsWith("'"))
            {
                value = value.Mid(1, value.Length - 2);
            }
            return value;
        }
    }
}
