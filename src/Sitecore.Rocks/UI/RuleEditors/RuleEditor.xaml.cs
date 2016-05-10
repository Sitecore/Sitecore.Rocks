// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Xml.Linq;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Data.DataServices;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Extensions.StringExtensions;
using Sitecore.Rocks.Extensions.XElementExtensions;

namespace Sitecore.Rocks.UI.RuleEditors
{
    public partial class RuleEditor : IContextProvider
    {
        private readonly List<ActionItem> actionItems = new List<ActionItem>();

        private readonly List<ConditionItem> conditionItems = new List<ConditionItem>();

        public RuleEditor()
        {
            InitializeComponent();

            DataSource = string.Empty;
        }

        [NotNull]
        public IEnumerable<ActionItem> ActionItems
        {
            get { return actionItems; }
        }

        public bool AllowMultiple { get; set; }

        [NotNull]
        public IEnumerable<ConditionItem> ConditionItems
        {
            get { return conditionItems; }
        }

        [NotNull]
        public string DataSource { get; set; }

        public bool HideActions { get; set; }

        [NotNull]
        public RuleModel RuleModel { get; set; }

        [NotNull]
        protected DatabaseUri DatabaseUri { get; set; }

        public object GetContext()
        {
            return new RuleEditorContext
            {
                RuleEditor = this
            };
        }

        public void Initialize([NotNull] DatabaseUri databaseUri, [NotNull] string dataSource, [NotNull] RuleModel ruleModel)
        {
            Assert.ArgumentNotNull(databaseUri, nameof(databaseUri));
            Assert.ArgumentNotNull(dataSource, nameof(dataSource));
            Assert.ArgumentNotNull(ruleModel, nameof(ruleModel));

            DatabaseUri = databaseUri;
            DataSource = dataSource;
            RuleModel = ruleModel;

            RulePresenter.DatabaseUri = databaseUri;

            LoadConditionAndActions();
        }

        public event EventHandler Modified;

        protected void RenderRules()
        {
            RulePresenter.Render();
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

                var actionItem = item.Tag as ActionItem;
                if (actionItem == null)
                {
                    continue;
                }

                item.Visibility = actionItem.Text.IsFilterMatch(ActionSelectorFilter.Text) ? Visibility.Visible : Visibility.Collapsed;
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

            var action = Actions.SelectedItem as ListBoxItem;
            if (action == null)
            {
                return;
            }

            var actionItem = action.Tag as ActionItem;
            if (actionItem == null)
            {
                return;
            }

            RulePresenter.AddAction(actionItem);
        }

        private void AddCondition([NotNull] object sender, [NotNull] MouseButtonEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            var condition = Conditions.SelectedItem as ListBoxItem;
            if (condition == null)
            {
                return;
            }

            var conditionItem = condition.Tag as ConditionItem;
            if (conditionItem == null)
            {
                return;
            }

            RulePresenter.AddCondition(conditionItem);
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

                var conditionItem = item.Tag as ConditionItem;
                if (conditionItem == null)
                {
                    continue;
                }

                item.Visibility = conditionItem.Text.IsFilterMatch(ConditionSelectorFilter.Text) ? Visibility.Visible : Visibility.Collapsed;
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

        private void LoadConditionAndActions()
        {
            ExecuteCompleted completed = delegate(string response, ExecuteResult result)
            {
                if (!DataService.HandleExecute(response, result))
                {
                    return;
                }

                var root = response.ToXElement();
                if (root == null)
                {
                    return;
                }

                ParseConditionsAndActions(root);

                RenderConditions();
                RenderActions();

                RulePresenter.Conditions = ConditionItems;
                RulePresenter.Actions = ActionItems;

                RulePresenter.RuleModel = RuleModel;

                RenderRules();
            };

            AppHost.Server.Rules.GetConditionsAndActions(DatabaseUri, DataSource, completed);
        }

        private void OpenContextMenu([NotNull] object sender, [NotNull] ContextMenuEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            var selectedItem = RulePresenter.Listbox.SelectedItem as ListBoxItem;

            var context = new RuleEditorContext
            {
                RuleEditor = this,
                RulePresenter = RulePresenter,
                SelectedRuleEntry = selectedItem
            };

            PresenterPane.ContextMenu = AppHost.ContextMenus.Build(context, e);
        }

        private void ParseConditionsAndActions([NotNull] XElement root)
        {
            Debug.ArgumentNotNull(root, nameof(root));

            var conditions = root.Element(@"conditions");
            if (conditions != null)
            {
                foreach (var element in conditions.Elements())
                {
                    var condition = new ConditionItem();
                    conditionItems.Add(condition);

                    condition.Id = element.GetAttributeValue("id");
                    condition.Category = element.GetAttributeValue("category");
                    condition.Text = element.Value;
                }
            }

            var actions = root.Element(@"actions");
            if (actions != null)
            {
                foreach (var element in actions.Elements())
                {
                    var action = new ActionItem();
                    actionItems.Add(action);

                    action.Id = element.GetAttributeValue("id");
                    action.Category = element.GetAttributeValue("category");
                    action.Text = element.Value;
                }
            }
        }

        private void RaiseModified()
        {
            var modified = Modified;
            if (modified != null)
            {
                modified(this, EventArgs.Empty);
            }
        }

        private void RenderActions()
        {
            string category = null;
            var margin = new Thickness(16, 0, 0, 0);

            foreach (var item in ActionItems.OrderBy(c => c.Category).ThenBy(c => c.Text))
            {
                if (item.Category != category)
                {
                    category = item.Category;

                    var c = new ListBoxItem
                    {
                        Content = category,
                        IsEnabled = false,
                        FontWeight = FontWeights.Bold,
                        BorderThickness = new Thickness(0, 0, 0, 1),
                        BorderBrush = SystemColors.ControlDarkBrush,
                        Foreground = SystemColors.WindowTextBrush,
                        Margin = new Thickness(4, 8, 4, 0)
                    };

                    Actions.Items.Add(c);
                }

                var listBoxItem = new ListBoxItem
                {
                    Content = RenderText(item.Text.Trim()),
                    Tag = item,
                    Margin = margin
                };

                Actions.Items.Add(listBoxItem);
            }
        }

        private void RenderConditions()
        {
            string category = null;
            var margin = new Thickness(16, 0, 0, 0);

            foreach (var item in ConditionItems.OrderBy(c => c.Category).ThenBy(c => c.Text))
            {
                if (item.Category != category)
                {
                    category = item.Category;

                    var c = new ListBoxItem
                    {
                        Content = category,
                        IsEnabled = false,
                        FontWeight = FontWeights.Bold,
                        BorderThickness = new Thickness(0, 0, 0, 1),
                        BorderBrush = SystemColors.ControlDarkBrush,
                        Foreground = SystemColors.WindowTextBrush,
                        Margin = new Thickness(4, 8, 4, 0)
                    };

                    Conditions.Items.Add(c);
                }

                var listBoxItem = new ListBoxItem
                {
                    Content = RenderText(item.Text.Trim()),
                    Tag = item,
                    Margin = margin
                };

                Conditions.Items.Add(listBoxItem);
            }
        }

        [NotNull]
        private TextBlock RenderText([NotNull] string text)
        {
            Debug.ArgumentNotNull(text, nameof(text));

            var result = new TextBlock();

            var s = text.IndexOf('[');
            while (s >= 0)
            {
                var e = text.IndexOf(']', s);
                if (e < 0)
                {
                    break;
                }

                result.Inlines.Add(new Run(text.Left(s)));

                var macro = text.Mid(s + 1, e - s - 1);
                text = text.Mid(e + 1);

                var parts = macro.Split(',');

                if (parts.Length >= 4)
                {
                    macro = parts[3];
                }

                var run = new Run(macro)
                {
                    Foreground = Brushes.Blue
                };

                result.Inlines.Add(run);
                s = text.IndexOf('[');
            }

            if (!string.IsNullOrEmpty(text))
            {
                result.Inlines.Add(new Run(text));
            }

            return result;
        }

        private void SetModified([NotNull] object sender, [NotNull] EventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            RaiseModified();
        }

        public class ActionItem
        {
            [NotNull]
            public string Category { get; set; }

            [NotNull]
            public string Id { get; set; }

            [NotNull]
            public string Text { get; set; }
        }

        public class ConditionItem
        {
            [NotNull]
            public string Category { get; set; }

            [NotNull]
            public string Id { get; set; }

            [NotNull]
            public string Text { get; set; }
        }
    }
}
