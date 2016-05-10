// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Sitecore.Data.Items;
using Sitecore.Extensions.StringExtensions;
using Sitecore.IO;

namespace Sitecore.Rocks.Server.Layouts
{
    public static class PlaceHolderAnalyzer
    {
        [NotNull]
        public static IEnumerable<string> Analyze([NotNull] Item item)
        {
            var placeHolders = item["Place Holders"];
            if (!string.IsNullOrEmpty(placeHolders))
            {
                return placeHolders.Split(',').Where(s => !string.IsNullOrEmpty(s)).Select(s => s.Trim());
            }

            return AnalyzeFile(item, false);
        }

        public static IEnumerable<string> AnalyzeFile(Item item, bool includeDynamicPlaceholders)
        {
            var path = FileUtil.MapPath(item["Path"]);
            if (!FileUtil.Exists(path))
            {
                return Enumerable.Empty<string>();
            }

            var source = FileUtil.ReadFromFile(path);

            if (string.Compare(Path.GetExtension(path), ".ascx", StringComparison.CurrentCultureIgnoreCase) == 0 || string.Compare(Path.GetExtension(path), ".aspx", StringComparison.CurrentCultureIgnoreCase) == 0)
            {
                return AnalyzeWebFormsFile(source);
            }

            if (string.Compare(Path.GetExtension(path), ".cshtml", StringComparison.CurrentCultureIgnoreCase) == 0)
            {
                return includeDynamicPlaceholders ? AnalyzeViewFileWithDynamicPlaceholders(source) : AnalyzeViewFile(source);
            }

            return Enumerable.Empty<string>();
        }

        private static IEnumerable<string> AnalyzeViewFile(string source)
        {
            var matches = Regex.Matches(source, "\\@Html\\.Sitecore\\(\\)\\.Placeholder\\(\"([^\"]*)\"\\)", RegexOptions.IgnoreCase);
            return matches.OfType<Match>().Select(i => i.Groups[1].ToString().Trim());
        }

        private static IEnumerable<string> AnalyzeViewFileWithDynamicPlaceholders(string source)
        {
            var matches = Regex.Matches(source, "\\@Html\\.Sitecore\\(\\)\\.Placeholder\\(([^\"\\)]*)\"([^\"]*)\"\\)", RegexOptions.IgnoreCase);

            var result = new List<string>();
            foreach (var match in matches.OfType<Match>())
            {
                var prefix = match.Groups[1].ToString().Trim();
                var name = match.Groups[2].ToString().Trim();

                if (!string.IsNullOrEmpty(prefix))
                {
                    if (name.StartsWith("."))
                    {
                        name = name.Mid(1);
                    }

                    name = "$Id." + name;
                }

                result.Add(name);
            }

            return result;
        }

        private static IEnumerable<string> AnalyzeWebFormsFile(string source)
        {
            var matches = Regex.Matches(source, "<[^>]*Placeholder[^>]*Key=\"([^\"]*)\"[^>]*>", RegexOptions.IgnoreCase);

            return matches.OfType<Match>().Select(i => i.Groups[1].ToString().Trim());
        }
    }
}
