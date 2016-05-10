// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Diagnostics;

namespace Sitecore.Rocks.Data.DataServices
{
    public class ExecuteResult
    {
        public ExecuteResult([NotNull] DataService dataService, [CanBeNull] Exception error, bool cancelled)
        {
            Assert.ArgumentNotNull(dataService, nameof(dataService));

            DataService = dataService;
            Error = error;
            Cancelled = cancelled;
        }

        public bool Cancelled { get; private set; }

        public DataService DataService { get; private set; }

        public Exception Error { get; private set; }
    }
}
