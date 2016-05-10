// © 2015 Sitecore Corporation A/S. All rights reserved.

using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Xml;

namespace SItecore.Rocks.IconGen
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            const string Folder = "E:\\Sitecore\\Icons\\Apps";

            var fileName = Path.GetFileNameWithoutExtension(Folder);
            var fileNames = Directory.GetFiles(Folder).OrderBy(n => Path.GetFileName(n)).ToList();

            var writer = new StringWriter();
            var output = new XmlTextWriter(writer);
            output.Formatting = Formatting.Indented;

            output.WriteStartElement("map");
            output.WriteAttributeString("name", fileName);

            var maxY = (fileNames.Count / 24) + 1;

            var image = new Bitmap((24 * 40) + 8, (maxY * 40) + 8);
            using (var g = Graphics.FromImage(image))
            {
                var index = 0;
                for (var y = 0; y < maxY; y++)
                {
                    for (var x = 0; x < 24; x++)
                    {
                        var iconFileName = fileNames.ElementAt(index);

                        var name = Path.GetFileNameWithoutExtension(iconFileName).Replace("_", " ").Replace("-", " ");
                        if (name.StartsWith("flag ", StringComparison.InvariantCultureIgnoreCase))
                        {
                            name = name.Substring(5);
                        }

                        name = name.Substring(0, 1).ToUpperInvariant() + name.Substring(1);

                        var icon = Image.FromFile(iconFileName);

                        g.DrawImage(icon, (x * 40) + 4, (y * 40) + 4, 32, 32);

                        output.WriteStartElement("i");
                        output.WriteAttributeString("c", ((x * 40) + 4) + "," + ((y * 40) + 4));
                        output.WriteAttributeString("n", name);
                        output.WriteAttributeString("p", Path.GetFileNameWithoutExtension(iconFileName).ToLowerInvariant());
                        output.WriteEndElement();

                        index++;
                        if (index >= fileNames.Count)
                        {
                            break;
                        }
                    }
                }
            }

            output.WriteEndElement();

            File.WriteAllText(fileName + ".xml", writer.ToString());
            image.Save("icons_" + fileName + ".png", ImageFormat.Png);
        }
    }
}
