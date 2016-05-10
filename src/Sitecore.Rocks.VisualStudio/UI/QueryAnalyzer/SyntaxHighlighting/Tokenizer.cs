// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Tokenizer.cs" company="Sitecore A/S">
//   Copyright (C) 2010 by Sitecore A/S
// </copyright>
// <summary>
//   Defines the  class.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Sitecore.VisualStudio.UI.QueryAnalyzer.SyntaxHighlighting
{
  using System;
  using System.Text;
  using Microsoft.VisualStudio.Text;
  using Sitecore.VisualStudio.Annotations;
  using Sitecore.VisualStudio.Diagnostics;

  /// <summary>Defines the <see cref="Tokenizer"/> class.</summary>
  internal class Tokenizer
  {
    #region Constructors and Destructors

    /// <summary>Initializes a new instance of the <see cref="Tokenizer"/> class.</summary>
    /// <param name="snapshot">The snapshot.</param>
    /// <param name="start">The start.</param>
    /// <param name="end">The end.</param>
    public Tokenizer([NotNull] ITextSnapshot snapshot, int start, int end)
    {
      Debug.ArgumentNotNull(snapshot, "snapshot");

      this.Snapshot = snapshot;
      this.Start = start;
      this.End = end;

      this.Position = this.Start;
      this.TokenBuilder = new StringBuilder();
    }

    #endregion

    #region Properties

    /// <summary>
    /// Gets the token.
    /// </summary>
    [NotNull]
    public string Token
    {
      get
      {
        return this.TokenBuilder.ToString();
      }
    }

    /// <summary>
    /// Gets the type of the token.
    /// </summary>
    public TokenType TokenType { get; private set; }

    /// <summary>
    /// Gets or sets the end.
    /// </summary>
    /// <value>The end.</value>
    private int End { get; set; }

    /// <summary>
    /// Gets a value indicating whether this <see cref="Tokenizer"/> is EOF.
    /// </summary>
    private bool Eof
    {
      get
      {
        return this.Position >= this.End;
      }
    }

    /// <summary>Gets or sets Position.</summary>
    private int Position { get; set; }

    /// <summary>
    /// Gets or sets the snapshot.
    /// </summary>
    /// <value>The snapshot.</value>
    private ITextSnapshot Snapshot { get; set; }

    /// <summary>
    /// Gets or sets the start.
    /// </summary>
    /// <value>The start.</value>
    private int Start { get; set; }

    /// <summary>
    /// Gets or sets the start position.
    /// </summary>
    /// <value>The start position.</value>
    private int StartPosition { get; set; }

    /// <summary>
    /// Gets or sets the token.
    /// </summary>
    /// <value>The token.</value>
    private StringBuilder TokenBuilder { get; set; }

    #endregion

    #region Public Methods

    /// <summary>Gets the span.</summary>
    /// <returns>Returns the span.</returns>
    public Span GetSpan()
    {
      return Span.FromBounds(this.StartPosition, this.Position);
    }

    /// <summary>Nexts the token.</summary>
    [CanBeNull]
    public void NextToken()
    {
      this.TokenBuilder.Clear();
      this.TokenType = TokenType.Unknown;

      this.SkipWhitespace();

      if (this.Eof)
      {
        this.TokenType = TokenType.End;
        return;
      }

      this.StartPosition = this.Position;

      var nextChar = this.NextChar();
      switch (nextChar)
      {
        case '/':
          this.TokenType = TokenType.Operator;
          this.MatchChar();
          if (this.NextChar() == '/')
          {
            this.MatchChar();
          }

          break;

        case '>':
          this.TokenType = TokenType.Operator;
          this.MatchChar();
          if (this.NextChar() == '=')
          {
            this.MatchChar();
          }

          break;

        case '<':
          this.TokenType = TokenType.Operator;
          this.MatchChar();
          if (this.NextChar() == '=')
          {
            this.MatchChar();
          }

          break;

        case '!':
          this.TokenType = TokenType.Operator;
          this.MatchChar();
          if (this.NextChar() == '=')
          {
            this.MatchChar();
          }

          break;

        case ':':
          this.TokenType = TokenType.Operator;
          this.MatchChar();
          if (this.NextChar() == ':')
          {
            this.MatchChar();
          }

          break;

        case '.':
          this.TokenType = TokenType.Operator;
          this.MatchChar();
          if (this.NextChar() == '.')
          {
            this.MatchChar();
          }

          break;

        case '=':
        case '*':
        case '[':
        case ']':
        case '-':
        case '+':
        case '(':
        case ')':
        case '$':
        case ',':
        case '|':
        case ';':
          this.TokenType = TokenType.Operator;
          this.MatchChar();
          break;

        case '"':
          this.TokenType = TokenType.String;
          this.MatchQuotedString('"');
          break;

        case '\'':
          this.TokenType = TokenType.String;
          this.MatchQuotedString('\'');
          break;

        case '#':
          this.TokenType = TokenType.Identifier;
          this.MatchQuotedString('#');
          break;

        case '0':
        case '1':
        case '2':
        case '3':
        case '4':
        case '5':
        case '6':
        case '7':
        case '8':
        case '9':
          this.TokenType = TokenType.Number;
          this.MatchNumber();
          break;

        default:
          if (Char.IsLetterOrDigit(nextChar))
          {
            this.TokenType = TokenType.Identifier;
            this.MatchIdentifier();
          }
          else
          {
            this.MatchChar();
          }

          break;
      }
    }

    #endregion

    #region Methods

    /// <summary>Matches the char.</summary>
    private void MatchChar()
    {
      this.TokenBuilder.Append(this.Snapshot[this.Position]);
      this.Position++;
    }

    /// <summary>Matches the identifier.</summary>
    private void MatchIdentifier()
    {
      while (!this.Eof && Char.IsLetterOrDigit(this.NextChar()))
      {
        this.MatchChar();
      }
    }

    /// <summary>Matches the number.</summary>
    private void MatchNumber()
    {
      while (!this.Eof)
      {
        if (!Char.IsDigit(this.NextChar()))
        {
          break;
        }

        this.MatchChar();
      }
    }

    /// <summary>Matches the quoted string.</summary>
    /// <param name="p0">The p0.</param>
    private void MatchQuotedString(char p0)
    {
      this.SkipChar();

      var isTerminated = false;
      while (!this.Eof)
      {
        if (this.NextChar() == p0)
        {
          isTerminated = true;
          this.SkipChar();
          break;
        }

        this.MatchChar();
      }

      if (!isTerminated)
      {
        this.TokenType = TokenType.Unknown;
      }
    }

    /// <summary>Nexts the char.</summary>
    /// <returns>Returns the char.</returns>
    private char NextChar()
    {
      return this.Eof ? '\0' : this.Snapshot[this.Position];
    }

    /// <summary>Skips the char.</summary>
    private void SkipChar()
    {
      this.Position++;
    }

    /// <summary>Skips the whitespace.</summary>
    private void SkipWhitespace()
    {
      while (this.Position < this.End)
      {
        var c = this.Snapshot[this.Position];
        if (!Char.IsWhiteSpace(c))
        {
          return;
        }

        this.Position++;
      }
    }

    #endregion
  }
}