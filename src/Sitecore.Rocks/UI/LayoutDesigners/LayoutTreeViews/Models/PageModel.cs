// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Extensions.XElementExtensions;

namespace Sitecore.Rocks.UI.LayoutDesigners.LayoutTreeViews.Models
{
    public class PageModel : INotifyPropertyChanged
    {
        private DeviceModel device;

        public PageModel([NotNull] DatabaseUri databaseUri, [NotNull] XElement layout)
        {
            Assert.ArgumentNotNull(databaseUri, nameof(databaseUri));
            Assert.ArgumentNotNull(layout, nameof(layout));

            DatabaseUri = databaseUri;
            Devices = new ObservableCollection<DeviceModel>();

            ParseLayout(layout);
        }

        [NotNull]
        public DatabaseUri DatabaseUri { get; private set; }

        [CanBeNull]
        public DeviceModel Device
        {
            get { return device; }

            set
            {
                Assert.ArgumentNotNull(value, nameof(value));

                if (value == device)
                {
                    return;
                }

                device = value;

                OnPropertyChanged(nameof(Device));
                OnPropertyChanged(nameof(DeviceName));
            }
        }

        [NotNull]
        public string DeviceName
        {
            get
            {
                var deviceModel = Device;
                return deviceModel != null ? deviceModel.DeviceName : string.Empty;
            }
        }

        [NotNull]
        public ObservableCollection<DeviceModel> Devices { get; }

        public event EventHandler Modified;

        public event PropertyChangedEventHandler PropertyChanged;

        public void RaiseModified()
        {
            var modified = Modified;
            if (modified != null)
            {
                modified(this, EventArgs.Empty);
            }
        }

        public void Save([NotNull] XmlTextWriter output, bool isCopy)
        {
            Assert.ArgumentNotNull(output, nameof(output));

            foreach (var d in Devices)
            {
                d.Save(output, isCopy);
            }
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

        private void ParseLayout([NotNull] XElement layout)
        {
            Debug.ArgumentNotNull(layout, nameof(layout));

            var l0 = layout.Element("layout");
            if (l0 == null || !l0.HasElements)
            {
                l0 = layout.Element("devices");
                if (l0 == null)
                {
                    return;
                }

                foreach (var deviceElement in l0.Elements())
                {
                    var deviceId = new ItemId(new Guid(deviceElement.GetAttributeValue("id")));
                    var deviceName = deviceElement.Value;

                    var d = new DeviceModel(this, deviceId, deviceName);

                    Devices.Add(d);
                }

                Device = Devices.First();
                return;
            }

            foreach (var deviceElement in l0.Elements())
            {
                var deviceId = new ItemId(new Guid(deviceElement.GetAttributeValue("id")));
                var deviceName = deviceElement.GetAttributeValue("n");

                var d = new DeviceModel(this, deviceId, deviceName);

                d.ParseLayout(deviceElement);

                Devices.Add(d);
            }

            Device = Devices.FirstOrDefault();
        }
    }
}
