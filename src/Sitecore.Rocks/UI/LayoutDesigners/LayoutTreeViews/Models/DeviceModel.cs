// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Extensions.XElementExtensions;
using Sitecore.Rocks.Extensions.XmlTextWriterExtensions;
using Sitecore.Rocks.IO;
using Sitecore.Rocks.Shell.ComponentModel;
using Sitecore.Rocks.UI.LayoutDesigners.Extensions;
using Sitecore.Rocks.UI.LayoutDesigners.Items;

namespace Sitecore.Rocks.UI.LayoutDesigners.LayoutTreeViews.Models
{
    public class DeviceModel : IRenderingContainer, INotifyPropertyChanged
    {
        private readonly ObservableCollection<RenderingItem> renderings = new ObservableCollection<RenderingItem>();

        private string layoutId;

        public DeviceModel([NotNull] PageModel pageModel, [NotNull] ItemId deviceId, [NotNull] string deviceName)
        {
            Assert.ArgumentNotNull(pageModel, nameof(pageModel));
            Assert.ArgumentNotNull(deviceId, nameof(deviceId));
            Assert.ArgumentNotNull(deviceName, nameof(deviceName));

            PageModel = pageModel;
            DeviceId = deviceId;
            DeviceName = deviceName;
            Icon = Icon.Empty;
            LayoutPlaceHolders = string.Empty;
            LayoutId = string.Empty;
        }

        public DatabaseUri DatabaseUri => PageModel.DatabaseUri;

        [NotNull]
        public ItemId DeviceId { get; }

        [NotNull]
        public string DeviceName { get; }

        [NotNull]
        public Icon Icon { get; private set; }

        public string Layout
        {
            get
            {
                var output = new OutputWriter(new StringWriter());

                Save(output, false);

                return output.ToString();
            }
        }

        [NotNull]
        public string LayoutId
        {
            get { return layoutId; }

            set
            {
                Assert.ArgumentNotNull(value, nameof(value));

                if (value == layoutId)
                {
                    return;
                }

                layoutId = value;
                OnPropertyChanged(nameof(LayoutId));
            }
        }

        [NotNull]
        public string LayoutPlaceHolders { get; set; }

        [NotNull]
        public PageModel PageModel { get; }

        [NotNull]
        public ObservableCollection<RenderingItem> Renderings => renderings;

        IEnumerable<RenderingItem> IRenderingContainer.Renderings => Renderings;

        public void Delete([NotNull] RenderingItem rendering)
        {
            Assert.ArgumentNotNull(rendering, nameof(rendering));

            var renderingList = new List<RenderingItem>
            {
                rendering
            };

            foreach (var placeHolder in rendering.GetPlaceHolderNames())
            {
                FindRenderingsInPlaceHolder(renderingList, placeHolder);
            }

            foreach (var r in renderingList)
            {
                Renderings.Remove(r);
            }

            PageModel.RaiseModified();
        }

        public void GetDataBindingValues(RenderingItem renderingItem, DynamicProperty dynamicProperty, List<string> values)
        {
            Assert.ArgumentNotNull(renderingItem, nameof(renderingItem));
            Assert.ArgumentNotNull(dynamicProperty, nameof(dynamicProperty));
            Assert.ArgumentNotNull(values, nameof(values));

            this.GetRenderingContainerDataBindingValues(renderingItem, dynamicProperty, values);
        }

        public void ParseLayout([NotNull] XElement device)
        {
            Assert.ArgumentNotNull(device, nameof(device));

            LayoutId = device.GetAttributeValue("l");
            LayoutPlaceHolders = device.GetAttributeValue("ph");
            Icon = new Icon(PageModel.DatabaseUri.Site, device.GetAttributeValue("ic"));

            foreach (var rendering in device.Elements().Select(r => new RenderingItem(this, DatabaseUri, r)))
            {
                rendering.Modified += (sender, args) => PageModel.RaiseModified();

                Renderings.Add(rendering);
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public void Save([NotNull] XmlTextWriter output, bool isCopy)
        {
            Assert.ArgumentNotNull(output, nameof(output));

            output.WriteStartElement(@"d");
            output.WriteAttributeStringNotEmpty(@"id", DeviceId.ToString());
            output.WriteAttributeStringNotEmpty(@"l", LayoutId);

            foreach (var item in Renderings)
            {
                var listItem = item as LayoutDesignerItem;

                if (listItem != null)
                {
                    listItem.Commit();
                    listItem.Write(output, isCopy);
                }
            }

            output.WriteEndElement();
        }

        public override string ToString()
        {
            return DeviceName;
        }

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([NotNull] string propertyName)
        {
            Debug.ArgumentNotNull(propertyName, nameof(propertyName));
            var handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        private void FindRenderingsInPlaceHolder([NotNull] List<RenderingItem> renderingList, [NotNull] string placeHolderName)
        {
            Debug.ArgumentNotNull(renderingList, nameof(renderingList));
            Debug.ArgumentNotNull(placeHolderName, nameof(placeHolderName));

            foreach (var rendering in Renderings.Where(rendering => rendering.PlaceholderKey.ToString() == placeHolderName))
            {
                renderingList.Add(rendering);

                foreach (var p in rendering.GetPlaceHolderNames())
                {
                    FindRenderingsInPlaceHolder(renderingList, p);
                }
            }
        }
    }
}
