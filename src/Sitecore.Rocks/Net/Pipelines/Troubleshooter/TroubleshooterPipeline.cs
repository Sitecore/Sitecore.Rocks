// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.ServiceModel.Description;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Extensibility.Pipelines;

namespace Sitecore.Rocks.Net.Pipelines.Troubleshooter
{
    public class TroubleshooterPipeline : Pipeline<TroubleshooterPipeline>
    {
        public bool Cancelled { get; set; }

        [NotNull]
        public DataService DataService { get; set; }

        public ServiceEndpoint Endpoint { get; set; }

        [CanBeNull]
        public Exception Exception { get; set; }

        public bool Retry { get; set; }

        public bool Retryable { get; set; }

        public bool Silent { get; set; }

        public bool StartedDevelopmentServer { get; set; }

        public bool UpdatedServerComponents { get; set; }

        [NotNull]
        public TroubleshooterPipeline WithParameters([NotNull] DataService dataService)
        {
            Assert.ArgumentNotNull(dataService, nameof(dataService));

            DataService = dataService;
            Start();

            return this;
        }

        [NotNull]
        public TroubleshooterPipeline WithParameters([NotNull] DataService dataService, bool retryable, [NotNull] Exception exception, [CanBeNull] ServiceEndpoint endpoint)
        {
            Assert.ArgumentNotNull(dataService, nameof(dataService));
            Assert.ArgumentNotNull(exception, nameof(exception));

            return WithParameters(dataService, retryable, exception, endpoint, false);
        }

        [NotNull]
        public TroubleshooterPipeline WithParameters([NotNull] DataService dataService, bool retryable, [NotNull] Exception exception, [CanBeNull] ServiceEndpoint endpoint, bool silent)
        {
            Assert.ArgumentNotNull(dataService, nameof(dataService));
            Assert.ArgumentNotNull(exception, nameof(exception));

            DataService = dataService;
            Exception = exception;
            Endpoint = endpoint;
            Retryable = retryable;
            Silent = silent;

            Start();

            return this;
        }
    }
}
