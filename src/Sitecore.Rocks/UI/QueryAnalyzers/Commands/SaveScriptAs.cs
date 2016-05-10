// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.IO;
using System.Text;
using System.Windows.Forms;
using Sitecore.Rocks.Commands;

namespace Sitecore.Rocks.UI.QueryAnalyzers.Commands
{
    [Command]
    public class SaveScriptAs : CommandBase
    {
        public SaveScriptAs()
        {
            Text = Resources.SaveScriptAs_SaveScriptAs_Save_Script_As;
            Group = "File";
            SortingValue = 2200;
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

            if (string.IsNullOrEmpty(context.QueryAnalyzer.ScriptFileName))
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
                return;
            }

            var fileName = context.QueryAnalyzer.ScriptFileName;

            if (string.IsNullOrEmpty(fileName))
            {
                fileName = AppHost.Settings.Get("QueryAnalyzer", "RecentFile", "script.txt") as string ?? string.Empty;
            }

            var dialog = new SaveFileDialog
            {
                Title = "Save Script As",
                CheckPathExists = true,
                OverwritePrompt = true,
                FileName = Path.GetFileName(fileName),
                InitialDirectory = Path.GetDirectoryName(fileName),
                Filter = @"Text files|*.txt|All files|*.*"
            };

            if (dialog.ShowDialog() != DialogResult.OK)
            {
                return;
            }

            fileName = dialog.FileName;

            AppHost.Settings.Set("QueryAnalyzer", "RecentFile", fileName);

            context.QueryAnalyzer.ScriptFileName = fileName;

            AppHost.Files.WriteAllText(fileName, context.Script, Encoding.UTF8);
        }
    }
}
