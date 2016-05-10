// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.IO;
using Sitecore.Data;
using Sitecore.Data.Items;
using Sitecore.Diagnostics;
using Sitecore.Rocks.Server.QueryAnalyzers.Parsers;

namespace Sitecore.Rocks.Server.QueryAnalyzers.ImportSources
{
    [ImportSource("tree", QueryAnalyzerTokenType.Tree)]
    public class TreeImportSource : ImportSourceBase
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
            var level = 0;
            var count = 0;

            while (true)
            {
                var newItem = GetNewItem(reader);
                if (newItem == null)
                {
                    break;
                }

                if (newItem.Level > level + 1)
                {
                    throw new Exception("Cannot create an item without a parent - level skips more than 1.");
                }

                while (newItem.Level <= level)
                {
                    item = item.Parent;
                    level--;
                }

                var template = item.Database.GetItem(newItem.TemplateName);
                if (template == null)
                {
                    throw new Exception("Template not found: " + newItem.TemplateName);
                }

                if (newItem.Repeat > 1)
                {
                    var parent = item;

                    for (var index = 1; index <= newItem.Repeat; index++)
                    {
                        item = parent.Add(newItem.ItemName + " " + index, new TemplateID(template.ID));
                        count++;
                    }

                    count--;
                }
                else
                {
                    item = item.Add(newItem.ItemName, new TemplateID(template.ID));
                }

                level++;
                count++;
            }

            return count;
        }

        public override void Parse(Parser parser)
        {
            Assert.ArgumentNotNull(parser, nameof(parser));

            parser.Match(QueryAnalyzerTokenType.Tree, "\"tree\" expected");

            ParseSource(parser);
        }

        [CanBeNull]
        private NewItem GetNewItem(StringReader reader)
        {
            string line;
            do
            {
                line = reader.ReadLine();
            }
            while (line == string.Empty);

            if (line == null)
            {
                return null;
            }

            var result = new NewItem
            {
                ItemName = string.Empty,
                TemplateName = string.Empty
            };

            var mode = 0;
            var repeat = string.Empty;

            for (var index = 0; index < line.Length; index++)
            {
                var c = line[index];

                switch (mode)
                {
                    case 0:
                        if (c == '*')
                        {
                            result.Level++;
                            continue;
                        }

                        result.ItemName += c;
                        mode = 1;
                        break;

                    case 1:
                        if (c == '|')
                        {
                            mode = 2;
                            continue;
                        }

                        result.ItemName += c;
                        break;

                    case 2:
                        if (c == '|')
                        {
                            mode = 3;
                            continue;
                        }

                        result.TemplateName += c;
                        break;

                    case 3:
                        repeat += c;
                        break;
                }
            }

            if (!string.IsNullOrEmpty(repeat))
            {
                result.Repeat = MainUtil.GetInt(repeat, 0);
            }

            result.ItemName = result.ItemName.Trim();
            result.TemplateName = GetTemplateName(result.TemplateName.Trim());

            return result;
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

        public class NewItem
        {
            public string ItemName { get; set; }

            public int Level { get; set; }

            public int Repeat { get; set; }

            public string TemplateName { get; set; }
        }
    }
}
