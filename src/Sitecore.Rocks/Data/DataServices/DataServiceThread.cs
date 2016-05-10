// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Threading;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Sites;

namespace Sitecore.Rocks.Data.DataServices
{
    public class DataServiceThread
    {
        private readonly Thread thread;

        public DataServiceThread([NotNull] ThreadStart threadStart)
        {
            Assert.ArgumentNotNull(threadStart, nameof(threadStart));

            thread = new Thread(threadStart);
        }

        public bool Start([NotNull] Site site)
        {
            Assert.ArgumentNotNull(site, nameof(site));

            if (!site.DataService.CheckDataService())
            {
                return false;
            }

            thread.Start();

            return true;
        }
    }
}
