// --------------------------------------------------------------------------------------------------------------------
// <copyright file="RuleAction.cs" company="Sitecore A/S">
//   Copyright (C) 2010 by Sitecore A/S
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Sitecore.VisualStudio.ContentEditors.Dialogs.Rules.Instances
{
  using System;
  using System.Collections.Generic;

  /// <summary>Defines the <see cref="RuleAction"/> class.</summary>
  public class RuleAction
  {
    #region Constructors and Destructors

    /// <summary>Initializes a new instance of the <see cref="RuleAction"/> class.</summary>
    public RuleAction()
    {
      this.Parameters = new Dictionary<string, string>();
    }

    #endregion

    #region Properties

    /// <summary>
    /// Gets or sets the item id.
    /// </summary>
    /// <value>The item id.</value>
    public Guid ItemId { get; set; }

    /// <summary>
    /// Gets the parameters.
    /// </summary>
    /// <value>The parameters.</value>
    public Dictionary<string, string> Parameters { get; private set; }

    /// <summary>
    /// Gets or sets the unique id.
    /// </summary>
    /// <value>The unique id.</value>
    public Guid UniqueId { get; set; }

    #endregion
  }
}