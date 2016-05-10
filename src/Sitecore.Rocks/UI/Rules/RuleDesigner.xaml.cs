// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Extensions.ContextMenuExtensions;
using Sitecore.Rocks.Extensions.StringExtensions;
using Sitecore.Rocks.Rules;

namespace Sitecore.Rocks.UI.Rules
{
    public partial class RuleDesigner : IContextProvider
    {
        private static readonly Dictionary<string, string> emptyDictionary = new Dictionary<string, string>();

        public RuleDesigner()
        {
            InitializeComponent();
        }

        public Rule Rule { get; set; }

        [CanBeNull]
        protected object Parameter { get; set; }

        public object GetContext()
        {
            return new RuleDesignerContext
            {
                RuleDesigner = this
            };
        }

        public void Initialize([NotNull] Rule rule, [CanBeNull] object parameter)
        {
            Assert.ArgumentNotNull(rule, nameof(rule));

            Rule = rule;
            Parameter = parameter;

            RenderConditions();
            RenderActions();
            RenderRule(rule);
        }

        public void RenderRule([NotNull] Rule rule)
        {
            Assert.ArgumentNotNull(rule, nameof(rule));

            Description.Items.Clear();

            var isFirst = true;
            foreach (var descriptor in rule.ConditionDescriptors)
            {
                var listBoxItem = new ListBoxItem
                {
                    Content = FormatCondition(descriptor, isFirst),
                    Tag = descriptor
                };

                Description.Items.Add(listBoxItem);
                isFirst = false;
            }

            for (var index = 0; index < rule.ActionDescriptors.Count; index++)
            {
                var descriptor = rule.ActionDescriptors[index];
                var listBoxItem = new ListBoxItem
                {
                    Content = FormatAction(descriptor, index < rule.ActionDescriptors.Count - 1),
                    Tag = descriptor
                };

                Description.Items.Add(listBoxItem);
            }
        }

        private void ActionFilterTextChanged([NotNull] object sender, [NotNull] EventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            foreach (ListBoxItem item in Actions.Items)
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

                item.Visibility = text.IsFilterMatch(ActionSelectorFilter.Text) ? Visibility.Visible : Visibility.Collapsed;
            }

            var hasItems = false;

            for (var n = Actions.Items.Count - 1; n >= 0; n--)
            {
                var item = Actions.Items[n] as ListBoxItem;
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

        private void AddAction([NotNull] object sender, [NotNull] MouseButtonEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            var selectedItem = Actions.SelectedItem as ListBoxItem;
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

            Rule.ActionDescriptors.Add(descriptor);

            RenderRule(Rule);
        }

        private void AddCondition([NotNull] object sender, [NotNull] MouseButtonEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            var selectedItem = Conditions.SelectedItem as ListBoxItem;
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

            Rule.ConditionDescriptors.Add(descriptor);

            RenderRule(Rule);
        }

        private void ConditionFilterTextChanged([NotNull] object sender, [NotNull] EventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            foreach (ListBoxItem item in Conditions.Items)
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

                item.Visibility = text.IsFilterMatch(ConditionSelectorFilter.Text) ? Visibility.Visible : Visibility.Collapsed;
            }

            var hasItems = false;

            for (var n = Conditions.Items.Count - 1; n >= 0; n--)
            {
                var item = Conditions.Items[n] as ListBoxItem;
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

        private void EditValue([NotNull] Hyperlink hyperlink, [NotNull] Dictionary<string, string> parameters, [NotNull] string[] parts)
        {
            Debug.ArgumentNotNull(hyperlink, nameof(hyperlink));
            Debug.ArgumentNotNull(parameters, nameof(parameters));
            Debug.ArgumentNotNull(parts, nameof(parts));

            string value;
            if (!parameters.TryGetValue(parts[0], out value))
            {
                value = string.Empty;
            }

            var editorName = parts[1];

            var editor = RuleManager.GetParameterEditor(editorName);

            value = editor.GetValue(value, Parameter);

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

        [NotNull]
        private TextBlock FormatAction([NotNull] RuleActionDescriptor descriptor, bool isLast)
        {
            Debug.ArgumentNotNull(descriptor, nameof(descriptor));

            var textBlock = new TextBlock();

            FormatText(textBlock, descriptor.DisplayText, descriptor.Parameters, true);

            if (isLast)
            {
                textBlock.Inlines.Add(new Run(@" and"));
            }

            return textBlock;
        }

        [NotNull]
        private TextBlock FormatCondition([NotNull] RuleConditionDescriptor descriptor, bool isFirst)
        {
            Debug.ArgumentNotNull(descriptor, nameof(descriptor));

            var textBlock = new TextBlock();

            if (!isFirst)
            {
                var span = new Span();
                FormatOperator(descriptor, span);
                span.Tag = descriptor;

                textBlock.Inlines.Add(span);
                textBlock.Inlines.Add(new Run(@" "));
            }

            var notHyperlink = new Hyperlink(new Run(descriptor.IsNot ? @"except when" : @"when"));
            notHyperlink.Click += ToggleNot;
            notHyperlink.Tag = descriptor;
            textBlock.Inlines.Add(notHyperlink);
            textBlock.Inlines.Add(new Run(@" "));

            FormatText(textBlock, descriptor.DisplayText, descriptor.Parameters, true);

            return textBlock;
        }

        [NotNull]
        private TextBlock FormatDisplayText([NotNull] string displayText)
        {
            Debug.ArgumentNotNull(displayText, nameof(displayText));

            var textBlock = new TextBlock();

            FormatText(textBlock, displayText, emptyDictionary, false);

            return textBlock;
        }

        private void FormatOperator([NotNull] RuleConditionDescriptor descriptor, [NotNull] Span span)
        {
            Debug.ArgumentNotNull(descriptor, nameof(descriptor));
            Debug.ArgumentNotNull(span, nameof(span));

            if (descriptor.Operator == RuleConditionDescriptorOperator.And)
            {
                span.Inlines.Add(new Run(@"    "));
            }

            var op = descriptor.Operator == RuleConditionDescriptorOperator.And ? @"and" : @"or";
            var operatorHyperlink = new Hyperlink(new Run(op));
            operatorHyperlink.Click += ToggleOperator;
            operatorHyperlink.Tag = descriptor;

            span.Inlines.Add(operatorHyperlink);
        }

        private void FormatText([NotNull] TextBlock textBlock, [NotNull] string displayText, [NotNull] Dictionary<string, string> parameters, bool editable)
        {
            Debug.ArgumentNotNull(textBlock, nameof(textBlock));
            Debug.ArgumentNotNull(displayText, nameof(displayText));
            Debug.ArgumentNotNull(parameters, nameof(parameters));

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
                        hyperlink.Click += delegate { EditValue(hyperlink, parameters, parts); };
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

        [NotNull]
        private ListBoxItem GetCategory([NotNull] string category)
        {
            Debug.ArgumentNotNull(category, nameof(category));

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

        [NotNull]
        private ListBoxItem GetListBoxItem([NotNull] string displayText, [NotNull] object tag)
        {
            Debug.ArgumentNotNull(displayText, nameof(displayText));
            Debug.ArgumentNotNull(tag, nameof(tag));

            var listBoxItem = new ListBoxItem
            {
                Content = FormatDisplayText(displayText),
                Tag = tag,
                Margin = new Thickness(16, 0, 0, 0)
            };

            return listBoxItem;
        }

        private void OpenDescriptionContextMenu([NotNull] object sender, [NotNull] ContextMenuEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            DescriptionPane.ContextMenu = null;

            var context = GetContext() as RuleDesignerContext;
            if (context == null)
            {
                return;
            }

            context.Description = Description;

            var commands = Rocks.Commands.CommandManager.GetCommands(context).ToList();
            if (!commands.Any())
            {
                e.Handled = true;
                return;
            }

            var contextMenu = new ContextMenu();

            contextMenu.Build(commands, context);

            DescriptionPane.ContextMenu = contextMenu;
        }

        private void RenderActions()
        {
            var actions = RuleManager.GetActions(Parameter).OrderBy(descriptor => descriptor.Attribute.Category).ThenBy(descriptor => descriptor.Attribute.DisplayText);

            string category = null;
            foreach (var action in actions)
            {
                if (category != action.Attribute.Category)
                {
                    category = action.Attribute.Category;
                    Actions.Items.Add(GetCategory(category));
                }

                Actions.Items.Add(GetListBoxItem(action.Attribute.DisplayText, action));
            }
        }

        private void RenderConditions()
        {
            var conditions = RuleManager.Conditions.OrderBy(descriptor => descriptor.Attribute.Category).ThenBy(descriptor => descriptor.Attribute.DisplayText);

            string category = null;
            foreach (var condition in conditions)
            {
                if (category != condition.Attribute.Category)
                {
                    category = condition.Attribute.Category;
                    Conditions.Items.Add(GetCategory(category));
                }

                var displayText = @"when " + condition.Attribute.DisplayText;

                Conditions.Items.Add(GetListBoxItem(displayText, condition));
            }
        }

        private void ToggleNot([NotNull] object sender, [NotNull] RoutedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            var hyperlink = (Hyperlink)sender;
            var descriptor = (RuleConditionDescriptor)hyperlink.Tag;

            descriptor.IsNot = !descriptor.IsNot;

            hyperlink.Inlines.Clear();
            hyperlink.Inlines.Add(new Run(descriptor.IsNot ? @"except when" : @"when"));
        }

        private void ToggleOperator([NotNull] object sender, [NotNull] RoutedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            var hyperlink = (Hyperlink)sender;
            var span = (Span)hyperlink.Parent;

            var descriptor = (RuleConditionDescriptor)span.Tag;
            descriptor.Operator = descriptor.Operator == RuleConditionDescriptorOperator.And ? RuleConditionDescriptorOperator.Or : RuleConditionDescriptorOperator.And;

            span.Inlines.Clear();
            FormatOperator(descriptor, span);
        }
    }
}
