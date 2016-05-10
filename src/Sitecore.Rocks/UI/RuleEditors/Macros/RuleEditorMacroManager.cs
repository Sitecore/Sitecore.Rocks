// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.Collections.Generic;
using System.Xml.Linq;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Extensibility;
using Sitecore.Rocks.Extensions.XElementExtensions;

namespace Sitecore.Rocks.UI.RuleEditors.Macros
{
    [ExtensibilityInitialization(PreInit = "Clear")]
    public static class RuleEditorMacroManager
    {
        private static readonly string ruleEditorMacroInterface = typeof(IRuleEditorMacro).FullName;

        private static readonly Dictionary<string, RuleEditorMacro> types = new Dictionary<string, RuleEditorMacro>();

        [NotNull]
        public static Dictionary<string, RuleEditorMacro> Macros
        {
            get { return types; }
        }

        public static void Add([NotNull] string typeName, [NotNull] RuleEditorMacro macro)
        {
            Assert.ArgumentNotNull(typeName, nameof(typeName));
            Assert.ArgumentNotNull(macro, nameof(macro));

            Macros[typeName] = macro;
        }

        [UsedImplicitly]
        public static void Clear()
        {
            Macros.Clear();
        }

        [NotNull]
        public static IRuleEditorMacro GetDefault()
        {
            return new TextMacro();
        }

        [CanBeNull]
        public static IRuleEditorMacro GetInstance([NotNull] string typeName)
        {
            Assert.ArgumentNotNull(typeName, nameof(typeName));

            typeName = typeName.ToUpperInvariant();

            RuleEditorMacro ruleEditorMacro;

            if (!types.TryGetValue(typeName, out ruleEditorMacro))
            {
                return null;
            }

            var constructor = ruleEditorMacro.Type.GetConstructor(Type.EmptyTypes);
            if (constructor == null)
            {
                return null;
            }

            return constructor.Invoke(null) as IRuleEditorMacro;
        }

        [CanBeNull]
        public static object GetMacroControl([NotNull] XElement element, [NotNull] DatabaseUri databaseUri, [NotNull] string macroText, bool isEditable)
        {
            Assert.ArgumentNotNull(element, nameof(element));
            Assert.ArgumentNotNull(databaseUri, nameof(databaseUri));
            Assert.ArgumentNotNull(macroText, nameof(macroText));

            var parts = macroText.Split(',');
            if (parts.Length < 4)
            {
                return null;
            }

            var id = parts[0];
            var type = parts[1];
            var parameters = parts[2];
            var defaultValue = parts[3];

            var value = element.GetAttributeValue(id);

            var macro = GetInstance(type) ?? GetDefault();

            macro.Element = element;
            macro.DatabaseUri = databaseUri;
            macro.Id = id;
            macro.DefaultValue = defaultValue;
            macro.Parameters = parameters;
            macro.Value = value;

            if (isEditable)
            {
                return macro.GetEditableControl();
            }

            return macro.GetReadOnlyControl();
        }

        public static void LoadType([NotNull] Type type, [NotNull] RuleEditorMacroAttribute attribute)
        {
            Assert.ArgumentNotNull(type, nameof(type));
            Assert.ArgumentNotNull(attribute, nameof(attribute));

            var i = type.GetInterface(ruleEditorMacroInterface);
            if (i == null)
            {
                return;
            }

            var macro = new RuleEditorMacro
            {
                Type = type,
                MacroName = attribute.MacroName
            };

            Add(attribute.MacroName.ToUpperInvariant(), macro);
        }

        public class RuleEditorMacro
        {
            [NotNull]
            public string MacroName { get; set; }

            [NotNull]
            public Type Type { get; set; }
        }
    }
}
