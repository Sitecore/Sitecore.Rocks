// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.Windows;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Extensibility.Pipelines;
using Sitecore.Rocks.Extensions.StringExtensions;

namespace Sitecore.Rocks.Net.Pipelines.Troubleshooter
{
    [Pipeline(typeof(TroubleshooterPipeline), 9999)]
    public class ShowMessage : PipelineProcessor<TroubleshooterPipeline>
    {
        protected override void Process(TroubleshooterPipeline pipeline)
        {
            Debug.ArgumentNotNull(pipeline, nameof(pipeline));

            if (pipeline.Silent)
            {
                return;
            }

            var text = Resources.ShowMessage_Process_;

            var exception = pipeline.Exception;
            if (exception == null)
            {
                AppHost.Usage.ReportServerError(text);
            }
            else
            {
                text = ParseMessage(exception.Message);
                AppHost.Usage.ReportServerError(text);

                var inner = exception.InnerException;
                while (inner != null)
                {
                    text += Environment.NewLine + ParseMessage(inner.Message);

                    inner = inner.InnerException;
                }

                text += Environment.NewLine + @"_______________________________________________________________________" + Environment.NewLine + Environment.NewLine;

                text += Resources.ShowMessage_Process_Do_you_want_to_retry_;
            }

            if (AppHost.MessageBox(text, Resources.Error, MessageBoxButton.OKCancel, MessageBoxImage.Error) == MessageBoxResult.OK)
            {
                pipeline.Retry = true;
            }
            else
            {
                pipeline.Cancelled = true;
            }
        }

        [NotNull]
        private string ParseMessage([NotNull] string message)
        {
            Debug.ArgumentNotNull(message, nameof(message));

            var start = message.IndexOf(@"<title>", StringComparison.InvariantCultureIgnoreCase);
            if (start < 0)
            {
                return message;
            }

            var end = message.IndexOf(@"</title>", StringComparison.InvariantCultureIgnoreCase);
            if (end < 0)
            {
                return message;
            }

            return message.Mid(start + 7, end - start - 7);
        }
    }
}
