// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Sitecore.Data.Items;
using Sitecore.Diagnostics;
using Sitecore.Extensions.StringExtensions;
using Sitecore.IO;
using Sitecore.Layouts;
using Sitecore.Rocks.Server.Pipelines.GetFieldValue;
using Sitecore.Text;

namespace Sitecore.Rocks.Server.Validations.Items.Presentation
{
    [Validation("Referenced placeholder does not exist", "Presentation")]
    public class ReferenceToNonExistingPlaceholder : ItemValidation
    {
        public override bool CanCheck(string contextName, Item item)
        {
            Assert.ArgumentNotNull(contextName, nameof(contextName));
            Assert.ArgumentNotNull(item, nameof(item));

            return contextName == "Site";
        }

        public override void Check(ValidationWriter output, Item item)
        {
            Assert.ArgumentNotNull(output, nameof(output));
            Assert.ArgumentNotNull(item, nameof(item));

            var layoutField = item.Fields[FieldIDs.LayoutField];
            if (layoutField.ContainsStandardValue)
            {
                return;
            }

            var value = GetFieldValuePipeline.Run().WithParameters(layoutField).Value;
            if (string.IsNullOrEmpty(value))
            {
                return;
            }

            var layoutDefinition = LayoutDefinition.Parse(value);
            if (layoutDefinition == null)
            {
                return;
            }

            foreach (DeviceDefinition device in layoutDefinition.Devices)
            {
                var renderings = device.Renderings;
                if (renderings == null || renderings.Count == 0)
                {
                    continue;
                }

                var layoutId = device.Layout;
                if (string.IsNullOrEmpty(layoutId))
                {
                    continue;
                }

                var layoutItem = item.Database.GetItem(layoutId);
                if (layoutItem == null)
                {
                    continue;
                }

                var deviceName = "Unknown";
                var deviceId = device.ID;

                if (!string.IsNullOrEmpty(deviceId))
                {
                    var deviceItem = item.Database.GetItem(deviceId);
                    if (deviceItem != null)
                    {
                        deviceName = deviceItem.Name;
                    }
                }

                var placeholders = new List<string>();
                placeholders.AddRange(Analyze(layoutItem, string.Empty));

                foreach (RenderingDefinition rendering in renderings)
                {
                    var itemId = rendering.ItemID;
                    if (string.IsNullOrEmpty(itemId))
                    {
                        continue;
                    }

                    var renderingItem = item.Database.GetItem(itemId);
                    if (renderingItem == null)
                    {
                        continue;
                    }

                    var parameters = new UrlString(rendering.Parameters);
                    var id = parameters["Id"];

                    placeholders.AddRange(Analyze(renderingItem, id));
                }

                foreach (RenderingDefinition rendering in renderings)
                {
                    var placeholder = rendering.Placeholder;
                    if (string.IsNullOrEmpty(placeholder))
                    {
                        continue;
                    }

                    var renderingName = rendering.UniqueId ?? string.Empty;

                    var itemId = rendering.ItemID;
                    if (!string.IsNullOrEmpty(itemId))
                    {
                        var renderingItem = item.Database.GetItem(itemId);
                        if (renderingItem != null)
                        {
                            renderingName = renderingItem.Name;
                        }
                    }

                    if (!Contains(placeholders, placeholder))
                    {
                        output.Write(SeverityLevel.Warning, "Referenced placeholder does not exist", string.Format("The placeholder \"{0}\" which is referenced by the \"{1}\" rendering in the \"{2}\" device does not exist.", placeholder, renderingName, deviceName), string.Format("Set the placeholder of the \"{0}\" rendering in the \"{2}\" device to one of the following placeholders: {1}.", renderingName, string.Join(", ", placeholders.ToArray()), deviceName), item);
                    }
                }
            }
        }

        [NotNull]
        private IEnumerable<string> Analyze([NotNull] Item item, [NotNull] string id)
        {
            Debug.ArgumentNotNull(item, nameof(item));
            Debug.ArgumentNotNull(id, nameof(id));

            var placeHolders = item["Place Holders"];
            if (!string.IsNullOrEmpty(placeHolders))
            {
                return placeHolders.Split(',').Where(s => !string.IsNullOrEmpty(s)).Select(s => s.Trim().Replace("$Id", id));
            }

            return AnalyzeFile(item, false);
        }

        [NotNull]
        private IEnumerable<string> AnalyzeFile([NotNull] Item item, bool includeDynamicPlaceholders)
        {
            Debug.ArgumentNotNull(item, nameof(item));

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

        [NotNull]
        private IEnumerable<string> AnalyzeViewFile([NotNull] string source)
        {
            Debug.ArgumentNotNull(source, nameof(source));

            var matches = Regex.Matches(source, "\\@Html\\.Sitecore\\(\\)\\.Placeholder\\(\"([^\"]*)\"\\)", RegexOptions.IgnoreCase);
            return matches.OfType<Match>().Select(i => i.Groups[1].ToString().Trim());
        }

        [NotNull]
        private IEnumerable<string> AnalyzeViewFileWithDynamicPlaceholders([NotNull] string source)
        {
            Debug.ArgumentNotNull(source, nameof(source));

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

        [NotNull]
        private static IEnumerable<string> AnalyzeWebFormsFile([NotNull] string source)
        {
            Debug.ArgumentNotNull(source, nameof(source));

            var matches = Regex.Matches(source, "<[^>]*Placeholder[^>]*Key=\"([^\"]*)\"[^>]*>", RegexOptions.IgnoreCase);

            return matches.OfType<Match>().Select(i => i.Groups[1].ToString().Trim());
        }

        private bool Contains([NotNull] List<string> placeholders, [NotNull] string placeholder)
        {
            Debug.ArgumentNotNull(placeholders, nameof(placeholders));
            Debug.ArgumentNotNull(placeholder, nameof(placeholder));

            if (placeholder.IndexOf('/') < 0)
            {
                foreach (var ph in placeholders)
                {
                    if (string.Compare(ph, placeholder, StringComparison.InvariantCultureIgnoreCase) == 0)
                    {
                        return true;
                    }
                }

                return false;
            }

            var separator = new[]
            {
                '/'
            };

            var parts = placeholder.Split(separator, StringSplitOptions.RemoveEmptyEntries);

            foreach (var part in parts)
            {
                var placeholderPart = part;

                if (!placeholders.Any(p => string.Compare(p, placeholderPart, StringComparison.InvariantCultureIgnoreCase) == 0))
                {
                    return false;
                }
            }

            return true;
        }

        private void GetPlaceholders([NotNull] List<string> placeholders, [NotNull] string layoutFile)
        {
            Debug.ArgumentNotNull(layoutFile, nameof(layoutFile));
            Debug.ArgumentNotNull(placeholders, nameof(placeholders));

            var path = FileUtil.MapPath(layoutFile);

            if (!File.Exists(path))
            {
                return;
            }

            string source;
            try
            {
                source = FileUtil.ReadFromFile(path);
            }
            catch
            {
                return;
            }

            var matches = Regex.Matches(source, "<[^>]*Placeholder[^>]*Key=\"([^\"]*)\"[^>]*>", RegexOptions.IgnoreCase);
            if (matches.Count == 0)
            {
                return;
            }

            foreach (Match match in matches)
            {
                placeholders.Add(match.Groups[1].ToString());
            }
        }
    }
}
