// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CompletionSourceProvider.cs" company="Sitecore A/S">
//   Copyright (C) 2010 by Sitecore A/S
// </copyright>
// <summary>
//   Defines the  class.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Sitecore.VisualStudio.UI.QueryAnalyzer.IntelliSense
{
  using System.ComponentModel.Composition;
  using Microsoft.VisualStudio.Language.Intellisense;
  using Microsoft.VisualStudio.Text;
  using Microsoft.VisualStudio.Text.Operations;
  using Microsoft.VisualStudio.Utilities;
  using Sitecore.VisualStudio.Annotations;
  using Sitecore.VisualStudio.Diagnostics;

  /// <summary>Defines the <see cref="CompletionSourceProvider"/> class.</summary>
  /* [Export(typeof(ICompletionSourceProvider))]
  [ContentType(@"Sitecore.QueryAnalyzer")]
  [Name(@"Sitecore.QueryAnalyzer.TokenCompletion")]
  [UsedImplicitly] */
  public class CompletionSourceProvider : ICompletionSourceProvider
  {
    #region Properties

    /// <summary>
    /// Gets or sets the navigator service.
    /// </summary>
    /// <value>The navigator service.</value>
    [Import]
    public ITextStructureNavigatorSelectorService NavigatorService { get; set; }

    #endregion

    #region Implemented Interfaces

    #region ICompletionSourceProvider

    /// <summary>Creates a completion provider for the given context.</summary>
    /// <param name="textBuffer">The text buffer over which to create a provider.</param>
    /// <returns>A valid <see cref="T:Microsoft.VisualStudio.Language.Intellisense.ICompletionSource"/> instance, or null if none could be created.</returns>
    [NotNull]
    public ICompletionSource TryCreateCompletionSource([NotNull] ITextBuffer textBuffer)
    {
      Debug.ArgumentNotNull(textBuffer, "textBuffer");

      return new CompletionSource(this, textBuffer);
    }

    #endregion

    #endregion
  }
}