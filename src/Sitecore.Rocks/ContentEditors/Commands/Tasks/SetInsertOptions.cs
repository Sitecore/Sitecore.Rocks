// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SetInsertOptions.cs" company="Sitecore A/S">
//   Copyright (C) by Sitecore A/S
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Sitecore.VisualStudio.ContentEditors.Commands.Tasks
{
  using System;
  using System.Collections.Generic;
  using System.Linq;
  using Sitecore.VisualStudio.Annotations;
  using Sitecore.VisualStudio.Commands;
  using Sitecore.VisualStudio.Data;
  using Sitecore.VisualStudio.Diagnostics;
  using Sitecore.VisualStudio.Shell;
  using Sitecore.VisualStudio.UI.SelectTemplates;

  /// <summary>Defines the raw values command class.</summary>
  [Command(Submenu = TasksSubmenu.Name)]
  [CommandId(CommandIds.ItemEditor.SetInsertOptions, typeof(ContentEditorContext))]
  public class SetInsertOptions : CommandBase
  {
    #region Constructors and Destructors

    /// <summary>Initializes a new instance of the <see cref="SetInsertOptions"/> class.</summary>
    public SetInsertOptions()
    {
      this.Text = Resources.SetInsertOptions_SetInsertOptions_Set_Insert_Options;
      this.Group = "Fields";
      this.SortingValue = 2000;
    }

    #endregion

    #region Public Methods

    /// <summary>Defines the method that determines whether the command can execute in its current state.</summary>
    /// <param name="parameter">Data used by the command.  If the command does not require data to be passed, this object can be set to null.</param>
    /// <returns>true if this command can be executed; otherwise, false.</returns>
    public override bool CanExecute(object parameter)
    {
      var context = parameter as ContentEditorContext;
      if (context == null)
      {
        return false;
      }

      if (!context.ContentEditor.ContentModel.IsSingle)
      {
        return false;
      }

      var field = this.GetInsertOptionsField(context);
      if (field == null)
      {
        return false;
      }

      return true;
    }

    /// <summary>The execute.</summary>
    /// <param name="parameter">The parameter.</param>
    public override void Execute(object parameter)
    {
      var context = parameter as ContentEditorContext;
      if (context == null)
      {
        return;
      }

      context.ContentEditor.ContentModel.GetChanges();

      var field = this.GetInsertOptionsField(context);
      if (field == null)
      {
        return;
      }

      var value = field.Value;

      var selectedItems = new List<ItemId>();
      foreach (var s in value.Split('|'))
      {
        if (string.IsNullOrEmpty(s))
        {
          continue;
        }

        selectedItems.Add(new ItemId(new Guid(s)));
      }

      var d = new SelectTemplatesDialog();
      d.Initialize(Resources.SetInsertOptions_Execute_Insert_Options, field.FieldUris.First().ItemVersionUri.DatabaseUri, selectedItems, true);
      if (d.ShowDialog() != true)
      {
        return;
      }

      value = string.Empty;
      foreach (var selectedItem in d.SelectedItems)
      {
        if (!string.IsNullOrEmpty(value))
        {
          value += '|';
        }

        value += selectedItem.ToString();
      }

      field.Value = value;
      if (field.Control != null)
      {
        field.Control.SetValue(value);
      }

      context.ContentEditor.ContentModel.IsModified = true;
    }

    #endregion

    #region Methods

    /// <summary>Gets the icon field.</summary>
    /// <param name="context">The context.</param>
    /// <returns>Returns the icon field.</returns>
    [CanBeNull]
    private Field GetInsertOptionsField([NotNull] ContentEditorContext context)
    {
      Debug.ArgumentNotNull(context, "context");

      var insertOptionsField = IdManager.GetFieldId("/sitecore/templates/System/Templates/Sections/Insert Options/Insert Options/__Masters");

      foreach (var field in context.ContentEditor.ContentModel.Fields)
      {
        if (field.FieldUris.First().FieldId == insertOptionsField)
        {
          return field;
        }
      }

      return null;
    }

    #endregion
  }
}