// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.IO;
using System.Windows.Forms;
using Sitecore.Rocks.Commands;

namespace Sitecore.Rocks.UI.QueryAnalyzers.Commands
{
    [Command]
    public class OpenScript : CommandBase
    {
        public OpenScript()
        {
            Text = Resources.OpenScript_OpenScript_Open_Script___;
            Group = "File";
            SortingValue = 2000;
        }

        public override bool CanExecute(object parameter)
        {
            var context = parameter as QueryAnalyzerContext;
            if (context == null)
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

            var fileName = AppHost.Settings.Get("QueryAnalyzer", "RecentFile", string.Empty) as string ?? string.Empty;

            var dialog = new OpenFileDialog
            {
                Title = Resources.OpenScript_Execute_Open_Script,
                CheckFileExists = true,
                DefaultExt = @".txt",
                Filter = @"Text files|*.txt|All files|*.*",
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

            context.QueryAnalyzer.ScriptFileName = fileName;

            AppHost.Settings.Set("QueryAnalyzer", "RecentFile", fileName);

            context.QueryAnalyzer.SetScript(AppHost.Files.ReadAllText(fileName));
        }
    }
}
