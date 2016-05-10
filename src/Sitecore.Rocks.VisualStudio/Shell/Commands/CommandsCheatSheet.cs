// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CommandsCheatSheet.cs" company="Sitecore A/S">
//   Copyright (C) by Sitecore A/S
// </copyright>
// <summary>
//   Defines the view command class.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Sitecore.VisualStudio.Shell.Commands
{
  using Sitecore.VisualStudio.Commands;
  using Sitecore.VisualStudio.ContentTrees.Commands.Settings;
  using Sitecore.VisualStudio.Extensibility;

  /// <summary>Defines the view command class.</summary>
  [Command]
  [ShellMenuCommand(CommandIds.CommandsCheatSheet)]
  [Feature(FeatureNames.AdvancedOperations)]
  public class CommandsCheatSheet : CommandBase
  {
    #region Constructors and Destructors

    /// <summary>Initializes a new instance of the <see cref="CommandsCheatSheet"/> class.</summary>
    public CommandsCheatSheet()
    {
      this.Text = "Commands Cheat Sheet";
    }

    #endregion

    #region Public Methods

    /// <summary>Defines the method that determines whether the command can execute in its current state.</summary>
    /// <param name="parameter">Data used by the command.  If the command does not require data to be passed, this object can be set to null.</param>
    /// <returns>true if this command can be executed; otherwise, false.</returns>
    public override bool CanExecute(object parameter)
    {
      return parameter is ShellContext;
    }

    /// <summary>The execute.</summary>
    /// <param name="parameter">The parameter.</param>
    public override void Execute(object parameter)
    {
      var command = new CommandCheatSheet();

      if (command.CanExecute(parameter))
      {
        AppHost.Usage.ReportCommand(command, parameter);
        command.Execute(parameter);
      }
    }

    #endregion
  }
}