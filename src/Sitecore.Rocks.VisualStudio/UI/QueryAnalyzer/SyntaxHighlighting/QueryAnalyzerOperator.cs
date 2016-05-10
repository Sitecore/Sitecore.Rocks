// --------------------------------------------------------------------------------------------------------------------
// <copyright file="QueryAnalyzerOperator.cs" company="Sitecore A/S">
//   Copyright (C) 2010 by Sitecore A/S
// </copyright>
// <summary>
//   This class contains the format for the hyperlink.  If you would like the text to retain its forground color (as it does in VS)
//   then just remove the line that specifies the Foreground brush.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Sitecore.VisualStudio.UI.QueryAnalyzer.SyntaxHighlighting
{
  using System.ComponentModel.Composition;
  using System.Windows.Media;
  using Microsoft.VisualStudio.Text.Classification;
  using Microsoft.VisualStudio.Utilities;
  using Sitecore.VisualStudio.Annotations;

  /// <summary>This class contains the format for the hyperlink.  If you would like the text to retain its forground color (as it does in VS)
  /// then just remove the line that specifies the Foreground brush.</summary>
  [Export(typeof(EditorFormatDefinition))]
  [ClassificationType(ClassificationTypeNames = @"Sitecore.QueryAnalyzer.Operator")]
  [Name(@"Sitecore.QueryAnalyzer.Operator")]
  [UserVisible(true)]
  [Order(After = Priority.Default, Before = Priority.Default)]
  [UsedImplicitly]
  internal sealed class QueryAnalyzerOperator : ClassificationFormatDefinition
  {
    #region Constructors and Destructors

    /// <summary>Initializes a new instance of the <see cref="QueryAnalyzerOperator"/> class.</summary>
    public QueryAnalyzerOperator()
    {
      this.ForegroundColor = Colors.Purple;
      this.DisplayName = @"Sitecore Query Analyzer Operator";
    }

    #endregion
  }
}