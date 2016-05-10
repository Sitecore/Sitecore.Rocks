// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Extensions.XElementExtensions;
using Sitecore.Rocks.Rules;

namespace Sitecore.Rocks.UI.Macros
{
    public static class MacroManager
    {
        private static readonly List<Macro> macros = new List<Macro>();

        static MacroManager()
        {
            Load();
        }

        [NotNull]
        public static IEnumerable<Macro> Macros
        {
            get { return macros; }
        }

        public static void Add([NotNull] Macro macro)
        {
            Assert.ArgumentNotNull(macro, nameof(macro));

            macros.Add(macro);

            Save();
        }

        public static void Delete([NotNull] Macro macro)
        {
            Assert.ArgumentNotNull(macro, nameof(macro));

            macros.Remove(macro);
            Save();
        }

        [NotNull]
        public static IEnumerable<Macro> GetAllMacros()
        {
            return macros ?? Enumerable.Empty<Macro>();
        }

        [NotNull]
        public static IEnumerable<Macro> GetMacros([CanBeNull] object context)
        {
            return macros.Where(macro => macro.Rule.Evaluate(context) && macro.Rule.Actions.All(a => a.CanExecute(context))).ToList();
        }

        public static bool HasMacros([CanBeNull] object context)
        {
            return macros.Any(macro => macro.Rule.Evaluate(context) && macro.Rule.Actions.All(a => a.CanExecute(context)));
        }

        public static void Save()
        {
            var fileName = AppHost.Settings.Options.MacrosFileName;
            Directory.CreateDirectory(Path.GetDirectoryName(fileName));

            var writer = new StreamWriter(fileName, false, Encoding.UTF8);
            var output = new XmlTextWriter(writer)
            {
                Formatting = Formatting.Indented,
                Indentation = 2
            };

            output.WriteStartElement(@"macros");

            foreach (var macro in macros)
            {
                WriteMacro(output, macro);
            }

            output.WriteEndElement();

            output.Flush();
            writer.Close();
        }

        private static T GetInstance<T>([NotNull] string typeName)
        {
            Debug.ArgumentNotNull(typeName, nameof(typeName));

            var type = Type.GetType(typeName);

            return (T)Activator.CreateInstance(type);
        }

        private static void Load()
        {
            var fileName = AppHost.Settings.Options.MacrosFileName;
            if (!File.Exists(fileName))
            {
                return;
            }

            var doc = XDocument.Load(fileName);
            var root = doc.Root;
            if (root == null)
            {
                Trace.TraceError("Root is empty");
                return;
            }

            macros.Clear();

            foreach (var element in root.Elements())
            {
                LoadMacro(element);
            }
        }

        private static void LoadActions([NotNull] XElement actions, [NotNull] Rule rule)
        {
            Debug.ArgumentNotNull(actions, nameof(actions));
            Debug.ArgumentNotNull(rule, nameof(rule));

            foreach (var element in actions.Elements())
            {
                var action = new RuleActionDescriptor
                {
                    Action = GetInstance<IRuleAction>(element.GetAttributeValue("type"))
                };

                var parameters = element.Element(@"parameters");
                if (parameters != null)
                {
                    LoadParameters(parameters, action.Parameters);
                }

                rule.ActionDescriptors.Add(action);
            }
        }

        private static void LoadConditions([NotNull] XElement conditions, [NotNull] Rule rule)
        {
            Debug.ArgumentNotNull(conditions, nameof(conditions));
            Debug.ArgumentNotNull(rule, nameof(rule));

            foreach (var element in conditions.Elements())
            {
                var condition = new RuleConditionDescriptor
                {
                    IsNot = element.GetAttributeValue("not") == @"true",
                    Operator = element.GetAttributeValue("operator") == @"and" ? RuleConditionDescriptorOperator.And : RuleConditionDescriptorOperator.Or,
                    Condition = GetInstance<RuleCondition>(element.GetAttributeValue("type"))
                };

                var parameters = element.Element(@"parameters");
                if (parameters != null)
                {
                    LoadParameters(parameters, condition.Parameters);
                }

                rule.ConditionDescriptors.Add(condition);
            }
        }

        private static void LoadMacro([NotNull] XElement element)
        {
            Debug.ArgumentNotNull(element, nameof(element));

            var name = element.GetAttributeValue("name");
            var scope = element.GetAttributeValue("scope") == @"peritem" ? MacroScope.PerItem : MacroScope.Once;
            var rule = new Rule();

            var conditions = element.Element(@"conditions");
            if (conditions != null)
            {
                LoadConditions(conditions, rule);
            }

            var actions = element.Element(@"actions");
            if (conditions != null)
            {
                LoadActions(actions, rule);
            }

            var macro = new Macro(rule, name)
            {
                Scope = scope
            };
            macros.Add(macro);
        }

        private static void LoadParameters([NotNull] XElement parameters, [NotNull] Dictionary<string, string> dictionary)
        {
            Debug.ArgumentNotNull(parameters, nameof(parameters));
            Debug.ArgumentNotNull(dictionary, nameof(dictionary));

            foreach (var element in parameters.Elements())
            {
                var key = element.GetAttributeValue("key");
                var value = element.Value;

                dictionary[key] = value;
            }
        }

        private static void WriteActions([NotNull] XmlTextWriter output, [NotNull] List<RuleActionDescriptor> actions)
        {
            Debug.ArgumentNotNull(output, nameof(output));
            Debug.ArgumentNotNull(actions, nameof(actions));

            output.WriteStartElement(@"actions");

            foreach (var descriptor in actions)
            {
                output.WriteStartElement(@"action");
                output.WriteAttributeString(@"type", descriptor.Action.GetType().FullName);

                WriteParameters(output, descriptor.Parameters);

                output.WriteEndElement();
            }

            output.WriteEndElement();
        }

        private static void WriteConditions([NotNull] XmlTextWriter output, [NotNull] List<RuleConditionDescriptor> conditions)
        {
            Debug.ArgumentNotNull(output, nameof(output));
            Debug.ArgumentNotNull(conditions, nameof(conditions));

            output.WriteStartElement(@"conditions");

            foreach (var descriptor in conditions)
            {
                output.WriteStartElement(@"condition");

                output.WriteAttributeString(@"operator", descriptor.Operator == RuleConditionDescriptorOperator.And ? @"and" : @"or");
                output.WriteAttributeString(@"not", descriptor.IsNot ? @"true" : @"false");
                output.WriteAttributeString(@"type", descriptor.Condition.GetType().FullName);

                WriteParameters(output, descriptor.Parameters);

                output.WriteEndElement();
            }

            output.WriteEndElement();
        }

        private static void WriteMacro([NotNull] XmlTextWriter output, [NotNull] Macro macro)
        {
            Debug.ArgumentNotNull(output, nameof(output));
            Debug.ArgumentNotNull(macro, nameof(macro));

            output.WriteStartElement(@"macro");
            output.WriteAttributeString(@"name", macro.Text);
            output.WriteAttributeString(@"scope", macro.Scope == MacroScope.PerItem ? @"peritem" : @"once");

            WriteConditions(output, macro.Rule.ConditionDescriptors);
            WriteActions(output, macro.Rule.ActionDescriptors);

            output.WriteEndElement();
        }

        private static void WriteParameters([NotNull] XmlTextWriter output, [NotNull] Dictionary<string, string> parameters)
        {
            Debug.ArgumentNotNull(output, nameof(output));
            Debug.ArgumentNotNull(parameters, nameof(parameters));

            output.WriteStartElement(@"parameters");

            foreach (var parameter in parameters)
            {
                output.WriteStartElement(@"parameter");
                output.WriteAttributeString(@"key", parameter.Key);
                output.WriteValue(parameter.Value);
                output.WriteEndElement();
            }

            output.WriteEndElement();
        }
    }
}
