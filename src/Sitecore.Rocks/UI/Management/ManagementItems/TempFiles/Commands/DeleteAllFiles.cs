// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using Sitecore.Rocks.Commands;

namespace Sitecore.Rocks.UI.Management.ManagementItems.TempFiles.Commands
{
    [Command]
    public class DeleteAllFiles : DeleteFiles
    {
        public DeleteAllFiles()
        {
            Text = Resources.DeleteAllFiles_DeleteAllFiles_Delete_All_Files___;
            Group = "Delete";
            SortingValue = 1000;
            Timestamp = DateTime.MaxValue;
        }
    }
}
