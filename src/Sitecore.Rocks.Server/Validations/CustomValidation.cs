// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;
using Sitecore.Collections;
using Sitecore.Data.Items;
using Sitecore.Diagnostics;
using Sitecore.Rocks.Server.Extensions.XElementExtensions;
using Sitecore.Xml;

namespace Sitecore.Rocks.Server.Validations
{
    public enum CustomValidationType
    {
        XPath,

        Query,

        WebConfig,

        ExpandedWebConfig,

        WebFileSystem,

        DataFileSystem
    }

    public class CustomValidation
    {
        public string Category { get; set; }

        public string Code { get; set; }

        public string Problem { get; set; }

        public SeverityLevel Severity { get; set; }

        public string Solution { get; set; }

        public string Title { get; set; }

        public CustomValidationType Type { get; set; }

        public bool WhenExists { get; set; }

        public void Load([NotNull] XElement element)
        {
            Assert.ArgumentNotNull(element, nameof(element));

            var type = element.GetAttributeValue("type");

            Type = (CustomValidationType)int.Parse(type);
            Severity = (SeverityLevel)int.Parse(type);
            Code = element.GetElementValue("code");
            Category = element.GetElementValue("category");
            Title = element.GetElementValue("title");
            Problem = element.GetElementValue("problem");
            Solution = element.GetElementValue("solution");
            WhenExists = element.GetElementValue("whenexists") == "true";
        }

        public void Process([NotNull] ValidationWriter output, [NotNull] ValidationAnalyzerOptions options, [NotNull] XmlDocument webConfig, [NotNull] XmlDocument expandedWebConfig, [NotNull] FileSystemNavigator webFileSystem, [NotNull] FileSystemNavigator dataFileSystem)
        {
            Assert.ArgumentNotNull(output, nameof(output));
            Assert.ArgumentNotNull(options, nameof(options));
            Assert.ArgumentNotNull(webConfig, nameof(webConfig));
            Assert.ArgumentNotNull(expandedWebConfig, nameof(expandedWebConfig));
            Assert.ArgumentNotNull(webFileSystem, nameof(webFileSystem));
            Assert.ArgumentNotNull(dataFileSystem, nameof(dataFileSystem));

            switch (Type)
            {
                case CustomValidationType.Query:
                    ProcessQuery(output, options);
                    break;
                case CustomValidationType.XPath:
                    ProcessXPath(output, options);
                    break;
                case CustomValidationType.WebConfig:
                    ProcessWebConfig(output, webConfig);
                    break;
                case CustomValidationType.ExpandedWebConfig:
                    ProcessExpandedWebConfig(output, expandedWebConfig);
                    break;
                case CustomValidationType.WebFileSystem:
                    ProcessFileSystem(output, webFileSystem);
                    break;
                case CustomValidationType.DataFileSystem:
                    ProcessFileSystem(output, dataFileSystem);
                    break;
            }
        }

        [NotNull]
        private string Expand([NotNull] string text, [NotNull] XmlNode node)
        {
            Debug.ArgumentNotNull(text, nameof(text));
            Debug.ArgumentNotNull(node, nameof(node));

            text = text.Replace("$name", node.LocalName);

            if (text.IndexOf("$path", StringComparison.InvariantCultureIgnoreCase) >= 0)
            {
                text = text.Replace("$path", XmlUtil.GetPath(node));
            }

            return text;
        }

        [NotNull]
        private string Expand([NotNull] string text, [NotNull] XPathNavigator navigator)
        {
            Debug.ArgumentNotNull(text, nameof(text));
            Debug.ArgumentNotNull(navigator, nameof(navigator));

            text = text.Replace("$name", navigator.LocalName);

            if (text.IndexOf("$path", StringComparison.InvariantCultureIgnoreCase) >= 0)
            {
                var path = string.Empty;
                var n = navigator.Clone();

                while (n != null)
                {
                    if (!string.IsNullOrEmpty(n.LocalName))
                    {
                        path = "\\" + n.LocalName + path;
                    }

                    if (!n.MoveToParent())
                    {
                        break;
                    }
                }

                text = text.Replace("$path", path);
            }

            return text;
        }

        private void ProcessExpandedWebConfig([NotNull] ValidationWriter output, [NotNull] XmlDocument webConfig)
        {
            Debug.ArgumentNotNull(output, nameof(output));
            Debug.ArgumentNotNull(webConfig, nameof(webConfig));

            XmlNodeList nodes = null;
            try
            {
                nodes = webConfig.SelectNodes(Code);
            }
            catch (Exception ex)
            {
                Log.Error("Custom Validation", ex, GetType());
            }

            if (WhenExists)
            {
                if (nodes != null)
                {
                    foreach (XmlNode node in nodes)
                    {
                        output.Write(Severity, Title, Expand(Problem, node), Expand(Solution, node));
                    }
                }
            }
            else
            {
                if (nodes == null || nodes.Count == 0)
                {
                    output.Write(Severity, Title, Problem, Solution);
                }
            }
        }

        private void ProcessFileSystem([NotNull] ValidationWriter output, [NotNull] FileSystemNavigator webFileSystem)
        {
            Debug.ArgumentNotNull(output, nameof(output));
            Debug.ArgumentNotNull(webFileSystem, nameof(webFileSystem));

            XPathNodeIterator nodes = null;
            try
            {
                nodes = webFileSystem.Select(Code);
            }
            catch (Exception ex)
            {
                Log.Error("Custom Validation", ex, GetType());
            }

            if (WhenExists)
            {
                if (nodes != null)
                {
                    foreach (XPathNavigator navigator in nodes)
                    {
                        output.Write(Severity, Title, Expand(Problem, navigator), Expand(Solution, navigator));
                    }
                }
            }
            else
            {
                if (nodes == null || nodes.Count == 0)
                {
                    output.Write(Severity, Title, Problem, Solution);
                }
            }
        }

        private void ProcessQuery([NotNull] ValidationWriter output, [NotNull] ValidationAnalyzerOptions options)
        {
            Debug.ArgumentNotNull(output, nameof(output));
            Debug.ArgumentNotNull(options, nameof(options));

            foreach (var database in options.Databases)
            {
                Item[] items;
                try
                {
                    items = database.SelectItems(Code);
                }
                catch (Exception ex)
                {
                    Log.Error("Custom Validation", ex, GetType());
                    continue;
                }

                if (WhenExists)
                {
                    foreach (var item in items)
                    {
                        output.Write(Severity, Title, Problem, Solution, item);
                    }
                }
                else
                {
                    if (!items.Any())
                    {
                        output.Write(Severity, Title, Problem, Solution);
                    }
                }
            }
        }

        private void ProcessWebConfig([NotNull] ValidationWriter output, [NotNull] XmlDocument webConfig)
        {
            Debug.ArgumentNotNull(output, nameof(output));
            Debug.ArgumentNotNull(webConfig, nameof(webConfig));

            XmlNodeList nodes = null;
            try
            {
                nodes = webConfig.SelectNodes(Code);
            }
            catch (Exception ex)
            {
                Log.Error("Custom Validation", ex, GetType());
            }

            if (WhenExists)
            {
                if (nodes != null)
                {
                    foreach (XmlNode node in nodes)
                    {
                        output.Write(Severity, Title, Expand(Problem, node), Expand(Solution, node));
                    }
                }
            }
            else
            {
                if (nodes == null || nodes.Count == 0)
                {
                    output.Write(Severity, Title, Problem, Solution);
                }
            }
        }

        private void ProcessXPath([NotNull] ValidationWriter output, [NotNull] ValidationAnalyzerOptions options)
        {
            Debug.ArgumentNotNull(output, nameof(output));
            Debug.ArgumentNotNull(options, nameof(options));

            foreach (var database in options.Databases)
            {
                ItemList items;
                try
                {
                    items = database.SelectItemsUsingXPath(Code);
                }
                catch (Exception ex)
                {
                    Log.Error("Custom Validation", ex, GetType());
                    continue;
                }

                if (WhenExists)
                {
                    foreach (var item in items)
                    {
                        output.Write(Severity, Title, Problem, Solution, item);
                    }
                }
                else
                {
                    if (!items.Any())
                    {
                        output.Write(Severity, Title, Problem, Solution);
                    }
                }
            }
        }
    }
}
