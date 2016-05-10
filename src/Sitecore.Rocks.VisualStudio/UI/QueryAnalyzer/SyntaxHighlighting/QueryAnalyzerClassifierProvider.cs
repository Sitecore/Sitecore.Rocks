// --------------------------------------------------------------------------------------------------------------------
// <copyright file="QueryAnalyzerClassifierProvider.cs" company="Sitecore A/S">
//   Copyright (C) 2010 by Sitecore A/S
// </copyright>
// <summary>
//   This class registers the classifier in the system.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Sitecore.VisualStudio.UI.QueryAnalyzer.SyntaxHighlighting
{
  using System.ComponentModel.Composition;
  using Microsoft.VisualStudio.Text;
  using Microsoft.VisualStudio.Text.Classification;
  using Microsoft.VisualStudio.Utilities;
  using Sitecore.VisualStudio.Annotations;
  using Sitecore.VisualStudio.Diagnostics;

  // ReSharper disable RedundantDefaultFieldInitializer

  /// <summary>This class registers the classifier in the system.</summary>
  [Export(typeof(IClassifierProvider))]
  [ContentType(@"Sitecore.QueryAnalyzer")]
  [UsedImplicitly]
  internal sealed class QueryAnalyzerClassifierProvider : IClassifierProvider
  {
    #region Constants and Fields

    /// <summary>The sitecore query analyzer content definition field.</summary>
    [Export]
    [Name(@"Sitecore.QueryAnalyzer")]
    [BaseDefinition(@"code")]
    internal static ContentTypeDefinition SitecoreQueryAnalyzerContentDefinition = null;

    /// <summary>The classification type registry service.</summary>
    [Import]
    [UsedImplicitly]
    internal IClassificationTypeRegistryService ClassificationTypeRegistryService = null;

    /// <summary>The url classification type definition.</summary>
    [Export]
    [Name(@"Sitecore.QueryAnalyzer.Keyword")]
    [UsedImplicitly]
    internal ClassificationTypeDefinition KeywordClassificationTypeDefinition = null;

    [Export]
    [Name(@"Sitecore.QueryAnalyzer.Operator")]
    [UsedImplicitly]
    internal ClassificationTypeDefinition OperatorClassificationTypeDefinition = null;

    [Export]
    [Name(@"Sitecore.QueryAnalyzer.String")]
    [UsedImplicitly]
    internal ClassificationTypeDefinition StringClassificationTypeDefinition = null;

    [Export]
    [Name(@"Sitecore.QueryAnalyzer.Number")]
    [UsedImplicitly]
    internal ClassificationTypeDefinition NumberClassificationTypeDefinition = null;

    #endregion

    #region Implemented Interfaces

    #region IClassifierProvider

    /// <summary>The get classifier.</summary>
    /// <param name="textBuffer">The text buffer.</param>
    /// <returns>A classifier for the text buffer, or null if the provider cannot do so in its current state.</returns>
    [NotNull]
    public IClassifier GetClassifier([NotNull] ITextBuffer textBuffer)
    {
      Debug.ArgumentNotNull(textBuffer, "textBuffer");

      QueryAnalyzerClassifier classifier;

      if (!textBuffer.Properties.TryGetProperty(typeof(QueryAnalyzerClassifier), out classifier))
      {
        classifier = new QueryAnalyzerClassifier(this.ClassificationTypeRegistryService);
        textBuffer.Properties.AddProperty(typeof(QueryAnalyzerClassifier), classifier);
      }

      return classifier;
    }

    #endregion

    #endregion
  }
}