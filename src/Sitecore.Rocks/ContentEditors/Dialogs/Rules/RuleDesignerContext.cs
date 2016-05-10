// --------------------------------------------------------------------------------------------------------------------
// <copyright file="RuleDesignerContext.cs" company="Sitecore A/S">
//   Copyright (C) 2010 by Sitecore A/S
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Sitecore.VisualStudio.ContentEditors.Dialogs.Rules
{
  using System.Windows.Controls;

  /// <summary>Defines the <see cref="RuleFieldDesignerContext"/> class.</summary>
  public class RuleFieldDesignerContext
  {
    #region Properties

    /// <summary>
    /// Gets or sets the description.
    /// </summary>
    /// <value>The description.</value>
    public ListBox Description { get; set; }

    /// <summary>
    /// Gets or sets the rule designer.
    /// </summary>
    /// <value>The rule designer.</value>
    public RuleFieldDesigner RuleFieldDesigner { get; set; }

    #endregion
  }
}