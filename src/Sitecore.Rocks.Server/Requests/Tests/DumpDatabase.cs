// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DumpDatabase.cs" company="Sitecore A/S">
//   Copyright (C) 2010 by Sitecore A/S
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Sitecore.Rocks.Server.Requests.Tests
{
  using System;
  using System.IO;
  using System.Xml;
  using Sitecore.Configuration;
  using Sitecore.Data.Fields;
  using Sitecore.Data.Items;
  using Sitecore.Diagnostics;
  using Sitecore.IO;

  /// <summary>The change template.</summary>
  [UsedImplicitly]
  public class DumpDatabase
  {
    #region Public Methods

    /// <summary>
    /// Executes the specified output.
    /// </summary>
    /// <param name="databaseName">Name of the database.</param>
    /// <returns>The result</returns>
    /// <exception cref="Exception">Item not found</exception>
    [NotNull]
    [UsedImplicitly]
    public string Execute([NotNull] string databaseName)
    {
      Assert.ArgumentNotNull(databaseName, "databaseName");

      var database = Factory.GetDatabase(databaseName);
      if (database == null)
      {
        throw new Exception("Database not found");
      }

      var writer = new StringWriter();
      var output = new XmlTextWriter(writer);

      var item = database.GetRootItem();

      this.Write(output, item);

      FileUtil.WriteToFile("e:\\db.xml", writer.ToString());

      return string.Empty;
    }

    #endregion

    #region Methods

    /// <summary>Writes the specified output.</summary>
    /// <param name="output">The output.</param>
    /// <param name="item">The item.</param>
    private void Write([NotNull] XmlTextWriter output, [NotNull] Item item)
    {
      Debug.ArgumentNotNull(output, "output");
      Debug.ArgumentNotNull(item, "item");

      output.WriteStartElement("item");

      output.WriteAttributeString("id", item.ID.ToString());
      output.WriteAttributeString("parentid", item.ParentID.ToString());
      output.WriteAttributeString("name", item.Name);
      output.WriteAttributeString("templateid", item.TemplateID.ToString());
      output.WriteAttributeString("templatename", item.TemplateName);
      output.WriteAttributeString("sortorder", item.Appearance.Sortorder.ToString());
      output.WriteAttributeString("icon", item.Appearance.Icon);

      foreach (Field field in item.Fields)
      {
        output.WriteStartElement("field");

        output.WriteAttributeString("id", field.ID.ToString());
        output.WriteAttributeString("name", field.Name);
        output.WriteAttributeString("value", field.Value);

        output.WriteEndElement();
      }

      foreach (Item child in item.Children)
      {
        this.Write(output, child);
      }

      output.WriteEndElement();
    }

    #endregion
  }
}