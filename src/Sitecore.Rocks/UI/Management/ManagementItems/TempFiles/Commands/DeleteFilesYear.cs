// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using Sitecore.Rocks.Commands;

namespace Sitecore.Rocks.UI.Management.ManagementItems.TempFiles.Commands
{
    [Command]
    public class DeleteFilesYear : DeleteFiles
    {
        public DeleteFilesYear()
        {
            Text = Resources.DeleteFilesYear_DeleteFilesYear_Delete_Files_Older_Than_One_Year___;
            Group = "Delete";
            SortingValue = 1300;

            Timestamp = DateTime.UtcNow.AddYears(-1);
        }
    }
}
