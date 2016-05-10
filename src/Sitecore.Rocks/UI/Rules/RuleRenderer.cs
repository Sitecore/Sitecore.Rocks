// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Collections.Generic;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Extensions.StringExtensions;
using Sitecore.Rocks.Rules;

namespace Sitecore.Rocks.UI.Rules
{
    public static class RuleRenderer
    {
        public static void Render([NotNull] Rule rule, [NotNull] ListBox listBox)
        {
            Assert.ArgumentNotNull(rule, nameof(rule));
            Assert.ArgumentNotNull(listBox, nameof(listBox));

            listBox.Items.Clear();

            var isFirst = true;
            foreach (var descriptor in rule.ConditionDescriptors)
            {
                var listBoxItem = new ListBoxItem
                {
                    Content = FormatCondition(descriptor, isFirst),
                    Tag = descriptor
                };

                listBox.Items.Add(listBoxItem);
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

                listBox.Items.Add(listBoxItem);
            }
        }

        [NotNull]
        private static TextBlock FormatAction([NotNull] RuleActionDescriptor descriptor, bool isLast)
        {
            Debug.ArgumentNotNull(descriptor, nameof(descriptor));

            var textBlock = new TextBlock();

            FormatText(textBlock, descriptor.DisplayText, descriptor.Parameters);

            if (isLast)
            {
                textBlock.Inlines.Add(new Run(@" and"));
            }

            return textBlock;
        }

        [NotNull]
        private static TextBlock FormatCondition([NotNull] RuleConditionDescriptor descriptor, bool isFirst)
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
            notHyperlink.Tag = descriptor;
            textBlock.Inlines.Add(notHyperlink);
            textBlock.Inlines.Add(new Run(@" "));

            FormatText(textBlock, descriptor.DisplayText, descriptor.Parameters);

            return textBlock;
        }

        private static void FormatOperator([NotNull] RuleConditionDescriptor descriptor, [NotNull] Span span)
        {
            Debug.ArgumentNotNull(descriptor, nameof(descriptor));
            Debug.ArgumentNotNull(span, nameof(span));

            if (descriptor.Operator == RuleConditionDescriptorOperator.And)
            {
                span.Inlines.Add(new Run(@"    "));
            }

            var op = descriptor.Operator == RuleConditionDescriptorOperator.And ? @"and" : @"or";
            var operatorHyperlink = new Hyperlink(new Run(op));
            operatorHyperlink.Tag = descriptor;

            span.Inlines.Add(operatorHyperlink);
        }

        private static void FormatText([NotNull] TextBlock textBlock, [NotNull] string displayText, [NotNull] Dictionary<string, string> parameters)
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

                    var run = new Run(value)
                    {
                        Foreground = Brushes.Blue
                    };

                    textBlock.Inlines.Add(run);
                }

                start = end + 1;
                end = displayText.IndexOf('[', start);
            }
        }
    }
}
