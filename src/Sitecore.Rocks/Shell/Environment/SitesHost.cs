// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Collections;
using System.Collections.Generic;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Sites;
using Sitecore.Rocks.Sites.Connections;

namespace Sitecore.Rocks.Shell.Environment
{
    public class SitesHost : IEnumerable<Site>
    {
        public void Delete([NotNull] Site site)
        {
            Assert.ArgumentNotNull(site, nameof(site));

            SiteManager.Delete(site);
        }

        public void Disconnect([NotNull] Site site)
        {
            Assert.ArgumentNotNull(site, nameof(site));

            SiteManager.Delete(site);
            ConnectionManager.Remove(site.Connection);
            ConnectionManager.Save();

            Notifications.RaiseSiteDeleted(site, site);
        }

        public IEnumerator<Site> GetEnumerator()
        {
            return SiteManager.Sites.GetEnumerator();
        }

        [CanBeNull]
        public Site NewConnection()
        {
            return SiteManager.NewConnection();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return SiteManager.Sites.GetEnumerator();
        }
    }
}
