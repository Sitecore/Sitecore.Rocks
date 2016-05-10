@mkdir Tools\Sitecore.Rocks.RequestCodeGenerator\Temp
xcopy Components\Sitecore.Kernel.dll Tools\Sitecore.Rocks.RequestCodeGenerator\Temp /e /d /i /y
xcopy Components\Sitecore.Client.dll Tools\Sitecore.Rocks.RequestCodeGenerator\Temp /e /d /i /y
xcopy Features\Sitecore.Rocks.Core\Sitecore.Rocks.Server.Core\bin\debug\Sitecore.Rocks.Server.Core.dll Tools\Sitecore.Rocks.RequestCodeGenerator\Temp /e /d /i /y
xcopy Features\Sitecore.Rocks.Core\Sitecore.Rocks.Server.Cms7\bin\debug\Sitecore.Rocks.Server.Cms7.dll Tools\Sitecore.Rocks.RequestCodeGenerator\Temp /e /d /i /y
xcopy Sitecore.Rocks.Server\bin\debug\Sitecore.Rocks.Server.dll Tools\Sitecore.Rocks.RequestCodeGenerator\Temp /e /d /i /y
xcopy Features\Sitecore.Rocks.Forms\Sitecore.Rocks.Server.Forms\bin\debug\Sitecore.Rocks.Server.Forms.dll Tools\Sitecore.Rocks.RequestCodeGenerator\Temp /e /d /i /y
xcopy Features\Sitecore.Rocks.Speak\Sitecore.Rocks.Server.Speak\bin\debug\Sitecore.Rocks.Server.Speak.dll Tools\Sitecore.Rocks.RequestCodeGenerator\Temp /e /d /i /y
xcopy Features\Sitecore.Rocks.Validation\Sitecore.Rocks.Server.Validation\bin\debug\Sitecore.Rocks.Server.Validation.dll Tools\Sitecore.Rocks.RequestCodeGenerator\Temp /e /d /i /y

Tools\Sitecore.Rocks.RequestCodeGenerator\bin\debug\Sitecore.Rocks.RequestCodeGenerator.exe Tools\Sitecore.Rocks.RequestCodeGenerator\Temp\Sitecore.Rocks.Server.dll Sitecore.Rocks\Shell\Environment\ServerHost.Server.cs
Tools\Sitecore.Rocks.RequestCodeGenerator\bin\debug\Sitecore.Rocks.RequestCodeGenerator.exe Tools\Sitecore.Rocks.RequestCodeGenerator\Temp\Sitecore.Rocks.Server.Cms7.dll Sitecore.Rocks\Shell\Environment\ServerHost.Server.Cms7.cs /i
Tools\Sitecore.Rocks.RequestCodeGenerator\bin\debug\Sitecore.Rocks.RequestCodeGenerator.exe Tools\Sitecore.Rocks.RequestCodeGenerator\Temp\Sitecore.Rocks.Server.Forms.dll Sitecore.Rocks\Shell\Environment\ServerHost.Forms.cs
Tools\Sitecore.Rocks.RequestCodeGenerator\bin\debug\Sitecore.Rocks.RequestCodeGenerator.exe Tools\Sitecore.Rocks.RequestCodeGenerator\Temp\Sitecore.Rocks.Server.Speak.dll Sitecore.Rocks\Shell\Environment\ServerHost.Speak.cs
Tools\Sitecore.Rocks.RequestCodeGenerator\bin\debug\Sitecore.Rocks.RequestCodeGenerator.exe Tools\Sitecore.Rocks.RequestCodeGenerator\Temp\Sitecore.Rocks.Server.Validation.dll Sitecore.Rocks\Shell\Environment\ServerHost.Validation.cs

@pause