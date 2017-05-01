// © 2015-2017 Sitecore Corporation A/S. All rights reserved.

using System.IO;

namespace Sitecore.Rocks.Server.IO
{
    public class YamlTextWriter
    {
        public YamlTextWriter([NotNull] TextWriter innerWriter)
        {
            InnerWriter = innerWriter;
        }

        public YamlTextWriter([NotNull] TextWriter innerWriter, int indent)
        {
            InnerWriter = innerWriter;
            Indent = indent;
        }

        public int Indent { get; protected set; }

        public int Indentation { get; set; } = 4;

        [NotNull]
        protected TextWriter InnerWriter { get; }

        public void WriteAttributeString([NotNull] string key, [NotNull] string value = "")
        {
            InnerWriter.Write(new string(' ', Indent * Indentation));
            InnerWriter.Write(key);
            InnerWriter.Write(": ");

            WriteValue(value);
        }

        public void WriteAttributeStringIf([NotNull] string key, [NotNull] string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return;
            }

            InnerWriter.Write(new string(' ', Indent * Indentation));
            InnerWriter.Write(key);
            InnerWriter.Write(": ");

            WriteValue(value);
        }

        public void WriteAttributeStringIf([NotNull] string key, int value, int defaultValue = 0)
        {
            if (value == defaultValue)
            {
                return;
            }

            InnerWriter.Write(new string(' ', Indent * Indentation));
            InnerWriter.Write(key);
            InnerWriter.Write(": ");
            InnerWriter.WriteLine(value);
        }

        public void WriteAttributeStringIf([NotNull] string key, bool value, bool defaultValue = true)
        {
            if (value == defaultValue)
            {
                return;
            }

            InnerWriter.Write(new string(' ', Indent * Indentation));
            InnerWriter.Write(key);
            InnerWriter.Write(": ");
            InnerWriter.WriteLine(value);
        }

        public void WriteEndElement()
        {
            Indent--;
        }

        public void WriteStartElement([NotNull] string key, [NotNull] string value = "")
        {
            if (!key.StartsWith("-"))
            {
                key = "- " + key;
            }

            InnerWriter.Write(new string(' ', Indent * Indentation));
            InnerWriter.Write(key);
            InnerWriter.Write(": ");
            InnerWriter.WriteLine(value);
            Indent++;
        }

        private void WriteValue(string value)
        {
            if (value.IndexOf('\r') >= 0 || value.IndexOf('\n') >= 0)
            {
                value = value.Replace('\n', '\r').Replace("\r\r", "\r");

                InnerWriter.WriteLine('|');

                Indent++;
                foreach (var line in value.Split('\r'))
                {
                    InnerWriter.Write(new string(' ', Indent * Indentation));
                    InnerWriter.WriteLine(line);
                }

                Indent--;
            }
            else
            {
                InnerWriter.WriteLine(value);
            }
        }
    }
}
