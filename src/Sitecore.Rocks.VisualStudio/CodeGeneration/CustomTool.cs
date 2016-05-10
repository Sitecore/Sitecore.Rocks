// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Text;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.Win32;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Projects;
using Sitecore.Rocks.Sites;

namespace Sitecore.Rocks.CodeGeneration
{
    [ComVisible(true), UsedImplicitly]
    public abstract class CustomTool : IVsSingleFileGenerator, IObjectWithSite
    {
        [CanBeNull]
        public object Site { get; private set; }

        [CanBeNull]
        protected string InputFileContents { get; set; }

        [CanBeNull]
        protected string InputFilePath { get; set; }

        [ComRegisterFunction, UsedImplicitly]
        public static void Register([NotNull] Type type)
        {
            Assert.ArgumentNotNull(type, nameof(type));

            using (var parent = Registry.LocalMachine.OpenSubKey(@"Software\Microsoft\VisualStudio\9.0", true))
            {
                foreach (CustomToolRegistrationAttribute ourData in type.GetCustomAttributes(typeof(CustomToolRegistrationAttribute), false))
                {
                    ourData.Register(parent.CreateSubKey, (x, name, value) => x.SetValue(name, value));
                }
            }
        }

        [ComUnregisterFunction, UsedImplicitly]
        public static void Unregister([NotNull] Type type)
        {
            Assert.ArgumentNotNull(type, nameof(type));

            using (var parent = Registry.LocalMachine.OpenSubKey(@"Software\Microsoft\VisualStudio\9.0", true))
            {
                if (parent == null)
                {
                    return;
                }

                var p = parent;
                foreach (CustomToolRegistrationAttribute ourData in type.GetCustomAttributes(typeof(CustomToolRegistrationAttribute), false))
                {
                    ourData.Unregister(x => p.DeleteSubKey(x, false));
                }
            }
        }

        [CanBeNull, Localizable(false)]
        protected virtual string GetOutput()
        {
            return null;
        }

        [CanBeNull]
        protected virtual byte[] GetOutputBytes()
        {
            string result;
            try
            {
                result = GetOutput();
            }
            catch (Exception ex)
            {
                result = string.Format("/*\r\nAn exception was thrown:\r\n{0}\r\n\r\n{1}\r\n*/", ex.Message, ex.StackTrace);
            }

            if (result == null)
            {
                return null;
            }

            return Encoding.UTF8.GetBytes(result);
        }

        [CanBeNull]
        protected Site GetSite()
        {
            var inputFilePath = InputFilePath;
            if (string.IsNullOrEmpty(inputFilePath))
            {
                return null;
            }

            foreach (var project in ProjectManager.Projects)
            {
                if (inputFilePath.StartsWith(project.FolderName, StringComparison.InvariantCultureIgnoreCase))
                {
                    return project.Site;
                }
            }

            return null;

            /*
      try
      {
        var path = this.GetSolutionExplorerPath(inputFilePath);

        var item = SitecorePackage.Instance.Dte.ToolWindows.SolutionExplorer.GetItem(path);
        if (item == null)
        {
          return null;
        }

        var projectItem = item.Object as EnvDTE.ProjectItem;
        if (projectItem == null)
        {
          return null;
        }

        var project = ProjectManager.GetProject(projectItem);
        if (project == null)
        {
          return null;
        }

        return project.Site;
      }
      catch
      {
        return null;
      }
      */
        }

        int IVsSingleFileGenerator.DefaultExtension([NotNull] out string defaultExtension)
        {
            defaultExtension = @".Designer.cs";
            return 0;
        }

        int IVsSingleFileGenerator.Generate([CanBeNull] string inputFilePath, [CanBeNull] string inputFileContents, [CanBeNull] string defaultNamespace, [NotNull] IntPtr[] outputFileContents, out uint output, [CanBeNull] IVsGeneratorProgress progress)
        {
            Debug.ArgumentNotNull(outputFileContents, nameof(outputFileContents));

            InputFilePath = inputFilePath;
            InputFileContents = inputFileContents;

            var bytes = GetOutputBytes() ?? new byte[0];

            var outputLength = bytes.Length;

            outputFileContents[0] = Marshal.AllocCoTaskMem(outputLength);
            Marshal.Copy(bytes, 0, outputFileContents[0], outputLength);

            output = (uint)outputLength;
            return VSConstants.S_OK;
        }

        void IObjectWithSite.GetSite(ref Guid id, out IntPtr site)
        {
            IntPtr intPointer;

            var unknown = Marshal.GetIUnknownForObject(Site);
            Marshal.QueryInterface(unknown, ref id, out intPointer);

            site = intPointer;
        }

        void IObjectWithSite.SetSite([CanBeNull] object site)
        {
            Site = site;
        }
    }
}
