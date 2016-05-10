// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.Windows.Documents;
using System.Windows.Media;
using System.Xml.Linq;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Extensions.DataServiceExtensions;
using Sitecore.Rocks.Extensions.GuidExtensions;

namespace Sitecore.Rocks.UI.RuleEditors.Macros
{
    public abstract class RuleEditorMacroBase : IRuleEditorMacro
    {
        private Run run;

        private string value;

        public DatabaseUri DatabaseUri { get; set; }

        public string DefaultValue { get; set; }

        public XElement Element { get; set; }

        public string Id { get; set; }

        public string Parameters { get; set; }

        public string Value
        {
            get { return value; }

            set
            {
                Assert.ArgumentNotNull(value, nameof(value));

                if (Element == null)
                {
                    throw Exceptions.InvalidOperation(@"Element property must be set before Value property.");
                }

                this.value = value;
                Element.SetAttributeValue(Id, value);
            }
        }

        public abstract object GetEditableControl();

        public void GetItemName([NotNull] string itemId, [NotNull] Action<string> completed)
        {
            Assert.ArgumentNotNull(itemId, nameof(itemId));
            Assert.ArgumentNotNull(completed, nameof(completed));

            if (DatabaseUri == DatabaseUri.Empty)
            {
                completed(itemId);
                return;
            }

            if (string.IsNullOrEmpty(itemId))
            {
                completed(string.Empty);
                return;
            }

            Guid guid;
            if (!Guid.TryParse(itemId, out guid))
            {
                completed(string.Empty);
                return;
            }

            var itemUri = new ItemUri(DatabaseUri, new ItemId(guid));

            DatabaseUri.Site.DataService.GetItemHeader(itemUri, header => completed(header.Name));
        }

        public virtual object GetReadOnlyControl()
        {
            var v = GetDisplayValue();

            run = new Run(v)
            {
                Foreground = Brushes.Blue
            };

            if (v.IsGuid())
            {
                GetItemName(v, s => run.Text = s);
            }

            return run;
        }

        [NotNull]
        protected virtual string GetDisplayValue()
        {
            return GetValue();
        }

        [NotNull]
        protected virtual string GetValue()
        {
            if (!string.IsNullOrEmpty(Value))
            {
                return Value;
            }

            return DefaultValue;
        }
    }
}
