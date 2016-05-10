// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using Sitecore.Rocks.Commands;

namespace Sitecore.Rocks.UI.Management.ManagementItems.TempFiles.Commands
{
    [Command]
    public class DeleteFilesMonth : DeleteFiles
    {
        public DeleteFilesMonth()
        {
            Text = Resources.DeleteFilesMonth_DeleteFilesMonth_Delete_Files_Older_Than_One_Month___;
            Group = "Delete";
            SortingValue = 1200;

            Timestamp = DateTime.UtcNow.AddMonths(-1);
        }
    }
}
