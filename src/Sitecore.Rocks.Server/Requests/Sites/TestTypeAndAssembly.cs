// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Diagnostics;
using Sitecore.Reflection;

namespace Sitecore.Rocks.Server.Requests.Sites
{
    public class TestTypeAndAssembly
    {
        [NotNull]
        public string Execute([NotNull] string typeName)
        {
            Assert.ArgumentNotNull(typeName, nameof(typeName));

            try
            {
                if (ReflectionUtil.GetTypeInfo(typeName) != null)
                {
                    return "ok";
                }
            }
            catch
            {
                return "failed";
            }

            return "failed";
        }
    }
}
