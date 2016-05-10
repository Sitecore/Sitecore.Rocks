// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.Text;
using System.Text.RegularExpressions;
using Sitecore.Data.Items;
using Sitecore.Diagnostics;
using Sitecore.Extensions.StringExtensions;
using Sitecore.Rocks.Server.Extensibility.Pipelines;

namespace Sitecore.Rocks.Server.Pipelines.BreadcrumbDropDown
{
    [Pipeline(typeof(BreadcrumbDropDownPipeline), 1000)]
    public class PartialMatch : PipelineProcessor<BreadcrumbDropDownPipeline>
    {
        public string GetPattern(string query)
        {
            var sb = new StringBuilder();

            foreach (var c in query.ToCharArray())
            {
                if (char.IsUpper(c))
                {
                    sb.Append(".*");
                    sb.Append(c);
                }
                else
                {
                    sb.Append(c);
                }
            }

            return sb.ToString();
        }

        protected override void Process([NotNull] BreadcrumbDropDownPipeline pipeline)
        {
            Assert.ArgumentNotNull(pipeline, nameof(pipeline));

            var path = pipeline.Path.Replace("\\", "/");
            if (path.StartsWith("sitecore/", StringComparison.InvariantCultureIgnoreCase))
            {
                path = "/" + path;
            }

            var n = path.LastIndexOf("/", StringComparison.InvariantCultureIgnoreCase);
            if (n < 0)
            {
                return;
            }

            var basePath = path.Left(n);
            var partial = path.Mid(n + 1);

            if (string.IsNullOrEmpty(basePath) || string.IsNullOrEmpty(partial))
            {
                return;
            }

            var item = pipeline.Database.GetItem(basePath);
            if (item == null)
            {
                return;
            }

            var pattern = GetPattern(partial);

            foreach (Item child in item.Children)
            {
                if (Regex.IsMatch(child.Name, pattern, RegexOptions.IgnoreCase | RegexOptions.IgnorePatternWhitespace | RegexOptions.CultureInvariant))
                {
                    pipeline.Items.Add(child);
                }
            }

            pipeline.Abort();
        }
    }
}
