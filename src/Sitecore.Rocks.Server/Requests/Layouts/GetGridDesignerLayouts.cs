// © 2015 Sitecore Corporation A/S. All rights reserved.

using System;
using System.IO;
using System.Xml;
using Sitecore.Configuration;
using Sitecore.Data.Items;
using Sitecore.Diagnostics;

namespace Sitecore.Rocks.Server.Requests.Layouts
{
  public class GetGridDesignerLayouts
  {
    [NotNull]
    public string Execute([NotNull] string databaseName)
    {
      Assert.ArgumentNotNull(databaseName, "databaseName");

      var database = Factory.GetDatabase(databaseName);
      if (database == null)
      {
        throw new Exception("Database not found");
      }

      var item = database.GetItem("{41094694-5D1C-487A-90CF-79E0ECB2CBD3}");
      if (item == null)
      {
        return string.Empty;
      }

      var writer = new StringWriter();
      var output = new XmlTextWriter(writer);

      output.WriteStartElement("layouts");

      foreach (Item child in item.Children)
      {
        output.WriteStartElement("layout");
        output.WriteAttributeString("name", child.Name);
        output.WriteAttributeString("id", child.ID.ToString());
        output.WriteAttributeString("large", child["large"]);
        output.WriteAttributeString("medium", child["medium"]);
        output.WriteAttributeString("small", child["small"]);
        output.WriteAttributeString("xsmall", child["x-small"]);
        output.WriteEndElement();
      }

      output.WriteEndElement();

      return writer.ToString();
    }
  }
}
