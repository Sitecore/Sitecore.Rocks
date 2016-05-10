// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Runtime.InteropServices;
using System.Xml.Linq;
using Microsoft.VisualStudio.Shell;
using Sitecore.Rocks.Extensions.XElementExtensions;

namespace Sitecore.Rocks.CodeGeneration
{
    [ComVisible(true), Guid(@"34a00137-c1f7-4368-aba0-47e8674b0be8"), CustomToolRegistration(@"SitecoreCodeGenerator", typeof(CodeGeneratorCustomTool)), ProvideObject(typeof(CodeGeneratorCustomTool))]
    public class CodeGeneratorCustomTool : CustomTool
    {
        protected override string GetOutput()
        {
            XDocument doc;

            try
            {
                doc = XDocument.Parse(InputFileContents);
            }
            catch
            {
                return "// The input file is not a valid XML document.";
            }

            var root = doc.Root;
            if (root == null)
            {
                return "// The input file is not a valid XML document.";
            }

            var name = root.GetAttributeValue("Generator");
            if (string.IsNullOrEmpty(name))
            {
                return "// The Generator attribute on the root element is missing.";
            }

            var generator = CodeGenerationManager.GetGenerator(name);
            if (generator == null)
            {
                return "// The expected generator \"{0}\" was not found.";
            }

            generator.Load(root);

            var site = GetSite();
            var fileName = InputFilePath;

            return generator.GetOutput(fileName, site);
        }
    }
}
