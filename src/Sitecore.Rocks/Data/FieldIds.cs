// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

namespace Sitecore.Rocks.Data
{
    public static class FieldIds
    {
        public static readonly FieldId Icon;

        public static readonly FieldId StandardValues;

        static FieldIds()
        {
            Icon = IdManager.GetFieldId("/sitecore/templates/System/Templates/Sections/Appearance/Appearance/__Icon");
            StandardValues = IdManager.GetFieldId("/sitecore/templates/System/Templates/Sections/Advanced/Advanced/__Standard values");
        }
    }
}
