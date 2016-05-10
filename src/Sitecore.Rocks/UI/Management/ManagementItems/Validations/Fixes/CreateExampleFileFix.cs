// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CreateExampleFileFix.cs" company="Sitecore A/S">
//   Copyright (C) by Sitecore A/S
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Sitecore.VisualStudio.UI.Management.ManagementItems.Validations.Fixes
{
  using System;
  using System.IO;
  using Sitecore.VisualStudio.Annotations;
  using Sitecore.VisualStudio.Diagnostics;
  using Sitecore.VisualStudio.Extensions.StringExtensions;

  /// <summary>
  /// Class CreateBddTestFileFix
  /// </summary>
  [Fix]
  public class CreateExampleFileFix : IFix
  {
    #region Public Methods

    /// <summary>Determines whether this instance can fix the specified validation.</summary>
    /// <param name="validationDescriptor">The validation.</param>
    /// <returns><c>true</c> if this instance can fix the specified validation; otherwise, <c>false</c>.</returns>
    public bool CanFix(ValidationDescriptor validationDescriptor)
    {
      Assert.ArgumentNotNull(validationDescriptor, "validationDescriptor");

      if (validationDescriptor.Name != "Control does not have an Example file")
      {
        return false;
      }

      if (string.IsNullOrEmpty(validationDescriptor.ItemUri.Site.WebRootPath))
      {
        return false;
      }

      var fileName = this.ParseFileName(validationDescriptor);

      return !string.IsNullOrEmpty(fileName);
    }

    /// <summary>Fixes the specified validation.</summary>
    /// <param name="validationDescriptor">The validation.</param>
    public void Fix(ValidationDescriptor validationDescriptor)
    {
      Assert.ArgumentNotNull(validationDescriptor, "validationDescriptor");

      var fileName = this.ParseFileName(validationDescriptor);
      var itemName = this.ParseItemName(validationDescriptor);

      fileName = Path.Combine(validationDescriptor.ItemUri.Site.WebRootPath, fileName);

      const string Text = @"@using Sitecore.Mvc.Presentation
@using Sitecore.Mvc
@using Sitecore.Web.UI.Controls.Common
@model RenderingModel

<h3>Apperance</h3>
<p>
  {0} is a good control, does what it is supposed to do and clean up after itself.
</p>
<p>
  @Html.Sitecore().Controls().{0}(""{0}1"", new {{ Text = ""Hello"" }})
</p>

<pre>
&#64;Html.Sitecore().Controls().{0}(""{0}1"", new {{ Text = ""Hello"" }})
</pre>
";

      var contents = string.Format(Text, itemName);

      var folder = Path.GetDirectoryName(fileName);
      Directory.CreateDirectory(folder);

      File.WriteAllText(fileName, contents);

      AppHost.Files.OpenFile(fileName);
    }

    #endregion

    #region Methods

    /// <summary>Parses the specified text.</summary>
    /// <param name="validationDescriptor">The validation.</param>
    /// <returns>Returns the string.</returns>
    [NotNull]
    private string ParseFileName([NotNull] ValidationDescriptor validationDescriptor)
    {
      var start = validationDescriptor.Solution.IndexOf(":", StringComparison.Ordinal);
      if (start < 0)
      {
        return string.Empty;
      }

      return validationDescriptor.Solution.Mid(start + 1);
    }

    /// <summary>Parses the specified text.</summary>
    /// <param name="validationDescriptor">The validation.</param>
    /// <returns>Returns the string.</returns>
    [NotNull]
    private string ParseItemName([NotNull] ValidationDescriptor validationDescriptor)
    {
      var start = validationDescriptor.ItemPath.LastIndexOf("/", StringComparison.Ordinal);
      if (start < 0)
      {
        return string.Empty;
      }

      return validationDescriptor.ItemPath.Mid(start + 1);
    }

    #endregion
  }
}