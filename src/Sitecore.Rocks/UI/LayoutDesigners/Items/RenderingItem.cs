// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing.Design;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using System.Windows.Media.Imaging;
using System.Xml;
using System.Xml.Linq;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Data.DataServices;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Extensions.DateTimeExtensions;
using Sitecore.Rocks.Extensions.GuidExtensions;
using Sitecore.Rocks.Extensions.StringExtensions;
using Sitecore.Rocks.Extensions.XElementExtensions;
using Sitecore.Rocks.Extensions.XmlTextWriterExtensions;
using Sitecore.Rocks.Shell.ComponentModel;
using Sitecore.Rocks.Text;
using Sitecore.Rocks.UI.LayoutDesigners.Properties;
using Sitecore.Rocks.UI.LayoutDesigners.Properties.DataBindings;
using Sitecore.Rocks.UI.LayoutDesigners.Properties.DropDowns;
using Sitecore.Rocks.UI.LayoutDesigners.Properties.Parameters;
using Sitecore.Rocks.UI.LayoutDesigners.Properties.PlaceHolders;

namespace Sitecore.Rocks.UI.LayoutDesigners.Items
{
    public class RenderingItem : LayoutDesignerItem, IItem, INotifyPropertyChanged
    {
        private bool cacheable;

        private string conditions;

        private string dataSource;

        private string dataSourceLocation;

        private string dataSourceTemplate;

        private bool disableRenaming;

        private string filePath;

        private bool hasParameters;

        private Icon icon;

        private string itemId;

        private string multiVariateTests;

        private string name;

        private ParameterDictionary parameterDictionary;

        private string parameters;

        private string parameterTemplateId;

        private PlaceHolderKey placeHolderKey;

        private string ruleset;

        private string uniqueId;

        private bool varyByData;

        private bool varyByDevice;

        private bool varyByLogin;

        private bool varyByParameters;

        private bool varyByQueryString;

        private bool varyByUser;

        public RenderingItem([CanBeNull] IRenderingContainer renderingContainer, [NotNull] IItem item)
        {
            Assert.ArgumentNotNull(item, nameof(item));

            RenderingContainer = renderingContainer;
            Name = item.Name;

            Cacheable = false;
            Conditions = string.Empty;
            DataSource = string.Empty;
            ItemId = item.ItemUri.ItemId.ToString();
            MultiVariateTests = string.Empty;
            Parameters = string.Empty;
            PlaceholderKey = new PlaceHolderKey(string.Empty);
            UniqueId = Guid.NewGuid().Format();
            VaryByData = false;
            VaryByDevice = false;
            VaryByLogin = false;
            VaryByParameters = false;
            VaryByQueryString = false;
            VaryByUser = false;
            PlaceHolders = string.Empty;
            DataSourceLocation = string.Empty;
            DataSourceTemplate = string.Empty;
            FilePath = string.Empty;
            ParameterTemplateId = string.Empty;
            Ruleset = string.Empty;
            SpeakCoreVersion = string.Empty;
            SpeakCoreVersionId = string.Empty;

            ParameterDictionary = new ParameterDictionary(new Dictionary<string, string>());

            Icon = item.Icon;
            ItemUri = item.ItemUri;
        }

        public RenderingItem([CanBeNull] IRenderingContainer renderingContainer, [NotNull] DatabaseUri databaseUri, [NotNull] XElement element)
        {
            Assert.ArgumentNotNull(databaseUri, nameof(databaseUri));
            Assert.ArgumentNotNull(element, nameof(element));

            RenderingContainer = renderingContainer;
            Name = element.GetAttributeValue("name");
            Cacheable = element.GetAttributeValue("cac") == @"1";
            Conditions = element.GetAttributeValue("cnd");
            DataSource = element.GetAttributeValue("ds");
            ItemId = element.GetAttributeValue("id");
            MultiVariateTests = element.GetAttributeValue("mvt");
            Parameters = element.GetAttributeValue("par");
            PlaceholderKey = new PlaceHolderKey(element.GetAttributeValue("ph"));
            UniqueId = element.GetAttributeValue("uid");
            VaryByData = element.GetAttributeValue("vbd") == @"1";
            VaryByDevice = element.GetAttributeValue("vbdev") == @"1";
            VaryByLogin = element.GetAttributeValue("vbl") == @"1";
            VaryByParameters = element.GetAttributeValue("vbp") == @"1";
            VaryByQueryString = element.GetAttributeValue("vbqs") == @"1";
            VaryByUser = element.GetAttributeValue("vbu") == @"1";
            Ruleset = element.GetAttributeValue("rls");

            ParameterDictionary = new ParameterDictionary(new Dictionary<string, string>());
            PlaceHolders = element.GetAttributeValue("placeholders");
            DataSourceLocation = element.GetAttributeValue("dsl");
            DataSourceTemplate = element.GetAttributeValue("dst");
            FilePath = element.GetAttributeValue("path");
            ParameterTemplateId = element.GetAttributeValue("templateid");
            ItemUri = ItemUri.Empty;
            Icon = new Icon(databaseUri.Site, element.GetAttributeValue("ic"));
            SpeakCoreVersion = element.GetAttributeValue("speakcoreversion");
            SpeakCoreVersionId = element.GetAttributeValue("speakcoreversionid");

            if (!string.IsNullOrEmpty(ItemId))
            {
                ItemUri = new ItemUri(databaseUri, new ItemId(new Guid(ItemId)));
            }

            var parameterValues = new UrlString(Parameters);
            foreach (string key in parameterValues.Parameters.Keys)
            {
                ParameterDictionary.Parameters[key] = parameterValues.Parameters[key];
            }

            var templateElement = element.Element("template");
            if (templateElement != null)
            {
                ParseTemplate(templateElement);
            }
        }

        public RenderingItem([CanBeNull] IRenderingContainer renderingContainer, [NotNull] DatabaseUri databaseUri, [NotNull] XElement element, bool isDuplicate) : this(renderingContainer, databaseUri, element)
        {
            Assert.ArgumentNotNull(databaseUri, nameof(databaseUri));
            Assert.ArgumentNotNull(element, nameof(element));

            hasParameters = isDuplicate;
            if (!isDuplicate)
            {
                return;
            }

            UniqueId = Guid.NewGuid().ToString("B").ToUpperInvariant();

            var idProperty = DynamicProperties.FirstOrDefault(p => string.Compare(p.Name, @"id", StringComparison.InvariantCultureIgnoreCase) == 0);
            if (idProperty != null)
            {
                var n = idProperty.Value as string;
                if (!string.IsNullOrEmpty(n))
                {
                    n = n.TrimEnd('0', '1', '2', '3', '4', '5', '6', '7', '8', '9');
                }
                else
                {
                    n = Name;
                }

                disableRenaming = true;
                idProperty.Value = GetNewControlId(n.GetSafeCodeIdentifier());
                disableRenaming = false;
            }
        }

        [Category("Caching"), Description("Determines if the rendering can be cached.")]
        public bool Cacheable
        {
            get { return cacheable; }

            set
            {
                if (cacheable == value)
                {
                    return;
                }

                cacheable = value;
                RaiseModified();
                OnPropertyChanged(nameof(Cacheable));
            }
        }

        [NotNull, Category("Obsolete"), Description("Rules that are run when the rendering is rendered."), Browsable(false)]
        public string Conditions
        {
            get { return conditions; }

            set
            {
                Assert.ArgumentNotNull(value, nameof(value));

                if (conditions == value)
                {
                    return;
                }

                conditions = value;
                RaiseModified();
                OnPropertyChanged(nameof(Conditions));
            }
        }

        [CanBeNull, Category("Behavior"), Description("Sets the context item for the rendering."), Editor(typeof(DataSourceTypeEditor), typeof(UITypeEditor))]
        public string DataSource
        {
            get { return dataSource ?? string.Empty; }

            set
            {
                if (dataSource == value)
                {
                    return;
                }

                dataSource = value;
                RaiseModified();
                OnPropertyChanged(nameof(DataSource));
            }
        }

        [NotNull, Browsable(false)]
        public string DataSourceLocation
        {
            get { return dataSourceLocation ?? string.Empty; }

            set
            {
                Assert.ArgumentNotNull(value, nameof(value));

                if (value == dataSourceLocation)
                {
                    return;
                }

                dataSourceLocation = value;
                OnPropertyChanged(nameof(DataSourceLocation));
            }
        }

        [NotNull, Browsable(false)]
        public string DataSourceTemplate
        {
            get { return dataSourceTemplate ?? string.Empty; }

            set
            {
                Assert.ArgumentNotNull(value, nameof(value));

                if (value == dataSourceTemplate)
                {
                    return;
                }

                dataSourceTemplate = value;
                OnPropertyChanged(nameof(DataSourceTemplate));
            }
        }

        [NotNull, Browsable(false)]
        public string FilePath
        {
            get { return filePath ?? string.Empty; }

            set
            {
                Assert.ArgumentNotNull(value, nameof(value));

                if (value == filePath)
                {
                    return;
                }

                filePath = value;
                OnPropertyChanged(nameof(FilePath));
            }
        }

        [Browsable(false)]
        public Icon Icon
        {
            get { return icon; }

            set
            {
                Assert.ArgumentNotNull(value, nameof(value));

                if (icon == value)
                {
                    return;
                }

                icon = value;
                RaiseModified();
                OnPropertyChanged(nameof(Icon));
                OnPropertyChanged(nameof(IconSource));
            }
        }

        [Browsable(false)]
        public BitmapImage IconSource => icon.GetSource();

        [NotNull, Browsable(false)]
        public string ItemId
        {
            get { return itemId; }

            set
            {
                Assert.ArgumentNotNull(value, nameof(value));

                if (itemId == value)
                {
                    return;
                }

                itemId = value;
                RaiseModified();
                OnPropertyChanged(nameof(ItemId));
            }
        }

        [Browsable(false)]
        public ItemUri ItemUri { get; set; }

        [NotNull, Category("Testing"), Description("Multivariate test configuration."), Editor(typeof(DataSourceTypeEditor), typeof(UITypeEditor))]
        public string MultiVariateTests
        {
            get { return multiVariateTests; }

            set
            {
                Assert.ArgumentNotNull(value, nameof(value));

                if (multiVariateTests == value)
                {
                    return;
                }

                multiVariateTests = value;
                RaiseModified();
                OnPropertyChanged(nameof(MultiVariateTests));
            }
        }

        [Category("Design"), Description("Name."), Browsable(false)]
        public string Name
        {
            get { return name; }

            set
            {
                Assert.ArgumentNotNull(value, nameof(value));

                if (name == value)
                {
                    return;
                }

                name = value;
                RaiseModified();
                OnPropertyChanged(nameof(Name));
            }
        }

        [NotNull, Category("Behavior"), Description("The parameters of the rendering."), DisplayName("Parameters"), Editor(typeof(ParameterDictionaryTypeEditor), typeof(UITypeEditor))]
        public ParameterDictionary ParameterDictionary
        {
            get { return parameterDictionary; }

            set
            {
                if (parameterDictionary == value)
                {
                    return;
                }

                parameterDictionary = value;
                RaiseModified();
                OnPropertyChanged(nameof(ParameterDictionary));
            }
        }

        [NotNull, Browsable(false)]
        public string Parameters
        {
            get { return parameters; }

            set
            {
                Assert.ArgumentNotNull(value, nameof(value));

                if (parameters == value)
                {
                    return;
                }

                parameters = value;
                RaiseModified();
                OnPropertyChanged(nameof(Parameters));
            }
        }

        [NotNull, Browsable(false)]
        public string ParameterTemplateId
        {
            get { return parameterTemplateId ?? string.Empty; }

            set
            {
                Assert.ArgumentNotNull(value, nameof(value));

                if (value == parameterTemplateId)
                {
                    return;
                }

                parameterTemplateId = value;
                OnPropertyChanged(nameof(ParameterTemplateId));
            }
        }

        [NotNull, Category("Behavior"), Description("The name of the place holder where the rendering will be inserted."), Editor(typeof(PlaceholderKeyTypeEditor), typeof(UITypeEditor)), MergableProperty(true)]
        public PlaceHolderKey PlaceholderKey
        {
            get { return placeHolderKey ?? PlaceHolderKey.Empty; }

            set
            {
                if (placeHolderKey == value)
                {
                    return;
                }

                placeHolderKey = value;
                RaiseModified();
                OnPropertyChanged(nameof(PlaceholderKey));
            }
        }

        [NotNull, Browsable(false)]
        public string PlaceHolders { get; set; }

        [CanBeNull, Browsable(false)]
        public IRenderingContainer RenderingContainer { get; set; }

        [NotNull, Category("Personalization"), Editor(typeof(RulesTypeEditor), typeof(UITypeEditor))]
        public string Ruleset
        {
            get { return ruleset ?? string.Empty; }

            set
            {
                Assert.ArgumentNotNull(value, nameof(value));

                if (value == ruleset)
                {
                    return;
                }

                ruleset = value;
                OnPropertyChanged(nameof(Ruleset));
            }
        }

        [NotNull, Browsable(false)]
        public string SpeakCoreVersion { get; set; }

        [NotNull, Browsable(false)]
        public string SpeakCoreVersionId { get; set; }

        [NotNull, Browsable(false)]
        public string UniqueId
        {
            get { return uniqueId; }

            set
            {
                Assert.ArgumentNotNull(value, nameof(value));

                if (uniqueId == value)
                {
                    return;
                }

                uniqueId = value;
                RaiseModified();
                OnPropertyChanged(nameof(UniqueId));
            }
        }

        [Category("Caching"), Description("Vary by data source.")]
        public bool VaryByData
        {
            get { return varyByData; }

            set
            {
                if (varyByData == value)
                {
                    return;
                }

                varyByData = value;
                RaiseModified();
                OnPropertyChanged(nameof(VaryByData));
            }
        }

        [Category("Caching"), Description("Vary by device.")]
        public bool VaryByDevice
        {
            get { return varyByDevice; }

            set
            {
                if (varyByDevice == value)
                {
                    return;
                }

                varyByDevice = value;
                RaiseModified();
                OnPropertyChanged(nameof(VaryByDevice));
            }
        }

        [Category("Caching"), Description("Vary by login.")]
        public bool VaryByLogin
        {
            get { return varyByLogin; }

            set
            {
                if (varyByLogin == value)
                {
                    return;
                }

                varyByLogin = value;
                RaiseModified();
                OnPropertyChanged(nameof(VaryByLogin));
            }
        }

        [Category("Caching"), Description("Vary by parameters.")]
        public bool VaryByParameters
        {
            get { return varyByParameters; }

            set
            {
                if (varyByParameters == value)
                {
                    return;
                }

                varyByParameters = value;
                RaiseModified();
                OnPropertyChanged(nameof(VaryByParameters));
            }
        }

        [Category("Caching"), Description("Vary by query string.")]
        public bool VaryByQueryString
        {
            get { return varyByQueryString; }

            set
            {
                if (varyByQueryString == value)
                {
                    return;
                }

                varyByQueryString = value;
                RaiseModified();
                OnPropertyChanged(nameof(VaryByQueryString));
            }
        }

        [Category("Caching"), Description("Vary by user.")]
        public bool VaryByUser
        {
            get { return varyByUser; }

            set
            {
                if (varyByUser == value)
                {
                    return;
                }

                RaiseModified();
                varyByUser = value;
                OnPropertyChanged(nameof(VaryByUser));
            }
        }

        [CanBeNull]
        private string OldId { get; set; }

        public void ChangeRendering([NotNull] RenderingItem newRenderingItem, [NotNull] Action completed)
        {
            Assert.ArgumentNotNull(completed, nameof(completed));
            Assert.ArgumentNotNull(newRenderingItem, nameof(newRenderingItem));

            Name = newRenderingItem.Name;
            ItemUri = newRenderingItem.ItemUri;
            ItemId = newRenderingItem.ItemId;
            hasParameters = true;
            Icon = newRenderingItem.Icon;

            ExecuteCompleted c = delegate(string response, ExecuteResult result)
            {
                if (!DataService.HandleExecute(response, result))
                {
                    return;
                }

                var root = response.ToXElement();
                if (root == null)
                {
                    return;
                }

                PlaceHolders = root.GetAttributeValue("placeholders");
                DataSourceLocation = root.GetAttributeValue("dsl");
                DataSourceTemplate = root.GetAttributeValue("dst");
                FilePath = root.GetAttributeValue("path");
                ParameterTemplateId = root.GetAttributeValue("templateid");

                var id = GetControlId();

                ParseTemplate(root);

                var idProperty = DynamicProperties.FirstOrDefault(p => string.Compare(p.Name, @"id", StringComparison.InvariantCultureIgnoreCase) == 0);
                if (idProperty != null && string.IsNullOrEmpty(idProperty.Value as string))
                {
                    idProperty.Value = id;
                }

                ReloadPropertyDescriptors();

                RaiseParametersLoaded();

                completed();
            };

            AppHost.Server.Layouts.GetRenderingParameters(newRenderingItem.ItemUri, c);
        }

        public override void Commit()
        {
            var parms = new UrlString();

            foreach (var pair in ParameterDictionary.Parameters)
            {
                if (pair.Value != null)
                {
                    parms[pair.Key] = pair.Value;
                }
            }

            foreach (var property in DynamicProperties)
            {
                string v;

                var value = property.Value;
                if (value == null)
                {
                    v = string.Empty;
                }
                else if (property.Type == typeof(bool?))
                {
                    if (value is string)
                    {
                        v = (value as string == @"1") || (string.Compare(value as string, @"true", StringComparison.InvariantCultureIgnoreCase) == 0) ? @"1" : @"0";
                    }
                    else
                    {
                        v = (bool)value ? @"1" : @"0";
                    }
                }
                else if (property.Type == typeof(DateTime))
                {
                    if (value is string)
                    {
                        v = value as string;
                    }
                    else
                    {
                        var dateTime = (DateTime)value;
                        v = dateTime == DateTime.MinValue ? string.Empty : DateTimeExtensions.ToIsoDate(dateTime);
                    }
                }
                else if (property.Type == typeof(DataBinding))
                {
                    if (value is string)
                    {
                        v = value as string;
                    }
                    else
                    {
                        v = ((DataBinding)value).Binding;
                    }

                    var dataBindingConverter = property.Converter as DataBindingConverter;
                    if (dataBindingConverter != null && dataBindingConverter.Values != null)
                    {
                        var item = v;
                        var tuple = dataBindingConverter.Values.FirstOrDefault(t => t.Item1 == item);
                        if (tuple != null)
                        {
                            v = tuple.Item2;
                        }
                    }

                    if (string.Compare(property.TypeName, "Checkbox", StringComparison.InvariantCultureIgnoreCase) == 0)
                    {
                        if (string.Compare(v, "True", StringComparison.InvariantCultureIgnoreCase) == 0)
                        {
                            v = "1";
                        }
                        else if (string.Compare(v, "False", StringComparison.InvariantCultureIgnoreCase) == 0)
                        {
                            v = "0";
                        }
                    }

                    if (string.Compare(property.TypeName, "TriState", StringComparison.InvariantCultureIgnoreCase) == 0)
                    {
                        if (v == "1" || string.Compare(v, "True", StringComparison.InvariantCultureIgnoreCase) == 0)
                        {
                            v = "1";
                        }
                        else if (v == "0" || string.Compare(v, "False", StringComparison.InvariantCultureIgnoreCase) == 0)
                        {
                            v = "0";
                        }
                        else if (v == "-" || string.Compare(v, "Indetermined", StringComparison.InvariantCultureIgnoreCase) == 0)
                        {
                            v = "-";
                        }
                    }
                }
                else if (property.Type == typeof(DropDownValue))
                {
                    if (value is string)
                    {
                        v = value as string;
                    }
                    else
                    {
                        var tuple = ((DropDownValue)value).Value;
                        v = tuple != null ? tuple.Item2 : string.Empty;
                    }
                }
                else
                {
                    v = value.ToString();
                }

                if (!string.IsNullOrEmpty(v))
                {
                    parms[property.Name] = v;
                }
                else
                {
                    parms.Remove(property.Name);
                }
            }

            Parameters = parms.ToString();
        }

        public void CopyFrom([NotNull] RenderingItem item)
        {
            Assert.ArgumentNotNull(item, nameof(item));

            Cacheable = item.Cacheable;
            Conditions = item.Conditions;
            DataSource = item.DataSource;
            ItemId = item.ItemId;
            MultiVariateTests = item.MultiVariateTests;
            Name = item.Name + @" - " + "Copy";
            Parameters = item.Parameters;
            UniqueId = Guid.NewGuid().Format();
            VaryByData = item.VaryByData;
            VaryByDevice = item.VaryByDevice;
            VaryByLogin = item.VaryByLogin;
            VaryByParameters = item.VaryByParameters;
            VaryByQueryString = item.VaryByQueryString;
            VaryByUser = item.VaryByUser;
            Icon = item.Icon;
            DataSourceLocation = item.DataSourceLocation;
            DataSourceTemplate = item.DataSourceTemplate;
            FilePath = item.FilePath;
            ParameterTemplateId = item.ParameterTemplateId;
            ItemUri = item.ItemUri;

            ParameterDictionary.Parameters.Clear();
            foreach (var parameter in item.ParameterDictionary.Parameters)
            {
                ParameterDictionary.Parameters[parameter.Key] = parameter.Value;
            }

            ParameterDictionary.Parameters["Id"] = Name;
        }

        [NotNull]
        public string GetControlId()
        {
            var property = DynamicProperties.FirstOrDefault(p => string.Compare(p.Name, @"Id", StringComparison.InvariantCultureIgnoreCase) == 0);
            if (property == null)
            {
                return string.Empty;
            }

            return property.Value as string ?? string.Empty;
        }

        [NotNull]
        public string GetDisplayName()
        {
            var result = string.Empty;

            var id = DynamicProperties.FirstOrDefault(p => string.Compare(p.Name, @"id", StringComparison.InvariantCultureIgnoreCase) == 0);
            if (id != null)
            {
                result = id.Value as string ?? string.Empty;
            }

            if (string.IsNullOrEmpty(result))
            {
                result = Name;
            }

            return result;
        }

        [NotNull]
        public string GetNewControlId([NotNull] string controlName, bool addNumber = false)
        {
            Assert.ArgumentNotNull(controlName, nameof(controlName));

            var id = controlName;
            var index = 1;

            controlName = controlName.TrimEnd('0', '1', '2', '3', '4', '5', '6', '7', '8', '9');
            if (addNumber && controlName == id)
            {
                id = controlName + 1;
                index++;
            }

            while (ControlIdExists(id))
            {
                id = controlName + index;
                index++;
            }

            return id;
        }

        [CanBeNull]
        public string GetParameterValue([NotNull] string parameterName)
        {
            Assert.ArgumentNotNull(parameterName, nameof(parameterName));

            var property = DynamicProperties.FirstOrDefault(p => p.Name == parameterName);
            if (property == null)
            {
                return null;
            }

            var value = property.Value;
            if (value == null)
            {
                return null;
            }

            return value.ToString();
        }

        [NotNull]
        public IEnumerable<string> GetPlaceHolderNames()
        {
            var list = PlaceHolders.Replace('|', ',');

            foreach (var placeHolders in list.Split(','))
            {
                if (string.IsNullOrEmpty(placeHolders))
                {
                    continue;
                }

                yield return GetPlaceHolderName(this, placeHolders);
            }
        }

        public void GetRenderingParameters([NotNull] ItemUri itemUri, [NotNull] Action completed)
        {
            Assert.ArgumentNotNull(itemUri, nameof(itemUri));
            Assert.ArgumentNotNull(completed, nameof(completed));

            if (hasParameters)
            {
                return;
            }

            hasParameters = true;

            ExecuteCompleted c = delegate(string response, ExecuteResult result)
            {
                if (!DataService.HandleExecute(response, result))
                {
                    return;
                }

                var root = response.ToXElement();
                if (root == null)
                {
                    return;
                }

                if (string.IsNullOrEmpty(Name))
                {
                    Name = root.GetAttributeValue("name");
                }

                Icon = new Icon(itemUri.DatabaseUri.Site, root.GetAttributeValue("icon"));
                PlaceHolders = root.GetAttributeValue("placeholders");
                DataSourceLocation = root.GetAttributeValue("dsl");
                DataSourceTemplate = root.GetAttributeValue("dst");
                FilePath = root.GetAttributeValue("path");
                ParameterTemplateId = root.GetAttributeValue("templateid");

                ApplyDefaultParameters(root);
                ParseTemplate(root);

                var idProperty = DynamicProperties.FirstOrDefault(p => string.Compare(p.Name, @"id", StringComparison.InvariantCultureIgnoreCase) == 0);
                if (idProperty != null && string.IsNullOrEmpty(idProperty.Value as string))
                {
                    disableRenaming = true;
                    idProperty.Value = GetNewControlId(Name.GetSafeCodeIdentifier() + "1");
                    disableRenaming = false;
                }

                ReloadPropertyDescriptors();

                RaiseParametersLoaded();

                completed();
            };

            AppHost.Server.Layouts.GetRenderingParameters(itemUri, c);
        }

        public event EventHandler ParametersLoaded;

        public event PropertyChangedEventHandler PropertyChanged;

        public void SetParameterValue([NotNull] string parameterName, [CanBeNull] object value)
        {
            Assert.ArgumentNotNull(parameterName, nameof(parameterName));

            var property = DynamicProperties.FirstOrDefault(p => p.Name == parameterName);

            if (property != null)
            {
                property.Value = value;
            }
        }

        public override void Write(XmlTextWriter output, bool isCopy)
        {
            Assert.ArgumentNotNull(output, nameof(output));

            output.WriteStartElement(@"r");

            output.WriteAttributeStringNotEmpty(@"cac", Cacheable ? @"1" : string.Empty);
            output.WriteAttributeStringNotEmpty(@"cnd", Conditions);
            output.WriteAttributeStringNotEmpty(@"ds", DataSource);
            output.WriteAttributeStringNotEmpty(@"id", ItemId);
            output.WriteAttributeStringNotEmpty(@"mvt", MultiVariateTests);
            output.WriteAttributeStringNotEmpty(@"par", Parameters);
            output.WriteAttributeStringNotEmpty(@"ph", PlaceholderKey.Key);
            output.WriteAttributeStringNotEmpty(@"uid", UniqueId);
            output.WriteAttributeStringNotEmpty(@"vbd", VaryByData ? @"1" : string.Empty);
            output.WriteAttributeStringNotEmpty(@"vbdev", VaryByDevice ? @"1" : string.Empty);
            output.WriteAttributeStringNotEmpty(@"vbl", VaryByLogin ? @"1" : string.Empty);
            output.WriteAttributeStringNotEmpty(@"vbp", VaryByParameters ? @"1" : string.Empty);
            output.WriteAttributeStringNotEmpty(@"vbqs", VaryByQueryString ? @"1" : string.Empty);
            output.WriteAttributeStringNotEmpty(@"vbu", VaryByUser ? @"1" : string.Empty);

            if (isCopy)
            {
                output.WriteAttributeStringNotEmpty(@"name", Name);
                output.WriteAttributeStringNotEmpty(@"placeholders", PlaceHolders);
                output.WriteAttributeStringNotEmpty(@"dsl", DataSourceLocation);
                output.WriteAttributeStringNotEmpty(@"dst", DataSourceTemplate);
                output.WriteAttributeStringNotEmpty(@"path", FilePath);
                output.WriteAttributeStringNotEmpty(@"templateid", ParameterTemplateId);
                output.WriteAttributeStringNotEmpty(@"ic", Icon.IconPath);

                output.WriteStartElement("template");

                foreach (var property in DynamicProperties)
                {
                    output.WriteStartElement("field");

                    output.WriteAttributeString("name", property.Name);
                    output.WriteAttributeString("displayname", property.DisplayName);
                    output.WriteAttributeString("category", property.Category);
                    output.WriteAttributeString("type", property.TypeName);
                    output.WriteAttributeString("description", property.Description);
                    output.WriteAttributeString("bindmode", property.Attributes["bindmode"] != null ? property.Attributes["bindmode"].ToString() : string.Empty);

                    var dataBindingConverter = property.Converter as DataBindingConverter;
                    if (dataBindingConverter != null && dataBindingConverter.Values != null)
                    {
                        output.WriteStartElement("values");

                        foreach (var value in dataBindingConverter.Values)
                        {
                            output.WriteStartElement("value");
                            output.WriteAttributeString("displayname", value.Item1);
                            output.WriteValue(value.Item2);
                            output.WriteEndElement();
                        }

                        output.WriteEndElement();
                    }

                    var dropDownValueConverter = property.Converter as DropDownValueConverter;
                    if (dropDownValueConverter != null)
                    {
                        output.WriteStartElement("values");

                        foreach (var value in dropDownValueConverter.Values)
                        {
                            output.WriteStartElement("value");
                            output.WriteAttributeString("displayname", value.Item1);
                            output.WriteValue(value.Item2);
                            output.WriteEndElement();
                        }

                        output.WriteEndElement();
                    }

                    output.WriteEndElement();
                }

                output.WriteEndElement();
            }

            if (!string.IsNullOrEmpty(Ruleset))
            {
                output.WriteRaw("<rls>" + Ruleset + "</rls>");
            }

            output.WriteEndElement();
        }

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([NotNull] string propertyName)
        {
            Debug.ArgumentNotNull(propertyName, nameof(propertyName));

            var handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        private void ApplyDefaultParameters([NotNull] XElement templateElement)
        {
            Debug.ArgumentNotNull(templateElement, nameof(templateElement));

            PlaceHolders = templateElement.GetAttributeValue("placeholders");

            var defaultParameters = templateElement.GetAttributeValue("parameters");
            if (string.IsNullOrEmpty(defaultParameters))
            {
                return;
            }

            var parameterValues = new UrlString(Parameters);
            var p = new UrlString(defaultParameters);

            foreach (string key in p.Parameters.Keys)
            {
                switch (key.ToUpperInvariant())
                {
                    case "PLACEHOLDER":
                        PlaceholderKey = new PlaceHolderKey(p.Parameters[key]);
                        break;
                    case "DATASOURCE":
                        DataSource = p.Parameters[key];
                        break;
                    default:
                        parameterValues.Parameters[key] = p.Parameters[key];
                        break;
                }
            }

            parameters = parameterValues.ToString();
        }

        private bool ControlIdExists([NotNull] string id)
        {
            Debug.ArgumentNotNull(id, nameof(id));

            if (RenderingContainer == null)
            {
                return false;
            }

            return RenderingContainer.Renderings.Any(r => string.Compare(r.GetControlId(), id, StringComparison.InvariantCultureIgnoreCase) == 0);
        }

        [NotNull]
        private string GetPlaceHolderName([NotNull] RenderingItem rendering, [NotNull] string placeHolders)
        {
            Debug.ArgumentNotNull(rendering, nameof(rendering));
            Debug.ArgumentNotNull(placeHolders, nameof(placeHolders));

            return placeHolders.Replace("$Id", rendering.GetControlId()).Trim();
        }

        private void ParseTemplate([NotNull] XElement templateElement)
        {
            Debug.ArgumentNotNull(templateElement, nameof(templateElement));

            var parameterValues = new UrlString(Parameters);

            DynamicProperties.Clear();

            foreach (var field in templateElement.Elements())
            {
                var n = field.GetAttributeValue("name");
                if (DynamicProperties.Any(p => p.Name == n))
                {
                    continue;
                }

                var displayName = field.GetAttributeValue("displayname");
                var category = field.GetAttributeValue("category");
                var typeName = field.GetAttributeValue("type");
                var subType = field.GetAttributeValue("subtype");
                var description = field.GetAttributeValue("description");
                var bindModeString = field.GetAttributeValue("bindmode");
                var editor = field.GetAttributeValue("editor");

                var value = HttpUtility.UrlDecode(parameterValues[n] ?? string.Empty);

                var bindMode = BindingMode.None;
                if (!string.IsNullOrEmpty(bindModeString))
                {
                    Enum.TryParse(bindModeString, out bindMode);
                }

                List<Tuple<string, string>> values = null;
                var valuesElement = field.Element("values");
                if (valuesElement != null)
                {
                    values = new List<Tuple<string, string>>();
                    foreach (var element in valuesElement.Elements())
                    {
                        values.Add(new Tuple<string, string>(element.GetAttributeValue("displayname", element.Value), element.Value));
                    }
                }

                parameterValues.Remove(n);

                object actualValue = null;
                Type type;
                if (bindMode == BindingMode.ReadWrite || bindMode == BindingMode.Write || string.Compare(category, @"Data Bindings", StringComparison.InvariantCultureIgnoreCase) == 0)
                {
                    type = typeof(DataBinding);
                    var bindingValue = value;

                    if (string.Compare(typeName, @"checkbox", StringComparison.InvariantCultureIgnoreCase) == 0)
                    {
                        if (value == @"1" || string.Compare(value, @"true", StringComparison.InvariantCultureIgnoreCase) == 0)
                        {
                            bindingValue = "True";
                        }
                        else if (value == @"0" || string.Compare(value, @"false", StringComparison.InvariantCultureIgnoreCase) == 0)
                        {
                            bindingValue = "False";
                        }
                    }

                    if (values != null)
                    {
                        var item = values.FirstOrDefault(i => i.Item2 == bindingValue);
                        if (item != null)
                        {
                            bindingValue = item.Item1;
                        }
                    }

                    actualValue = new DataBinding(bindingValue);
                }
                else if (string.Compare(typeName, @"checkbox", StringComparison.InvariantCultureIgnoreCase) == 0)
                {
                    type = typeof(bool?);
                    if (value == @"1" || string.Compare(value, @"true", StringComparison.InvariantCultureIgnoreCase) == 0)
                    {
                        actualValue = true;
                    }
                    else if (value == @"0" || string.Compare(value, @"false", StringComparison.InvariantCultureIgnoreCase) == 0)
                    {
                        actualValue = false;
                    }
                }
                else if (string.Compare(typeName, @"tristate", StringComparison.InvariantCultureIgnoreCase) == 0)
                {
                    type = typeof(bool?);
                    if (value == @"1" || string.Compare(value, @"true", StringComparison.InvariantCultureIgnoreCase) == 0)
                    {
                        actualValue = true;
                    }
                    else if (value == @"0" || string.Compare(value, @"false", StringComparison.InvariantCultureIgnoreCase) == 0)
                    {
                        actualValue = false;
                    }
                    else if (value == @"-")
                    {
                        actualValue = null;
                    }
                }
                else if (string.Compare(typeName, @"datetime", StringComparison.InvariantCultureIgnoreCase) == 0 || string.Compare(typeName, @"date", StringComparison.InvariantCultureIgnoreCase) == 0)
                {
                    type = typeof(DateTime);
                    actualValue = value != null ? DateTimeExtensions.FromIso(value) : DateTime.MinValue;
                }
                else if (values != null)
                {
                    type = typeof(DropDownValue);

                    var item = values.FirstOrDefault(i => i.Item2 == value) ?? new Tuple<string, string>(value, value);
                    actualValue = new DropDownValue(item);
                }
                else
                {
                    type = typeof(string);
                    actualValue = value;
                }

                var dynamicProperty = new DynamicProperty(GetType(), n, displayName, type, typeName, category, description, editor, actualValue, this);
                dynamicProperty.Modified += RaiseModified;
                dynamicProperty.PropertyChanged += RaisePropertyChanged;

                if (type == typeof(DataBinding))
                {
                    dynamicProperty.Converter = new DataBindingConverter(values);
                }
                else if (type == typeof(DropDownValue) && values != null)
                {
                    dynamicProperty.Converter = new DropDownValueConverter(values);
                }

                if (bindMode == BindingMode.Read)
                {
                    dynamicProperty.IsReadOnly = true;
                }

                if (n == "Id")
                {
                    OldId = value;
                }

                dynamicProperty.Attributes["subtype"] = subType;
                dynamicProperty.Attributes["bindmode"] = bindMode;

                DynamicProperties.Add(dynamicProperty);
            }

            ParameterDictionary.Parameters.Clear();
            foreach (string key in parameterValues.Parameters.Keys)
            {
                ParameterDictionary.Parameters[key] = parameterValues.Parameters[key];
            }
        }

        private void RaiseModified([NotNull] object sender, [NotNull] EventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            RaiseModified();
        }

        private void RaiseParametersLoaded()
        {
            var handler = ParametersLoaded;
            if (handler != null)
            {
                handler(this, EventArgs.Empty);
            }
        }

        private void RaisePropertyChanged([NotNull] object sender, [NotNull] PropertyChangedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            var property = sender as DynamicProperty;
            if (property == null)
            {
                return;
            }

            OnPropertyChanged(property.Name);

            if (property.Name == "Id")
            {
                RenameRendering(GetControlId());
            }
        }

        private void RenameRendering([NotNull] string newId)
        {
            Debug.ArgumentNotNull(newId, nameof(newId));

            var oldId = OldId;
            OldId = newId;
            if (string.IsNullOrEmpty(oldId))
            {
                return;
            }

            if (disableRenaming)
            {
                return;
            }

            var renderingContainer = RenderingContainer;
            if (renderingContainer == null)
            {
                return;
            }

            var pattern = "\\b" + oldId + "\\b";

            foreach (var renderingItem in renderingContainer.Renderings)
            {
                var placeHolder = renderingItem.PlaceholderKey;
                if (placeHolder.Key != null)
                {
                    var key = Regex.Replace(placeHolder.Key, pattern, newId);
                    if (key != placeHolder.Key)
                    {
                        renderingItem.PlaceholderKey = new PlaceHolderKey(key);
                    }
                }

                foreach (var dynamicProperty in renderingItem.DynamicProperties)
                {
                    if (dynamicProperty.Name == "Id")
                    {
                        continue;
                    }

                    var binding = dynamicProperty.Value as DataBinding;
                    if (binding == null || binding.Binding == null)
                    {
                        continue;
                    }

                    var bindingValue = Regex.Replace(binding.Binding, pattern, newId);
                    if (bindingValue != binding.Binding)
                    {
                        dynamicProperty.Value = new DataBinding(bindingValue);
                    }
                }
            }
        }
    }
}
