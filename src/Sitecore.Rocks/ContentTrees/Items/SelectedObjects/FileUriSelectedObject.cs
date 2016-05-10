// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.ComponentModel;
using System.IO;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Diagnostics;

namespace Sitecore.Rocks.ContentTrees.Items.SelectedObjects
{
    public class FileUriSelectedObject : SiteSelectedObject
    {
        public FileUriSelectedObject([NotNull] FileUri fileUri, [NotNull] string serverFileName = "") : base(fileUri.Site)
        {
            Assert.ArgumentNotNull(fileUri, nameof(fileUri));
            Assert.ArgumentNotNull(serverFileName, nameof(serverFileName));

            FileUri = fileUri;
            ServerFileName = serverFileName;
        }

        [NotNull, Description("The location of the file."), DisplayName("Base Folder"), Category("File")]
        public string BaseFolder
        {
            get
            {
                switch (FileUri.BaseFolder)
                {
                    case FileUriBaseFolder.Web:
                        return "Web Folder";
                    case FileUriBaseFolder.Data:
                        return "Data Folder";
                }

                return string.Empty;
            }
        }

        [NotNull, Description("The name of the file."), DisplayName("File Name"), Category("File")]
        public string FileName
        {
            get { return Path.GetFileName(FileUri.FileName) ?? string.Empty; }
        }

        [NotNull, Description("The location of the file on the local machine."), DisplayName("Local File Name"), Category("File")]
        public string LocalFileName
        {
            get
            {
                var webRootPath = Site.WebRootPath;
                if (string.IsNullOrEmpty(webRootPath))
                {
                    return "[Not found]";
                }

                string fileName;

                if (FileUri.BaseFolder == FileUriBaseFolder.Data)
                {
                    fileName = ServerFileName;
                }
                else
                {
                    fileName = Path.Combine(webRootPath, FileUri.RelativeFileName);
                }

                if (!File.Exists(fileName) && !Directory.Exists(fileName))
                {
                    return "[Not Found]";
                }

                return fileName;
            }
        }

        [NotNull]
        protected FileUri FileUri { get; }

        [NotNull]
        protected string ServerFileName { get; }
    }
}
