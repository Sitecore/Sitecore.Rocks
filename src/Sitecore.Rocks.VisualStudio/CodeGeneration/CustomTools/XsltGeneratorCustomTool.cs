// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;
using Microsoft.VisualStudio.Shell;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Data.DataServices;
using Sitecore.Rocks.Diagnostics;

namespace Sitecore.Rocks.CodeGeneration.CustomTools
{
    [ComVisible(true), Guid(@"db80f93c-0a95-4bc8-8fa5-4b26795161be"), CustomToolRegistration(@"SitecoreServerTransformation", typeof(XsltGeneratorCustomTool)), ProvideObject(typeof(XsltGeneratorCustomTool))]
    public class XsltGeneratorCustomTool : CustomTool
    {
        protected override string GetOutput()
        {
            var site = GetSite();
            if (site == null)
            {
                return "// The Visual Studio project is not connected to a Sitecore web site. Right-click the project, select Sitecore and Connect.";
            }

            var databaseName = GetDatabaseName(InputFileContents);

            string result = null;

            ExecuteCompleted completed = delegate(string response, ExecuteResult executeResult)
            {
                if (!DataService.HandleExecute(response, executeResult))
                {
                    result = string.Empty;
                    return;
                }

                result = response ?? string.Empty;
            };

            site.DataService.ExecuteAsync("CodeGeneration.XsltGenerator", completed, databaseName, InputFileContents);

            while (result == null)
            {
                AppHost.DoEvents();
            }

            return result;
        }

        [Localizable(false), NotNull]
        private string GetDatabaseName([NotNull] string inputFileContents)
        {
            Debug.ArgumentNotNull(inputFileContents, nameof(inputFileContents));

            XDocument doc;
            try
            {
                doc = XDocument.Parse(inputFileContents);
            }
            catch
            {
                return "master";
            }

            var resolver = new XmlNamespaceManager(new NameTable());
            resolver.AddNamespace("xsl", "http://www.w3.org/1999/XSL/Transform");

            var element = doc.XPathSelectElement(@"/xsl:stylesheet/xsl:variable[@name='sourceDatabase']", resolver);
            if (element == null)
            {
                return @"master";
            }

            return element.Value;
        }
    }
}
