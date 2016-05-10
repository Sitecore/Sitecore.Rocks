// --------------------------------------------------------------------------------------------------------------------
// <copyright file="RuleFieldDesigner.xaml.cs" company="Sitecore A/S">
//   Copyright (C) 2010 by Sitecore A/S
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Sitecore.VisualStudio.ContentEditors.Dialogs.Rules
{
  using System;
  using System.Collections.Generic;
  using System.Linq;
  using System.Windows;
  using System.Windows.Controls;
  using System.Windows.Documents;
  using System.Windows.Input;
  using System.Windows.Media;
  using System.Xml.Linq;
  using Sitecore.VisualStudio.Annotations;
  using Sitecore.VisualStudio.ContentEditors.Dialogs.Rules.Definitions;
  using Sitecore.VisualStudio.ContentEditors.Dialogs.Rules.Instances;
  using Sitecore.VisualStudio.Data;
  using Sitecore.VisualStudio.Data.DataServices;
  using Sitecore.VisualStudio.Diagnostics;
  using Sitecore.VisualStudio.Extensions.ContextMenuExtensions;
  using Sitecore.VisualStudio.Extensions.StringExtensions;
  using Sitecore.VisualStudio.Extensions.XElementExtensions;
  using Sitecore.VisualStudio.Rules;

  /// <summary>Interaction logic for RuleDesigner.xaml</summary>
  public partial class RuleFieldDesigner : IContextProvider
  {
    #region Constants and Fields

    /// <summary>The empty dictionary field.</summary>
    private static readonly Dictionary<string, string> emptyDictionary = new Dictionary<string, string>();

    #endregion

    #region Constructors and Destructors

    /// <summary>Initializes a new instance of the <see cref="RuleFieldDesigner"/> class.</summary>
    public RuleFieldDesigner()
    {
      this.InitializeComponent();

      this.ActionDescriptors = new List<RuleFieldActionDefinition>();
      this.ConditionDescriptors = new List<RuleFieldConditionDefinition>();
      this.RuleSet = new Instances.Rule();
    }

    #endregion

    #region Properties

    /// <summary>
    /// Gets or sets the database URI.
    /// </summary>
    /// <value>The database URI.</value>
    public DatabaseUri DatabaseUri { get; set; }

    /// <summary>
    /// Gets or sets the rule.
    /// </summary>
    /// <value>The rule.</value>
    public RuleSet RuleSet { get; set; }

    /// <summary>
    /// Gets or sets the actions.
    /// </summary>
    /// <value>The actions.</value>
    protected List<RuleFieldActionDefinition> ActionDescriptors { get; set; }

    /// <summary>
    /// Gets or sets the condition descriptors.
    /// </summary>
    /// <value>The condition descriptors.</value>
    protected List<RuleFieldConditionDefinition> ConditionDescriptors { get; set; }

    /// <summary>
    /// Gets or sets the parameter.
    /// </summary>
    /// <value>The parameter.</value>
    [CanBeNull]
    protected object Parameter { get; set; }

    #endregion

    #region Public Methods

    /// <summary>Initializes the specified rule.</summary>
    /// <param name="databaseUri">The database URI.</param>
    /// <param name="value">The value.</param>
    /// <param name="parameter">The parameter.</param>
    public void Initialize([NotNull] DatabaseUri databaseUri, [NotNull] string value, [CanBeNull] object parameter)
    {
      Assert.ArgumentNotNull(databaseUri, "databaseUri");
      Assert.ArgumentNotNull(value, "value");

      this.DatabaseUri = databaseUri;
      this.Parameter = parameter;

      this.LoadRule(value);
      this.LoadRules();
    }

    /// <summary>Loads the rule.</summary>
    /// <param name="value">The value.</param>
    private void LoadRule([NotNull] string value)
    {
      Debug.ArgumentNotNull(value, "value");

      if (string.IsNullOrEmpty(value))
      {
        return;
      }

      var element = value.ToXElement();
      if (element == null)
      {
        return;
      }
    }

    /// <summary>Renders the rule.</summary>
    /// <param name="rule">The rule.</param>
    public void RenderRule([NotNull] Instances.Rule rule)
    {
      Debug.ArgumentNotNull(rule, "rule");

      this.Description.Items.Clear();

      var isFirst = true;
      foreach (var descriptor in rule.ConditionDescriptors)
      {
        var listBoxItem = new ListBoxItem
        {
          Content = this.FormatCondition(descriptor, isFirst), 
          Tag = descriptor
        };

        this.Description.Items.Add(listBoxItem);
        isFirst = false;
      }

      for (var index = 0; index < rule.ActionDescriptors.Count; index++)
      {
        var descriptor = rule.ActionDescriptors[index];
        var listBoxItem = new ListBoxItem
        {
          Content = this.FormatAction(descriptor, index < rule.ActionDescriptors.Count - 1), 
          Tag = descriptor
        };

        this.Description.Items.Add(listBoxItem);
      }
    }

    #endregion

    #region Implemented Interfaces

    #region IContextProvider

    /// <summary>Gets the context.</summary>
    /// <returns>Returns the context.</returns>
    public object GetContext()
    {
      return new RuleFieldDesignerContext
      {
        RuleFieldDesigner = this
      };
    }

    #endregion

    #endregion

    #region Methods

    /// <summary>Conditions the filter text changed.</summary>
    /// <param name="sender">The sender.</param>
    /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
    private void ActionFilterTextChanged([NotNull] object sender, [NotNull] EventArgs e)
    {
      Debug.ArgumentNotNull(sender, "sender");
      Debug.ArgumentNotNull(e, "e");

      foreach (ListBoxItem item in this.Actions.Items)
      {
        if (!item.IsEnabled)
        {
          continue;
        }

        var descriptor = item.Tag as RuleManager.RuleActionInfo;
        if (descriptor == null)
        {
          continue;
        }

        var text = descriptor.Attribute.DisplayText;

        item.Visibility = text.IsFilterMatch(this.ActionSelectorFilter.Text) ? Visibility.Visible : Visibility.Collapsed;
      }

      var hasItems = false;

      for (var n = this.Actions.Items.Count - 1; n >= 0; n--)
      {
        var item = this.Actions.Items[n] as ListBoxItem;
        if (item == null)
        {
          continue;
        }

        if (!item.IsEnabled)
        {
          item.Visibility = hasItems ? Visibility.Visible : Visibility.Collapsed;
          hasItems = false;
          continue;
        }

        if (item.Visibility == Visibility.Visible)
        {
          hasItems = true;
        }
      }
    }

    /// <summary>Adds the action.</summary>
    /// <param name="sender">The sender.</param>
    /// <param name="e">The <see cref="System.Windows.Input.MouseButtonEventArgs"/> instance containing the event data.</param>
    private void AddAction([NotNull] object sender, [NotNull] MouseButtonEventArgs e)
    {
      Debug.ArgumentNotNull(sender, "sender");
      Debug.ArgumentNotNull(e, "e");

      var selectedItem = this.Actions.SelectedItem as ListBoxItem;
      if (selectedItem == null)
      {
        return;
      }

      var action = selectedItem.Tag as RuleManager.RuleActionInfo;
      if (action == null)
      {
        return;
      }

      var descriptor = new RuleActionDescriptor
      {
        Action = action.GetInstance()
      };

      /*
      this.Rule.ActionDescriptors.Add(descriptor);

      this.RenderRule(this.Rule);
      */
    }

    /// <summary>Adds the condition.</summary>
    /// <param name="sender">The sender.</param>
    /// <param name="e">The <see cref="System.Windows.Input.MouseButtonEventArgs"/> instance containing the event data.</param>
    private void AddCondition([NotNull] object sender, [NotNull] MouseButtonEventArgs e)
    {
      Debug.ArgumentNotNull(sender, "sender");
      Debug.ArgumentNotNull(e, "e");

      var selectedItem = this.Conditions.SelectedItem as ListBoxItem;
      if (selectedItem == null)
      {
        return;
      }

      var condition = selectedItem.Tag as RuleManager.RuleConditionInfo;
      if (condition == null)
      {
        return;
      }

      var descriptor = new RuleConditionDescriptor
      {
        Condition = condition.GetInstance(), 
        Operator = RuleConditionDescriptorOperator.And, 
        IsNot = false
      };

      /*
      this.Rule.ConditionDescriptors.Add(descriptor);

      this.RenderRule(this.Rule);
      */
    }

    /// <summary>Conditions the filter text changed.</summary>
    /// <param name="sender">The sender.</param>
    /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
    private void ConditionFilterTextChanged([NotNull] object sender, [NotNull] EventArgs e)
    {
      Debug.ArgumentNotNull(sender, "sender");
      Debug.ArgumentNotNull(e, "e");

      foreach (ListBoxItem item in this.Conditions.Items)
      {
        if (!item.IsEnabled)
        {
          continue;
        }

        var descriptor = item.Tag as RuleManager.RuleConditionInfo;
        if (descriptor == null)
        {
          continue;
        }

        var text = descriptor.Attribute.DisplayText;

        item.Visibility = text.IsFilterMatch(this.ConditionSelectorFilter.Text) ? Visibility.Visible : Visibility.Collapsed;
      }

      var hasItems = false;

      for (var n = this.Conditions.Items.Count - 1; n >= 0; n--)
      {
        var item = this.Conditions.Items[n] as ListBoxItem;
        if (item == null)
        {
          continue;
        }

        if (!item.IsEnabled)
        {
          item.Visibility = hasItems ? Visibility.Visible : Visibility.Collapsed;
          hasItems = false;
          continue;
        }

        if (item.Visibility == Visibility.Visible)
        {
          hasItems = true;
        }
      }
    }

    /// <summary>Edits the value.</summary>
    /// <param name="hyperlink">The hyperlink.</param>
    /// <param name="parameters">The parameters.</param>
    /// <param name="parts">The parts.</param>
    private void EditValue([NotNull] Hyperlink hyperlink, [NotNull] Dictionary<string, string> parameters, [NotNull] string[] parts)
    {
      Debug.ArgumentNotNull(hyperlink, "hyperlink");
      Debug.ArgumentNotNull(parameters, "parameters");
      Debug.ArgumentNotNull(parts, "parts");

      string value;
      if (!parameters.TryGetValue(parts[0], out value))
      {
        value = string.Empty;
      }

      var editorName = parts[1];

      var editor = RuleManager.GetParameterEditor(editorName);

      value = editor.GetValue(value, this.Parameter);

      hyperlink.Inlines.Clear();

      if (string.IsNullOrEmpty(value))
      {
        parameters.Remove(parts[0]);
        hyperlink.Inlines.Add(new Run(parts[3]));
      }
      else
      {
        parameters[parts[0]] = value;
        hyperlink.Inlines.Add(new Run(value));
      }
    }

    /// <summary>Formats the condition.</summary>
    /// <param name="descriptor">The descriptor.</param>
    /// <param name="isLast">if set to <c>true</c> [is last].</param>
    /// <returns>Returns the condition.</returns>
    [NotNull]
    private TextBlock FormatAction([NotNull] RuleActionDescriptor descriptor, bool isLast)
    {
      Debug.ArgumentNotNull(descriptor, "descriptor");

      var textBlock = new TextBlock();

      this.FormatText(textBlock, descriptor.DisplayText, descriptor.Parameters, true);

      if (isLast)
      {
        textBlock.Inlines.Add(new Run(@" and"));
      }

      return textBlock;
    }

    /// <summary>Formats the condition.</summary>
    /// <param name="descriptor">The descriptor.</param>
    /// <param name="isFirst">if set to <c>true</c> [is first].</param>
    /// <returns>Returns the condition.</returns>
    [NotNull]
    private TextBlock FormatCondition([NotNull] RuleConditionDescriptor descriptor, bool isFirst)
    {
      Debug.ArgumentNotNull(descriptor, "descriptor");

      var textBlock = new TextBlock();

      if (!isFirst)
      {
        var span = new Span();
        this.FormatOperator(descriptor, span);
        span.Tag = descriptor;

        textBlock.Inlines.Add(span);
        textBlock.Inlines.Add(new Run(@" "));
      }

      var notHyperlink = new Hyperlink(new Run(descriptor.IsNot ? @"except when" : @"when"));
      notHyperlink.Click += this.ToggleNot;
      notHyperlink.Tag = descriptor;
      textBlock.Inlines.Add(notHyperlink);
      textBlock.Inlines.Add(new Run(@" "));

      this.FormatText(textBlock, descriptor.DisplayText, descriptor.Parameters, true);

      return textBlock;
    }

    /// <summary>Formats the specified display text.</summary>
    /// <param name="displayText">The display text.</param>
    /// <returns>Returns the .</returns>
    [NotNull]
    private TextBlock FormatDisplayText([NotNull] string displayText)
    {
      Debug.ArgumentNotNull(displayText, "displayText");

      var textBlock = new TextBlock();

      this.FormatText(textBlock, displayText, emptyDictionary, false);

      return textBlock;
    }

    /// <summary>Formats the operator.</summary>
    /// <param name="descriptor">The descriptor.</param>
    /// <param name="span">The span.</param>
    [NotNull]
    private void FormatOperator([NotNull] RuleConditionDescriptor descriptor, [NotNull] Span span)
    {
      Debug.ArgumentNotNull(descriptor, "descriptor");
      Debug.ArgumentNotNull(span, "span");

      if (descriptor.Operator == RuleConditionDescriptorOperator.And)
      {
        span.Inlines.Add(new Run(@"    "));
      }

      var op = descriptor.Operator == RuleConditionDescriptorOperator.And ? @"and" : @"or";
      var operatorHyperlink = new Hyperlink(new Run(op));
      operatorHyperlink.Click += this.ToggleOperator;
      operatorHyperlink.Tag = descriptor;

      span.Inlines.Add(operatorHyperlink);
    }

    /// <summary>Formats the text.</summary>
    /// <param name="textBlock">The text block.</param>
    /// <param name="displayText">The display text.</param>
    /// <param name="parameters">The parameters.</param>
    /// <param name="editable">if set to <c>true</c> [editable].</param>
    private void FormatText([NotNull] TextBlock textBlock, [NotNull] string displayText, [NotNull] Dictionary<string, string> parameters, bool editable)
    {
      Debug.ArgumentNotNull(textBlock, "textBlock");
      Debug.ArgumentNotNull(displayText, "displayText");
      Debug.ArgumentNotNull(parameters, "parameters");

      var start = 0;
      var end = displayText.IndexOf('[', start);

      if (end < 0)
      {
        textBlock.Inlines.Add(new Run(displayText));
      }

      while (end >= 0)
      {
        var text = displayText.Mid(start, end - start);
        if (!string.IsNullOrEmpty(text))
        {
          textBlock.Inlines.Add(new Run(text));
        }

        start = end;
        end = displayText.IndexOf(']', start);
        if (end < 0)
        {
          break;
        }

        var parts = displayText.Mid(start + 1, end - start - 1).Split(',');
        if (parts.Length == 4)
        {
          string value;
          if (!parameters.TryGetValue(parts[0], out value))
          {
            value = parts[3];
          }

          if (editable)
          {
            var hyperlink = new Hyperlink(new Run(value));
            hyperlink.Click += delegate { this.EditValue(hyperlink, parameters, parts); };
            textBlock.Inlines.Add(hyperlink);
          }
          else
          {
            var run = new Run(value)
            {
              Foreground = Brushes.Blue
            };

            textBlock.Inlines.Add(run);
          }
        }

        start = end + 1;
        end = displayText.IndexOf('[', start);
      }
    }

    /// <summary>Gets the category.</summary>
    /// <param name="category">The category.</param>
    /// <returns>Returns the category.</returns>
    [NotNull]
    private ListBoxItem GetCategory([NotNull] string category)
    {
      Debug.ArgumentNotNull(category, "category");

      return new ListBoxItem
      {
        Content = category, 
        IsEnabled = false, 
        FontWeight = FontWeights.Bold, 
        BorderThickness = new Thickness(0, 0, 0, 1), 
        BorderBrush = SystemColors.ControlDarkBrush, 
        Foreground = SystemColors.WindowTextBrush, 
        Margin = new Thickness(4, 8, 4, 0)
      };
    }

    /// <summary>Gets the list box item.</summary>
    /// <param name="displayText">The display text.</param>
    /// <param name="tag">The tag.</param>
    /// <returns>Returns the list box item.</returns>
    [NotNull]
    private ListBoxItem GetListBoxItem([NotNull] string displayText, [NotNull] object tag)
    {
      Debug.ArgumentNotNull(displayText, "displayText");
      Debug.ArgumentNotNull(tag, "tag");

      var listBoxItem = new ListBoxItem
      {
        Content = this.FormatDisplayText(displayText), 
        Tag = tag, 
        Margin = new Thickness(16, 0, 0, 0)
      };

      return listBoxItem;
    }

    /// <summary>Loads the conditions.</summary>
    /// <param name="root">The root.</param>
    private void LoadActions([NotNull] XElement root)
    {
      Debug.ArgumentNotNull(root, "root");

      this.ActionDescriptors.Clear();

      var actions = root.Element("actions");
      if (actions == null)
      {
        return;
      }

      foreach (var element in actions.Elements())
      {
        var id = Guid.Parse(element.GetAttributeValue("id"));
        var category = element.GetAttributeValue("category");
        var displayText = element.Value;

        var action = new RuleFieldActionDefinition(id, category, displayText);

        this.ActionDescriptors.Add(action);
      }
    }

    /// <summary>Loads the conditions.</summary>
    /// <param name="root">The root.</param>
    private void LoadConditions([NotNull] XElement root)
    {
      Debug.ArgumentNotNull(root, "root");

      this.ConditionDescriptors.Clear();

      var conditions = root.Element("conditions");
      if (conditions == null)
      {
        return;
      }

      foreach (var element in conditions.Elements())
      {
        var id = Guid.Parse(element.GetAttributeValue("id"));
        var category = element.GetAttributeValue("category");
        var displayText = element.Value;

        var condition = new RuleFieldConditionDefinition(id, category, displayText);

        this.ConditionDescriptors.Add(condition);
      }
    }

    /// <summary>Loads the rules.</summary>
    private void LoadRules()
    {
      ExecuteCompleted c = delegate(string response, ExecuteResult result)
      {
        if (!DataService.HandleExecute(response, result))
        {
          return;
        }

        var root = response.ToXElement();
        if (root != null)
        {
          this.LoadConditions(root);
          this.LoadActions(root);
        }

        this.RenderConditions();
        this.RenderActions();
      };

      this.DatabaseUri.Site.DataService.ExecuteAsync("Rules.GetConditionsAndActions", c, this.DatabaseUri.DatabaseName.ToString(), string.Empty);
    }

    /// <summary>Opens the description context menu.</summary>
    /// <param name="sender">The sender.</param>
    /// <param name="e">The <see cref="System.Windows.Controls.ContextMenuEventArgs"/> instance containing the event data.</param>
    private void OpenDescriptionContextMenu([NotNull] object sender, [NotNull] ContextMenuEventArgs e)
    {
      Debug.ArgumentNotNull(sender, "sender");
      Debug.ArgumentNotNull(e, "e");

      this.DescriptionPane.ContextMenu = null;

      var context = this.GetContext() as RuleFieldDesignerContext;
      if (context == null)
      {
        return;
      }

      context.Description = this.Description;

      var commands = VisualStudio.Commands.CommandManager.GetCommands(context);
      if (!commands.Any())
      {
        e.Handled = true;
        return;
      }

      var contextMenu = new ContextMenu();

      contextMenu.Build(commands, context);

      this.DescriptionPane.ContextMenu = contextMenu;
    }

    /// <summary>Renders the conditions.</summary>
    private void RenderActions()
    {
      var actions = this.ActionDescriptors.OrderBy(descriptor => descriptor.Category).ThenBy(descriptor => descriptor.DisplayText);

      string category = null;
      foreach (var action in actions)
      {
        if (category != action.Category)
        {
          category = action.Category;
          this.Actions.Items.Add(this.GetCategory(category));
        }

        this.Actions.Items.Add(this.GetListBoxItem(action.DisplayText, action));
      }
    }

    /// <summary>Renders the conditions.</summary>
    private void RenderConditions()
    {
      var conditions = this.ConditionDescriptors.OrderBy(descriptor => descriptor.Category).ThenBy(descriptor => descriptor.DisplayText);

      string category = null;
      foreach (var condition in conditions)
      {
        if (category != condition.Category)
        {
          category = condition.Category;
          this.Conditions.Items.Add(this.GetCategory(category));
        }

        var displayText = @"when " + condition.DisplayText;

        this.Conditions.Items.Add(this.GetListBoxItem(displayText, condition));
      }
    }

    /// <summary>Toggles the not.</summary>
    /// <param name="sender">The sender.</param>
    /// <param name="e">The <see cref="System.Windows.RoutedEventArgs"/> instance containing the event data.</param>
    private void ToggleNot([NotNull] object sender, [NotNull] RoutedEventArgs e)
    {
      Debug.ArgumentNotNull(sender, "sender");
      Debug.ArgumentNotNull(e, "e");

      var hyperlink = (Hyperlink)sender;
      var descriptor = (RuleConditionDescriptor)hyperlink.Tag;

      descriptor.IsNot = !descriptor.IsNot;

      hyperlink.Inlines.Clear();
      hyperlink.Inlines.Add(new Run(descriptor.IsNot ? @"except when" : @"when"));
    }

    /// <summary>Toggles the operator.</summary>
    /// <param name="sender">The sender.</param>
    /// <param name="e">The <see cref="System.Windows.RoutedEventArgs"/> instance containing the event data.</param>
    private void ToggleOperator([NotNull] object sender, [NotNull] RoutedEventArgs e)
    {
      Debug.ArgumentNotNull(sender, "sender");
      Debug.ArgumentNotNull(e, "e");

      var hyperlink = (Hyperlink)sender;
      var span = (Span)hyperlink.Parent;

      var descriptor = (RuleConditionDescriptor)span.Tag;
      descriptor.Operator = descriptor.Operator == RuleConditionDescriptorOperator.And ? RuleConditionDescriptorOperator.Or : RuleConditionDescriptorOperator.And;

      span.Inlines.Clear();
      this.FormatOperator(descriptor, span);
    }

    #endregion
  }
}