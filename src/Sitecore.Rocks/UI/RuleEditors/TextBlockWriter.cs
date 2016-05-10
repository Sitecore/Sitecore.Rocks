namespace Sitecore.VisualStudio.UI.RuleEditors
{
  using System.Collections.Generic;
  using System.Linq;
  using System.Windows;
  using System.Windows.Controls;
  using System.Windows.Documents;
  using System.Windows.Media;
  using Sitecore.VisualStudio.Annotations;
  using Sitecore.VisualStudio.Diagnostics;

  /// <summary>
  ///   Defines the <see cref="TextBlockWriter" /> class.
  /// </summary>
  public class TextBlockWriter
  {
    #region Constants and Fields

    /// <summary>
    ///   The stack field.
    /// </summary>
    private readonly Stack<TextBlock> stack = new Stack<TextBlock>();

    #endregion

    #region Constructors and Destructors

    /// <summary>
    ///   Initializes a new instance of the <see cref="TextBlockWriter" /> class.
    /// </summary>
    /// <param name="textBlock"> The text block. </param>
    public TextBlockWriter([NotNull] TextBlock textBlock)
    {
      Assert.ArgumentNotNull(textBlock, "textBlock");

      this.TextBlock = textBlock;

      this.stack.Push(textBlock);
    }

    #endregion

    #region Public Properties

    /// <summary>
    ///   Gets the current.
    /// </summary>
    [NotNull]
    public TextBlock Current
    {
      get
      {
        return this.stack.Peek();
      }
    }

    /// <summary>
    ///   Gets the text block.
    /// </summary>
    /// <value> The text block. </value>
    [NotNull]
    public TextBlock TextBlock { get; private set; }

    /// <summary>
    /// Gets a value indicating whether this instance is empty.
    /// </summary>
    public bool IsEmpty
    {
      get
      {
        return !this.TextBlock.Inlines.Any();
      }
    }

    #endregion

    #region Public Methods

    /// <summary>
    ///   Clears this instance.
    /// </summary>
    public void Clear()
    {
      this.Current.Inlines.Clear();
    }

    /// <summary>
    ///   Writes the specified text.
    /// </summary>
    /// <param name="text"> The text. </param>
    /// <param name="foreground"> The foreground. </param>
    public void Write([NotNull] string text, [CanBeNull] Brush foreground = null)
    {
      Assert.ArgumentNotNull(text, "text");

      var inline = new Run(text);

      if (foreground != null)
      {
        inline.Foreground = foreground;
      }

      this.Current.Inlines.Add(inline);
    }

    /// <summary>
    ///   Writes the specified output.
    /// </summary>
    /// <param name="output"> The output. </param>
    public void Write([NotNull] TextBlockWriter output)
    {
      Assert.ArgumentNotNull(output, "output");

      this.Current.Inlines.Add(output.TextBlock);
    }

    /// <summary>Writes the specified element.</summary>
    /// <param name="element">The element.</param>
    public void Write([NotNull] UIElement element)
    {
      Assert.ArgumentNotNull(element, "element");

      this.Current.Inlines.Add(element);
    }

    /// <summary>
    ///   Writes the end block.
    /// </summary>
    public void WriteEndBlock()
    {
      this.stack.Pop();
    }

    /// <summary>
    ///   Writes the hyperlink.
    /// </summary>
    /// <param name="textBlock"> The text block. </param>
    /// <param name="text"> The text. </param>
    /// <param name="click"> The click. </param>
    public void WriteHyperlink([NotNull] string text, [NotNull] RoutedEventHandler click)
    {
      Assert.ArgumentNotNull(text, "text");
      Assert.ArgumentNotNull(click, "click");

      var inline = new Run(text);

      var hyperlink = new Hyperlink(inline);
      hyperlink.Click += click;

      this.Current.Inlines.Add(hyperlink);
    }

    /// <summary>
    ///   Writes the line.
    /// </summary>
    /// <param name="textBlock"> The text block. </param>
    /// <param name="text"> The text. </param>
    /// <param name="foreground"> The foreground. </param>
    public void WriteLine([NotNull] string text, [CanBeNull] Brush foreground = null)
    {
      Assert.ArgumentNotNull(text, "text");

      var inline = new Run(text);

      if (foreground != null)
      {
        inline.Foreground = foreground;
      }

      this.Current.Inlines.Add(inline);
      this.Current.Inlines.Add(new LineBreak());
    }

    /// <summary>
    ///   Writes the line.
    /// </summary>
    public void WriteLine()
    {
      this.Current.Inlines.Add(new LineBreak());
    }

    /// <summary>
    ///   Writes the begin block.
    /// </summary>
    /// <param name="margin"> The margin. </param>
    /// <param name="padding"> The padding. </param>
    /// <param name="horizontalAlignment"> The horizontal alignment. </param>
    /// <param name="verticalAlignment"> The vertical alignment. </param>
    public void WriteStartBlock(Thickness margin = default(Thickness), Thickness padding = default(Thickness), HorizontalAlignment horizontalAlignment = HorizontalAlignment.Left, VerticalAlignment verticalAlignment = VerticalAlignment.Top, Visibility visibility = Visibility.Visible)
    {
      var block = new TextBlock();

      if (margin != default(Thickness))
      {
        block.Margin = margin;
      }

      if (padding != default(Thickness))
      {
        block.Padding = margin;
      }

      block.HorizontalAlignment = horizontalAlignment;
      block.VerticalAlignment = verticalAlignment;
      block.Visibility = visibility;

      this.Current.Inlines.Add(block);

      this.stack.Push(block);
    }

    #endregion
  }
}