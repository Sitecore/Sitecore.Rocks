// --------------------------------------------------------------------------------------------------------------------
// <copyright file="LayoutDesignerPane.cs" company="Sitecore A/S">
//   Copyright (C) 2010 by Sitecore A/S
// </copyright>
// <summary>
//   Defines the job viewer pane class.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Sitecore.VisualStudio.UI.LayoutDesigner
{
  using System.Runtime.InteropServices;
  using System.Windows;
  using Microsoft.VisualStudio;
  using Microsoft.VisualStudio.Shell.Interop;
  using Sitecore.VisualStudio.Annotations;
  using Sitecore.VisualStudio.Data;
  using Sitecore.VisualStudio.Diagnostics;
  using Sitecore.VisualStudio.Shell;
  using Sitecore.VisualStudio.Shell.Panes;

  /// <summary>Defines the job viewer pane class.</summary>
  [Guid(@"134bd20c-b692-4afe-9309-d9af984b6fb9")]
  public class LayoutDesignerPane : EditorPane<LayoutDesignerFactory, LayoutDesigner>, IEditorPane
  {
    #region Constants and Fields

    /// <summary>The layout builder.</summary>
    private LayoutDesigner layoutDesigner;

    #endregion

    #region Public Methods

    /// <summary>Initializes the specified item.</summary>
    /// <param name="item">The item.</param>
    public void Initialize([NotNull] Item item)
    {
      Assert.ArgumentNotNull(item, "item");

      this.layoutDesigner.Initialize(item);
    }

    #endregion

    #region Implemented Interfaces

    #region IPane

    /// <summary>Closes this instance.</summary>
    public void Close()
    {
      var windowFrame = (IVsWindowFrame)this.Frame;

      ErrorHandler.ThrowOnFailure(windowFrame.Hide());
    }

    /// <summary>
    /// Sets the modified.
    /// </summary>
    /// <param name="flag">if set to <c>true</c> [flag].</param>
    public void SetModifiedFlag(bool flag)
    {
      if (flag)
      {
        this.SetModified();
      }
    }

    #endregion

    #endregion

    #region Methods

    /// <summary>Initializes this instance.</summary>
    protected override void Initialize()
    {
      base.Initialize();

      this.layoutDesigner = (LayoutDesigner)this.Content;
      this.layoutDesigner.Pane = this;
      this.layoutDesigner.GotFocus += delegate { SitecorePackage.Instance.GotFocus(this); };
    }

    /// <summary>Saves the file.</summary>
    /// <param name="fileName">Name of the file.</param>
    protected override void SaveFile(string fileName)
    {
      Debug.ArgumentNotNull(fileName, "fileName");

      this.layoutDesigner.Save();
    }

    /// <summary>Called when close.</summary>
    protected override void OnClose()
    {
      Notifications.RaiseUnloaded(this, this.layoutDesigner);
      base.OnClose();
    }

    #endregion
  }
}