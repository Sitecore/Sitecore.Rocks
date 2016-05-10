// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

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
    public partial class ActionListBoxItem
    {
        public ActionListBoxItem([NotNull] RulesPresenter rulesPresenter, [NotNull] XElement element, int index, bool isEditable)
        {
            Assert.ArgumentNotNull(rulesPresenter, nameof(rulesPresenter));
            Assert.ArgumentNotNull(element, nameof(element));

            InitializeComponent();

            RulesPresenter = rulesPresenter;
            Element = element;
            Index = index;
            IsEditable = isEditable;

            Refresh();
        }

        public bool CanMoveDown { get; set; }

        public bool CanMoveUp { get; set; }

        [NotNull]
        public XElement Element { get; }

        public int Index { get; }

        public bool IsEditable { get; set; }

        [NotNull]
        public RulesPresenter RulesPresenter { get; }

        private void AddMacro([NotNull] TextBlock result, [NotNull] XElement element, [NotNull] DatabaseUri databaseUri, [NotNull] string macroText)
        {
            Debug.ArgumentNotNull(result, nameof(result));
            Debug.ArgumentNotNull(element, nameof(element));
            Debug.ArgumentNotNull(databaseUri, nameof(databaseUri));
            Debug.ArgumentNotNull(macroText, nameof(macroText));

            var control = RuleEditorMacroManager.GetMacroControl(element, databaseUri, macroText, IsEditable);
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

        private void Refresh()
        {
            var id = Element.GetAttributeValue("id");

            var condition = RulesPresenter.Actions.FirstOrDefault(c => c.Id == id);

            if (condition == null)
            {
                Text.Text = Rocks.Resources.ActionListBoxItem_Refresh_Unknown_action_ + @" " + id;
                return;
            }

            Text.Inlines.Clear();
            Text.Inlines.Add(RenderText(condition.Text, Element, RulesPresenter.DatabaseUri ?? DatabaseUri.Empty));
        }

        [NotNull]
        private TextBlock RenderText([NotNull] string text, [NotNull] XElement element, [NotNull] DatabaseUri databaseUri)
        {
            Debug.ArgumentNotNull(text, nameof(text));
            Debug.ArgumentNotNull(element, nameof(element));
            Debug.ArgumentNotNull(databaseUri, nameof(databaseUri));

            var result = new TextBlock();

            if (Index > 0)
            {
                result.Inlines.Add(@"and ");
            }

            var s = text.IndexOf('[');
            while (s >= 0)
            {
                var e = text.IndexOf(']', s);
                if (e < 0)
                {
                    break;
                }

                result.Inlines.Add(new Run(text.Left(s)));

                AddMacro(result, element, databaseUri, text.Mid(s + 1, e - s - 1));

                text = text.Mid(e + 1);
                s = text.IndexOf('[');
            }

            if (!string.IsNullOrEmpty(text))
            {
                result.Inlines.Add(new Run(text));
            }

            return result;
        }
    }
}
