// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Xml.Linq;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Extensions.GuidExtensions;
using Sitecore.Rocks.Extensions.XElementExtensions;

namespace Sitecore.Rocks.UI.RuleEditors
{
    public class RuleModel
    {
        public RuleModel([NotNull] XElement rules)
        {
            Assert.ArgumentNotNull(rules, nameof(rules));

            Rules = rules;
            CurrentRule = Rules.Elements().FirstOrDefault();
        }

        [CanBeNull]
        public XElement CurrentRule { get; set; }

        public bool IsEmpty
        {
            get { return false; }
        }

        [NotNull]
        public XElement Rules { get; }

        public void AddAction([NotNull] RuleEditor.ActionItem actionItem)
        {
            Assert.ArgumentNotNull(actionItem, nameof(actionItem));

            if (CurrentRule == null)
            {
                return;
            }

            var actionsElement = CurrentRule.Element(@"actions");
            if (actionsElement == null)
            {
                actionsElement = new XElement(@"actions");
                CurrentRule.Add(actionsElement);
            }

            AddNewActionElement(actionsElement, actionItem);
        }

        public void AddCondition([NotNull] RuleEditor.ConditionItem conditionItem)
        {
            Assert.ArgumentNotNull(conditionItem, nameof(conditionItem));

            if (CurrentRule == null)
            {
                return;
            }

            var conditionsElement = CurrentRule.Element(@"conditions");
            if (conditionsElement == null)
            {
                conditionsElement = new XElement(@"conditions");
                CurrentRule.Add(conditionsElement);
            }

            AddNewConditionElement(conditionsElement, conditionItem);
        }

        [NotNull]
        public XElement AddRule()
        {
            var rule = new XElement(@"rule");
            rule.SetAttributeValue(@"uid", Guid.NewGuid().ToShortId());

            var conditions = new XElement(@"conditions");
            conditions.SetAttributeValue(@"uid", Guid.NewGuid().ToShortId());
            rule.Add(conditions);

            var actions = new XElement(@"actions");
            actions.SetAttributeValue(@"uid", Guid.NewGuid().ToShortId());
            rule.Add(actions);

            Rules.Add(rule);

            CurrentRule = rule;

            return rule;
        }

        public void DeleteAction([NotNull] XElement element)
        {
            Assert.ArgumentNotNull(element, nameof(element));

            element.Remove();
        }

        public void DeleteCondition([NotNull] XElement element)
        {
            Assert.ArgumentNotNull(element, nameof(element));

            var conditionsElement = GetConditionsElement(element);
            if (conditionsElement == null)
            {
                return;
            }

            var uid = element.GetAttributeValue("uid");

            var elements = new List<XElement>();

            Flatten(conditionsElement, elements, @"or", string.Empty);

            for (var n = elements.Count - 1; n >= 0; n--)
            {
                var e = elements[n];

                if (e.GetAttributeValue("uid") != uid)
                {
                    continue;
                }

                elements.Remove(e);

                break;
            }

            Deepen(conditionsElement, elements);
        }

        public void DeleteRule([NotNull] XElement element)
        {
            Assert.ArgumentNotNull(element, nameof(element));

            element.Remove();

            if (CurrentRule == element)
            {
                CurrentRule = null;
            }
        }

        public void Flatten([NotNull] XElement parent, [NotNull] List<XElement> elements, [Localizable(false), NotNull] string op, [NotNull] string opid)
        {
            Assert.ArgumentNotNull(parent, nameof(parent));
            Assert.ArgumentNotNull(elements, nameof(elements));
            Assert.ArgumentNotNull(op, nameof(op));
            Assert.ArgumentNotNull(opid, nameof(opid));

            if (parent.Name.LocalName == @"condition")
            {
                elements.Add(parent);
                parent.SetAttributeValue(@"_operator", op);
                parent.SetAttributeValue(@"_operatorid", opid);
                return;
            }

            if (parent.Name.LocalName == @"not")
            {
                var operand = parent.Element(0);
                if (operand == null)
                {
                    return;
                }

                Flatten(operand, elements, op + @"not", opid);

                return;
            }

            var left = parent.Element(0);
            if (left == null)
            {
                return;
            }

            Flatten(left, elements, op, opid);

            var right = parent.Element(1);
            if (right == null)
            {
                return;
            }

            Flatten(right, elements, parent.Name.LocalName, parent.GetAttributeValue("uid"));
        }

        public void MoveActionDown([NotNull] XElement element)
        {
            Assert.ArgumentNotNull(element, nameof(element));

            var next = element.ElementsAfterSelf().FirstOrDefault();
            if (next == null)
            {
                return;
            }

            element.Remove();
            next.AddAfterSelf(element);
        }

        public void MoveActionUp([NotNull] XElement element)
        {
            Assert.ArgumentNotNull(element, nameof(element));

            var previous = element.ElementsBeforeSelf().LastOrDefault();
            if (previous == null)
            {
                return;
            }

            element.Remove();
            previous.AddBeforeSelf(element);
        }

        public void MoveConditionDown([NotNull] XElement element)
        {
            Assert.ArgumentNotNull(element, nameof(element));

            var conditionsElement = GetConditionsElement(element);
            if (conditionsElement == null)
            {
                return;
            }

            MoveCondition(conditionsElement, element, "down");
        }

        public void MoveConditionUp([NotNull] XElement element)
        {
            Assert.ArgumentNotNull(element, nameof(element));

            var conditionsElement = GetConditionsElement(element);
            if (conditionsElement == null)
            {
                return;
            }

            MoveCondition(conditionsElement, element, "up");
        }

        public void RemoveOperatorAttributes()
        {
            RemoveOperatorAttributes(Rules);
        }

        public void ToggleExcept([NotNull] XElement element)
        {
            Assert.ArgumentNotNull(element, nameof(element));

            var isExcept = element.GetAttributeValue("except") == @"true";
            element.SetAttributeValue(@"except", isExcept ? null : @"true");
        }

        public void ToggleOperator([NotNull] XElement element)
        {
            Assert.ArgumentNotNull(element, nameof(element));

            var conditionsElement = GetConditionsElement(element);
            if (conditionsElement == null)
            {
                return;
            }

            var name = element.Name.LocalName;
            switch (name)
            {
                case "and":
                    name = @"or";
                    break;
                case "or":
                    name = @"and";
                    break;
            }

            element.Name = name;

            var elements = new List<XElement>();

            Flatten(conditionsElement, elements, "or", string.Empty);
            Deepen(conditionsElement, elements);
        }

        [NotNull]
        public override string ToString()
        {
            return Rules.ToString();
        }

        protected void MoveCondition([NotNull] XElement conditionsElement, [NotNull] XElement element, [Localizable(false), NotNull] string direction)
        {
            Debug.ArgumentNotNull(conditionsElement, nameof(conditionsElement));
            Debug.ArgumentNotNull(element, nameof(element));
            Debug.ArgumentNotNull(direction, nameof(direction));

            var elements = new List<XElement>();

            Flatten(conditionsElement, elements, "or", string.Empty);

            var index = elements.IndexOf(element);

            if (direction == "up")
            {
                if (index > 0)
                {
                    elements.RemoveAt(index);
                    elements.Insert(index - 1, element);
                }
            }
            else
            {
                if (index < elements.Count - 1)
                {
                    elements.RemoveAt(index);
                    elements.Insert(index + 1, element);
                }
            }

            Deepen(conditionsElement, elements);
        }

        private void AddNewActionElement([NotNull] XElement actionsElement, [NotNull] RuleEditor.ActionItem actionItem)
        {
            Debug.ArgumentNotNull(actionsElement, nameof(actionsElement));
            Debug.ArgumentNotNull(actionItem, nameof(actionItem));

            var actionElement = new XElement(@"action");
            actionElement.Add(new XAttribute(@"id", actionItem.Id));
            actionElement.Add(new XAttribute(@"uid", Guid.NewGuid().ToString(@"B").ToUpperInvariant()));

            actionsElement.Add(actionElement);
        }

        private void AddNewConditionElement([NotNull] XElement conditionsElement, [NotNull] RuleEditor.ConditionItem conditionItem)
        {
            Debug.ArgumentNotNull(conditionsElement, nameof(conditionsElement));
            Debug.ArgumentNotNull(conditionItem, nameof(conditionItem));

            var conditionElement = new XElement(@"condition");
            conditionElement.Add(new XAttribute(@"id", conditionItem.Id));
            conditionElement.Add(new XAttribute(@"uid", Guid.NewGuid().ToShortId()));

            var root = conditionsElement.Element(0);

            if (root == null)
            {
                conditionsElement.Add(conditionElement);
                return;
            }

            var rightLeaf = GetRightLeaf(root);

            var and = new XElement(@"and");
            and.Add(new XAttribute(@"uid", Guid.NewGuid().ToShortId()));

            rightLeaf.ReplaceWith(and);

            and.Add(rightLeaf);
            and.Add(conditionElement);
        }

        private void Deepen([NotNull] XElement conditionsElement, [NotNull] List<XElement> elements)
        {
            Debug.ArgumentNotNull(conditionsElement, nameof(conditionsElement));
            Debug.ArgumentNotNull(elements, nameof(elements));

            conditionsElement.RemoveNodes();

            if (elements.Count == 0)
            {
                return;
            }

            var current = elements[0];
            if (current == null)
            {
                return;
            }

            current.SetAttributeValue(@"_operator", null);
            current.SetAttributeValue(@"_operatorid", null);

            var root = current;

            for (var n = 1; n < elements.Count; n++)
            {
                var element = elements[n];

                var op = element.GetAttributeValue("_operator");
                element.SetAttributeValue(@"_operator", null);
                element.SetAttributeValue(@"_operatorid", null);

                var operatorElement = new XElement(op);
                operatorElement.SetAttributeValue(@"uid", Guid.NewGuid().ToShortId());

                if (op == @"and")
                {
                    Assert.IsNotNull(current, "current  is null");

                    var parent = current.Parent;
                    if (parent != null)
                    {
                        current.Remove();
                        parent.Add(operatorElement);
                    }

                    operatorElement.Add(current);
                    operatorElement.Add(element);

                    if (current == root)
                    {
                        root = operatorElement;
                    }

                    current = operatorElement.Element(1);
                }
                else
                {
                    operatorElement.Add(root);
                    operatorElement.Add(element);

                    current = operatorElement.Element(1);

                    root = operatorElement;
                }
            }

            conditionsElement.Add(root);
        }

        [CanBeNull]
        private XElement GetConditionsElement([NotNull] XElement element)
        {
            Debug.ArgumentNotNull(element, nameof(element));

            while (element != null)
            {
                if (element.Name == @"conditions")
                {
                    return element;
                }

                element = element.Parent;
            }

            return null;
        }

        [NotNull]
        private static XElement GetRightLeaf([NotNull] XElement element)
        {
            Debug.ArgumentNotNull(element, nameof(element));

            var right = element.Element(1);
            return right != null ? GetRightLeaf(right) : element;
        }

        private void RemoveOperatorAttributes([NotNull] XElement element)
        {
            Debug.ArgumentNotNull(element, nameof(element));

            element.SetAttributeValue(@"_operator", null);
            element.SetAttributeValue(@"_operatorid", null);

            foreach (var child in element.Elements())
            {
                RemoveOperatorAttributes(child);
            }
        }
    }
}
