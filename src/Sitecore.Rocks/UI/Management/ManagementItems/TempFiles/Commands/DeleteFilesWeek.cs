// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using Sitecore.Rocks.Commands;

namespace Sitecore.Rocks.UI.Management.ManagementItems.TempFiles.Commands
{
    [Command]
    public class DeleteFilesWeek : DeleteFiles
    {
        public DeleteFilesWeek()
        {
            Text = Resources.DeleteFilesWeek_DeleteFilesWeek_Delete_Files_Older_Than_One_Week___;
            Group = "Delete";
            SortingValue = 1100;

            Timestamp = DateTime.UtcNow.AddDays(-7);
        }
    }
}
