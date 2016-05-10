// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.IO;
using System.Text;
using System.Xml;
using Sitecore.Configuration;
using Sitecore.Data;
using Sitecore.Data.Managers;
using Sitecore.Diagnostics;
using Sitecore.IO;
using Sitecore.Rocks.Server.Jobs;
using Sitecore.Rocks.Server.Validations;

namespace Sitecore.Rocks.Server.Requests.Validations
{
    public class StartItemValidation
    {
        [NotNull]
        public string Execute([NotNull] string contextName, [NotNull] string databaseName, [NotNull] string itemId, [NotNull] string languageName, [NotNull] string version, [NotNull] string inactiveValidations, [NotNull] string customValidations, bool deep)
        {
            Assert.ArgumentNotNull(contextName, nameof(contextName));
            Assert.ArgumentNotNull(databaseName, nameof(databaseName));
            Assert.ArgumentNotNull(itemId, nameof(itemId));
            Assert.ArgumentNotNull(languageName, nameof(languageName));
            Assert.ArgumentNotNull(version, nameof(version));
            Assert.ArgumentNotNull(inactiveValidations, nameof(inactiveValidations));
            Assert.ArgumentNotNull(customValidations, nameof(customValidations));

            return BackgroundJob.Run("Validation", "Management", () => Process(contextName, databaseName, itemId, languageName, version, inactiveValidations, customValidations, deep));
        }

        public void Process([NotNull] string contextName, [NotNull] string databaseName, [NotNull] string itemId, [NotNull] string languageName, [NotNull] string version, [NotNull] string inactiveValidations, [NotNull] string customValidations, bool deep)
        {
            Assert.ArgumentNotNull(contextName, nameof(contextName));
            Assert.ArgumentNotNull(databaseName, nameof(databaseName));
            Assert.ArgumentNotNull(itemId, nameof(itemId));
            Assert.ArgumentNotNull(languageName, nameof(languageName));
            Assert.ArgumentNotNull(version, nameof(version));
            Assert.ArgumentNotNull(inactiveValidations, nameof(inactiveValidations));
            Assert.ArgumentNotNull(customValidations, nameof(customValidations));

            var v = Version.Latest;
            if (!string.IsNullOrEmpty(version))
            {
                v = Version.Parse(version);
            }

            var database = Factory.GetDatabase(databaseName);
            var language = LanguageManager.GetLanguage(languageName);
            var rootItem = database.GetItem(itemId, language, v);

            TempFolder.EnsureFolder();
            var fileName = Path.Combine(FileUtil.MapPath(TempFolder.Folder), string.Format("validation_{0}_{1}_{2}_{3}_{4}{5}.xml", contextName, database.Name, rootItem.ID.ToShortID(), language.Name, v, deep ? "_deep" : string.Empty));
            var tempFileName = fileName + ".tmp";

            try
            {
                using (var writer = new FileStream(tempFileName, FileMode.Create, FileAccess.Write, FileShare.None))
                {
                    var output = new XmlTextWriter(writer, Encoding.UTF8)
                    {
                        Formatting = Formatting.Indented,
                    };

                    var options = new ValidationAnalyzerOptions
                    {
                        ContextName = contextName,
                        InactiveValidations = inactiveValidations,
                        CustomValidations = customValidations,
                        RootItem = rootItem,
                        ProcessValidations = false,
                        ProcessCustomValidations = false,
                        Deep = deep
                    };

                    options.DatabasesAndLanguages.Add(new DatabaseLanguageDescriptor(database, language));

                    var analyzer = new ValidationAnalyzer();
                    analyzer.Process(output, options);

                    output.Flush();
                    writer.Flush();
                    writer.Close();
                }

                File.Delete(fileName);
                File.Move(tempFileName, fileName);
            }
            finally
            {
                if (File.Exists(tempFileName))
                {
                    File.Delete(tempFileName);
                }
            }
        }
    }
}
