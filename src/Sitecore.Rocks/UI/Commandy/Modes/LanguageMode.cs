// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Collections.Generic;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Data.DataServices;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Extensibility.Composition;
using Sitecore.Rocks.Extensions.StringExtensions;
using Sitecore.Rocks.Extensions.XElementExtensions;

namespace Sitecore.Rocks.UI.Commandy.Modes
{
    [Export(typeof(IMode))]
    public class LanguageMode : ModeBase
    {
        private readonly List<LanguageDescriptor> languages = new List<LanguageDescriptor>();

        public LanguageMode([NotNull] Commandy commandy) : base(commandy)
        {
            Assert.ArgumentNotNull(commandy, nameof(commandy));

            Name = "Language";
            Alias = "l";

            var context = commandy.Parameter as IDatabaseSelectionContext;
            if (context == null)
            {
                IsReady = true;
                return;
            }

            if (context.DatabaseUri == DatabaseUri.Empty)
            {
                IsReady = true;
                return;
            }

            var databaseUri = context.DatabaseUri;

            databaseUri.Site.DataService.ExecuteAsync("Languages.GetLanguages", LoadLanguages, databaseUri.DatabaseName.ToString());
        }

        [NotNull]
        public IEnumerable<LanguageDescriptor> Languages
        {
            get { return languages; }
        }

        public override string Watermark
        {
            get { return "Language"; }
        }

        public override bool CanExecute(object parameter)
        {
            var context = parameter as IDatabaseSelectionContext;
            if (context == null)
            {
                return false;
            }

            return context.DatabaseUri != DatabaseUri.Empty;
        }

        public override void Execute(Hit hit, object parameter)
        {
            Assert.ArgumentNotNull(hit, nameof(hit));
            Assert.ArgumentNotNull(parameter, nameof(parameter));

            var context = parameter as IDatabaseSelectionContext;
            if (context == null)
            {
                return;
            }

            var language = hit.Tag as LanguageDescriptor;
            if (language == null)
            {
                return;
            }

            AppHost.Globals.CurrentLanguage = new Language(language.Name);
            AppHost.Server.Languages.SetContextLanguage(context.DatabaseUri.Site, language.Name, (response, result) => DataService.HandleExecute(response, result));
        }

        private void LoadLanguages([NotNull] string response, [NotNull] ExecuteResult executeResult)
        {
            Debug.ArgumentNotNull(response, nameof(response));
            Debug.ArgumentNotNull(executeResult, nameof(executeResult));

            if (!DataService.HandleExecute(response, executeResult))
            {
                return;
            }

            languages.Clear();
            IsReady = true;

            var root = response.ToXElement();
            if (root == null)
            {
                return;
            }

            foreach (var element in root.Elements())
            {
                var name = element.GetAttributeValue("name");
                var displayName = element.GetAttributeValue("displayname");

                languages.Add(new LanguageDescriptor(name, displayName));
            }
        }

        public class LanguageDescriptor
        {
            public LanguageDescriptor([NotNull] string name, [NotNull] string displayName)
            {
                Assert.ArgumentNotNull(name, nameof(name));
                Assert.ArgumentNotNull(displayName, nameof(displayName));

                Name = name;
                DisplayName = displayName;
            }

            [NotNull]
            public string DisplayName { get; private set; }

            [NotNull]
            public string Name { get; }
        }
    }
}
