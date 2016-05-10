// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Diagnostics;

namespace Sitecore.Rocks.Data
{
    public class DataServiceException : Exception
    {
        public DataServiceException([NotNull] string message) : base(message)
        {
            Assert.ArgumentNotNull(message, nameof(message));
        }
    }
}
