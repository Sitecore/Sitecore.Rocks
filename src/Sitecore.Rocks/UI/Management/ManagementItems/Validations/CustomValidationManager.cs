// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Diagnostics;

namespace Sitecore.Rocks.UI.Management.ManagementItems.Validations
{
    public static class CustomValidationManager
    {
        private static readonly List<CustomValidation> validations = new List<CustomValidation>();

        static CustomValidationManager()
        {
            Load();
        }

        [NotNull]
        public static IEnumerable<CustomValidation> CustomValidations
        {
            get { return validations; }
        }

        public static void Add([NotNull] CustomValidation customValidation)
        {
            Assert.ArgumentNotNull(customValidation, nameof(customValidation));

            customValidation.FileName = Path.ChangeExtension(customValidation.Title.Replace("\\W", string.Empty), ".xml");

            Save(customValidation);

            validations.Add(customValidation);
        }

        public static void Delete([NotNull] CustomValidation customValidation)
        {
            Assert.ArgumentNotNull(customValidation, nameof(customValidation));

            var fileName = Path.Combine(GetFolder(), customValidation.FileName);
            File.Delete(fileName);

            validations.Remove(customValidation);
        }

        [NotNull]
        public static string GetFolder()
        {
            var result = Path.Combine(AppHost.User.SharedFolder, "Validations");

            Directory.CreateDirectory(result);

            return result;
        }

        public static void Invalidate()
        {
            validations.Clear();
            Load();
        }

        public static void Load()
        {
            Load(GetFolder());
        }

        public static void Update([NotNull] CustomValidation customValidation)
        {
            Assert.ArgumentNotNull(customValidation, nameof(customValidation));

            Save(customValidation);
        }

        private static void Load([NotNull] string folder)
        {
            Debug.ArgumentNotNull(folder, nameof(folder));

            foreach (var fileName in AppHost.Files.GetFiles(folder, "*.xml"))
            {
                XDocument doc;
                try
                {
                    doc = XDocument.Load(fileName);
                }
                catch (Exception ex)
                {
                    Trace.TraceError(ex.Message);
                    continue;
                }

                var root = doc.Root;
                if (root == null)
                {
                    continue;
                }

                var customValidation = new CustomValidation();
                customValidation.Load(root);
                customValidation.FileName = Path.GetFileName(fileName);

                validations.Add(customValidation);
            }

            foreach (var subfolder in AppHost.Files.GetDirectories(folder))
            {
                Load(subfolder);
            }
        }

        private static void Save([NotNull] CustomValidation customValidation)
        {
            Debug.ArgumentNotNull(customValidation, nameof(customValidation));

            var fileName = Path.Combine(GetFolder(), customValidation.FileName);
            using (var output = new XmlTextWriter(fileName, Encoding.UTF8))
            {
                output.Formatting = Formatting.Indented;
                output.Indentation = 2;

                customValidation.Save(output);
            }
        }
    }
}
