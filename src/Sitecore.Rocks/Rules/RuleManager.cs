// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.Collections.Generic;
using System.Linq;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Extensibility;
using Sitecore.Rocks.Rules.ParameterEditors;

namespace Sitecore.Rocks.Rules
{
    [ExtensibilityInitialization(PreInit = "Clear")]
    public static class RuleManager
    {
        private static readonly List<RuleActionInfo> actions = new List<RuleActionInfo>();

        private static readonly List<RuleConditionInfo> conditions = new List<RuleConditionInfo>();

        private static readonly List<RuleParameterEditorInfo> parameterEditors = new List<RuleParameterEditorInfo>();

        [NotNull]
        public static IEnumerable<RuleConditionInfo> Conditions
        {
            get { return conditions; }
        }

        [UsedImplicitly]
        public static void Clear()
        {
            conditions.Clear();
            actions.Clear();
            parameterEditors.Clear();
        }

        [NotNull]
        public static IEnumerable<RuleActionInfo> GetActions([CanBeNull] object parameter)
        {
            return actions.ToList();
        }

        [NotNull]
        public static IRuleParameterEditor GetParameterEditor([NotNull] string editorName)
        {
            Assert.ArgumentNotNull(editorName, nameof(editorName));

            IRuleParameterEditor result = null;

            var editor = parameterEditors.FirstOrDefault(parameterEditor => parameterEditor.EditorName == editorName);
            if (editor != null)
            {
                result = Activator.CreateInstance(editor.Type) as IRuleParameterEditor;
            }

            if (result == null)
            {
                result = new ValueRuleParameterEditor();
            }

            return result;
        }

        public static void LoadType([NotNull] Type type, [NotNull] RuleConditionAttribute ruleConditionAttribute)
        {
            Assert.ArgumentNotNull(type, nameof(type));
            Assert.ArgumentNotNull(ruleConditionAttribute, nameof(ruleConditionAttribute));

            conditions.Add(new RuleConditionInfo(type, ruleConditionAttribute));
        }

        public static void LoadType([NotNull] Type type, [NotNull] RuleActionAttribute ruleActionAttribute)
        {
            Assert.ArgumentNotNull(type, nameof(type));
            Assert.ArgumentNotNull(ruleActionAttribute, nameof(ruleActionAttribute));

            actions.Add(new RuleActionInfo(type, ruleActionAttribute));
        }

        public static void LoadType([NotNull] Type type, [NotNull] RuleParameterAttribute ruleParameterAttribute)
        {
            Assert.ArgumentNotNull(type, nameof(type));
            Assert.ArgumentNotNull(ruleParameterAttribute, nameof(ruleParameterAttribute));

            var parameter = new RuleParameterEditorInfo
            {
                Type = type,
                EditorName = ruleParameterAttribute.EditorName
            };

            parameterEditors.Add(parameter);
        }

        public class RuleActionInfo
        {
            public RuleActionInfo([NotNull] Type type, [NotNull] RuleActionAttribute attribute)
            {
                Assert.ArgumentNotNull(type, nameof(type));
                Assert.ArgumentNotNull(attribute, nameof(attribute));

                var list = new List<string>();

                Type = type;
                Attribute = attribute;
            }

            [NotNull]
            public RuleActionAttribute Attribute { get; private set; }

            [NotNull]
            public Type Type { get; }

            [NotNull]
            public IRuleAction GetInstance()
            {
                return (IRuleAction)Activator.CreateInstance(Type);
            }
        }

        public class RuleConditionInfo
        {
            public RuleConditionInfo([NotNull] Type type, [NotNull] RuleConditionAttribute attribute)
            {
                Assert.ArgumentNotNull(type, nameof(type));
                Assert.ArgumentNotNull(attribute, nameof(attribute));

                Type = type;
                Attribute = attribute;
            }

            [NotNull]
            public RuleConditionAttribute Attribute { get; private set; }

            [NotNull]
            public Type Type { get; }

            [NotNull]
            public RuleCondition GetInstance()
            {
                return (RuleCondition)Activator.CreateInstance(Type);
            }
        }

        public class RuleParameterEditorInfo
        {
            [NotNull]
            public string EditorName { get; set; }

            [NotNull]
            public Type Type { get; set; }
        }
    }
}
