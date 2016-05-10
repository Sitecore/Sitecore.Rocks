// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.IO;
using System.Text;
using System.Xml;
using Sitecore.Diagnostics;
using Sitecore.IO;
using Sitecore.Rocks.Server.Jobs;
using Sitecore.Rocks.Server.Validations;

namespace Sitecore.Rocks.Server.Requests.Validations
{
    public class StartValidationAssessment
    {
        [NotNull]
        public string Execute([NotNull] string contextName, [NotNull] string databasesAndLanguages, [NotNull] string inactiveValidations, [NotNull] string customValidations)
        {
            Assert.ArgumentNotNull(contextName, nameof(contextName));
            Assert.ArgumentNotNull(databasesAndLanguages, nameof(databasesAndLanguages));
            Assert.ArgumentNotNull(inactiveValidations, nameof(inactiveValidations));
            Assert.ArgumentNotNull(customValidations, nameof(customValidations));

            return BackgroundJob.Run("Validation", "Management", () => Process(contextName, databasesAndLanguages, inactiveValidations, customValidations));
        }

        [NotNull]
        public string Process([NotNull] string contextName, [NotNull] string databasesAndLanguages, [NotNull] string inactiveValidations, [NotNull] string customValidations)
        {
            Assert.ArgumentNotNull(contextName, nameof(contextName));
            Assert.ArgumentNotNull(databasesAndLanguages, nameof(databasesAndLanguages));
            Assert.ArgumentNotNull(inactiveValidations, nameof(inactiveValidations));
            Assert.ArgumentNotNull(customValidations, nameof(customValidations));

            TempFolder.EnsureFolder();
            var fileName = Path.Combine(FileUtil.MapPath(TempFolder.Folder), contextName + "_validation.xml");
            var tempFileName = fileName + ".tmp";

            try
            {
                using (var writer = new FileStream(tempFileName, FileMode.Create, FileAccess.Write, FileShare.None))
                {
                    var output = new XmlTextWriter(writer, Encoding.UTF8)
                    {
                        Formatting = Formatting.Indented,
                        Indentation = 2
                    };

                    var options = new ValidationAnalyzerOptions
                    {
                        ContextName = contextName,
                        InactiveValidations = inactiveValidations,
                        CustomValidations = customValidations
                    };

                    options.ParseDatabaseAndLanguages(databasesAndLanguages);

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

            return fileName;
        }
    }
}
