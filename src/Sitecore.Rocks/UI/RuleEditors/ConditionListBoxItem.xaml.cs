// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Xml.Linq;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Extensions.StringExtensions;
using Sitecore.Rocks.Extensions.XElementExtensions;
using Sitecore.Rocks.UI.RuleEditors.Macros;

namespace Sitecore.Rocks.UI.RuleEditors
{
    public partial class ConditionListBoxItem
    {
        public ConditionListBoxItem([NotNull] RulesPresenter rulesPresenter, [NotNull] XElement element, int index, bool isEditable)
        {
            Assert.ArgumentNotNull(rulesPresenter, nameof(rulesPresenter));
            Assert.ArgumentNotNull(element, nameof(element));

            InitializeComponent();

            RulesPresenter = rulesPresenter;
            Element = element;
            Index = index;
            IsEditable = isEditable;
            OperatorId = element.GetAttributeValue("_operatorid");
            Operator = element.GetAttributeValue("_operator");

            Refresh();
        }

        public bool CanMoveDown { get; set; }

        public bool CanMoveUp { get; set; }

        [NotNull]
        public XElement Element { get; }

        public int Index { get; }

        public bool IsEditable { get; }

        [NotNull]
        public string OperatorId { get; set; }

        [NotNull]
        public RulesPresenter RulesPresenter { get; }

        [NotNull]
        protected string Operator { get; set; }

        public void Refresh()
        {
            var id = Element.GetAttributeValue("id");

            var condition = RulesPresenter.Conditions.FirstOrDefault(c => c.Id == id);

            if (condition == null)
            {
                Text.Text = Rocks.Resources.ConditionListBoxItem_Refresh_Unknown_condition_ + " " + id;
                return;
            }

            Text.Inlines.Clear();
            Text.Inlines.Add(RenderText(condition.Text, Element));
        }

        public event EventHandler ToggleExcept;

        public event EventHandler ToggleOperator;

        private void AddMacro([NotNull] TextBlock result, [NotNull] XElement element, [NotNull] string macroText)
        {
            Debug.ArgumentNotNull(result, nameof(result));
            Debug.ArgumentNotNull(element, nameof(element));
            Debug.ArgumentNotNull(macroText, nameof(macroText));

            var control = RuleEditorMacroManager.GetMacroControl(element, RulesPresenter.DatabaseUri ?? DatabaseUri.Empty, macroText, IsEditable);
            if (control == null)
            {
                result.Inlines.Add(macroText);
                return;
            }

            var inline = control as Inline;
            if (inline != null)
            {
                result.Inlines.Add(inline);
                return;
            }

            var uielement = control as UIElement;
            if (uielement != null)
            {
                result.Inlines.Add(uielement);
            }
        }

        private void AddOperator([NotNull] XElement element, [NotNull] TextBlock result)
        {
            Debug.ArgumentNotNull(element, nameof(element));
            Debug.ArgumentNotNull(result, nameof(result));

            if (Index <= 0)
            {
                return;
            }

            if (!IsEditable)
            {
                switch (Operator)
                {
                    case "or":
                        result.Inlines.Add("or ");
                        break;

                    case "and":
                        result.Inlines.Add("    and ");
                        break;
                }

                return;
            }

            result.Inlines.Add(Operator == @"and" ? @"    " : @" ");

            var link = new Hyperlink(new Run(Operator));
            link.Click += RaiseToggleOperator;
            result.Inlines.Add(link);

            result.Inlines.Add(" ");
        }

        private void AddText([NotNull] XElement element, [NotNull] string text, [NotNull] TextBlock result)
        {
            Debug.ArgumentNotNull(element, nameof(element));
            Debug.ArgumentNotNull(text, nameof(text));
            Debug.ArgumentNotNull(result, nameof(result));

            var s = text.IndexOf('[');
            while (s >= 0)
            {
                var e = text.IndexOf(']', s);
                if (e < 0)
                {
                    break;
                }

                var run = new Run(text.Left(s));
                result.Inlines.Add(run);

                AddMacro(result, element, text.Mid(s + 1, e - s - 1));

                text = text.Mid(e + 1);
                s = text.IndexOf('[');
            }

            if (!string.IsNullOrEmpty(text))
            {
                result.Inlines.Add(new Run(text));
            }
        }

        private void AddWhere([NotNull] XElement element, [NotNull] string text, [NotNull] TextBlock result)
        {
            Debug.ArgumentNotNull(element, nameof(element));
            Debug.ArgumentNotNull(text, nameof(text));
            Debug.ArgumentNotNull(result, nameof(result));

            var where = element.GetAttributeValue("except") == @"true" ? Rocks.Resources.ConditionListBoxItem_AddWhere_except_where : Rocks.Resources.ConditionListBoxItem_AddWhere_where;

            if (!IsEditable)
            {
                result.Inlines.Add(where);
                result.Inlines.Add(new Run(@" "));
                return;
            }

            var link = new Hyperlink(new Run(where));
            link.Click += RaiseToggleExcept;

            result.Inlines.Add(link);
            result.Inlines.Add(new Run(" "));
        }

        private void RaiseToggleExcept([NotNull] object sender, [NotNull] RoutedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            var toggleExcept = ToggleExcept;
            if (toggleExcept != null)
            {
                toggleExcept(this, EventArgs.Empty);
            }
        }

        private void RaiseToggleOperator([NotNull] object sender, [NotNull] RoutedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            var toggleOperator = ToggleOperator;
            if (toggleOperator != null)
            {
                toggleOperator(this, EventArgs.Empty);
            }
        }

        [NotNull]
        private TextBlock RenderText([NotNull] string text, [NotNull] XElement element)
        {
            Debug.ArgumentNotNull(text, nameof(text));
            Debug.ArgumentNotNull(element, nameof(element));

            if (text.StartsWith(@"where "))
            {
                text = text.Mid(6);
            }

            var result = new TextBlock();

            AddOperator(element, result);
            AddWhere(element, text, result);
            AddText(element, text, result);

            return result;
        }
    }
}
