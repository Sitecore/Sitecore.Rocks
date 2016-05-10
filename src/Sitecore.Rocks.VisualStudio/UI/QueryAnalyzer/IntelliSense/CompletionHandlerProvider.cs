// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CompletionHandlerProvider.cs" company="Sitecore A/S">
//   Copyright (C) 2010 by Sitecore A/S
// </copyright>
// <summary>
//   The completion handler provider.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Sitecore.VisualStudio.UI.QueryAnalyzer.IntelliSense
{
  using System;
  using System.ComponentModel.Composition;
  using Microsoft.VisualStudio.Editor;
  using Microsoft.VisualStudio.Language.Intellisense;
  using Microsoft.VisualStudio.Shell;
  using Microsoft.VisualStudio.Text.Editor;
  using Microsoft.VisualStudio.TextManager.Interop;
  using Microsoft.VisualStudio.Utilities;
  using Sitecore.VisualStudio.Annotations;
  using Sitecore.VisualStudio.Diagnostics;

  /// <summary>The completion handler provider.</summary>
  [Export(typeof(IVsTextViewCreationListener))]
  [Name(@"Sitecore.QueryAnalyzer.Token Completion Handler")]
  [ContentType(@"Sitecore.QueryAnalyzer")]
  [TextViewRole(PredefinedTextViewRoles.Editable)]
  [UsedImplicitly]
  public class CompletionHandlerProvider : IVsTextViewCreationListener
  {
    #region Constants and Fields

    /// <summary>The adapter service.</summary>
    [Import]
    public IVsEditorAdaptersFactoryService AdapterService;

    #endregion

    #region Properties

    /// <summary>Gets or sets CompletionBroker.</summary>
    [Import]
    public ICompletionBroker CompletionBroker { get; set; }

    /// <summary>Gets or sets ServiceProvider.</summary>
    [Import]
    public SVsServiceProvider ServiceProvider { get; set; }

    #endregion

    #region Implemented Interfaces

    #region IVsTextViewCreationListener

    /// <summary>Called when a <see cref="T:Microsoft.VisualStudio.TextManager.Interop.IVsTextView"/> adapter has been created and initialized.</summary>
    /// <param name="textViewAdapter">The newly created and initialized text view
    /// adapter.</param>
    public void VsTextViewCreated([NotNull] IVsTextView textViewAdapter)
    {
      Assert.ArgumentNotNull(textViewAdapter, "textViewAdapter");

      var textView = this.AdapterService.GetWpfTextView(textViewAdapter);
      if (textView == null)
      {
        return;
      }

      Func<CompletionCommandHandler> createCommandHandler = () => new CompletionCommandHandler(textViewAdapter, textView, this);

      textView.Properties.GetOrCreateSingletonProperty(createCommandHandler);
    }

    #endregion

    #endregion
  }
}