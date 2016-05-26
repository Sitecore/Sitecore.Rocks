# Get started

1. Install the [Visual Studio SDK](https://msdn.microsoft.com/en-us/library/bb166441.aspx?f=255&MSPPError=-2147217396).
2. Create NuGet package for Sitecore.Kernel, Sitecore.Client, Sitecore.Update and Lucene.Net using SIM.
   You should have these package in a NuGet repository:
   * SC.Lucene.Net
   * SC.Sitecore.Client
   * SC.Sitecore.Kernel
   * SC.Sitecore.Update
3. Build.

## Debugging
1. Make Sitecore.Rocks.VisualStudio the StartUp Project.
2. In the Sitecore.Rocks.VisualStudio project properties, set Debug | Start Action to Start External Program to "C:\Program Files (x86)\Microsoft Visual Studio 14.0\Common7\IDE\devenv.exe"
3. To use the Visual Studio experimental hive, set Command line arguments to "/rootsuffix Exp".
