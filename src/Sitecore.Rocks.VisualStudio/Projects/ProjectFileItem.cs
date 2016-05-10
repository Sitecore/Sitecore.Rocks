// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Xml.Linq;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Extensions.DataServiceExtensions;
using Sitecore.Rocks.Extensions.XElementExtensions;
using Sitecore.Rocks.IO;
using Sitecore.Rocks.Sites;

namespace Sitecore.Rocks.Projects
{
    public class ProjectFileItem : ProjectItem
    {
        public ProjectFileItem([NotNull] ProjectBase project) : base(project)
        {
            Assert.ArgumentNotNull(project, nameof(project));

            Items = new List<ItemUri>();
        }

        [NotNull]
        public string AbsoluteFileName
        {
            get { return System.IO.Path.Combine(Project.FolderName, File); }
        }

        [NotNull]
        public string File { get; set; }

        public long FileSize { get; set; }

        public long FileTimestamp { get; set; }

        public override bool IsAdded
        {
            get { return ServerFileSize == 0 && ServerTimestamp == 0; }
        }

        public override bool IsModified
        {
            get
            {
                if (!IsValid)
                {
                    return false;
                }

                var fileInfo = new FileInfo(AbsoluteFileName);
                return fileInfo.LastWriteTimeUtc.Ticks != FileTimestamp || fileInfo.Length != FileSize;
            }
        }

        public override bool IsValid
        {
            get { return System.IO.File.Exists(AbsoluteFileName); }
        }

        [NotNull]
        public List<ItemUri> Items { get; }

        public override string Path
        {
            get { return File; }
        }

        public long ServerFileSize { get; set; }

        public long ServerTimestamp { get; set; }

        public override void Commit(ProcessedEventHandler callback)
        {
            Assert.ArgumentNotNull(callback, nameof(callback));

            if (IsAdded && !IsValid)
            {
                Project.Remove(this);
                callback(this, new ProcessedEventArgs(Resources.ProjectFileItem_Commit_deleted));
                return;
            }

            var fileInfo = new FileInfo(AbsoluteFileName);
            var fileContent = string.Empty;
            var action = string.Empty;

            if (IsValid)
            {
                fileContent = Convert.ToBase64String(System.IO.File.ReadAllBytes(AbsoluteFileName));
            }
            else
            {
                action += Resources.ProjectFileItem_Commit__delete;
            }

            switch (ConflictResolution)
            {
                case ConflictResolution.NotResolved:
                    callback(this, new ProcessedEventArgs(Resources.ProjectFileItem_Commit_skipped, Resources.ProjectFileItem_Commit_Conflict_not_resolved_));
                    return;
                case ConflictResolution.UseLocalVersion:
                    action += Resources.ProjectFileItem_Commit__overwrite;
                    break;
                case ConflictResolution.UseServerVersion:
                    callback(this, new ProcessedEventArgs(Resources.ProjectFileItem_Commit_skipped, Resources.ProjectFileItem_Commit_Use_Update_command_to_overwrite_local_version_with_server_version));
                    return;
            }

            CommitCompleted process = delegate(string result, long serverTimestamp, long serverFileSize)
            {
                var processed = new ProcessedEventArgs(result);

                switch (result)
                {
                    case "ok":
                        processed.Text = IsAdded ? Resources.ProjectFileItem_Commit_added : Resources.ProjectFileItem_Commit_committed;
                        FileSize = fileInfo.Length;
                        FileTimestamp = fileInfo.LastWriteTimeUtc.Ticks;
                        ServerTimestamp = serverTimestamp;
                        ServerFileSize = serverFileSize;
                        ConflictResolution = ConflictResolution.None;
                        break;
                    case "deleted":
                        Project.Remove(this);
                        processed.Text = Resources.ProjectFileItem_Commit_deleted;
                        break;
                    case "conflict":
                        ServerTimestamp = serverTimestamp;
                        ServerFileSize = serverFileSize;
                        ConflictResolution = ConflictResolution.NotResolved;
                        processed.Text = Resources.ProjectFileItem_Commit_conflict;
                        break;
                }

                callback(this, processed);
            };

            var site = Project.Site;
            if (site != null)
            {
                site.DataService.Commit(File, fileContent, ServerTimestamp, ServerFileSize, action, false, process);
            }
        }

        [NotNull]
        public static ProjectFileItem Load([NotNull] ProjectBase project, [NotNull] string fileName)
        {
            Assert.ArgumentNotNull(project, nameof(project));
            Assert.ArgumentNotNull(fileName, nameof(fileName));

            var result = new ProjectFileItem(project)
            {
                File = project.GetRelativeFileName(fileName),
                FileSize = 0,
                FileTimestamp = 0,
                ServerTimestamp = 0,
                ServerFileSize = 0,
                ConflictResolution = ConflictResolution.None,
                HideFromToolbox = false
            };

            return result;
        }

        public override void Load(XElement element)
        {
            Assert.ArgumentNotNull(element, nameof(element));

            ConflictResolution conflictResolution;
            if (!Enum.TryParse(element.GetAttributeValue("ConflictResolution"), out conflictResolution))
            {
                conflictResolution = ConflictResolution.None;
            }

            File = element.GetAttributeValue("Include");
            ConflictResolution = conflictResolution;
            FileTimestamp = element.GetAttributeLong("FileTimestamp", 0);
            FileSize = element.GetAttributeLong("FileSize", 0);
            ServerTimestamp = element.GetAttributeLong("ServerTimestamp", 0);
            ServerFileSize = element.GetAttributeLong("ServerFileSize", 0);
            HideFromToolbox = string.Compare(element.GetAttributeValue("HideFromToolbox"), @"True", StringComparison.OrdinalIgnoreCase) == 0;

            var site = Project.Site ?? Site.Empty;

            foreach (var itemElement in element.Elements(@"Item"))
            {
                var itemId = new ItemId(new Guid(itemElement.Value));
                var databaseName = new DatabaseName(itemElement.GetAttributeValue("DatabaseName"));

                Items.Add(new ItemUri(new DatabaseUri(site, databaseName), itemId));
            }
        }

        public override void Revert(EventHandler<ProcessedEventArgs> callback)
        {
            Assert.ArgumentNotNull(callback, nameof(callback));

            UpdateCompleted process = delegate(string result, ref byte[] file, long serverTimestamp, long serverFileSize)
            {
                var processed = new ProcessedEventArgs(result);

                switch (result)
                {
                    case "ok":
                        processed.Text = Resources.ProjectFileItem_Revert_reverted;
                        WriteFile(ref file, serverTimestamp, serverFileSize, processed);
                        ConflictResolution = ConflictResolution.None;
                        break;
                    case "deleted":
                        processed.Text = Resources.ProjectFileItem_Revert_deleted;
                        DeleteFile(processed, serverTimestamp, serverFileSize);
                        Project.Remove(this);
                        break;
                    case "conflict":
                        ServerTimestamp = serverTimestamp;
                        ServerFileSize = serverFileSize;
                        ConflictResolution = ConflictResolution.NotResolved;
                        processed.Text = Resources.ProjectFileItem_Revert_conflict;
                        break;
                    case "unchanged":
                        processed.Ignore = true;
                        break;
                }

                callback(this, processed);
            };

            var site = Project.Site;
            if (site != null)
            {
                site.DataService.Update(File, ServerTimestamp.ToString(CultureInfo.InvariantCulture), ServerFileSize, @"revert", process);
            }
        }

        public override void Save(OutputWriter output)
        {
            Assert.ArgumentNotNull(output, nameof(output));

            output.WriteStartElement("File");

            output.WriteAttributeString("Include", File);
            output.WriteAttributeString(@"FileTimestamp", FileTimestamp.ToString(CultureInfo.InvariantCulture));
            output.WriteAttributeString(@"FileSize", FileSize.ToString(CultureInfo.InvariantCulture));
            output.WriteAttributeString(@"ServerTimestamp", ServerTimestamp.ToString(CultureInfo.InvariantCulture));
            output.WriteAttributeString(@"ServerFileSize", ServerFileSize.ToString(CultureInfo.InvariantCulture));

            if (IsConflict)
            {
                output.WriteAttributeString(@"IsConflict", @"True");
                output.WriteAttributeString(@"ConflictResolution", ConflictResolution.ToString());
            }

            if (HideFromToolbox)
            {
                output.WriteAttributeString(@"HideFromToolbox", @"True");
            }

            foreach (var itemUri in Items)
            {
                output.WriteStartElement("Item");
                output.WriteAttributeString(@"DatabaseName", itemUri.DatabaseName.ToString());
                output.WriteValue(itemUri.ItemId.ToString());
                output.WriteEndElement();
            }

            output.WriteEndElement();
        }

        public override void Update(ProcessedEventHandler callback)
        {
            Assert.ArgumentNotNull(callback, nameof(callback));

            var action = string.Empty;

            switch (ConflictResolution)
            {
                case ConflictResolution.NotResolved:
                    callback(this, new ProcessedEventArgs(Resources.ProjectFileItem_Update_skipped, Resources.ProjectFileItem_Update_Conflict_not_resolved));
                    return;
                case ConflictResolution.UseLocalVersion:
                    callback(this, new ProcessedEventArgs(Resources.ProjectFileItem_Update_skipped, Resources.ProjectFileItem_Update_Use_Commit_command_to_overwrite_server_version_with_local_version));
                    return;
                case ConflictResolution.UseServerVersion:
                    action += Resources.ProjectFileItem_Update__revert;
                    break;
            }

            UpdateCompleted process = delegate(string result, ref byte[] file, long serverTimestamp, long serverFileSize)
            {
                var processed = new ProcessedEventArgs(result);

                switch (result)
                {
                    case "ok":
                        if ((IsModified || IsAdded) && action.IndexOf(Resources.ProjectFileItem_Update_revert, StringComparison.InvariantCultureIgnoreCase) < 0)
                        {
                            ServerTimestamp = serverTimestamp;
                            ServerFileSize = serverFileSize;
                            ConflictResolution = ConflictResolution.NotResolved;
                            processed.Text = Resources.ProjectFileItem_Update_conflict;
                            processed.Comment = Resources.ProjectFileItem_Update_Local_file_has_been_modified;
                        }
                        else
                        {
                            processed.Text = Resources.ProjectFileItem_Update_updated;
                            WriteFile(ref file, serverTimestamp, serverFileSize, processed);
                            ConflictResolution = ConflictResolution.None;
                        }

                        break;
                    case "deleted":
                        processed.Text = Resources.ProjectFileItem_Update_deleted;
                        DeleteFile(processed, serverTimestamp, serverFileSize);
                        break;
                    case "conflict":
                        ServerTimestamp = serverTimestamp;
                        ServerFileSize = serverFileSize;
                        ConflictResolution = ConflictResolution.NotResolved;
                        processed.Text = Resources.ProjectFileItem_Update_conflict;
                        break;
                    case "unchanged":
                        processed.Ignore = true;
                        break;
                }

                callback(this, processed);
            };

            Project.Site.DataService.Update(File, ServerTimestamp.ToString(CultureInfo.InvariantCulture), ServerFileSize, action, process);
        }

        private void DeleteFile([NotNull] ProcessedEventArgs processed, long serverTimestamp, long serverFileSize)
        {
            Debug.ArgumentNotNull(processed, nameof(processed));

            if (IsModified || IsAdded)
            {
                ServerTimestamp = serverTimestamp;
                ServerFileSize = serverFileSize;
                ConflictResolution = ConflictResolution.NotResolved;
                processed.Text = Resources.ProjectFileItem_DeleteFile_conflict;
                processed.Comment = Resources.ProjectFileItem_DeleteFile_Local_file_has_been_modified;
                return;
            }

            if (IsValid)
            {
                try
                {
                    System.IO.File.Delete(AbsoluteFileName);
                }
                catch (Exception ex)
                {
                    processed.Text = Resources.ProjectFileItem_DeleteFile_failed;
                    processed.Comment = ex.Message;
                }
            }

            Project.Remove(this);
        }

        private void WriteFile([NotNull] ref byte[] file, long serverTimestamp, long serverFileSize, [NotNull] ProcessedEventArgs processed)
        {
            Debug.ArgumentNotNull(file, nameof(file));
            Debug.ArgumentNotNull(processed, nameof(processed));

            try
            {
                var directoryName = System.IO.Path.GetDirectoryName(AbsoluteFileName) ?? string.Empty;
                Directory.CreateDirectory(directoryName);

                System.IO.File.WriteAllBytes(AbsoluteFileName, file);

                var info = new FileInfo(AbsoluteFileName);

                FileSize = info.Length;
                FileTimestamp = info.LastWriteTimeUtc.Ticks;
                ServerTimestamp = serverTimestamp;
                ServerFileSize = serverFileSize;
            }
            catch (Exception ex)
            {
                processed.Text = Resources.ProjectFileItem_WriteFile_failed;
                processed.Comment = ex.Message;
            }
        }
    }
}
