// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.ComponentModel;
using System.Diagnostics;
using Sitecore.Rocks.Annotations;

namespace Sitecore.Rocks.Diagnostics
{
    [Localizable(false), UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
    public static class Trace
    {
        [Conditional("TRACE")]
        public static void Assert(bool condition)
        {
            System.Diagnostics.Trace.Assert(condition);
        }

        [Conditional("TRACE")]
        public static void Assert(bool condition, [CanBeNull] string message)
        {
            System.Diagnostics.Trace.Assert(condition, message);
        }

        [Conditional("TRACE")]
        public static void Assert(bool condition, [CanBeNull] string message, [CanBeNull] string detailMessage)
        {
            System.Diagnostics.Trace.Assert(condition, message, detailMessage);
        }

        [Conditional("TRACE")]
        public static void Catch([CanBeNull] Exception exception)
        {
            if (exception == null)
            {
                return;
            }

            TraceError(exception.Message);
            WriteStackTrace(1, true);
        }

        [Conditional("TRACE")]
        public static void EnumerableContainsNull()
        {
            TraceWarning("Enumerable contains a null item");
            WriteStackTrace(1, true);
        }

        [Conditional("TRACE")]
        public static void Expected([CanBeNull] Type type)
        {
            if (type == null)
            {
                return;
            }

            TraceWarning("Expected type {0}", type.FullName);
            WriteStackTrace(1, true);
        }

        [Conditional("TRACE")]
        public static void Fail([CanBeNull] string message)
        {
            System.Diagnostics.Trace.Fail(message);
        }

        [Conditional("TRACE")]
        public static void Fail([CanBeNull] string message, [CanBeNull] string detailMessage)
        {
            System.Diagnostics.Trace.Fail(message, detailMessage);
        }

        [Conditional("TRACE")]
        public static void Indent()
        {
            System.Diagnostics.Trace.Indent();
        }

        [Conditional("TRACE")]
        public static void TraceError([CanBeNull] string message)
        {
            System.Diagnostics.Trace.TraceError(message);
            AppHost.Output.Log(message);
        }

        [Conditional("TRACE")]
        public static void TraceError([CanBeNull] string format, [CanBeNull] params object[] args)
        {
            System.Diagnostics.Trace.TraceError(format, args);
            AppHost.Output.Log(format, args);
        }

        [Conditional("TRACE")]
        public static void TraceInformation([CanBeNull] string message)
        {
            System.Diagnostics.Trace.TraceInformation(message);
            AppHost.Output.Log(message);
        }

        [Conditional("TRACE")]
        public static void TraceInformation([CanBeNull] string format, [CanBeNull] params object[] args)
        {
            System.Diagnostics.Trace.TraceInformation(format, args);
            AppHost.Output.Log(format, args);
        }

        [Conditional("TRACE")]
        public static void TraceWarning([CanBeNull] string message)
        {
            System.Diagnostics.Trace.TraceWarning(message);
            AppHost.Output.Log(message);
        }

        [Conditional("TRACE")]
        public static void TraceWarning([CanBeNull] string format, [CanBeNull] params object[] args)
        {
            System.Diagnostics.Trace.TraceWarning(format, args);
            AppHost.Output.Log(format, args);
        }

        [Conditional("TRACE")]
        public static void Unindent()
        {
            System.Diagnostics.Trace.Unindent();
        }

        [Conditional("TRACE")]
        public static void Write([CanBeNull] string message)
        {
            System.Diagnostics.Trace.Write(message);
            AppHost.Output.Log(message);
        }

        [Conditional("TRACE")]
        public static void Write([CanBeNull] object value)
        {
            System.Diagnostics.Trace.Write(value);
            AppHost.Output.Log(value.ToString());
        }

        [Conditional("TRACE"), StringFormatMethod("message")]
        public static void Write([CanBeNull] string format, [CanBeNull] params string[] args)
        {
            System.Diagnostics.Trace.Write(string.Format(format, args));
            AppHost.Output.Log(format, args);
        }

        [Conditional("TRACE")]
        public static void WriteIf(bool condition, [CanBeNull] string message)
        {
            System.Diagnostics.Trace.WriteIf(condition, message);
            if (condition)
            {
                AppHost.Output.Log(message);
            }
        }

        [Conditional("TRACE")]
        public static void WriteIf(bool condition, [CanBeNull] object value)
        {
            System.Diagnostics.Trace.WriteIf(condition, value);
        }

        [Conditional("TRACE"), StringFormatMethod("message")]
        public static void WriteIf(bool condition, [CanBeNull] string message, [CanBeNull] params string[] parameters)
        {
            System.Diagnostics.Trace.WriteIf(condition, string.Format(message, parameters));
        }

        [Conditional("TRACE")]
        public static void WriteLine([CanBeNull] string message)
        {
            System.Diagnostics.Trace.WriteLine(message);
            AppHost.Output.Log(message);
        }

        [Conditional("TRACE")]
        public static void WriteLine([CanBeNull] object value)
        {
            System.Diagnostics.Trace.WriteLine(value);
            AppHost.Output.Log(value.ToString());
        }

        [Conditional("TRACE"), StringFormatMethod("message")]
        public static void WriteLine([CanBeNull] string message, [CanBeNull] params string[] parameters)
        {
            System.Diagnostics.Trace.WriteLine(string.Format(message, parameters));
            AppHost.Output.Log(string.Format(message, parameters));
        }

        [Conditional("TRACE")]
        public static void WriteLineIf(bool condition, [CanBeNull] string message)
        {
            System.Diagnostics.Trace.WriteLineIf(condition, message);
        }

        [Conditional("TRACE")]
        public static void WriteLineIf(bool condition, [CanBeNull] object value)
        {
            System.Diagnostics.Trace.WriteLineIf(condition, value);
        }

        [Conditional("TRACE"), StringFormatMethod("message")]
        public static void WriteLineIf(bool condition, [CanBeNull] string message, [CanBeNull] params string[] parameters)
        {
            System.Diagnostics.Trace.WriteLineIf(condition, string.Format(message, parameters));
        }

        [Conditional("TRACE")]
        public static void WriteStackTrace()
        {
            WriteStackTrace(1, true);
        }

        [Conditional("TRACE")]
        public static void WriteStackTrace(int skipFrames, bool indent)
        {
            if (indent)
            {
                Indent();
            }

            var stackTrace = new StackTrace(skipFrames + 1, true);
            var text = stackTrace.ToString();
            WriteLine(text);
            AppHost.Output.Log(text);

            if (indent)
            {
                Unindent();
            }
        }
    }
}
