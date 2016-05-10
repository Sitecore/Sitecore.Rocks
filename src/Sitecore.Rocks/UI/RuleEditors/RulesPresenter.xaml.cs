// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;
using System.Xml.Linq;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Extensions.XElementExtensions;

namespace Sitecore.Rocks.UI.RuleEditors
{
    public partial class RulesPresenter
    {
        public RulesPresenter()
        {
            InitializeComponent();
        }

        [NotNull]
        public IEnumerable<RuleEditor.ActionItem> Actions { get; set; }

        [NotNull]
        public IEnumerable<RuleEditor.ConditionItem> Conditions { get; set; }

        [CanBeNull]
        public DatabaseUri DatabaseUri { get; set; }

        [CanBeNull]
        public string DataSource { get; set; }

        public bool IsEditable { get; set; }

        [NotNull]
        public RuleModel RuleModel { get; set; }

        public void AddAction([NotNull] RuleEditor.ActionItem actionItem)
        {
            Assert.ArgumentNotNull(actionItem, nameof(actionItem));

            if (RuleModel.CurrentRule == null && RuleModel.Rules.Elements().Count() == 1)
            {
                RuleModel.CurrentRule = RuleModel.Rules.Elements().FirstOrDefault();
            }

            RuleModel.AddAction(actionItem);

            Render();
            RaiseModified();
        }

        public void AddCondition([NotNull] RuleEditor.ConditionItem conditionItem)
        {
            Assert.ArgumentNotNull(conditionItem, nameof(conditionItem));

            if (RuleModel.CurrentRule == null && RuleModel.Rules.Elements().Count() == 1)
            {
                RuleModel.CurrentRule = RuleModel.Rules.Elements().FirstOrDefault();
            }

            RuleModel.AddCondition(conditionItem);

            Render();
            RaiseModified();
        }

        public void AddRule()
        {
            var element = RuleModel.AddRule();

            Render();
            RaiseModified();

            var item = Listbox.Items.Cast<ListBoxItem>().FirstOrDefault(i => i.Tag == element);
            if (item != null)
            {
                item.IsSelected = true;
            }
        }

        public void DeleteAction([NotNull] XElement element)
        {
            Assert.ArgumentNotNull(element, nameof(element));

            RuleModel.DeleteAction(element);

            Render();
            RaiseModified();
        }

        public void DeleteCondition([NotNull] XElement element)
        {
            Assert.ArgumentNotNull(element, nameof(element));

            RuleModel.DeleteCondition(element);

            Render();
            RaiseModified();
        }

        public void DeleteRule([NotNull] XElement element)
        {
            Assert.ArgumentNotNull(element, nameof(element));

            RuleModel.DeleteRule(element);

            Render();
            RaiseModified();
        }

        public event EventHandler Modified;

        public void MoveActionDown([NotNull] XElement element)
        {
            Assert.ArgumentNotNull(element, nameof(element));

            RuleModel.MoveActionDown(element);

            Render();
            RaiseModified();
        }

        public void MoveActionUp([NotNull] XElement element)
        {
            Assert.ArgumentNotNull(element, nameof(element));

            RuleModel.MoveActionUp(element);

            Render();
            RaiseModified();
        }

        public void MoveConditionDown([NotNull] XElement element)
        {
            Assert.ArgumentNotNull(element, nameof(element));

            RuleModel.MoveConditionDown(element);

            Render();
            RaiseModified();
        }

        public void MoveConditionUp([NotNull] XElement element)
        {
            Assert.ArgumentNotNull(element, nameof(element));

            RuleModel.MoveConditionUp(element);

            Render();
            RaiseModified();
        }

        public void Render()
        {
            object tag = null;

            var selectedItem = Listbox.SelectedItem as ListBoxItem;
            if (selectedItem != null)
            {
                tag = selectedItem.Tag;
            }

            Listbox.Items.Clear();

            if (RuleModel.IsEmpty)
            {
                RenderNoRules();
                return;
            }

            RenderRules(RuleModel);

            if (tag == null)
            {
                return;
            }

            selectedItem = Listbox.Items.Cast<ListBoxItem>().FirstOrDefault(i => i.Tag == tag);
            if (selectedItem != null)
            {
                selectedItem.IsSelected = true;
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

        private void RenderAction([NotNull] RuleListBoxItem rule, [NotNull] XElement element, int index, int actionCount)
        {
            Debug.ArgumentNotNull(rule, nameof(rule));
            Debug.ArgumentNotNull(element, nameof(element));

            var listBoxItem = new ListBoxItem();
            Listbox.Items.Add(listBoxItem);

            var action = new ActionListBoxItem(this, element, index, IsEditable)
            {
                CanMoveUp = index > 0,
                CanMoveDown = index < actionCount - 1
            };

            listBoxItem.Content = action;
            listBoxItem.Tag = element;
        }

        private void RenderActions([NotNull] RuleListBoxItem rule, [NotNull] IEnumerable<XElement> elements)
        {
            Debug.ArgumentNotNull(rule, nameof(rule));
            Debug.ArgumentNotNull(elements, nameof(elements));

            var list = elements.ToList();

            var index = 0;
            var count = list.Count();

            foreach (var element in list)
            {
                RenderAction(rule, element, index, count);
                index++;
            }
        }

        private void RenderCondition([NotNull] RuleListBoxItem rule, [NotNull] XElement element, int index, int conditionCount)
        {
            Debug.ArgumentNotNull(rule, nameof(rule));
            Debug.ArgumentNotNull(element, nameof(element));

            var listBoxItem = new ListBoxItem();
            Listbox.Items.Add(listBoxItem);

            var item = new ConditionListBoxItem(this, element, index, IsEditable);
            item.ToggleExcept += ToggleExcept;
            item.ToggleOperator += ToggleOperator;
            item.CanMoveUp = index > 0;
            item.CanMoveDown = index < conditionCount - 1;

            listBoxItem.Content = item;
            listBoxItem.Tag = element;
        }

        private void RenderConditions([NotNull] RuleListBoxItem rule, [NotNull] IEnumerable<XElement> elements)
        {
            Debug.ArgumentNotNull(rule, nameof(rule));
            Debug.ArgumentNotNull(elements, nameof(elements));

            var list = elements.ToList();

            var index = 0;
            var count = list.Count();

            foreach (var element in list)
            {
                RenderCondition(rule, element, index, count);
                index++;
            }
        }

        private void RenderNoRules()
        {
        }

        private void RenderRule([NotNull] XElement ruleElement, int index)
        {
            Debug.ArgumentNotNull(ruleElement, nameof(ruleElement));

            var listBoxItem = new ListBoxItem();
            Listbox.Items.Add(listBoxItem);

            var rule = new RuleListBoxItem
            {
                Element = ruleElement,
                Index = index
            };

            listBoxItem.Content = rule;
            listBoxItem.Tag = ruleElement;

            var elements = new List<XElement>();
            var conditions = ruleElement.Element(@"conditions");
            if (conditions != null)
            {
                RuleModel.Flatten(conditions, elements, @"or", string.Empty);
                RenderConditions(rule, elements);
                RuleModel.RemoveOperatorAttributes();
            }

            var actions = ruleElement.Element(@"actions");
            if (actions != null)
            {
                RenderActions(rule, actions.Elements());
            }
        }

        private void RenderRules([NotNull] RuleModel ruleModel)
        {
            Debug.ArgumentNotNull(ruleModel, nameof(ruleModel));

            var index = 1;
            foreach (var element in ruleModel.Rules.Elements())
            {
                RenderRule(element, index);
                index++;
            }
        }

        private void SetSelectedRule([NotNull] object sender, [NotNull] SelectionChangedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            var listBoxItem = Listbox.SelectedItem as ListBoxItem;
            if (listBoxItem == null)
            {
                return;
            }

            var element = listBoxItem.Tag as XElement;
            if (element == null)
            {
                return;
            }

            while (element != null)
            {
                if (element.Name == @"rule")
                {
                    RuleModel.CurrentRule = element;
                    return;
                }

                element = element.Parent;
            }
        }

        private void ToggleExcept([NotNull] object sender, [NotNull] EventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            var item = sender as ConditionListBoxItem;
            if (item == null)
            {
                return;
            }

            RuleModel.ToggleExcept(item.Element);

            item.Refresh();
            RaiseModified();
        }

        private void ToggleOperator([NotNull] object sender, [NotNull] EventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            var item = sender as ConditionListBoxItem;
            if (item == null)
            {
                return;
            }

            var parent = item.Element.Parent;
            var operatorId = item.OperatorId;

            var element = item.Element;
            while (element != null)
            {
                if (element.GetAttributeValue("uid") == operatorId)
                {
                    parent = element;
                    break;
                }

                element = element.Parent;
            }

            if (parent != null)
            {
                RuleModel.ToggleOperator(parent);
            }

            Render();
            RaiseModified();
        }
    }
}
