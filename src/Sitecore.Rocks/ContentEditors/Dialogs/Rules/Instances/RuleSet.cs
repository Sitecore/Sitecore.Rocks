// --------------------------------------------------------------------------------------------------------------------
// <copyright file="RuleSet.cs" company="Sitecore A/S">
//   Copyright (C) 2010 by Sitecore A/S
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Sitecore.VisualStudio.ContentEditors.Dialogs.Rules.Instances
{
  using System.Collections.Generic;
  using System.Xml.Linq;
  using Sitecore.VisualStudio.Annotations;
  using Sitecore.VisualStudio.Diagnostics;

  /// <summary>The rule field.</summary>
  public class RuleSet
  {
    #region Constructors and Destructors

    /// <summary>Initializes a new instance of the <see cref="RuleSet"/> class.</summary>
    public RuleSet()
    {
      this.Rules = new List<Rule>();
    }

    #endregion

    #region Properties

    /// <summary>
    /// Gets or sets the actions.
    /// </summary>
    /// <value>The actions.</value>
    public List<Rule> Rules { get; set; }

    #endregion

    #region Public Methods

    /// <summary>Parses the specified document.</summary>
    /// <param name="document">The document.</param>
    /// <returns>Returns the rule set.</returns>
    [NotNull]
    public static RuleSet Parse([NotNull] XDocument document)
    {
      Assert.ArgumentNotNull(document, "document");

      var result = new RuleSet();

      var element = document.Root;
      if (element == null)
      {
        return result;
      }

      foreach (var ruleElement in element.Elements("rules"))
      {
        var rule = Rule.Parse(ruleElement);

        result.Rules.Add(rule);
      }

      return result;
    }

    #endregion
  }
}