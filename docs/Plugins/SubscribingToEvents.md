# Subscribing to Events

Sitecore Rocks has a central notification service ```Sitecore.Notifications``` where it is possible to subscribe to various events like ```ItemsSaved```, ```ItemAdded```, ```ItemDeleted``` etc.

## WPF Events
In WPF events are normally subscribed to in the constructor or in the WPF ```Loaded``` event and unsubscribed in the ``` Unloaded``` event.

The WPF ```Loaded``` event is triggered when a control is inserted into the WPF Visual Tree and the ```Unloaded``` event is triggered when removed.

The window management system in Visual Studio makes frequent use of the WPF ```TabControl``` which removes a tab page from the Visual Tree when it becomes invisible - effectively triggering the ```Unloaded``` event - and when it becomes visible again, triggers the ```Loaded``` event.

## Subscribing to Events
Assuming that a control wants to remain subscribed even when invisible, the standard approach will not work.

Sitecore Rocks solves this by introducing the ```RegisterXXX``` methods on the ```Notification``` object.

```
Notifications.RegisterItemEvents(this, modified: this.ItemModified, renamed: this.ItemRenamed, deleted: this.ItemDeleted);
Notifications.RegisterTemplateEvents(this, saved: this.TemplateSaved);
```

The ```RegisterXXX``` subscribes to the ```Notifications.Unloaded``` event. When the ```Unloaded``` event is triggered, the method checks if the subscribed control in contained within the unloading control, and if so, unsubscribes the events.

There is no need to unregister since this is handled automatically.

Every pane in Sitecore Rocks raises the ```Unloaded``` event, which will unsubscribe any events within that pane.