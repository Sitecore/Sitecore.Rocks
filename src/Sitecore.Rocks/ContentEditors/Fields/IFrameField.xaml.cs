// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Linq;
using System.Windows.Controls;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Text;

namespace Sitecore.Rocks.ContentEditors.Fields
{
    [FieldControl("iframe")]
    public partial class IframeField : IReusableFieldControl
    {
        private string value;

        public IframeField()
        {
            InitializeComponent();
        }

        public Control GetControl()
        {
            return this;
        }

        public Control GetFocusableControl()
        {
            return null;
        }

        public string GetValue()
        {
            return value;
        }

        public bool IsSupported(Field sourceField)
        {
            Assert.ArgumentNotNull(sourceField, nameof(sourceField));

            return true;
        }

        public void SetField(Field sourceField)
        {
            Assert.ArgumentNotNull(sourceField, nameof(sourceField));

            Resizer.FieldId = sourceField.FieldUris.First().FieldId;

            value = sourceField.Value;

            var fieldUri = sourceField.FieldUris.First();

            var url = new UrlString(sourceField.Source);

            url["id"] = fieldUri.ItemId.ToString();
            url["la"] = fieldUri.Language.ToString();
            url["vs"] = fieldUri.Version.ToString();
            url["sc_content"] = fieldUri.DatabaseName.ToString();

            Browser.Navigate(AppHost.Browsers.GetUrl(fieldUri.Site, url.ToString()));
        }

        public void SetValue(string value)
        {
            Assert.ArgumentNotNull(value, nameof(value));

            if (value == this.value)
            {
                return;
            }

            this.value = value;

            var valueModified = ValueModified;
            if (valueModified != null)
            {
                valueModified();
            }
        }

        public void UnsetField()
        {
        }

        public event ValueModifiedEventHandler ValueModified;
    }
}
