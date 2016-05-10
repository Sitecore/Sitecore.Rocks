// --------------------------------------------------------------------------------------------------------------------
// <copyright file="TasksSubmenu.cs" company="Sitecore A/S">
//   Copyright (C) by Sitecore A/S
// </copyright>
// <summary>
//   Defines the view command class.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Sitecore.VisualStudio.ContentEditors.Commands.Tasks
{
  using Sitecore.VisualStudio.Commands;

  /// <summary>Defines the view command class.</summary>
  [Command]
  public class TasksSubmenu : Submenu
  {
    /// <summary>The name field.</summary>
    public const string Name = "Tasks";

    #region Constructors and Destructors

    /// <summary>Initializes a new instance of the <see cref="TasksSubmenu"/> class.</summary>
    public TasksSubmenu()
    {
      this.Text = Resources.TasksSubmenu_TasksSubmenu_Tasks;
      this.Group = "Design";
      this.SortingValue = 2200;
      this.SubmenuName = Name;
      this.ContextType = typeof(ContentEditorContext);
    }

    #endregion
  }
}