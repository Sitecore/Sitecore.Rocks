// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Xml.Linq;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Data;

namespace Sitecore.Rocks.UI.RuleEditors.Macros
{
    public interface IRuleEditorMacro
    {
        [NotNull]
        DatabaseUri DatabaseUri { get; set; }

        [NotNull]
        string DefaultValue { get; set; }

        [NotNull]
        XElement Element { get; set; }

        [NotNull]
        string Id { get; set; }

        [NotNull]
        string Parameters { get; set; }

        [NotNull]
        string Value { get; set; }

        [NotNull]
        object GetEditableControl();

        [NotNull]
        object GetReadOnlyControl();
    }
}
