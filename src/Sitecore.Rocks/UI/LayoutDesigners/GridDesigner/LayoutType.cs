// © 2015 Sitecore Corporation A/S. All rights reserved.

namespace Sitecore.Rocks.UI.LayoutDesigners.GridDesigner
{
  using System;

  /// <summary>
  /// LayoutTypes class.
  /// </summary>
  public class LayoutType
  {
    /// <summary>
    /// Gets or sets the identifier.
    /// </summary>
    /// <value>
    /// The identifier.
    /// </value>
    public Guid Id { get; set; }

    /// <summary>
    /// Gets or sets the name.
    /// </summary>
    /// <value>
    /// The name.
    /// </value>
    public string Name { get; set; }

    /// <summary>
    /// Large size 
    /// </summary>
    public int Large { get; set; }

    /// <summary>
    /// Large size 
    /// </summary>
    public int Medium { get; set; }

    /// <summary>
    /// Small size
    /// </summary>
    public int Small { get; set; }

    /// <summary>
    /// XSmall size
    /// </summary>
    public int XSmall { get; set; }
  }
}
