// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.IO;
using System.Text;
using System.Windows.Forms;
using Sitecore.Rocks.Commands;
using Sitecore.Rocks.Diagnostics;

namespace Sitecore.Rocks.UI.QueryAnalyzers.Commands
{
    [Command]
    public class SaveScript : CommandBase
    {
        public SaveScript()
        {
            Text = Resources.SaveScript_SaveScript_Save_Script;
            Group = "File";
            SortingValue = 2100;
        }

        public override bool CanExecute(object parameter)
        {
            var context = parameter as QueryAnalyzerContext;
            if (context == null)
            {
                return false;
            }

            if (string.IsNullOrEmpty(context.Script))
            {
                return false;
            }

            return true;
        }

        public override void Execute(object parameter)
        {
            var context = parameter as QueryAnalyzerContext;
            if (context == null)
            {
                Trace.Expected(typeof(QueryAnalyzerContext));
                return;
            }

            var fileName = context.QueryAnalyzer.ScriptFileName;

            if (string.IsNullOrEmpty(fileName))
            {
                fileName = AppHost.Settings.Get("QueryAnalyzer", "RecentFile", "script.txt") as string ?? string.Empty;

                var dialog = new SaveFileDialog
                {
                    Title = Resources.SaveScript_SaveScript_Save_Script,
                    CheckPathExists = true,
                    OverwritePrompt = true,
                    Filter = @"Text files|*.txt|All files|*.*"
                };

                if (!string.IsNullOrEmpty(fileName))
                {
                    dialog.FileName = Path.GetFileName(fileName);
                    dialog.InitialDirectory = Path.GetDirectoryName(fileName);
                }

                if (dialog.ShowDialog() != DialogResult.OK)
                {
                    return;
                }

                fileName = dialog.FileName;

                AppHost.Settings.Set("QueryAnalyzer", "RecentFile", fileName);
            }

            context.QueryAnalyzer.ScriptFileName = fileName;

            AppHost.Files.WriteAllText(fileName, context.Script, Encoding.UTF8);
        }
    }
}
