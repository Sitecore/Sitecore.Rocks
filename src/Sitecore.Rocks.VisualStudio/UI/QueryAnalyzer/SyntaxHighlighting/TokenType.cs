// --------------------------------------------------------------------------------------------------------------------
// <copyright file="TokenType.cs" company="Sitecore A/S">
//   Copyright (C) 2010 by Sitecore A/S
// </copyright>
// <summary>
//   The token type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Sitecore.VisualStudio.UI.QueryAnalyzer.SyntaxHighlighting
{
  /// <summary>The token type.</summary>
  internal enum TokenType
  {
    /// <summary>The keyword.</summary>
    Identifier, 

    /// <summary>The operator.</summary>
    Operator, 

    /// <summary>The unknown.</summary>
    Unknown, 

    /// <summary>The end.</summary>
    End, 

    /// <summary>The string.</summary>
    String, 

    /// <summary>The number.</summary>
    Number
  }
}