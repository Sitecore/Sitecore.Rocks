// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Rule.cs" company="Sitecore A/S">
//   Copyright (C) 2010 by Sitecore A/S
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Sitecore.VisualStudio.ContentEditors.Dialogs.Rules.Instances
{
  using System;
  using System.Collections.Generic;
  using System.Xml.Linq;
  using Sitecore.VisualStudio.Annotations;
  using Sitecore.VisualStudio.Diagnostics;
  using Sitecore.VisualStudio.Extensions.XElementExtensions;

  /// <summary>The rule field.</summary>
  public class Rule
  {
    #region Constructors and Destructors

    /// <summary>Initializes a new instance of the <see cref="Rule"/> class.</summary>
    public Rule()
    {
      this.Actions = new List<RuleAction>();
    }

    #endregion

    #region Properties

    /// <summary>
    /// Gets or sets the actions.
    /// </summary>
    /// <value>The actions.</value>
    public List<RuleAction> Actions { get; set; }

    /// <summary>
    /// Gets or sets the conditions.
    /// </summary>
    /// <value>The conditions.</value>
    public RuleFieldCondition Condition { get; set; }

    #endregion

    /// <summary>Parses the specified element.</summary>
    /// <param name="element">The element.</param>
    /// <returns>Returns the rule.</returns>
    [NotNull]
    public static Rule Parse([NotNull] XElement element)
    {
      Assert.ArgumentNotNull(element, "element");

      var rule = new Rule();

      ParseCondition(rule, element);
      ParseActions(rule, element);

      return rule;
    }

    /// <summary>Parses the actions.</summary>
    /// <param name="rule">The rule.</param>
    /// <param name="element">The element.</param>
    private static void ParseActions([NotNull] Rule rule, [NotNull] XElement element)
    {
      Debug.ArgumentNotNull(rule, "rule");
      Debug.ArgumentNotNull(element, "element");

      foreach (var action in element.Elements("actions"))
      {
        var a = new RuleAction
        {
          ItemId = new Guid(action.GetAttributeValue("id")),
          UniqueId = new Guid(action.GetAttributeValue("uid"))
        };

        foreach (var attribute in action.Attributes())
        {
          if (attribute.Name == "id" || attribute.Name == "uid")
          {
            continue;
          }

          a.Parameters[attribute.Name.ToString()] = attribute.Value;
        }

        rule.Actions.Add(a);
      }
    }
  }
}