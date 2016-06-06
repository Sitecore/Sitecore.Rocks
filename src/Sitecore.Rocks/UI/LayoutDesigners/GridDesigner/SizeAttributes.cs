// © 2015 Sitecore Corporation A/S. All rights reserved.

using System.Collections.Generic;
using System.Runtime.Serialization;
using Newtonsoft.Json;

namespace Sitecore.Rocks.UI.LayoutDesigners.GridDesigner
{
  /// <summary>
  /// SizeAttributes class.
  /// </summary>
  [DataContract]
  public class SizeAttributes
  {
    /// <summary>
    /// Gets or sets the cell attributes.
    /// </summary>
    /// <value>
    /// The cell attributes.
    /// </value>
    [JsonProperty("cellAttributes")]
    public List<CellAttributes> CellAttributes { get; set; }

    /// <summary>
    /// Gets or sets the number of columns.
    /// </summary>
    /// <value>
    /// The number of columns.
    /// </value>
    [JsonProperty("numberOfColumns")]
    public int NumberOfColumns { get; set; }

    /// <summary>
    /// Gets or sets the size.
    /// </summary>
    /// <value>
    /// The size.
    /// </value>
    [JsonProperty("size")]
    public string Size { get; set; }
  }
}
