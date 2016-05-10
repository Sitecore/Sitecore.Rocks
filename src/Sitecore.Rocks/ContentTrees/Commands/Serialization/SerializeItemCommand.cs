// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Linq;
using System.Windows;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Commands;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Data.DataServices;
using Sitecore.Rocks.Diagnostics;

namespace Sitecore.Rocks.ContentTrees.Commands.Serialization
{
    public abstract class SerializeItemCommand : CommandBase
    {
        [NotNull]
        protected string ConfirmationText { get; set; }

        protected SerializationOperation SerializationOperation { get; set; }

        [NotNull]
        protected string SerializationText { get; set; }

        public override bool CanExecute(object parameter)
        {
            var context = parameter as IItemSelectionContext;
            if (context == null)
            {
                return false;
            }

            if (context.Items.Any(i => !i.ItemUri.Site.DataService.CanExecuteAsync("Serialization.SerializeItem")))
            {
                return false;
            }

            return context.Items.Any();
        }

        public override void Execute(object parameter)
        {
            var context = parameter as IItemSelectionContext;
            if (context == null)
            {
                return;
            }

            if (AppHost.MessageBox(ConfirmationText, "Confirmation", MessageBoxButton.OKCancel, MessageBoxImage.Question) != MessageBoxResult.OK)
            {
                return;
            }

            var count = context.Items.Count();

            ExecuteCompleted callback = delegate(string response, ExecuteResult executeResult)
            {
                count--;
                if (count == 0)
                {
                    AppHost.Statusbar.SetText(string.Empty);

                    foreach (var item in context.Items)
                    {
                        Notifications.RaiseItemSerialized(this, item.ItemUri, SerializationOperation);
                    }
                }

                DataService.HandleExecute(response, executeResult);

                Executed(parameter);
            };

            foreach (var item in context.Items)
            {
                Execute(item.ItemUri, callback);
            }

            AppHost.Statusbar.SetText(SerializationText);
        }

        protected abstract void Execute([NotNull] ItemUri itemUri, [NotNull] ExecuteCompleted callback);

        protected virtual void Executed([NotNull] object parameter)
        {
            Debug.ArgumentNotNull(parameter, nameof(parameter));
        }
    }
}
