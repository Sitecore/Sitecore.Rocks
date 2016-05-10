// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.IO;
using System.Text;
using System.Web;
using Sitecore.Diagnostics;
using Sitecore.IO;

namespace Sitecore.Rocks.Server.Requests.CodeGeneration
{
    public class AspxGenerator
    {
        [NotNull]
        public string Execute([NotNull] string page, [NotNull] string name, [NotNull] string parameters)
        {
            Assert.ArgumentNotNull(page, nameof(page));
            Assert.ArgumentNotNull(name, nameof(name));
            Assert.ArgumentNotNull(parameters, nameof(parameters));

            var fileName = TempFolder.GetFilename(name);
            var path = Path.ChangeExtension(FileUtil.MapPath(fileName), ".aspx");

            File.WriteAllText(path, page, Encoding.UTF8);

            var writer = new StringWriter();

            var httpContext = HttpContext.Current;

            httpContext.Session["SC_CODEGENERATOR_PARAMETERS"] = parameters;

            httpContext.Server.Execute(fileName, writer);
            var result = writer.ToString();

            httpContext.Session.Remove("SC_CODEGENERATOR_PARAMETERS");

            File.Delete(path);

            return result;
        }
    }
}
