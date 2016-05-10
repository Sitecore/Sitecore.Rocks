// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.Security.Cryptography;
using System.Text;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Diagnostics;

namespace Sitecore.Rocks.UI.Management.ManagementItems.Validations
{
    public class ValidationDescriptor
    {
        public ValidationDescriptor([NotNull] string name, SeverityLevel severity, [NotNull] string category, [NotNull] string title, [NotNull] string problem, [NotNull] string solution)
        {
            Assert.ArgumentNotNull(name, nameof(name));
            Assert.ArgumentNotNull(category, nameof(category));
            Assert.ArgumentNotNull(title, nameof(title));
            Assert.ArgumentNotNull(problem, nameof(problem));
            Assert.ArgumentNotNull(solution, nameof(solution));

            Name = name;
            Severity = severity;
            Title = title;
            Problem = problem;
            Solution = solution;
            Category = category;

            ExternalLink = string.Empty;
            ItemUri = ItemVersionUri.Empty;
            ItemPath = string.Empty;
        }

        [NotNull]
        public string Category { get; }

        [NotNull]
        public string ExternalLink { get; set; }

        [NotNull]
        public string ItemPath { get; set; }

        [NotNull]
        public ItemVersionUri ItemUri { get; set; }

        [NotNull]
        public string Name { get; set; }

        [NotNull]
        public string Problem { get; }

        public SeverityLevel Severity { get; set; }

        [NotNull]
        public string Solution { get; }

        [NotNull]
        public string Title { get; }

        [NotNull]
        public string GetKey()
        {
            var s = Severity.ToString() + ',' + Category + ',' + Title + ',' + Problem + ',' + Solution;
            if (ItemUri != ItemVersionUri.Empty)
            {
                s += "," + ItemUri;
            }

            var bytes = Encoding.UTF8.GetBytes(s);

            var hash = MD5.Create().ComputeHash(bytes);

            return new Guid(hash).ToString(@"B").ToUpperInvariant();
        }
    }
}
