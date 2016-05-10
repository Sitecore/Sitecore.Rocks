// --------------------------------------------------------------------------------------------------------------------
// <copyright file="RuleFieldConditionDefinition.cs" company="Sitecore A/S">
//   Copyright (C) 2010 by Sitecore A/S
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Sitecore.VisualStudio.ContentEditors.Dialogs.Rules.Definitions
{
  using System;
  using Sitecore.VisualStudio.Annotations;
  using Sitecore.VisualStudio.Diagnostics;

  /// <summary>Defines the <see cref="RuleFieldConditionDefinition"/> class.</summary>
  public class RuleFieldConditionDefinition
  {
    #region Constructors and Destructors

    /// <summary>Initializes a new instance of the <see cref="RuleFieldConditionDefinition"/> class.</summary>
    /// <param name="id">The id.</param>
    /// <param name="category">The category.</param>
    /// <param name="displayText">The display text.</param>
    public RuleFieldConditionDefinition(Guid id, [NotNull] string category, [NotNull] string displayText)
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
    /// Gets or sets the display name.
    /// </summary>
    /// <value>The display name.</value>
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