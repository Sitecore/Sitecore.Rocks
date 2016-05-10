// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Collections.Generic;
using Sitecore.Rocks.CodeGeneration.CodeFirstTemplates.Templates;
using Sitecore.Rocks.Diagnostics;

namespace Sitecore.Rocks.CodeGeneration.CodeFirstTemplates.FieldTypes
{
    [FieldTypeHandler]
    public class StandardFieldTypes : IFieldTypeHandler
    {
        private readonly Dictionary<string, string> fieldTypes = new Dictionary<string, string>();

        public StandardFieldTypes()
        {
            fieldTypes["System.String"] = "Single-Line Text";
            fieldTypes["System.Boolean"] = "Checkbox";
            fieldTypes["System.DateTime"] = "Datetime";
            fieldTypes["System.Int32"] = "Integer";
            fieldTypes["System.Int64"] = "Integer";
            fieldTypes["System.Decimal"] = "Number";
            fieldTypes["System.Double"] = "Number";
            fieldTypes["System.Float"] = "Number";
            fieldTypes["System.Guid"] = "Droptree";

            fieldTypes["Sitecore.Data.Fields.CheckboxField"] = "Checkbox";
            fieldTypes["Sitecore.Data.Fields.DateField"] = "Datetime";
            fieldTypes["Sitecore.Data.Fields.FileField"] = "File";
            fieldTypes["Sitecore.Data.Fields.ImageField"] = "Image";
            fieldTypes["Sitecore.Data.Fields.HtmlField"] = "Rich Text";
            fieldTypes["Sitecore.Data.Fields.TextField"] = "Single-Line Text";
            fieldTypes["Sitecore.Data.Fields.WordDocumentField"] = "Word Document";
            fieldTypes["Sitecore.Data.Fields.TextField"] = "Multi-Line Text";
            fieldTypes["Sitecore.Data.Fields.MultilistField"] = "Checklist";
            fieldTypes["Sitecore.Data.Fields.ValueLookupField"] = "Droplist";
            fieldTypes["Sitecore.Data.Fields.GroupedDroplinkField"] = "Grouped Droplink";
            fieldTypes["Sitecore.Data.Fields.GroupedDroplistField"] = "Grouped Droplist";
            fieldTypes["Sitecore.Data.Fields.MultilistField"] = "TreelistEx";
            fieldTypes["Sitecore.Data.Fields.LookupField"] = "Droplink";
            fieldTypes["Sitecore.Data.Fields.ReferenceField"] = "Droptree";
            fieldTypes["Sitecore.Data.Fields.LinkField"] = "General Link";
            fieldTypes["Sitecore.Data.Fields.VersionLinkField"] = "Version Link";
            fieldTypes["Sitecore.Analytics.Data.TrackingField"] = "Tracking";
            fieldTypes["Sitecore.Data.Fields.DatasourceField"] = "Datasource";
            fieldTypes["Sitecore.Data.Fields.CustomCustomField"] = "Custom";
            fieldTypes["Sitecore.Data.Fields.InternalLinkField"] = "Internal Link";
            fieldTypes["Sitecore.Data.Fields.LayoutField"] = "Layout";
            fieldTypes["Sitecore.Data.Fields.TemplateFieldSourceField"] = "Template Field Source";
            fieldTypes["Sitecore.Data.Fields.FileDropAreaField"] = "File Drop Area";
            fieldTypes["Sitecore.Data.Fields.ThumbnailField"] = "Thumbnail";

            fieldTypes["Glass.Sitecore.Mapper.FieldTypes.File"] = "File";
            fieldTypes["Glass.Sitecore.Mapper.FieldTypes.Image"] = "Image";
            fieldTypes["Glass.Sitecore.Mapper.FieldTypes.Link"] = "General Link";
            fieldTypes["Glass.Sitecore.Mapper.FieldTypes.TriState"] = "Tristate";
        }

        public bool CanHandle(string type)
        {
            Assert.ArgumentNotNull(type, nameof(type));

            string fieldType;

            return fieldTypes.TryGetValue(type, out fieldType);
        }

        public void Handle(string type, TemplateField field)
        {
            Assert.ArgumentNotNull(type, nameof(type));
            Assert.ArgumentNotNull(field, nameof(field));

            string fieldType;

            if (!fieldTypes.TryGetValue(type, out fieldType))
            {
                return;
            }

            field.Type = fieldType;
        }
    }
}
