// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.IO;
using System.Windows.Controls;
using System.Xml;
using System.Xml.Linq;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Data.DataServices;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Sites;

namespace Sitecore.Rocks.CodeGeneration.StronglyTypedItems
{
    [CodeGenerator("Strongly Typed Items"), Obsolete]
    public class StronglyTypedItemsCodeGenerator : CodeGenerator
    {
        public override Control GetConfigurationUserControl()
        {
            return null;
        }

        public override string GetOutput(string fileName, Site site)
        {
            Assert.ArgumentNotNull(fileName, nameof(fileName));

            if (site == null)
            {
                return "// The Visual Studio project is not connected to a Sitecore web site. Right-click the project, select Sitecore and Connect.";
            }

            string result = null;

            var aspx = Path.ChangeExtension(fileName, @".scx.aspx");
            if (!File.Exists(aspx))
            {
                return string.Empty;
            }

            var fileContents = AppHost.Files.ReadAllText(aspx);

            ExecuteCompleted completed = delegate(string response, ExecuteResult executeResult)
            {
                if (!DataService.HandleExecute(response, executeResult))
                {
                    result = string.Empty;
                    return;
                }

                result = response ?? string.Empty;
            };

            site.DataService.ExecuteAsync("CodeGeneration.AspxGenerator", completed, fileContents, Path.GetFileName(aspx), string.Empty);

            while (result == null)
            {
                AppHost.DoEvents();
            }

            return result;
        }

        public override void Load(XElement root)
        {
            Assert.ArgumentNotNull(root, nameof(root));
        }

        public override void Save(XmlTextWriter output)
        {
            Assert.ArgumentNotNull(output, nameof(output));
        }
    }
}
