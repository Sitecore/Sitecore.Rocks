// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Collections.Generic;
using System.Linq;
using Sitecore.Configuration;
using Sitecore.Data;
using Sitecore.Data.Items;
using Sitecore.Data.Managers;
using Sitecore.Diagnostics;

namespace Sitecore.Rocks.Server.Validations
{
    public class ValidationAnalyzerOptions
    {
        public ValidationAnalyzerOptions()
        {
            DatabasesAndLanguages = new List<DatabaseLanguageDescriptor>();
            ContextName = string.Empty;
            CustomValidations = string.Empty;
            InactiveValidations = string.Empty;

            Deep = true;
            ProcessValidations = true;
            ProcessItems = true;
            ProcessCustomValidations = true;
        }

        [NotNull]
        public string ContextName { get; set; }

        [NotNull]
        public string CustomValidations { get; set; }

        [NotNull]
        public IEnumerable<Database> Databases
        {
            get { return DatabasesAndLanguages.Select(d => d.Database).Distinct(); }
        }

        [NotNull]
        public IList<DatabaseLanguageDescriptor> DatabasesAndLanguages { get; }

        public bool Deep { get; set; }

        [NotNull]
        public string InactiveValidations { get; set; }

        public bool ProcessCustomValidations { get; set; }

        public bool ProcessItems { get; set; }

        public bool ProcessValidations { get; set; }

        [CanBeNull]
        public Item RootItem { get; set; }

        public void ParseDatabaseAndLanguages([NotNull] string databasesAndLanguages)
        {
            Assert.ArgumentNotNull(databasesAndLanguages, nameof(databasesAndLanguages));

            foreach (var pair in databasesAndLanguages.Split('|'))
            {
                var parts = pair.Split('^');

                var database = Factory.GetDatabase(parts[0]);
                if (database == null)
                {
                    continue;
                }

                for (var i = 1; i < parts.Length; i++)
                {
                    var languageName = parts[i];
                    var language = LanguageManager.GetLanguage(languageName, database);

                    if (language != null)
                    {
                        DatabasesAndLanguages.Add(new DatabaseLanguageDescriptor(database, language));
                    }
                }
            }
        }
    }
}
