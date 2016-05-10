// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.ComponentModel;
using System.Drawing.Design;
using System.Windows.Media.Imaging;
using System.Xml;
using System.Xml.Linq;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Extensions.XElementExtensions;
using Sitecore.Rocks.Extensions.XmlTextWriterExtensions;
using Sitecore.Rocks.UI.LayoutDesigners.Properties;

namespace Sitecore.Rocks.UI.LayoutDesigners.Items
{
    public class PlaceholderItem : LayoutDesignerItem
    {
        private string id;

        private string metaDataItemId;

        public PlaceholderItem([NotNull] DatabaseUri databaseUri)
        {
            Assert.ArgumentNotNull(databaseUri, nameof(databaseUri));

            DatabaseUri = databaseUri;
            Id = string.Empty;
            MetaDataItemId = string.Empty;
            UniqueId = string.Empty;

            Icon = Icon.Empty;
        }

        public PlaceholderItem([NotNull] DatabaseUri databaseUri, [NotNull] XElement element)
        {
            Assert.ArgumentNotNull(databaseUri, nameof(databaseUri));
            Assert.ArgumentNotNull(element, nameof(element));

            DatabaseUri = databaseUri;
            Id = element.GetAttributeValue("key");
            MetaDataItemId = element.GetAttributeValue("md");
            UniqueId = element.GetAttributeValue("uid");

            Icon = new Icon(databaseUri.Site, element.GetAttributeValue("ic"));
        }

        [NotNull, Browsable(false)]
        public DatabaseUri DatabaseUri { get; private set; }

        [CanBeNull, Browsable(false)]
        public Icon Icon { get; set; }

        [Browsable(false)]
        public BitmapImage IconSource
        {
            get
            {
                if (Icon == null)
                {
                    return null;
                }

                return Icon.GetSource();
            }
        }

        [NotNull, Category("Behavior"), Description("The key of the place holder."), DisplayName("Key")]
        public string Id
        {
            get { return id; }

            set
            {
                if (id == value)
                {
                    return;
                }

                id = value;
                RaiseModified();
            }
        }

        [NotNull, Category("Behavior"), Description("The ID of the item that holds meta data for this place holder."), Editor(typeof(MetaDataItemIdTypeEditor), typeof(UITypeEditor))]
        public string MetaDataItemId
        {
            get { return metaDataItemId; }

            set
            {
                if (metaDataItemId == value)
                {
                    return;
                }

                metaDataItemId = value;
                RaiseModified();
            }
        }

        [NotNull, Browsable(false)]
        public string Name => "Placeholder";

        [NotNull, Browsable(false)]
        public string UniqueId { get; set; }

        public override void Commit()
        {
        }

        public override void Write(XmlTextWriter output, bool isCopy)
        {
            Assert.ArgumentNotNull(output, nameof(output));

            output.WriteStartElement(@"p");

            output.WriteAttributeStringNotEmpty(@"key", Id);
            output.WriteAttributeStringNotEmpty(@"uid", UniqueId);
            output.WriteAttributeStringNotEmpty(@"md", MetaDataItemId);

            if (isCopy)
            {
                output.WriteAttributeStringNotEmpty(@"ic", Icon.IconPath);
            }

            output.WriteEndElement();
        }
    }
}
