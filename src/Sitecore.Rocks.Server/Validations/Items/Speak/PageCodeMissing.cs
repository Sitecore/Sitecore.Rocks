// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Data;
using Sitecore.Data.Items;

namespace Sitecore.Rocks.Server.Validations.Items
{
    [Validation("Page is missing 'PageCode' rendering", "SPEAK")]
    public class PageCodeMissing : SpeakPageValidation
    {
        public static readonly ID PageCodeRenderingId = new ID("{DAFAFFB8-74AF-4141-A96A-70B16834CEC6}");

        public override bool CanCheck(string contextName, Item item)
        {
            return contextName == "Site";
        }

        protected override void CheckDevice(ValidationWriter output, Item item, DeviceItem device)
        {
            var renderingReferences = item.Visualization.GetRenderings(device, false);

            foreach (var renderingReference in renderingReferences)
            {
                if (renderingReference.RenderingID == PageCodeRenderingId)
                {
                    if (renderingReference.Placeholder != "Page.Code")
                    {
                        output.Write(SeverityLevel.Error, "'PageCode' rendering is added to incorrect place holder.", string.Format("The 'PageCode' has been added to the '{0}' place holder in the '{1}' device. By convention the 'PageCode' rendering must be added to the 'Page.Code' place holder.", renderingReference.Placeholder, device.Name), string.Format("Change 'PageCode' rendering place holder from '{0}' to 'Page.Code' in the '{1}' device.", renderingReference.Placeholder, device.Name), item);
                    }

                    return;
                }
            }

            output.Write(SeverityLevel.Error, "'PageCode' rendering is missing.", string.Format("The item '{1}' is using the SPEAK layout in the '{0}' device, but does not include the 'PageCode' rendering. The rendering will most probably not function correctly.", device.Name, item.Name), string.Format("Add the 'PageCode' rendering to layout in the '{0}' device.", device.Name), item);
        }
    }
}
