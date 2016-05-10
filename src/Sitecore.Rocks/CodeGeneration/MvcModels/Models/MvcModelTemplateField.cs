// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Xml;
using System.Xml.Linq;
using Sitecore.Rocks.CodeGeneration.TemplateClasses.Models;
using Sitecore.Rocks.Extensions.XElementExtensions;

namespace Sitecore.Rocks.CodeGeneration.MvcModels.Models
{
    public class MvcModelTemplateField : TemplateField
    {
        [Category("MVC"), Description("The MVC Data Type attribute."), DisplayName("Data Type")]
        public DataType DataType { get; set; }

        [Category("MVC"), Description("The MVC Range attribute."), DisplayName("Range End")]
        public int RangeEnd { get; set; }

        [Category("MVC"), Description("The MVC Range attribute."), DisplayName("Range Start")]
        public int RangeStart { get; set; }

        [Category("MVC"), Description("The MVC RegularExpression attribute."), DisplayName("Regular Expression")]
        public string RegularExpression { get; set; }

        [Category("MVC"), Description("The MVC Required attribute."), DisplayName("Required")]
        public bool Required { get; set; }

        [Category("MVC"), Description("The MVC StringLength attribute."), DisplayName("String Length")]
        public int StringLength { get; set; }

        public override void Load(XElement element)
        {
            base.Load(element);

            Required = element.GetAttributeValue("required") == "true";
            StringLength = element.GetAttributeInt("stringlength", 0);
            RangeStart = element.GetAttributeInt("rangestart", 0);
            RangeEnd = element.GetAttributeInt("rangeend", 0);
            RegularExpression = element.GetAttributeValue("regularexpression");

            DataType e;
            var s = element.GetAttributeValue("datatype");
            if (Enum.TryParse(s, out e))
            {
                DataType = e;
            }
        }

        public override void Parse(XElement element)
        {
            base.Parse(element);

            Required = element.GetAttributeValue("required") == "true";
            StringLength = element.GetAttributeInt("stringlength", 0);
            RangeStart = element.GetAttributeInt("rangestart", 0);
            RangeEnd = element.GetAttributeInt("rangeend", 0);
            RegularExpression = element.GetAttributeValue("regularexpression");

            DataType e;
            var s = element.GetAttributeValue("datatype");
            if (Enum.TryParse(s, out e))
            {
                DataType = e;
            }
        }

        protected override void SaveFields(XmlTextWriter output)
        {
            base.SaveFields(output);

            output.WriteAttributeString("required", Required ? "true" : "false");
            output.WriteAttributeString("datatype", DataType.ToString());
            output.WriteAttributeString("stringlength", StringLength.ToString(CultureInfo.CurrentCulture));
            output.WriteAttributeString("regularexpression", RegularExpression);
            output.WriteAttributeString("rangestart", RangeStart.ToString(CultureInfo.CurrentCulture));
            output.WriteAttributeString("rangeend", RangeEnd.ToString(CultureInfo.CurrentCulture));
        }
    }
}
