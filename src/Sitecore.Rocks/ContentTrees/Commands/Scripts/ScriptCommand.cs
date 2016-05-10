// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Commands;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Diagnostics;

namespace Sitecore.Rocks.ContentTrees.Commands.Scripts
{
    public abstract class ScriptCommand : CommandBase
    {
        public override bool CanExecute(object parameter)
        {
            var context = parameter as IItemSelectionContext;
            if (context == null)
            {
                return false;
            }

            if (!context.Items.Any())
            {
                return false;
            }

            if (!context.Items.All(i => i.ItemUri.Site.DataService.CanExecuteAsync("QueryAnalyzer.Run")))
            {
                return false;
            }

            return true;
        }

        public override void Execute(object parameter)
        {
            var context = parameter as IItemSelectionContext;
            if (context == null)
            {
                return;
            }

            var items = GetItems(context);
            var databaseUri = items.First().ItemUri.DatabaseUri;
            var script = GetScript(items);

            OpenQueryAnalyzer(databaseUri, script);
        }

        [NotNull]
        protected string FormatPath([NotNull] string path)
        {
            Debug.ArgumentNotNull(path, nameof(path));

            var result = new StringBuilder();

            foreach (var s in path.Split('/'))
            {
                if (string.IsNullOrEmpty(s))
                {
                    continue;
                }

                result.Append('/');

                if (Regex.IsMatch(s, @"^\w+$"))
                {
                    result.Append(s);
                }
                else
                {
                    result.Append('#');
                    result.Append(s);
                    result.Append('#');
                }
            }

            return result.ToString();
        }

        [NotNull]
        protected List<IItem> GetItems([NotNull] IItemSelectionContext context)
        {
            Debug.ArgumentNotNull(context, nameof(context));

            var items = new List<IItem>();

            foreach (var item in context.Items)
            {
                items.Add(item);
            }

            return items;
        }

        [NotNull]
        protected abstract string GetScript([NotNull] List<IItem> items);

        protected void OpenQueryAnalyzer([NotNull] DatabaseUri databaseUri, [NotNull] string script)
        {
            Debug.ArgumentNotNull(databaseUri, nameof(databaseUri));
            Debug.ArgumentNotNull(script, nameof(script));

            var queryAnalyzer = AppHost.Windows.Factory.OpenQueryAnalyzer(databaseUri);
            if (queryAnalyzer == null)
            {
                Trace.TraceError("Could not open query analyzer");
                return;
            }

            queryAnalyzer.AppendScript(script);
        }
    }
}
