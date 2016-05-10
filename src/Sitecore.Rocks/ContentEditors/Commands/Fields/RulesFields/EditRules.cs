// --------------------------------------------------------------------------------------------------------------------
// <copyright file="EditRules.cs" company="Sitecore A/S">
//   Copyright (C) 2010 by Sitecore A/S
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Sitecore.VisualStudio.ContentEditors.Commands.Fields.RulesFields
{
  using Sitecore.VisualStudio.Commands;
  using Sitecore.VisualStudio.ContentEditors.Dialogs.Rules;
  using Sitecore.VisualStudio.ContentEditors.Fields;
  using Sitecore.VisualStudio.UI.Rules;

  /// <summary>Defines the raw values command class.</summary>
  [Command]
  public class EditRules : CommandBase
  {
    #region Constructors and Destructors

    /// <summary>Initializes a new instance of the <see cref="EditRules"/> class.</summary>
    public EditRules()
    {
      this.Text = "Edit Rules...";
      this.Group = "Rules";
      this.SortingValue = 100;
    }

    #endregion

    #region Public Methods

    /// <summary>Defines the method that determines whether the command can execute in its current state.</summary>
    /// <param name="parameter">Data used by the command.  If the command does not require data to be passed, this object can be set to null.</param>
    /// <returns>true if this command can be executed; otherwise, false.</returns>
    public override bool CanExecute(object parameter)
    {
      var context = parameter as ContentEditorFieldContext;
      if (context == null)
      {
        return false;
      }

      var field = context.Field;
      if (!(field.Control is RuleField))
      {
        return false;
      }

      return true;
    }

    /// <summary>The execute.</summary>
    /// <param name="parameter">The parameter.</param>
    public override void Execute(object parameter)
    {
      var context = parameter as ContentEditorFieldContext;
      if (context == null)
      {
        return;
      }

      var field = context.Field;
      if (field == null)
      {
        return;
      }

      var dialog = new RuleFieldEditDialog();

      dialog.Initialize(field);

      dialog.ShowDialog();
    }

    #endregion
  }
}