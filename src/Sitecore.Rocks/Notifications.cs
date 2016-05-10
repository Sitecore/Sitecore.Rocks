// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Windows;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.ContentEditors;
using Sitecore.Rocks.ContentTrees.Items;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Extensions.DependencyObjectExtensions;
using Sitecore.Rocks.Sites;

namespace Sitecore.Rocks
{
    public static class Notifications
    {
        public delegate void DatabaseEventHandler([NotNull] object sender, [NotNull] DatabaseUri databaseUri);

        public delegate void DataServiceStatusChangedEventHandler(DataService dataService, DataServiceStatus newStatus, DataServiceStatus previousStatus);

        public delegate void FieldChangedEventHandler(object sender, [NotNull] FieldUri fieldUri, [NotNull] string newValue);

        public delegate void FileSavedEventHandler([NotNull] string fileName);

        public delegate void ItemAddedEventHandler(object sender, ItemVersionUri itemVersionUri, ItemUri parentItemUri);

        public delegate void ItemDeletedEventHandler(object sender, ItemUri itemUri);

        public delegate void ItemDuplicatedEventHandler(object sender, ItemUri newItemUri, ItemUri sourceItemUri);

        public delegate void ItemModifiedEventHandler(object sender, ContentModel contentModel, bool isModified);

        public delegate void ItemMovedEventHandler(object sender, ItemUri newParentUri, ItemVersionUri itemUri);

        public delegate void ItemRenamedEventHandler(object sender, ItemUri itemUri, string newName);

        public delegate void ItemSerializedEventHandler(object sender, ItemUri itemUri, SerializationOperation serializationOperation);

        public delegate void ItemsSavedEventHandler(object sender, ContentModel contentModel);

        public delegate void ItemTemplateChangedEventHandler(object sender, ItemUri itemUri, ItemUri newTemplateUri);

        public delegate void ItemTreeViewDragOverEventHandler(object sender, ItemTreeViewItem item, DragEventArgs args);

        public delegate void ItemTreeViewDropEventHandler(object sender, ItemTreeViewItem item, DragEventArgs args);

        public delegate void ItemXmlPastedEventHandler(object sender, ItemUri parentItemUri, string xml, bool changeIds);

        public delegate void MediaUploadedEventHandler(object sender, ItemHeader itemHeader);

        public delegate void PublishingEventHandler(object sender, int mode, DatabaseName databaseName);

        public delegate void PublishingItemEventHandler(object sender, ItemUri itemUri, bool deep, bool compareRevisisions);

        public delegate void SettingChangedEventHandler(string path, string key);

        public delegate void SiteEventHandler(object sender, Site site);

        public delegate void SiteModifiedEventHandler(object sender, Site site, string oldHostName, string oldUserName);

        public delegate void TemplateSavedEventHandler(object sender, ItemUri templateUri, string templateName);

        public delegate void UnloadedEventHandler(object sender, object window);

        public delegate void VersionAddedEventHandler(object sender, ItemVersionUri itemVersionUri);

        public delegate void VersionRemovedEventHandler(object sender, ItemVersionUri deletedVersionUri, ItemVersionUri newVersionUri);

        public static event DatabaseEventHandler ActiveDatabaseChanged;

        public static event DataServiceStatusChangedEventHandler DataServiceStatusChanged;

        public static event RoutedEventHandler FeaturesChanged;

        public static event FieldChangedEventHandler FieldChanged;

        public static event FileSavedEventHandler FileSaved;

        public static event ItemAddedEventHandler ItemAdded;

        public static event ItemDeletedEventHandler ItemDeleted;

        public static event ItemDuplicatedEventHandler ItemDuplicated;

        public static event ItemModifiedEventHandler ItemModified;

        public static event ItemMovedEventHandler ItemMoved;

        public static event ItemRenamedEventHandler ItemRenamed;

        public static event ItemSerializedEventHandler ItemSerialized;

        public static event ItemsSavedEventHandler ItemsSaved;

        public static event ItemTemplateChangedEventHandler ItemTemplateChanged;

        public static event ItemTreeViewDragOverEventHandler ItemTreeViewDragOver;

        public static event ItemTreeViewDropEventHandler ItemTreeViewDrop;

        public static event ItemXmlPastedEventHandler ItemXmlPasted;

        public static event MediaUploadedEventHandler MediaUploaded;

        public static event PublishingEventHandler Publishing;

        public static event PublishingItemEventHandler PublishingItem;

        public static void RaiseActiveDatabaseChanged([NotNull] object sender, [NotNull] DatabaseUri newActiveDatabase)
        {
            Assert.ArgumentNotNull(sender, nameof(sender));
            Assert.ArgumentNotNull(newActiveDatabase, nameof(newActiveDatabase));

            var activeDatabaseChanged = ActiveDatabaseChanged;
            if (activeDatabaseChanged != null)
            {
                activeDatabaseChanged(sender, newActiveDatabase);
            }
        }

        public static void RaiseDataServiceStatusChanged([NotNull] DataService dataService, DataServiceStatus newStatus, DataServiceStatus previousStatus)
        {
            Assert.ArgumentNotNull(dataService, nameof(dataService));

            var changed = DataServiceStatusChanged;
            if (changed != null)
            {
                changed(dataService, newStatus, previousStatus);
            }
        }

        public static void RaiseFeaturesChanged([NotNull] object sender)
        {
            Assert.ArgumentNotNull(sender, nameof(sender));

            var changed = FeaturesChanged;
            if (changed != null)
            {
                changed(sender, new RoutedEventArgs());
            }
        }

        public static void RaiseFieldChanged([NotNull] object sender, [NotNull] FieldUri fieldUri, [NotNull] string newValue)
        {
            Assert.ArgumentNotNull(sender, nameof(sender));
            Assert.ArgumentNotNull(fieldUri, nameof(fieldUri));
            Assert.ArgumentNotNull(newValue, nameof(newValue));

            var fieldChanged = FieldChanged;
            if (fieldChanged != null)
            {
                fieldChanged(sender, fieldUri, newValue);
            }
        }

        public static void RaiseFileSaved([NotNull] string fileName)
        {
            Assert.ArgumentNotNull(fileName, nameof(fileName));

            var fileSaved = FileSaved;
            if (fileSaved != null)
            {
                fileSaved(fileName);
            }
        }

        public static void RaiseItemAdded([NotNull] object sender, [NotNull] ItemVersionUri itemVersionUri, [NotNull] ItemUri parentItemUri)
        {
            Assert.ArgumentNotNull(sender, nameof(sender));
            Assert.ArgumentNotNull(itemVersionUri, nameof(itemVersionUri));
            Assert.ArgumentNotNull(parentItemUri, nameof(parentItemUri));

            var itemAdded = ItemAdded;
            if (itemAdded != null)
            {
                itemAdded(sender, itemVersionUri, parentItemUri);
            }
        }

        public static void RaiseItemDeleted([NotNull] object sender, [NotNull] ItemUri itemUri)
        {
            Assert.ArgumentNotNull(sender, nameof(sender));
            Assert.ArgumentNotNull(itemUri, nameof(itemUri));

            var itemDeleted = ItemDeleted;
            if (itemDeleted != null)
            {
                itemDeleted(sender, itemUri);
            }
        }

        public static void RaiseItemDuplicated([NotNull] object sender, [NotNull] ItemUri newItemUri, [NotNull] ItemUri sourceItemUri)
        {
            Assert.ArgumentNotNull(sender, nameof(sender));
            Assert.ArgumentNotNull(newItemUri, nameof(newItemUri));
            Assert.ArgumentNotNull(sourceItemUri, nameof(sourceItemUri));

            var itemDuplicated = ItemDuplicated;
            if (itemDuplicated != null)
            {
                itemDuplicated(sender, newItemUri, sourceItemUri);
            }
        }

        public static void RaiseItemlXmlPasted([NotNull] object sender, [NotNull] ItemUri parentItemUri, [NotNull] string xml, bool changeIds)
        {
            Assert.ArgumentNotNull(sender, nameof(sender));
            Assert.ArgumentNotNull(parentItemUri, nameof(parentItemUri));
            Assert.ArgumentNotNull(xml, nameof(xml));

            var itemXmlPasted = ItemXmlPasted;
            if (itemXmlPasted != null)
            {
                itemXmlPasted(sender, parentItemUri, xml, changeIds);
            }
        }

        public static void RaiseItemModified([NotNull] object sender, [NotNull] ContentModel contentModel, bool isModified)
        {
            Assert.ArgumentNotNull(sender, nameof(sender));
            Assert.ArgumentNotNull(contentModel, nameof(contentModel));

            var itemModified = ItemModified;
            if (itemModified != null)
            {
                itemModified(sender, contentModel, isModified);
            }
        }

        public static void RaiseItemMoved([NotNull] object sender, [NotNull] ItemUri newParentUri, [NotNull] ItemVersionUri itemUri)
        {
            Assert.ArgumentNotNull(sender, nameof(sender));
            Assert.ArgumentNotNull(newParentUri, nameof(newParentUri));
            Assert.ArgumentNotNull(itemUri, nameof(itemUri));

            var itemMoved = ItemMoved;
            if (itemMoved != null)
            {
                itemMoved(sender, newParentUri, itemUri);
            }
        }

        public static void RaiseItemRenamed([NotNull] object sender, [NotNull] ItemUri itemUri, [NotNull] string newName)
        {
            Assert.ArgumentNotNull(sender, nameof(sender));
            Assert.ArgumentNotNull(itemUri, nameof(itemUri));
            Assert.ArgumentNotNull(newName, nameof(newName));

            var itemRenamed = ItemRenamed;
            if (itemRenamed != null)
            {
                itemRenamed(sender, itemUri, newName);
            }
        }

        public static void RaiseItemSerialized([NotNull] object sender, [NotNull] ItemUri itemUri, SerializationOperation serializationOperation)
        {
            Assert.ArgumentNotNull(sender, nameof(sender));
            Assert.ArgumentNotNull(itemUri, nameof(itemUri));

            var itemSerialized = ItemSerialized;
            if (itemSerialized != null)
            {
                itemSerialized(sender, itemUri, serializationOperation);
            }
        }

        public static void RaiseItemsSaved([NotNull] object sender, [NotNull] ContentModel contentModel)
        {
            Assert.ArgumentNotNull(sender, nameof(sender));
            Assert.ArgumentNotNull(contentModel, nameof(contentModel));

            var itemsSaved = ItemsSaved;
            if (itemsSaved != null)
            {
                itemsSaved(sender, contentModel);
            }
        }

        public static void RaiseItemTemplateChanged([NotNull] object sender, [NotNull] ItemUri itemUri, [NotNull] ItemUri newTemplateUri)
        {
            Assert.ArgumentNotNull(sender, nameof(sender));
            Assert.ArgumentNotNull(newTemplateUri, nameof(newTemplateUri));
            Assert.ArgumentNotNull(itemUri, nameof(itemUri));

            var itemTemplateChanged = ItemTemplateChanged;
            if (itemTemplateChanged != null)
            {
                itemTemplateChanged(sender, itemUri, newTemplateUri);
            }
        }

        public static void RaiseItemTreeViewDragOver([NotNull] object sender, [NotNull] ItemTreeViewItem item, [NotNull] DragEventArgs args)
        {
            Assert.ArgumentNotNull(sender, nameof(sender));
            Assert.ArgumentNotNull(item, nameof(item));
            Assert.ArgumentNotNull(args, nameof(args));

            var dragOver = ItemTreeViewDragOver;
            if (dragOver != null)
            {
                dragOver(sender, item, args);
            }
        }

        public static void RaiseItemTreeViewDrop([NotNull] object sender, [NotNull] ItemTreeViewItem item, [NotNull] DragEventArgs args)
        {
            Assert.ArgumentNotNull(sender, nameof(sender));
            Assert.ArgumentNotNull(item, nameof(item));
            Assert.ArgumentNotNull(args, nameof(args));

            var drop = ItemTreeViewDrop;
            if (drop != null)
            {
                drop(sender, item, args);
            }
        }

        public static void RaiseMediaUploaded([NotNull] object sender, [NotNull] ItemHeader itemHeader)
        {
            Assert.ArgumentNotNull(sender, nameof(sender));
            Assert.ArgumentNotNull(itemHeader, nameof(itemHeader));

            var mediaUploaded = MediaUploaded;
            if (mediaUploaded != null)
            {
                mediaUploaded(sender, itemHeader);
            }
        }

        public static void RaisePublishing([NotNull] object sender, int mode, [NotNull] DatabaseName databaseName)
        {
            Assert.ArgumentNotNull(sender, nameof(sender));
            Assert.ArgumentNotNull(databaseName, nameof(databaseName));

            var publishing = Publishing;
            if (publishing != null)
            {
                publishing(sender, mode, databaseName);
            }
        }

        public static void RaisePublishingItem([NotNull] object sender, [NotNull] ItemUri itemUri, bool deep, bool compareRevisions)
        {
            Assert.ArgumentNotNull(sender, nameof(sender));
            Assert.ArgumentNotNull(itemUri, nameof(itemUri));

            var versionRemoved = PublishingItem;
            if (versionRemoved != null)
            {
                versionRemoved(sender, itemUri, deep, compareRevisions);
            }
        }

        public static void RaiseSettingChanged([NotNull] string path, [NotNull] string key)
        {
            var eventHandler = SettingChanged;
            if (eventHandler != null)
            {
                eventHandler(path, key);
            }
        }

        public static void RaiseSiteAdded([NotNull] object sender, [NotNull] Site site)
        {
            Assert.ArgumentNotNull(sender, nameof(sender));
            Assert.ArgumentNotNull(site, nameof(site));

            var siteAdded = SiteAdded;
            if (siteAdded != null)
            {
                siteAdded(sender, site);
            }
        }

        public static void RaiseSiteChanged([NotNull] object sender, [NotNull] Site site)
        {
            Assert.ArgumentNotNull(sender, nameof(sender));
            Assert.ArgumentNotNull(site, nameof(site));

            var siteChanged = SiteChanged;
            if (siteChanged != null)
            {
                siteChanged(sender, site);
            }
        }

        public static void RaiseSiteDeleted([NotNull] object sender, [NotNull] Site site)
        {
            Assert.ArgumentNotNull(sender, nameof(sender));
            Assert.ArgumentNotNull(site, nameof(site));

            var siteDeleted = SiteDeleted;
            if (siteDeleted != null)
            {
                siteDeleted(sender, site);
            }
        }

        public static void RaiseSiteModified([NotNull] object sender, [NotNull] Site site, [NotNull] string oldHostName, [NotNull] string oldUserName)
        {
            Assert.ArgumentNotNull(sender, nameof(sender));
            Assert.ArgumentNotNull(site, nameof(site));
            Assert.ArgumentNotNull(oldHostName, nameof(oldHostName));
            Assert.ArgumentNotNull(oldUserName, nameof(oldUserName));

            var siteModified = SiteModified;
            if (siteModified != null)
            {
                siteModified(sender, site, oldHostName, oldUserName);
            }
        }

        public static void RaiseTemplateSaved([NotNull] object sender, [NotNull] ItemUri templateUri, [NotNull] string templateName)
        {
            Assert.ArgumentNotNull(sender, nameof(sender));
            Assert.ArgumentNotNull(templateUri, nameof(templateUri));
            Assert.ArgumentNotNull(templateName, nameof(templateName));

            var templateSaved = TemplateSaved;
            if (templateSaved != null)
            {
                templateSaved(sender, templateUri, templateName);
            }
        }

        public static void RaiseUnloaded([NotNull] object sender, [NotNull] object window)
        {
            Assert.ArgumentNotNull(sender, nameof(sender));
            Assert.ArgumentNotNull(window, nameof(window));

            var unloaded = Unloaded;
            if (unloaded != null)
            {
                unloaded(sender, window);
            }
        }

        public static void RaiseVersionAdded([NotNull] object sender, [NotNull] ItemVersionUri newItemVersionUri)
        {
            Assert.ArgumentNotNull(sender, nameof(sender));
            Assert.ArgumentNotNull(newItemVersionUri, nameof(newItemVersionUri));

            var versionAdded = VersionAdded;
            if (versionAdded != null)
            {
                versionAdded(sender, newItemVersionUri);
            }
        }

        public static void RaiseVersionRemoved([NotNull] object sender, [NotNull] ItemVersionUri deletedVersionUri, [NotNull] ItemVersionUri newVersionUri)
        {
            Assert.ArgumentNotNull(sender, nameof(sender));
            Assert.ArgumentNotNull(deletedVersionUri, nameof(deletedVersionUri));
            Assert.ArgumentNotNull(newVersionUri, nameof(newVersionUri));

            var versionRemoved = VersionRemoved;
            if (versionRemoved != null)
            {
                versionRemoved(sender, deletedVersionUri, newVersionUri);
            }
        }

        public static void RegisterFieldEvents([NotNull] DependencyObject ownerWindow, [CanBeNull] FieldChangedEventHandler changed = null)
        {
            Assert.ArgumentNotNull(ownerWindow, nameof(ownerWindow));

            if (changed != null)
            {
                FieldChanged += changed;
            }

            UnloadedEventHandler unloaded = null;
            unloaded = delegate(object sender, object o)
            {
                if (!ownerWindow.IsContainedIn(o))
                {
                    return;
                }

                if (changed != null)
                {
                    FieldChanged -= changed;
                }

                Unloaded -= unloaded;
            };

            Unloaded += unloaded;
        }

        public static void RegisterItemEvents([NotNull] DependencyObject ownerWindow, [CanBeNull] ItemAddedEventHandler added = null, [CanBeNull] ItemDeletedEventHandler deleted = null, [CanBeNull] ItemDuplicatedEventHandler duplicated = null, [CanBeNull] ItemModifiedEventHandler modified = null, [CanBeNull] ItemMovedEventHandler moved = null, [CanBeNull] ItemRenamedEventHandler renamed = null, [CanBeNull] ItemsSavedEventHandler saved = null, [CanBeNull] ItemXmlPastedEventHandler xmlPasted = null, [CanBeNull] ItemSerializedEventHandler serialized = null)
        {
            Assert.ArgumentNotNull(ownerWindow, nameof(ownerWindow));

            if (added != null)
            {
                ItemAdded += added;
            }

            if (deleted != null)
            {
                ItemDeleted += deleted;
            }

            if (duplicated != null)
            {
                ItemDuplicated += duplicated;
            }

            if (modified != null)
            {
                ItemModified += modified;
            }

            if (moved != null)
            {
                ItemMoved += moved;
            }

            if (renamed != null)
            {
                ItemRenamed += renamed;
            }

            if (saved != null)
            {
                ItemsSaved += saved;
            }

            if (xmlPasted != null)
            {
                ItemXmlPasted += xmlPasted;
            }

            if (serialized != null)
            {
                ItemSerialized += serialized;
            }

            UnloadedEventHandler unloaded = null;
            unloaded = delegate(object sender, object o)
            {
                if (!ownerWindow.IsContainedIn(o))
                {
                    return;
                }

                if (added != null)
                {
                    ItemAdded -= added;
                }

                if (deleted != null)
                {
                    ItemDeleted -= deleted;
                }

                if (duplicated != null)
                {
                    ItemDuplicated -= duplicated;
                }

                if (modified != null)
                {
                    ItemModified -= modified;
                }

                if (moved != null)
                {
                    ItemMoved -= moved;
                }

                if (renamed != null)
                {
                    ItemRenamed -= renamed;
                }

                if (saved != null)
                {
                    ItemsSaved -= saved;
                }

                if (xmlPasted != null)
                {
                    ItemXmlPasted -= xmlPasted;
                }

                if (serialized != null)
                {
                    ItemSerialized -= serialized;
                }

                Unloaded -= unloaded;
            };

            Unloaded += unloaded;
        }

        public static void RegisterItemVersionEvents([NotNull] DependencyObject ownerWindow, [CanBeNull] VersionAddedEventHandler versionAdded = null, [CanBeNull] VersionRemovedEventHandler versionRemoved = null)
        {
            Assert.ArgumentNotNull(ownerWindow, nameof(ownerWindow));

            if (versionAdded != null)
            {
                VersionAdded += versionAdded;
            }

            if (versionRemoved != null)
            {
                VersionRemoved += versionRemoved;
            }

            UnloadedEventHandler unloaded = null;
            unloaded = delegate(object sender, object o)
            {
                if (!ownerWindow.IsContainedIn(o))
                {
                    return;
                }

                if (versionAdded != null)
                {
                    VersionAdded -= versionAdded;
                }

                if (versionRemoved != null)
                {
                    VersionRemoved -= versionRemoved;
                }

                Unloaded -= unloaded;
            };

            Unloaded += unloaded;
        }

        public static void RegisterMediaEvents([NotNull] DependencyObject ownerWindow, [CanBeNull] MediaUploadedEventHandler uploaded = null)
        {
            Assert.ArgumentNotNull(ownerWindow, nameof(ownerWindow));

            if (uploaded != null)
            {
                MediaUploaded += uploaded;
            }

            UnloadedEventHandler unloaded = null;
            unloaded = delegate(object sender, object o)
            {
                if (!ownerWindow.IsContainedIn(o))
                {
                    return;
                }

                if (uploaded != null)
                {
                    MediaUploaded -= uploaded;
                }

                Unloaded -= unloaded;
            };

            Unloaded += unloaded;
        }

        public static void RegisterPublishingEvents([NotNull] DependencyObject ownerWindow, [CanBeNull] PublishingEventHandler publishing = null, [CanBeNull] PublishingItemEventHandler publishingItem = null)
        {
            Assert.ArgumentNotNull(ownerWindow, nameof(ownerWindow));

            if (publishing != null)
            {
                Publishing += publishing;
            }

            if (publishingItem != null)
            {
                PublishingItem += publishingItem;
            }

            UnloadedEventHandler unloaded = null;
            unloaded = delegate(object sender, object o)
            {
                if (!ownerWindow.IsContainedIn(o))
                {
                    return;
                }

                if (publishing != null)
                {
                    Publishing -= publishing;
                }

                if (publishingItem != null)
                {
                    PublishingItem -= publishingItem;
                }

                Unloaded -= unloaded;
            };

            Unloaded += unloaded;
        }

        public static void RegisterSiteEvents([NotNull] DependencyObject ownerWindow, [CanBeNull] SiteEventHandler added = null, [CanBeNull] SiteEventHandler deleted = null, [CanBeNull] SiteEventHandler changed = null, [CanBeNull] DatabaseEventHandler activeDatabaseChanged = null, [CanBeNull] DataServiceStatusChangedEventHandler dataServiceStatusChanged = null)
        {
            Assert.ArgumentNotNull(ownerWindow, nameof(ownerWindow));

            if (added != null)
            {
                SiteAdded += added;
            }

            if (deleted != null)
            {
                SiteDeleted += deleted;
            }

            if (changed != null)
            {
                SiteChanged += changed;
            }

            if (dataServiceStatusChanged != null)
            {
                DataServiceStatusChanged += dataServiceStatusChanged;
            }

            if (activeDatabaseChanged != null)
            {
                ActiveDatabaseChanged += activeDatabaseChanged;
            }

            UnloadedEventHandler unloaded = null;
            unloaded = delegate(object sender, object o)
            {
                if (!ownerWindow.IsContainedIn(o))
                {
                    return;
                }

                if (added != null)
                {
                    SiteAdded -= added;
                }

                if (deleted != null)
                {
                    SiteDeleted -= deleted;
                }

                if (changed != null)
                {
                    SiteChanged -= changed;
                }

                if (activeDatabaseChanged != null)
                {
                    ActiveDatabaseChanged -= activeDatabaseChanged;
                }

                if (dataServiceStatusChanged != null)
                {
                    DataServiceStatusChanged -= dataServiceStatusChanged;
                }

                Unloaded -= unloaded;
            };

            Unloaded += unloaded;
        }

        public static void RegisterSystemEvent([NotNull] DependencyObject ownerWindow, [CanBeNull] SettingChangedEventHandler settingChanged = null)
        {
            Assert.ArgumentNotNull(ownerWindow, nameof(ownerWindow));

            if (settingChanged != null)
            {
                SettingChanged += settingChanged;
            }

            UnloadedEventHandler unloaded = null;
            unloaded = delegate(object sender, object o)
            {
                if (!ownerWindow.IsContainedIn(o))
                {
                    return;
                }

                if (settingChanged != null)
                {
                    SettingChanged -= settingChanged;
                }

                Unloaded -= unloaded;
            };

            Unloaded += unloaded;
        }

        public static void RegisterTemplateEvents([NotNull] DependencyObject ownerWindow, [CanBeNull] ItemTemplateChangedEventHandler changed = null, [CanBeNull] TemplateSavedEventHandler saved = null)
        {
            Assert.ArgumentNotNull(ownerWindow, nameof(ownerWindow));

            if (changed != null)
            {
                ItemTemplateChanged += changed;
            }

            if (saved != null)
            {
                TemplateSaved += saved;
            }

            UnloadedEventHandler unloaded = null;
            unloaded = delegate(object sender, object o)
            {
                if (!ownerWindow.IsContainedIn(o))
                {
                    return;
                }

                if (changed != null)
                {
                    ItemTemplateChanged -= changed;
                }

                if (saved != null)
                {
                    TemplateSaved -= saved;
                }

                Unloaded -= unloaded;
            };

            Unloaded += unloaded;
        }

        public static void RegisterTreeViewEvents([NotNull] DependencyObject ownerWindow, [CanBeNull] ItemTreeViewDragOverEventHandler dragOver = null, [CanBeNull] ItemTreeViewDropEventHandler drop = null)
        {
            Assert.ArgumentNotNull(ownerWindow, nameof(ownerWindow));

            if (dragOver != null)
            {
                ItemTreeViewDragOver += dragOver;
            }

            if (drop != null)
            {
                ItemTreeViewDrop += drop;
            }

            UnloadedEventHandler unloaded = null;
            unloaded = delegate(object sender, object o)
            {
                if (!ownerWindow.IsContainedIn(o))
                {
                    return;
                }

                if (dragOver != null)
                {
                    ItemTreeViewDragOver -= dragOver;
                }

                if (drop != null)
                {
                    ItemTreeViewDrop -= drop;
                }

                Unloaded -= unloaded;
            };

            Unloaded += unloaded;
        }

        public static event SettingChangedEventHandler SettingChanged;

        public static event SiteEventHandler SiteAdded;

        public static event SiteEventHandler SiteChanged;

        public static event SiteEventHandler SiteDeleted;

        public static event SiteModifiedEventHandler SiteModified;

        public static event TemplateSavedEventHandler TemplateSaved;

        public static event UnloadedEventHandler Unloaded;

        public static event VersionAddedEventHandler VersionAdded;

        public static event VersionRemovedEventHandler VersionRemoved;
    }
}
