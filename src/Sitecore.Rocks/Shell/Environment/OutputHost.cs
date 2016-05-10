// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Diagnostics;

namespace Sitecore.Rocks.Shell.Environment
{
    public class OutputHost
    {
        public virtual void Clear()
        {
            throw new InvalidOperationException("Output host must be overridden.");
        }

        public virtual void Log([CanBeNull] string text)
        {
            if (text == null)
            {
                return;
            }

            if (AppHost.Settings.Options.IsLogEnabled)
            {
                Write("Sitecore Rocks Log", text);
            }
        }

        public virtual void Log([CanBeNull] string format, [CanBeNull] params object[] args)
        {
            if (format == null)
            {
                return;
            }

            if (args == null)
            {
                Log(format);
                return;
            }

            if (AppHost.Settings.Options.IsLogEnabled)
            {
                Write("Sitecore Rocks Log", string.Format(format, args));
            }
        }

        public virtual void LogException([NotNull] Exception exception)
        {
            Assert.ArgumentNotNull(exception, nameof(exception));

            Log(exception.Message);
        }

        public virtual void Show()
        {
            throw new InvalidOperationException("Output host must be overridden.");
        }

        public virtual void Write([NotNull] string text)
        {
            Assert.ArgumentNotNull(text, nameof(text));

            Write("Sitecore Rocks", text);
        }

        protected virtual void Write([NotNull] string pane, [NotNull] string text)
        {
            Debug.ArgumentNotNull(pane, nameof(pane));
            Debug.ArgumentNotNull(text, nameof(text));
        }
    }
}
