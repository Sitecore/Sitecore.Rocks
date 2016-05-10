// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.CodeDom.Compiler;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using Sitecore.Configuration;
using Sitecore.Diagnostics;
using Sitecore.IO;
using Sitecore.Web.UI.WebControls;

namespace Sitecore.Rocks.Server.Requests.CodeGeneration
{
    public class XsltGenerator
    {
        [NotNull]
        public string Execute([NotNull] string databaseName, [NotNull] string xslt)
        {
            Assert.ArgumentNotNull(databaseName, nameof(databaseName));
            Assert.ArgumentNotNull(xslt, nameof(xslt));

            var database = Factory.GetDatabase(databaseName);
            if (database == null)
            {
                throw new Exception("Database not found");
            }

            var item = database.GetRootItem();

            var fileName = TempFolder.GetFilename("TransformCodeGeneration.xslt");
            var path = FileUtil.MapPath(fileName);

            File.WriteAllText(path, xslt, Encoding.UTF8);

            var xslFile = new XslFile
            {
                Path = fileName
            };

            xslFile.XslExtensions["http://www.sitecore.net/codegeneration"] = new Helper();

            var result = xslFile.Transform(item);

            File.Delete(path);

            return result;
        }

        public class Helper
        {
            public string GetValidIdentifier([NotNull] string name)
            {
                Assert.ArgumentNotNull(name, nameof(name));

                var regex = new Regex(@"[^\p{Ll}\p{Lu}\p{Lt}\p{Lo}\p{Nd}\p{Nl}\p{Mn}\p{Mc}\p{Cf}\p{Pc}\p{Lm}]");

                var safeName = regex.Replace(name, string.Empty);
                if (!char.IsLetter(safeName, 0) || !CodeDomProvider.CreateProvider("C#").IsValidIdentifier(safeName))
                {
                    safeName = string.Concat("_", safeName);
                }

                return safeName;
            }
        }
    }
}
