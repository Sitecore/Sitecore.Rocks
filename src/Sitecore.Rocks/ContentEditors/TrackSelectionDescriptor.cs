// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.ComponentModel;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Diagnostics;

namespace Sitecore.Rocks.ContentEditors
{
    [DisplayName(@"Item"), DefaultProperty("Name"), Description("Sitecore Item")]
    public class TrackSelectionDescriptor
    {
        private readonly Item item;

        private string path;

        public TrackSelectionDescriptor([NotNull] Item item)
        {
            Assert.ArgumentNotNull(item, nameof(item));

            this.item = item;
        }

        [NotNull, Description("The name of the database."), DisplayName("Database Name"), Category("Item")]
        public string DatabaseName
        {
            get { return item.ItemUri.DatabaseName.ToString(); }
        }

        [NotNull, Description("The ID of the ID."), DisplayName("Item ID"), Category("Item")]
        public string ID
        {
            get { return item.Uri.ItemId.ToString(); }
        }

        [NotNull, Description("Language."), DisplayName("Language"), Category("Item")]
        public string Language
        {
            get { return item.Uri.Language.Name; }
        }

        [NotNull, Description("The name of the item."), DisplayName("Item Name"), Category("Item")]
        public string Name
        {
            get { return item.Name; }
        }

        [NotNull, Description("The path of the item."), DisplayName("Item Path"), Category("Item")]
        public string Path
        {
            get
            {
                if (string.IsNullOrEmpty(path))
                {
                    path = string.Empty;

                    foreach (var itemPath in item.Path)
                    {
                        path = @"/" + itemPath.Name + path;
                    }
                }

                return path;
            }
        }

        [NotNull, Description("The id of the template."), Category("Template"), DisplayName("Template ID")]
        public string TemplateId
        {
            get { return item.TemplateId.ToString(); }
        }

        [NotNull, Description("The name of the template."), Category("Template"), DisplayName("Template Name")]
        public string TemplateName
        {
            get { return item.TemplateName; }
        }

        [NotNull, Description("The version of the item."), DisplayName("Version"), Category("Item")]
        public string Version
        {
            get { return item.Uri.Version.ToString(); }
        }
    }
}
