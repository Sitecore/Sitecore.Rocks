// � 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Linq;
using System.Windows;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Controls;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Extensibility.Pipelines;

namespace Sitecore.Rocks.ContentTrees.Pipelines.DragMove
{
    [Pipeline(typeof(DragMovePipeline), 1000)]
    public class Confirmation : PipelineProcessor<DragMovePipeline>
    {
        protected override void Process([NotNull] DragMovePipeline pipeline)
        {
            Assert.ArgumentNotNull(pipeline, nameof(pipeline));

            if ((pipeline.KeyStates & DragDropKeyStates.ShiftKey) == DragDropKeyStates.ShiftKey)
            {
                return;
            }

            if (!pipeline.Confirm)
            {
                return;
            }

            var count = pipeline.Items.Count();

            string text;
            if (count == 1)
            {
                switch (pipeline.Position)
                {
                    case ControlDragAdornerPosition.Top:
                        text = string.Format(Resources.Confirmation_Process_Are_you_sure_you_want_to_move___0___before___1___, pipeline.Items.First().Name, pipeline.Target.Text);
                        break;

                    case ControlDragAdornerPosition.Bottom:
                        text = string.Format(Resources.Confirmation_Process_Are_you_sure_you_want_to_move___0___after___1___, pipeline.Items.First().Name, pipeline.Target.Text);
                        break;

                    default:
                        text = string.Format(Resources.Confirmation_Process_Are_you_sure_you_want_to_move___0___to___1___, pipeline.Items.First().Name, pipeline.Target.Text);
                        break;
                }
            }
            else
            {
                switch (pipeline.Position)
                {
                    case ControlDragAdornerPosition.Top:
                        text = string.Format(Resources.Confirmation_Process_Are_you_sure_you_want_to_move_these___0___items_before___1___, count, pipeline.Target.Text);
                        break;

                    case ControlDragAdornerPosition.Bottom:
                        text = string.Format(Resources.Confirmation_Process_Are_you_sure_you_want_to_move_these___0___items_after___1___, count, pipeline.Target.Text);
                        break;

                    default:
                        text = string.Format(Resources.Confirmation_Process_Are_you_sure_you_want_to_move_these___0___items_to___1___, count, pipeline.Target.Text);
                        break;
                }
            }

			var confirmation = AppHost.MessageBox(text, Resources.Confirmation, MessageBoxButton.OKCancel, MessageBoxImage.Exclamation);
			if (confirmation != MessageBoxResult.OK)
            {
                pipeline.Abort();
            }
        }
    }
}
