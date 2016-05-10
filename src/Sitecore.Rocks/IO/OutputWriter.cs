// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.IO;
using System.Xml;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Diagnostics;

namespace Sitecore.Rocks.IO
{
    public class OutputWriter : XmlTextWriter
    {
        private readonly StringWriter _stringWriter;

        public OutputWriter([NotNull] StringWriter stringWriter) : base(stringWriter)
        {
            Assert.ArgumentNotNull(stringWriter, nameof(stringWriter));

            _stringWriter = stringWriter;
            Formatting = Formatting.Indented;
        }

        public override string ToString()
        {
            return _stringWriter.ToString();
        }
    }
}
