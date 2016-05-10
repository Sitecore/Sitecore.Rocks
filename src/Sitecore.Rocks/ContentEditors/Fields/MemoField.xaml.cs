// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Linq;
using System.Windows.Controls;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Diagnostics;

namespace Sitecore.Rocks.ContentEditors.Fields
{
    [FieldControl("memo"), FieldControl("multi-line text")]
    public partial class MemoField : IReusableFieldControl, ISupportsTextOperations, ISupportsXmlOperations
    {
        public MemoField()
        {
            InitializeComponent();

            Edit.TextChanged += EditChanged;
        }

        [CanBeNull]
        protected Field SourceField { get; private set; }

        public Control GetFocusableControl()
        {
            return Edit;
        }

        public bool IsSupported(Field sourceField)
        {
            Assert.ArgumentNotNull(sourceField, nameof(sourceField));

            return true;
        }

        public void UnsetField()
        {
            SourceField = null;
        }

        public event ValueModifiedEventHandler ValueModified;

        private void EditChanged([NotNull] object sender, [NotNull] TextChangedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            var valueModified = ValueModified;
            if (valueModified != null)
            {
                valueModified();
            }
        }

        Control IFieldControl.GetControl()
        {
            return this;
        }

        string IFieldControl.GetValue()
        {
            return Edit.Text;
        }

        void IFieldControl.SetField(Field sourceField)
        {
            Debug.ArgumentNotNull(sourceField, nameof(sourceField));

            Resizer.FieldId = sourceField.FieldUris.First().FieldId;

            SourceField = sourceField;
        }

        void IFieldControl.SetValue(string value)
        {
            Debug.ArgumentNotNull(value, nameof(value));

            Edit.Text = value;
        }
    }
}
