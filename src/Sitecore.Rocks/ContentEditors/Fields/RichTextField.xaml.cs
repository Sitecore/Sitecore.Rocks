// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Linq;
using System.Windows.Controls;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Text;

namespace Sitecore.Rocks.ContentEditors.Fields
{
    [FieldControl("html"), FieldControl("rich text")]
    public partial class RichTextField : IReusableFieldControl, ISupportsTextOperations, ISupportsXmlOperations
    {
        public RichTextField()
        {
            InitializeComponent();

            Editor.TextChanged += TextChanged;
        }

        [CanBeNull]
        protected Field SourceField { get; set; }

        public Control GetFocusableControl()
        {
            return Editor.Editor;
        }

        public bool IsSupported(Field sourceField)
        {
            Assert.ArgumentNotNull(sourceField, nameof(sourceField));

            return true;
        }

        public void UnsetField()
        {
            SourceField = null;
            Resizer.FieldId = null;
        }

        public event ValueModifiedEventHandler ValueModified;

        Control IFieldControl.GetControl()
        {
            return this;
        }

        string IFieldControl.GetValue()
        {
            return Editor.Text;
        }

        void IFieldControl.SetField(Field sourceField)
        {
            Debug.ArgumentNotNull(sourceField, nameof(sourceField));

            Resizer.FieldId = sourceField.FieldUris.First().FieldId;

            SourceField = sourceField;

            var urlString = new UrlString(SourceField.Source);
            var syntax = urlString.Parameters["syntax"];
            if (syntax == null)
            {
                Editor.LoadXhtmlSyntax();
                return;
            }

            switch (syntax.ToLowerInvariant())
            {
                case "js":
                    Editor.LoadJavaScriptSyntax();
                    break;
                case "css":
                    Editor.LoadCssSyntax();
                    break;
                default:
                    Editor.LoadXhtmlSyntax();
                    break;
            }
        }

        void IFieldControl.SetValue(string value)
        {
            Debug.ArgumentNotNull(value, nameof(value));

            Editor.Text = value;
        }

        private void TextChanged([NotNull] object sender, [NotNull] TextChangedEventArgs args)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(args, nameof(args));

            var valueModified = ValueModified;
            if (valueModified != null)
            {
                valueModified();
            }
        }
    }
}
