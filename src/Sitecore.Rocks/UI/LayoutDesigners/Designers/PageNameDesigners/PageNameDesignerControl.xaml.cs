// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PageNameDesignerControl.xaml.cs" company="Sitecore A/S">
//   Copyright (C) by Sitecore A/S
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
namespace Sitecore.VisualStudio.UI.LayoutDesigners.Designers.PageNameDesigners
{
  using System.Windows;
  using Sitecore.VisualStudio.Annotations;
  using Sitecore.VisualStudio.Diagnostics;
  using Sitecore.VisualStudio.UI.LayoutDesigners.LayoutTreeViews.Models;
  using Sitecore.VisualStudio.UI.Prompts;

  /// <summary>
  /// Interaction logic for ParameterDesigner.xaml
  /// </summary>
  public partial class PageNameDesignerControl : IDesigner
  {
    #region Constructors and Destructors

    /// <summary>Initializes a new instance of the <see cref="PageNameDesignerControl"/> class.</summary>
    /// <param name="pageModel">The device.</param>
    public PageNameDesignerControl([NotNull] PageModel pageModel)
    {
      Assert.ArgumentNotNull(pageModel, "pageModel");

      this.InitializeComponent();

      this.PageModel = pageModel;
      this.PageNameTextBox.Text = this.PageModel.Text;

      this.DataContext = this;
    }

    #endregion

    #region Public Properties

    /// <summary>
    /// Gets the layout builder.
    /// </summary>
    /// <value>The layout builder.</value>
    [NotNull]
    public PageModel PageModel { get; private set; }

    #endregion

    #region Public Methods and Operators

    /// <summary>
    /// Activates this instance.
    /// </summary>
    public void Activate()
    {
    }

    /// <summary>
    /// Closes this instance.
    /// </summary>
    public void Close()
    {
    }

    #endregion

    #region Methods

    /// <summary>Browses the specified sender.</summary>
    /// <param name="sender">The sender.</param>
    /// <param name="e">The <see cref="System.Windows.RoutedEventArgs"/> instance containing the event data.</param>
    private void Rename([NotNull] object sender, [NotNull] RoutedEventArgs e)
    {
      Debug.ArgumentNotNull(sender, "sender");
      Debug.ArgumentNotNull(e, "e");

      var newName = Prompt.Show("Enter a new name for the page:", "Rename", this.PageModel.Text ?? string.Empty);
      if (string.IsNullOrEmpty(newName))
      {
        return;
      }

      this.PageModel.Text = newName;

      this.PageModel.ItemUri.Site.DataService.Rename(this.PageModel.ItemUri, newName);

      Notifications.RaiseItemRenamed(this, this.PageModel.ItemUri, newName);
    }

    #endregion
  }
}