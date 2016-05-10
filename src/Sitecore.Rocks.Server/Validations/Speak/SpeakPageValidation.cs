// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Data;
using Sitecore.Data.Items;
using Sitecore.Rocks.Server.Pipelines.GetFieldValue;

namespace Sitecore.Rocks.Server.Validations
{
    public abstract class SpeakPageValidation : ItemValidation
    {
        public static readonly ID Page = new ID("{4F72A793-BDBC-4262-9523-6AADD8FEA487}");

        public override void Check(ValidationWriter output, Item item)
        {
            var layout = GetFieldValuePipeline.Run().WithParameters(item.Fields[FieldIDs.LayoutField]).Value ?? string.Empty;
            if (string.IsNullOrEmpty(layout))
            {
                return;
            }

            var devices = item.Database.GetItem(ItemIDs.DevicesRoot);

            foreach (Item deviceItem in devices.Children)
            {
                var device = new DeviceItem(deviceItem);

                if (item.Visualization.GetLayoutID(device) == Page)
                {
                    CheckDevice(output, item, device);
                }
            }
        }

        protected abstract void CheckDevice(ValidationWriter output, Item item, DeviceItem device);
    }
}
