// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.Collections.Generic;
using System.Reflection;
using EnvDTE;
using Microsoft.VisualStudio.TemplateWizard;
using Microsoft.Win32;

namespace Sitecore.Rocks.TemplateWizard
{
    public class Wizard : IWizard
    {
        private readonly IWizard wizard;

        public Wizard()
        {
            var type = Type.GetType("Sitecore.Rocks.Shell.Wizards.Wizard");

            if (type == null)
            {
                var registryKey = Registry.CurrentUser.OpenSubKey("Software\\Sitecore\\Sitecore.Rocks.VisualStudio\\");
                if (registryKey == null)
                {
                    return;
                }

                var path = registryKey.GetValue("ExtensionAssemblyPath") as string ?? string.Empty;
                if (!string.IsNullOrEmpty(path))
                {
                    var assembly = Assembly.LoadFile(path);
                    if (assembly != null)
                    {
                        type = assembly.GetType("Sitecore.Rocks.Shell.Wizards.Wizard");
                    }
                }
            }

            if (type == null)
            {
                return;
            }

            wizard = Activator.CreateInstance(type) as IWizard;
        }

        public void BeforeOpeningFile(ProjectItem projectItem)
        {
            if (wizard != null)
            {
                wizard.BeforeOpeningFile(projectItem);
            }
        }

        public void ProjectFinishedGenerating(Project project)
        {
            if (wizard != null)
            {
                wizard.ProjectFinishedGenerating(project);
            }
        }

        public void ProjectItemFinishedGenerating(ProjectItem projectItem)
        {
            if (wizard != null)
            {
                wizard.ProjectItemFinishedGenerating(projectItem);
            }
        }

        public void RunFinished()
        {
            if (wizard != null)
            {
                wizard.RunFinished();
            }
        }

        public void RunStarted(object automationObject, Dictionary<string, string> replacementsDictionary, WizardRunKind runKind, object[] customParams)
        {
            if (wizard != null)
            {
                wizard.RunStarted(automationObject, replacementsDictionary, runKind, customParams);
            }
        }

        public bool ShouldAddProjectItem(string filePath)
        {
            if (wizard != null)
            {
                return wizard.ShouldAddProjectItem(filePath);
            }

            return true;
        }
    }
}
