// --------------------------------------------------------------------------------------------------------------------
// <copyright file="RuleFieldEditDialog.xaml.cs" company="Sitecore A/S">
//   Copyright (C) 2010 by Sitecore A/S
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Sitecore.VisualStudio.ContentEditors.Dialogs.Rules
{
  using System.Linq;
  using System.Windows;
  using Sitecore.VisualStudio.Annotations;
  using Sitecore.VisualStudio.Data;
  using Sitecore.VisualStudio.Diagnostics;

  /// <summary>Interaction logic for RuleEditor.xaml</summary>
  public partial class RuleFieldEditDialog
  {
    #region Constants and Fields

    /// <summary>
    /// The source field currently editing.
    /// </summary>
    private Field field;

    #endregion

    #region Constructors and Destructors

    /// <summary>
    /// Initializes a new instance of the <see cref="RuleFieldEditDialog"/> class.
    /// </summary>
    public RuleFieldEditDialog()
    {
      this.InitializeComponent();
    }

    #endregion

    #region Public Methods

    /// <summary>Initializes the specified source field.</summary>
    /// <param name="field">The source field.</param>
    public void Initialize([NotNull] Field field)
    {
      Assert.ArgumentNotNull(field, "field");

      this.field = field;

      this.RuleDesigner.Initialize(field.FieldUris.First().ItemVersionUri.DatabaseUri, null);
    }

    #endregion

    #region Methods

    /// <summary>Cancels the button click.</summary>
    /// <param name="sender">The sender.</param>
    /// <param name="e">The <see cref="System.Windows.RoutedEventArgs"/> instance containing the event data.</param>
    private void CancelClick([NotNull] object sender, [NotNull] RoutedEventArgs e)
    {
      Assert.ArgumentNotNull(sender, "sender");
      Assert.ArgumentNotNull(e, "e");

      this.DialogResult = false;

      return;
    }

    /// <summary>Oks the button click.</summary>
    /// <param name="sender">The sender.</param>
    /// <param name="e">The <see cref="System.Windows.RoutedEventArgs"/> instance containing the event data.</param>
    private void OkClick([NotNull] object sender, [NotNull] RoutedEventArgs e)
    {
      Assert.ArgumentNotNull(sender, "sender");
      Assert.ArgumentNotNull(e, "e");

      this.DialogResult = true;

      return;
    }

    #endregion
  }
}