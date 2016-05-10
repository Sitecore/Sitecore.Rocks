// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.ComponentModel;
using Sitecore.Rocks.Annotations;

namespace Sitecore.Rocks.Diagnostics
{
    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    public static class Assert
    {
        [AssertionMethod]
        public static void ArgumentNotNull([CanBeNull, AssertionCondition(AssertionConditionType.IS_NOT_NULL)] object argument, [Localizable(false), CanBeNull, InvokerParameterName] string argumentName)
        {
            if (argument != null)
            {
                return;
            }

            if (argumentName != null)
            {
                Trace.TraceError("Argument is null: {0}", argumentName);
                Trace.WriteStackTrace(1, true);
                throw new ArgumentNullException(argumentName);
            }

            Trace.TraceError("Argument is null");
            Trace.WriteStackTrace(1, true);
            throw new ArgumentNullException();
        }

        [AssertionMethod]
        public static void ArgumentNotNullOrEmpty([CanBeNull, AssertionCondition(AssertionConditionType.IS_NOT_NULL)] string argument, [CanBeNull, InvokerParameterName, Localizable(false)] string argumentName)
        {
            if (!string.IsNullOrEmpty(argument))
            {
                return;
            }

            if (argument == null)
            {
                if (argumentName != null)
                {
                    Trace.TraceError("Argument is null: {0}", argumentName);
                    Trace.WriteStackTrace(1, true);
                    throw new ArgumentNullException(argumentName);
                }

                Trace.TraceError("Argument is null");
                Trace.WriteStackTrace(1, true);
                throw new ArgumentNullException();
            }

            if (argumentName != null)
            {
                Trace.TraceError("Empty strings are not allowed: {0}", argumentName);
                Trace.WriteStackTrace(1, true);
                throw new ArgumentException("Empty strings are not allowed.", argumentName);
            }

            Trace.TraceError("Empty strings are not allowed");
            Trace.WriteStackTrace(1, true);
            throw new ArgumentException("Empty strings are not allowed.");
        }

        [AssertionMethod]
        public static void IsFalse([AssertionCondition(AssertionConditionType.IS_FALSE)] bool condition, [NotNull] string message)
        {
            ArgumentNotNull(message, "message");

            if (!condition)
            {
                return;
            }

            Trace.TraceError("Condition is false: {0}", message);
            Trace.WriteStackTrace(1, true);
            throw new InvalidOperationException(message);
        }

        [AssertionMethod]
        public static void IsNotNull([CanBeNull, AssertionCondition(AssertionConditionType.IS_NOT_NULL)] object value, [Localizable(false), NotNull] string message)
        {
            if (value != null)
            {
                return;
            }

            Trace.TraceError("Value is null: {0}", message);
            Trace.WriteStackTrace(1, true);
            throw new InvalidOperationException(message);
        }

        [AssertionMethod]
        public static void IsNotNullOrEmpty([NotNull, AssertionCondition(AssertionConditionType.IS_NOT_NULL)] string value, [NotNull] string message)
        {
            if (!string.IsNullOrEmpty(value))
            {
                return;
            }

            Trace.TraceError("Value is null or empty: {0}", message);
            Trace.WriteStackTrace(1, true);
            throw new InvalidOperationException(message);
        }

        [AssertionMethod]
        public static void IsNull([CanBeNull, AssertionCondition(AssertionConditionType.IS_NULL)] object value, [NotNull] string message)
        {
            if (value == null)
            {
                return;
            }

            Trace.TraceError("Value is not null: {0}", message);
            Trace.WriteStackTrace(1, true);
            throw new InvalidOperationException(message);
        }

        [AssertionMethod]
        public static void IsTrue([AssertionCondition(AssertionConditionType.IS_TRUE)] bool condition, [NotNull] string message)
        {
            if (condition)
            {
                return;
            }

            Trace.TraceError("Condition is true: {0}", message);
            Trace.WriteStackTrace(1, true);
            throw new InvalidOperationException(message);
        }
    }
}
