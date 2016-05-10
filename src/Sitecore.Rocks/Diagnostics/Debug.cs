// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.ComponentModel;
using Sitecore.Rocks.Annotations;

namespace Sitecore.Rocks.Diagnostics
{
    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    public static class Debug
    {
        [AssertionMethod]
        public static void ArgumentNotNull([CanBeNull, AssertionCondition(AssertionConditionType.IS_NOT_NULL)] object argument, [Localizable(false), CanBeNull, InvokerParameterName] string argumentName)
        {
            Diagnostics.Assert.ArgumentNotNull(argument, nameof(argument));
        }

        public static void ArgumentNotNullOrEmpty([CanBeNull] string argument, [CanBeNull] string argumentName)
        {
            Diagnostics.Assert.ArgumentNotNullOrEmpty(argument, argumentName);
        }

        [AssertionMethod]
        public static void Assert([AssertionCondition(AssertionConditionType.IS_TRUE)] bool condition, [NotNull] string errorMessage)
        {
            if (condition)
            {
                return;
            }

            Trace.TraceError("Debug assertion failed: {0}", errorMessage);
            Trace.WriteStackTrace(1, true);
            throw new Exception(@"Debug assertion failed: " + errorMessage);
        }
    }
}
