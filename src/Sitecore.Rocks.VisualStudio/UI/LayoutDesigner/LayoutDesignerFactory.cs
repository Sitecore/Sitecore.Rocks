// --------------------------------------------------------------------------------------------------------------------
// <copyright file="LayoutDesignerFactory.cs" company="Sitecore A/S">
//   Copyright (C) 2010 by Sitecore A/S
// </copyright>
// <summary>
//   Defines the content editor factory class.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Sitecore.VisualStudio.UI.LayoutDesigner
{
  using System.Runtime.InteropServices;
  using Microsoft.VisualStudio.Shell.Interop;
  using Sitecore.VisualStudio.Annotations;
  using Sitecore.VisualStudio.Diagnostics;
  using Sitecore.VisualStudio.Shell;
  using Sitecore.VisualStudio.Shell.Panes;

  /// <summary>Defines the content editor factory class.</summary>
  [Guid(GuidList.LayoutDesignerFactoryString)]
  public class LayoutDesignerFactory : EditorFactory<LayoutDesignerPane>
  {
    #region Public Methods

    /// <summary>Shows the pane.</summary>
    /// <param name="document">The document.</param>
    /// <returns>Returns the pane.</returns>
    [CanBeNull]
    public static LayoutDesignerPane CreateEditor([NotNull] string document)
    {
      Assert.ArgumentNotNull(document, "document");

      var windowFrame = CreateFrame(document, GuidList.LayoutDesignerFactoryString);
      if (windowFrame == null)
      {
        return null;
      }

      windowFrame.Show();

      object value;
      windowFrame.GetProperty((int)__VSFPROPID.VSFPROPID_DocView, out value);

      var pane = value as LayoutDesignerPane;
      if (pane != null)
      {
        pane.Frame = windowFrame;
      }

      return pane;
    }

    #endregion
  }
}