// © 2015 Sitecore Corporation A/S. All rights reserved.

using System.Collections.Generic;
using System.Runtime.Serialization;
using Newtonsoft.Json;

namespace Sitecore.Rocks.UI.LayoutDesigners.GridDesigner
{
  using System;

  /// <summary>
  /// GridAttributes class.
  /// </summary>
  [DataContract]
  public class GridAttributes
  {
    /// <summary>
    /// Gets or sets the number of cells.
    /// </summary>
    /// <value>
    /// The number of cells.
    /// </value>
    [JsonProperty("numberOfCells")]
    public int NumberOfCells { get; set; }

    /// <summary>
    /// Gets or sets the type of the layout.
    /// </summary>
    /// <value>
    /// The type of the layout.
    /// </value>
    [JsonProperty("layoutType")]
    public Guid LayoutType { get; set; }

    /// <summary>
    /// Gets or sets the size attributes.
    /// </summary>
    /// <value>
    /// The size attributes.
    /// </value>
    [JsonProperty("sizeAttributes")]
    public List<SizeAttributes> SizeAttributes { get; set; }
  }
}
