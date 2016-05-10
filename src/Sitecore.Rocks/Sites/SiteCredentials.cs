// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Diagnostics;

namespace Sitecore.Rocks.Sites
{
    public class SiteCredentials
    {
        public SiteCredentials()
        {
            UserName = string.Empty;
            Password = string.Empty;
        }

        public SiteCredentials([NotNull] string userName, [NotNull] string password)
        {
            Assert.ArgumentNotNull(userName, nameof(userName));
            Assert.ArgumentNotNull(password, nameof(password));

            UserName = userName;
            Password = password;
        }

        [NotNull]
        public string Password { get; set; }

        [NotNull]
        public string UserName { get; set; }
    }
}
