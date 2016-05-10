// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Extensions.XElementExtensions;
using Sitecore.Rocks.Shell;

namespace Sitecore.Rocks.CodeGeneration
{
    public partial class CodeGenerationConfigurator
    {
        public CodeGenerationConfigurator()
        {
            InitializeComponent();
        }

        [CanBeNull]
        public IEditorPane Pane { get; set; }

        [CanBeNull]
        protected ICodeGenerator Generator { get; private set; }

        [CanBeNull]
        protected string GeneratorName { get; private set; }

        public void Load([NotNull] string fileName)
        {
            Assert.ArgumentNotNull(fileName, nameof(fileName));

            XDocument doc;
            try
            {
                doc = XDocument.Load(fileName);
            }
            catch
            {
                return;
            }

            var root = doc.Root;
            if (root == null)
            {
                return;
            }

            var name = root.GetAttributeValue("Generator");
            if (string.IsNullOrEmpty(name))
            {
                return;
            }

            GeneratorName = name;

            Generator = CodeGenerationManager.GetGenerator(name);
            if (Generator == null)
            {
                return;
            }

            Generator.Pane = Pane;
            Generator.Load(root);

            var control = Generator.GetConfigurationUserControl();
            if (control == null)
            {
                control = new GenericConfigurator(this);
            }

            Configurator.Child = control;
        }

        public void RunCustomTool()
        {
            if (Pane == null)
            {
                return;
            }

            Pane.SetModifiedFlag(true);

            AppHost.Shell.SaveActiveDocument();
        }

        public void Save([NotNull] string fileName)
        {
            Assert.ArgumentNotNull(fileName, nameof(fileName));

            var generator = Generator;

            using (var writer = new FileStream(fileName, FileMode.Create, FileAccess.Write, FileShare.Read))
            {
                var output = new XmlTextWriter(writer, Encoding.UTF8)
                {
                    Formatting = Formatting.Indented,
                    Indentation = 2
                };

                output.WriteStartElement(@"CodeGeneration");

                if (generator != null && GeneratorName != null)
                {
                    output.WriteAttributeString(@"Generator", GeneratorName);

                    generator.Save(output);
                }

                output.WriteEndElement();

                output.Flush();
            }
        }
    }
}
