// © 2015 Sitecore Corporation A/S. All rights reserved.

using System.Runtime.Serialization;
using Newtonsoft.Json;

namespace Sitecore.Rocks.UI.LayoutDesigners.GridDesigner
{
  /// <summary>
  /// CellAttributes class.
  /// </summary>
  [DataContract]
  public class CellAttributes
  {
    /// <summary>
    /// Gets or sets the index.
    /// </summary>
    /// <value>
    /// The index.
    /// </value>
    [JsonProperty("index")]
    public int Index { get; set; }

    /// <summary>
    /// Gets or sets the span.
    /// </summary>
    /// <value>
    /// The span.
    /// </value>
    [JsonProperty("span")]
    public int Span { get; set; }
  }
}
