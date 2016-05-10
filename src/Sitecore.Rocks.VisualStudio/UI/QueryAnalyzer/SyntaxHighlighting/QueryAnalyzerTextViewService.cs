// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SitecoreLinkTextViewService.cs" company="Sitecore A/S">
//   Copyright (C) 2010 by Sitecore A/S
// </copyright>
// <summary>
//   This class listens for when text views are created, attaches to their mouse hover
//   event and then brings up a tooltip when someone hovers over a hyperlink.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Sitecore.VisualStudio.UI.QueryAnalyzer.SyntaxHighlighting
{
  using System.ComponentModel.Composition;
  using Microsoft.VisualStudio.Text.Editor;
  using Microsoft.VisualStudio.Utilities;
  using Sitecore.VisualStudio.Annotations;
  using Sitecore.VisualStudio.Diagnostics;

  /// <summary>This class listens for when text views are created, attaches to their mouse hover
  /// event and then brings up a tooltip when someone hovers over a hyperlink.</summary>
  [Export(typeof(IWpfTextViewCreationListener))]
  [ContentType(@"Sitecore.QueryAnalyzer")]
  [TextViewRole(PredefinedTextViewRoles.Document)]
  [UsedImplicitly]
  internal sealed class QueryAnalyzerTextViewService : IWpfTextViewCreationListener
  {
    #region Implemented Interfaces

    #region IWpfTextViewCreationListener

    /// <summary>Called when a text view having matchine roles is created over a text data model having a matching content type.</summary>
    /// <param name="textView">The newly created text view.</param>
    public void TextViewCreated([NotNull] IWpfTextView textView)
    {
      Debug.ArgumentNotNull(textView, "textView");
    }

    #endregion

    #endregion
  }
}