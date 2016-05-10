// --------------------------------------------------------------------------------------------------------------------
// <copyright file="QueryAnalyzerClassifier.cs" company="Sitecore A/S">
//   Copyright (C) 2010 by Sitecore A/S
// </copyright>
// <summary>
//   This class implements the classifier that marks the hyperlinks in the text buffer.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Sitecore.VisualStudio.UI.QueryAnalyzer.SyntaxHighlighting
{
  using System;
  using System.Collections.Generic;
  using Microsoft.VisualStudio.Text;
  using Microsoft.VisualStudio.Text.Classification;
  using Sitecore.VisualStudio.Annotations;
  using Sitecore.VisualStudio.Diagnostics;

  /// <summary>This class implements the classifier that marks the hyperlinks in the text buffer.</summary>
  public class QueryAnalyzerClassifier : IClassifier
  {
    #region Constructors and Destructors

    /// <summary>Initializes a new instance of the <see cref="QueryAnalyzerClassifier"/> class.</summary>
    /// <param name="registry">The registry.</param>
    public QueryAnalyzerClassifier([NotNull] IClassificationTypeRegistryService registry)
    {
      Debug.ArgumentNotNull(registry, "registry");

      this.Keyword = registry.GetClassificationType(@"Sitecore.QueryAnalyzer.Keyword");
      this.Operator = registry.GetClassificationType(@"Sitecore.QueryAnalyzer.Operator");
      this.String = registry.GetClassificationType(@"Sitecore.QueryAnalyzer.String");
      this.Number = registry.GetClassificationType(@"Sitecore.QueryAnalyzer.Number");
    }

    #endregion

    #region Events

#pragma warning disable 67
    /// <summary>The classification changed.</summary>
    public event EventHandler<ClassificationChangedEventArgs> ClassificationChanged;
#pragma warning restore 67

    #endregion

    #region Properties

    /// <summary>
    /// Gets or sets the keyword.
    /// </summary>
    /// <value>The keyword.</value>
    protected IClassificationType Keyword { get; set; }

    /// <summary>
    /// Gets or sets the number.
    /// </summary>
    /// <value>The number.</value>
    protected IClassificationType Number { get; set; }

    /// <summary>
    /// Gets or sets the operator.
    /// </summary>
    /// <value>The operator.</value>
    protected IClassificationType Operator { get; set; }

    /// <summary>
    /// Gets or sets the string.
    /// </summary>
    /// <value>The string.</value>
    protected IClassificationType String { get; set; }

    #endregion

    #region Implemented Interfaces

    #region IClassifier

    /// <summary>Find any urls in the tracking span and return the classification spans for those urls.</summary>
    /// <param name="trackingSpan">The tracking Span.</param>
    /// <returns>Returns the classification spans.</returns>
    /// <remarks>This code is designed to not allocate memory by reading in strings from the snapshot.</remarks>
    [NotNull]
    public IList<ClassificationSpan> GetClassificationSpans(SnapshotSpan trackingSpan)
    {
      var classificationSpans = new List<ClassificationSpan>();

      var snapshot = trackingSpan.Snapshot;
      int start = snapshot.GetLineFromPosition(trackingSpan.Start).Start;
      int end = snapshot.GetLineFromPosition(trackingSpan.End).End;

      this.Classify(snapshot, classificationSpans, start, end);

      return classificationSpans;
    }

    #endregion

    #endregion

    #region Methods

    /// <summary>Classifies the specified snapshot.</summary>
    /// <param name="snapshot">The snapshot.</param>
    /// <param name="classificationSpans">The classification spans.</param>
    /// <param name="start">The start.</param>
    /// <param name="end">The end.</param>
    private void Classify([NotNull] ITextSnapshot snapshot, [NotNull] List<ClassificationSpan> classificationSpans, int start, int end)
    {
      Debug.ArgumentNotNull(snapshot, "snapshot");
      Debug.ArgumentNotNull(classificationSpans, "classificationSpans");

      var tokenizer = new Tokenizer(snapshot, start, end);

      tokenizer.NextToken();
      while (tokenizer.TokenType != TokenType.End)
      {
        switch (tokenizer.TokenType)
        {
          case TokenType.Operator:
            classificationSpans.Add(new ClassificationSpan(new SnapshotSpan(snapshot, tokenizer.GetSpan()), this.Operator));
            break;

          case TokenType.String:
            classificationSpans.Add(new ClassificationSpan(new SnapshotSpan(snapshot, tokenizer.GetSpan()), this.String));
            break;

          case TokenType.Identifier:
            var token = tokenizer.Token;

            switch (token.ToLowerInvariant())
            {
              case "select":
              case "from":
              case "search":
              case "query":
              case "fastquery":
              case "delete":
              case "insert":
              case "set":
              case "values":
              case "into":
              case "help":
              case "update":
              case "create":
              case "template":
              case "section":
              case "use":
              case "shared":
              case "unversioned":
              case "publish":
              case "as":
              case "serialize":
                classificationSpans.Add(new ClassificationSpan(new SnapshotSpan(snapshot, tokenizer.GetSpan()), this.Keyword));
                break;
            }

            break;
        }

        tokenizer.NextToken();
      }
    }

    #endregion
  }
}