// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.ComponentModel;
using System.Windows.Input;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Extensions.StringExtensions;

namespace Sitecore.Rocks.Shell.Environment
{
    public class UsageHost
    {
        public void Report([Localizable(false), NotNull] string text)
        {
            Assert.ArgumentNotNull(text, nameof(text));
        }

        public void ReportCommand([NotNull] ICommand command, [CanBeNull] object parameter)
        {
            Assert.ArgumentNotNull(command, nameof(command));

            var context = parameter != null ? parameter.GetType().FullName : "null";

            var text = $"Command: {command.GetType().Name} - {command.GetType().FullName}({context})";

            AppHost.Output.Log(text);
        }

        public void ReportRequest([NotNull] string text)
        {
            Assert.ArgumentNotNull(text, nameof(text));

            var name = text;
            var n = name.LastIndexOf(".", StringComparison.Ordinal);
            string s;

            if (n >= 0)
            {
                name = name.Mid(n + 1);
                s = $"Request: {name} - {text}";
            }
            else
            {
                s = $"Request: {text}";
            }

            AppHost.Output.Log(s);
        }

        public void ReportServerError([NotNull] string message)
        {
            Assert.ArgumentNotNull(message, nameof(message));

            var text = $"Server Error: {message}";

            AppHost.Output.Log(text);
        }
    }
}
