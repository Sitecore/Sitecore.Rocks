// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CompletionSource.cs" company="Sitecore A/S">
//   Copyright (C) 2010 by Sitecore A/S
// </copyright>
// <summary>
//   The completion source.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Sitecore.VisualStudio.UI.QueryAnalyzer.IntelliSense
{
  using System;
  using System.Collections.Generic;
  using Microsoft.VisualStudio.Language.Intellisense;
  using Microsoft.VisualStudio.Text;
  using Sitecore.VisualStudio.Annotations;
  using Sitecore.VisualStudio.Diagnostics;

  /// <summary>The completion source.</summary>
  public class CompletionSource : ICompletionSource
  {
    #region Constants and Fields

    /// <summary>The source provider.</summary>
    private readonly CompletionSourceProvider sourceProvider;

    /// <summary>The text buffer.</summary>
    private readonly ITextBuffer textBuffer;

    /// <summary>The comp list.</summary>
    private List<Completion> completions;

    /// <summary>The is disposed.</summary>
    private bool isDisposed;

    #endregion

    #region Constructors and Destructors

    /// <summary>Initializes a new instance of the <see cref="CompletionSource"/> class.</summary>
    /// <param name="sourceProvider">The source provider.</param>
    /// <param name="textBuffer">The text buffer.</param>
    public CompletionSource([NotNull] CompletionSourceProvider sourceProvider, [NotNull] ITextBuffer textBuffer)
    {
      Assert.ArgumentNotNull(sourceProvider, "sourceProvider");
      Assert.ArgumentNotNull(textBuffer, "textBuffer");

      this.sourceProvider = sourceProvider;
      this.textBuffer = textBuffer;
    }

    #endregion

    #region Implemented Interfaces

    #region ICompletionSource

    /// <summary>Determines which <see cref="T:Microsoft.VisualStudio.Language.Intellisense.CompletionSet"/>s should be part of the specified <see cref="T:Microsoft.VisualStudio.Language.Intellisense.ICompletionSession"/>.</summary>
    /// <param name="session">The session for which completions are to be computed.</param>
    /// <param name="completionSets">The set of the completionSets to be added to the session.</param>
    void ICompletionSource.AugmentCompletionSession([NotNull] ICompletionSession session, [NotNull] IList<CompletionSet> completionSets)
    {
      Debug.ArgumentNotNull(session, "session");
      Debug.ArgumentNotNull(completionSets, "completionSets");

      var words = new List<string>
      {
        @"select",
        @"where"
      };

      // select @Text, @Title where /sitecore//content/Home;
      this.completions = new List<Completion>();

      foreach (var word in words)
      {
        var completion = new Completion(word, word, word, null, null);

        this.completions.Add(completion);
      }

      var completionSet = new CompletionSet(
        @"Sitecore.QueryAnalyzer", 
        @"Query Analyzer", 
        this.FindTokenSpanAtPosition(session), 
        this.completions, 
        null);

      completionSets.Add(completionSet);
    }

    #endregion

    #region IDisposable

    /// <summary>The dispose.</summary>
    public void Dispose()
    {
      if (!this.isDisposed)
      {
        GC.SuppressFinalize(this);
        this.isDisposed = true;
      }
    }

    #endregion

    #endregion

    #region Methods

    /// <summary>Finds the token span at position.</summary>
    /// <param name="session">The session.</param>
    /// <returns>Returns the token span at position.</returns>
    [CanBeNull]
    private ITrackingSpan FindTokenSpanAtPosition([NotNull] ICompletionSession session)
    {
      Debug.ArgumentNotNull(session, "session");

      var currentPoint = session.TextView.Caret.Position.BufferPosition - 1;

      var navigator = this.sourceProvider.NavigatorService.GetTextStructureNavigator(this.textBuffer);

      var extent = navigator.GetExtentOfWord(currentPoint);

      return currentPoint.Snapshot.CreateTrackingSpan(extent.Span, SpanTrackingMode.EdgeInclusive);
    }

    #endregion
  }
}