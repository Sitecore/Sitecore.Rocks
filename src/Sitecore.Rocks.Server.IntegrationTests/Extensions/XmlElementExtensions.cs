using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using Newtonsoft.Json;

namespace Sitecore.Rocks.Server.IntegrationTests.Extensions
{
    public static class XmlElementExtensions
    {
        public static dynamic ToDynamic(this XmlElement xml)
        {
            var doc = XDocument.Parse(xml.OuterXml);
            var jsonText = JsonConvert.SerializeXNode(doc);
            jsonText = jsonText.Replace("\"@", "\"");
            jsonText = jsonText.Replace("\"#text\"", "\"text\"");
            var dyn = JsonConvert.DeserializeObject<ExpandoObject>(jsonText);
            return dyn;
        }
    }
}
