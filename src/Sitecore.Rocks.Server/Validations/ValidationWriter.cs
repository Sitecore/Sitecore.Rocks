// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Collections.Generic;
using System.Xml;
using Sitecore.Data.Items;
using Sitecore.Diagnostics;

namespace Sitecore.Rocks.Server.Validations
{
    public class ValidationWriter
    {
        private readonly List<Record> records = new List<Record>();

        public void Write(SeverityLevel severity, [NotNull] string title, [NotNull] string problem, [NotNull] string solution)
        {
            Assert.ArgumentNotNull(title, nameof(title));
            Assert.ArgumentNotNull(problem, nameof(problem));
            Assert.ArgumentNotNull(solution, nameof(solution));

            var record = new Record
            {
                Title = title,
                Problem = problem,
                Solution = solution,
                Severity = severity
            };

            records.Add(record);
        }

        public void Write(SeverityLevel severity, [NotNull] string title, [NotNull] string problem, [NotNull] string solution, [NotNull] Item item)
        {
            Assert.ArgumentNotNull(title, nameof(title));
            Assert.ArgumentNotNull(problem, nameof(problem));
            Assert.ArgumentNotNull(solution, nameof(solution));
            Assert.ArgumentNotNull(item, nameof(item));

            var record = new Record
            {
                Title = title,
                Problem = problem,
                Solution = solution,
                Severity = severity,
                Item = item
            };

            records.Add(record);
        }

        internal void Clear()
        {
            records.Clear();
        }

        internal void Write([NotNull] XmlTextWriter output, [NotNull] string category, [NotNull] string name)
        {
            Debug.ArgumentNotNull(output, nameof(output));
            Debug.ArgumentNotNull(category, nameof(category));
            Debug.ArgumentNotNull(name, nameof(name));

            foreach (var record in records)
            {
                var severityText = string.Empty;

                switch (record.Severity)
                {
                    case SeverityLevel.None:
                        severityText = "none";
                        break;
                    case SeverityLevel.Error:
                        severityText = "error";
                        break;
                    case SeverityLevel.Warning:
                        severityText = "warning";
                        break;
                    case SeverityLevel.Suggestion:
                        severityText = "suggestion";
                        break;
                    case SeverityLevel.Hint:
                        severityText = "hint";
                        break;
                }

                output.WriteStartElement("item");

                output.WriteAttributeString("name", name);
                output.WriteAttributeString("severity", severityText);
                output.WriteAttributeString("category", category);

                if (record.Item != null)
                {
                    output.WriteAttributeString("item", string.Format("{0}/{1}/{2}/{3}", record.Item.Database.Name, record.Item.ID.ToString(), record.Item.Language.ToString(), record.Item.Version.ToString()));
                    output.WriteAttributeString("itempath", record.Item.Paths.Path);
                }

                if (record.ExternalLink != null)
                {
                    output.WriteAttributeString("link", record.ExternalLink);
                }

                output.WriteElementString("title", record.Title);
                output.WriteElementString("problem", record.Problem);
                output.WriteElementString("solution", record.Solution);

                output.WriteEndElement();
            }
        }

        private class Record
        {
            public string ExternalLink { get; set; }

            public Item Item { get; set; }

            public string Problem { get; set; }

            public SeverityLevel Severity { get; set; }

            public string Solution { get; set; }

            public string Title { get; set; }
        }
    }
}
