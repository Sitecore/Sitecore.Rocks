// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using Sitecore.Rocks.Annotations;

namespace Sitecore.Rocks.Diagnostics
{
    public static class Exceptions
    {
        [NotNull]
        public static InvalidOperationException InvalidOperation()
        {
            Trace.TraceError("Invalid Operation");

            return new InvalidOperationException();
        }

        [NotNull]
        public static InvalidOperationException InvalidOperation([NotNull] string message)
        {
            Assert.ArgumentNotNull(message, nameof(message));

            Trace.TraceError("Invalid Operation: {0}", message);

            return new InvalidOperationException(message);
        }

        [NotNull]
        public static UserMessageException UserMessage([NotNull] string message)
        {
            Assert.ArgumentNotNull(message, nameof(message));

            return new UserMessageException(message);
        }
    }
}
