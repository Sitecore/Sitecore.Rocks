// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Windows;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Rules.Conditions.Logic;

namespace Sitecore.Rocks.Rules
{
    public class Rule
    {
        private readonly List<RuleActionDescriptor> actionDescriptors = new List<RuleActionDescriptor>();

        private readonly List<RuleConditionDescriptor> conditionDescriptors = new List<RuleConditionDescriptor>();

        private readonly string name;

        public Rule()
        {
            // Intentionally left blank.
        }

        public Rule([NotNull] string name) : this()
        {
            Assert.ArgumentNotNull(name, nameof(name));

            this.name = name;
        }

        public Rule([NotNull] List<RuleConditionDescriptor> ruleConditionDescriptors, [NotNull] RuleActionDescriptor ruleActionDescriptor)
        {
            Assert.ArgumentNotNull(ruleConditionDescriptors, nameof(ruleConditionDescriptors));
            Assert.ArgumentNotNull(ruleActionDescriptor, nameof(ruleActionDescriptor));

            conditionDescriptors = ruleConditionDescriptors;
            actionDescriptors.Add(ruleActionDescriptor);
        }

        public Rule([NotNull] List<RuleConditionDescriptor> ruleConditionDescriptors, [NotNull] IEnumerable<RuleActionDescriptor> ruleActionDescriptors)
        {
            Assert.ArgumentNotNull(ruleConditionDescriptors, nameof(ruleConditionDescriptors));
            Assert.ArgumentNotNull(ruleActionDescriptors, nameof(ruleActionDescriptors));

            conditionDescriptors = ruleConditionDescriptors;
            actionDescriptors.AddRange(ruleActionDescriptors);
        }

        [NotNull]
        public List<RuleActionDescriptor> ActionDescriptors
        {
            get { return actionDescriptors; }
        }

        [NotNull]
        public List<IRuleAction> Actions
        {
            get { return BuildActions(); }
        }

        [CanBeNull]
        public RuleCondition Condition
        {
            get { return BuildCondition(); }
        }

        [NotNull]
        public List<RuleConditionDescriptor> ConditionDescriptors
        {
            get { return conditionDescriptors; }
        }

        public bool IsAborted { get; set; }

        public bool Evaluate([CanBeNull] object ruleContext)
        {
            var c = Condition;
            if (c == null)
            {
                return true;
            }

            var stack = new RuleStack();

            c.Evaluate(ruleContext, stack);

            if (stack.Count == 0)
            {
                return false;
            }

            return (bool)stack.Pop();
        }

        public void Execute([CanBeNull] object ruleContext)
        {
            foreach (var action in Actions)
            {
                if (!action.CanExecute(ruleContext))
                {
                    var typeName = @"null";

                    if (ruleContext != null)
                    {
                        typeName = ruleContext.GetType().FullName;
                    }

                    AppHost.MessageBox(string.Format(Resources.Rule_Execute_The_action___0___cannot_execute_in_the_current_context___1___, action.Text, typeName), Resources.Error, MessageBoxButton.OK, MessageBoxImage.Error);
                    IsAborted = true;
                    return;
                }
            }

            foreach (var action in Actions)
            {
                action.Execute(ruleContext);

                if (IsAborted)
                {
                    return;
                }
            }
        }

        private void ApplyParameters([NotNull] object target, [NotNull] Dictionary<string, string> parameters)
        {
            Debug.ArgumentNotNull(target, nameof(target));
            Debug.ArgumentNotNull(parameters, nameof(parameters));

            foreach (var parameter in parameters)
            {
                var propertyInfo = target.GetType().GetProperty(parameter.Key, BindingFlags.Instance | BindingFlags.Public | BindingFlags.IgnoreCase);
                if (propertyInfo == null)
                {
                    continue;
                }

                propertyInfo.SetValue(target, parameter.Value, null);
            }
        }

        [NotNull]
        private List<IRuleAction> BuildActions()
        {
            var result = new List<IRuleAction>();

            foreach (var descriptor in actionDescriptors)
            {
                ApplyParameters(descriptor.Action, descriptor.Parameters);
                result.Add(descriptor.Action);
            }

            return result;
        }

        [CanBeNull]
        private RuleCondition BuildCondition()
        {
            if (!conditionDescriptors.Any())
            {
                return null;
            }

            RuleCondition root = null;
            RuleCondition current = null;

            foreach (var descriptor in conditionDescriptors)
            {
                if (current == null)
                {
                    current = GetCondition(descriptor);
                    continue;
                }

                if (descriptor.Operator == RuleConditionDescriptorOperator.And)
                {
                    current = new AndCondition(current, GetCondition(descriptor));

                    var binaryCondition = root as BinaryCondition;
                    if (binaryCondition != null)
                    {
                        binaryCondition.RightOperand = current;
                    }
                }
                else
                {
                    var right = GetCondition(descriptor);
                    root = new OrCondition(root ?? current, right);
                    current = right;
                }
            }

            return root ?? current;
        }

        [NotNull]
        private RuleCondition GetCondition([NotNull] RuleConditionDescriptor descriptor)
        {
            Debug.ArgumentNotNull(descriptor, nameof(descriptor));

            ApplyParameters(descriptor.Condition, descriptor.Parameters);

            if (!descriptor.IsNot)
            {
                return descriptor.Condition;
            }

            return new NotCondition(descriptor.Condition);
        }
    }
}
