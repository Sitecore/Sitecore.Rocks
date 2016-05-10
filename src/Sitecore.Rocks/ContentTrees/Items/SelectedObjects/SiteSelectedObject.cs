// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.ComponentModel;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Sites;

namespace Sitecore.Rocks.ContentTrees.Items.SelectedObjects
{
    public class SiteSelectedObject : BaseSelectedObject
    {
        public SiteSelectedObject([NotNull] Site site)
        {
            Assert.ArgumentNotNull(site, nameof(site));

            Site = site;
        }

        [NotNull, Description("The data service in use."), DisplayName("Data Service"), Category("Site")]
        public string DataServiceName
        {
            get { return Site.DataServiceName; }
        }

        [NotNull, Description("The version of Sitecore running the web site."), DisplayName("Sitecore Version"), Category("Site")]
        public string SitecoreVersion
        {
            get { return Site.DataService.SitecoreVersionString; }
        }

        [NotNull, Description("The name of the site."), DisplayName("Site"), Category("Site")]
        public string SiteName
        {
            get { return Site.HostName; }
        }

        [NotNull, Description("The user name."), DisplayName("User Name"), Category("Site")]
        public string UserName
        {
            get { return Site.UserName; }
        }

        [NotNull, Description("The root of the web site."), DisplayName("Web Root Path"), Category("Site")]
        public string WebRootPath
        {
            get { return Site.WebRootPath; }
        }

        [NotNull]
        protected internal Site Site { get; }
    }
}
