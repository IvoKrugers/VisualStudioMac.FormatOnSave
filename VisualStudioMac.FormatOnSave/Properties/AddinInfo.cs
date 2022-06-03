using System;
using Mono.Addins;
using Mono.Addins.Description;
using VisualStudioMac.FormatOnSave;

[assembly: Addin(
    "FormatOnSave",
    Namespace = "VisualStudioMac",
    Version = Constants.Version,
    Category = "IDE extensions"
)]

[assembly: AddinName("Format (⌘ + i) and Sort + Remove unused usings on Save")]
[assembly: AddinDescription("This extension auto formats (⌘ + i) and Sort + Remove unused usings on save.\n\nby Ivo Krugers")]
[assembly: AddinAuthor("Ivo Krugers")]
[assembly: AddinUrl("https://github.com/IvoKrugers")]

[assembly: AddinDependency("::MonoDevelop.Core", MonoDevelop.BuildInfo.Version)]
[assembly: AddinDependency("::MonoDevelop.Ide", MonoDevelop.BuildInfo.Version)]
