// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SetIcon.cs" company="Sitecore A/S">
//   Copyright (C) by Sitecore A/S
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Sitecore.VisualStudio.ContentEditors.Commands.Tasks
{
  using System.Linq;
  using System.Windows.Controls;
  using Sitecore.VisualStudio.Annotations;
  using Sitecore.VisualStudio.Commands;
  using Sitecore.VisualStudio.ContentEditors.Dialogs;
  using Sitecore.VisualStudio.Data;
  using Sitecore.VisualStudio.Diagnostics;
  using Sitecore.VisualStudio.Extensions.FrameworkElementExtensions;
  using Sitecore.VisualStudio.Shell;

  /// <summary>Defines the raw values command class.</summary>
  [Command(Submenu = TasksSubmenu.Name)]
  [CommandId(CommandIds.ItemEditor.SetIcon, typeof(ContentEditorContext))]
  public class SetIcon : CommandBase
  {
    #region Constructors and Destructors

    /// <summary>Initializes a new instance of the <see cref="SetIcon"/> class.</summary>
    public SetIcon()
    {
      this.Text = Resources.SetIcon_SetIcon_Set_Icon;
      this.Group = "Fields";
      this.SortingValue = 1000;
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

      var field = this.GetIconField(context);
      if (field == null)
      {
        return false;
      }

      if (!context.ContentEditor.ContentModel.FirstItem.ItemUri.Site.DataService.HasWebSite)
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

      var field = this.GetIconField(context);
      if (field == null)
      {
        return;
      }

      var d = new SetIconDialog();
      d.Initialize(field.FieldUris.First().ItemVersionUri.Site, field.Value);
      if (d.ShowDialog() != true)
      {
        return;
      }

      context.ContentEditor.ContentModel.IsModified = true;
      field.Value = d.FileName;
      if (field.Control != null)
      {
        field.Control.SetValue(d.FileName);
      }

      var control = context.ContentEditor.AppearanceOptions.Skin.GetControl();

      var image = control.FindChild<Image>(@"QuickInfoIcon");
      if (image == null)
      {
        return;
      }

      var path = @"/sitecore/shell/~/icon/" + d.FileName.Replace(@"16x16", @"32x32");

      var icon = new Icon(field.FieldUris.First().Site, path);

      image.Source = icon.GetSource();
    }

    /// <summary>
    /// Gets the icon field.
    /// </summary>
    /// <param name="context">The context.</param>
    /// <returns>Returns the icon field.</returns>
    [CanBeNull]
    private Field GetIconField([NotNull] ContentEditorContext context)
    {
      Debug.ArgumentNotNull(context, "context");

      var iconFieldId = IdManager.GetFieldId("/sitecore/templates/System/Templates/Sections/Appearance/Appearance/__Icon");

      foreach (var field in context.ContentEditor.ContentModel.Fields)
      {
        if (field.FieldUris.First().FieldId == iconFieldId)
        {
          return field;
        }
      }

      return null;
    }

    #endregion
  }
}