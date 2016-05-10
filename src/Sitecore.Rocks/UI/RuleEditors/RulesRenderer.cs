namespace Sitecore.VisualStudio.UI.RuleEditors
{
  using System;
  using System.Windows;
  using System.Windows.Controls;
  using System.Windows.Media;
  using System.Xml.Linq;
  using Sitecore.VisualStudio.Annotations;
  using Sitecore.VisualStudio.Diagnostics;
  using Sitecore.VisualStudio.Extensions.XElementExtensions;

  /// <summary>
  ///   Defines the <see cref="RulesRenderer" /> class.
  /// </summary>
  public class RulesRenderer
  {
    #region Constants and Fields

    /// <summary>
    ///   The rules field.
    /// </summary>
    private readonly XDocument rules;

    #endregion

    #region Constructors and Destructors

    /// <summary>
    ///   Initializes a new instance of the <see cref="RulesRenderer" /> class.
    /// </summary>
    /// <param name="doc"> The doc. </param>
    public RulesRenderer([NotNull] XDocument doc)
    {
      Assert.ArgumentNotNull(doc, "doc");

      this.rules = doc;
    }

    /// <summary>
    /// Gets or sets a value indicating whether this instance is editable.
    /// </summary>
    /// <value><c>true</c> if this instance is editable; otherwise, <c>false</c>.</value>
    public bool IsEditable { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether [allow multiple].
    /// </summary>
    /// <value><c>true</c> if [allow multiple]; otherwise, <c>false</c>.</value>
    public bool AllowMultiple { get; set; }

    /// <summary>
    /// Gets or sets the current rule id.
    /// </summary>
    /// <value>The current rule id.</value>
    public Guid CurrentRuleId { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether [skip actions].
    /// </summary>
    /// <value><c>true</c> if [skip actions]; otherwise, <c>false</c>.</value>
    public bool SkipActions { get; set; }

    #endregion

    #region Public Methods

    /// <summary>
    ///   Renders the specified output.
    /// </summary>
    /// <param name="output"> The output. </param>
    public void Render([NotNull] TextBlockWriter output)
    {
      Assert.ArgumentNotNull(output, "output");

      output.Clear();

      var root = this.rules.Root;
      if (root == null)
      {
        this.RenderNoRules(output);
        return;
      }

      this.RenderRules(output, root);
    }

    /// <summary>Renders the rules.</summary>
    /// <param name="output">The output.</param>
    /// <param name="root">The root.</param>
    private void RenderRules([NotNull] TextBlockWriter output, [NotNull] XElement root)
    {
      Debug.ArgumentNotNull(output, "output");
      Debug.ArgumentNotNull(root, "root");

      var index = 1;

      foreach (var rule in root.Elements("rule"))
      {
        this.RenderRule(output, rule, index);

        index++;
      }

      if (index == 1)
      {
        this.RenderNoRules(output);
      }

      if (index > 1)
      {
        this.RenderAddNewRule(output);
      }
    }

    /// <summary>Renders the rule.</summary>
    /// <param name="output">The output.</param>
    /// <param name="rule">The rule.</param>
    /// <param name="index">The index.</param>
    private void RenderRule([NotNull] TextBlockWriter output, [NotNull] XElement rule, int index)
    {
      Debug.ArgumentNotNull(output, "output");
      Debug.ArgumentNotNull(rule, "rule");

      Guid id;
      var uid = rule.GetAttributeValue("uid");

      if (!Guid.TryParse(uid, out id))
      {
        id = Guid.Empty;
      }

      var isClosed = this.IsEditable && this.AllowMultiple && this.CurrentRuleId != Guid.Empty && this.CurrentRuleId != id;
      var ruleId = "rule_" + id.ToString("B").ToUpperInvariant();
      var title = this.GetRuleTitle(rule, index);
      var style = isClosed ? Visibility.Collapsed : Visibility.Visible;

      this.RenderRuleTitle(output, uid, title, ruleId, isClosed);

      output.WriteStartBlock(padding: new Thickness(16, 0, 0, 0), visibility: style);

      var inner = new TextBlockWriter(new TextBlock());

      this.RenderConditions(inner, rule);
      if (!this.SkipActions)
      {
        this.RenderActions(inner, rule);
      }

      if (!inner.IsEmpty)
      {
        output.Write(inner);
      }
      else
      {
        output.WriteStartBlock(padding: new Thickness(0, 4, 0, 2));
        output.Write("This rule has no conditions.", Brushes.DarkGray);
        output.WriteEndBlock();
      }

      output.WriteEndBlock();

      if (this.AllowMultiple)
      {
        output.WriteLine("--------------------------------------------------------");
      }
    }

    /// <summary>Renders the conditions.</summary>
    /// <param name="output">The output.</param>
    /// <param name="rule">The rule.</param>
    private void RenderConditions([NotNull] TextBlockWriter output, [NotNull] XElement rule)
    {
      Debug.ArgumentNotNull(output, "output");
      Debug.ArgumentNotNull(rule, "rule");

      var conditions = rule.Element("conditions");
      if (conditions == null)
      {
        return;
      }

      var element = conditions.Element(0);
      if (element == null)
      {
        return;
      }

      var leftLeaf = GetLeftLeaf(element);

      var uid = leftLeaf.GetAttributeValue("uid");
      if (string.IsNullOrEmpty(uid))
      {
        return;
      }

      output.WriteStartBlock();

      if (this.IsEditable)
      {
        // RenderConditionButtons(output, uid);
      }

      this.RenderConditionsRecursive(output, element);

      output.WriteEndBlock();
    }

    /// <summary>Renders the conditions recursive.</summary>
    /// <param name="output">The output.</param>
    /// <param name="parent">The parent.</param>
    private void RenderConditionsRecursive([NotNull] TextBlockWriter output, [NotNull] XElement parent)
    {
      Debug.ArgumentNotNull(output, "output");
      Debug.ArgumentNotNull(parent, "parent");

      if (parent.Name.LocalName == "condition")
      {
        this.RenderCondition(output, parent);
        return;
      }

      if (parent.Name.LocalName == "not")
      {
        var operand = parent.Element(0);
        if (operand == null)
        {
          return;
        }

        this.RenderUnaryOperator(output, parent);

        this.RenderConditionsRecursive(output, operand);

        return;
      }

      var left = parent.Element(0);
      if (left == null)
      {
        return;
      }

      var right = parent.Element(1);
      if (right == null)
      {
        return;
      }

      this.RenderConditionsRecursive(output, left);

      this.RenderBinaryOperator(output, parent);

      this.RenderConditionsRecursive(output, right);
    }

    /// <summary>Renders the condition.</summary>
    /// <param name="output">The output.</param>
    /// <param name="condition">The condition.</param>
    private void RenderCondition([NotNull] TextBlockWriter output, [NotNull] XElement condition)
    {
      Assert.ArgumentNotNull(output, "output");
      Assert.ArgumentNotNull(condition, "condition");

      var id = condition.GetAttributeValue("id");
      if (string.IsNullOrEmpty(id))
      {
        return;
      }

      var uid = condition.GetAttributeValue("uid");
      if (string.IsNullOrEmpty(uid))
      {
        return;
      }

      Item item;
      using (new SecurityDisabler())
      {
        item = Client.ContentDatabase.GetItem(id);
      }

      if (item == null)
      {
        output.Write(Translate.Text(Texts.UnknownCondition0, id));
        return;
      }

      var itemText = this.GetItemText(item);

      var text = this.RenderPrefix(output, condition, itemText);

      this.RenderText(output, condition, text);
    }

    /// <summary>Renders the binary operator.</summary>
    /// <param name="output">The output.</param>
    /// <param name="operatorElement">The operator element.</param>
    private void RenderBinaryOperator([NotNull] TextBlockWriter output, [NotNull] XElement operatorElement)
    {
      Debug.ArgumentNotNull(output, "output");
      Debug.ArgumentNotNull(operatorElement, "operatorElement");

      var op = operatorElement.Name.LocalName;

      // var uid = operatorElement.GetAttributeValue("uid");
      // var click = StringUtil.EscapeQuote("ToggleOperator(\"" + uid + "\")");

      var right = operatorElement.Element(1);
      if (right == null)
      {
        return;
      }

      // var leftLeaf = GetLeftLeaf(right);
      // var conditionUid = leftLeaf.GetAttributeValue("uid");

      output.WriteEndBlock();
      output.WriteStartBlock();

      if (op == "and")
      {
        output.Write("    ");
      }

      if (this.IsEditable)
      {
        output.WriteHyperlink(op, this.ToggleOperator);
      }
      else
      {
        output.Write(op);
      }

      output.Write(" ");
    }

    /// <summary>Renders the unary operator.</summary>
    /// <param name="output">The output.</param>
    /// <param name="operatorElement">The operator element.</param>
    private void RenderUnaryOperator([NotNull] TextBlockWriter output, [NotNull] XElement operatorElement)
    {
      Debug.ArgumentNotNull(output, "output");
      Debug.ArgumentNotNull(operatorElement, "operatorElement");

      var op = operatorElement.Name.LocalName;

      if (this.IsEditable)
      {
        output.WriteHyperlink(op, ToggleOperator);
      }
      else
      {
        output.Write(op);
      }
    }

    /// <summary>Toggles the operator.</summary>
    /// <param name="sender">The sender.</param>
    /// <param name="e">The <see cref="System.Windows.RoutedEventArgs"/> instance containing the event data.</param>
    private void ToggleOperator([NotNull] object sender, [NotNull] RoutedEventArgs e)
    {
      Debug.ArgumentNotNull(sender, "sender");
      Debug.ArgumentNotNull(e, "e");
    }

    /// <summary>Gets the left leaf.</summary>
    /// <param name="element">The element.</param>
    /// <returns>Returns the left leaf element.</returns>
    [NotNull]
    private static XElement GetLeftLeaf([NotNull] XElement element)
    {
      Debug.ArgumentNotNull(element, "element");

      var left = element.Element(0);
      return left != null ? GetLeftLeaf(left) : element;
    }

    /// <summary>Renders the rule title.</summary>
    /// <param name="output">The output.</param>
    /// <param name="uid">The uid.</param>
    /// <param name="title">The title.</param>
    /// <param name="ruleId">The rule id.</param>
    /// <param name="isClosed">if set to <c>true</c> [is closed].</param>
    private void RenderRuleTitle([NotNull] TextBlockWriter output, [NotNull] string uid, [NotNull] string title, [NotNull] string ruleId, bool isClosed)
    {
      Debug.ArgumentNotNull(output, "output");
      Debug.ArgumentNotNull(uid, "uid");
      Debug.ArgumentNotNull(title, "title");
      Debug.ArgumentNotNull(ruleId, "ruleId");

      if (!this.IsEditable || !this.AllowMultiple)
      {
        output.WriteStartBlock();
        output.Write(title);
        output.WriteEndBlock();

        return;
      }

      // var activeStyle = isClosed ? string.Empty : " scRuleActive";
      output.WriteStartBlock();

      output.WriteHyperlink(title, ToggleRule);

      var stackPanel = new StackPanel
      {
        Orientation = Orientation.Horizontal
      };

      stackPanel.Children.Add(new Button { Content = "Edit" });
      stackPanel.Children.Add(new Button { Content = "Delete" });

      output.Write(stackPanel);

      output.WriteEndBlock();
    }

    /// <summary>Toggles the rule.</summary>
    /// <param name="sender">The sender.</param>
    /// <param name="e">The <see cref="System.Windows.RoutedEventArgs"/> instance containing the event data.</param>
    private void ToggleRule([NotNull] object sender, [NotNull] RoutedEventArgs e)
    {
      Debug.ArgumentNotNull(sender, "sender");
      Debug.ArgumentNotNull(e, "e");
    }

    /// <summary>Renders the no rules.</summary>
    /// <param name="output">The output.</param>
    private void RenderNoRules([NotNull] TextBlockWriter output)
    {
      Debug.ArgumentNotNull(output, "output");

      output.WriteStartBlock(padding: new Thickness(16, 0, 0, 0));
      output.Write("There are no rules defined.", Brushes.DarkGray);
      output.WriteEndBlock();
    }


    /// <summary>Gets the rule title.</summary>
    /// <param name="rule">The rule.</param>
    /// <param name="index">The index.</param>
    /// <returns>Returns the rule title.</returns>
    [NotNull]
    private string GetRuleTitle([NotNull] XElement rule, int index)
    {
      Debug.ArgumentNotNull(rule, "rule");

      var name = rule.GetAttributeValue("name");
      if (!string.IsNullOrEmpty(name))
      {
        return name;
      }

      return "Rule " + index;
    }

    /// <summary>Renders the add new rule.</summary>
    /// <param name="output">The output.</param>
    private void RenderAddNewRule([NotNull] TextBlockWriter output)
    {
      Debug.ArgumentNotNull(output, "output");

      if (!this.IsEditable || !this.AllowMultiple)
      {
        return;
      }

      output.WriteStartBlock(padding: new Thickness(16, 0, 0, 0));
      output.WriteHyperlink("Add a new rule", AddNewRule);
      output.WriteEndBlock();
    }

    /// <summary>Adds the new rule.</summary>
    /// <param name="sender">The sender.</param>
    /// <param name="e">The <see cref="System.Windows.RoutedEventArgs"/> instance containing the event data.</param>
    private void AddNewRule([NotNull] object sender, [NotNull] RoutedEventArgs e)
    {
      Debug.ArgumentNotNull(sender, "sender");
      Debug.ArgumentNotNull(e, "e");
    }

    #endregion
  }
}