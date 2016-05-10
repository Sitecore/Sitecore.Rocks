# Sitecore Rocks

Sitecore Rocks makes Sitecore developers happy!

Sitecore Rocks integrates directly with Microsoft Visual Studio 2013 and 2015 
and gives developers a fast, streamlined development experience by letting them use the tools 
that they are familiar with. 

![Sitecore Rocks](https://i1.visualstudiogallery.msdn.s-msft.com/44a26c88-83a7-46f6-903c-5c59bcd3d35b/image/file/148813/1/readme1.png)

## Features

* Sitecore Explorer: View the content tree for multiple websites and multiple databases. Delete, edit, or drag and drop multiple items simultaneously.
* Item Editor: Edit the content of Sitecore items directly inside Microsoft Visual Studio - no need to open a web browser. The Item Editor supports multi-edit so that multiple items can be updated in a single operation. 
* Sitecore Query Analyzer: scripting CRUD.
* Layout Designer: Use drag and drop to setup presentation for multiple devices.
* Template Designer: Quick and easy configuration of data templates.
* Log Viewer: Keep track of what is happening on a Sitecore website with a running log.
* Job Viewer: See which jobs are running in the background on a Sitecore website.
* Search: Fully integrated Sitecore search.
* Media Library: Query-based media library. Upload new media using drag and drop.
* Link Viewer: Quickly locate referrers and references.
* Sitecore Project Management: Connect a Visual Studio project to a Sitecore website and create Sitecore items from aspx, ascx and xsl files with a few clicks. Track file operations (duplicate, copy, move, rename) and update the Sitecore items automatically.
* Site Validation: Validate your site against more than 70 rules.

Sitecore Rocks is fully extendable using plug-ins that allows you to add new buttons, item editor skins, field types, pipelines and more.

## Downloading
* [Sitecore Rocks Visual Studio](https://visualstudiogallery.msdn.microsoft.com/44a26c88-83a7-46f6-903c-5c59bcd3d35b) (plugin for Visual Studio)

## Videos
* [YouTube Playlist](https://www.youtube.com/view_play_list?p=2B8CA35C742803E4)
* [Videos by developer Jakob Christensen](https://www.youtube.com/playlist?list=PLWIbrolNZWfk2WZcNefluTlW0QQmrMj1q)
* [Sitecore Rocks series by Sen Gupta](https://www.youtube.com/watch?v=O4R7AbwotS0&list=PLFNs4m6IdelTc277XFzwxh2AaXC4bzyrg)

## External blog posts
* [How and why to use Sitecore Rocks by John West](http://www.sitecore.net/Community/Technical-Blogs/John-West-Sitecore-Blog/Posts/2011/07/Sitecore-Differentiating-Factors-Blog-Series-Sitecore-Rocks.aspx)
* [28 Days of Sitecore Rocks by Trevor Campbell](http://www.sitecore.net/Community/Technical-Blogs/Trevor-Campbell.aspx)

## Tips and tricks
* [Tips](docs/Tips/Tips.md)

## Layout Designer
* [Rendering Chunks](docs/Layouts/RenderingChunks.md)
* [Editing Versioned Layouts](docs/Layouts/VersionedLayouts.md)

## Query Analyzer
* [Samples](docs/QueryAnalyzer/QueryAnalyzerSamples.md)
* [Sitecore Rocks' Query Analyzer handy queries (www.newguid.net)](http://www.newguid.net/sitecore/2012/sitecore-rocks-query-analyzer-handy-queries/)
* [Sitecore Rocks' Query Analyzer handy queries - part 2 (www.newguid.net)](http://www.newguid.net/uncategorized/2012/sitecore-rocks-query-analyzer-handy-queries-part-2/)

## Publishing
* [Advanced Publishing](docs/Publishing/AdvancedPublishing.md)

## Media
* [Uploading media](docs/Media/UploadingMedia.md)

## Tools
* [Folder Synchronization](docs/Tools/FolderSynchronization.md)
* [Run Validations on build or in a CI environment](docs/Tools/ConfigureBuildTask.md)

## Sitecore Rocks Plugins
* [Web based plugins](docs/Plugins/WebBasedPlugins.md)
* [Creating Visual Studio projects](docs/Plugins/CreatingVisualStudioProjects.md)
* [Debugging a Visual Studio project](docs/Plugins/DebuggingVisualStudioProject.md)
* [Plugin Architecture overview](docs/Plugins/PluginArchitecture.md)
* [Commands](docs/Plugins/Commands.md)
* [Using Command Contexts](docs/Plugins/CommandContexts.md)
* [Pipelines](docs/Plugins/Pipelines.md)
* [Field controls in the Item Editor](docs/Plugins/FieldControls.md)
* [Subscribing to Events](docs/Plugins/SubscribingToEvents.md)
* [File/Item handlers](docs/Plugins/FileItemHandlers.md)

# Sitecore Rocks version 2

In order to make Rocks open source, we had to make some significant changes to get it past our legal department.
The main showstopper was the use of the ActiPro commercial WPF components as ActiPro does not have a suitable open-source
license. So we had to replace the components with free and open source alternatives. Some of these new components 
look different, behave differently, or in rare cases does not provide similar functionality. For the SyntaxEditor 
we chose to replace it with a standard WPF TextBox which has no syntax highlighting or advanced editing features 
(we may introduce AvalonEdit from SharpDevelop at a later point). 

Since we had to make significant changes, we took the oppotunity to remove some of the bloat from Sitecore Rocks - and
some features have been entirely removed. The reasons for this range from poor and unstable implementation, 
experimental features, features that are never used, and functionality that is better supplied by other tools (like
TDS and SIM).

The following features have been remove:

* Content Editor Auto Fill
* Content Tree Quick View
* Start PowerShell command
* Sitecore.PowerShell is no longer included
* Sitecore.NuGet is no longer included
* ReSharper integration is no longer included
* Relink Links in Subitems command
* Edit Layout as File command
* Debug and Trace window
* Visual Studio Editor Link Classifier
* Task Lists.

In addition to the removed features, we also removed Sitecore Rocks Windows (the standard alone version, that did not
require Visual Studio). It relied heavily on the ActiPro components, and very few people were using it. Since 
Visual Studio Community is now free and supports extensions, Sitecore Rocks Windows is not really relevant.

We have deprecated support for Visual Studio 2010 and 2012. We did not remove any code - only a couple of flags in the
.vsix manifest - so it should still work.

Now for a controversial decision: Sitecore Rocks officially only supports Sitecore CMS 8.x and later. Unofficially 
CMS 5.x, 6.x and 7.x still works. Maintaining backwards compatibility is difficult and in some cases (we are 
looking at you, Lucene), we had to jump through too many hoops to make things work. In pratice, don't worry too 
much about this - this is just Sitecore pushing you towards CMS 8.x. Things still work the way they used to - 
with the exception of a couple of features.

Having Sitecore Rocks on GitHub, allows us to have a well-known process for reporting bugs and having discussions.

## Regarding the code
Sitecore Rocks was developed as a side project of a very small team over the course of 5 years. Please be aware, that
the coding standards in Sitecore Rocks does not reflect the general coding standards in Sitecore. The goal was never 
to produce a masterpiece of code, but to deliver - fast!
