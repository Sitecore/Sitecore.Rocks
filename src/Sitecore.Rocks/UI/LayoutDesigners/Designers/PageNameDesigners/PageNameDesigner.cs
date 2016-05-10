namespace Sitecore.VisualStudio.UI.LayoutDesigners.Designers.PageNameDesigners
{
  using System.Windows;
  using Sitecore.VisualStudio.Extensibility.Composition;

  /// <summary>
  /// Class PropertyRenderingDesigner
  /// </summary>
  [Export(typeof(BaseDesigner), Priority = 1000, CreationPolicy = CreationPolicy.Shared)]
  public class PageNameDesigner : BaseDesigner
  {
    #region Constructors and Destructors

    /// <summary>
    /// Initializes a new instance of the <see cref="PageNameDesigner"/> class.
    /// </summary>
    public PageNameDesigner()
    {
      this.Name = "Page";
    }

    #endregion

    #region Public Methods and Operators

    /// <summary>Determines whether this instance can design the specified rendering.</summary>
    /// <param name="parameter">The parameter.</param>
    /// <returns><c>true</c> if this instance can design the specified rendering; otherwise, <c>false</c>.</returns>
    public override bool CanDesign(object parameter)
    {
      return parameter is PageContext;
    }

    /// <summary>Gets the designer.</summary>
    /// <param name="parameter">The parameter.</param>
    /// <returns>The designer.</returns>
    public override FrameworkElement GetDesigner(object parameter)
    {
      var context = parameter as PageContext;
      if (context == null)
      {
        return null;
      }

      return new PageNameDesignerControl(context.PageModel);
    }

    #endregion
  }
}