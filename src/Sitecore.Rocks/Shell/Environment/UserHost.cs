// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.IO;
using Sitecore.Rocks.Annotations;

namespace Sitecore.Rocks.Shell.Environment
{
    public class UserHost
    {
        public UserHost()
        {
            UserFolderName = "Sitecore.Rocks.VisualStudio";
        }

        [NotNull]
        public virtual string SharedFolder
        {
            get
            {
                var result = AppHost.Settings.Options.SharedFolder;
                if (!string.IsNullOrEmpty(result))
                {
                    return result;
                }

                return UserFolder;
            }
        }

        [NotNull]
        public virtual string UserFolder
        {
            get
            {
                if (AppHost.Mock.IsActive)
                {
                    return "c:\\Users\\Sitecore.Rocks.VisualStudio";
                }

                var folder = System.Environment.GetFolderPath(System.Environment.SpecialFolder.LocalApplicationData);
                folder = Path.Combine(folder, @"Sitecore");

                var folderName = UserFolderName;
                var cmd = AppHost.Shell.CommandLineArguments;
                if (cmd.IndexOf(@"/rootsuffix", StringComparison.InvariantCultureIgnoreCase) >= 0)
                {
                    folderName += @".Exp";
                }

                folder = Path.Combine(folder, folderName);

                if (!AppHost.Files.FolderExists(folder))
                {
                    AppHost.Files.CreateDirectory(folder);
                }

                return folder;
            }
        }

        [NotNull]
        public string UserFolderName { get; set; }
    }
}
