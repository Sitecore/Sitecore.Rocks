// --------------------------------------------------------------------------------------------------------------------
// <copyright file="RuleFieldActionDefinition.cs" company="Sitecore A/S">
//   Copyright (C) 2010 by Sitecore A/S
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Sitecore.VisualStudio.ContentEditors.Dialogs.Rules.Definitions
{
  using System;
  using Sitecore.VisualStudio.Annotations;
  using Sitecore.VisualStudio.Diagnostics;

  /// <summary>Defines the <see cref="RuleFieldActionDefinition"/> class.</summary>
  public class RuleFieldActionDefinition
  {
    #region Constructors and Destructors

    /// <summary>Initializes a new instance of the <see cref="RuleFieldActionDefinition"/> class.</summary>
    /// <param name="id">The id.</param>
    /// <param name="category">The category.</param>
    /// <param name="displayText">The display text.</param>
    public RuleFieldActionDefinition(Guid id, [NotNull] string category, [NotNull] string displayText)
    {
      Assert.ArgumentNotNull(category, "category");
      Assert.ArgumentNotNull(displayText, "displayText");

      this.ID = id;
      this.Category = category;
      this.DisplayText = displayText;
    }

    #endregion

    #region Properties

    /// <summary>
    /// Gets or sets the category.
    /// </summary>
    /// <value>The category.</value>
    [NotNull]
    public string Category { get; set; }

    /// <summary>
    /// Gets or sets the display text.
    /// </summary>
    /// <value>The display text.</value>
    [NotNull]
    public string DisplayText { get; set; }

    /// <summary>
    /// Gets or sets the ID.
    /// </summary>
    /// <value>The ID.</value>
    public Guid ID { get; set; }

    #endregion
  }
}